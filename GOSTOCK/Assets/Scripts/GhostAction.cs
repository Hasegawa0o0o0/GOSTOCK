
// 2018.06.25　アニメーションを追加
// 2018.06.29 3カメおばけの反転の仕方を変更
// 2018.06.30 アニメーションを追加
// 2018.07.13 消えた時のエフェクトを追加
//            1カメのカーソルの色が変わりっぱなしになるのを防ぐ
// 2018.10.16 アニメーション関係を変更
// 2018.11.27 吸い込まれた時のスプライトの変化
// 2019.01.19 もう一つのほうのスプライトも変える
//			  スプライトが変わっていたら止める
// 2019.02.03 吸い込まれているときは炎を消す
// 2019.03.03 吸い込まれているときは撮影されないように
using UnityEngine;

public class GhostAction : MonoBehaviour
{
	public enum GhostType
	{
		Normal,		// 普通の動き
		GoldFish,	// 金魚動き
		Flip,		// 画面端で数回反射
		Wave,		// 波形
		Fat,		// だんだん早くなる
	}
    public Rigidbody rigidbody;
	public Rigidbody rb;
	SpriteRenderer spriteRenderer;
	public float speed;
	public float direction;	// degの角度が入る
	public bool isLeft;     // 左回りかどうか
	public float curve;		// カーブの大きさ
	public float zSpeed;
	public GhostType ghostType;
	float angle;			// radの角度が入る
	public float xSpeed;
	float ySpeed;
	GhostMaster gm;
	PlayerControl playerControl;
	public const int cIsCamera = 0;
	public const int cWhereSpowner = 1;
	public const int cNumber = 2;
	public int[] data = new int[3]; // 1,2カメどちらか、どのスポナーから生まれたか、何番目に生まれたか
	public GameObject linkGhost;
	bool inCursor = false;	// 2018.07.13

	Transform frontObj;							// 3カメ用のおばけ
	Anim mainAnim;									// 変更 2018.10.16 anim1 -> 
	Anim anim2;                                     // 2カメのおばけのアニメーション
	Anim frontAnim;									// 変更 2018.10.16 anim3 ->
	public Sprite[] backSprite = new Sprite[4];		// 変更 2018.10.16 ghostSprite1 -> 
	//public Sprite[] ghost2Sprites = new Sprite[4];  // 2カメのおばけのスプライト 2018.10.16
	public Sprite[] frontSprite = new Sprite[4];	// 変更 2018.10.16 ghostSprite3 ->
	public GameObject deletePrefab;                 // 2018.07.13
	public Sprite inholeSprite;						// 吸い込まれる直前のスプライト 2018.11.27

	// 金魚の動きで使う
	float xSpeedMax;			// xの最大スピード
	float ySpeedMax;			// yの最大スピード
	// 反射の動きで使う
	int flipCnt = 0;			// 反射した回数
	const int flipCntMax = 2;   // 反射する回数
	int hitFrameX = 0;			// 当たった後に再判定しない用(X)
	int hitFrameY = 0;          // 当たった後に再判定しない用(y)
	public bool inCamera = false;
	// だんだん早くなる動きで使う
	float speedMax;
	// 波形の動きで使う
	float addDir = 1f;	// 加える角度
	int changeFrame = 0;
	bool isStart = true;

	// 2018.06.30
	int fireAnimFrame = 0;
	public Sprite[] smallFireSpriteList = new Sprite[2];	// 小さい炎のスプライトリスト
	public Sprite[] bigFireSpriteList = new Sprite[2];		// 大きい炎のスプライトリスト
	public Anim smallFireAnimation;							// 小さい炎用のアニメーションクラス
	public Anim bigFireAnimation;							// 大きい炎用のアニメーションクラス
	public bool camera2;

	// [Re]追加部分
	Score score;
	public GameObject vanishingPrefab;						// お化けが消えるアニメーション？
	private EnergyRe energyRe;

	// 2019.03.22
	public BossAction bossAction;							// ボスのスクリプト
	
	private void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		gm = FindObjectOfType<GhostMaster>();
		playerControl = FindObjectOfType<PlayerControl>();
		mainAnim = GetComponent<Anim>();
		// 自分のデータをゴーストマスターに送る
		//gm.ghostObj[data[cIsCamera], data[cWhereSpowner], data[cNumber]] = gameObject;
		if (ghostType == GhostType.GoldFish)
		{
			CalcSpeed();
			xSpeedMax = xSpeed;
			ySpeedMax = ySpeed;
		}
		else if (ghostType == GhostType.Fat)
		{
			speedMax = speed;
			speed = 0;
		}
		// 変更 2018.10.16
		frontObj = transform.Find("Ghost3");
		frontAnim = frontObj.GetComponent<Anim>();
		// [Re]追加部分
		score = FindObjectOfType<Score>();
		energyRe = FindObjectOfType<EnergyRe>();
		if (GameObject.Find("Boss(Clone)") == true)				// 2019.03.22
		{
			bossAction = GameObject.Find("Boss(Clone)").GetComponent<BossAction>();
		}
	}

	public void Update()
	{
		// 1カメのおばけだったときはリンクしているおばけを取得
		//if (linkGhost == null && data[cIsCamera] == 0)
		//{
		//	//linkGhost = gm.ghostObj[1, data[cWhereSpowner], data[cNumber]];
		//	// リンクしているおばけのrigidbodyを取得
		//	if (rb == null && linkGhost) { rb = linkGhost.GetComponent<Rigidbody>(); }
		//	// リンクしているおばけのAnimを取得
		//	if (anim2 == null && linkGhost) { anim2 = linkGhost.GetComponent<Anim>(); }
		//	// 3カメのおばけのAnimをリンクしているおばけから取得
		//	if (frontObj == null && linkGhost)
		//	{
		//	}
		//}
		//if (data[cIsCamera] == 0)
		//{
		//	if (anim2) { anim2.Animation(ghost2Sprites, 4); }
		//	if (frontAnim)
		//	{
		//		// 3カメのおばけをここで制御 2018.06.29
		//	}
		//}

		// 2019.03.22
		if (bossAction != null)
		{
			if (bossAction.ghostStop == true)
			{
				spriteRenderer.color -= new Color(0, 0, 0, 0.05f);
				if (spriteRenderer.color.a <= 0)
				{
					Destroy(gameObject);
				}
			}
		}

		if (mainAnim.spriteRenderer.sprite != inholeSprite || frontAnim.spriteRenderer.sprite != inholeSprite)
		{
			// アニメーション 2018.10.16
			mainAnim.Animation(backSprite, 4);
			frontAnim.Animation(frontSprite, 4);
			frontObj.localPosition = new Vector3(0, 0, 0.05f * Mathf.Sign(ySpeed));
		}
		// 炎アニメーション
		if (camera2)
		{
			if (fireAnimFrame <= 600)
			{
				// アニメーションフレームの更新
				++fireAnimFrame;
				// 一定の間だけ小さい炎のアニメーション
				if (fireAnimFrame % 180 <= 60)
				{
					smallFireAnimation.Animation(smallFireSpriteList, 2);
				}
				// 一定の間以外は非表示
				else
				{
					smallFireAnimation.OffSprite();
				}
				// 一定の間だけ大きい炎のアニメーション
				if (fireAnimFrame % 120 <= 60)
				{
					bigFireAnimation.Animation(bigFireSpriteList, 2);
				}
				// 一定の間以外は非表示
				else
				{
					bigFireAnimation.OffSprite();
				}
			}
			else
			{
				fireAnimFrame = 0;
			}
		}	
		// それぞれのタイプでスピードを更新する
		// 普通
		if (ghostType == GhostType.Normal)
		{
			CalcSpeedNormal();
		}
		// 金魚
		else if (ghostType == GhostType.GoldFish)
		{
			CalcSpeedGoldFish();
		}
		// 反転
		else if (ghostType == GhostType.Flip)
		{
			CalcSpeedNormal();
			if (flipCnt < flipCntMax && inCamera)
			{
				if ((transform.localPosition.x < 12 || transform.localPosition.x > 28) && hitFrameX <= 0)
				{
					// 90度との差を出してその分反転する
					float flipDeg = 90 - direction;
					direction = 90 + flipDeg;
					// 反転した回数をカウント
					++flipCnt;
					// 再判定しないようにする
					hitFrameX = 10;
				}
				if ((transform.localPosition.y < -4 || transform.localPosition.y > 4) && hitFrameY <= 0)
				{
					// 180度との差を出してその分反転する
					float flipDeg = 180 - direction;
					direction = 180 + flipDeg;
					// 反転した回数をカウント
					++flipCnt;
					// 再判定しないようにする
					hitFrameY = 10;
				}
				hitFrameX--;
				hitFrameY--;
			}
			if ((transform.localPosition.x > 12 && transform.localPosition.x < 28 && transform.localPosition.y > -4 && transform.localPosition.y < 4) && !inCamera)
			{
				inCamera = true;
			}
		}
		// だんだん早くなる
		else if (ghostType == GhostType.Fat)
		{
			CalcSpeedNormal();
			speed += speedMax / 100;
		}
		// 波形
		else if (ghostType == GhostType.Wave)
		{
			CalcSpeedWave();
		}
		// スプライトが変わっていたら止める 2019.01.19
		if (mainAnim.spriteRenderer.sprite == inholeSprite || frontAnim.spriteRenderer.sprite == inholeSprite)
		{
			xSpeed = ySpeed = zSpeed = 0f;
			rigidbody.velocity = Vector3.zero;
			// 炎を消す 2019.02.03
			smallFireAnimation.OffSprite();
			bigFireAnimation.OffSprite();
		}
		// 適用
		if (rigidbody && data[cIsCamera] == 0)
		{
			rigidbody.velocity = new Vector3(xSpeed, ySpeed, zSpeed) * PlayerControl.DEBUGDOUBLE;
		}
		if (rb && data[cIsCamera] == 0)
		{
			rb.velocity = new Vector3(xSpeed, ySpeed, zSpeed) * PlayerControl.DEBUGDOUBLE;
		}
		// 絵の反転
		spriteRenderer.flipX = xSpeed > 0;
		if (anim2) { anim2.spriteRenderer.flipX = xSpeed > 0; }
		if (frontAnim) { frontAnim.spriteRenderer.flipX = xSpeed > 0; }     // 2018.06.29
	}

	// スタンダードなスピードの計算
	void CalcSpeed()
	{
		// 円形になるようにスピードを振り分ける
		angle = direction * Mathf.Deg2Rad;
		xSpeed = speed * Mathf.Cos(angle);
		ySpeed = speed * Mathf.Sin(angle);
	}

	// スタンダードなカーブの計算
	void CalcCurve(float min = 10, float max = 10)
	{
		// 方向を少し変える
		float curve2 = Random.Range(-curve * min, curve * max) + curve;
		// 左回りか右回りかで回り方を変える
		if (isLeft)
		{
			direction += curve2;
		}
		else
		{
			direction -= curve2;
		}
	}

	// 普通の動きのためのスピード更新
	void CalcSpeedNormal()
	{
		CalcSpeed();
		CalcCurve();
	}

	// 金魚の動きのためのスピード更新
	void CalcSpeedGoldFish()
	{
		// スピードを減らしていく
		// x
		if (Mathf.Abs(xSpeed) > 0)
		{
			xSpeed -= xSpeedMax / 300;
		}
		// y
		if (Mathf.Abs(ySpeed) > 0)
		{
			ySpeed -= ySpeedMax / 300;
		}
		// スピードが極小になったらスピードを計算しなおす
		if (Mathf.Abs(xSpeed) < 1 && Mathf.Abs(ySpeed) < 1)
		{
			CalcCurve(0,200);
			CalcSpeed();
			xSpeedMax = xSpeed;
			ySpeedMax = ySpeed;
		}
	}

	// 波形の動きのためのスピード更新
	void CalcSpeedWave()
	{
		CalcSpeed();
		if (changeFrame < 60)
		{
			direction += addDir;
		}
		if (changeFrame > 120)
		{
			addDir *= -1;
			changeFrame = 0;
			if (isStart)
			{
				addDir *= 2;
				isStart = false;
			}
		}
		changeFrame++;
	}

	// スプライトを吸い込まれる直前のスプライトに変える 2018.11.27
	public void ChangeSpriteDeleteInhole()
	{
		mainAnim.spriteRenderer.sprite = inholeSprite;
		mainAnim.spriteRenderer.sortingOrder = 3;
		frontAnim.spriteRenderer.sprite = inholeSprite;	// 2019.01.19
	}

	void OnTriggerEnter(Collider oth)
	{
		// 2018.12.06 コメント化
		//if (oth.tag == "Light" || oth.tag == "Enemy")
		//{
		//	if (inCursor)
		//	{
		//		PlayerControl p = FindObjectOfType<PlayerControl>();
		//		p.ghostIn = false;
		//	}
		//	// 2カメの自分とリンクしているおばけを消す
		//	//2018.07.13
		//	if (data[cIsCamera] == 0)
		//	{
		//		Instantiate(deletePrefab, transform.position, deletePrefab.transform.localRotation);
		//		//Instantiate(deletePrefab, linkGhost.transform.position, linkGhost.transform.localRotation);
		//		Destroy(linkGhost);
		//		Destroy(gameObject);
		//	}
		//	BGMMaster.instance.SoundEffect(5);
		//}
		if (oth.tag == "Player")
		{
			inCursor = true;
		}
		// [Re]追加部分	条件追加 2019.03.03
		if (oth.tag == "Photographing" && mainAnim.spriteRenderer.sprite != inholeSprite)
		{
			//Instantiate(deletePrefab, transform.position, deletePrefab.transform.localRotation);
			// 絵画のオバケと通常のオバケの差別化
			if (this.tag == "Goast" || this.tag == "TargetGhost")
			{
				Instantiate(deletePrefab, transform.position, deletePrefab.transform.localRotation);
				playerControl.power++;
			}
			else if (this.tag == "pGhost")
			{
				// オバケが消えるときの色を変更(重いかな？)
				GameObject del = Instantiate(deletePrefab, transform.position, deletePrefab.transform.localRotation);
				SpriteRenderer delSR = del.GetComponent<SpriteRenderer>();
				delSR.color = new Color(0, 1, 1, 1);
				playerControl.power += 3;
				// ゲージの量を確認する エラーのための条件追加 2019.01.18 (DeleteGhostAnimationから流用)
				if ((energyRe && playerControl) && energyRe.gageMax != 3 && playerControl.power <= 15)
				{
					energyRe.InsSparkleOnUI(playerControl.power - 1, false);
				}
			}
			score.PhotoAdd();
			Destroy(gameObject);
		}
	}

	//void OnTriggerStay(Collider col)
	//{
	//	if (cm.nowCamera == 0)
	//	{
	//		if (col.tag == "Player")
	//		{
	//			if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button5)) && !playerControl.inDamageItem)
	//			{
	//				// 2カメの自分とリンクしているおばけを消す
	//				// 2018.07.13
	//				if (data[cIsCamera] == 0)
	//				{
	//					Instantiate(deletePrefab, transform.position, Quaternion.identity);
	//					Instantiate(deletePrefab, linkGhost.transform.position, linkGhost.transform.localRotation);
	//					Destroy(linkGhost);
	//					Destroy(gameObject);
	//				}
	//				BGMMaster.instance.SoundEffect(5);
	//			}
	//		}
	//	}
	//}

	void OnTriggerExit(Collider col)
	{
		if (col.tag == "DestroyArea")
		{
			// 2カメの自分とリンクしているおばけを消す
			// 2018.07.13
			if (data[cIsCamera] == 0)
			{
				Instantiate(vanishingPrefab, transform.position, vanishingPrefab.transform.localRotation);			// 追記:2018.12.03 縦シュンに変更
				//Instantiate(deletePrefab, linkGhost.transform.position, linkGhost.transform.localRotation);
				Destroy(linkGhost);
				Destroy(gameObject);
			}
			BGMMaster.instance.SoundEffect(5);
		}
		if (col.tag == "Player")
		{
			inCursor = false;
		}
	}
}

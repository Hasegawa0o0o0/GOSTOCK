//-------------------------------------------
// プレイヤーの操作
// PayerManager.cs
// 17CU0126 長谷川勇太
// 2018.05.14
// 更新
// 2018.05.15 2カメの動作
// 2018.05.16 連続撮影できなくする
//            力を貯める
//            2カメでのバレットの発射
// 2018.05.17 
// 2018.05.18 
// 2018.05.26 いろいろ追加
// 2018.06.15 ほかにも追加
// 2018.06.21 3カメをエネルギー依存(S)
// 2018.06.24 同時撮影もろもろ
// 2018.06.30 パワーの上限
// 2018.07.11 2カメと3カメの発射の分割
// 2018.07.13 1カメカーソルの色の変え方を変更
// 2018.07.18 色の変え方を戻す
// 2018.10.13 Re:元2カメ(新規カメラ)での撮影
// 2018.10.16 攻撃と撮影の切り替え
// 2018.10.17 エネミー削除による変更
//-------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
	// デバッグ用変数
	public bool[] dInputH = new bool[2];
	public bool[] dInputW = new bool[2];
	public bool dInputX = false;
	public bool dInputO = false;
	public bool inputSpace = false;
	public bool changeInput = true;
	int recoveryFrame = 0;
	const int recoveryFrameMax = 60;
	const int DEBUGMODE = 0;
	public const float DEBUGDOUBLE = 1.5f;
	int hpMax;
	public GameObject powerUpText;
	public Sprite point2;
	public Sprite point1;

	// プレイヤー情報
	public int hp;
	Rigidbody rigidbody;					// Rigidbodyの情報
	float inputX;							// 横文字の入力
	float inputY;							// 縦方向の入力
	public float speed;						// 移動速度
	Vector2 direction;						// プレイヤーの移動方向
	SpriteRenderer spriteRenderer;			// カーソルのスプライトレンダラー
	public SpriteRenderer chainCursorSp;	// 連鎖しているときのカーソルスプライトレンダラー(unity側設定)
	int chainColor = -1;					// 色を反転させるときに使う
	public SpriteRenderer sp2;				// 2カメカーソルのスプライトレンダラー(unity側設定)
	bool isLife = true;
	public bool ghostIn = false;			// おばけが入っているかどうか 2017.07.13

	Vector3 cameraDirection;				// カメラの移動方向
	Rigidbody camera2RD;					// 2カメのrigidbody2d
	Rigidbody camera3RD;

	// カメラたち
	GameObject[] cameras = new GameObject[3];

	// 攻撃関係
	public int power = 0;								// パワー
	public int powerCnt = 0;							// 
	public int powerCntMax;								// 
	public int powerUpFrame = 0;						// パワーアップするまでのフレーム数
	const int powerUpFrameMax = 60;						// パワーアップするフレーム数
	const int power2UpFrameMax = powerUpFrameMax * 4;	// バレット第三形態
	public GameObject bulletPrefab;						// バレットプレハブ
	public GameObject bulletPrefab1;
	public GameObject bulletPrefab3;
	int attackFrame = 0;                    // アタックラグ
	public int attackFrameMax;              // アタックラグの最大値
	int collectFrame = 0;
	public bool inDamageItem = false;       // ダメージアイテムと重なっているか
	int powerFrame;                     // 連鎖する時に使う
	int powerFrame2;					// 2段階目の連鎖で使う

	public SpriteRenderer darkSp;           // 点滅オブジェクトのスプライトレンダラー  unity側設定
	int darkFrame = 0;						// 点滅オブジェクトの表示を管理
	public SpriteRenderer blackSp;			// 暗転オブジェクトのスプライトレンダラー  unity側設定

	public GameObject bulletSpownerObj;     // 1カメにバレットを生成させる為のオブジェクト

	// 1カメのバレットスポナー系
	public Rigidbody bsr;
	public Vector2 bsDir;

	EnemyAction enemy;						// エネミーに力の情報を送るため

	int updateFrame = 0;            // 強化のカウント

	// ダメージを受けたときに使う
	int damageFrame = 0;
	const int damageFrameMax = 70;
	int sign = 1;
	public bool isDamage = false;
	public int damageCameraNum;

	public bool camera = false;

	// [Re]追加部分
	// オブジェクト系
	//public GameObject PhotoPrefab;					// 撮影用判定オブジェクト
	public bool photoFlag = false;						// カメラの切り替え(false:撮影、true:攻撃)
	public GameObject cursorobj;						// カーソルの検索用
	public GameObject[] cursorChilds;					// カーソルの子供([0]左(L) [1]真ん中(C) [2]右(R))
	public GameObject[] CCJugeOnly;						// カーソルの子供(撮影範囲の確認用 CC=cursorChilds)
	private Rigidbody cursorRB;							// カーソルのRB(RigidBody)用
	private SpriteRenderer cursorSR;					// カーソルのSP(SpriteRenderer)用
	public GameObject reBulletPrefab;					// Re用のバレット(当たり判定などを変更)
	public Sprite[] cursorSprite = new Sprite[3];		// cursorSprite(3種)を入れる([0]撮影 [1]攻撃 [2]撮影範囲(緑))
	//public BoxCollider cursorRestriction;				// カーソルの移動制限用→もしかしたら座標に変えるかも
	private int photoFrame = 0;							// 撮影の間隔
	private int judgmentCC = 1;							// カーソルの子オブジェクトの判定用
	public float shot = 0;								// 命中率用、発射回数
	public float shotNoHit = 0;							// 命中率用、当たってない回数
	//public Color inAreaColor;							// エリア内に入った時のカーソルの色
	private int inAreaReset = 0;							// カーソルの色が変わったら戻すために使用
	public GameObject crackObj;							// 画面のひび割れ
	public SpriteRenderer crackSR;						// ↑のSR
	public Sprite[] crackImgs;							// 各種画面のひび割れのリスト
	public bool parFlag = true;							// 弾のヒットエフェクトのありなし(true:あり false:なし)

	// 各種スクリプト取得用
	public EnergyRe energy;								// ゲージ用
	public UIManager uiManager;							// HP用
	public Score score;
	public Pause pause;
	public TitleBGM titleBGM;							// タイトルに戻った時にBGMを再生
	public MainBGM mainBGM;								// デバック用
	public AudioClip audioClip;							// デバック用、オーディオソース

	//--------------------------------------
	void Start()
	{
		// Rigidbody2Dの情報を格納・代入
		rigidbody = GetComponent<Rigidbody>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		cameras[0] = GameObject.Find("Camera01");
		cameras[1] = GameObject.Find("Camera02");      // 探して代入
		cameras[2] = GameObject.Find("Camera03");
		enemy = FindObjectOfType<EnemyAction>();
		// ついているコンポーネントを代入
		camera2RD = cameras[1].GetComponent<Rigidbody>();
		camera3RD = cameras[2].GetComponent<Rigidbody>();
		// 1カメにバレットを生成させる為のオブジェクトのrigidbody2d
		bsr = bulletSpownerObj.GetComponent<Rigidbody>();
		// 点滅オブジェクトを非表示
		darkSp.enabled = false;
		// 暗転オブジェクトを非表示
		blackSp.color = new Color(1, 1, 1, 0);
		// カーソルの色を赤にする
		spriteRenderer.color = new Color(1, 0, 0);
		// 
		chainCursorSp.color = new Color(0, 1, 0.5f);
		// 2カメのカーソルを赤にする
		sp2.color = new Color(1, 0, 0);
		//
		hpMax = hp;
		// 連鎖フレームを初めから連鎖しないように初期化
		powerFrame = 500;
		powerFrame2 = 500;
		// [Re]追加部分
		//cursorobj = GameObject.Find("Cursor02");
		cursorobj = GameObject.Find("ReCursor");				// Re用
		cursorRB = cursorobj.GetComponent<Rigidbody>();
		cursorSR = cursorobj.GetComponent<SpriteRenderer>();
		for(int i = 0; i < 3; i++)
		{
			cursorChilds[i].SetActive(false);
			CCJugeOnly[i].SetActive(false);
		}
		energy = FindObjectOfType<EnergyRe>();
		uiManager = FindObjectOfType<UIManager>();
		score = FindObjectOfType<Score>();
		titleBGM = FindObjectOfType<TitleBGM>();
		mainBGM = FindObjectOfType<MainBGM>();
	}
	//--------------------------------------
	void Update()
	{
	}
	// デバッグ用関数
	public void DebugAction()
	{
		// デバッグ用パワーアップ-------------
		//if (Input.GetKey(KeyCode.V))
		//{
		//	powerUpFrame += powerUpFrameMax;
		//	sp2.color = new Color(0.5f, 0, 1);
		//	if (powerUpFrame >= power2UpFrameMax)
		//	{
		//		sp2.color = new Color(0.1f, 1, 1);
		//	}
		//}
		//if (hp < hpMax) { recoveryFrame++; }
		//if (recoveryFrame >= recoveryFrameMax) { hp++; recoveryFrame = 0; }
		// デバッグ用入力--------------------
		inputY = 0;
		inputX = 0;
		if (dInputH[0]) { inputY++; }
		if (dInputH[1]) { inputY--; }
		if (dInputW[0]) { inputX++; }
		if (dInputW[1]) { inputX--; }
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene("re_title");
			mainBGM.audioSource.Stop();
			mainBGM.audioSource.clip = audioClip;
			titleBGM.rePlay = true;
		}
		// [Re]デバッグ
		if (Input.GetKey(KeyCode.Tab))					// パワーの追加
		{
			++power;
		}
		if (Input.GetKeyDown(KeyCode.Backspace))		// リザルト画面へ
		{
			score.finishFlag = true;
			SceneManager.LoadScene("clear");
		}
		//----------------------------------
	}
	//------------------------------------------
	// プレイヤーのアクションをする(実質Update関数)
	//------------------------------------------
	public void Action()
	{
		// 入力
		if (DEBUGMODE == 0)
		{
			inputY = 0;
			inputX = 0;
			if (isLife)
			{
				// 左右の入力を取得（X軸）					2019.02.25 十字キーにも対応
				if (Input.GetAxisRaw("Horizontal") != 0)
				{
					inputX = Input.GetAxisRaw("Horizontal");
				}
				else if (Input.GetAxisRaw("CrossX") != 0)
				{
					inputX = Input.GetAxisRaw("CrossX");
				}
				if (Setting.FlipHorizontal == true)
				{
					// 反転した左右の入力を取得（X軸）
					if (Input.GetAxisRaw("Horizontal") != 0)
					{
						inputX = -Input.GetAxisRaw("Horizontal");
					}
					else if (Input.GetAxisRaw("CrossX") != 0)
					{
						inputX = -Input.GetAxisRaw("CrossX");
					}
				}
				// 上下の入力を取得（Y軸）
				if (Input.GetAxisRaw("Vertical") != 0)
				{
					inputY = Input.GetAxisRaw("Vertical");
				}
				else if (Input.GetAxisRaw("CrossY") != 0)
				{
					inputY = Input.GetAxisRaw("CrossY");
				}
				if (Setting.FlipUpDown == true)
				{
					// 反転した上下の入力を取得（Y軸）
					if (Input.GetAxisRaw("Vertical") != 0)
					{
						inputY = -Input.GetAxisRaw("Vertical");
					}
					else if (Input.GetAxisRaw("CrossY") != 0)
					{
						inputY = -Input.GetAxisRaw("CrossY");
					}
				}
			}
		}

		// hpがなくなったらクリア(リザルト)へ		2019.01.31 変更
		if (hp <= 0)
		{
			// カーソルを非表示
			//spriteRenderer.color = new Color(1, 1, 1, 0);
			//isLife = false;
			blackSp.color += new Color(0, 0, 0, 0.03f);
			
			//score.finishFlag = true;
			if (blackSp.color.a >= 1)
			{
				SceneManager.LoadScene("clear");
			}
	}
		// それぞれのフレームを減算
		attackFrame--;
		collectFrame--;								// 2018.07.11
		// アタックフレームが0以下になったら0に戻す
		if (attackFrame < 0) { attackFrame = 0; }
		if (collectFrame < 0) { collectFrame = 0; }	// 2018.07.11
		// 大きさをもとに戻す処理
		if (updateFrame != 0)
		{
			--updateFrame;
		}
		else if (transform.localScale.x > 1)
		{
			transform.localScale -= new Vector3(0.02f, 0.02f, 0.02f);
			if (transform.localScale.x < 1)
			{
				transform.localScale = new Vector3(1, 1, 1);
			}
		}
		// エネミーに力の情報を渡す 2018.10.17
		//enemy.playerPower = power;

		//// 1カメのカーソルの色を変える 2018.07.13
		//if (ghostIn)
		//{
		//	// 緑
		//	spriteRenderer.color = new Color(0, 1, 0.5f);
		//}
		//else
		//{
		//	// 赤
		//	spriteRenderer.color = Color.red;
		//}
		// 連鎖を解除するためのカウント
		powerFrame++;
		powerFrame2++;
		// 連鎖中は点滅させる
		if (powerFrame < 60)
		{
			chainCursorSp.enabled = true;
			// 色を変える
			if (powerFrame % 3 == 0)
			{
				Color temp = new Color(-1 * chainColor, 1 * chainColor, 0.5f * chainColor);
				chainCursorSp.color += temp;
				chainColor *= -1;
			}
		}
		else
		{
			chainCursorSp.enabled = false;
		}
		// ダメージを受けているときのアクション
		if (isDamage)
		{
			isDamage = false;
			damageFrame = damageFrameMax;
			// [Re]
			if (hp <= 2)
			{
				uiManager.colorChange = true;
			}
			uiManager.Damages(true);
			crackSR.sprite = crackImgs[hp];
			crackSR.color = new Color(crackSR.color.r, crackSR.color.g, crackSR.color.b, 1);
			BGMMaster.instance.SoundEffect(11);
		}
		if (damageFrame > 0)
		{
			--damageFrame;
			// [Re]
			crackSR.color -= new Color(0, 0, 0, 0.015f);			// 2019.02.26
			//if (damageFrame % 2 == 0)
			//{
			//	DamageAction();
			//}
		}
		//// 3カメになった時の処理
		//if (cm.nowCamera == 2)
		//{
		//	// 他のカメラでの移動量を0にする
		//	rigidbody.velocity = Vector3.zero;
		//	// 他のカメラでの移動量を0にする
		//	camera2RD.velocity = Vector3.zero;
		//	bsr.velocity = Vector3.zero;
		//	// 3カメの処理
		//	Camera03();
		//}
		// 2カメになった時の処理
		//else if (cm.nowCamera == 1)
		//{
		//	// 他のカメラでの移動量を0にする
		//	rigidbody.velocity = Vector3.zero;
		//	// 他のカメラでの移動量を0にする
		//	camera3RD.velocity = Vector3.zero;
			// 2カメの処理
			Camera02();
		//}
		//// 1カメになった時の処理
		//else if (cm.nowCamera == 0)
		//{
		//	// 他のカメラでの移動量を0にする
		//	camera2RD.velocity = Vector3.zero;
		//	bsr.velocity = Vector3.zero;
		//	// 他のカメラでの移動量を0にする
		//	camera3RD.velocity = Vector3.zero;
		//	// 1カメの処理
		//	Camera01();
		//}
		// 点滅オブジェクトを非表示にする
		if (darkFrame-- == 0)
		{
			darkSp.enabled = false;
		}

		// 2018.12.17
		if (hp == 5)
		{
			score.noDamageFlag = true;
		}
		else
		{
			score.noDamageFlag = false;
		}

		// 2019.02.26 パーティクルのオンオフ
		if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
		{
			if (parFlag == true)
			{
				parFlag = false;
			}
			else
			{
				parFlag = true;
			}
		}
	}
	//-----------------------------------------
	// 1カメがアクティブな時
	//-----------------------------------------
	void Camera01()
	{
		// 移動する向きを求める
		direction = new Vector2(inputX, inputY).normalized;
		// 移動する向きとスピードを代入する
		rigidbody.velocity = direction * speed * DEBUGDOUBLE;
	}
	//-----------------------------------------
	// 2カメがアクティブな時
	//-----------------------------------------
	void Camera02()
	{
		//// カメラを移動させる向きを求める
		//cameraDirection = new Vector3(inputX, 0.0f, -inputY).normalized;
		//// 移動する向きを代入
		//camera2RD.velocity = cameraDirection * speed * DEBUGDOUBLE;
		//// 壁の位置が変わらない限り変わらない
		//const float kaenaide = 0.88f;
		//bsDir = new Vector2(camera2RD.velocity.x * kaenaide, 0);
		//bsr.velocity = bsDir;

		// [Re]カーソルでの移動(出来なくはないけど、撮影の調整が必要)
		cameraDirection = new Vector3(inputX, 0.0f, -inputY)/*.normalized*/;
		cursorRB.velocity = cameraDirection * speed * DEBUGDOUBLE;

		// 移動制限
		if (cursorobj.transform.position.x <= -45)
		{
			cursorobj.transform.position = new Vector3(-45, cursorobj.transform.position.y, cursorobj.transform.position.z);
		}
		else if (cursorobj.transform.position.x >= -21)
		{
			cursorobj.transform.position = new Vector3(-21, cursorobj.transform.position.y, cursorobj.transform.position.z);
		}
		if(cursorobj.transform.position.z <= -16.5)
		{
			cursorobj.transform.position = new Vector3(cursorobj.transform.position.x, cursorobj.transform.position.y, -16.5f);
		}
		else if (cursorobj.transform.position.z >= -3.5)
		{
			cursorobj.transform.position = new Vector3(cursorobj.transform.position.x, cursorobj.transform.position.y, -3.5f);
		}

		// 2018.12.03 移動に対してカーソルの当たり判定を変更( -37 → -33 → -29?(ここ変更多い) )
		if (cursorobj.transform.position.x <= -37)
		{
			judgmentCC = 2;
		}
		else if (cursorobj.transform.position.x >= -28f)
		{
			judgmentCC = 0;
		}
		else
		{
			judgmentCC = 1;
		}

		// 2019.01.13 撮影範囲を合わせる 2019.02.26 バグを回避するために多少変更
		for (int i = 0; i < 3; i++)
		{
			CCJugeOnly[i].SetActive(false);
			if (judgmentCC == i)
			{
				CCJugeOnly[i].SetActive(true);
			}
			if (cursorChilds[i].activeSelf == true)
			{
				cursorChilds[i].SetActive(false);
			}
		}

		// 2019.02.26
		if (cursorSR.sprite == cursorSprite[2])
		{
			++inAreaReset;
			if (inAreaReset == 10)
			{
				cursorSR.sprite = cursorSprite[0];
				inAreaReset = 0;
			}
		}

		// [Re] 撮影関係
		if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Joystick1Button4) || Input.GetKeyDown(KeyCode.Joystick1Button5))
		{
			if (photoFlag == true)
			{
				photoFlag = false;
				cursorSR.sprite = cursorSprite[0];
			}
			else
			{
				photoFlag = true;
				cursorSR.sprite = cursorSprite[1];
			}
			for (int i = 0; i < 3; i++)
			{
				cursorChilds[i].SetActive(false);
			}
			photoFrame = 0;
		}
		if (photoFlag == true)
		{
			// 入力されたら発射
			if ((Input.GetKey(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Joystick1Button1)
				|| Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.Joystick1Button3)) && attackFrame == 0)
			{
				// エネルギーがない		2018.03.03
				if (power == 0 && energy.gageMax == 0)
				{
					BGMMaster.instance.SoundEffect(13);
				}
				// エネルギーがあるとき
				if (power != 0)
				{
					// バレットの生成
					//GameObject bullet = Instantiate(bulletPrefab, cameras[1].transform.position, bulletPrefab.transform.rotation);
					// [Re]カーソル移動用
					//Vector3 bulletVec = new Vector3(cursorobj.transform.position.x, cursorobj.transform.position.y - 10, cursorobj.transform.position.z);
					//GameObject bullet = Instantiate(reBulletPrefab, bulletVec, reBulletPrefab.transform.rotation);
					GameObject bullet = Instantiate(reBulletPrefab, cameras[1].transform.position, reBulletPrefab.transform.rotation);
					// 2018.11.27 撃った回数の加算
					shot++;
					// パワーを減らす(2018.11.18修正、報告ありがとう！)
					power--;
					// SE再生
					BGMMaster.instance.SoundEffect(12);

					//// バレットの強化
					//if (powerUpFrame >= powerUpFrameMax)
					//{
					//	// バレットの情報を取得
					//	Bullet b = bullet.GetComponent<Bullet>();
					//	// 第三形態
					//	if (powerUpFrame >= power2UpFrameMax)
					//	{
					//		b.status = Bullet.BulletStatus.ThirdPower;
					//		BGMMaster.instance.SoundEffect(13);
					//	}
					//	// 第二形態
					//	else
					//	{
					//		b.status = Bullet.BulletStatus.SecondPower;
					//		BGMMaster.instance.SoundEffect(13);

					//	}
					//	InitBullet();
					//}
					//else
					//{
					//	BGMMaster.instance.SoundEffect(12);
					//}
				}
				else if (power == 0)
				{
					if (energy.gageMax != 0)
					{
						// バレットの生成
						//GameObject bullet = Instantiate(bulletPrefab, cameras[1].transform.position, bulletPrefab.transform.rotation);
						// [Re]カーソル移動用
						//Vector3 bulletVec = new Vector3(cursorobj.transform.position.x, cursorobj.transform.position.y - 10, cursorobj.transform.position.z);
						//GameObject bullet = Instantiate(reBulletPrefab, bulletVec, reBulletPrefab.transform.rotation);
						GameObject bullet = Instantiate(reBulletPrefab, cameras[1].transform.position, reBulletPrefab.transform.rotation);
						// 2018.11.27 撃った回数の加算
						shot++;
						energy.gageMax--;
						energy.coreFlag = true;
						power = 15;
						// SE再生
						BGMMaster.instance.SoundEffect(12);
					}
				}
				// フレームを代入
				attackFrame = attackFrameMax / (int)Mathf.Ceil(DEBUGDOUBLE);
			}
		}
		else
		{
			// [Re]撮影部分
			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Joystick1Button1)
				|| Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.Joystick1Button3))
			{
				for (int i = 0; i < 3; i++)
				{
					cursorChilds[i].SetActive(false);
					if (judgmentCC == i)
					{
						cursorChilds[i].SetActive(true);
					}
				}
				BGMMaster.instance.SoundEffect(1);
			}
			if (cursorChilds[judgmentCC].activeSelf == true)
			{
				photoFrame++;
				if (photoFrame > 10)
				{
					for (int i = 0; i < 3; i++)
					{
						cursorChilds[i].SetActive(false);
					}
				}
				else
				{
					photoFrame = 0;
				}
			}
		}
	}
	//-----------------------------------------
	// 3カメがアクティブな時
	//-----------------------------------------
	void Camera03()
	{
		// カメラを移動させる向きを求める
		cameraDirection = new Vector3(-inputX, 0.0f, -inputY).normalized;
		// 移動する向きを代入
		camera3RD.velocity = cameraDirection * speed * DEBUGDOUBLE;
		if ((Input.GetKey(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button5)) && collectFrame == 0 && power > 0)
		{
			Instantiate(bulletPrefab3, cameras[2].transform.position, bulletPrefab3.transform.rotation);
			collectFrame = attackFrameMax;			// 2018.07.11
			BGMMaster.instance.SoundEffect(20);
			// パワーを減らす
			power--;
		}
	}
	//------------------------------------
	// バレットのパワーアップ関数
	//------------------------------------
	public void PowerUpBullet()
	{
		++powerUpFrame;
		// 色を変える
		if (powerUpFrame >= power2UpFrameMax)
		{
			// 青
			sp2.color = new Color(0.1f, 1, 1);
		}
		else if (powerUpFrame >= powerUpFrameMax)
		{
			// 紫
			sp2.color = new Color(0.5f, 0, 1);
		}
	}
	//------------------------------------
	// バレットの強化状態を初期化する関数
	//------------------------------------
	public void InitBullet()
	{
		powerUpFrame = 0;
		sp2.color = new Color(1, 0, 0);
	}
	//------------------------------------
	// カーソルの速度を設定する 2019.03.02
	//------------------------------------
	public void SetCursorVelocity(Vector3 velocity)
	{
		cursorRB.velocity = velocity;
	}
	//-------------------------------
	// ダメージを受けたときのアクション
	//-------------------------------
	void DamageAction()
	{
		//if (damageCameraNum == 0)
		//{
		//	cameras[damageCameraNum].transform.position += new Vector3(0.1f * sign, 0, 0);
		//}
		//else if (damageCameraNum == 1)
		//{
		//	cameras[damageCameraNum].transform.position += new Vector3(0.2f * sign, 0, 0);
		//}
		//sign *= -1;
	}
	//-----------------------------------------
	// すり抜けオブジェクトに当たっている
	//-----------------------------------------
	//void OnTriggerStay(Collider col)
	//{
	//	if (cm.nowCamera == 0)
	//	{
	//		// ゴーストに当たっている時
	//		if (col.tag == "Goast" && isLife && !inDamageItem)
	//		{
	//			// ボタンが押されたら
	//			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button5))
	//			{
	//				BGMMaster.instance.SoundEffect (1);
	//				Score.instance.score2 = 50;
	//				// 点滅オブジェクトを1f表示
	//				darkSp.enabled = true;
	//				darkFrame = 2;
	//				// 赤
	//				spriteRenderer.color = new Color(1, 0, 0);
	//				// 1連鎖目
	//				if ((powerFrame < 60 && powerFrame != 0) && (powerFrame2 > 60 || powerFrame2 == 0))
	//				{
	//					// 同時に撮っても増分は2
	//					if (powerFrame2 > 60 || powerFrame2 != 0)
	//					{
	//						powerCnt += 2;
	//					}
	//					powerFrame = 1;
	//					powerFrame2 = 0;
	//					// ポイントの生成(デバッグ用)
	//					SpriteRenderer sp = Instantiate(powerUpText, col.transform.position, Quaternion.identity).GetComponent<SpriteRenderer>();
	//					sp.sprite = point2;
	//				}
	//				// 2連鎖目
	//				else if (powerFrame2 < 60 && powerFrame2 < 60)
	//				{
	//					// 同時に撮っても増分は3
	//					if (powerFrame != 1 && powerFrame2 != 1)
	//					{
	//						powerCnt += 3;
	//					}
	//					powerFrame = 1;
	//					powerFrame2 = 1;
	//					// ポイントの生成(デバッグ用)
	//					Instantiate(powerUpText, col.transform.position, Quaternion.identity);
	//				}
	//				// 連鎖なし
	//				else
	//				{
	//					// 同時に撮っても増分は1
	//					if (powerFrame != 0)
	//					{
	//						// カウント設定
	//						powerCnt++;
	//					}
	//					powerFrame = 0;
	//					// ポイントの生成(デバッグ用)
	//					SpriteRenderer sp = Instantiate(powerUpText, col.transform.position, Quaternion.identity).GetComponent<SpriteRenderer>();
	//					sp.sprite = point1;
	//				}
	//				while(powerCnt >= 3)
	//				{
	//					powerCnt -= 3;
	//					power++;
	//				}
	//				// 2018.06.30
	//				if (power >= 5)
	//				{
	//					power = 5;
	//					powerCnt = 0;
	//				}
	//			}
	//		}
	//	}
	//}
	void OnTriggerEnter(Collider oth)
	{
		if (oth.tag == "Goast")
		{
			// 緑
			spriteRenderer.color = new Color(0, 1, 0.5f);
			//ghostIn = true;		// 2018.07.13
			if (camera == false) {
				BGMMaster.instance.SoundEffect (4);
				camera = true;
			}
		}
	}
	void OnTriggerExit(Collider oth)
	{
		if (oth.tag == "Goast")
		{
			// 赤
			spriteRenderer.color = new Color(1, 0, 0);
			//ghostIn = false;	// 2018.07.13
			if (camera == true) {
				camera = false;
			}
		}
	}

	// 2019.01.15 オバケが撮影範囲に入ったらカーソルの色を変更
	public void PhotoArea(bool flag)
	{
		// カーソルが撮影状態の時だけ判断
		if (photoFlag == false)
		{
			// 撮影範囲内
			if (flag == true)
			{
				cursorSR.sprite = cursorSprite[2];
			}
			// 撮影範囲外
			else
			{
				cursorSR.sprite = cursorSprite[0];
			}
		}
	}
}

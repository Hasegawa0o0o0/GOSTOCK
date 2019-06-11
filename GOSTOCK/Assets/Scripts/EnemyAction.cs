// 更新
// 2018.06.25　アニメーション
//           　3カメの時は攻撃しないように修正
// 2018.06.27 矢印の追加
// 2018.06.28 魂と同じように変更
// 2018.06.30 強化状態の細かい数値の調整
// 2018.07.13 Pause中は動かないように設定
using UnityEngine;
using System.IO;                            // ログ書き出しに使用

public class EnemyAction : MonoBehaviour
{
	public enum EnemyStatus
	{
		Normal,
		Walk,
		PhysicsAttack,
		LightAttack,
		Damage,
		Dead,
		None,
	}
	public enum EnemyMode
	{
		first,
		second,
		third,
		forth,
		fifth,
	}
	//public float speed;						// エネミーの動く速度
	//public GameObject Enemy;				// エネミーのオブジェクトを入れる変数
	//private bool isLightAttack;				// お化けに対する攻撃ができるか否か
	//   private bool isPhysicsAttack;			// カメラに対する攻撃ができるか否か
	//   private bool isAction;					// エネミーが移動できるか否か
	public bool xInverted = false;          // 見えない壁xにぶつかったら反転するか否か
	public bool yInverted = false;          // 見えない壁yにぶつかったか否か
	public int actionDelay;                 // エネミーが行動するまでの時間
	public int actionDelayMax = 120;        // エネミーが行動するるまでの最大の時間
	public int frameCnt;                    // フレーム数カウント
	public EnemyStatus status;              // エネミーの行動状態
											//public Vector3 pos;						// 位置情報を入れる変数
	public float xSpeed;                    // x軸のスピード
	float nowXSpeed = 0;                    // 反映させるスピードX
	public float ySpeed;                    // y軸のスピード
	float nowYSpeed = 0;                    // 反映させるスピードY
	public float zSpeed;                    // z軸のスピード
	float nowZSpeed = 0;                    // 反映させるスピードZ
	//float addSpeed = 0.1f;					// 加速させる値
	private Rigidbody rb;
	GameObject enemy2;                      // 2カメのエネミー
	// 2018.06.27
	public SpriteRenderer[] arwSp = new SpriteRenderer[8];  // 矢印のスプライトレンダラー(unity)、右上から右回りに1,2,3,4,5,6,7,8番
															// 2018.06.28
	int renderFrame = 0;

	private Rigidbody c2Rigidbody;
	//private CircleCollider2D Scollider;
	Enemy2Status statusData2;
	//private Vector3 direction;
	SpriteRenderer spriteRenderer;
	public int ranNum = 0;                  // 行動選択用変数
	public int[] colFrame = new int[3] { 0, 0, 0 }; // 当たらなかったりするバグ防止用
	Anim body1Anim;                                 // 本体(1カメ)のアニメーション
	public Sprite[] body1Sprites = new Sprite[4];	// 本体(1カメ)のスプライト
	public Anim canon1Anim;                         // 大砲(1カメ)のアニメーション(unity側設定)
	public Sprite[] canon1Sprites = new Sprite[3];	// 大砲(1カメ)のアニメーション
	Anim body2Anim;                                 // 本体(2カメ)のアニメーション
	public Sprite[] body2Sprites = new Sprite[4];	// 本体(2カメ)のアニメーション
	public Anim canon2Anim;                         // 大砲(2カメ)のアニメーション(unity)
	public Sprite[] canon2Sprites = new Sprite[3];	// 大砲(2カメ)のアニメーション
	public Anim body3Anim;                          // 本体(3カメ)のアニメーション(unity)
	public Sprite[] body3Sprites = new Sprite[4];	// 本体(3カメ)のアニメーション
	public Anim canon3Anim;                         // 大砲(3カメ)のアニメーション(unity)
	public Sprite[] canon3Sprites = new Sprite[3];	// 大砲(3カメ)のアニメーション

	// ライト系---------------------------------------------------------------------------
	public GameObject lightCursor2;         // ライトカーソル_unity側設定
	LightCursor lc;                         // ライトのスクリプト
	public LineRenderer lineRenderer;		// ラインレンダラー_unity側設定
	int lAttackFrame = 0;
	public int lAttackFrameMax;
	public GameObject lightObj;
	public GameObject lightObj2;

	// 攻撃に使用した変数------------------------------------------------------------------
	public GameObject EAImageClose;         // エネミーの閉じたアームの画像
	public GameObject EAImageOpen;          // エネミーの開いてるアームの画像
	public GameObject damagePrefab;         // エネミーの物理攻撃のオブジェクト
	public GameObject damage2Prefab;
	private GameObject damageItem;          // エネミーの物理攻撃のオブジェクト(生成後用)
	public int AttackDelay = 500;           // エネミーが攻撃するまでのカウント(閉じて開くまでの時間)
	public int PAttackMax;                  // エネミーの攻撃のフレーム管理に使用(最大値)
	public int pAttackMaxLate;				// プラスのところ
	int attackFrame = 0;
	//public float zspeed;                    // ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ
	public bool PAttack = false;            // オブジェクトを生成しているか
	public int NowCamera;                   // CM君の情報を貰う変数
	DamageItem di;
	Vector3 shotItemPos;					// ダメージアイテムが発射された位置
	int pObjMoveFrame = 15;
	public int playerPower = 0;
	//-------------------------------------------------------------------------------------
	public Score score;
	public EnemyMode mode;
	int modeCheckNum = 0;

	// 攻撃を受けた時に使う------------------------------------------------------------------
	public int damageFrame = 0;             // ダメージを受けてからのフレーム数
	public const int damageFrameMax = 30;   // ダメージ時のアクションのフレーム数
	public int sign = 1;                    // 左右に揺らすときの符号
	public float nockbackSpeed = -0.2f;         // ノックバックするスピード

	//デバッグ用--------------------------------------------
	int recoveryFrame;					// 回復
	const int recoveryFrameMax = 60;
	//-----------------------------------------------------

	public Camera camera02;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody>();
		enemy2 = GameObject.Find("Enemy2");
		statusData2 = enemy2.GetComponent<Enemy2Status>();
		c2Rigidbody = enemy2.GetComponent<Rigidbody>();
		status = EnemyStatus.Walk;
		body1Anim = GetComponent<Anim>();
		body2Anim = enemy2.GetComponent<Anim>();
		// アームを開いた状態に設定
		EAImageClose.SetActive(true);
		EAImageOpen.SetActive(false);
		lightObj.SetActive(false);

		lc = lightCursor2.GetComponent<LightCursor>();
		lineRenderer.enabled = false;
		// 情報を格納
		score = GetComponent<Score> ();
		// 物理攻撃関係の情報の初期化
		AttackDelay = 500;
		PAttackMax = AttackDelay - 120;
		pAttackMaxLate = AttackDelay - PAttackMax - 1;
		// スピードの初期化
		nowXSpeed = xSpeed / 3;
		nowYSpeed = ySpeed / 3;
		nowZSpeed = zSpeed / 3;
		// 非表示
		for (int i = 0; i < 8; ++i)
		{
			arwSp[i].enabled = false;
			arwSp[i].color = new Color(1, 1, 1, 1);
		}
	}

	void Update()
	{
	}
	// デバッグ用関数
	public void DebugAction()
	{
		// デバッグ用----------------------------------------------------
		// 回復
		//if (statusData2.Hp < 10) { recoveryFrame++; }
		//if (recoveryFrame >= recoveryFrameMax) { statusData2.Hp++; recoveryFrame = 0; }
		// 強化
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			statusData2.Hp = 8;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			statusData2.Hp = 6;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			statusData2.Hp = 4;
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			statusData2.Hp = 2;
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			statusData2.Hp = 1;
		}
		//--------------------------------------------------------------
	}
	//-----------------------------------------
	// エネミーのアクションをする(実質Update関数)
	//-----------------------------------------
	public void Action(int pHp)
	{
		// Pause中は動かないように 2018.07.13
		if (Time.timeScale == 0)
		{
			return;
		}
		// ラインの結ぶ位置を更新
		lineRenderer.SetPosition(0, enemy2.transform.localPosition);
		lineRenderer.SetPosition(1, lightCursor2.transform.localPosition);
		// 本体のアニメーション
		body1Anim.Animation(body1Sprites, 4);
		body2Anim.Animation(body2Sprites, 4);
		body3Anim.Animation(body3Sprites, 4);
		// 体力が無くなったのでエネミーを消す
		if (statusData2.Hp <= 0)
		{
			Destroy(enemy2);
			Destroy(gameObject);
			//SceneManager.LoadScene("clear");
		}
		// ライトカーソルを画面外に行かないように補正
		lc.CheckPos();
		//// 矢印の表示 2018.06.27
		//Vector3 distance = enemy2.transform.localPosition;
		//distance.x -= CM.camera02.transform.localPosition.x;
		//distance.y -= CM.camera02.transform.localPosition.y;
		//// 画面外の敵に合わせて点滅表示
		//// 右上
		//if (distance.x > 10.5f && distance.y > 7.5)
		//{
		//	ShowArrow(0, 8);
		//	FlashingArrow(0, 8);
		//}
		//// 右下
		//else if (distance.x > 10.5f && distance.y < -7.5)
		//{
		//	ShowArrow(2, 8);
		//	FlashingArrow(2, 8);
		//}
		//// 左下
		//else if (distance.x < -10.5f && distance.y < -7.5)
		//{
		//	ShowArrow(4, 8);
		//	FlashingArrow(4, 8);
		//}
		//// 左上
		//else if (distance.x < -10.5f && distance.y > 7.5)
		//{
		//	ShowArrow(6, 8);
		//	FlashingArrow(6, 8);
		//}
		//// 上
		//else if (distance.y > 7.5)
		//{
		//	ShowArrow(7, 8);
		//	FlashingArrow(7, 8);
		//}
		//// 右
		//else if (distance.x > 10.5f)
		//{
		//	ShowArrow(1, 8);
		//	FlashingArrow(1, 8);
		//}
		//// 下
		//else if (distance.y < -7.5)
		//{
		//	ShowArrow(3, 8);
		//	FlashingArrow(3, 8);
		//}
		//// 左
		//else if (distance.x < -10.5f)
		//{
		//	ShowArrow(5, 8);
		//	FlashingArrow(5, 8);
		//}
		//// 画面内にいるときは表示しない
		//else
		//{
		//	for (int i = 0; i < 8; ++i)
		//	{
		//		arwSp[i].enabled = false;
		//	}
		//}
		// 矢印の表示 2018.06.28
		Vector3 distance = enemy2.transform.localPosition;
		Vector3 visiblePos = distance;
		// 距離の設定
		distance.x -= camera02.transform.localPosition.x;
		distance.z -= camera02.transform.localPosition.z;
		distance.y -= camera02.transform.localPosition.y;
		// 見える位置の設定
		visiblePos.z = visiblePos.y = visiblePos.x = distance.z;
		// 右上
		if ((Mathf.Abs(distance.x) > Mathf.Abs(visiblePos.x) && Mathf.Sign(distance.x) > 0) && (Mathf.Abs(distance.y) > Mathf.Abs(visiblePos.y - 3) && Mathf.Sign(distance.y) > 0))
		{
			ShowArrow(0, 8);
			FlashingArrow(0, 8);
		}
		// 右下
		else if ((Mathf.Abs(distance.x) > Mathf.Abs(visiblePos.x) && Mathf.Sign(distance.x) > 0) && (Mathf.Abs(distance.y) > Mathf.Abs(visiblePos.y - 3) && Mathf.Sign(distance.y) < 0))
		{
			ShowArrow(2, 8);
			FlashingArrow(2, 8);
		}
		// 左下
		else if ((Mathf.Abs(distance.x) > Mathf.Abs(visiblePos.x) && Mathf.Sign(distance.x) < 0) && (Mathf.Abs(distance.y) > Mathf.Abs(visiblePos.y - 3) && Mathf.Sign(distance.y) < 0))
		{
			ShowArrow(4, 8);
			FlashingArrow(4, 8);
		}
		// 左上
		else if ((Mathf.Abs(distance.x) > Mathf.Abs(visiblePos.x) && Mathf.Sign(distance.x) < 0) && (Mathf.Abs(distance.y) > Mathf.Abs(visiblePos.y - 3) && Mathf.Sign(distance.y) > 0))
		{
			ShowArrow(6, 8);
			FlashingArrow(6, 8);
		}
		// 上
		else if (Mathf.Abs(distance.y) > Mathf.Abs(visiblePos.y - 3) && Mathf.Sign(distance.y) > 0)
		{
			ShowArrow(7, 8);
			FlashingArrow(7, 8);
		}
		// 右
		else if (Mathf.Abs(distance.x) > Mathf.Abs(visiblePos.x) && Mathf.Sign(distance.x) > 0)
		{
			ShowArrow(1, 8);
			FlashingArrow(1, 8);
		}
		// 下
		else if (Mathf.Abs(distance.y) > Mathf.Abs(visiblePos.y - 3) && Mathf.Sign(distance.y) < 0)
		{
			ShowArrow(3, 8);
			FlashingArrow(3, 8);
		}
		// 左
		else if (Mathf.Abs(distance.x) > Mathf.Abs(visiblePos.x) && Mathf.Sign(distance.x) < 0)
		{
			ShowArrow(5, 8);
			FlashingArrow(5, 8);
		}
		// 画面内にいるときは表示しない
		else
		{
			for (int i = 0; i < 8; ++i)
			{
				arwSp[i].enabled = false;
				arwSp[i].color = new Color(1, 1, 1, 1);
			}
		}
		switch (status)
		{
			case EnemyStatus.Normal:
				break;
			case EnemyStatus.Walk:
				// モード切り換え
				if (statusData2)
				{
					//Modecheck(Score.instance.totalScore, statusData2.Hp);
				}
				// ↓後回し
				//nowXSpeed += addSpeed;
				//nowYSpeed += addSpeed;
				//nowZSpeed += addSpeed;
				// 移動
				Move();
				// ライト攻撃----------------------------------------
				lightObj.SetActive(true);
				lightObj2.SetActive(true);
				//--------------------------------------------------
				// 2カメのライトカーソル移動
				lc.MoveCursor();
				// 行動選択
				ranNum = Random.Range(0, 256);
				actionDelay++;
				// 1fに1/256の確立、または設定した時間が過ぎたら行動が変わる
				if (ranNum % 256 < 1 || actionDelay > actionDelayMax)
				{
					// 体力に応じてランダムに行動選択
					// ライトと物理の最終的な比は1:2
					// 初期は4:1
					int hpNum = statusData2.Hp;
					if (hpNum < 3){ hpNum = 3; }
					int num = Random.Range(0, hpNum);
					// 物理攻撃
					//if (num < 2 || playerPower >= 4)
					if (true)
					{
						status = EnemyStatus.PhysicsAttack;
						// ライトオフ
						lc.spriteRenderer.sprite = lc.lightOff;
						lightObj.SetActive(false);
						lightObj2.SetActive(false);
						lineRenderer.enabled = false;
					}
					// ライト攻撃
					else
					{
						status = EnemyStatus.LightAttack;
						lightObj.SetActive(true);
						lightObj2.SetActive(true);
						lineRenderer.enabled = true;
						// 向く方向を取得
						Vector3 look = lightCursor2.transform.position - enemy2.transform.position;
						// 向く
						lightObj.transform.localRotation = Quaternion.LookRotation(look,Vector3.up);
						lAttackFrame = lAttackFrameMax;
						lc.rigidbody.velocity = Vector3.zero;
					}
					// 動きを止める
					//rb.velocity = Vector2.zero;
					//c2Rigidbody.velocity = Vector3.zero;
					// 変数のリセット
					actionDelay = 0;
					ranNum = 99;
				}
				break;
			case EnemyStatus.PhysicsAttack:
			//bool one = false;
				// 移動
				Move();
				// ライトカーソルのスピード補正
				lc.SetSpeed();
				// フレーム数管理(左辺最少(固定)、右辺最大)、物理アイテムの生成、アームを開く
				if (attackFrame % AttackDelay >= 0 && attackFrame % AttackDelay <= PAttackMax)
				{

					//if (CM.nowCamera == 0)
					//{
						// キャノンの上げるアニメーション
						//if (canon1Anim.spriteRenderer.sprite != canon1Sprites[2])
						//{
						//	canon1Anim.Animation(canon1Sprites, 3);
						//}
						if (canon2Anim.spriteRenderer.sprite != canon2Sprites[2])
						{
							canon2Anim.Animation(canon2Sprites, 3);
						}
						//if (canon3Anim.spriteRenderer.sprite != canon3Sprites[2])
						//{
						//	canon3Anim.Animation(canon3Sprites, 3);
						//}
					//}
					// アームを開く
					EAImageClose.SetActive(false);
					EAImageOpen.SetActive(true);
					// 物理攻撃アイテムの生成
					if (attackFrame % AttackDelay == 0)
					{
						PAttack = true;
					}
					if (PAttack == true)
					{
						// 生成位置の設定
						Vector3 insPos = Vector3.zero;
						//if (CM.nowCamera == 0)
						//{

						//	insPos = EAImageClose.transform.position;
						//	//insPos.y -= 1;
						//	damageItem = Instantiate(damagePrefab, insPos, Quaternion.identity);

						//}
						//else if (CM.nowCamera == 1)
						//{
							insPos = enemy2.transform.position;
							insPos.y -= 0.5f;
							insPos.z -= 1;
							damageItem = Instantiate(damage2Prefab, insPos, damage2Prefab.transform.rotation);
						//BGMMaster.instance.SoundEffect (18);
						//}
						// 3カメの時は攻撃しない
						//else if (CM.nowCamera == 2)
						//{
						//	//insPos = enemy2.transform.position;
						//	//insPos.y += 0.5f;
						//	//damageItem = Instantiate(damage2Prefab, insPos, damage2Prefab.transform.rotation);
						//	// アタックフレームを0に戻す
						//	attackFrame = 0;
						//	status = EnemyStatus.Walk;
						//	return;
						//}
						// ダメージオブジェクトを一度親子関係にする
						damageItem.transform.parent = transform;
						// ダメージアイテムの情報を取得
						di = damageItem.GetComponent<DamageItem>();
						//di.nc = CM.nowCamera;
						di.nc = 1;
						damageItem.SetActive(false);
						PAttack = false;
					}

				}
				// フレーム数管理(左辺最少(固定)、右辺最大)
				else if (attackFrame % AttackDelay > PAttackMax && attackFrame % AttackDelay <= PAttackMax + pAttackMaxLate)
				{
					// 大砲のアニメーション(戻す)
					if (canon1Anim.spriteRenderer.sprite != canon1Sprites[0])
					{
						canon1Anim.Animation(canon1Sprites, 3, false);
					}
					if (canon2Anim.spriteRenderer.sprite != canon2Sprites[0])
					{
						canon2Anim.Animation(canon2Sprites, 3, false);
					}
					if (canon3Anim.spriteRenderer.sprite != canon3Sprites[0])
					{
						canon3Anim.Animation(canon3Sprites, 3, false);
					}
					if (damageItem)
					{
					//if (one == false) {
					//	//BGMMaster.instance.SoundEffect (18);
					//	one = true;
					//}
						if (damageItem.transform.parent)
						{
							// 親子関係を解除する
							damageItem.transform.parent = null;
							// ダメージアイテムを表示
							damageItem.SetActive(true);
							BGMMaster.instance.SoundEffect (18);
							// 近づけるときの距離の比率
							shotItemPos = damageItem.transform.position;
						}
						// 生成したアイテムを近づける
						di.MovePos(shotItemPos, pObjMoveFrame);
					}
					if (attackFrame % AttackDelay == PAttackMax + pAttackMaxLate)
					{
						//NowCamera = CM.nowCamera;
						if (damageItem)
							//if (di.nc == CM.nowCamera)
							//{
								// ダメージ
								di.ColcDamage();
								//CM.ShowCrack(pHp, di.nc);
								Debug.Log("Damage!");
							//}
						status = EnemyStatus.Walk;
						// アームを閉じる
						EAImageClose.SetActive(true);
						EAImageOpen.SetActive(false);
						// アタックフレームを0に戻す
						attackFrame = 0;
						// プレイヤーの状態を当たっていない状態にする
						di.OutDamageItem();
						// 飛んできたオブジェクトは邪魔なので消す
						Destroy(damageItem);
						break;
					}
				}
				else
				{
					// アームを閉じる
					EAImageClose.SetActive(true);
					EAImageOpen.SetActive(false);
				}
				++attackFrame;
				break;
			case EnemyStatus.LightAttack:
				// 一度onにする
				lc.spriteRenderer.sprite = lc.lightOn;
				// lightCursorのフレームをカウントする
				++lc.activeFrame;
				// こちらのカウントが0以下になったら状態を戻す
				if (lAttackFrame <= 0)
				{
					status = EnemyStatus.Walk;
					// ライトをoffにする
					lc.spriteRenderer.sprite = lc.lightOff;
					lightObj.SetActive(false);
					lightObj2.SetActive(false);
					lineRenderer.enabled = false;
					// lightCursorのカウントを０にする
					lc.activeFrame = 0;
				}
				lAttackFrame--;
				break;
		}
		// ダメージを受けていたらアクションに移行する
		if (statusData2.isDamage)
		{
			statusData2.isDamage = false;
			damageFrame = damageFrameMax;
			// ノックバック
			nockbackSpeed = 0.2f;
			//nowYSpeed = Mathf.Abs(ySpeed) * 1.25f;
		}
		// ダメージを受けているときのアクション
		if (damageFrame > 0)
		{
			--damageFrame;
			if (damageFrame % 2 == 0)
			{
				DamageAction();
			}
		}
		// パワーアップ
		switch (mode)
		{
		case EnemyMode.first:
			// 物理攻撃全体の長さ
			AttackDelay = 240;
			// 物を持っている時間を設定(後ろの数字は物が投げられている時間)
			PAttackMax = AttackDelay - 150;
			pAttackMaxLate = AttackDelay - PAttackMax - 1;
			pObjMoveFrame = 17;
			spriteRenderer.color = new Color(1, 1, 1);
			nowXSpeed = xSpeed / 3;
			nowYSpeed = ySpeed / 3;
			nowZSpeed = zSpeed / 3;
			break;
		case EnemyMode.second:
			// 物理攻撃全体の長さ
			AttackDelay = 210;
			// 物を持っている時間を設定(後ろの数字は物が投げられている時間)
			PAttackMax = AttackDelay - 120;
			pAttackMaxLate = AttackDelay - PAttackMax - 1;
			pObjMoveFrame = 13;
			spriteRenderer.color = new Color(1, 0.8f, 0.8f);
			nowXSpeed = xSpeed / 2.5f;
			nowYSpeed = ySpeed / 2.5f;
			nowZSpeed = zSpeed / 2.5f;
			break;
		case EnemyMode.third:
			// 物理攻撃全体の長さ
			AttackDelay = 180;
			// 物を持っている時間を設定(後ろの数字は物が投げられている時間)
			PAttackMax = AttackDelay - 90;
			pAttackMaxLate = AttackDelay - PAttackMax - 1;
			pObjMoveFrame = 10;
			spriteRenderer.color = new Color(1, 0.6f, 0.6f);
			nowXSpeed = xSpeed / 2;
			nowYSpeed = ySpeed / 2;
			nowZSpeed = zSpeed / 2;
			break;
		case EnemyMode.forth:
			// 物理攻撃全体の長さ
			AttackDelay = 125;
			// 物を持っている時間を設定(後ろの数字は物が投げられている時間)
			PAttackMax = AttackDelay - 75;
			pAttackMaxLate = AttackDelay - PAttackMax - 1;
			pObjMoveFrame = 8;
			spriteRenderer.color = new Color(1, 0.4f, 0.4f);
			nowXSpeed = xSpeed / 1.5f;
			nowYSpeed = ySpeed / 1.5f;
			nowZSpeed = zSpeed / 1.5f;
			break;
		case EnemyMode.fifth:
			// 物理攻撃全体の長さ
			AttackDelay = 120;				// default 100
			// 物を持っている時間を設定(後ろの数字は物が投げられている時間)
			PAttackMax = AttackDelay - 60;	// default 60
			pAttackMaxLate = AttackDelay - PAttackMax - 1;
			pObjMoveFrame = 7;
			spriteRenderer.color = new Color(1, 0.2f, 0.2f);
			nowXSpeed = xSpeed;
			nowYSpeed = ySpeed;
			nowZSpeed = zSpeed;
			break;
		}
		++frameCnt;         // フレームカウント
		for (int i = 0; i < 3; ++i)
		{
			colFrame[i]++;
		}
		// 移動制限
		// X
		if (transform.localPosition.x >= 4.5f)
		{
			float temp = Mathf.Abs(xSpeed);
			xSpeed = -temp;
		}
		else if (transform.localPosition.x <= -4.5f)
		{
			float temp = Mathf.Abs(xSpeed);
			xSpeed = temp;
		}
		// Y
		if (transform.localPosition.y >= 3.61)
		{
			float temp = Mathf.Abs(ySpeed);
			ySpeed = -temp;
		}
		else if (transform.localPosition.y <= -0.39f)
		{
			float temp = Mathf.Abs(ySpeed);
			ySpeed = temp;
		}
		// Z
		if (transform.localPosition.z >= 15)
		{
			float temp = Mathf.Abs(zSpeed);
			zSpeed = -temp;
		}
		else if (transform.localPosition.z <= 5)
		{
			float temp = Mathf.Abs(zSpeed);
			zSpeed = temp;
		}
	}

	// 移動関数
	void Move()
	{
		if (rb)
		{
			rb.velocity = new Vector3(nowXSpeed, nowYSpeed, nowZSpeed);
		}
		if (c2Rigidbody)
		{
			c2Rigidbody.velocity = new Vector3(nowXSpeed, nowYSpeed, nowZSpeed);
		}
	}

	//-------------------------------
	// ダメージを受けたときのアクション
	//-------------------------------
	void DamageAction()
	{
		// 震える
		transform.position += new Vector3(0.1f * sign, 0, 0);
		enemy2.transform.position += new Vector3(0.1f * sign, 0, 0);
		sign *= -1;
		Vector3 nockbackPos = new Vector3(0, nockbackSpeed, 0);
		transform.position += nockbackPos;
		enemy2.transform.position += nockbackPos;
		if (nockbackSpeed > -0.2f)
		{
			nockbackSpeed -= 0.015f;
			if (nockbackSpeed < -0.2f)
			{
				nockbackSpeed = -0.2f;
			}
		}
		//// 元のスピードに戻す
		//if (nowYSpeed > ySpeed)
		//{
		//	nowYSpeed -= ySpeed * 0.5f;
		//	if (nowYSpeed < ySpeed)
		//	{
		//		nowYSpeed = ySpeed;
		//	}
		//}
		//else if (nowYSpeed < ySpeed)
		//{
		//	nowYSpeed += ySpeed / 1.2f;
		//	if (nowYSpeed > ySpeed)
		//	{
		//		nowYSpeed = ySpeed;
		//	}
		//}
	}

	//---------------------------
	// 矢印の表示 2018.06.27
	//---------------------------
	void ShowArrow(int index, int indexMax)
	{
		for (int i = 0; i < indexMax; ++i)
		{
			if (i == index)
			{
				arwSp[i].enabled = true;
			}
			else
			{
				arwSp[i].enabled = false;
			}
		}
		// ここでカウントの設定
		++renderFrame;
	}

	//------------------------------
	// 点滅 2018.06.28
	//------------------------------
	void FlashingArrow(int index, int indexMax)
	{
		if (renderFrame % 10 == 0)
		{
			for (int i = 0; i < indexMax; ++i)
			{
				if (i == index)
				{
					if (arwSp[i].color.a > 0.5f)
					{
						arwSp[i].color = new Color(1, 1, 1, 0);
					}
					else
					{
						arwSp[i].color = new Color(1, 1, 1, 1);
					}
				}
			}
			renderFrame = 0;
		}
	}

	void Modecheck(int score, int hp)
	{
		if (hp >= 8)
		{
			mode = EnemyMode.first;
		}
		else if (hp >= 6)
		{
			mode = EnemyMode.second;
		}
		else if (hp >= 4)
		{
			mode = EnemyMode.third;
		}
		else if (hp >= 2)
		{
			mode = EnemyMode.forth;
		}
		else if (hp < 2)
		{
			mode = EnemyMode.fifth;
		}
	}

	void OnTriggerEnter(Collider oth)
	{
		// ↓たぶん意味ない
		if (oth.tag == "Bullet")
		{
			statusData2.Hp--;
		}
	}
}

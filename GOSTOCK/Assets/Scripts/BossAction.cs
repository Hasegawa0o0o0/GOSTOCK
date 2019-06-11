/*
 * BossAction.cs
 * 2018.10.17 作成
 *		2018.10.19 ガード作成
 *		2018.10.20 つかみ、パンチ作成
 *		2018.10.23 体力減少による行動選択の確立変動
 *		2018.10.24 ちょっとバグ防止
 *		2018.10.25 バグ防止、調整
 *		2018.10.26 両手シリーズ作成
 *				   いろいろ変更
 *		2018.10.27 ザコを持たせる
 *				   プレイヤーを回復させる
 *				   手を点滅させる
 *				   ガードの位置を調整
 *		2018.11.01 吸い込み
 *				   ワンツー
 *		2018.11.02 演出
 *		2018.11.06 手を握る、瞬きする
 *		2018.11.08 デバッグアクションを追加
 *		2018.11.16 パンチの時に手を開いてから動き出す
 *		2018.11.17 連続で瞬きさせない
 *		2018.11.27 吸い込み完了の直前におばけのスプライトを変える
 *				   手を開くやつの修正
 *		2018.12.04 吸い込む手にブラックホールを付けるための変更
 *		2018.12.06 お亡くなりになった時に爆発する
 *		2018.12.20 ザコの体当たりを追加
 *		2018.12.24 エラー防止追加
 *		2018.12.25 数値いじり
 *		2018.12.27 エラー防止、数値いじり
 *		2018.12.28 パンチのデバッグでも弾を当てる回数を3にする
 *		2019.01.03 回復パンチの時にスパークルを生成する
 *		2019.01.04 ボスの行動を遅くする(かなり追加)
 *		2019.01.05 回復パンチをしたときにUIにもキラキラを生成する
 *		2019.01.08 音を鳴らす
 *		2019.01.08 アニメーション追加
 *		2019.01.09 アニメーション続き
 *		2019.01.13 手のノックバック
 *		2019.01.15 ばんそうこうを泣いているときから表示する
 *				   手のばんそうこうを表示する
 *		2019.01.19 無敵時間追加
 *				   回復パンチを体力が半分以上の時にしかしないように修正
 *				   テレポート追加
 *				   手の位置がおかしくなるのを修正
 *		2019.01.23 パーティクルの量などの調整
 *		2019.01.27 カウンター追加
 *				   ザコの位置調整
 *		2019.01.31 カウンター前に震える
 *		2019.02.03 ザコの位置調整
 *				   テレポートの演出を変更
 *		2019.02.11 体力が0になった時にもノックバックをするように修正
 *		2019.02.15 パンチの3回目のノックバック少し長く
 *		2019.02.16 ボスが怒る
 *		2019.02.19 ボスが怒った時に出るやつの調整
 *		2019.02.20 お亡くなりのやつを変更
 *		2019.02.24 バグ修正
 *		2019.02.26 いろいろ調整
 *		2019.02.27 カウンターのパンチ解除はダメージを受けない
 *		2019.03.02 怒りマークを揺らさない
 *		2019.03.03 ボスが倒された時に手紙が出るようになる
 *		2019.03.18 CSVファイルの読み込み
 *		2019.03.20 手紙が出現するタイミング調整
 *		2019.03.30 ばんそうこうが揺れる
 *		2019.04.03 体力が2/3を切った時にも怒る
 *		2019.04.06 ↑体力が4減った時に変更
 *		2019.04.13 体力減少バグ修正
 */
/* ボスの操作 */
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;

public class BossAction : MonoBehaviour
{
	// ボスの両手構造体
	public struct BothHands
	{
		public BossHand right;
		public BossHand left;
	}

	// ボスの行動集
	public enum BossConduct
	{
		MOVE,			// ただ動く
		PUNCH,			// 殴る
		PUSH,			// おばけを追い払う
		GUARD,			// 守る
		DELIVER,		// ザコを繰り出す 2018.10.27
		RECOVER,		// 回復 2018.10.27
		INHOLE,			// 吸い込み 2018.11.01
		ONETWO,			// ワンツー 2018.11.01
		TELEPORT,		// テレポート 2019.01.19
		COUNTERBLOW,	// カウンター 2019.01.27
		NONE,
	}

	// 動かす手
	public enum ActionHands
	{
		RIGHT,  // 右手
		LEFT,   // 左手
		BOTH,   // 両手
		NONE,
	}
	// 変数宣言--------------------------------------------------------------------
	BothHands hands;							// 両手
	Rigidbody rb;								// Rigidbody
	BossConduct nowAction;						// ボスがする行動
	BossConduct previousAction;					// ボスがしていた行動 2019.02.03
	public const int maxHp = 15;				// 最大体力 2018.10.27
	int hp = maxHp;								// 体力
	ActionHands actionHands = ActionHands.NONE;	// アクションする手
	SphereCollider sphereCollider;				// 判定
	Anim anim;									// アニメーション 2018.11.06
	ParticleSystem deadParticle;				// ぽろぽろ出てくるやつ 2019.02.20
	public GameObject deadEffect;				// お亡くなりになった時に生成する爆発 2018.12.06
	int angryFrame = 0;							// 体力が半分になった時に一度だけ怒る 2019.02.16
	int angryFrameMax = 600;					// 怒るフレーム数 2019.02.16
	public GameObject dropItemPrefab;			// クラッコが倒された時に出てくるもの(手紙) 2019.03.03
	SpriteRenderer dropItem;					// クラッコが倒された時に出てくるものを管理する 2019.03.03

	int actionFrame = 0;                // 移動以外の行動に移るまでの時間
	public int actionFrameMax;          // 移動以外の行動に移るまでの最大時間

	float multipleSpeed = 2f;			// スピードに乗算する値
	Vector3 speed = Vector3.one;		// 本体のスピード
	float handsRadius = 1.0f;           // 手が回る時の半径
	float handsSpeedRadian = 0.0f;      // 手が中心から何度の位置にあるか

	public int attackWaitFrame;                         // 攻撃がされるまでのフレーム数
	GameObject playerCamera;                            // プレイヤーの実体
	bool endAttack = false;                             // 攻撃が終わったか
	bool[] endOneTwo = new bool[2] { false, false };    // ワンツー攻撃が終わったか 2018.11.01

	bool isGetGhost = true;             // おばけをつかめたか

	public GameObject blackholeObject;					// 吸い込むときのブラックホール 2018.12.04
	int inholeGhistCnt = 0;								// 吸い込んだ数 2019.01.19
	int inholeGhostCntMax = 0;							// 吸い込みに移った時の吸い込むおばけの数 2019.01.19

	public GameObject zakoPrefab;									// ザコプレハブ 2018.10.27
	List<ZakoAction> zakoObj = new List<ZakoAction>();				// ザコオブジェクト
	List<Vector2Int> zakoTargetPosList = new List<Vector2Int>();	// ザコの目指す位置 2019.01.27
	bool isZakoAttacking = false;									// 誰かザコが体当たりをしているか 2018.12.20
	int attackingZakoNum = 0;										// 体当たりしているザコの要素番号 2018.12.20
	bool endPointing = false;										// 指さしが終わったか 2018.10.27
	float shake;													// 縦にどれくらい動いたか 2018.01.19
	float addShake = 0.0005f;										// 揺れる速さ 2019.01.19
	int zakoBlinkFrame = 0;											// 無敵時間中のザコを点滅させるため 2019.02.24
	// ガードするときの手の位置(ローカル座標) 調整 2018.10.27
	Vector3[] guardHandsPos_local = new Vector3[2]
	{
		new Vector3(0.8f, 0.0f, -1.5f),		// 右
		new Vector3(-0.9f, -1.0f, -1.25f)	// 左
	};
	public int guardFrameMax;

	// テレポート先の位置 2019.01.19
	Vector3 nextTereportPos = Vector3.zero;

	// アクションした後に戻る位置
	Vector3[] returnHandsPos_local = new Vector3[2] { Vector3.zero, Vector3.zero };

	// ダメージを受けた時に使う
	int damageFrame = 0;				// ダメージを受けてからのフレーム数
	const int damageFrameMax = 30;		// ダメージ時のアクションのフレーム数
	int sign = 1;						// 左右に揺らすときの符号
	float nockbackSpeed = -0.2f;		// ノックバックするスピード
	const int bandaidFrameMax = 150;	// ばんそうこうを表示する時間 2019.01.09
	int handNockbackFrame = 0;			// 手をノックバックさせるフレーム数 2019.01.13
	int invincibleFrame = 0;			// 無敵フレーム

	// 絵 2018.11.01
	int winkFrame = 10;															// 瞬き用フレーム 2018.11.06
	public Sprite[] bodySprite = new Sprite[5];									// 本体 2018.11.06
	public Sprite cancelAttackSprite;											// 攻撃が解除された時のスプライト 2019.02.26
	public Sprite normalHandSprite;												// 普通の手
	public Sprite[] knuckleHandSprite = new Sprite[2];							// こぶし 2018.11.06
	public Sprite punchHandSprite;												// パンチの赤
	public Sprite guardHandSprite;												// ガード
	public Sprite deliverHandSprite;											// 呼び出し
	public Sprite recoverHandSprite;											// 回復の緑
	public List<SpriteRenderer> bandaidSRList = new List<SpriteRenderer>();		// ばんそうこうリスト 2019.01.09
	public Sprite[] angrySprite = new Sprite[2];								// 怒った本体のスプライト 2019.02.16
	public List<SpriteRenderer> angryEffect1List = new List<SpriteRenderer>();	// 怒りマークリスト 2019.02.16
	public SpriteRenderer angryEffect2Prefab;									// 怒りけむり 2019.02.16
	List<SpriteRenderer> angryEffect2List = new List<SpriteRenderer>();			// 怒りけむりリスト 2019.02.16
	List<Vector3> angryEffect2Velocity = new List<Vector3>();					// 怒りけむりを移動させる速度リスト 2019.02.16

	// 2019.02.26 弾の被弾エフェクトを追加するために変更
	public ParticleSystem fireworkPar;					// 被弾エフェクト用パーティクル

	// 2019.03.22 オバケを止めるために使用
	public bool ghostStop = false;

	UIManager uiManager;								// 2018.11.13 HPのUI表示
	Score score;										// 2018.11.15 撃破の判定
	EnergyRe energyRe;									// 緑パンチでキラキラを出すために取得 2019.01.05
	PlayerControl playerControl;						// パーティクルのオンオフに使用 2019.02.26

	//------------------------------------------------------------------------------
	void Start()
	{
		// 情報を取得
		rb = GetComponent<Rigidbody>();
		playerCamera = GameObject.Find("Camera02");
		// 2018.11.02
		sphereCollider = GetComponent<SphereCollider>();
		sphereCollider.enabled = false;
		// 2018.11.06
		anim = GetComponent<Anim>();
		// 両手の情報を取得 !!!!子オブジェクトの並びは上から右->左にすること!!!!
		hands.right = transform.GetChild(0).GetComponent<BossHand>();
		hands.left = transform.GetChild(1).GetComponent<BossHand>();
		// 2019.02.20
		deadParticle = transform.Find("DeadParticle").GetComponent<ParticleSystem>();
		// 状態の設定
		nowAction = BossConduct.MOVE;
		previousAction = BossConduct.NONE;
		actionHands = ActionHands.NONE;
		hp = maxHp;
		// 2018.11.02
		returnHandsPos_local[0] = new Vector3(-2.5f, -1.0f, 0.0f);
		returnHandsPos_local[1] = new Vector3(2.5f, -1.0f, 0.0f);
		hands.right.inholeLevel = 15;
		hands.left.inholeLevel = 15;
		// 2018.11.13
		uiManager = FindObjectOfType<UIManager>();
		uiManager.DisplayEGages(true);
		// 2018.11.15
		score = FindObjectOfType<Score>();
		// 2019.01.05
		energyRe = FindObjectOfType<EnergyRe>();
		// 2019.02.26
		playerControl = FindObjectOfType<PlayerControl>();
		// 2019.01.09
		for (int i = 0; i < bandaidSRList.Count; ++i)
		{
			bandaidSRList[i].enabled = false;
		}
		// 怒りマークを消す 2019.02.16
		for (int i = 0; i < angryEffect1List.Count; ++i)
		{
			angryEffect1List[i].enabled = false;
		}
		// 2019.02.20
		deadParticle.Stop(true);
		// 2019.03.18
		OpenCSVAndSetStatus();
	}
	//--------------------------------------------------------------------------------
	// CSVファイルを読み込んでステータスを反映させる 2019.03.18
	void OpenCSVAndSetStatus()
	{
		FileInfo fi = new FileInfo(Application.dataPath + "/CSVFiles/BossStatus.csv");
		if (fi != null)
		{
			StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8);
			List<string[]> data = new List<string[]>();
			while(sr.Peek() != -1)
			{
				string line = sr.ReadLine();
				data.Add(line.Split(','));
			}
			angryFrameMax = int.Parse(data[1][1]);
			attackWaitFrame = int.Parse(data[2][1]);
			guardFrameMax = int.Parse(data[3][1]);
			multipleSpeed = float.Parse(data[4][1]);
			speed *= multipleSpeed;
		}
	}
	//--------------------------------------------------------------------------------
	void Update()
	{
	}

	// デバッグ用 2018.11.08-------------------------------------------------------------
	public void DebugAction()
	{
		if (nowAction != BossConduct.MOVE) return;
		bool isInput = false;
		if (isInput = Input.GetKeyDown(KeyCode.Alpha1))
		{
			nowAction = BossConduct.PUNCH;
			// 手の体力を設定 2018.12.28
			hands.right.hp = 3;
			hands.left.hp = 3;
		}
		else if (isInput = Input.GetKeyDown(KeyCode.Alpha2))
		{
			nowAction = BossConduct.INHOLE;
			inholeGhistCnt = 0;
			if (actionHands == ActionHands.RIGHT)
			{
				inholeGhostCntMax = hands.right.targetGhostList.Count;
				hands.right.isInhole = true;
			}
			else if (actionHands == ActionHands.LEFT)
			{
				inholeGhostCntMax = hands.left.targetGhostList.Count;
				hands.left.isInhole = true;
			}
		}
		else if (isInput = Input.GetKeyDown(KeyCode.Alpha3))
		{
			nowAction = BossConduct.GUARD;
		}
		else if (isInput = Input.GetKeyDown(KeyCode.Alpha4))
		{
			nowAction = BossConduct.ONETWO;
			// 手の体力を設定 2018.12.28
			hands.right.hp = 3;
			hands.left.hp = 3;
		}
		else if (isInput = Input.GetKeyDown(KeyCode.Alpha5))
		{
			nowAction = BossConduct.DELIVER;
		}
		else if (isInput = Input.GetKeyDown(KeyCode.Alpha6))
		{
			nowAction = BossConduct.RECOVER;
		}
		else if (isInput = Input.GetKeyDown(KeyCode.Alpha7))
		{
			nowAction = BossConduct.TELEPORT;
			nextTereportPos = new Vector3(Random.Range(-4.5f - 33f, 4.5f - 33f),
				Random.Range(-0.39f + 4f, 3.61f + 4f), Random.Range(-12.5f, -7f));
		}
		else if (Input.GetKeyDown(KeyCode.Alpha8))  // 2018.12.06
		{
			hp = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha0))  // 2019.01.09
		{
			winkFrame = 0;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha9))	// 2019.02.16
		{
			angryFrame = angryFrameMax;
		}
		if (isInput)
		{
			// スピードをそれぞれ0にする
			speed = Vector3.zero;
			hands.right.InitSpeed();
			hands.left.InitSpeed();
			// 元の位置を保存しておく
			returnHandsPos_local[0] = hands.right.transform.localPosition;
			returnHandsPos_local[1] = hands.left.transform.localPosition;
			// 前の行動を保存しておく 2019.02.03
			previousAction = nowAction;
			// 手の弾が当たった判定をなくしておく 2019.02.24
			hands.right.isHitBullet = false;
			hands.left.isHitBullet = false;
			actionFrame = 0;

		}
	}

	// メインのアクション 引数追加 2019.01.04-----------------------------------------------------------------
	public void Action(PlayerControl playerControl, bool clockworkFlag)
	{
		sign *= -1;
		// ザコを揺らす 2019.01.19
		shake -= addShake;
		if (Mathf.Abs(shake) > 0.02f) { addShake *= -1; }
		if (hp <= 0)
		{
			// 2018.12.06
			Dead();
			for (int i = 0; i < bandaidSRList.Count; ++i)
			{
				bandaidSRList[i].enabled = false;
			}
			// 怒りマークを消す 2019.02.16	2019.04.06
			for (int i = 0; i < angryEffect1List.Count; ++i)
			{
				angryEffect1List[i].enabled = false;
			}
			for (int i = 0; i < angryEffect2List.Count; ++i)
			{
				angryEffect2List[i].enabled = false;
			}
			// 2018.11.15 撃破のフラグを建てる
			score.defeatFlag = true;
			EnemyManager.bossDefeat = true;	// 2019.03.10
			return;
		}
		sphereCollider.enabled = true;  // 2018.11.02
		// ザコの行動を変更 2018.12.20	2019.01.04
		if (!isZakoAttacking && Random.Range(0,128) == 0)
		{
			SwitchZakoAction();
		}
		// ザコの操作(foreachをforに変更)2018.12.20 条件を追加 2018.12.24
		for (int i = 0; zakoObj.Count > 0 && i < zakoObj.Count; ++i)
		{
			// 処理の位置をfor文の下から変更 2018.12.27
			if (!isZakoAttacking || i != attackingZakoNum)
			{
				zakoObj[i].NormalAction();
				zakoObj[i].transform.position += new Vector3(0f, 0f, shake);
			}
			// 条件追加 2018.12.24
			if (isZakoAttacking && i == attackingZakoNum && zakoObj[attackingZakoNum].isActiveAndEnabled)
			{
				// 戻り値を代入 2018.12.24
				isZakoAttacking = !zakoObj[attackingZakoNum].Attack(playerControl, playerCamera);
			}
			// オブジェクトがアクティブでなければ排除する 2018.12.24
			else if (!zakoObj[i].isActiveAndEnabled)
			{
				GameObject deleteObj = zakoObj[i].gameObject;
				isZakoAttacking = false;
				zakoObj.RemoveAt(i);
				zakoTargetPosList.RemoveAt(i);
				Destroy(deleteObj);
				attackingZakoNum -= attackingZakoNum - 1 < 0 ? 0 : 1;
			}
			// 無敵のザコは点滅させる 2019.02.24
			else if (zakoObj[i].isInvincible)
			{
				if (zakoBlinkFrame % 6 < 3)
				{
					zakoObj[i].SetSpriteEnabled(false);
				}
				else
				{
					zakoObj[i].SetSpriteEnabled(true);
				}
			}
			else
			{
				zakoObj[i].SetSpriteEnabled(true);
			}
		}
		++zakoBlinkFrame;
		if (zakoBlinkFrame >= 65534) { zakoBlinkFrame = 0; }
		// 怒りエフェクトを操作する 2019.02.16
		for (int i = 0; i < angryEffect2List.Count; ++i)
		{
			// エフェクトを移動させる
			angryEffect2List[i].transform.localPosition += angryEffect2Velocity[i];
			// スピードを減らす	2019.02.19
			angryEffect2Velocity[i] /= 1.1f;
			float velocityValue = angryEffect2Velocity[i].sqrMagnitude;
			float baseValue = new Vector3(0.15f, 0.06f, 0f).sqrMagnitude;
			// 元のスピードの5%未満になったら消す
			if (velocityValue / baseValue < 0.05f)
			{
				Destroy(angryEffect2List[i]);
				angryEffect2List.RemoveAt(i);
				angryEffect2Velocity.RemoveAt(i);
				continue;
			}
			// エフェクトの色をなくしていく
			angryEffect2List[i].color = new Color(angryEffect2List[i].color.r, angryEffect2List[i].color.g,
				angryEffect2List[i].color.b, velocityValue / baseValue);
		}
		// 確率で瞬きする 2018.11.06	連続で瞬きさせない 2018.11.17	2019.01.04
		if (winkFrame > 60 && Random.Range(0, 50) == 0) { winkFrame = 0; }
		// 瞬きの枚数追加 2019.01.09
		if (winkFrame < 3 || (winkFrame >= 6 && winkFrame < 9))
		{
			anim.spriteRenderer.sprite = bodySprite[1];
		}
		else if (winkFrame >= 3  && winkFrame < 6)
		{
			anim.spriteRenderer.sprite = bodySprite[2];
		}
		else
		{
			anim.spriteRenderer.sprite = bodySprite[0];
		}
		++winkFrame;
		// 怒り状態の時の処理 2019.02.16
		if (damageFrame <= bandaidFrameMax && angryFrame > 0)
		{
			// スプライトを怒りの絵に変える
			if (angryFrame > angryFrameMax - 10)
			{
				anim.spriteRenderer.sprite = angrySprite[0];
			}
			else
			{
				anim.spriteRenderer.sprite = angrySprite[1];
			}
			// 怒りのエフェクトを生成する
			if (angryFrame % 30 == 0)
			{
				angryEffect2List.Add(Instantiate(angryEffect2Prefab, new Vector3(transform.position.x + 1f,
					transform.position.y - 0.7f, transform.position.z - 0.3f),
					angryEffect2Prefab.transform.localRotation, transform));
				angryEffect2Velocity.Add(new Vector3(0.15f, 0.06f, 0f));
			}
			else if (angryFrame % 34 == 0)
			{
				SpriteRenderer temp = Instantiate(angryEffect2Prefab, new Vector3(transform.position.x - 1f,
					transform.position.y - 0.7f, transform.position.z - 0.3f),
					angryEffect2Prefab.transform.localRotation, transform);
				temp.flipX = true;
				angryEffect2List.Add(temp);
				angryEffect2Velocity.Add(new Vector3(-0.2f, 0.08f, 0f));
			}
			// 怒りマークを表示
			for (int i = 0; nowAction != BossConduct.TELEPORT && i < angryEffect1List.Count; ++i)
			{
				angryEffect1List[i].enabled = true;
			}
			--angryFrame;
		}
		// 怒りマークを消す
		else
		{
			for (int i = 0; i < angryEffect1List.Count; ++i)
			{
				angryEffect1List[i].enabled = false;
			}
		}
		// ダメージを受けているときのアクション 瞬きと処理の順番を入れ替え・追加 2019.01.08
		if (damageFrame > bandaidFrameMax)
		{
			DamageAction();
			// ボスの絵を泣いている絵にする 2019.01.08	ばんそうこう表示時間を条件に追加 修正 2019.01.13
			if (damageFrame < (damageFrameMax / 3 * 2) + bandaidFrameMax && damageFrame > (damageFrameMax / 3) + bandaidFrameMax)
			{
				anim.spriteRenderer.sprite = bodySprite[4];
			}
			else if (damageFrame < damageFrameMax + bandaidFrameMax || damageFrame < (damageFrameMax / 3) + bandaidFrameMax)
			{
				anim.spriteRenderer.sprite = bodySprite[3];
			}
		}
		// ばんそうこうを一定時間表示 2019.01.09 条件を付けて泣いている間にも表示するように 2019.01.15
		for (int i = 0; i < bandaidSRList.Count; ++i)
		{
			bandaidSRList[i].enabled = damageFrame > 0;
		}
		// 無敵時間は点滅する 2019.01.19
		if (damageFrame <= bandaidFrameMax && invincibleFrame > 0)
		{
			anim.spriteRenderer.enabled = invincibleFrame % 4 < 2;
		}
		invincibleFrame -= invincibleFrame - 1 < 0 ? 0 : 1;
		damageFrame -= damageFrame - 1 < 0 ? 0 : 1;
		// 状態によってアクションを変える
		if(nowAction == BossConduct.MOVE)
		{
			MoveNormal();
			// 体力が半分を切っていたら確率で怒る
			if(hp < maxHp / 2 && angryFrame <= 0 && Random.Range(0, 128) == 0)
			{
				angryFrame = angryFrameMax;
			}
			// 体力が少なくなったら確率でテレポートする 2019.01.19
			if (hp <= maxHp * 0.5 && Random.Range(0, 96) == 0)
			{
				// スピードをそれぞれ0にする
				speed = Vector3.zero;
				hands.right.InitSpeed();
				hands.left.InitSpeed();
				// 元の位置を保存しておく
				returnHandsPos_local[0] = hands.right.transform.localPosition;
				returnHandsPos_local[1] = hands.left.transform.localPosition;
				// 前の行動を保存しておく 2019.02.03
				previousAction = nowAction;
				nowAction = BossConduct.TELEPORT;
				nextTereportPos = new Vector3(Random.Range(-4.5f - 33f, 4.5f - 33f),
					Random.Range(-0.39f + 4f, 3.61f + 4f), Random.Range(-12.5f, -7f));
			}
			// 1fに1/256の確立、または設定した時間が過ぎたら行動が変わる	2019.01.04
			if (Random.Range(0, 256) == 0 || actionFrame++ > actionFrameMax)
			{
				RandomSwitchAction();
				actionFrame = 0;
			}
			// 怒り状態であればさらにワンツーをしやすくなる 2019.02.16
			if (angryFrame > 0 && Random.Range(0, 128) == 0)
			{
				// スピードをそれぞれ0にする
				speed = Vector3.zero;
				hands.right.InitSpeed();
				hands.left.InitSpeed();
				// 元の位置を保存しておく
				returnHandsPos_local[0] = hands.right.transform.localPosition;
				returnHandsPos_local[1] = hands.left.transform.localPosition;
				// 前の行動を保存しておく 2019.02.03
				previousAction = nowAction;
				nowAction = BossConduct.ONETWO;
				hands.right.hp = 3;
				hands.left.hp = 3;
			}
		}
		else if (nowAction == BossConduct.PUNCH)
		{
			PunchAction(playerControl, actionHands);    // 2018.10.26
		}
		else if (nowAction == BossConduct.PUSH)
		{
			// 2018.10.26
			if (actionHands == ActionHands.BOTH)
			{
				if (PushAction(ActionHands.LEFT) && PushAction(ActionHands.RIGHT))
				{
					MoveStart();
				}
			}
			else
			{
				// 2018.10.26
				if (PushAction(actionHands))
				{
					MoveStart();
				}
			}
		}
		else if (nowAction == BossConduct.GUARD)
		{
			GuardAction();
		}
		else if (nowAction == BossConduct.DELIVER)
		{
			DeliverZako();
		}
		else if (nowAction == BossConduct.RECOVER)
		{
			RecoverPlayer(playerControl);
		}
		// 2018.11.01
		else if (nowAction == BossConduct.ONETWO)
		{
			OneTwoPunchAction(playerControl);
		}
		else if (nowAction == BossConduct.INHOLE)
		{
			InholeAction(actionHands);
		}
		else if (nowAction == BossConduct.TELEPORT)
		{
			TereportAction();
		}
		else if (nowAction == BossConduct.COUNTERBLOW)
		{
			CounterblowAction();
		}
		// 適用	2019.01.04
		rb.velocity = speed;
	}

	// 行動を変える----------------------------------------------------------------------
	void RandomSwitchAction()
	{
		// スピードをそれぞれ0にする
		speed = Vector3.zero;
		hands.right.InitSpeed();
		hands.left.InitSpeed();
		// 元の位置を保存しておく
		returnHandsPos_local[0] = hands.right.transform.localPosition;
		returnHandsPos_local[1] = hands.left.transform.localPosition;
		// 前の行動を保存しておく 2019.02.03
		previousAction = nowAction;
		// 手の弾が当たった判定をなくしておく 2019.02.24
		hands.right.isHitBullet = false;
		hands.left.isHitBullet = false;
		int ran = Random.Range(0, 100);
		int judgeGuard = 20;
		int judgePush = 50;
		int judgeOneTwo = 3;		// 2018.11.01
		int judgeDeliver = 4;		// 値を変更 2018.11.01	2019.01.19
		// 体力が半分を下回っていたら行動する手をランダムにして数値を再設定 2018.10.26
		if (hp <= maxHp * 0.5)
		{
			ran = Random.Range(0, 4);   // 値を変更 2018.11.01
			judgeGuard = 1;
			judgePush = 2;
		}
		// 体力が30%以下なら行動を増やす 2018.10.27
		if (hp <= maxHp * 0.3)
		{
			ran = Random.Range(0, 6);   // 値を変更 2018.11.01
			// 2018.11.01
		}
		// 行動選択
		if (ran < judgeGuard)
		{
			nowAction = BossConduct.GUARD;
		}
		else if (ran < judgePush)
		{
			// 2018.11.01
			nowAction = BossConduct.INHOLE;
			isGetGhost = false;
			// 2018.10.25
			hands.right.isInhole = true;
			hands.left.isInhole = true;
			actionHands = ActionHands.LEFT;
		}
		else if (ran < judgeOneTwo)	// 2018.11.01
		{
			nowAction = BossConduct.ONETWO;
			hands.right.hp = 3;
			hands.left.hp = 3;
		}
		else if (hp <= maxHp * 0.3 && ran < judgeDeliver)   // 2018.10.27
		{
			// ザコが最大数だったらテレポートする 2019.01.27
			if (zakoObj.Count >= 21)
			{
				nowAction = BossConduct.TELEPORT;
				nextTereportPos = new Vector3(Random.Range(-4.5f - 33f, 4.5f - 33f),
					Random.Range(-0.39f + 4f, 3.61f + 4f), Random.Range(-12.5f, -7f));
			}
			// 召喚
			else
			{
				nowAction = BossConduct.DELIVER;
			}
		}
		// 条件追加 2018.12.28
		else
		{
			nowAction = BossConduct.PUNCH;
			actionHands = ActionHands.RIGHT;
			hands.right.hp = 3;
			hands.left.hp = 3;
		}
		if (hp <= maxHp / 2)
		{
			// 2018.11.01
			if (Random.Range(0, 2) == 0)
			{
				actionHands = ActionHands.LEFT;
			}
			else
			{
				actionHands = ActionHands.RIGHT;
			}
		}
		inholeGhistCnt = 0;
		if (actionHands == ActionHands.RIGHT)
		{
			inholeGhostCntMax = hands.right.targetGhostList.Count;
			hands.left.isInhole = false;
		}
		else if (actionHands == ActionHands.LEFT)
		{
			inholeGhostCntMax = hands.left.targetGhostList.Count;
			hands.right.isInhole = false;
		}
	}

	// 動き始める------------------------------------------------------------------------------
	void MoveStart()
	{
		previousAction = nowAction;
		nowAction = BossConduct.MOVE;
		speed = Vector3.one * multipleSpeed;
		// 進む方向を決める
		speed.x *= Mathf.Sign(Random.insideUnitSphere.x);
		speed.z *= Mathf.Sign(Random.insideUnitSphere.z);
		speed.y *= Mathf.Sign(Random.insideUnitSphere.y);
		// 手の弾が当たった判定をなくす 2019.02.24
		hands.right.isHitBullet = false;
		hands.left.isHitBullet = false;
	}

	// 二つの位置がどれくらい近いか比べる-------------------------------------------------------
	bool IsNear(Vector3 a, Vector3 b, float value)
	{
		return Mathf.Abs(a.x - b.x) < value && Mathf.Abs(a.y - b.y) < value && Mathf.Abs(a.z - b.z) < value;
	}

	// 元の位置に戻る publicに 2018.11.02------------------------------------------------------
	public void MoveReturn()
	{
		// 左右それぞれ数値いじり(第三引数:5.0f -> 10f) 2018.12.27	2019.01.04
		hands.right.transform.localPosition = Vector3.MoveTowards(
			hands.right.transform.localPosition,
			returnHandsPos_local[0],
			15f * Time.deltaTime);
		hands.left.transform.localPosition = Vector3.MoveTowards(
			hands.left.transform.localPosition,
			returnHandsPos_local[1],
			15f * Time.deltaTime);
	}

	// 待つ 2018.10.26-----------------------------------------------------------------------
	void Wait(BossHand waitHand, Vector3 waitPos)
	{
		// 数値いじり(第三引数:1.0f -> 5f) 2018.12.27
		waitHand.transform.localPosition = Vector3.Lerp(waitHand.transform.localPosition,
				waitPos,
				5f * Time.deltaTime);
	}

	// 普通の動き-----------------------------------------------------------------------------
	void MoveNormal()
	{
		// 手を動かす
		Vector3 handsSpd = Vector3.zero;
		handsSpd.x = handsRadius * (Mathf.Cos(handsSpeedRadian * Mathf.Deg2Rad));	//2019.01.04
		handsSpd.z = handsRadius * (Mathf.Sin(handsSpeedRadian * Mathf.Deg2Rad));	// 2019.01.04
		handsSpd += speed;
		hands.right.speed = hands.left.speed = handsSpd;
		handsSpeedRadian += 3.0f;
		// 本体の移動制限
		// X
		if (transform.localPosition.x >= 4.5f - 33.0f)
		{
			float temp = Mathf.Abs(speed.x);
			speed.x = -temp;
		}
		else if (transform.localPosition.x <= -4.5f - 33.0f)
		{
			float temp = Mathf.Abs(speed.x);
			speed.x = temp;
		}
		// Y 数値いじり 2018.12.25
		if (transform.localPosition.y >= 3.61f + 4f)
		{
			float temp = Mathf.Abs(speed.y);
			speed.y = -temp;
		}
		else if (transform.localPosition.y <= -0.39f + 4f)
		{
			float temp = Mathf.Abs(speed.y);
			speed.y = temp;
		}
		// Z 数値いじり 2018.12.25
		if (transform.localPosition.z >= -7)
		{
			float temp = Mathf.Abs(speed.z);
			speed.z = -temp;
		}
		else if (transform.localPosition.z <= -12.5f)
		{
			float temp = Mathf.Abs(speed.z);
			speed.z = temp;
		}
	}

	// パンチをするときの一連の行動--------------------------------------------------------
	void PunchAction(PlayerControl playerControl, ActionHands actionHands)
	{
		BossHand punchHand = hands.left;
		BossHand freeHand = hands.right;
		Vector3 punchReturnPos = returnHandsPos_local[1];
		Vector3 freeReturnPos = returnHandsPos_local[0];
		Vector3 punchWaitPos = new Vector3(2.0f, 0.5f, 0.0f);
		Vector3 freeWaitPos = new Vector3(-3.0f, -0.5f, 0.0f);
		Vector3 freePunchingWaitPos = new Vector3(-4.0f, 0.0f, 0.0f);
		if (actionHands == ActionHands.RIGHT)
		{
			punchHand = hands.right;
			freeHand = hands.left;
			punchReturnPos = returnHandsPos_local[0];
			freeReturnPos = returnHandsPos_local[1];
			punchWaitPos.x *= -1;
			freeWaitPos.x *= -1;
			freePunchingWaitPos.x *= -1;
		}
		// 攻撃が終わったらもとの位置に戻す
		if (endAttack)
		{
			// 修正 2018.11.01
			MoveReturn();
			if (IsNear(punchHand.transform.localPosition, punchReturnPos, 0.01f) &&
				IsNear(freeHand.transform.localPosition, freeReturnPos, 0.01f))
			{
				// 元の位置に戻す 2019.01.19
				hands.right.transform.localPosition = returnHandsPos_local[0];
				hands.left.transform.localPosition = returnHandsPos_local[1];
				MoveStart();
				actionFrame = 0;
				// 2018.11.16
				punchHand.anim.spriteRenderer.sprite = normalHandSprite;
				endAttack = false;
			}
			// 近づいてから手を広げる 2018.11.16
			else if (IsNear(punchHand.transform.localPosition, punchReturnPos, 0.5f) &&
				IsNear(freeHand.transform.localPosition, freeReturnPos, 0.5f))
			{
				punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[0];
			}
		}
		// 待っているときは所定の位置に 2019.01.04	条件追加 2019.02.11
		else if (actionFrame++ < attackWaitFrame || punchHand.hp <= 0)
		{
			// 音を鳴らす 2019.02.27
			if (actionFrame == 1)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossPunch);
			}
			// スパークルを生成する 2019.01.03
			if (actionFrame < attackWaitFrame / 5 * 4 && actionFrame % 20 == 0)
			{
				punchHand.InsRedSparckle();
			}
			// 色を点滅させる 2018.10.27 スプライトを変える 2018.11.01	2019.01.04
			if (actionFrame % 10 < 5)
			{
				punchHand.anim.spriteRenderer.sprite = punchHandSprite;
				if (actionFrame > attackWaitFrame / 10 && punchHand.hp > 0)
				{
					// ばんそうこうを少しずらす 2019.03.30
					punchHand.OffsetBandaidLocalPos(Vector3.down);
				}
			}
			else
			{
				punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[1];
				if (actionFrame > attackWaitFrame / 10 && punchHand.hp > 0)
				{
					// ばんそうこうを少しずらす 2019.03.30
					punchHand.OffsetBandaidLocalPos(Vector3.up);
				}
			}
			// 少し握る絵を入れる 2018.11.06
			if (actionFrame < attackWaitFrame / 10)
			{
				punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[0];
			}
			// 待つ
			if (actionHands == ActionHands.BOTH)
			{
				Wait(hands.right, new Vector3(-0.5f, 3.5f, 0.0f));
				Wait(hands.left, new Vector3(0.5f, 3.5f, 0.0f));
				// 震える 2018.11.02
				ShakeObj(punchHand.gameObject);
				ShakeObj(freeHand.gameObject);
			}
			else
			{
				// 2018.10.26
				Wait(punchHand, punchWaitPos);
				Wait(freeHand, freeWaitPos);
				// 震える 2018.11.02
				ShakeObj(punchHand.gameObject);
			}
			// ばんそうこうの表示 2019.01.15
			if (punchHand.hp <= 0)
			{
				punchHand.bandaidFrame = 120;
			}
			// 弾が当てられたら手を少しノックバック 2019.01.13	2019.02.19
			if (punchHand.isHitBullet)
			{
				// 修正 2019.02.21
				if (punchHand.hp > 0)
				{
					nockbackSpeed = 0.2f;
					handNockbackFrame = 17;
					--punchHand.hp;		// 調整 2019.02.16
					// 手の体力が0以下になっていたらノックバックを長くする 2019.02.15
					if (punchHand.hp <= 0)
					{
						handNockbackFrame *= 3;
						nockbackSpeed *= 3f;
						if (previousAction != BossConduct.TELEPORT)
						{
							// 怒っていなければダメージ 2019.04.06	変数ミス 2019.04.13
							if (angryFrame <= 0)
							{
								--hp;
								// 2018.11.13 UI処理
								uiManager.Damages(false);
							}
							// 音を鳴らす 2019.01.08
							EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossDamage);
							// 体力が半分の時に怒る 2019.02.16	2019.04.03	2019.04.06
							if(hp == maxHp / 2 || hp == maxHp - 4)
							{
								angryFrame = angryFrameMax;
							}
							if (hp == 0)
							{
								actionFrame = 0;
							}
						}
					}
				}
				punchHand.isHitBullet = false;
			}
			// 手のノックバック 2019.01.13
			if (handNockbackFrame > 0)
			{
				DamageHandAction(punchHand);
				--handNockbackFrame;
				if (punchHand.hp <= 0)
				{
					// 絵を戻す 2018.11.01
					punchHand.anim.spriteRenderer.sprite = normalHandSprite;
					// 本体の絵を変える 2019.02.26
					anim.spriteRenderer.sprite = cancelAttackSprite;
				}
			}
			// ノックバックをしていないかつ手の体力が0以下になった時はやめる 2019.02.11
			else if (punchHand.hp <= 0)
			{
				// 絵を戻す 2018.11.01
				punchHand.anim.spriteRenderer.sprite = normalHandSprite;
				endAttack = true;
				freeHand.hp = 0;
				// 条件追加 2019.04.06
				if (previousAction != BossConduct.TELEPORT || damageFrame <= 0)
				{
					damageFrame = damageFrameMax + bandaidFrameMax / 3;	// ばんそうこう表示時間を追加 2019.01.09
					invincibleFrame = damageFrameMax;					// 無敵時間適用 2019.01.19
				}
			}
		}
		else
		{
			// 音を鳴らす 2019.02.21
			if (actionFrame == attackWaitFrame + 1)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossPunch);
			}
			// プレイヤーに向かってパンチ 2018.10.26	2019.01.04
			if (actionHands == ActionHands.BOTH)
			{
				punchHand.transform.position = Vector3.MoveTowards(punchHand.transform.position,
					playerCamera.transform.position,
					10.0f * Time.deltaTime);
				freeHand.transform.position = Vector3.MoveTowards(freeHand.transform.position,
					playerCamera.transform.position,
					10.0f * Time.deltaTime);
			}
			else
			{
				// 数値いじり(第三引数:10.0f -> 15f) 2018.12.27	2019.01.04
				punchHand.transform.position = Vector3.MoveTowards(punchHand.transform.position,
					playerCamera.transform.position,
					15f * Time.deltaTime);
				freeHand.transform.localPosition = Vector3.Lerp(freeHand.transform.localPosition,
					freePunchingWaitPos,
					1.0f * Time.deltaTime);
			}
			// 2018.11.01
			punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[1];
			if (IsNear(punchHand.transform.position, playerCamera.transform.position, 0.01f))
			{
				playerControl.hp--;
				playerControl.damageCameraNum = 1;      // プレイヤーのほうを変えるべき
				playerControl.isDamage = true;
				endAttack = true;
			}
		}
	}

	// ワンツー一連の行動 2018.11.01-----------------------------------------------------------
	void OneTwoPunchAction(PlayerControl playerControl)
	{
		// 情報の設定
		BossHand punchHand = hands.left;
		BossHand freeHand = hands.right;
		Vector3 punchWaitPos = new Vector3(2.0f, 0.5f, 0.0f);
		Vector3 freeWaitPos = new Vector3(-3.0f, -0.5f, 0.0f);
		Vector3 freePunchingWaitPos = new Vector3(-4.0f, 0.0f, 0.0f);
		if (endOneTwo[0])
		{
			punchHand = hands.right;
			freeHand = hands.left;
			punchWaitPos.x *= -1;
			freeWaitPos.x *= -1;
			freePunchingWaitPos.x *= -1;
		}
		// どちらの攻撃も終わっていたらもとの位置に戻って動き始める
		if (endOneTwo[0] && endOneTwo[1])
		{
			MoveReturn();
			if (IsNear(hands.right.transform.localPosition, returnHandsPos_local[0], 0.01f) &&
				IsNear(hands.left.transform.localPosition, returnHandsPos_local[1], 0.01f))
			{
				// 元の位置に戻す 2019.01.19
				hands.right.transform.localPosition = returnHandsPos_local[0];
				hands.left.transform.localPosition = returnHandsPos_local[1];
				MoveStart();
				actionFrame = 0;
				// 2018.11.16
				punchHand.anim.spriteRenderer.sprite = normalHandSprite;
				for (int i = 0; i < 2; ++i)
				{
					endOneTwo[i] = false;
				}
			}
			// 2018.11.16
			else if (IsNear(hands.right.transform.localPosition, returnHandsPos_local[0], 0.5f) &&
				IsNear(hands.left.transform.localPosition, returnHandsPos_local[1], 0.5f))
			{
				punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[0];
			}
		}
		// 攻撃するまで待つ 2019.01.04	条件追加 2019.02.11
		else if (actionFrame++ < attackWaitFrame || punchHand.hp <= 0)
		{
			// 音を鳴らす 2019.01.08
			if (actionFrame == 1)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossPunch);
			}
			// スパークルを生成する 2019.01.03
			if (actionFrame < attackWaitFrame / 5 * 4 && actionFrame % 20 == 0)
			{
				punchHand.InsRedSparckle();
			}
			// スプライトを変える	2019.01.04
			if (actionFrame % 10 < 5)
			{
				punchHand.anim.spriteRenderer.sprite = punchHandSprite;
				if (actionFrame > attackWaitFrame / 10 && punchHand.hp > 0)
				{
					// ばんそうこうを少しずらす 2019.03.30
					punchHand.OffsetBandaidLocalPos(Vector3.down);
				}
			}
			else
			{
				punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[1];
				if (actionFrame > attackWaitFrame / 10 && punchHand.hp > 0)
				{
					// ばんそうこうを少しずらす 2019.03.30
					punchHand.OffsetBandaidLocalPos(Vector3.up);
				}
			}
			// 少し握る絵を入れる 2018.11.06
			if (actionFrame < attackWaitFrame / 10)
			{
				punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[0];
			}
			// 待つ
			Wait(punchHand, punchWaitPos);
			Wait(freeHand, freeWaitPos);
			// 震える 2018.11.02
			ShakeObj(punchHand.gameObject);
			// ばんそうこうの表示 2019.01.15
			if (punchHand.hp <= 0)
			{
				// ばんそうこうの表示 2019.01.15
				punchHand.bandaidFrame = 120;
			}
			// 弾が当てられたら手を少しノックバック 2019.01.15	2019.02.19
			if (punchHand.isHitBullet)
			{
				// 修正 2019.02.21
				if (punchHand.hp > 0)
				{
					nockbackSpeed = 0.2f;
					handNockbackFrame = 17;
					--punchHand.hp;		// 調整 2019.02.16
					// 手の体力が0以下になっていたらノックバックを長くする 2019.02.15
					if (punchHand.hp <= 0)
					{
						handNockbackFrame *= 3;
						nockbackSpeed *= 3f;
						// 怒っていなければダメージを受ける 2019.04.06
						if (angryFrame <= 0)
						{
							--hp;
							// 2018.11.13 UI処理
							uiManager.Damages(false);
						}
						// 音を鳴らす 2019.01.08
						EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossDamage);
						// 体力が半分の時に怒る 2019.02.16	2019.04.03	2019.04.06
						if(hp == maxHp / 2 || hp == maxHp - 4)
						{
							angryFrame = angryFrameMax;
						}
						if (hp == 0)
						{
							actionFrame = 0;
						}
					}
				}
				punchHand.isHitBullet = false;
			}
			// 手のノックバック 2019.01.15
			if (handNockbackFrame > 0)
			{
				DamageHandAction(punchHand);
				--handNockbackFrame;
				if (punchHand.hp <= 0)
				{
					// 絵を戻す 2018.11.01
					punchHand.anim.spriteRenderer.sprite = normalHandSprite;
					// 本体の絵を変える 2019.02.26
					anim.spriteRenderer.sprite = cancelAttackSprite;
				}
			}
			// ノックバックをしていないかつ手の体力が0の時
			else if (punchHand.hp <= 0)
			{
				punchHand.anim.spriteRenderer.sprite = normalHandSprite;
				actionFrame = 0;
				if (endOneTwo[0])
				{
					endOneTwo[1] = true;
				}
				else
				{
					endOneTwo[0] = true;
				}
				// 怒っていなければそれぞれ時間を適用 2019.04.06
				if (angryFrame <= 0)
				{
					damageFrame = damageFrameMax + bandaidFrameMax / 3;	// ばんそうこう表示時間を追加 2019.01.09
					invincibleFrame = damageFrameMax;					// 無敵時間適用 2019.01.19
				}
			}
			// 待っているときの手の絵を変える 2018.11.16 数値を変更 2018.11.27
			if (endOneTwo[0] && IsNear(freeHand.transform.localPosition, freeWaitPos, 0.8f))
			{
				freeHand.anim.spriteRenderer.sprite = normalHandSprite;
			}
			else if (endOneTwo[0] && IsNear(freeHand.transform.localPosition, freeWaitPos, 1.0f))
			{
				freeHand.anim.spriteRenderer.sprite = knuckleHandSprite[0];
			}
		}
		else
		{
			// 音を鳴らす 2019.02.21
			if (actionFrame == attackWaitFrame + 1)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossPunch);
			}
			// パンチする 数値いじり(第三引数:10.0f -> 15f) 2018.12.27	2019.01.04
			punchHand.transform.position = Vector3.MoveTowards(punchHand.transform.position,
				playerCamera.transform.position,
				15f * Time.deltaTime);
			freeHand.transform.localPosition = Vector3.Lerp(freeHand.transform.localPosition,
				freePunchingWaitPos,
				1.0f * Time.deltaTime);
			punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[1];
			// プレイヤーに当たったら解除
			if (IsNear(punchHand.transform.position, playerCamera.transform.position, 0.01f))
			{
				playerControl.hp--;
				playerControl.damageCameraNum = 1;
				playerControl.isDamage = true;
				if (endOneTwo[0])
				{
					endOneTwo[1] = true;
				}
				else
				{
					endOneTwo[0] = true;
				}
				actionFrame = 0;
			}
		}
	}

	// おばけを捕まえに行くときの一連の行動 2018.10.26 戻り値つけた(元の位置に戻ったか)-----------
	bool PushAction(ActionHands actionHands)
	{
		bool isEnd = false;
		BossHand pushHand = hands.right;
		Vector3 returnPos = returnHandsPos_local[0];
		if (actionHands == ActionHands.LEFT)
		{
			pushHand = hands.left;
			returnPos = returnHandsPos_local[1];
		}
		// おばけを捕まえていたらもとに位置に戻す
		if (isGetGhost)
		{
			MoveReturn();
			pushHand.targetGhost = null;	// 2018.10.24
			// 元の位置に戻ったら動き出す
			if (IsNear(pushHand.transform.localPosition, returnPos, 0.01f))
			{
				// 元の位置に戻す 2019.01.19
				hands.right.transform.localPosition = returnHandsPos_local[0];
				hands.left.transform.localPosition = returnHandsPos_local[1];
				isEnd = true;
			}
		}
		// ターゲットになるおばけがいなかったら元の位置に戻して動き始める
		else if (!pushHand.targetGhost)
		{
			if (IsNear(pushHand.transform.localPosition, returnPos, 0.01f))
			{
				// 2018.10.25
				hands.right.isPush = false;
				hands.left.isPush = false;
				isEnd = true;
			}
			else
			{
				MoveReturn();
			}
			return false;   // ?(忘れた)
		}
		else
		{
			// ターゲットのおばけに向かう MoveTowards->Lerp 2018.10.25
			pushHand.transform.position = Vector3.Lerp(pushHand.transform.position,
				pushHand.targetGhost.transform.position, 7.5f * Time.deltaTime);
			// おばけに近づいたら戻り始める
			if (IsNear(pushHand.transform.position, pushHand.targetGhost.transform.position, 1.0f))
			{
				isGetGhost = true;
				Destroy(pushHand.targetGhost);
				pushHand.targetGhost = null;
				// 2018.10.25
				hands.right.isPush = false;
				hands.left.isPush = false;
			}
		}
		return isEnd;
	}

	// 吸い込み一連の行動 2018.11.01----------------------------------------------------------
	void InholeAction(ActionHands actionHands)
	{
		// 情報設定
		BossHand inholeHand = hands.left;
		Vector3 returnPos = returnHandsPos_local[1];
		Vector3 waitPos = new Vector3(1.15f, -1f, -1.65f);
		// ブラックホールの設定 2018.12.04
		blackholeObject.transform.position = hands.left.transform.position;
		if (actionHands == ActionHands.RIGHT)
		{
			inholeHand = hands.right;
			returnPos = returnHandsPos_local[0];
			blackholeObject.transform.position = hands.right.transform.position;    // 2018.12.04
			waitPos.x *= -1;
		}
		blackholeObject.transform.position -= new Vector3(0.0f, 0.5f, 0.0f);		// 2018.12.04
		// ブラックホールを回しておく 2018.12.04
		blackholeObject.transform.eulerAngles -= new Vector3(0.0f, 0.0f, 4.0f);
		if (blackholeObject.transform.eulerAngles.z < -360)
		{
			blackholeObject.transform.eulerAngles += new Vector3(0.0f, 0.0f, 360.0f);
		}
		// 全て吸い取ったら元の位置に戻り動き始める
		if (inholeHand.targetGhostList.Count <= 0 || inholeGhistCnt == inholeGhostCntMax)
		{
			MoveReturn();
			// ブラックホールを縮小する 2018.12.04
			if (blackholeObject.transform.localScale.x > 0.0f)
			{
				blackholeObject.transform.localScale = Vector3.MoveTowards(blackholeObject.transform.localScale,
					Vector3.zero, 3.0f * Time.deltaTime);
				// 少し変更 2018.12.04
				if (blackholeObject.transform.localScale.x < 0.001f)
				{
					blackholeObject.transform.localScale = Vector3.zero;
				}
			}
			// ブラックホールが見えなくなり、もとの位置に戻ったら動く 2018.12.04
			if (IsNear(inholeHand.transform.localPosition, returnPos, 0.01f) && blackholeObject.transform.localScale.Equals(Vector3.zero))
			{
				// 元の位置に戻す 2019.01.19
				hands.right.transform.localPosition = returnHandsPos_local[0];
				hands.left.transform.localPosition = returnHandsPos_local[1];
				hands.right.isInhole = false;
				hands.left.isInhole = false;
				actionFrame = 0;
				MoveStart();
			}
		}
		// おばけを吸い込む
		else
		{
			Wait(inholeHand, waitPos);
			// ブラックホールを広げる 2018.12.04
			if (blackholeObject.transform.localScale.x <= 1.0f)
			{
				blackholeObject.transform.localScale = Vector3.Lerp(blackholeObject.transform.localScale,
					Vector3.one * 1.005f, 5.0f * Time.deltaTime);
			}
			// 音を鳴らす 2019.03.31
			if(actionFrame == 10)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossInhale);
			}
			// 一泊置いてから吸い込み始める 2019.01.19
			if (++actionFrame < 60)
			{
				foreach (GameObject o in inholeHand.targetGhostList)
				{
					if (o == null) { continue; }
					// スプライトを変える 2018.11.27
					if (!inholeHand.endChangeGhostSprite[inholeHand.targetGhostList.IndexOf(o)])
					{
						o.GetComponent<GhostAction>().ChangeSpriteDeleteInhole();
						inholeHand.endChangeGhostSprite[inholeHand.targetGhostList.IndexOf(o)] = true;
					}
					// 震える
					ShakeObj(o);
				}
			}
			// 吸い込み始めて、近づいたら消す
			else
			{
				GameObject removeObj = null;
				foreach (GameObject o in inholeHand.targetGhostList)
				{
					if (o == null) { continue; }
					o.transform.position = Vector3.MoveTowards(o.transform.position, inholeHand.transform.position, 10f * Time.deltaTime);
					if (IsNear(o.transform.position, inholeHand.transform.position, 0.05f))
					{
						removeObj = o;
					}
				}
				if (removeObj)
				{
					inholeHand.endChangeGhostSprite.RemoveAt(inholeHand.targetGhostList.IndexOf(removeObj));
					inholeHand.targetGhostList.Remove(removeObj);
					Destroy(removeObj);
					++inholeGhistCnt;
				}
			}
		}
	}

	// 守る時の一連の行動------------------------------------------------------------------
	void GuardAction()
	{
		// 絵を変える 2018.11.01
		hands.right.anim.spriteRenderer.sprite = guardHandSprite;
		hands.left.anim.spriteRenderer.sprite = guardHandSprite;
		// ガードしておく	2019.01.04
		if (actionFrame++ < guardFrameMax)
		{
			// 音を鳴らす 2019.02.27
			if (actionFrame == 10)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossGuard);
			}
			// 手を守るための位置に持ってくる 左右それぞれ数値いじり(第三引数:2.5f -> 4f) 2018.12.27	2019.01.04
			hands.right.transform.localPosition = Vector3.Lerp(
				hands.right.transform.localPosition,
				guardHandsPos_local[0],
				4f * Time.deltaTime);
			hands.left.transform.localPosition = Vector3.Lerp(
				hands.left.transform.localPosition,
				guardHandsPos_local[1],
				4f * Time.deltaTime);
			// 攻撃を受けたらカウンターに移行 2019.01.27
			if (hands.left.isHitBullet || hands.right.isHitBullet)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossGuardHit);		// SE追加 2019.03.26
				nowAction = BossConduct.COUNTERBLOW;
				hands.left.isHitBullet = hands.right.isHitBullet = false;
				nextTereportPos = new Vector3(Random.Range(-4.5f - 33f, 4.5f - 33f),
					Random.Range(-0.39f + 4f, 3.61f + 4f), Random.Range(-12.5f, -7f));
				actionFrame = 0;
			}
		}
		// 手をもとの位置に戻す
		else
		{
			MoveReturn();
		}
		// 近くまで来ていたら動き出す
		if (IsNear(hands.right.transform.localPosition, returnHandsPos_local[0], 0.01f) &&
			IsNear(hands.left.transform.localPosition, returnHandsPos_local[1], 0.01f))
		{
			// 元の位置に戻す 2019.01.19
			hands.right.transform.localPosition = returnHandsPos_local[0];
			hands.left.transform.localPosition = returnHandsPos_local[1];
			MoveStart();
			actionFrame = 0;
			// 2018.11.01
			hands.right.anim.spriteRenderer.sprite = normalHandSprite;
			hands.left.anim.spriteRenderer.sprite = normalHandSprite;
		}
	}

	// ザコを繰り出す 2018.10.27-------------------------------------------------------------
	void DeliverZako()
	{
		// 指さしが終わったら元の位置に戻る
		if (endPointing)
		{
			// 2018.11.01
			hands.right.anim.spriteRenderer.sprite = normalHandSprite;
			MoveReturn();
			// 近くまで来ていたら動き出す
			if (IsNear(hands.right.transform.localPosition, returnHandsPos_local[0], 0.01f) &&
				IsNear(hands.left.transform.localPosition, returnHandsPos_local[1], 0.01f))
			{
				// 元の座標に戻す 2019.01.19
				hands.right.transform.localPosition = returnHandsPos_local[0];
				hands.left.transform.localPosition = returnHandsPos_local[1];
				endPointing = false;
				MoveStart();
				actionFrame = 0;
			}
		}
		// 指をさしておばけを繰り出す 数値いじり 60 -> 45 2018.12.27	2019.01.04
		else if (actionFrame++ > 45)
		{
			// 2018.11.01
			hands.right.anim.spriteRenderer.sprite = deliverHandSprite;
			Vector3 rightPos = new Vector3(-2.0f, -0.2f, -1.4f);
			Vector3 leftPos = new Vector3(4.0f, 0.0f, 0.0f);
			// 移動させる 数値いじり(第三引数:1.0f -> 3f) 2018.12.27
			hands.right.transform.localPosition = Vector3.Lerp(hands.right.transform.localPosition,
				rightPos,
				3f * Time.deltaTime);
			hands.left.transform.localPosition = Vector3.Lerp(hands.left.transform.transform.localPosition,
				leftPos,
				1.0f * Time.deltaTime);
			// 指をさしたら生成する
			if (IsNear(hands.right.transform.localPosition, rightPos, 0.25f) &&
				IsNear(hands.left.transform.localPosition, leftPos, 0.25f))
			{
				// 3体生成する 2019.01.19
				for (int i = 0; i < 3 && zakoObj.Count < 21; ++i)
				{
					InsZako();
				}
				endPointing = true;
			}
		}
		// ちょっと後ろにためる
		else
		{
			Wait(hands.right, new Vector3(-2.0f, 0.5f, 0.0f));
			Wait(hands.left, new Vector3(3.0f, -0.5f, 0.0f));
		}
	}

	// プレイヤーを回復させる時の動作 2018.10.27-------------------------------------------------
	void RecoverPlayer(PlayerControl playerControl)
	{
		BossHand punchHand = hands.right;
		BossHand freeHand = hands.left;
		Vector3 punchReturnPos = returnHandsPos_local[0];
		Vector3 freeReturnPos = returnHandsPos_local[1];
		Vector3 punchWaitPos = new Vector3(-2.0f, 0.5f, 0.0f);
		Vector3 freeWaitPos = new Vector3(3.0f, -0.5f, 0.0f);
		Vector3 freePunchingWaitPos = new Vector3(4.0f, 0.0f, 0.0f);
		// 終わったらもとの位置に戻す
		if (endAttack)
		{
			// 6行の処理 -> MoveReturn() 2018.12.27
			MoveReturn();
			if (IsNear(punchHand.transform.localPosition, punchReturnPos, 0.01f) &&
				IsNear(freeHand.transform.localPosition, freeReturnPos, 0.01f))
			{
				MoveStart();
				actionFrame = 0;
				endAttack = false;
			}
		}
		// 待っているときは所定の位置に	2019.01.04
		else if (actionFrame++ < attackWaitFrame)
		{
			// スパークルを生成する 2019.01.03
			if (actionFrame % 5 == 0)
			{
				punchHand.InsGreenSparckle();
			}
			if (actionFrame % 10 < 5)
			{
				// 2018.11.01
				punchHand.anim.spriteRenderer.sprite = recoverHandSprite;
			}
			else
			{
				// 2018.11.01	2018.11.06
				punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[1];
			}
			// 少し握る絵を入れる 2018.11.06
			if (actionFrame < attackWaitFrame / 10)
			{
				punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[0];
			}
			// 待つ
			Wait(punchHand, punchWaitPos);
			Wait(freeHand, freeWaitPos);
			// 弾が当てられたら終わる
			if (punchHand.isHitBullet)
			{
				punchHand.anim.spriteRenderer.color = Color.white;
				freeHand.anim.spriteRenderer.color = Color.white;
				endAttack = true;
				punchHand.isHitBullet = false;  // 2018.10.25
				freeHand.hp = 0;
				// 2018.11.01
				punchHand.anim.spriteRenderer.sprite = normalHandSprite;
			}
		}
		else
		{
			// プレイヤーに向かってパンチ 2018.10.26 数値いじり(第三引数:10.0f -> 15f) 2018.12.27
			punchHand.transform.position = Vector3.MoveTowards(punchHand.transform.position,
				playerCamera.transform.position,
				15f * Time.deltaTime);
			freeHand.transform.localPosition = Vector3.Lerp(freeHand.transform.localPosition,
				freePunchingWaitPos,
				1.0f * Time.deltaTime);
			// 2018.11.01	2018.11.06
			punchHand.anim.spriteRenderer.sprite = knuckleHandSprite[1];
			if (IsNear(punchHand.transform.position, playerCamera.transform.position, 0.01f))
			{
				playerControl.power += 3;
				// ゲージの制限
				if (playerControl.power > 16)
				{
					playerControl.power = 16;
				}
				// キラキラを生成する 2019.01.05
				energyRe.InsSparkleOnUI(playerControl.power - 2, true);
				endAttack = true;
				// 2018.11.01
				punchHand.anim.spriteRenderer.sprite = normalHandSprite;
			}
		}
	}

	// テレポートする 2019.01.19--------------------------------------------------------
	void TereportAction()
	{
		++actionFrame;
		// 手を元の位置に戻しておく 2019.01.27
		MoveReturn();
		// テレポートしていたら元の大きさに戻す
		if (transform.position == nextTereportPos)
		{
			// まず縦に伸ばす
			if (actionFrame < 25 && actionFrame > 15)
			{
				transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0f, 1.8f, 1f), 10f * Time.deltaTime);
			}
			// 次に横に伸ばす
			else if (actionFrame >= 25)
			{
				transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1.4f, 1f, 1f), 10f * Time.deltaTime);
				// xの大きさが1を超えたら動き始める
				if (transform.localScale.x >= 1f)
				{
					// 元の位置に戻す 2019.01.19
					hands.right.transform.localPosition = returnHandsPos_local[0];
					hands.left.transform.localPosition = returnHandsPos_local[1];
					transform.localScale = Vector3.one;
					// 前の状態を保持しておく
					BossConduct temp = previousAction;
					MoveStart();
					// 前の行動がカウンターであればパンチに移行する
					if (temp == BossConduct.COUNTERBLOW)
					{
						// スピードをそれぞれ0にする
						speed = Vector3.zero;
						hands.right.InitSpeed();
						hands.left.InitSpeed();
						nowAction = BossConduct.PUNCH;
						actionHands = ActionHands.RIGHT;
						hands.right.hp = 1;
						hands.left.hp = 1;
						actionFrame = attackWaitFrame / 5 * 3;
					}
				}
			}
		}
		// テレポートしていなかったら縮める
		else
		{
			// まず横に伸ばす
			if (actionFrame < 10)
			{
				transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1.3f, 0f, 1f), 10f * Time.deltaTime);
			}
			// 次に縦に伸ばす
			else
			{
				transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(-0.4f, 1.8f, 1f), 10f * Time.deltaTime);
				// xの大きさが0.1を下回ったらテレポートする
				if (transform.localScale.x <= 0.1f)
				{
					transform.position = nextTereportPos;
					actionFrame = 0;
					transform.localScale = new Vector3(1.3f, 0.0001f, 1f);
				}
			}
		}
	}

	// カウンター 2019.01.27----------------------------------------------------------------
	void CounterblowAction()
	{
		// 待ったらテレポートする 2019.01.31
		if (actionFrame++ > 30)
		{
			// テレポートする
			previousAction = nowAction;
			nowAction = BossConduct.TELEPORT;
			hands.right.anim.spriteRenderer.sprite = normalHandSprite;
			hands.left.anim.spriteRenderer.sprite = normalHandSprite;
			actionFrame = 0;
		}
		// 待っている間震える 2019.01.31
		else
		{
			ShakeObj(gameObject);
			ShakeObj(hands.right.gameObject);
			ShakeObj(hands.left.gameObject);
		}
	}

	// ザコを生成する 2019.02.03---------------------------------------------------------
	void InsZako()
	{
		ZakoAction za = Instantiate(zakoPrefab, transform.position, zakoPrefab.transform.localRotation).GetComponent<ZakoAction>();
		Vector2Int targetPos = Vector2Int.zero;
		targetPos.x = Random.Range(-3, 3 + 1) * 2 - 33;
		targetPos.y = Random.Range(-5, -3 + 1) * 2 - 1;
		// 重ならないようにする 2019.01.27
		while (zakoTargetPosList.Contains(targetPos))
		{
			targetPos.x = Random.Range(-3, 3 + 1) * 2 - 33;
			targetPos.y = Random.Range(-5, -3 + 1) * 2 - 1;
		}
		za.targetPos = new Vector3(targetPos.x, Random.Range(4.5f, 5.5f), targetPos.y);
		zakoObj.Add(za);
		zakoTargetPosList.Add(targetPos);
	}

	// 渡されたオブジェクトを呼ばれている間震えさせる 2018.11.02---------------------------
	void ShakeObj(GameObject shake)
	{
		// 震える
		shake.transform.position += new Vector3(0.1f * sign, 0, 0);
	}

	// ダメージを受けたときのアクション---------------------------------------------------
	void DamageAction()
	{
		// 震える 2018.11.02
		ShakeObj(gameObject);
		Vector3 nockbackPos = new Vector3(0, nockbackSpeed, 0);
		transform.position += nockbackPos;
		if (nockbackSpeed > -0.2f)
		{
			nockbackSpeed -= 0.015f;	// 数値いじり 2019.01.19
			if (nockbackSpeed < -0.2f)
			{
				nockbackSpeed = -0.2f;
			}
		}
	}

	// パーティクルの表示非表示2019.02.20------------------------------------------------
	public void SetParticle(bool set)
	{
		deadParticle.Play(set);
		deadParticle.Stop(!set);
	}

	// 手がダメージを受けた時のアクション 2019.01.13--------------------------------------
	void DamageHandAction(BossHand dmgHand)
	{
		Vector3 nockbackPos = new Vector3(0, nockbackSpeed, 0);
		dmgHand.transform.position += nockbackPos;
		if (nockbackSpeed > -0.2f)
		{
			nockbackSpeed -= 0.02f;
			if (nockbackSpeed < -0.2f)
			{
				nockbackSpeed = -0.2f;
			}
		}
	}

	// お亡くなりになった時のアクション 2018.12.06------------------------------------------
	void Dead()
	{
		// オバケを停止させる 2019.03.22
		ghostStop = true;
		// パーティクルを再生する 2019.02.20
		if (EnemyManager.isOnParticle)
		{
			SetParticle(true);
			deadParticle.Play();
		}
		// ザコを消す 2019.02.26
		for (int i = 0; i < zakoObj.Count; ++i)
		{
			if (zakoObj[i].isActiveAndEnabled)
			{
				zakoObj[i].DeadAction();
			}
		}
		// スピードを0にする
		rb.velocity = Vector3.zero;
		// ばんそうこうが表示されていたら消す 2019.01.09
		for (int i = 0; i < bandaidSRList.Count; ++i)
		{
			bandaidSRList[i].enabled = false;
		}
		hands.right.InitSpeed();
		hands.left.InitSpeed();
		// 本体の絵を変える 2019.02.26
		anim.spriteRenderer.sprite = cancelAttackSprite;
		if (anim.spriteRenderer.color.a <= 0.0f)
		{
			SetParticle(false);	// 2019.03.03
			anim.spriteRenderer.color = Color.clear;
			hands.right.anim.spriteRenderer.color = Color.clear;
			hands.left.anim.spriteRenderer.color = Color.clear;
			if (actionFrame == 30)
			{
				// 2019.03.03
				dropItem = Instantiate(dropItemPrefab, transform.position, dropItemPrefab.transform.localRotation).GetComponent<SpriteRenderer>();
				dropItem.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				dropItem.color = new Color(1f, 1f, 1f, 0f);
				GameObject o = Instantiate(deadEffect, transform.position, deadEffect.transform.localRotation);
				o.transform.localScale = Vector3.one * 3;
				// UIを消す 2019.03.11
				uiManager.gameFinish();
			}
			if(actionFrame == 120)
			{
				// 音を鳴らす 2019.02.21 → タイミングの変更 2019.04.09
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossLetterGet);
			}
			// 値調整 2019.04.06
			else if (actionFrame > 420)
			{
				gameObject.SetActive(false);
				// 2018.11.14 clearへ移行
				SceneManager.LoadScene("clear");
			}
			// 手紙を画面に持ってくる 2019.03.03	値調整 2019.04.06
			else if (actionFrame > 290)
			{
				dropItem.transform.localScale = Vector3.Lerp(dropItem.transform.localScale, new Vector3(1.05f, 1.05f, 1.05f), 4f * Time.deltaTime);
				dropItem.transform.position = Vector3.Slerp(dropItem.transform.position, new Vector3(-33f, -1.15f, -10f), 2f * Time.deltaTime);
				if (dropItem.transform.localScale.x > 1f) { dropItem.transform.localScale = Vector3.one; }
			}
			// 手紙を揺らしておく 2019.03.03	値調整 2019.04.06
			else if (actionFrame > 110)
			{
				dropItem.transform.position += new Vector3(0f, 0f, shake);
				// 色を濃くする 2019.03.10
				dropItem.color += new Color(0.0f, 0.0f, 0.0f, 1f / 60.0f);
			}
		}
		else
		{
			// 少し止まる 2019.03.10
			if (actionFrame < 130)
			{
				SetParticle(false);
				deadParticle.Stop();
				if (((actionFrame > 30 && actionFrame < 50) || (actionFrame > 80 && actionFrame < 100)) && actionFrame % 3 == 0)
				{
					ShakeObj(gameObject);
					
				}
				if(actionFrame == 65)
				{
					// 音を鳴らす 2019.04.03
					EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossDie);
				}
			}
			else
			{
				
				// 位置を下げる
				transform.position += new Vector3(0.0f, 0.01f, 0.01f);
				// 色を薄くする
				anim.spriteRenderer.color -= new Color(0.0f, 0.0f, 0.0f, 0.8f / 60.0f);
				hands.right.anim.spriteRenderer.color -= new Color(0.0f, 0.0f, 0.0f, 0.8f / 60.0f);
				hands.left.anim.spriteRenderer.color -= new Color(0.0f, 0.0f, 0.0f, 0.8f / 60.0f);
				// 震える
				if (actionFrame % 3 == 0)
				{
					ShakeObj(gameObject);
				}
				if (anim.spriteRenderer.color.a <= 0.0f)
				{
					actionFrame = 0;
				}
			}
			// 爆発の生成
			if (actionFrame % 3 == 0)
			{
				if (EnemyManager.isOnParticle)
				{
					Vector3 insPos = transform.position;
					insPos.x += Random.Range(-1.0f, 1.0f);
					insPos.y += Random.Range(-1.0f, 1.0f);
					insPos.z += Random.Range(-1.0f, 1.0f);
					Instantiate(deadEffect, insPos, deadEffect.transform.localRotation);
				}
			}
		}
		actionFrame++;
	}

	// ザコの状態を体当たりにする 2018.12.20--------------------------
	void SwitchZakoAction()
	{
		if (zakoObj.Count <= 0) { return; }	// 2018.12.24
		isZakoAttacking = true;
		int cnt = 0;	// エラー防止 2018.12.28
		for (attackingZakoNum = Random.Range(0, zakoObj.Count);
			!zakoObj[attackingZakoNum].isActiveAndEnabled || zakoObj[attackingZakoNum].isInvincible && cnt < zakoObj.Count; ++attackingZakoNum, ++cnt)
		{
			// エラー防止 2018.12.28
			if (attackingZakoNum + 1 >= zakoObj.Count) { attackingZakoNum = -1; }
		}
		if (cnt == zakoObj.Count)
		{
			isZakoAttacking = false;
			attackingZakoNum = 0;
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Bullet")
		{
			// 無敵時間外であればダメージを受ける 2019.01.19	条件追加 2019.01.27
			if (invincibleFrame <= 0 && nowAction != BossConduct.TELEPORT)
			{
				--hp;
				damageFrame = damageFrameMax + bandaidFrameMax;	// ばんそうこう表示時間を追加 2019.01.09
				invincibleFrame = damageFrameMax + 90;			// 無敵時間適用 2019.01.19
				// ノックバック
				nockbackSpeed = 0.2f;
				Destroy(col.gameObject);
				// 音を鳴らす 2019.01.08
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossDamage);
				// 2018.11.13 UI処理
				uiManager.Damages(false);
				// 体力が半分の時に怒る 2019.02.16	2019.04.03
				if(hp == maxHp / 2 || hp == maxHp - 4)
				{
					angryFrame = angryFrameMax;
				}
				if (hp == 0)
				{
					actionFrame = 0;
				}
				// 2019.02.26 被弾エフェクト
				if (playerControl.parFlag == true)
				{
					Instantiate(fireworkPar, col.transform.position, fireworkPar.transform.localRotation);
				}
			}
		}
	}
}

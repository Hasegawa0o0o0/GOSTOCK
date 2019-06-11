/*---------------------------------------------------------------------------------------------------
* FileName			: GameMaster.cpp
* Author			: 17CU0126 長谷川勇太
* Created date		: 2018.0.
* log				: 2018.06.15 カメラマスターからゲームマスターに変更 長谷川
*					: 2018.06.28 カメラマスターを追加
*					: 2018.06.30 ゴーストマスターを追加
*					: 2018.07.11 スピードのあげ方の変更
*					: 2018.10.03 [Re]魂の削除
*					: 2018.10.14 EnemyManagerの追加
*					: 2018.10.17 エネミー削除
*					: 2018.10.20 EnemyManagerの引数にplayerControlを渡す
*					: 2019.01.03 時計が壊されたらボスの行動を遅くする
*					: 2019.02.05 始まる前に扉を動かす
*					: 2019.03.02 プレイヤーが操作可能かどうかのフラグを追加
---------------------------------------------------------------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
	PlayerControl playerControl;			// プレイヤー情報
	public static bool isOperationPlayer;	// プレイヤーが操作できるかどうか 2019.03.02
	//public EnemyAction enemyAction;		// エネミー情報(unity側設定) 2018.10.17
	EnemyManager enemyManager;				// エネミー管理 2018.10.14
	UIManager ui;							// UI管理情報
	bool accumulatePower = false;			// 力がたまったかどうか 2018.06.30
	GhostMaster ghostMaster;				// ゴーストマスター 2018.06.30
	int[] ghostMaxFrame = new int[100];		// 2018.06.30
	bool inNum;								// 
	public static int recovery;				// 回収率
	bool isPlay;							// プレイフラグ
	const int startFrameMax = 80;			// マックスの時間
	int startFrame = startFrameMax;			// ゲームが始まるまでのカウント用
	public AudioSource audioSource;

	// ドア関連
	public GameObject doors;				// ドア全体のオブジェクト 2019.02.05
	public GameObject gizmoL;				// ドア操作用のギズモ、Lは+、Rは-で開く
	public GameObject gizmoR;
	public int openTime;					// ドアが開ききるまでの時間
	private int openFrame;					// フレーム換算
	private float doorMove;					// 1フレームごとの回転、0~120°までが理想？
	private bool openFlag = false;			// ドアの判断

	// 2018.12.13 時計を破壊した時にゲームスピードを遅くする
	public bool clockworkFlag = false;
	private int slowFrame = 0;

	void Start()
	{
		// プレイヤーの情報を取得
		playerControl = FindObjectOfType<PlayerControl>();
		// EnemyManager取得 2018.10.14
		enemyManager = FindObjectOfType<EnemyManager>();
		// UI管理情報を取得
		ui = FindObjectOfType<UIManager>();
		// 2018.06.30
		ghostMaster = FindObjectOfType<GhostMaster>();
		// カウントダウン用にisPlayをfalseに
		isPlay = false;
		audioSource.volume = 0;
		this.audioSource.Play();
		// ドア関連
		openFrame = openTime * 60;
		doorMove = 120.0f / (float)openFrame;
		openFlag = true;
		// 2019.03.02
		isOperationPlayer = true;
	}

	void Update()
	{
		// デバッグ用-------------------------------
		{
			//{
			//	playerControl.DebugAction();
			//	//if (Input.GetKeyDown(KeyCode.B))
			//	//{
			//	//	clockworkFlag = true;
			//	//}
			//}
		}
		//-----------------------------------------------------
		if (audioSource.volume < 1)
		{
			audioSource.volume += 0.007f;
		}
		else
		{
			audioSource.volume = 1;
		}
		// ゲームプレイ中の時
		if (isPlay)
		{
			if (playerControl)
			{
				// 2019.03.02
				if (isOperationPlayer)
				{
					playerControl.Action();
				}
				else
				{
					playerControl.SetCursorVelocity(Vector3.zero);
				}
			}
			// 2018.10.14 これからEnemyActionの代わりになるもの
			if (enemyManager)
			{
				enemyManager.EnemyControl(playerControl, clockworkFlag);   // 2018.10.20 引数追加 2019.01.03
			}
			if (accumulatePower)
			{

			}
			else
			{
				if (playerControl.power >= 1) { accumulatePower = true; }
			}
		}
		// ゲーム開始時
		else
		{
			// ドアを開く 2019.02.05
			if (openFlag)
			{
				gizmoL.transform.Rotate(new Vector3(0, doorMove, 0));
				gizmoR.transform.Rotate(new Vector3(0, -doorMove, 0));
			}
			if (gizmoL.transform.localEulerAngles.y >= 120 || gizmoR.transform.localEulerAngles.y <= -120)
			{
				openFlag = false;
			}
			if (doors.transform.position.y > -17f)
			{
				doors.transform.Translate(new Vector3(0f, 0f, -0.298f));
			}
			--startFrame;
			// 3を表示する
			if (startFrame > startFrameMax - 70)
			{
				print("3");
				if (startFrame == startFrameMax - 79)
				{
					BGMMaster.instance.SoundEffect(23);
				}
			}
			// 2を表示する
			else if (startFrame > startFrameMax - 71)
			{
				print("2");
				if (startFrame == startFrameMax - 129)
				{
					BGMMaster.instance.SoundEffect(23);
				}
			}
			// 1を表示する
			else if (startFrame > startFrameMax - 72)
			{
				print("1");
				if (startFrame == startFrameMax - 189)
				{
					BGMMaster.instance.SoundEffect(24);
				}
			}
			// startを表示する
			else if (startFrame > 0)
			{
				//audioSource.volume += 0.01f;
				print("start!");
			}
			// ゲームを始める
			else
			{
				isPlay = true;
			}
		}
	}
}

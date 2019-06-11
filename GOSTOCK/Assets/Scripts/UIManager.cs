using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//-------------------------------------
// GameMasterに処理を任せる予定
//-------------------------------------
public class UIManager : MonoBehaviour
{
	public PlayerControl pc;							// unity側設定
	public Enemy2Status es;                             // 同上
	public GameObject e1;
	public GameObject e2;
	bool ins = false;
	public GameObject bomPrefab1;            // 爆発プレハブ
	public GameObject bomPrefab2;
	public GameObject[] playerHp = new GameObject[20];  // 同上
	public GameObject[] enemyHp = new GameObject[25];   // 同上
	public SpriteRenderer lanternGhostSp;				// 同上
	public Sprite[] lanternList = new Sprite[3];		// 同上
	int cntP = 0;
	int cntE = 0;
	public CurtainMove curtainMove;	// unity側の設定
	int frame;                      // シーン移行用
	const int frameMax = 120;       // 同上

	// [Re]追加分
	public Image[] cameraChange = new Image[4];			// カメラ切り替えのUI([0]撮影On [1]撮影Off [2]攻撃On [3]攻撃Off)
	public Image[] hpImages = new Image[2];				// HPゲージ操作用(0:プレイヤー、1:エネミー)
	private double[] decreasedLevels = new double[2];	// HPゲージの減少量、変更が可能になるように(0:プレイヤー、1:エネミー)
	public GameObject EgageFrame;						// エネミーのゲージ枠
	public Color[] pinchColors;							// HPゲージが少ない時(ピンチの時)に色を変更させる
	private bool fadeHPFlag = true;						// HPゲージを点滅させるために使用
	public bool colorChange = true;						// HPゲージの色を変えるために使用
	public GameObject[] uiSummary;						// UI達のまとめ(ゲームが終わった時に使用)

	void Start()
	{
		// UIの個数をカウント
		for (int i = 0; i < 20; ++i)
		{
			if (playerHp[i]) { cntP++; }
			else { break; }
		}
		for (int i = 0; i < 20; ++i)
		{
			if (enemyHp[i]) { cntE++; }
			else { break; }
		}
		// [Re]追加部分
		cameraChange[0].enabled = true;
		cameraChange[3].enabled = true;
		const double V = 1.0;
		// 減少量設定
		decreasedLevels[0] = V / (double)pc.hp;
		decreasedLevels[1] = V / 15.0;
		// クラッコのゲージを非表示に
		EgageFrame.SetActive(false);
		hpImages[1].enabled = false;
	}

	void Update()
	{
		// hpに合わせて消していく
		for (int i = 0; i < cntP; ++i)
		{
			// hpが下回っていたら消す
			if (pc.hp <= i)
			{
				playerHp[i].SetActive(false);
				break;
			}
			// デバッグ用
			else
			{
				playerHp[i].SetActive(true);
			}
		}

		// [Re]追加部分
		if (pc.photoFlag == true)		// 攻撃
		{
			for (int i = 0; i < 4; ++i)
			{
				cameraChange[i].enabled = false;
			}
			cameraChange[1].enabled = true;
			cameraChange[2].enabled = true;
		}
		else							// 撮影
		{
			for (int i = 0; i < 4; ++i)
			{
				cameraChange[i].enabled = false;
			}
			cameraChange[0].enabled = true;
			cameraChange[3].enabled = true;
		}

		// HPゲージの点滅			2019.03.05
		if (pc.hp == 2)
		{
			FadeHPUI(0);
		}
		else if (pc.hp == 1)
		{
			FadeHPUI(1);
		}
	}

	// [Re]ダメージ処理、EP=Player or Enemy(true:Player、false:Enemy)
	public void Damages(bool PEFlag)
	{
		if (PEFlag == true)
		{
			hpImages[0].fillAmount -= (float)decreasedLevels[0];
		}
		else
		{
			hpImages[1].fillAmount -= (float)decreasedLevels[1];
		}
	}

	// HPゲージ色を変更しつつ、点滅させる ------------------------------------------------------------------
	public void FadeHPUI(int num)
	{
		if (fadeHPFlag == true)
		{
			// 色変更をかける
			if (colorChange == true)
			{
				hpImages[0].color = pinchColors[num];
				colorChange = false;
			}
			// 状態によって点滅速度を変更させる
			if (num == 0)
			{
				hpImages[0].color -= new Color(0, 0, 0, 0.02f);
			}
			else
			{
				hpImages[0].color -= new Color(0, 0, 0, 0.05f);
			}
			if (hpImages[0].color.a <= 0)
			{
				fadeHPFlag = false;
			}
		}
		else
		{
			// 状態によって点滅速度を変更させる
			if (num == 0)
			{
				hpImages[0].color += new Color(0, 0, 0, 0.02f);
			}
			else
			{
				hpImages[0].color += new Color(0, 0, 0, 0.05f);
			}
			if (hpImages[0].color.a >= 1)
			{
				fadeHPFlag = true;
			}
		}
	}

	// [Re]ゲージを表示、非表示
	public void DisplayEGages(bool Flag)
	{
		if (Flag == true)
		{
			EgageFrame.SetActive(true);
			hpImages[1].enabled = true;
		}
		else
		{
			EgageFrame.SetActive(false);
			hpImages[1].enabled = false;
		}
	}

	// [Re] ゲームが終わった時に各種UIを非表示に
	public void gameFinish()
	{
		for(int i = 0; i < uiSummary.LongLength; ++i)
		{
			uiSummary[i].SetActive(false);
		}
	}
}

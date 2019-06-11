// 参考サイト：http://gomafrontier.com/unity/492
// 2018.07.13 ゲージの微調整
// 2018.10.31 [Re]ゲージの調整、及び仕様変更
// 2018.11.05 [Re]ゲージの増加傾向変更
// 2018.11.07 [Re]ゲージの増加の挙動を変更、ファイルを分割
// 2019.01.05 [Re]ボスが緑パンチをしたときにキラキラを出す
// 2019.01.13 [Re]キラキラの調整
// 2019.01.18 [Re]コアの電気マークを光らせる
// 2019.01.19 [Re]絵画のオバケを撮影した時にも水色のキラキラを
// 2019.03.07 [Re]ゲージの上昇にディレイをかける
// 2019.03.19 [Re]満タンになった時に起きるバグを回避？

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;	// リストを使うのに必要 2019.01.05

public class EnergyRe : MonoBehaviour
{
	public PlayerControl pc;

	// [Re]
	public Sprite[] coreImage = new Sprite[3];		// [0]Min [1]Max [2]光る
	public Image[] cores = new Image[3];
	public int gageMax = 0;
	public bool coreFlag = false;
	public Image[] energys = new Image[15];
	public bool coreFull = false;					// コアが満タンになったか確認する(true: 満タン、false:満タンじゃない)
	public bool energyFull = false;					// ↑＋満タンになったか確認する用(true: 満タン、false:満タンじゃない)
	private bool accumulate = false;				// コアが一回でも貯まったかどうか(true:貯まった、false:貯まっていない)

	// メモリの増加を遅らせるのに使用
	public int delayNow = 0;						// 現在のディレイ時間
	public int delayMax;							// ↑の最大値
	private bool delayStart = false;				// ディレイを始めるかどうか
	private bool risingFlag = false;				// メモリを上昇していいか(true:上昇、false:停止)

	// コアの電気マークを光らせるのに使用
	public int flashFrameMAX = 0;
	private int flashFrame = 0;
	private bool flashFlag = false;

	// [Re]キラキラを出すのに使う 2019.01.05
	Canvas canvas;
	public Image recoverSparklePrefab;
	List<Image> recoverSparkle = new List<Image>();
	List<Vector3> recoverSparkleVelocity = new List<Vector3>();

	// エネルギーの状態を示すUI群
	public SpriteRenderer cursorSR;					// カーソルの状態を把握するために
	public GameObject energyUI;						// エネルギーのUI
	public SpriteRenderer energyUISR;				// ↑のSR
	public Sprite[] energyUISprites;				// エネルギーのUIの種類([0]Empty [1]Stock lost [2]Stock full)
	public Color[] energyUIColors;					// ↑の色情報
	private bool fadeUIFlag = true;					// エネルギーUIを点滅させるために使用
	private bool colorChange = true;				// エネルギーUIの色を変えるために使用

	// パーティクルに違いを出すために使用
	private bool particleFlag;						// true:緑(ボス用) false:青(絵画用)

	// SE再生用
	public BGMMaster BGM;
	private int oldPower = 0;

	void Start()
	{
		// キャンバスを取得 2019.01.05
		canvas = FindObjectOfType<Canvas>();

		for(int i = 0; i < 15; ++i)
		{
			energys[i].enabled = false;
		}
	}

	void Update()
	{
		// デバック用
		if(Input.GetKey(KeyCode.Tab))
		{
			risingFlag = true;
		}

		// 満タンを確認する		2019.03.19 条件と位置変更
		if (coreFull == true && pc.power >= 15)
		{
			energyFull = true;
		}
		else
		{
			energyFull = false;
		}

		// メモリの上昇にディレイをかける
		if (delayStart == false && oldPower != pc.power)
		{
			delayStart = true;
		}

		if(delayStart == true)
		{
			++delayNow;
			if(delayNow >= delayMax)
			{
				delayNow = 0;
				delayStart = false;
				risingFlag = true;
			}
		}

		// エネルギーの上昇
		if(risingFlag == true)
		{
			// エネルギーのSE再生 2019.02.27
			if ((oldPower != pc.power) && (oldPower < pc.power) && energyFull == false)
			{
				BGMMaster.instance.SoundEffect(25);
				oldPower = pc.power;
				risingFlag = false;
			}
			// メモリ(+コア)の上昇
			switch(pc.power)
			{
				case 0:
					DisplayLoops(0);
					break;
				case 1:
					DisplayLoops(1);
					break;
				case 2:
					DisplayLoops(2);
					break;
				case 3:
					DisplayLoops(3);
					break;
				case 4:
					DisplayLoops(4);
					break;
				case 5:
					DisplayLoops(5);
					break;
				case 6:
					DisplayLoops(6);
					break;
				case 7:
					DisplayLoops(7);
					break;
				case 8:
					DisplayLoops(8);
					break;
				case 9:
					DisplayLoops(9);
					break;
				case 10:
					DisplayLoops(10);
					break;
				case 11:
					DisplayLoops(11);
					break;
				case 12:
					DisplayLoops(12);
					break;
				case 13:
					DisplayLoops(13);
					break;
				case 14:
					DisplayLoops(14);
					break;
				case 15:
					DisplayLoops(15);
					break;
				case 16:
					// 2019.02.27 SE追加
					if (energyFull == false)
					{
						BGMMaster.instance.SoundEffect(26);
					}
					// gageMaxが3より大きくならないように
					if(gageMax < 3)
					{
						gageMax++;
					}
					else
					{
						gageMax = 3;
					}
					// コアの生成
					if(gageMax == 1)
					{
						cores[0].sprite = coreImage[1];
						cores[1].sprite = coreImage[0];
						cores[2].sprite = coreImage[0];
					}
					else if(gageMax == 2)
					{
						cores[0].sprite = coreImage[1];
						cores[1].sprite = coreImage[1];
						cores[2].sprite = coreImage[0];
					}
					else if(gageMax == 3)
					{
						coreFull = true;			// 2019.03.19
						cores[0].sprite = coreImage[1];
						cores[1].sprite = coreImage[1];
						cores[2].sprite = coreImage[1];
					}
					if (energyFull == false)
					{
						pc.power = 0;
						DisplayLoops(0);
					}
					else
					{
						pc.power = 15;
						DisplayLoops(15);
					}
					// コアが貯まったか判断させる 2019.03.07
					if (accumulate == false)
					{
						accumulate = true;
					}
					break;
				default:
					pc.power = 15;
					DisplayLoops(15);
					break;
			}
		}
		// コアの減少
		if (coreFlag == true)
		{
			coreFull = false;
			if (gageMax == 0)
			{
				cores[0].sprite = coreImage[0];
				cores[1].sprite = coreImage[0];
				cores[2].sprite = coreImage[0];
			}
			else if (gageMax == 1)
			{
				cores[0].sprite = coreImage[1];
				cores[1].sprite = coreImage[0];
				cores[2].sprite = coreImage[0];
			}
			else if (gageMax == 2)
			{
				cores[0].sprite = coreImage[1];
				cores[1].sprite = coreImage[1];
				cores[2].sprite = coreImage[0];
			}
			DisplayLoops(15);
			coreFlag = false;
		}
		// コアが一個以上あるときに電気マークを光らせる
		++flashFrame;
		if(flashFrame >= flashFrameMAX)
		{
			if (flashFlag == true)
			{
				for (int i = 0; i < 3; ++i)
				{
					if (cores[i].sprite.name != "UI_Original_Core_1")
					{
						cores[i].sprite = coreImage[2];
					}
				}
				flashFlag = false;
				flashFrame = 0;
			}
			else
			{
				for (int i = 0; i < 3; ++i)
				{
					if (cores[i].sprite.name != "UI_Original_Core_1")
					{
						cores[i].sprite = coreImage[1];
					}
				}
				flashFlag = true;
				flashFrame = 0;
			}
		}

		// エネルギーのUI操作		2019.03.03 → 2019.03.05修正
		if (cursorSR.sprite.name == "攻撃 カーソル")
		{
			if (pc.power == 0 && gageMax == 0)
			{
				energyUI.SetActive(true);
				energyUISR.sprite = energyUISprites[0];
				FadeEnergyUI(0);
			}
			else if (gageMax == 0 && accumulate == true)
			{
				energyUI.SetActive(true);
				energyUISR.sprite = energyUISprites[1];
				FadeEnergyUI(1);
				if (pc.power <= 1)
				{
					colorChange = true;
					fadeUIFlag = true;
				}
			}
			else
			{
				energyUI.SetActive(false);
				energyUISR.color = Color.white;
				colorChange = true;
				fadeUIFlag = true;
			}
		}
		if (cursorSR.sprite.name == "撮影 カーソル" || cursorSR.sprite.name == "_Photo_Area")
		{
			if (energyFull == true)
			{
				energyUI.SetActive(true);
				energyUISR.sprite = energyUISprites[2];
				FadeEnergyUI(2);
			}
			else
			{
				energyUI.SetActive(false);
				energyUISR.color = Color.white;
				colorChange = true;
				fadeUIFlag = true;
			}
		}


		// スパークルの処理、アルファチャンネルが0になったものから排除 2019.01.05
		for (int i = 0; i < recoverSparkle.Count; ++i)
		{
			recoverSparkle[i].transform.position += recoverSparkleVelocity[i];
			recoverSparkle[i].color -= new Color(0f, 0f, 0f, 0.75f / 60f);
			// 点滅させる、緑と青色で分岐するように変更
			if (particleFlag == true)
			{
				recoverSparkle[i].color = new Color(Random.Range(20f / 255f, 120f / 255f), Random.Range(120f / 255f, 1f),
					Random.Range(20f / 255f, 120f / 255f), recoverSparkle[i].color.a);
			}
			else
			{
				recoverSparkle[i].color = new Color(Random.Range(0f / 0f, 0f / 0f), Random.Range(120f / 255f, 1f),
					Random.Range(120f / 255f, 1f), recoverSparkle[i].color.a);
			}
			// アルファチャンネルが0以下になったら削除する
			if (recoverSparkle[i].color.a < 0f)
			{
				recoverSparkleVelocity.RemoveAt(i);
				GameObject o = recoverSparkle[i].gameObject;
				recoverSparkle.RemoveAt(i);
				Destroy(o);
			}
		}
	}

	// 表示ループ ---------------------------------------------------------------------------------------------
	private void DisplayLoops(int d)
	{
		// 一回全部非表示
		for (int i = 0; i < 15; i++)
		{
			energys[i].enabled = false;
		}
		// 表示
		for (int i = 0; i < d; i++)
		{
			energys[i].enabled = true;
		}
		oldPower = pc.power;
	}

	// エネルギーUIの色を変更しつつ、点滅させる ------------------------------------------------------------------
	public void FadeEnergyUI(int num)
	{
		if (fadeUIFlag == true)
		{
			// 色変更をかける
			if (colorChange == true)
			{
				energyUISR.color = energyUIColors[num];
				colorChange = false;
			}
			// 状態によって点滅速度を変更させる
			if (num == 0)
			{
				energyUISR.color -= new Color(0, 0, 0, 0.05f);
			}
			else if (num != 2)
			{
				energyUISR.color -= new Color(0, 0, 0, 0.03f);
			}
			if (energyUISR.color.a <= 0)
			{
				fadeUIFlag = false;
			}
		}
		else
		{
			// 状態によって点滅速度を変更させる
			if (num == 0)
			{
				energyUISR.color += new Color(0, 0, 0, 0.05f);
			}
			else if(num != 2)
			{
				energyUISR.color += new Color(0, 0, 0, 0.03f);
			}
			if (energyUISR.color.a >= 1)
			{
				fadeUIFlag = true;
			}
		}
	}


	// スパークルを引数で渡されたUIの箇所に8つ生成する 2019.01.05 調整 2019.01.13----------------------------------
	public void InsSparkleOnUI(int uiNum, bool changeF)
	{
		for (int i = 0; i < 10; ++i)
		{
			Image insObj = Instantiate(recoverSparklePrefab, Vector3.zero,
				Quaternion.identity);
			insObj.rectTransform.localScale = Vector3.one * 0.1069167f;
			insObj.rectTransform.localPosition = energys[uiNum].rectTransform.position;
			// キャンバスの子どもにする(ImageはCanvasの子どもでないと表示されない)
			insObj.transform.SetParent(canvas.transform);
			recoverSparkle.Add(insObj);
			float randomX = Random.Range(-0.15f, 0.15f);
			float randomY = Random.Range(-0.15f, 0.15f);
			recoverSparkleVelocity.Add(new Vector3(randomX, randomY, 0f));
		}
		if (changeF == true)
		{
			particleFlag = true;
		}
		else
		{
			particleFlag = false;
		}
	}
}

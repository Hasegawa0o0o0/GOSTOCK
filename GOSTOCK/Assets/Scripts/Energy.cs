// 参考サイト：http://gomafrontier.com/unity/492
// 2018.07.13 ゲージの微調整
// 2018.10.31 [Re]ゲージの調整、及び仕様変更

using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
	public PlayerControl pc;
	//public int power;
	public Image ghostGage;

	// [Re]
	public Sprite[] coreImage = new Sprite[2];		// 0:Min/1:Max
	public Image[] cores = new Image[3];
	public int gageMax = 0;
	public bool coreFlag = false;

	void Start()
	{
		ghostGage.fillAmount = 0.0f;
		//power = pc.power;
	}

	void Update()
	{
		// うまく伸びないので数値を入れる
		//if (pc.power == 0)
		//{
		//	if (pc.powerCnt == 0)
		//	{
		//		ghostGage.fillAmount = 0;
		//	}
		//	else if (pc.powerCnt == 1)
		//	{
		//		ghostGage.fillAmount = 0.125f;
		//	}
		//	else if (pc.powerCnt == 2)
		//	{
		//		ghostGage.fillAmount = 0.177f;
		//	}
		//}
		//else if (pc.power == 1)
		//{
		//	if (pc.powerCnt == 0)
		//	{
		//		ghostGage.fillAmount = 0.23f;
		//	}
		//	else if (pc.powerCnt == 1)
		//	{
		//		ghostGage.fillAmount = 0.29f;
		//	}
		//	else if (pc.powerCnt == 2)
		//	{
		//		ghostGage.fillAmount = 0.345f;
		//	}
		//}
		//else if (pc.power == 2)
		//{
		//	if (pc.powerCnt == 0)
		//	{
		//		ghostGage.fillAmount = 0.405f;
		//	}
		//	else if (pc.powerCnt == 1)
		//	{
		//		ghostGage.fillAmount = 0.47f;
		//	}
		//	else if (pc.powerCnt == 2)
		//	{
		//		ghostGage.fillAmount = 0.52f;
		//	}
		//}
		//else if (pc.power == 3)
		//{
		//	if (pc.powerCnt == 0)
		//	{
		//		ghostGage.fillAmount = 0.575f;
		//	}
		//	else if (pc.powerCnt == 1)
		//	{
		//		ghostGage.fillAmount = 0.635f;
		//	}
		//	else if (pc.powerCnt == 2)
		//	{
		//		ghostGage.fillAmount = 0.685f;
		//	}
		//}
		//else if (pc.power == 4)
		//{
		//	if (pc.powerCnt == 0)
		//	{
		//		ghostGage.fillAmount = 0.75f;
		//	}
		//	else if (pc.powerCnt == 1)
		//	{
		//		ghostGage.fillAmount = 0.805f;
		//	}
		//	else if (pc.powerCnt == 2)
		//	{
		//		ghostGage.fillAmount = 0.865f;
		//	}
		//}
		//else
		//{
		//	ghostGage.fillAmount = 0.916f;  //2018.07.13
		//}

		// [Re]
		switch (pc.power)
		{
			case 0:
				ghostGage.fillAmount = 0;
				break;
			case 1:
				ghostGage.fillAmount = 0.125f;
				break;
			case 2:
				ghostGage.fillAmount = 0.177f;
				break;
			case 3:
				ghostGage.fillAmount = 0.23f;
				break;
			case 4:
				ghostGage.fillAmount = 0.29f;
				break;
			case 5:
				ghostGage.fillAmount = 0.345f;
				break;
			case 6:
				ghostGage.fillAmount = 0.405f;
				break;
			case 7:
				ghostGage.fillAmount = 0.47f;
				break;
			case 8:
				ghostGage.fillAmount = 0.52f;
				break;
			case 9:
				ghostGage.fillAmount = 0.575f;
				break;
			case 10:
				ghostGage.fillAmount = 0.635f;
				break;
			case 11:
				ghostGage.fillAmount = 0.685f;
				break;
			case 12:
				ghostGage.fillAmount = 0.75f;
				break;
			case 13:
				ghostGage.fillAmount = 0.805f;
				break;
			case 14:
				ghostGage.fillAmount = 0.865f;
				break;
			case 15:
				ghostGage.fillAmount = 0.916f;
				break;
			case 16:
				// gageMaxが3より大きくならないように
				if (gageMax < 3)
				{
					gageMax++;
				}
				else
				{
					gageMax = 3;
				}
				// コアの生成
				if (gageMax == 1)
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
				else if (gageMax == 3)
				{
					cores[0].sprite = coreImage[1];
					cores[1].sprite = coreImage[1];
					cores[2].sprite = coreImage[1];
				}
				pc.power = 0;
				ghostGage.fillAmount = 0;
				break;
		}
		// コアの減少
		if (coreFlag == true)
		{
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
			ghostGage.fillAmount = 09.16f;
			coreFlag = false;
		}

		//if (pc.power > power)
		//{
		//	power = pc.power;
		//	if (ghostGage.fillAmount != 1.0f)
		//	{
		//		ghostGage.fillAmount += 0.25f;
		//	}
		//}
		//if (pc.power < power)
		//{
		//	power = pc.power;
		//	if (ghostGage.fillAmount != 0.0f)
		//	{
		//		ghostGage.fillAmount -= 0.25f;
		//	}
		//}
	}
}

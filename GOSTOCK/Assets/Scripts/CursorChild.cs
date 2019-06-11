/* -----------------------------------------------------------------------------------------
 * CursorChild.cs
 * 2019.01.13 作成
 *		2019.01.13 撮影範囲に入った時にカーソルの色を変更出来るように
 *		2019.01.15 上手い具合に色(画像)が戻らなかったので少し変更
 *		2019.01.18 絵画のオバケが対象外だったことに気付いたので修正
 *		2019.02.26 うまく動かなくなってしまったので修正
 *		2019.03.03 コアがMAX、エネルギーのメモリも最大の時に緑にならないように
 ----------------------------------------------------------------------------------------- */
using UnityEngine;

public class CursorChild : MonoBehaviour
{
	public PlayerControl playerControl;
	public EnergyRe energyRe;

	void Start ()
	{
		
	}
	
	void Update ()
	{

	}

	// 撮影範囲に入ったらPlayerControlの関数を実行
	void OnTriggerEnter(Collider col)
	{
		// エネルギーが満タンじゃないときのみ実行
		if (energyRe.energyFull == false)
		{
			// 自身がアクティブかどうかを判断
			if (this.gameObject.activeSelf == true)
			{
				if (col.tag == "Goast" || col.tag == "pGhost")
				{
					playerControl.PhotoArea(true);
				}
			}
		}
	}

	// 撮影範囲内でオバケが消えたとき
	//void OnTriggerStay(Collider coll)
	//{
	//	// 自身がアクティブかどうかを判断
	//	if (this.gameObject.activeSelf == true)
	//	{
	//		if (coll.tag == "Goast" || coll.tag == "pGhost")
	//		{
	//			playerControl.PhotoArea(false);
	//		}
	//		else
	//		{
	//			playerControl.PhotoArea(false);
	//		}
	//	}
	//}

	// 撮影範囲からでたら戻す
	void OnTriggerExit(Collider colll)
	{
		// エネルギーが満タンじゃないときのみ実行
		if (energyRe.energyFull == false)
		{
			// 自身がアクティブかどうかを判断
			if (this.gameObject.activeSelf == true)
			{
				if (colll.tag == "Goast" || colll.tag == "pGhost")
				{
					playerControl.PhotoArea(false);
				}
			}
		}
	}
}

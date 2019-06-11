/*---------------------------------------------------------------------------------------------------
// タイトル画面演出その02
//
* FileName		: TextCmd.cpp
* Author		: 17CU0116 杉田優帆
* Log			: 2018.05.28 作成
---------------------------------------------------------------------------------------------------*/
using UnityEngine;

public class TextCmd : MonoBehaviour
{
	private string TextName;
	public TitleText TT;

	void Start()
	{
		// オブジェクトの名前取得
		TextName = transform.name;
		// TitleTextのコンポーネント取得
		TT = FindObjectOfType<TitleText>();
	}

	void OnTriggerStay2D(Collider2D col)
	{
		// 文字に見立てた判定に当たっている時
		if (col.tag == "Player")
		{
			if (TextName == "Play")
			{
				TT.isPlayText = true;
				TT.isSettingText = false;
				TT.isTutorialText = false;
			}
			else if (TextName == "Setting")
			{
				TT.isSettingText = true;
				TT.isPlayText = false;
				TT.isTutorialText = false;
			}
			else if (TextName == "Tutorial")
			{
				TT.isTutorialText = true;
				TT.isPlayText = false;
				TT.isSettingText = false;
			}
		}
	}

	void OnTriggerExit2D(Collider2D col)
	{
		TT.isSettingText = false;
		TT.isPlayText = false;
		TT.isTutorialText = false;
	}
}

/*---------------------------------------------------------------------------------------------------
 * Setting.cs
 * 2018.05.28 作成
 *		2018.05.31 title画面に戻るように仮修正
 *		2018.06.06 配置変更+
 *		2018.06.07 選んで選択できるように変更
 *		2018.07.04 設定画面変更
 *		2018.07.09 上下、左右反転のみに変更(使ってないものはコメント化)
 *		2018.07.10 mainのシーンで実際に反転出来るように変更
 *		2018.07.15 ボタンの修正
 *		2018.07.18 各種コントローラー設定、コントローラーでの動作確認
 *		2019.02.13 [Re]設定の変更に伴い修正、要らないものの削除
 *		2019.12.16 デモ画面の追加
 *		2019.12.19 デモ画面の動きの修正、ボタンの変更
 *		2019.03.14 文字色を部分的に変更、RichTextにチェックを入れるとHTMLタグで記述可能
---------------------------------------------------------------------------------------------------*/
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using System;

public class Setting : MonoBehaviour
{
	// 変数宣言----------------------------------------------------------------
	public GameObject Cursor;
	public SpriteRenderer cursorSR;
	public Text InformationText;
	public static bool FlipUpDown = false;				// 上下反転、true:反転あり、false:反転なし
	public static bool FlipHorizontal = false;			// 左右反転、true:反転あり、false:反転なし
	public int Item = 0;								// 現在の選択項目(超重要)
	//private bool LR = true;							// true:左側の項目、false:右側の項目
	private bool AcceptanceFlag = false;				// 入力受付(選択中かどうか)
	public float[] RGB = new float[3];					// カーソルのGameObject用RGB値(0.0～1.0)
	private float UDTriggerAnalog;						// アナログスティックに対する連続押しを解消するための変数
	private float UDTriggerCross;						// 十字キーに対する連続押しを解消するための変数
	private float LRTriggerAnalog;						// 上下反転の左右キー用(アナログスティックVer)
	private float LRTriggerCross;						// 上下反転の左右キー用(十字キーVer)
	private float LRTrigger2Analog;						// 左右反転の左右キー用(アナログスティックVer)
	private float LRTrigger2Cross;						// 左右反転の左右キー用(十字キーVer)
	private bool ResetFlag = false;						// リセットされたかどうか
	public Text[] itemTexts;							// 設定項目

	// 音
	public AudioSource bgm;								// 設定画面のBGM(タイトルと同じ)

	// 反転デモ用
	public Image demoCursor;							// デモ用に動かすカーソル
	private Vector3 demoCursorOldVec;					// ↑の初期位置
	public GameObject[] demoLRKeySet;					// デモのキーセット(左右Ver)
	public Image[] demoLRKeys;							// デモのキー(左右Ver)
	public GameObject[] demoUDKeySet;					// デモのキーセット(上下Ver)
	public Image[] demoUDKeys;							// デモのキー(上下Ver)
	private bool nowKey = true;							// 0か1か(キーの判断用)
	private int nextCnt = 0;							// 次のキーへ表示を変更させる
	private bool returnFlag;							// 動きを戻すために使用

	void Start()
	{
		// カーソルの位置を初期化
		Cursor.transform.position = itemTexts[0].transform.position;

		// デモ用のカーソルの初期位置を入れる
		demoCursorOldVec = demoCursor.transform.position;
		for(int i = 0; i < 2; ++i)
		{
			demoLRKeySet[i].SetActive(true);
			demoLRKeys[i].enabled = false;
			demoUDKeySet[i].SetActive(false);
			demoUDKeys[i].enabled = false;
		}
	}

	void Update()
	{
		// 現在の音楽の再生時間を取得		2019.02.21
		//TitleAnimation_Re.bgmTimes = bgm.time + 0.04f;

		if (AcceptanceFlag == false)
		{
			float UDXboxA = Input.GetAxisRaw("Vertical");
			float UDXboxC = Input.GetAxis("CrossY");
			// 上下の入力受付
			if (Input.GetKeyDown(KeyCode.DownArrow) || (UDXboxA < 0 && UDTriggerAnalog == 0.0f) || (UDXboxC < 0 && UDTriggerCross == 0.0f))
			{
				++Item;
				if (Item > 3)
				{
					Item = 0;
				}
			}
			if (Input.GetKeyDown(KeyCode.UpArrow) || (UDXboxA > 0 && UDTriggerAnalog == 0.0f) || (UDXboxC > 0 && UDTriggerCross == 0.0f))
			{
				--Item;
				if (Item < 0)
				{
					Item = 3;
				}
			}
			// カーソル移動
			CursorPosition();
			// 決定
			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
			{
				AcceptanceFlag = true;
			}
			UDTriggerAnalog = UDXboxA;
			UDTriggerCross = UDXboxC;
		}
		else
		{
			switch (Item)
			{
				// 上下反転 --------------------------------------------------------------------------------------
				case 0:
					cursorSR.color = new Color(RGB[0], RGB[1], RGB[2]);
					if (FlipUpDown == false)
					{
						InformationText.text = "移動時の上下反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転<color=#ff0000>なし</color> です。";
					}
					else
					{
						InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転あり です。";
					}
					float LRXboxA = Input.GetAxisRaw("Horizontal");
					float LRXboxC = Input.GetAxis("CrossX");
					if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow) ||
						((LRXboxA > 0 || LRXboxA < 0) && LRTriggerAnalog == 0.0f) || ((LRXboxC > 0 || LRXboxC < 0) && LRTriggerCross == 0.0f))
					{
						if (FlipUpDown == true)
						{
							FlipUpDown = false;
						}
						else
						{
							FlipUpDown = true;
						}
					}
					if (FlipUpDown == false)
					{
						InformationText.text = "移動時の上下反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転<color=#ff0000>なし</color> です。";
					}
					else
					{
						InformationText.text = "移動時の上下反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転<color=#ff0000>あり</color> です。";
					}
					if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						AcceptanceFlag = false;
						if (FlipUpDown == false)
						{
							InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転なし です。";
						}
						else
						{
							InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転あり です。";
						}
						cursorSR.color = new Color(1.0f, 1.0f, 1.0f);
					}
					LRTriggerAnalog = LRXboxA;
					LRTriggerCross = LRXboxC;
					FlipShow(false);
					break;
				// 左右反転 --------------------------------------------------------------------------------------
				case 1:
					cursorSR.color = new Color(RGB[0], RGB[1], RGB[2]);
					if (FlipHorizontal == false)
					{
						InformationText.text = "移動時の左右反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転<color=#ff0000>なし</color> です。";
					}
					else
					{
						InformationText.text = "移動時の左右反転設定ができます。\n現在の設定は 反転あり です。";
					}
					float LRXbox2A = Input.GetAxisRaw("Horizontal");
					float LRXbox2C = Input.GetAxis("CrossX");
					if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow) ||
						((LRXbox2A > 0 || LRXbox2A < 0) && LRTrigger2Analog == 0.0f) || ((LRXbox2C > 0 || LRXbox2C < 0) && LRTrigger2Cross == 0.0f))
					{
						if (FlipHorizontal == true) { FlipHorizontal = false; } else { FlipHorizontal = true; }
					}
					if (FlipHorizontal == false)
					{
						InformationText.text = "移動時の左右反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転<color=#ff0000>なし</color> です。";
					}
					else
					{
						InformationText.text = "移動時の左右反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転<color=#ff0000>あり</color> です。";
					}
					if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						AcceptanceFlag = false;
						if (FlipHorizontal == false)
						{
							InformationText.text = "移動時の左右反転設定ができます。\n現在の設定は 反転なし です。";
						}
						else
						{
							InformationText.text = "移動時の左右反転設定ができます。\n現在の設定は 反転あり です。";
						}
						cursorSR.color = new Color(1.0f, 1.0f, 1.0f);
					}
					LRTrigger2Analog = LRXbox2A;
					LRTrigger2Cross = LRXbox2C;
					FlipShow(true);
					break;
				// リセット ----------------------------------------------------------------------------------------
				case 2:
					cursorSR.color = new Color(RGB[0], RGB[1], RGB[2]);
					if (ResetFlag == true)
					{
						InformationText.text = "<color=#ff0000>初期化</color>しました。\nA(R)を押すと戻ります。";
					}
					else
					{
						InformationText.text = "設定を初期化します。\nB(Spece)をもう一度押すと初期化されます。\n戻る場合はA(R)を押してください。";
					}
					if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						// Reset
						FlipUpDown = false;
						FlipHorizontal = false;
						ResetFlag = true;
					}
					if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Joystick1Button0))
					{
						AcceptanceFlag = false;
						ResetFlag = false;
						cursorSR.color = new Color(1.0f, 1.0f, 1.0f);
					}
					break;
				// タイトル -----------------------------------------------------------------------------------------
				case 3:
					SceneManager.LoadScene("re_title");
					break;
			}
		}
	}

	// カーソルの位置移動＆テキストの変更
	private void CursorPosition()
	{
		Cursor.transform.position = itemTexts[Item].transform.position;
		demoCursor.transform.position = demoCursorOldVec;
		demoCursor.color = new Color(demoCursor.color.r, demoCursor.color.g, demoCursor.color.b, 1);
		switch (Item)
		{
			// 上下反転---------------------------------------------------------------
			case 0:
				if (FlipUpDown == false)
				{
					InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転なし です。";
				}
				else
				{
					InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転あり です。";
				}
				// 左右のカーソルを非表示
				for (int i = 0; i < 2; ++i)
				{
					demoLRKeySet[i].SetActive(false);
					demoLRKeys[i].enabled = false;
					demoUDKeySet[i].SetActive(true);
					demoUDKeys[i].enabled = false;
				}
				break;
			// 左右反転---------------------------------------------------------------
			case 1:
				if (FlipHorizontal == false)
				{
					InformationText.text = "移動時の左右反転設定ができます。\n現在の設定は 反転なし です。";
				}
				else
				{
					InformationText.text = "移動時の左右反転設定ができます。\n現在の設定は 反転あり です。";
				}
				// 上下のカーソルを非表示
				for (int i = 0; i < 2; ++i)
				{
					demoUDKeySet[i].SetActive(false);
					demoUDKeys[i].enabled = false;
					demoLRKeySet[i].SetActive(true);
					demoLRKeys[i].enabled = false;
				}
				break;
			// 初期化-----------------------------------------------------------------
			case 2:
				InformationText.text = "全ての設定項目をリセットします。";
				break;
			// タイトルへ-------------------------------------------------------------
			case 3:
				InformationText.text = "設定を保存してタイトルへ戻ります。";
				break;
		}
	}

	// 反転結果の表示------------------------------------------------------------------------------------------------------------
	private void FlipShow(bool BranchFlag)
	{
		// 左右反転
		if (BranchFlag == true)
		{
			// 上下のカーソルを非表示
			for(int i = 0; i < 2; ++i)
			{
				demoUDKeySet[i].SetActive(false);
				demoUDKeys[i].enabled = false;
				demoLRKeySet[i].SetActive(true);
				demoLRKeys[i].enabled = false;
			}
			if (FlipHorizontal == false)
			{
				switch (nowKey)
				{
					// 左に移動(正方向)---------------------------------------------------------------------
					case true:
						demoLRKeys[0].enabled = true;
						// 移動してフェードアウト
						if (returnFlag == false)
						{
							if (demoCursor.transform.position.x >= demoCursorOldVec.x - 1)
							{
								demoCursor.transform.position -= new Vector3(0.03f, 0, 0);
							}
							else
							{
								//demoCursor.transform.position = new Vector3(demoCursorOldVec.x - 1, demoCursorOldVec.y, demoCursorOldVec.z);
								DesKeyFade(true);
							}
						}
						// 元に戻す
						else
						{
							DesKeyFade(false);
						}
					break;
					// 右に移動(正方向)---------------------------------------------------------------------
					case false:
						demoLRKeys[1].enabled = true;
						// 移動してフェードアウト
						if (returnFlag == false)
						{
							if (demoCursor.transform.position.x <= demoCursorOldVec.x + 1)
							{
								demoCursor.transform.position += new Vector3(0.03f, 0, 0);
							}
							else
							{
								//demoCursor.transform.position = new Vector3(demoCursorOldVec.x + 1, demoCursorOldVec.y, demoCursorOldVec.z);
								DesKeyFade(true);
							}
						}
						// 元に戻す
						else
						{
							DesKeyFade(false);
						}
					break;
				}
			}
			else
			{
				switch (nowKey)
				{
					// 左に移動(逆方向)---------------------------------------------------------------------
					case true:
						demoLRKeys[0].enabled = true;
						// 移動してフェードアウト
						if (returnFlag == false)
						{
							if (demoCursor.transform.position.x <= demoCursorOldVec.x + 1)
							{
								demoCursor.transform.position += new Vector3(0.03f, 0, 0);
							}
							else
							{
								//demoCursor.transform.position = new Vector3(demoCursorOldVec.x + 1, demoCursorOldVec.y, demoCursorOldVec.z);
								DesKeyFade(true);
							}
						}
						// 元に戻す
						else
						{
							DesKeyFade(false);
						}
						break;
					// 右に移動(逆方向)---------------------------------------------------------------------
					case false:
						demoLRKeys[1].enabled = true;
						// 移動してフェードアウト
						if (returnFlag == false)
						{
							if (demoCursor.transform.position.x >= demoCursorOldVec.x - 1)
							{
								demoCursor.transform.position -= new Vector3(0.03f, 0, 0);
							}
							else
							{
								//demoCursor.transform.position = new Vector3(demoCursorOldVec.x - 1, demoCursorOldVec.y, demoCursorOldVec.z);
								DesKeyFade(true);
							}
						}
						// 元に戻す
						else
						{
							DesKeyFade(false);
						}
						break;
				}
			}
		}
		// 上下反転
		else
		{
			// 左右のカーソルを非表示
			for (int i = 0; i < 2; ++i)
			{
				demoLRKeySet[i].SetActive(false);
				demoLRKeys[i].enabled = false;
				demoUDKeySet[i].SetActive(true);
				demoUDKeys[i].enabled = false;
			}
			if (FlipUpDown == false)
			{
				switch (nowKey)
				{
					// 上に移動(正方向)---------------------------------------------------------------------
					case true:
						demoUDKeys[0].enabled = true;
						// 移動してフェードアウト
						if (returnFlag == false)
						{
							if (demoCursor.transform.position.y <= demoCursorOldVec.y + 1)
							{
								demoCursor.transform.position += new Vector3(0, 0.03f, 0);
							}
							else
							{
								//demoCursor.transform.position = new Vector3(demoCursorOldVec.x, demoCursorOldVec.y + 1, demoCursorOldVec.z);
								DesKeyFade(true);
							}
						}
						// 元に戻す
						else
						{
							DesKeyFade(false);
						}
						break;
					// 下に移動(正方向)---------------------------------------------------------------------
					case false:
						demoUDKeys[1].enabled = true;
						// 移動してフェードアウト
						if (returnFlag == false)
						{
							if (demoCursor.transform.position.y >= demoCursorOldVec.y - 1)
							{
								demoCursor.transform.position -= new Vector3(0, 0.03f, 0);
							}
							else
							{
								//demoCursor.transform.position = new Vector3(demoCursorOldVec.x, demoCursorOldVec.y - 1, demoCursorOldVec.z);
								DesKeyFade(true);
							}
						}
						// 元に戻す
						else
						{
							DesKeyFade(false);
						}
						break;
				}
			}
			else
			{
				switch (nowKey)
				{
					// 上に移動(逆方向)---------------------------------------------------------------------
					case true:
						demoUDKeys[0].enabled = true;
						// 移動してフェードアウト
						if (returnFlag == false)
						{
							if (demoCursor.transform.position.y >= demoCursorOldVec.y - 1)
							{
								demoCursor.transform.position -= new Vector3(0, 0.03f, 0);
							}
							else
							{
								//demoCursor.transform.position = new Vector3(demoCursorOldVec.x, demoCursorOldVec.y - 1, demoCursorOldVec.z);
								DesKeyFade(true);
							}
						}
						// 元に戻す
						else
						{
							DesKeyFade(false);
						}
						break;
					// 下に移動(逆方向)---------------------------------------------------------------------
					case false:
						demoUDKeys[1].enabled = true;
						// 移動してフェードアウト
						if (returnFlag == false)
						{
							if (demoCursor.transform.position.y <= demoCursorOldVec.y + 1)
							{
								demoCursor.transform.position += new Vector3(0, 0.03f, 0);
							}
							else
							{
								//demoCursor.transform.position = new Vector3(demoCursorOldVec.x, demoCursorOldVec.y + 1, demoCursorOldVec.z);
								DesKeyFade(true);
							}
						}
						// 元に戻す
						else
						{
							DesKeyFade(false);
						}
						break;
				}
			}
		}
	}

	// カーソルのフェードアウトとイン--------------------------------------------------------------------------------------------
	private void DesKeyFade(bool Flag)
	{
		// カーソルのフェードアウト
		if (Flag == true)
		{
			demoCursor.color -= new Color(0, 0, 0, 0.05f);
			if (demoCursor.color.a <= 0)
			{
				demoCursor.transform.position = demoCursorOldVec;
				returnFlag = true;
				++nextCnt;
				// キーの変化
				if (nextCnt > 1)
				{
					if (nowKey == true)
					{
						nowKey = false;
					}
					else
					{
						nowKey = true;
					}
					nextCnt = 0;
				}
			}
		}
		// カーソルのフェードイン
		else
		{
			demoCursor.color += new Color(0, 0, 0, 0.05f);
			if (demoCursor.color.a >= 1)
			{
				returnFlag = false;
			}
		}
	}

	// 値の（別シーン間での）受け渡し
	public static bool getFlipUpDown()
	{
		return FlipUpDown;
	}
	public static bool getFilpHorizontal()
	{
		return FlipHorizontal;
	}
}

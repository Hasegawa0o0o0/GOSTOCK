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
 *		2019.02.07 [Re]設定の変更に伴い修正
---------------------------------------------------------------------------------------------------*/
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using System;

public class Setting_old : MonoBehaviour
{
	// 変数宣言----------------------------------------------------------------
	public GameObject Cursor;
	public Text InformationText;
	//public Text VolText;
	// [00]LT [01]RT [02]LB [03]RB [04]左スティック [05]Y [06]X [07]B [08]十字キー [09]右スティック [10]A
	//public GameObject[] Ctrl = new GameObject[11];
	// [00]移動 [01]カメラ切り替えL [02]カメラ切り替えR [03]写真撮影 [04]発射
	//private string[] CtrlName = new string[5] { "左スティック", "LT", "RT", "RB", "RB" };
	//private bool MoveLR = true;						// 移動スティック true:左、false:右]
	//private int CameraR = 1;							// カメラ右回り、初期設定RTボタン(Ctrl変数と連動)
	//private int CameraL = 0;							// カメラ左回り、初期設定LTボタン(Ctrl変数と連動)
	//private int vol = 50;								// Vol変数(これで音量をいじるつもり)
	//private KeyCode getKeyCode;						// キーコード取得用変数
	public static bool FlipUpDown = false;				// 上下反転、true:反転あり、false:反転なし
	public static bool FlipHorizontal = false;			// 左右反転、true:反転あり、false:反転なし
	public int Item = 0;								// 現在の選択項目(超重要)
	private bool LR = true;								// true:左側の項目、false:右側の項目
	private bool AcceptanceFlag = false;				// 入力受付(選択中かどうか)
	public float[] RGB = new float[3];					// カーソルのGameObject用RGB値(0.0～1.0)
	private float UDTrigger;							// スティック対する連続押しを解消するための変数
	private float LRTrigger;
	private float LRTriggerV2;							// 上下反転の左右キー用
	private float LRTriggerV3;							// 左右反転の左右キー用
	private bool ResetFlag = false;						// リセットされたかどうか

	void Start()
	{
		//// 初期設定
		//for (int i = 0; i < 11; ++i)
		//{
		//	Ctrl[i].SetActive(false);
		//}
		// カーソルの位置を初期化
		Cursor.transform.position = new Vector2(-5.83f, 3.89f);
	}

	void Update()
	{
		if (AcceptanceFlag == false)
		{
			if (Item >= 0 && Item < 3)
			{
				float UDXbox = Input.GetAxis("CrossY");
				if (Input.GetKeyDown(KeyCode.DownArrow) || (UDXbox < 0 && UDTrigger == 0.0f))
				{
					++Item;
					if (Item > 2) { if (LR == true) Item = 0; else Item = 1; }
					CursorPosition(Item);
				}
				if (Input.GetKeyDown(KeyCode.UpArrow) || (UDXbox > 0 && UDTrigger == 0.0f))
				{
					--Item;
					if (LR == true && Item < 0) { Item = 2; }
					else if (LR == false && Item < 1) { Item = 2; }
					CursorPosition(Item);
				}
				UDTrigger = UDXbox;
			}
			float LRXbox = Input.GetAxis("CrossX");
			if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || ((LRXbox > 0 || LRXbox < 0) && LRTrigger == 0.0f))
			{
				if (LR == true) { LR = false; } else { LR = true; }
				if (LR == false && Item == 0) { Item = 1; }
				CursorPosition(Item);
			}
			LRTrigger = LRXbox;
			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1)) { AcceptanceFlag = true; }
		}
		else
		{
			// 左側
			if (LR == true)
			{
				switch (Item)
				{
					/* ---------------------------------------------------------------------------------------------------------------------------------------------------------------------
					case 0:				// 移動
						Cursor.GetComponent<SpriteRenderer>().color = new Color(RGB[0], RGB[1], RGB[2]);
						InformationText.text = "移動キーの設定ができます。\n←→で項目を切り替えられます。Backボタンで戻ります。\n現在のキーは " + CtrlName[0] + " です。";
						if (MoveLR == true) { CtrlName[0] = "左スティック"; Ctrl[4].SetActive(true); Ctrl[9].SetActive(false); }
						else { CtrlName[0] = "右スティック"; Ctrl[4].SetActive(false); Ctrl[9].SetActive(true); }
						if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
						{
							if (MoveLR == true) { CtrlName[0] = "左スティック"; Ctrl[4].SetActive(true); Ctrl[9].SetActive(false); }
							else { CtrlName[0] = "右スティック"; Ctrl[4].SetActive(false); Ctrl[9].SetActive(true); }
							if (MoveLR == true) { MoveLR = false; } else { MoveLR = true; }
						}
						InformationText.text = "移動キーの設定ができます。\n←→で項目を切り替えられます。Backボタンで戻ります。\n現在のキーは " + CtrlName[0] + " です。";
						if (Input.GetKeyDown(KeyCode.R))
						{
							AcceptanceFlag = false;
							Ctrl[4].SetActive(false); Ctrl[9].SetActive(false);
							InformationText.text = "移動キーの設定ができます。\n現在のキーは " + CtrlName[0] + " です。";
							Cursor.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
						}
						break;
					case 1:				// 反転
						Cursor.GetComponent<SpriteRenderer>().color = new Color(RGB[0], RGB[1], RGB[2]);
						if (InvertedFlag == false) { InformationText.text = "移動時の上下反転設定ができます。\n←→で項目を切り替えられます。Backボタンで戻ります。\n現在の設定は 反転なし です。"; }
						else { InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転あり です。"; }
						if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
						{
							if (InvertedFlag == true) { InvertedFlag = false; } else { InvertedFlag = true; }
						}
						if (InvertedFlag == false) { InformationText.text = "移動時の上下反転設定ができます。\n←→で項目を切り替えられます。Backボタンで戻ります。\n現在の設定は 反転なし です。"; }
						else { InformationText.text = "移動時の上下反転設定ができます。\n←→で項目を切り替えられます。Backボタンで戻ります。\n現在の設定は 反転あり です。"; }
						if (Input.GetKeyDown(KeyCode.R))
						{
							AcceptanceFlag = false;
							if (InvertedFlag == false) { InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転なし です。"; }
							else { InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転あり です。"; }
							Cursor.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
						}
						break;
					case 2:				// カメラ切り替え(未実装)
						Cursor.GetComponent<SpriteRenderer>().color = new Color(RGB[0], RGB[1], RGB[2]);
						InformationText.text = "カメラ切り替えのボタン設定ができます。\n←→で項目を切り替えられます。Backボタンで戻ります。\n現在の設定は 右回り"
							+ CtrlName[1] + " 、\n左回り" + CtrlName[2] + " です。";
						if (Input.GetKeyDown(KeyCode.R))
						{
							AcceptanceFlag = false;
							InformationText.text = "カメラ切り替えのボタン設定ができます。\n現在の設定は 右回り"
								+ CtrlName[1] + " 、\n左回り" + CtrlName[2] + " です。";
							Cursor.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
						}
						break;
					--------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
					case 0:				// 未使用
						Cursor.GetComponent<SpriteRenderer>().color = new Color(RGB[0], RGB[1], RGB[2]);
						InformationText.text = "空です。B(Space)で戻る";
						if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
						{
							AcceptanceFlag = false;
							InformationText.text = "空です。";
							Cursor.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
						}
						break;
					case 1:				// 上下反転
						Cursor.GetComponent<SpriteRenderer>().color = new Color(RGB[0], RGB[1], RGB[2]);
						if (FlipUpDown == false) { InformationText.text = "移動時の上下反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転なし です。"; }
						else { InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転あり です。"; }
						float LRXboxV2 = Input.GetAxis("CrossX");
						if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || ((LRXboxV2 > 0 || LRXboxV2 < 0) && LRTriggerV2 == 0.0f))
						{
							if (FlipUpDown == true) { FlipUpDown = false; } else { FlipUpDown = true; }
						}
						if (FlipUpDown == false) { InformationText.text = "移動時の上下反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転なし です。"; }
						else { InformationText.text = "移動時の上下反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転あり です。"; }
						if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
						{
							AcceptanceFlag = false;
							if (FlipUpDown == false) { InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転なし です。"; }
							else { InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転あり です。"; }
							Cursor.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
						}
						LRTriggerV2 = LRXboxV2;
						break;
					case 2:				// 左右反転
						Cursor.GetComponent<SpriteRenderer>().color = new Color(RGB[0], RGB[1], RGB[2]);
						if (FlipHorizontal == false) { InformationText.text = "移動時の左右反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転なし です。"; }
						else { InformationText.text = "移動時の左右反転設定ができます。\n現在の設定は 反転あり です。"; }
						float LRXboxV3 = Input.GetAxis("CrossX");
						if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || ((LRXboxV3 > 0 || LRXboxV3 < 0) && LRTriggerV3 == 0.0f))
						{
							if (FlipHorizontal == true) { FlipHorizontal = false; } else { FlipHorizontal = true; }
						}
						if (FlipHorizontal == false) { InformationText.text = "移動時の左右反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転なし です。"; }
						else { InformationText.text = "移動時の左右反転設定ができます。\n←→で項目を切り替え、B(Space)で戻ります。\n現在の設定は 反転あり です。"; }
						if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
						{
							AcceptanceFlag = false;
							if (FlipHorizontal == false) { InformationText.text = "移動時の左右反転設定ができます。\n現在の設定は 反転なし です。"; }
							else { InformationText.text = "移動時の左右反転設定ができます。\n現在の設定は 反転あり です。"; }
							Cursor.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
						}
						LRTriggerV3 = LRXboxV3;
						break;
				}
			}
			// 右側
			else if (LR == false)
			{
				switch (Item)
				{
					/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------
					case 0:				// 写真撮影
						Cursor.GetComponent<SpriteRenderer>().color = new Color(RGB[0], RGB[1], RGB[2]);
						Buttons();
						//InformationText.text = "1カメでの写真撮影ボタン設定ができます。\n設定したいキーを入力してください。\n現在の設定は " + CtrlName[3] + "です。";
						InformationText.text = "1カメでの写真撮影ボタン設定ができます。\n設定したいキーを入力してください。\n現在の設定は " + getKeyCode + "です。";
						if (Input.GetKeyDown(KeyCode.R))
						{
							AcceptanceFlag = false;
							//InformationText.text = "1カメでの写真撮影ボタン設定ができます。\n現在の設定は " + CtrlName[3] + "です。";
							InformationText.text = "1カメでの写真撮影ボタン設定ができます。\n現在の設定は " + getKeyCode + "です。";
							Cursor.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
						} break;
					case 1:				// 発射
						Cursor.GetComponent<SpriteRenderer>().color = new Color(RGB[0], RGB[1], RGB[2]);
						Buttons();
						//InformationText.text = "1・2カメでの発射ボタン設定ができます。\n設定したいキーを入力してください。\n現在の設定は " + CtrlName[4] + "です。";
						InformationText.text = "1・2カメでの写真撮影ボタン設定ができます。\n設定したいキーを入力してください。\n現在の設定は " + getKeyCode + "です。";
						if (Input.GetKeyDown(KeyCode.R))
						{
							AcceptanceFlag = false;
							//InformationText.text = "1・2カメでの発射ボタン設定ができます。\n現在の設定は " + CtrlName[4] + "です。";
							InformationText.text = "1・2カメでの写真撮影ボタン設定ができます。\n現在の設定は " + getKeyCode + "です。";
							Cursor.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
						} break;
					case 2:				// Vol
						Cursor.GetComponent<SpriteRenderer>().color = new Color(RGB[0], RGB[1], RGB[2]);
						InformationText.text = "ゲーム全体のボリューム設定ができます。\n←→で音量を変えられます。Backボタンで戻ります。";
						if (Input.GetKey(KeyCode.LeftArrow))
						{
							if (vol <= 100) { --vol; if (vol < 0) vol = 0; }
							VolText.text = vol.ToString();
						}
						if (Input.GetKey(KeyCode.RightArrow))
						{
							if (vol >= 0) { ++vol; if (vol > 100) vol = 100; }
							VolText.text = vol.ToString();
						}
						if (Input.GetKeyDown(KeyCode.R))
						{
							AcceptanceFlag = false;
							InformationText.text = "ゲーム全体のボリューム設定ができます。";
							Cursor.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
						} break;
					---------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
					case 1:				// 初期化
						Cursor.GetComponent<SpriteRenderer>().color = new Color(RGB[0], RGB[1], RGB[2]);
						if (ResetFlag == true)
						{
							InformationText.text = "初期化しました。\nBack(R)を押すと戻ります。";
						}
						else
						{
							InformationText.text = "設定を初期化します。\nB(Spece)をもう一度押すと初期化されます。\n戻る場合はBack(R)を押してください。";
						}
						if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
						{
							// Reset
							FlipUpDown = false;
							FlipHorizontal = false;
							ResetFlag = true;
						}
						if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Joystick1Button6))
						{
							AcceptanceFlag = false;
							ResetFlag = false;
							Cursor.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
						}
						break;
					case 2:				// タイトルへ(仮置きでmainへ)(セレクトに戻るように変更 2018.07.18)
						SceneManager.LoadScene("select");
						break;
				}
			}
		}
	}
	void CursorPosition(int item)
	{
		// 左側
		if (LR == true)
		{
			switch (item)
			{
				/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------
				case 0:				// 移動
					Cursor.transform.position = new Vector2(-5.83f, 3.89f);
					InformationText.text = "移動キーの設定ができます。\n現在のキーは " + CtrlName[0] + " です。"; break;
				case 1:				// 反転
					Cursor.transform.position = new Vector2(-5.83f, 2.58f);
					if (InvertedFlag == false) { InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転なし です。"; }
					else { InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転あり です。"; } break;
				case 2:				// カメラ切り替え
					Cursor.transform.position = new Vector2(-5.83f, 1.2f);
					InformationText.text = "カメラ切り替えのボタン設定ができます。\n現在の設定は 右回り"
						+ CtrlName[1] + " 、\n左回り" + CtrlName[2] + " です。"; break;
				---------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
				case 0:				// 未使用
					Cursor.transform.position = new Vector2(-5.83f, 3.89f);
					InformationText.text = "空です。"; break;
				case 1:				// 上下反転
					Cursor.transform.position = new Vector2(-5.83f, 2.58f);
					if (FlipUpDown == false) { InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転なし です。"; }
					else { InformationText.text = "移動時の上下反転設定ができます。\n現在の設定は 反転あり です。"; } break;
				case 2:				// 左右反転
					Cursor.transform.position = new Vector2(-5.83f, 1.2f);
					if (FlipHorizontal == false) { InformationText.text = "移動時の左右反転設定ができます。\n現在の設定は 反転なし です。"; }
					else { InformationText.text = "移動時の左右反転設定ができます。\n現在の設定は 反転あり です。"; } break;
			}
		}
		// 右側
		else if (LR == false)
		{
			switch (item)
			{
				/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------
				case 0:				// 写真撮影
					Cursor.transform.position = new Vector2(-0.09f, 3.89f);
					InformationText.text = "1カメでの写真撮影ボタン設定ができます。\n現在の設定は " + CtrlName[3] + "です。"; break;
				case 1:				// 発射
					Cursor.transform.position = new Vector2(-0.09f, 2.58f);
					InformationText.text = "1・2カメでの発射ボタン設定ができます。\n現在の設定は " + CtrlName[4] + "です。"; break;
				case 2:				// Vol
					Cursor.transform.position = new Vector2(-0.09f, 1.2f);
					InformationText.text = "ゲーム全体のボリューム設定ができます。"; break;
				---------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
				case 1:				// 初期化
					Cursor.transform.position = new Vector2(-0.09f, 2.58f);
					InformationText.text = "全ての設定項目をリセットします。"; break;
				case 2:				// タイトルへ
					Cursor.transform.position = new Vector2(-0.09f, 1.2f);
					//InformationText.text = "設定を保存してタイトル(ゲーム)へ戻ります。"; break;
					InformationText.text = "設定を保存してセレクトへ戻ります。"; break;
			}
		}
	}
	//// 入力されたキーの取得(キーボード)
	//void Buttons()
	//{
	//	if (Input.anyKeyDown)
	//	{
	//		foreach(KeyCode code in Enum.GetValues(typeof(KeyCode)))
	//		{
	//			if (Input.GetKeyDown(code))
	//			{
	//				if (code.ToString() == "R" || code.ToString() == "Joystick1Button6") { break; }
	//				getKeyCode = code;
	//				break;
	//			}
	//		}
	//	}
	//}

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

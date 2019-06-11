/*---------------------------------------------------------------------------------------------------
// ポーズ画面(main用)
//
* FileName		: Pause.cpp
* Author		: 17CU0116 杉田優帆
* Log			: 2018.07.12 作成
*				: 2018.07.13 全体を止める為、下記スクリプトを変更
*				:			(EnemyAction/Scroll/Scroll02/CameraMaster/CTMaster/GhostSpowner)
*				: 2019.02.25 [Re] 値の公開範囲の変更
---------------------------------------------------------------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
	// 変数宣言
	public bool ON = false;							// 2019.02.26 ポーズ自体のオンオフ
	public Camera UICamera;
	public GameObject pauseUI;
	public GameObject Cursor;
	public Text FlipUD;
	public Text FlipLR;
	public bool pauseFlag = false;					// ポーズ中(true:ポーズ中)
	private bool AcceptanceFlag = false;			// 操作受付
	private int Item = 0;
	private float beforeTrigger;
	private float beforeTriggerV2;

	void Start()
	{
		pauseUI.SetActive(false);
		Cursor.transform.localPosition = new Vector2(-10.0f, 86.0f);
		// ロードしたときに変更をかける
		if (Setting.FlipUpDown == true)
		{
			FlipUD.text = "上下反転　あり";
		}
		else
		{
			FlipUD.text = "上下反転　なし";
		}
		if (Setting.FlipHorizontal == true)
		{
			FlipLR.text = "左右反転　あり";
		}
		else
		{
			FlipLR.text = "左右反転　なし";
		}
	}

	void Update()
	{
		if (ON == true)
		{
			// 入力受付がないとき
			if (AcceptanceFlag == false)
			{
				// ポーズ
				if (pauseFlag == false)
				{
					if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Joystick1Button6))
					{
						AcceptanceFlag = true;
						pauseUI.SetActive(true);
						//UICamera.depth = 2;
						Time.timeScale = 0.0f;
					}
				}
				// ポーズ終了
				else
				{
					pauseFlag = false;
					pauseUI.SetActive(false);
					//UICamera.depth = 0;
					Time.timeScale = 1.0f;
				}
			}
			// 入力受付中
			else
			{
				float UDXbox = Input.GetAxis("CrossY");
				float UDXboxV2 = Input.GetAxis("Vertical");
				if (Input.GetKeyDown(KeyCode.DownArrow) || (UDXbox < 0 && beforeTrigger == 0.0f) || (UDXboxV2 < 0 && beforeTriggerV2 == 0.0f))
				{
					++Item;
					if (Item > 1) { Item = 0; }
					CursorPosition(Item);
				}
				if (Input.GetKeyDown(KeyCode.UpArrow) || (UDXbox > 0 && beforeTrigger == 0.0f) || (UDXboxV2 < 0 && beforeTriggerV2 == 0.0f))
				{
					--Item;
					if (Item < 0) { Item = 1; }
					CursorPosition(Item);
				}
				beforeTrigger = UDXbox;
				beforeTriggerV2 = UDXboxV2;

				switch (Item)
				{
					case 0:             // 上下反転
						if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
						{
							if (Setting.FlipUpDown == false)
							{
								Setting.FlipUpDown = true; FlipUD.text = "上下反転　あり";
							}
							else
							{
								Setting.FlipUpDown = false; FlipUD.text = "上下反転　なし";
							}
						}
						break;
					case 1:             // 左右反転
						if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
						{
							if (Setting.FlipHorizontal == false)
							{
								Setting.FlipHorizontal = true; FlipLR.text = "左右反転　あり";
							}
							else
							{
								Setting.FlipHorizontal = false; FlipLR.text = "左右反転　なし";
							}
						}
						break;
				}
				// ポーズ終了
				if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Joystick1Button6))
				{
					AcceptanceFlag = false;
					pauseFlag = true;
				}
			}
		}
	}

	void CursorPosition(int item)
	{
		switch (item)
		{
			case 0:				// 上下反転
				Cursor.transform.localPosition = new Vector2(-10.0f, 86.0f);
				break;
			case 1:				// 左右反転
				Cursor.transform.localPosition = new Vector2(-10.0f, -30.0f);
				break;
		}
	}
}

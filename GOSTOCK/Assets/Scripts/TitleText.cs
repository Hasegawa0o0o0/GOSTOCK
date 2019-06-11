/*---------------------------------------------------------------------------------------------------
// タイトル画面演出
// TextCmdの統合用、Title.csから移行
//
* FileName		: TitleText.cpp
* Author		: 17CU0116 杉田優帆
* Log			: 2018.05.29 作成
*				: 2018.12.28 メインが選択された時の行き先を変更
*				: 2019.01.13 調整
---------------------------------------------------------------------------------------------------*/
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleText : MonoBehaviour
{
	public bool isPlayText;
	public bool isSettingText;
	public bool isTutorialText;

	public bool decision = false;	// 決定されたかどうか
	int changeFrame;                // 決定されてから次のシーンに行くまでのカウント
	const int changeFrameMax = 120; // 決定されてから次のシーンに行くまで

	float downSpeed = 1.25f;
	float decisionDownSpeed = 1.25f;
	// 2019.01.13
	float shake;                    // 縦にどれくらい動いたか
	float addShake = 0.005f;       // 揺れる速さ

	// unity側設定
	public RectTransform rectTransform1;	// プレイ
	public RectTransform rectTransform2;	// セッティング
	public RectTransform rectTransform3;    // チュートリアル
	public Text playText;					// プレイをポイントしたときのテキスト
	public Text settingText;				// セッティングをポイントしたときのテキスト
	public Text tutorialText;               // チュートリアルをポイントしたときのテキスト
	public Sprite bossHandOpen;				// 開いているボスの手 2019.01.13
	public Sprite bossHandClose;				// 閉じているボスの手 2019.01.13

	Image playIcon;							// プレイアイコン
	Image settingIcon;                      // セッティングアイコン
	Image tutorialIcon;						// チュートリアルアイコン

	void Start()
	{
		playText.enabled = false;
		settingText.enabled = false;
		tutorialText.enabled = false;
		playIcon = rectTransform1.GetComponent<Image>();
		settingIcon = rectTransform2.GetComponent<Image>();
		tutorialIcon = rectTransform3.GetComponent<Image>();
	}

	void Update()
	{
		// 揺らす
		shake -= addShake;
		if (Mathf.Abs(shake) > 0.1f) { addShake *= -1; }
		// 選択
		// プレイ
		if (isPlayText == true)
		{
			//// 少し大きくする
			//rectTransform1.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			// テキスト表示
			playText.enabled = true;
			// ボスの手を開く 2019.01.13
			playIcon.sprite = bossHandOpen;
			// 決定された時の処理
			if (decision)
			{
				if (changeFrame > changeFrameMax)
				{
					// 行き先を変更 2018.12.28
					SceneManager.LoadScene("mainProduction");
				}
				else
				{
					++changeFrame;
					// 半分を過ぎたら消していく
					if (changeFrame > changeFrameMax / 2)
					{
						playIcon.color -= new Color(0, 0, 0, 0.035f);
					}
					// 跳ねる
					rectTransform1.position += new Vector3(0, decisionDownSpeed, 0);
					if (decisionDownSpeed < -1.25f)
					{
						decisionDownSpeed *= -1.05f;
					}
					decisionDownSpeed -= 0.075f;
				}
			}
			// 決定されていないときの処理
			else if (Input.GetKeyDown(KeyCode.Space))
			{
				decision = true;
			}
			// 揺らす 2019.01.13
			else
			{
				rectTransform1.position += new Vector3(0f, shake, 0f);
			}
		}
		else
		{
			//// 元の大きさに戻す
			//rectTransform1.localScale = new Vector3(1, 1, 1);
			// テキスト非表示
			playText.enabled = false;
			// ボスの手を閉じる 2019.01.13
			playIcon.sprite = bossHandClose;
			// 元の位置に戻す 2019.01.13
			playIcon.transform.localPosition = new Vector3(-378.4f, -107.1f, 0f);
			// 選択→決定がされなかったら、落とす
			if (decision)
			{
				rectTransform1.position += new Vector3(0, downSpeed, 0);
			}
		}
		// セッティング
		if (isSettingText == true)
		{
			//// 少し大きくする
			//rectTransform2.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			// テキスト表示
			settingText.enabled = true;
			// ボスの手を開く 2019.01.13
			settingIcon.sprite = bossHandOpen;
			// 決定された時の処理
			if (decision)
			{
				if (changeFrame > changeFrameMax)
				{
					SceneManager.LoadScene("setting");
				}
				else
				{
					++changeFrame;
					// 半分を過ぎたら消していく
					if (changeFrame > changeFrameMax / 2)
					{
						settingIcon.color -= new Color(0, 0, 0, 0.035f);
					}
					// 跳ねる
					rectTransform2.position += new Vector3(0, decisionDownSpeed, 0);
					if (decisionDownSpeed < -1.25f)
					{
						decisionDownSpeed *= -1.05f;
					}
					decisionDownSpeed -= 0.075f;
				}
			}
			// 決定されていないときの処理
			else if (Input.GetKeyDown(KeyCode.Space))
			{
				decision = true;
			}
			// 揺らす 2019.01.13
			else
			{
				rectTransform2.position += new Vector3(0f, shake, 0f);
			}
		}
		else
		{
			//// 元の大きさに戻す
			//rectTransform2.localScale = new Vector3(1, 1, 1);
			// テキスト非表示
			settingText.enabled = false;
			// ボスの手を閉じる 2019.01.13
			settingIcon.sprite = bossHandClose;
			// 元の位置に戻す 2019.01.13
			settingIcon.transform.localPosition = new Vector3(378.4f, -107.1f, 0f);
			// 選択→決定がされなかったら、落とす
			if (decision)
			{
				rectTransform2.position += new Vector3(0, downSpeed, 0);
			}
		}
		// チュートリアル
		if (isTutorialText == true)
		{
			// 少し大きくする
			rectTransform3.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			// テキスト表示
			tutorialText.enabled = true;
			// 決定された時の処理
			if (decision)
			{
				if (changeFrame > changeFrameMax)
				{
					SceneManager.LoadScene("tutorial");
				}
				else
				{
					++changeFrame;
					// 半分を過ぎたら消していく
					if (changeFrame > changeFrameMax / 2)
					{
						tutorialIcon.color -= new Color(0, 0, 0, 0.035f);
					}
					// 跳ねる
					rectTransform3.position += new Vector3(0, decisionDownSpeed, 0);
					if (decisionDownSpeed < -1.25f)
					{
						decisionDownSpeed *= -1.05f;
					}
					decisionDownSpeed -= 0.075f;
				}
			}
			// 決定されていないときの処理
			else if (Input.GetKeyDown(KeyCode.Space))
			{
				decision = true;
			}
		}
		else
		{
			// 元の大きさに戻す
			rectTransform3.localScale = new Vector3(1, 1, 1);
			// テキスト非表示
			tutorialText.enabled = false;
			// 選択→決定がされなかったら、落とす
			if (decision)
			{
				rectTransform3.position += new Vector3(0, downSpeed, 0);
			}
		}
		// いずれかの選択肢が決定されていた時
		if (decision)
		{
			// downSpeedを減らす
			downSpeed -= 0.075f;
			// テキストを非表示
			playText.color -= new Color(0, 0, 0, 0.01f);
			settingText.color -= new Color(0, 0, 0, 0.01f);
			tutorialText.color -= new Color(0, 0, 0, 0.01f);
		}
	}
}

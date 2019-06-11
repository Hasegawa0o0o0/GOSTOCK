/* -----------------------------------------------------------------------------------------
 * Ending.cs
 * 2018.12.15 作成
 *		2018.12.17 エンドロールの追加
 *		2018.12.23 動きの追加
 *		2018.12.25 動きの修正
 *		2019.01.30 ホールを表示させる前にフェードインを追加
 *		2019.02.07 紙吹雪に合わせてキャンバスを弄ったので、プログラムの修正
 *		2019.02.20 動きの修正、音楽の追加
 *		2019.02.21 BGM修正
 *		2019.04.03 デバックを追加
 ----------------------------------------------------------------------------------------- */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Ending : MonoBehaviour
{
	// カメラ(メイン制御系)
	public Camera edCamera;					// エンディング用のカメラ
	private int delayTime = 0;				// ディレイ用
	private int phase = 0;					// 段階(フェーズ)の管理用(0は未使用?1から使用)
	private bool one = true;				// 一回きりの判断用

	// ドア関連
	public GameObject gizmoL;				// ドア操作用のギズモ、Lは+、Rは-で開く
	public GameObject gizmoR;
	public int openTime;					// ドアが開ききるまでの時間
	private int openFrame;					// フレーム換算
	private float doorMove;					// 1フレームごとの回転、0~120°までが理想？
	private bool openFlag = false;			// ドアの判断

	// ホール
	public Image fadeDark;					// シーンをフェードイン、アウトさせるのに使用	'19.01.30
	//public SpriteRenderer hallDarkSR;		// ↑のSpriteRenderer							'19.01.30
	public GameObject hallObj;				// コンサートホール(表示、非表示を変化させる為)
	public Image hallImgsUI;				// オバケなどのイメージ(UI)

	// エンドロール関連
	public GameObject endrolImg;			// エンドロール(スタッフロール)、マスク
	public Text endrolText;					// エンドロール(スタッフロール)、テキスト
	public float scrollSped;				// 流れる速さ
	private Vector3 OldVec;					// エンドロールの元の位置
	private bool scrollFlag = false;		// スクロールの判断
	private bool titleBack = false;			// タイトルに戻る
	public SpriteRenderer[] hallDarkness;	// ホールを段々と暗くするために使用(スポットライトとセットで使用)
	public Image[] lights;					// スポットライト
	public GameObject balloon;				// オバケの吹き出し
	public SpriteRenderer balloonSR;		// ↑のSR
	public GameObject[] balloonSerifs;		// オバケのセリフ
	public SpriteRenderer[] serifsSR;		// ↑のSR(長いのでBalloonを省略)
	private bool fadeSerif = false;			// セリフを点滅させるか判断(長いのでBalloonを省略)
	private bool flashing = false;			// セリフを付けたり消したりするフラグ(長いのでBalloonを省略)

	// パーティクル関係
	public ParticleSystem confetti;			// 動かしたいパーティクル
	private bool oneV1 = true;

	// BGM
	public AudioSource bgm;
	public TitleBGM titleBGM;				// タイトルに戻った時にBGMを再生


	void Start ()
	{
		// カメラを初期位置へ
		edCamera.transform.position = new Vector3(0, -2, -10);
		// ホールとフェードイン用ダークを隠す	'19.01.30
		//hallDark.SetActive(false);
		fadeDark.enabled = false;
		hallObj.SetActive(false);
		// ドア関連
		openFrame = openTime * 60;
		doorMove = 120.0f / (float)openFrame;
		// エンドロール関連
		OldVec = endrolText.transform.position;
		endrolImg.SetActive(false);
		for (int i = 0; i < 2; ++i)
		{
			lights[i].enabled = false;
		}
		balloon.SetActive(false);
		// パーティクルの再生と停止
		confetti.Stop();
		// 動かす
		phase = 1;
		// 音を消しておく
		bgm.volume = 0;
		// タイトルBGMのスクリプトを取得
		titleBGM = FindObjectOfType<TitleBGM>();
	}
	
	void Update ()
	{
		// デバック用
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene("re_title");
			titleBGM.rePlay = true;
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			SceneManager.LoadScene("clear");
		}

		// 音楽再生(フェードイン)
		if (bgm.volume < 1)
		{
			bgm.volume += 0.005f;
		}
		else
		{
			bgm.volume = 1;
		}

		// メイン系
		if (phase == 1)				// 開始位置からドアまで
		{
			// 徐々に進むように
			edCamera.transform.position += new Vector3(0, 0, 0.1f);
			if (edCamera.transform.position.z >= -9)
			{
				edCamera.transform.position = new Vector3(0, -2, -9);
				phase = 2;
			}
		}
		else if (phase == 2)		// ドアを開く→進みながら電気点灯(ホールを表示)
		{
			openFlag = true;
			if (edCamera.transform.position.z < 9)
			{
				edCamera.transform.position += new Vector3(0, 0, 0.1f);
			}
			else
			{
				edCamera.transform.position = new Vector3(0, -2, 9);
				hallObj.SetActive(true);
				fadeDark.enabled = true;
				fadeDark.color -= new Color(0, 0, 0, 0.1f);
				if (fadeDark.color.a <= 0)
				{
					phase = 4;
				}
			}
		}
		else if (phase == 3)		// 電気をゆっくり点灯(ホールを表示)
		{
			if (edCamera.transform.position.z < 9)
			{
				edCamera.transform.position += new Vector3(0, 0, 0.1f);
			}
			else
			{
				edCamera.transform.position = new Vector3(0, -2, 9);
				hallObj.SetActive(true);						// 2個かかないとうまくいかないマン
				fadeDark.enabled = true;
				fadeDark.color -= new Color(0, 0, 0, 0.01f);
			}
			if (fadeDark.color.a <= 0)
			{
				phase = 4;
			}
		}
		else if (phase == 4)		// シアターが見える位置まで移動
		{
			// 徐々に進むように
			edCamera.transform.position += new Vector3(0, 0, 0.1f);
			if (edCamera.transform.position.z >= 25)
			{
				edCamera.transform.position = new Vector3(0, -2, 25);
				phase = 5;
			}
		}
		else if (phase == 5)		// スタッフロール
		{
			endrolImg.SetActive(true);
			if (one == true)
			{
				balloonSR.color = new Color(balloonSR.color.r, balloonSR.color.g, balloonSR.color.b, 0);
				balloon.SetActive(true);
				one = false;
			}
			else
			{
				if (balloonSR.color.a < 1)
				{
					balloonSR.color += new Color(0, 0, 0, 0.03f);
					serifsSR[0].color += new Color(0, 0, 0, 0.03f);
					serifsSR[1].color += new Color(0, 0, 0, 0.03f);
				}
			}
			if (hallDarkness[0].color.r >= 0.369)
			{
				for (int i = 0; i < 3; ++i)
				{
					hallDarkness[i].color -= new Color(0.05f, 0.05f, 0.05f, 0);
				}
			}
			else
			{
				++delayTime;
				if (delayTime >= 50)
				{
					for (int i = 0; i < 2; ++i)
					{
						lights[i].enabled = true;
					}
					if (oneV1 == true)
					{
						confetti.Play();
						oneV1 = false;
					}
				}
				if (delayTime >= 70)
				{
					scrollFlag = true;
				}
			}
		}
		else if (phase == 6)		// シーン移行→キーが押されたときに移動するよう変更
		{
			for (int i = 0; i < 3; ++i)
			{
				if(i != 2)
				{ 
					serifsSR[i].color -= new Color(0, 0, 0, 0.03f);
				}
				if (i != 2 && serifsSR[i].color.a <= 0 && fadeSerif == false)
				{
					serifsSR[2].color += new Color(0, 0, 0, 0.03f);
					if (serifsSR[2].color.a >= 1)
					{
						fadeSerif = true;
					}
				}
			}
			if (Input.anyKeyDown)
			{
				confetti.Stop();
				confetti.gameObject.SetActive(false);
				balloon.SetActive(false);
				titleBack = true;
			}
		}

		// Pressの文字を点滅させる
		if (fadeSerif == true)
		{
			if (flashing == true)
			{
				serifsSR[2].color += new Color(0, 0, 0, 0.02f);
				if (serifsSR[2].color.a >= 1)
				{
					flashing = false;
				}
			}
			else
			{
				serifsSR[2].color -= new Color(0, 0, 0, 0.02f);
				if (serifsSR[2].color.a <= 0)
				{
					flashing = true;
				}
			}
		}

		// ドアを開く(開ききったらフェーズ3へ移行)
		if (openFlag == true)
		{
			gizmoL.transform.Rotate(new Vector3(0, doorMove, 0));
			gizmoR.transform.Rotate(new Vector3(0, -doorMove, 0));
			if (gizmoL.transform.localEulerAngles.y >= 120 || gizmoR.transform.localEulerAngles.y <= -120)
			{
				openFlag = false;
				phase = 3;
			}
		}

		// スクロール開始
		if (scrollFlag == true)
		{
			if (endrolText.rectTransform.position.y <= 115)
			{
				endrolText.transform.position += new Vector3(0, scrollSped, 0);
			}
			else
			{
				scrollFlag = false;
				phase = 6;
			}
		}

		// タイトルへ戻るとき
		if (titleBack == true)
		{
			for (int i = 0; i < 2; ++i)
			{
				lights[i].enabled = false;
			}
			fadeDark.color += new Color(0, 0, 0, 0.01f);
			bgm.volume -= 1f / 60f;
			if (fadeDark.color.a >= 1)
			{
				SceneManager.LoadScene("re_title");
				titleBGM.rePlay = true;
			}
		}
	}
}

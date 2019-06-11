/* -----------------------------------------------------------------------------------------
 * Clear.cs
 * 2018.00.00 作成
 *		2018.06.30 オーディオソースを追加
 *		----------以下[Re]更新分-------------------------------
 *		2018.11.14 クリア画面の変更
 *		2018.11.15 色々編集、値を受けとり表示
 *		2018.11.18 ランクの表示(仮なのでテキスト)
 *		2018.12.24 ディレイの追加、演出を通す
 *		2019.01.02 蝋はんこ(ランク込みのもの)の追加
 *		2019.01.03 スクリプトの整理、使わないもの(使ってないもの)をコメント化
 *		2019.01.31 スコアの変化に伴い変更
 *		2019.02.21 ボスを倒していないときはフェードアウトしてタイトルへ
 *		2019.03.03 BGMのタイミングの変更
 *		2019.03.06 負けた時の動きの追加
 *		2019.03.07 ↑の動きの修正
 *		2019.04.02 BGMの修正
 ----------------------------------------------------------------------------------------- */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Clear : MonoBehaviour
{
	/* ---------------------------------------------------------------------------------------------
	public Text scoreText;				// unity側の設定
	//public Text rank;					// 同上
	public Image rank;					// 同上
	public Sprite d;					// 同上
	public Sprite c;					// 同上
	public Sprite b;					// 同上
	public Sprite a;					// 同上
	public Sprite s;					// 同上
	// リザルト用------------------------------------------
	public Image[] RESULT = new Image[6];
	int resultFrame = 0;
	int resultCut = 0;
	public Image[] RESoul = new Image[8];
	int soulFrame = 0;
	int SoulRand01 = 0;								// 乱数で魂のジャンプを操作
	int SoulRand02 = 0;
	public AudioClip[] cheer = new AudioClip[3];	// 歓声オーディオソース
	public AudioClip[] audios = new AudioClip[2];	// 0BGM,1SE
	int CheerCTR = 0;								// 歓声管理用変数(0:S、1:AB、2:CD)
	bool once = false;								// 一度処理用
	public Image wobbling;							// ユラユラオバケ
	int WobblingFrame = 0;
	// ---------------------------------------------------
	public GameObject darkLightObj;					// スポットライトオブジェクト(同上)
	public SpriteRenderer title;					// タイトルのスプライトレンダラー(同上)
	SpriteRenderer darkSpriteRenderer;				// スポットライトオブジェクトのスプライトレンダラー
	int score = 0;
	int addScore;									// スコアによる結果発表の時間を同じにするときに使う
	bool isBack = false;
	int frame = 0;									// タイトルに戻る時に使う
	int forcedFrame = 0;							// 強制的に戻すのに使う
	const int frameMax = 200;
	int cntFrame = 0;								// ランクが出るまでの遅延
	private AudioSource[] audioSource = new AudioSource[3];     // 2018.06.30 K:配列に変更(0:BGM 1:bang 2:cheer)
	--------------------------------------------------------------------------------------------- */

	// [Re]
	private int[] points = new int[5];				// [0]撮影 [1]ノーダメージボーナス [2]命中率 [3]クリアタイム [4]撃破ボーナス
	public Text[] pointsText = new Text[5];			// 上に同じ
	//private Text[] guideText = new Text[5];			// 上に同じ(案内用)
	public Text totalText;							// 合計スコア
	private int pointsTotal = 0;					// 合計スコア
	public GameObject scoreMas;						// スコアをまとめたもの
	public GameObject waxStamp;						// 蝋はんこ
	public SpriteRenderer waxStampSR;				// 蝋はんこ用のSR
	public Sprite[] wSSprites = new Sprite[5];		// 蝋はんこのスプライト(wS=waxStamp) 0~4=S~D
	public GameObject letter;						// 手紙(上に行くようにするため)
	private int phase = 0;							// フェーズ、スコアを順番に表示していく為に
	public int nextDelay;							// 次の項目を表示するまでのディレイ
	private int nowTime = 0;						// ディレイ時間計測
	public Image darkImg;							// フェードアウト用Image

	// [Re]敵に敗北時アニメーションさせる様
	private bool loseFlag = false;					// 負けたフラグ(true:敗北、false:勝利)
	public GameObject loseLetter;					// 負けた時に使う手紙
	public SpriteRenderer[] loseLetterSR;			// ↑のSR
	public GameObject[] loseZakos;					// 負けた時に使うザコ敵
	public SpriteRenderer[] loseZakosSR;			// ↑のSR
	private bool letterFlag = false;				// 手紙が全部表示しきったか
	private bool zakoFlag = false;					// ザコが全部表示しきったか
	private bool moveFlag = false;					// ゆらゆらさせていいかどうかのフラグ
	private bool swayingFlag = false;				// ザコ敵達をゆらゆらさせるフラグ
	private bool skipFlag = false;					// スキップされたか確認するフラグ

	// 音楽系
	public AudioSource bgm;							// BGM
	public AudioSource se;							// SE
	public AudioClip[] audioClips;					// SEの種類(大→中→小)
	public AudioClip ghostSE;						// オバケの笑い声

	public TitleBGM titleBGM;						// タイトルに戻った時にBGMを再生
	public MainBGM mainBGM;							// ゲームシーンに戻った時にBGMを再生 2019.03.19
	public AudioClip mainBGMClip;					// ↑のBGMClip、もう一回再生する時に変更されるように

	void Start ()
	{
		/* ---------------------------------------------------------------------------------------------
		rank.enabled = false;
		//// addScoreの値を決める
		//addScore = GameMaster.recovery / 10;
		//// addScoreが0未満にならないようにする
		//if (addScore == 0)
		//{
		//	addScore = 1;
		//}
		darkSpriteRenderer = darkLightObj.GetComponent<SpriteRenderer>();
		darkSpriteRenderer.color = new Color(1, 1, 1, 0);
		// タイトルを非表示
		title.color = new Color(1, 1, 1, 0);
		audioSource = GetComponents<AudioSource>();
		audioSource[0].clip = audios[0];
		audioSource[0].Play();
		--------------------------------------------------------------------------------------------- */
		// [Re]
		points = Score.GetPoints();				// 値をScoreから受け取る
		for (int i = 0; i < 5; ++i)
		{
			pointsTotal += (int)points[i];		// 合計スコアの計算
			pointsText[i].text = points[i].ToString();
			if (i == 4)
			{
				TotalDIsplay(pointsTotal);
				phase = 1;
			}
			pointsText[i].enabled = false;
		}
		waxStamp.SetActive(false);
		darkImg.color = new Color(darkImg.color.r, darkImg.color.g, darkImg.color.b, 0);
		// 勝敗の判断
		if (points[4] == 0)
		{
			loseFlag = true;
		}
		else
		{
			loseFlag = false;
			//for (int i = 0; i < loseLetterSR.LongLength; ++i)
			//{
			//	loseLetterSR[i].color = new Color(loseLetterSR[i].color.r, loseLetterSR[i].color.g, loseLetterSR[i].color.b, 0);
			//}
			//for (int i = 0; i < loseZakosSR.LongLength; ++i)
			//{
			//	loseZakosSR[i].color = new Color(loseZakosSR[i].color.r, loseZakosSR[i].color.g, loseZakosSR[i].color.b, 0);
			//}
		}
		// タイトルBGMとメイン(ゲームシーン)BGMのスクリプトを取得 2019.03.19
		titleBGM = FindObjectOfType<TitleBGM>();
		mainBGM = FindObjectOfType<MainBGM>();
		mainBGM.audioSource.clip = mainBGMClip;
	}
	
	void Update ()
	{
		// [Re]デバックキー
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			mainBGM.rePlay = true;				// 2019.03.19
			SceneManager.LoadScene("main");
		}

		// [Re]スペースキー(B)で表示スキップ
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
		{
			if (phase < 6)
			{
				skipFlag = true;
				//for(int i = 0; i < 5; i++)
				//{
				//	pointsText[i].enabled = true;
				//	bgm.volume -= 0.80f / 60f;
				//}
				////bgm.volume -= 0.80f / 60f;
				////if (bgm.volume == 0)
				////{
				//	bgm.Stop();
				//	phase = 6;
				////}
			}
		}

		// スキップ時の処理
		if (skipFlag == true)
		{
			bgm.volume -= 0.9f / 60f;
			for (int i = 0; i < 5; i++)
			{
				pointsText[i].enabled = true;
				
			}
			if (bgm.volume == 0)
			{
				bgm.Stop();
				skipFlag = false;
				phase = 6;
			}
		}

		// [Re]ランクを段々に表示→全部非表示→ハンコ(リザルト)
		++nowTime;
		if (nowTime >= nextDelay)
		{
			if (phase == 1)
			{
				pointsText[0].enabled = true;
				phase = 2;
			}
			else if (phase == 2)
			{
				pointsText[1].enabled = true;
				phase = 3;
			}
			else if (phase == 3)
			{
				pointsText[2].enabled = true;
				phase = 4;
			}
			else if (phase == 4)
			{
				pointsText[3].enabled = true;
				phase = 5;
			}
			else if (phase == 5)
			{
				pointsText[4].enabled = true;
				//phase = 6;
			}
			else if (phase == 6)
			{
				totalText.enabled = true;
				phase = 7;
			}
			else if (phase == 7)
			{
				// 時間稼ぎ？
				phase = 8;
			}
			else if (phase == 8)
			{
				se.Play();
				scoreMas.SetActive(false);
				waxStamp.SetActive(true);
				phase = 9;
			}
			else if (phase == 9)
			{
				// 時間稼ぎ？
				phase = 10;
			}
			// 10は↓
			else if (phase == 11)
			{
				// 時間稼ぎ？
				phase = 12;
			}
			else if (phase == 12)
			{
				if (loseFlag == false)
				{
					SceneManager.LoadScene("ending");
				}
				phase = 13;
			}
			else if (phase == 13)
			{
				phase = 14;
			}
			else if (phase == 14)
			{
				phase = 15;
			}
			nowTime = 0;
		}

		// 5の処理
		if (phase == 5)
		{
			bgm.volume -= 0.40f / 60f;
			if (bgm.volume == 0)
			{
				bgm.Stop();
				phase = 6;
			}
		}

		// 10の処理
		if (phase == 10)
		{
			letter.transform.position += new Vector3(0, 0.1f, 0);
			waxStamp.transform.position += new Vector3(0, 0.1f, 0);
			if (letter.transform.position.y >= 15)
			{
				phase = 11;
			}
		}

		// 14の処理(敗北時限定)
		if (phase == 15)
		{
			if (loseFlag == true)
			{
				darkImg.color += new Color(0, 0, 0, 0.03f);
				if (darkImg.color.a >= 1)
				{
					SceneManager.LoadScene("re_title");
					titleBGM.rePlay = true;
				}
			}
		}

		// 敗北時の処理
		// ザコ敵たちをフェードインさせる
		if (phase >= 11 && moveFlag == false && loseFlag == true)
		{
			for (int i = 0; i < loseLetterSR.LongLength; ++i)
			{
				loseLetterSR[i].color += new Color(0, 0, 0, 0.05f);
				if (i == loseLetterSR.LongLength - 1)			// 手紙を全部フェードインさせたか判断
				{
					if (loseLetterSR[i].color.a >= 1)
					{
						letterFlag = true;
					}
				}
			}
			for (int i = 0; i < loseZakosSR.LongLength; ++i)
			{
				loseZakosSR[i].color += new Color(0, 0, 0, 0.05f);
				if (i == loseZakosSR.LongLength - 1)			// ザコを全部フェードインさせたか判断
				{
					if (loseZakosSR[i].color.a >= 1)
					{
						zakoFlag = true;
					}
				}
			}
			if (letterFlag == true && zakoFlag == true)			// 全部表示しきったか
			{
				moveFlag = true;
				se.clip = ghostSE;
				se.Play();
			}
		}

		// 移動処理
		if (loseFlag == true)
		{
			for (int i = 0; i < loseZakos.LongLength; ++i)
			{
				if (moveFlag == true)
				{
					// ザコと手紙の上下移動をさせる
					if (swayingFlag == true)
					{
						loseLetter.transform.position += new Vector3(0, 0.02f, 0);
						loseZakos[i].transform.position += new Vector3(0, 0.04f, 0);
						if (loseLetter.transform.position.y >= 0.8f || loseZakos[i].transform.position.y >= 2.8f)
						{
							swayingFlag = false;
						}
					}
					else
					{
						loseLetter.transform.position -= new Vector3(0, 0.02f, 0);
						loseZakos[i].transform.position -= new Vector3(0, 0.04f, 0);
						if (loseLetter.transform.position.y <= -0.8f || loseZakos[i].transform.position.y <= 1.3f)
						{
							swayingFlag = true;
						}
					}
				}
			}
		}

		/* ---------------------------------------------------------------------------------------------
		++resultFrame;
		++soulFrame;
		++WobblingFrame;
		//++cntFrame;
		//if (cntFrame > 9)
		//{
		//	// スコアの加算
		//	score += addScore;
		//	cntFrame = 0;
		//}
		score += 2;
		// 超えないように制限
		if (score > GameMaster.recovery)
		{
			score = GameMaster.recovery;
			// 2018.06.30
			++forcedFrame;
			if (forcedFrame > 1200)
			{
				isBack = true;
			}
			// ランクの表示
			++cntFrame;
			if (cntFrame > 100)	// 100になってた←100に戻した K:ディレイ入れるために150に変更 ←P:やっぱり100にした
			{
				cntFrame = 50;
				rank.enabled = true;
				if (once == false && isBack == false)
				{
					audioSource[1].clip = audios[1];
					audioSource[1].Play();
					audioSource[2].clip = cheer[CheerCTR];
					audioSource[2].Play();
					once = true;
				}
			}
		}
		// リザルト文字動作
		if (resultFrame == 30)
		{
			RESULT[resultCut].transform.position += new Vector3(0.0f, 30.0f, 0.0f);
		}
		if (resultFrame == 40)
		{
			RESULT[resultCut].transform.position += new Vector3(0.0f, -30.0f, 0.0f);
			resultFrame = 0;
			resultCut++;
			if (resultCut >= 6) { resultCut = 0; }
		}
		// 魂
		if (soulFrame == 10)
		{
			SoulRand01 = Random.Range(0, 8);
			if (SoulRand01 != 8) { RESoul[SoulRand01].transform.position += new Vector3(0.0f, 30.0f, 0.0f); }
		}
		if (soulFrame == 15)
		{
			SoulRand02 = Random.Range(0, 8);
			if (SoulRand02 != 8) { RESoul[SoulRand02].transform.position += new Vector3(0.0f, 30.0f, 0.0f); }
		}
		if (soulFrame == 20)
		{
			if (SoulRand01 != 8) { RESoul[SoulRand01].transform.position += new Vector3(0.0f, -30.0f, 0.0f); }
		}
		if (soulFrame == 25)
		{
			if (SoulRand02 != 8) { RESoul[SoulRand02].transform.position += new Vector3(0.0f, -30.0f, 0.0f); }
			soulFrame = 0;
		}
		// ユラユラ
		if (WobblingFrame < 20)
		{
			wobbling.transform.position += new Vector3(0.0f, 0.5f, 0.0f);
		}
		if (WobblingFrame >= 20 && WobblingFrame < 40)
		{
			wobbling.transform.position += new Vector3(0.0f, -0.5f, 0.0f);
		}
		if (WobblingFrame == 41)
		{
			WobblingFrame = 0;
		}

		// ランクを設定
		// 回収率が100%の時はＳ
		if (GameMaster.recovery == 100)
		{
			rank.sprite = s;
			CheerCTR = 0;
		}
		else if (GameMaster.recovery < 100 && GameMaster.recovery >= 90)
		{
			rank.sprite = a;
			CheerCTR = 1;
		}
		else if (GameMaster.recovery < 90 && GameMaster.recovery >= 80)
		{
			rank.sprite = b;
			CheerCTR = 1;
		}
		else if (GameMaster.recovery < 80 && GameMaster.recovery >= 70)
		{
			rank.sprite = c;
			CheerCTR = 2;
		}
		else
		{
			rank.sprite = d;
			CheerCTR = 2;
		}
		// 表示の加算
		scoreText.text = score.ToString() + " ％";
		// シーン移行
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button5))
		{
			isBack = true;
		}
		// タイトルシーンに戻るまでの処理
		if (isBack)
		{
			TitleAnimation.afterEnding = true;
			audioSource[0].volume -= 0.0075f;	// 2018.06.30
			//scoreText.enabled = false;
			//rank.enabled = false;
			if (frame >= frameMax)
			{
				SceneManager.LoadScene("title");
			}
			// スポットライトを通す
			darkLightObj.transform.localScale -= new Vector3(0.055f, 0.055f, 0.055f);
			if (darkLightObj.transform.localScale.x < 0)
			{
				darkLightObj.transform.localScale = new Vector3(0, 0, 0);
			}
			// スポットライトを表示(タイトルシーンに戻るまでの時間が変わったら値が変わる)
			darkSpriteRenderer.color += new Color(0, 0, 0, 0.002f);
			// 定数はタイトルを表示するフレーム数
			if (frame >= frameMax - 80)
			{
				// タイトルの表示
				title.color += new Color(0, 0, 0, 0.02f);
			}
			++frame;
		}
		--------------------------------------------------------------------------------------------- */
	}

	// [Re]リザルトの合計(得点、ランク共々)表示
	private void TotalDIsplay(int point)
	{
		int MaxPoint = 20000;
		totalText.text = point.ToString();
		if (point >= MaxPoint)
		{
			waxStampSR.sprite = wSSprites[0];
			se.clip = audioClips[0];
		}
		else if (point >= MaxPoint - 5000)
		{
			waxStampSR.sprite = wSSprites[1];
			se.clip = audioClips[1];
		}
		else if(point >= MaxPoint - 10000)
		{
			waxStampSR.sprite = wSSprites[2];
			se.clip = audioClips[1];
		}
		else if(point >= MaxPoint - 15000)
		{
			waxStampSR.sprite = wSSprites[3];
			se.clip = audioClips[2];
		}
		else
		{
			waxStampSR.sprite = wSSprites[4];
			se.clip = audioClips[2];
		}
		totalText.enabled = false;
	}
}

/*
 * 2018.12.28 作成
 *		2019.02.09 大幅に変更
 *		2019.03.07 BGM,SEの追加
 */
 /* セレクトとメインの間にある演出のみを行う */
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class Production : MonoBehaviour
{
	int frame = 0;

	// フレーム一覧 2019.02.09
	const int kFadeInFrame = 90;													// シーンを表示するフレーム数
	const int kFallingLetterFrame = kFadeInFrame + 280;								// 手紙が落ちてくるフレーム数
	const int kTextWindowFrame = kFallingLetterFrame + 280;							// テキストを表示するフレーム数
	const int kEnemyWordsFrame = kTextWindowFrame + 210;							// 敵のセリフを表示するフレーム数
	const int kEnemyWaitFrame = 60;													// 敵を映し出すフレーム数
	const int kInholeLetterFrame_pos = kEnemyWordsFrame + kEnemyWaitFrame + 120;	// 手紙が位置で吸い込まれるフレーム数
	const int kInholeLetterFrame_scale = kInholeLetterFrame_pos + 120;				// 手紙がスケールで吸い込まれるフレーム数
	const int kShrinkBlackholeFrame = kInholeLetterFrame_scale + 100;				// ブラックホールを小さくするフレーム数

	float shake;					// 縦にどれくらい動いたか
	float addShake = 0.001f;		// 揺れる速さ

	public SpriteRenderer screenFlash;	// 画面にでる白

	// メッセージテキスト
	public Text message;
	public Text enemyWords;	// エネミーの台詞 2019.02.09
	// 手紙
	public GameObject letter;
	// 手紙を落とすときの位置 2019.02.09
	List<Vector3> letterTargetPosList = new List<Vector3>
	{
		new Vector3(-4.1f, 6f, 0f),
		new Vector3(4.1f, 3f, 0f),
		Vector3.zero
	};
	int nowTarget = 0;	// 現在の目的地 2019.02.09
	GameObject zako;	// 手紙に引っ付いているザコ 2019.02.09
	// ブラックホール
	public SpriteRenderer blackhole;
	// プレステキスト 2019.02.21
	public SpriteRenderer pressSr;
	Color addColor = new Color(0, 0, 0, 0.01f);
	// スキップ 2019.02.21
	bool isSkip = false;
	int inputFrame = 0;
	const int kSkipFadeIn = 60;
	const int kSkipFadeOut = kSkipFadeIn + 60;

	// BGM,SEの追加
	public AudioSource bgm;
	public AudioSource se;

	void Start ()
	{
		message.color = new Color(0.9f, 0.9f, 0.9f, 0f);
		enemyWords.color = new Color(0.9f, 0.9f, 0.9f, 0f);
		screenFlash.color = new Color(0f, 0f, 0f, 1f);
		zako = letter.transform.GetChild(0).gameObject;
		zako.SetActive(false);
		blackhole.enabled = false;
		//// 2019.03.07
		//bgm.volume = 0;
	}

	void Update ()
	{
		// 揺らす
		shake -= addShake;
		if (Mathf.Abs(shake) > 0.02f) { addShake *= -1; }
		if (!isSkip && Input.anyKeyDown)
		{
			isSkip = true;
			inputFrame = frame;
		}
		if (isSkip)
		{
			if (frame < kSkipFadeIn + inputFrame)
			{
				FadeIn(screenFlash, 0.1f);
			}
			else if (frame < kSkipFadeOut + inputFrame)
			{
				message.color = new Color(0.9f, 0.9f, 0.9f, 0f);
				enemyWords.color = new Color(0.9f, 0.9f, 0.9f, 0f);
				letter.SetActive(false);
				zako.SetActive(false);
				blackhole.enabled = false;
				FadeOut(screenFlash, 0.1f);
			}
			else
			{
				SceneManager.LoadScene("main");
			}
		}
		else
		{
			// フレームを過ぎたら揺らす 2019.02.09
			if (frame >= kTextWindowFrame + (kEnemyWordsFrame - kTextWindowFrame) / 4 * 3)
			{
				letter.transform.localPosition += new Vector3(0, shake, 0);
			}
			// ブラックホールを回転させる
			blackhole.transform.eulerAngles -= new Vector3(0.0f, 0.0f, 2.0f);
			if (blackhole.transform.eulerAngles.z < -360)
			{
				blackhole.transform.eulerAngles += new Vector3(0.0f, 0.0f, 360.0f);
			}
			// 画面を表示する
			if (frame < kFadeInFrame)
			{
				FadeOut(screenFlash, 2f / 60f);
			}
			// 手紙を落とす
			else if (frame < kFallingLetterFrame)
			{
				if (nowTarget < letterTargetPosList.Count - 1 && IsNear(letter.transform.position, letterTargetPosList[nowTarget], 0.2f))
				{
					++nowTarget;
				}
				letter.transform.position = Vector3.Lerp(letter.transform.position, letterTargetPosList[nowTarget], 3f * Time.deltaTime);
			}
			// 手紙説明のテキストを表示する
			else if (frame < kTextWindowFrame)
			{
				if (frame < kFallingLetterFrame + (kTextWindowFrame - kFallingLetterFrame) / 2)
				{
					FadeIn(message, 3f / 60f);
					FadeIn(screenFlash, 1f / 60f);
					if (screenFlash.color.a > 0.3f)
					{
						screenFlash.color = new Color(0f, 0f, 0f, 0.3f);
					}
				}
				else if (frame >= kFallingLetterFrame + (kTextWindowFrame - kFallingLetterFrame) / 3 * 2)
				{
					FadeOut(message, 3f / 60f);
					FadeOut(screenFlash, 1f / 60f);
				}
			}
			// 敵の台詞を表示する（ブラックホールの表示も行う）
			else if (frame < kEnemyWordsFrame + kEnemyWaitFrame)
			{
				if (frame < kTextWindowFrame + (kEnemyWordsFrame - kTextWindowFrame) / 5)
				{
					FadeIn(screenFlash, 0.2f);
					se.Play();
				}
				else if (frame < kTextWindowFrame + (kEnemyWordsFrame - kTextWindowFrame) / 5 * 2)
				{
					FadeIn(enemyWords, 5f / 60f);
				}
				else if (frame < kTextWindowFrame + (kEnemyWordsFrame - kTextWindowFrame) / 5 * 4 && frame >= kTextWindowFrame + (kEnemyWordsFrame - kTextWindowFrame) / 5 * 3)
				{
					FadeOut(enemyWords, 5f / 60f);
					blackhole.enabled = true;
					zako.SetActive(true);
				}
				else if (frame >= kTextWindowFrame + (kEnemyWordsFrame - kTextWindowFrame) / 5 * 4)
				{
					FadeOut(screenFlash, 0.2f);
				}
			}
			// 手紙を吸い込む（位置移動）
			else if (frame < kInholeLetterFrame_pos)
			{
				letter.transform.localScale = Vector3.Lerp(letter.transform.localScale,
					new Vector3(0.4f, 0.4f, 1f), 1f * Time.deltaTime);
				letter.transform.position = Vector3.Lerp(letter.transform.position,
					blackhole.transform.position, 1f * Time.deltaTime);
			}
			// 手紙を吸い込む（縮小）
			else if (frame < kInholeLetterFrame_scale)
			{
				letter.transform.localScale = Vector3.Lerp(letter.transform.localScale,
					Vector3.zero, 2f * Time.deltaTime);
			}
			// ブラックホールを縮小する
			else if (frame < kShrinkBlackholeFrame)
			{
				letter.transform.localScale = Vector3.zero;
				blackhole.transform.localScale = Vector3.Lerp(blackhole.transform.localScale, Vector3.zero, 3f * Time.deltaTime);
			}
			// シーン移行
			else
			{
				blackhole.transform.localScale = Vector3.zero;
				SceneManager.LoadScene("main");
			}
		}
		if (frame < kInholeLetterFrame_pos && !isSkip)
		{
			if (pressSr.color.a < 0 || pressSr.color.a > 1)
			{
				addColor *= -1;
				pressSr.color = new Color(pressSr.color.r, pressSr.color.g, pressSr.color.b, (int)pressSr.color.a);
			}
			pressSr.color += addColor;
		}
		else if (frame >= kInholeLetterFrame_pos || isSkip)
		{
			FadeOut(pressSr, 0.1f);
		}
		{
		}
		++frame;
	}

	// 二つの位置がどれくらい近いか比べる-------------------------------------------------------
	bool IsNear(Vector3 a, Vector3 b, float value)
	{
		return Mathf.Abs(a.x - b.x) < value && Mathf.Abs(a.y - b.y) < value && Mathf.Abs(a.z - b.z) < value;
	}

	// すーっと表示するための関数------------------------------------------------------------------
	void FadeIn(SpriteRenderer sr, float value = 0.01f, float max = 1f)
	{
		sr.color += new Color(0, 0, 0, value);
		if (sr.color.a > max)
		{
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, max);
		}
	}
	void FadeIn(Text txt, float value = 0.01f, float max = 1f)
	{
		txt.color += new Color(0, 0, 0, value);
		if (txt.color.a > max)
		{
			txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, max);
		}
	}

	// すーっと消すための関数----------------------------------------------------------------
	void FadeOut(SpriteRenderer sr, float value = 0.01f)
	{
		sr.color -= new Color(0, 0, 0, value);
		if (sr.color.a < 0f)
		{
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
		}
	}
	void FadeOut(Text txt, float value = 0.01f)
	{
		txt.color -= new Color(0, 0, 0, value);
		if (txt.color.a < 0f)
		{
			txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 0f);
		}
	}
}

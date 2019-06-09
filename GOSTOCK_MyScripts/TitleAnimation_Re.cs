/*
 * 2018.12.09 作成
 *		2018.12.15 最終画面でおばけとフラッシュを出す
 *		2018.12.16 招待状
 *		2018.12.28 招待状を個別のスクリプトへ
 *		2018.12.29 ボスの手を別々に動かす
 *		2018.12.30 フラッシュをフラッシュする
 *		2019.01.01 ボスの手の移動関係の変更
 *				   おばけが消えた時に消えた時のを出す
 *		2019.01.05 調整・演出カット
 *		2019.01.08 数値変更
 *		2019.01.13 手をセレクトの状態にする
 *		2019.01.18 ????
 *		2019.02.11 ポイントされている手を揺らす・数値調整
 *		2019.02.12 おばけが消えそうなときのスプライト
 *		2019.02.15 タイミング調整
 *				   ランダムおばけの表情を変更する
 *				   モード決定されたらカーソルを点滅させる
 *		2019.02.17 ムービーを流す
 *		2019.02.19 ボスの手がどこかに行くのを修正
 *				   ムービー管理をリストに変更
 *		2019.02.20 レンズ->目
 *				   ボスの手の再調整
 *		2019.02.21 音のフェードアウト
 *		2019.03.04 BGMの再生を修正、タイトル時にEscapeでアプリケーション終了
 */
/* 新しいタイトルのアニメーション */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TitleAnimation_Re : MonoBehaviour
{
	// 全体で使う
	int frame = 0;
	public static bool afterEnding = false;
	AudioSource audioSource;
	float shake;					// 縦にどれくらい動いたか
	float addShake = 0.0005f;		// 揺れる速さ
	bool isInput = false;			// 入力がされたか
	int inputFrame = 0;				// 入力がされた時間 2019.01.18
	bool isDicision = false;		// 決定したか 2019.01.18
	// ステップ別フレーム一覧
	const int startAllFrame = 30;							// 全てが始まるフレーム
	const int startMoveGhostFrame = startAllFrame + 140;	// おばけが動き出すフレーム
	const int startFlashFrame = startMoveGhostFrame + 180;	// フラッシュをやりだすフレーム
	const int startFinishFrame = startFlashFrame + 80;		// 最後の場面が始まるフレーム

	// しゅっと出す
	public SpriteRenderer titleLogo;
	public SpriteRenderer titleCamera;		// 2018.12.30
	public SpriteRenderer titleCameraLens;	// 2018.12.30
	// おばけ系
	public SpriteRenderer titleGhost;	// GameObject -> SpriteRenderer
	public Sprite ghostVanishSprite;	// おばけが消えそうなときのスプライト 2019.02.12
	Sprite ghostNormalSprite;			// おばけの通常状態の時のスプライト 2019.02.12
	public GameObject deleteGhost;		// 2019.01.01
	public Anim titleGhostAnim;			// タイトルのオバケのアニメーション 2019.03.04
	public Sprite[] titleGhostLists;	// ↑のアニメーションリスト			2019.03.04
	// おばけをとる
	public SpriteRenderer cameraFlash;	// フラッシュ
	public SpriteRenderer screenFlash;	// 画面のフラッシュ
	int flashFrame = 2;					// フラッシュを管理するフレーム 2018.12.30
	// ボスの手
	public SpriteRenderer titleBossHand_R;		// 右手
	public SpriteRenderer titleBossHand_L;		// 左手
	Vector2 bossHandMoveValue = Vector2.zero;	// ボスの手がどれくらい動いたか(絶対値) 2019.01.01
	// PressAnyButton
	public SpriteRenderer pressSr;
	Color addColor = new Color(0, 0, 0, 0.01f);
	SpriteRenderer randomGhost;					// 放っておいたら出てくるおばけ 2019.01.18
	Vector3 randomGhostVelocity = Vector3.zero;	// ランダムおばけの速度 2019.01.18
	bool isRandomGhostDelete = false;			// おばけが消されるか 2019.01.18
	// Ricecake
	public SpriteRenderer titleRicecake;	// ロゴにはまっているおもち 2018.12.30

	// スポットライト以外を削除 2018.12.28
	// スポットライト
	public GameObject spotLight;

	// 2019.01.18 追加分、入力後に使う
	public SpriteRenderer titleCursor;		// タイトルのカーソル
	public Sprite halfBossHandSprite;		// 半開きの手 2019.01.13
	public Sprite bossRockHand;				// グーの状態のボスの手
	public Sprite bossPlayHand;				// Playが書かれたボスの手
	public Sprite bossSetHand;				// Setが書かれたボスの手
	int cursorPosNum = 0;					// カーソルの位置を表す
	Vector2[] cursorPos = new Vector2[2];	// カーソルのターゲット
	int cursorMoveDelay = 0;				// カーソルを動かすときのディレイ

	// ムービー関連 2019.02.17
	public bool isPlayModeMovie = true;									// ムービーを流すかどうか
	public List<VideoPlayer> videoPlayerList = new List<VideoPlayer>();	// 動画の分のVideoPlayerリスト
	int playingVideoNum = 0;											// 再生中の動画の要素数
	bool isMovie = false;												// ムービー中かどうか
	bool isStopMovie = false;											// 入力されてムービーを止めるかどうか

	// レンズ(目)のスプライト 2019.02.20
	Sprite lensNormal;
	public Sprite lensLeft;
	public Sprite lensRight;
	public Sprite lensShut;
	int shutFrame = 2;

	// 決定音のSE 2019.02.27
	public AudioSource cameraSE;

	public bool fadeBGM = false;

	void Awake()
	{
		Application.targetFrameRate = 60;
		Screen.SetResolution(1920, 1080, true);
		Cursor.visible = false;
	}

	void Start ()
	{
		// 情報取得
		audioSource = GetComponent<AudioSource>();
		// 情報設定
		shake = 0f;
		ghostNormalSprite = titleGhost.sprite;
		titleLogo.color = new Color(1f, 1f, 1f, 0f);
		titleCamera.color = new Color(1f, 1f, 1f, 0f);		// 2018.12.30
		titleCameraLens.color = new Color(1f, 1f, 1f, 0f);	// 2018.12.30
		screenFlash.color = cameraFlash.color = new Color(1f, 1f, 1f, 0f);
		pressSr.color = new Color(pressSr.color.r, pressSr.color.g, pressSr.color.b, 0f);
		titleBossHand_R.color = titleBossHand_L.color = new Color(1f, 1f, 1f, 0f);
		spotLight.transform.localScale = new Vector3(0f, 0f, 1f);
		titleRicecake.color = new Color(1f, 1f, 1f, 0f);	// 2018.12.30
		titleCursor.color = new Color(1f, 0f, 0f, 0f);		// 2019.01.18
		lensNormal = titleCameraLens.sprite;				// 2019.02.20
	}

	void Update ()
	{
		// それぞれ揺らす
		shake -= addShake;
		if (Mathf.Abs(shake) > 0.02f) { addShake *= -1; }
		// 2018.12.15
		if (frame < startFlashFrame)
		{
			titleGhost.transform.localPosition += new Vector3(0, shake, 0);
		}
		// ランダムに出てきたオバケをペロペロさせる	2019.03.04
		if (randomGhost == true && randomGhost.sprite != ghostVanishSprite)
		{
			titleGhostAnim = randomGhost.GetComponent<Anim>();
			titleGhostAnim.Animation(titleGhostLists, 4);
		}
		// ランダムおばけが生成されていたら操作する 2019.01.18
		if (randomGhost)
		{
			if (flashFrame == 0) { isRandomGhostDelete = true; }
			// 消されるときの動作
			if (isRandomGhostDelete)
			{
				if (flashFrame == 46)
				{
					Instantiate(deleteGhost, randomGhost.transform.position, Quaternion.identity);
					Destroy(randomGhost);
					isRandomGhostDelete = false;
				}
				else
				{
					FadeOut(randomGhost);
					// おばけの絵を変える 2019.02.15
					randomGhost.sprite = ghostVanishSprite;
				}
			}
			// 移動
			else
			{
				randomGhost.transform.position += randomGhostVelocity;
				randomGhost.transform.localPosition += new Vector3(0, shake, 0);
				randomGhost.flipX = randomGhostVelocity.x > 0;
			}
		}
		// フラッシュを表示 2018.12.30
		if (flashFrame < 2)
		{
			cameraFlash.color = Color.white;
			screenFlash.color = new Color(1f, 1f, 1f, 0.05f);
		}
		else if (frame >= startFinishFrame + 10 && frame <= startFinishFrame + 300)
		{
			cameraFlash.color = new Color(1f, 1f, 1f, 0f);
			screenFlash.color = new Color(1f, 1f, 1f, 0f);
		}
		++flashFrame;
		if (shutFrame < 2)
		{
			titleCameraLens.sprite = lensShut;
		}
		else
		{
			titleCameraLens.sprite = lensNormal;
		}
		++shutFrame;
		// ロゴ表示
		if (startMoveGhostFrame > frame && frame > startAllFrame)
		{
			FadeIn(titleLogo);
			// カメラの表示 2018.12.30 処理の移動＆条件追加 2019.01.05
			if (frame > startMoveGhostFrame - 40)
			{
				FadeIn(titleCamera, 1f / 40f);
				FadeIn(titleCameraLens, 1f / 40f);
			}
		}
		// おばけが所定の位置に行くまでの処理
		else if (startFlashFrame > frame && frame >= startMoveGhostFrame)
		{
			// おばけの移動
			if (titleGhost.transform.localPosition.x > 1.8f)
			{
				titleGhost.transform.localPosition += new Vector3(-0.06f, 0, 0);
				// 位置補正
				if (titleGhost.transform.localPosition.x < 1.8f)
				{
					titleGhost.transform.localPosition = new Vector3(1.8f, titleGhost.transform.localPosition.y, titleGhost.transform.localPosition.z);
				}
			}
			// 一番最初に出てくるオバケをペロペロさせる 2019.03.04
			if (titleGhost.sprite != ghostVanishSprite)
			{
				titleGhostAnim.Animation(titleGhostLists, 4);
			}
		}
		// フラッシュをたいておばけを消すだけ
		else if (startFinishFrame > frame && frame >= startFlashFrame)
		{
			// フラッシュをたいておばけを消す 条件を変更・処理を移動 2018.12.30
			if (frame < startFlashFrame + 2)
			{
				// フラッシュを表示 2019.01.05
				FadeIn(cameraFlash, 0.5f);
				FadeIn(screenFlash, 0.05f);
				cameraSE.Play();		// 2019.03.03
			}
			// フラッシュを非表示にする 2019.01.05
			else if (frame < startFlashFrame + 12)
			{
				FadeOut(cameraFlash, 0.1f);
				FadeOut(screenFlash, 0.01f);
			}
			// おばけを薄くする 2019.01.05
			FadeOut(titleGhost);
			// おばけの絵を変える 2019.02.12
			titleGhost.sprite = ghostVanishSprite;
			// おばけを消す 2019.01.05
			if (frame == startFinishFrame - 34)
			{
				Instantiate(deleteGhost, titleGhost.transform.position, Quaternion.identity);
				titleGhost.transform.localPosition = new Vector3(-1.25f, 2.97f, titleGhost.transform.localPosition.z);
				titleGhost.color = new Color(1f, 1f, 1f, 0f);
			}
		}
		// 最後の場面
		else if (frame >= startFinishFrame)
		{
			// 入力受付 条件追加 2019.01.18	2019.02.17
			if (Input.anyKeyDown && !isInput && (frame <= startFinishFrame + 360 || !isPlayModeMovie))
			{
				isInput = true;
				inputFrame = frame;
				shake = 0;
				if (Mathf.Sign(addShake) > 0) { addShake *= -1; }	// 2019.02.11
				titleCursor.transform.position = cursorPos[0];		// 2019.02.20
			}
			// PressAnyButtonを点滅 入力をされていなかったらを追加 2019.01.18
			if (!isInput)
			{
				if (pressSr.color.a < 0 || pressSr.color.a > 1)
				{
					addColor *= -1;
					pressSr.color = new Color(pressSr.color.r, pressSr.color.g, pressSr.color.b, (int)pressSr.color.a);
				}
				pressSr.color += addColor;
			}
			// おばけを表示 2018.12.15 フラッシュの表示を削除・おもちを表示に変更 2018.12.30
			FadeIn(titleRicecake, 0.0075f);
			// 手を移動させながら表示 条件追加 2019.01.18
			if (!isDicision)
			{
				FadeIn(titleBossHand_R, 0.0075f);
				FadeIn(titleBossHand_L, 0.0075f);
			}
			// ボスの手を移動させる 2019.01.01
			float moveValueX = 0.0185f;
			float moveValueY = 0.02f;
			// 位置補正
			if (bossHandMoveValue.x + moveValueX > 2.75f)
			{
				moveValueX = 2.75f - bossHandMoveValue.x;
			}
			bossHandMoveValue.x += moveValueX;
			if (bossHandMoveValue.y + moveValueY > 3f)
			{
				moveValueY = 3f - bossHandMoveValue.y;
				// 2018.12.29 x軸も動かす 2019.01.01 条件追加 2019.01.18
				if (isInput && isDicision)
				{
					if (cursorPosNum == 0)
					{
						titleBossHand_R.transform.localPosition += new Vector3(0f, shake, 0f);
					}
					else if (cursorPosNum == 1)
					{
						titleBossHand_L.transform.localPosition += new Vector3(0f, -shake, 0f);
					}
					shake -= addShake;
				}
				// 入力を受けていて決定がされていなかったとき 2019.02.11
				else if (isInput && titleCursor.color.a > 0f)
				{
					if (cursorPosNum == 0)
					{
						titleBossHand_R.transform.localPosition += new Vector3(0f, shake, 0f);
					}
					else if (cursorPosNum == 1)
					{
						titleBossHand_L.transform.localPosition += new Vector3(0f, -shake, 0f);
					}
				}
				else if (!isInput)
				{
					titleBossHand_R.transform.localPosition += new Vector3((0.02f - shake) * Mathf.Sign(addShake * shake) / 4f, shake, 0f);
					titleBossHand_L.transform.localPosition += new Vector3((0.02f - shake) * Mathf.Sign(addShake * shake) / -4f, -shake, 0f);
				}
			}
			bossHandMoveValue.y += moveValueY;
			// 数値変更 2019.01.08
			titleBossHand_R.transform.position += new Vector3(-moveValueX, -moveValueY * 1.15f, 0f);
			titleBossHand_L.transform.position += new Vector3(moveValueX, -moveValueY * 0.85f, 0f);
			// カーソルの位置を設定、ぞれぞれ手の初期位置と演算 2019.01.18
			cursorPos[0] = new Vector2(-(bossHandMoveValue.x + 2.75f), 1.15f - bossHandMoveValue.y);
			cursorPos[1] = new Vector2(bossHandMoveValue.x + 2.75f, 1.15f - bossHandMoveValue.y);
			// 確率でフラッシュ 2018.12.30	2019.02.17
			if (flashFrame > 120 && Random.Range(0,128) == 0 && !isInput && frame <= startFinishFrame + 360)
			{
				flashFrame = 0;
			}
			// 一定時間でランダムおばけを生成 2019.01.18
			if (frame % 480 == 0 && !randomGhost && frame <= startFinishFrame + 360)
			{
				titleGhost.sprite = ghostNormalSprite;
				InsRandomGhost();
			}
			if (Random.Range(0, 128) == 0)
			{
				shutFrame = 0;
			}
		}
		// ムービーを流す 2019.02.17
		if (isMovie && isPlayModeMovie)
		{
			screenFlash.color = new Color(0f, 0f, 0f, screenFlash.color.a);
			// ムービーが終わりに近づくと暗転
			if ((ulong)videoPlayerList[playingVideoNum].frame >= videoPlayerList[playingVideoNum].frameCount - 10 || isStopMovie)
			{
				FadeIn(screenFlash, 0.1f);
			}
			// ムービーが再生されて15フレームで明るくする
			else if (videoPlayerList[playingVideoNum].frame > 15)
			{
				FadeOut(screenFlash, 0.1f);
			}
			if (Input.anyKeyDown)
			{
				isStopMovie = true;
			}
			// ムービー終わり
			if (((ulong)videoPlayerList[playingVideoNum].frame == videoPlayerList[playingVideoNum].frameCount && videoPlayerList[playingVideoNum].frame != 0) || (isStopMovie && screenFlash.color.a >= 1f))
			{
				isMovie = false;
				isStopMovie = false;
				videoPlayerList[playingVideoNum].Stop();
				SetFinishState();
				++playingVideoNum;
				if (playingVideoNum >= videoPlayerList.Count) { playingVideoNum = 0; }
			}
		}
		else if (!isInput && isPlayModeMovie)
		{
			// 入力がされないで一定時間たったらムービーに移行する	間隔を取るため、フレームの変更 2019.02.27
			if (frame > startFinishFrame + 500)
			{
				screenFlash.color = new Color(0f, 0f, 0f, screenFlash.color.a);
				FadeIn(screenFlash, 1f / 60f);
				if (screenFlash.color.a >= 1f)
				{
					isMovie = true;
					videoPlayerList[playingVideoNum].Play();
				}
			}
			else if (flashFrame >= 2)
			{
				FadeOut(screenFlash, 0.1f);
			}
			else
			{
				screenFlash.color = new Color(1f, 1f, 1f, screenFlash.color.a);
			}
		}
		// 入力をされたら かなり削除・変更 2018.12.28 中身をすべて変更 2019.01.18
		if (isInput)
		{
			// スポットライトがある程度広がったらセレクトに移行 削除 2019.01.18
			// pressをフェードアウト
			FadeOut(pressSr, 0.5f / 60f);
			// 入力をされてから一定時間後にカーソルを表示する	2019.02.15
			if (frame > inputFrame + 75 && !isDicision)
			{
				FadeIn(titleCursor, 0.05f);
			}
			// pressのフェードアウトと同時にボスの手を移動させる
			else if (!isDicision)
			{
				titleBossHand_R.transform.position = Vector2.MoveTowards(titleBossHand_R.transform.position,
					new Vector2(-(bossHandMoveValue.x + 2.75f), 1.15f - bossHandMoveValue.y), 1f * Time.deltaTime);
				titleBossHand_L.transform.position = Vector2.MoveTowards(titleBossHand_L.transform.position,
					new Vector2(bossHandMoveValue.x + 2.75f, 1.15f - bossHandMoveValue.y), 1f * Time.deltaTime);
				// 絵を変える
				if (frame > inputFrame + 20)
				{
					titleBossHand_L.sprite = bossRockHand;
					titleBossHand_R.sprite = bossRockHand;
				}
				else if (frame > inputFrame + 10)
				{
					titleBossHand_L.sprite = halfBossHandSprite;
					titleBossHand_R.sprite = halfBossHandSprite;
				}
			}
			if (titleCursor.color.a > 0 || isDicision)
			{
				if (isDicision)
				{
					// カーソルを点滅 2019.02.15
					if (frame % 6 < 3) { titleCursor.color = new Color(1f, 0.5f, 0.5f, titleCursor.color.a); }
					else { titleCursor.color = new Color(1f, 0f, 0f, titleCursor.color.a); }
					// 選ばれた手と選ばれなかった手を設定
					SpriteRenderer nonSelectHand = titleBossHand_R;
					SpriteRenderer selectedHand = titleBossHand_L;
					if (cursorPosNum == 0)
					{
						nonSelectHand = titleBossHand_L;
						selectedHand = titleBossHand_R;
					}
					// 選ばれなかったほうの手をフェードアウト
					FadeOut(nonSelectHand, 0.35f / 60f);
					// 音をフェードアウト 2019.02.21 条件変更、ロード画面の読み込みだけに
					if (cursorPosNum == 0)
					{
						fadeBGM = true;
					}
					// 選ばれなかったほうの手が消えたらすべてをフェードアウト
					if (nonSelectHand.color.a <= 0)
					{
						FadeOut(selectedHand, 1.1f / 60f);
						FadeOut(titleCursor, 1.1f / 60f);
						FadeOut(titleLogo, 1.1f / 60f);
						FadeOut(titleCamera, 1.1f / 60f);
						FadeOut(titleCameraLens, 1.1f / 60f);
						if (randomGhost) { FadeOut(randomGhost, 1.1f / 60f); }
					}
					// 全て消えたらシーン移行
					if (titleLogo.color.a <= 0)
					{
						
						if (cursorPosNum == 0)
						{
							SceneManager.LoadScene("Load");
						}
						else if (cursorPosNum == 1)
						{
							SceneManager.LoadScene("setting");
						}
					}
				}
				else
				{
					MoveCursor();
					// 手の絵を変える
					if (cursorPosNum == 0)
					{
						titleBossHand_R.sprite = bossPlayHand;
						titleBossHand_L.sprite = bossRockHand;
						titleCameraLens.sprite = lensLeft;
						// 2019.02.19
						titleBossHand_L.transform.position = new Vector3(bossHandMoveValue.x + 2.75f, 1.15f - bossHandMoveValue.y);
						titleCursor.transform.position = Vector2.Lerp(titleCursor.transform.position, titleBossHand_R.transform.position, 3f * Time.deltaTime);
						titleBossHand_L.flipX = true;
					}
					else if (cursorPosNum == 1)
					{
						titleBossHand_R.sprite = bossRockHand;
						titleBossHand_L.sprite = bossSetHand;
						titleCameraLens.sprite = lensRight;
						// 2019.02.19
						titleBossHand_R.transform.position = new Vector3(-(bossHandMoveValue.x + 2.75f), 1.15f - bossHandMoveValue.y);
						titleCursor.transform.position = Vector2.Lerp(titleCursor.transform.position, titleBossHand_L.transform.position, 3f * Time.deltaTime);
						titleBossHand_L.flipX = false;
					}
					if ((Input.GetKey(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1)) && !isDicision)
					{
						isDicision = true;
						shake = 0;
						cameraSE.Play();		// 2019.02.27
					}
				}
			}
		}
		// 演出カット 2019.01.05
		if ((Input.GetKey(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1)) && frame < startFinishFrame)
		{
			SetFinishState();
		}
		// アプリケーションの終了 2019.03.04
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
		++frame;
	}

	// 最後の場面の状態にする 2019.01.05------------------------------------------------
	void SetFinishState()
	{
		frame = startFinishFrame;
		shake = 0.0105f;
		if (Mathf.Sign(addShake) > 0) { addShake *= -1; }
		titleGhost.color = new Color(1f, 1f, 1f, 0f);
		titleLogo.color = new Color(1f, 1f, 1f, 1f);
		titleCamera.color = new Color(1f, 1f, 1f, 1f);
		titleCameraLens.color = new Color(1f, 1f, 1f, 1f);
		titleBossHand_R.transform.position = new Vector3(-5.5f, -2.15f, titleBossHand_R.transform.position.z);
		titleBossHand_L.transform.position = new Vector3(5.5f, -1.55f, titleBossHand_L.transform.position.z);
		bossHandMoveValue.x = 2.75f;
		bossHandMoveValue.y = 3f;
	}

	// カーソルを動かす 2019.01.18-----------------------------------------------------
	void MoveCursor()
	{
		float inputX = Input.GetAxis("Horizontal");
		if (Input.GetAxis("CrossX") != 0)
		{
			inputX = Input.GetAxis("CrossX");
		}
		if (cursorMoveDelay > 20)
		{
			if (inputX > 0f)
			{
				cursorPosNum = cursorPosNum + 1 > 1 ? 0 : cursorPosNum + 1;
			}
			if (inputX < 0f)
			{
				cursorPosNum = cursorPosNum - 1 < 0 ? 1 : cursorPosNum - 1;
			}
			cursorMoveDelay = 0;
			titleCursor.transform.position = cursorPos[cursorPosNum];
		}
		else
		{
			++cursorMoveDelay;
		}
		if (inputX == 0f)
		{
			cursorMoveDelay = 20;
		}
	}

	// 放っておいたら出てくるおばけを生成する
	void InsRandomGhost()
	{
		Vector2 insPos = Vector2.zero;
		float insPosXSign = Mathf.Sign(Random.Range(-1f, 1f));
		insPos.x = 12f * insPosXSign;
		insPos.y = 2f;
		randomGhost = Instantiate(titleGhost, insPos, Quaternion.identity);
		randomGhost.color = Color.white;
		randomGhostVelocity = new Vector3((Random.Range(0f, 0.03f) + 0.03f) * -insPosXSign, Random.Range(-0.01f, 0.01f), 0f);
	}

	// すーっと表示するための関数
	void FadeIn(SpriteRenderer sr, float value = 0.01f)
	{
		sr.color += new Color(0, 0, 0, value);
		if (sr.color.a > 1f)
		{
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
		}
	}
	void FadeIn(Text txt, float value = 0.01f)
	{
		txt.color += new Color(0, 0, 0, value);
		if (txt.color.a > 1f)
		{
			txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1f);
		}
	}

	// すーっと消すための関数
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

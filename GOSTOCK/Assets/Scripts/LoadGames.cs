/* -----------------------------------------------------------------------------------------
 * LoadGames.cs
 * 2019.02.07 作成
 *		2019.02.07 ロード画面の制御用
 *		2019.02.14 説明(移動)の追加
 *		2019.02.16 各説明の追加、動きの追加
 *		2019.02.19 コントローラー対応
 *		2019.02.20 バグの修正、動きの修正諸々
 *		2019.03.11 <>に押したリアクションを追加
 *		2019.03.14 オバケの再出現タイミングの調整、やられの表情差し替え
 ----------------------------------------------------------------------------------------- */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadGames : MonoBehaviour
{
	// フェードイン、アウト
	public Image fadeDark;					// シーンをフェードイン、アウトさせるのに使用

	// 各種イメージ
	public Image ghost;						// 回すオバケ
	public int rota;						// 回転角
	public Text text;						// ボタンを押させるテキスト
	private bool textFlag = false;			// ボタンのフェードインとアウトに使用
	public Text[] selecting;				// <>のテキスト→選択された時に赤色にするため 2019.03.11
	private int selectingDelay = 0;			// ↑のディレイ時間

	// 制御用
	private bool fadeInFlag = false;		// フェードイン中は動かないように
	private bool fadeOutFlag = false;		// フェードインからフェードアウトへの切り替え
	private bool moveFlag = false;			// シーン移動
	public int delayMax;					// ロード時間(時間稼ぎ用)
	public int delayNow = 0;				// 現在の経過時間(フレーム単位)

	// コントローラー対応
	private float LRTriggerAnalog;			// アナログスティックVer
	private float LRTriggerCross;			// 十字キーVer

	// 音楽
	public AudioSource bgm;
	public MainBGM mainBGM;

	// 一回限り
	public static bool loadOneFlag = true;	// 初回だけ時間を稼ぐ

	// 以下説明(簡易的なチュートリアル？※基本英表記はDes)用------------------------------------------------------------
	// 共通
	public Text[] desMainTexts;				// [0] ナンバリング、[1]説明用テキスト
	private int nowDes = 0;					// 現在の説明番号
	public int desDelay = 0;				// ディレイ
	private bool resetFlag = false;			// 各項目の初期化用フラグ

	// カーソルの移動系(略称はMoveのM。nowDes:0)
	public GameObject desMoveSet;			// 移動説明セット
	public Image desMCursor;				// 移動用で動かすカーソル
	private Vector3 desMCursorOldVec;		// ↑の初期位置
	public Image[] keyMColors;				// カーソルを押しているかのような色(十字キー)
	private int nowKey = 0;					// 現在のカーソル
	private int nextCnt = 0;				// ループカウンター
	private bool returnFlag = false;		// 動きを戻すために使用

	// 撮影系(略称はPhotoのP。nowDes:1)
	public GameObject desPhotoSet;			// 撮影説明セット
	public Image desPCursor;				// 撮影用で動かすカーソル
	private Vector3 desPCursorOldVec;		// ↑の初期位置
	public Image keyPColor;					// カーソルを押しているかのような色(B)
	public Image desPGhost;					// 撮影されるオバケ
	public GameObject deletePrefab;			// オバケが消えるアニメーション
	public Image[] desPMemorys;				// ゲージのメモリ
	private int memoryCnt = 0;				// メモリ表示
	private bool deletePFlag = false;		// オバケを消すのに使用
	public Text desPGaugeArrow;				// ゲージの上昇を示す矢印
	public Text[] desPConcentrationTexts;	// ゲージに注意を向けるように、集中線的なやつ

	// カーソルの切り替え(略称はChangeのC。nowDes:2)
	public GameObject desChangeSet;			// カーソル切り替えセット
	public Image desCCursor;				// 切り替えるカーソル
	public Sprite[] desCcursorImgs;			// 撮影とショットの画像
	public Image keyCColor;					// カーソルを押しているかのような色(RB)
	public GameObject desCZako;				// 震えるザコ敵
	private Vector3 desCZakoOldVec;			// ↑の初期位置
	private bool zakoTremble;				// ザコの震えに使用
	public ParticleSystem sweatPer;			// ザコの汗のパーティクル
	private bool one = true;				// パーティクルを一回再生するように

	// 攻撃系(略称はAttackのA。nowDes:3)
	public GameObject desAttackSet;			// 撮影説明セット
	public Image desACursor;				// 撮影用で動かすカーソル
	private Vector3 desACursorOldVec;		// ↑の初期位置
	public Image keyAColor;					// カーソルを押しているかのような色(B)
	public GameObject desAZako;				// ザコ敵セット
	public Image[] desAZakos;				// 攻撃されるザコ敵
	public Sprite[] desAZakoBeatens;		// ザコのやられの表情([0]ノーマル、[1]やられ)
	private int desABeatenCnt = 0;			// やられの表情の表示時間
	public Image[] desAMemorys;				// ゲージのメモリ
	private int memoryACnt = 3;				// メモリ表示
	private bool deleteAFlag = false;		// ザコ敵を消すのに使用
	public Text desAGaugeArrow;				// ゲージの減少を示す矢印
	public Text[] desAConcentrationTexts;	// ゲージに注意を向けるように、集中線的なやつ

	void Start ()
	{
		text.enabled = false;
		bgm.volume = 0;
		
		// カーソルの移動系、初期化
		desMCursorOldVec = desMCursor.transform.position;
		desMoveSet.SetActive(false);
		for(int i = 0; i < 4; ++i)
		{
			keyMColors[i].enabled = false;
		}
		// 撮影系、初期化
		desPCursorOldVec = desPCursor.transform.position;
		desPhotoSet.SetActive(false);
		desPGaugeArrow.enabled = false;
		for (int i = 0; i < 3; ++i)
		{
			desPConcentrationTexts[i].enabled = false;
		}
		// カーソル切り替え、初期化
		desChangeSet.SetActive(false);
		keyCColor.enabled = false;
		desCZakoOldVec = desCZako.transform.position;
		sweatPer.Stop();
		// 攻撃系、初期化
		desAttackSet.SetActive(false);
		desAGaugeArrow.enabled = false;
		for (int i = 0; i < 3; ++i)
		{
			desAConcentrationTexts[i].enabled = false;
		}
		// 2019.03.11
		mainBGM = FindObjectOfType<MainBGM>();
	}

	void Update()
	{
		// 音楽再生(フェードイン)
		if (bgm.volume < 1)
		{
			bgm.volume += 0.007f;
		}
		else
		{
			bgm.volume = 1;
		}

		// フェードイン
		if (fadeInFlag == false)
		{
			if (fadeDark.color.a >= 0)
			{
				fadeDark.color -= new Color(0, 0, 0, 0.1f);
			}
			else
			{
				fadeInFlag = true;
			}
		}

		// フェードインが終わり次第動かす
		if (fadeInFlag == true)
		{
			if (loadOneFlag == true)
			{
				++delayNow;
				ghost.transform.Rotate(new Vector3(0, 0, rota) * Time.deltaTime);
				if (delayNow >= delayMax / 2)
				{
					ghost.transform.position += new Vector3(0, 2, 0);
				}
				if (delayNow >= delayMax)
				{
					text.enabled = true;
					if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						moveFlag = true;
					}
					if (moveFlag == true)
					{
						fadeDark.color += new Color(0, 0, 0, 0.01f);
						bgm.volume -= 1f / 60f;
						if (fadeDark.color.a >= 1)
						{
							loadOneFlag = false;				// 2019.02.27
							// 2019.03.11
							if (mainBGM != null)
							{
								bool flag = mainBGM.rePlay;
								if (flag == false)
								{
									mainBGM.rePlay = true;
								}
							}
							SceneManager.LoadScene("mainProduction");
							
						}
					}
				}
			}
			else
			{
				text.enabled = true;
				if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
				{
					moveFlag = true;
				}
				if (moveFlag == true)
				{
					fadeDark.color += new Color(0, 0, 0, 0.01f);
					bgm.volume -= 1f / 60f;
					if (fadeDark.color.a >= 1)
					{
						// 2019.03.11
						if (mainBGM != null)
						{
							bool flag = mainBGM.rePlay;
							if (flag == false)
							{
								mainBGM.rePlay = true;
							}
						}
						SceneManager.LoadScene("mainProduction");
					}
				}
			}
		}

		// Pressの点滅
		if (text.enabled == true)
		{
			if (textFlag == true)
			{
				text.color -= new Color(0, 0, 0, 0.01f);
				if (text.color.a <= 0)
				{
					textFlag = false;
				}
			}
			else
			{
				text.color += new Color(0, 0, 0, 0.01f);
				if (text.color.a >= 1)
				{
					textFlag = true;
				}
			}
		}

		float LRXboxA = Input.GetAxisRaw("Horizontal");
		float LRXboxC = Input.GetAxis("CrossX");
		// 説明項目切り替え
		if (Input.GetKeyDown(KeyCode.RightArrow) || (LRXboxA > 0 && LRTriggerAnalog == 0.0f) || (LRXboxC > 0 && LRTriggerCross == 0.0f))
		{
			++nowDes;
			desDelay = 0;
			nextCnt = 0;
			nowKey = 0;
			sweatPer.Stop();
			if (nowDes >= 4)
			{
				nowDes = 0;
			}
			selecting[0].color = Color.red;			// 2019.03.11
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow) || (LRXboxA < 0 && LRTriggerAnalog == 0.0f) || (LRXboxC < 0 && LRTriggerCross == 0.0f))
		{
			--nowDes;
			desDelay = 0;
			nextCnt = 0;
			nowKey = 0;
			sweatPer.Stop();
			if (nowDes < 0)
			{
				nowDes = 3;
			}
			selecting[1].color = Color.red;			// 2019.03.11
		}
		LRTriggerAnalog = LRXboxA;
		LRTriggerCross = LRXboxC;

		// <>の色を元に戻す 2019.03.11
		if (selecting[0].color == Color.red || selecting[1].color == Color.red)
		{
			++selectingDelay;
			if (selectingDelay >= 20)
			{
				selecting[0].color = Color.white;
				selecting[1].color = Color.white;
				selectingDelay = 0;
			}
		}


		if (resetFlag == false)
		{
			resetFlag = true;
			switch (nowDes)
			{
				case 0:
					DesMove();
					break;
				case 1:
					DesPhoto();
					break;
				case 2:
					DesChange();
					break;
				case 3:
					DesAttack();
					break;
			}
		}
	}

	// カーソル移動--------------------------------------------------------------------------------------------------------------
	private void DesMove()
	{
		// 初期化
		if (resetFlag == true)
		{
			desMoveSet.SetActive(true);
			desPhotoSet.SetActive(false);
			desChangeSet.SetActive(false);
			desAttackSet.SetActive(false);
			desMainTexts[0].text = "1";
			desMainTexts[1].text = "カーソルの移動";
			for (int i = 0; i < 4; ++i)
			{
				keyMColors[i].enabled = false;
				if (i == nowKey)
				{
					keyMColors[nowKey].enabled = true;
				}
			}
			resetFlag = false;
		}

		// 移動の説明
		switch (nowKey)
		{
			// 左に移動-------------------------------------------------------------------------
			case 0:
				// 移動してフェードアウト
				if (returnFlag == false)
				{
					if (desMCursor.transform.position.x > desMCursorOldVec.x - 1)
					{
						desMCursor.transform.position -= new Vector3(0.03f, 0, 0);
					}
					else
					{
						//desMCursor.transform.position = new Vector3(desMCursorOldVec.x - 1, desMCursorOldVec.y, desMCursorOldVec.z);
						DesKeyFade(true, desMCursor, desMCursorOldVec, nowDes);
					}
				}
				// 元に戻す
				else
				{
					DesKeyFade(false, desMCursor, desMCursorOldVec, nowDes);
				}
			break;

			// 上に移動--------------------------------------------------------------------------
			case 1:
				// 移動してフェードアウト
				if (returnFlag == false)
				{
					if (desMCursor.transform.position.y < desMCursorOldVec.y + 1)
					{
						desMCursor.transform.position += new Vector3(0, 0.03f, 0);
					}
					else
					{
						//desMCursor.transform.position = new Vector3(desMCursorOldVec.x, desMCursorOldVec.y + 1, desMCursorOldVec.z);
						DesKeyFade(true, desMCursor, desMCursorOldVec, nowDes);
					}
				}
				// 元に戻す
				else
				{
					DesKeyFade(false, desMCursor, desMCursorOldVec, nowDes);
				}
			break;

			// 下に移動--------------------------------------------------------------------------
			case 2:
				// 移動してフェードアウト
				if (returnFlag == false)
				{
					if (desMCursor.transform.position.y > desMCursorOldVec.y - 1)
					{
						desMCursor.transform.position -= new Vector3(0, 0.03f, 0);
					}
					else
					{
						//desMCursor.transform.position = new Vector3(desMCursorOldVec.x, desMCursorOldVec.y - 1, desMCursorOldVec.z);
						DesKeyFade(true, desMCursor, desMCursorOldVec, nowDes);
					}
				}
				// 元に戻す
				else
				{
					DesKeyFade(false, desMCursor, desMCursorOldVec, nowDes);
				}
			break;

			// 右に移動-------------------------------------------------------------------------
			case 3:
				// 移動してフェードアウト
				if (returnFlag == false)
				{
					if (desMCursor.transform.position.x < desMCursorOldVec.x + 1)
					{
						desMCursor.transform.position += new Vector3(0.03f, 0, 0);
					}
					else
					{
						//desMCursor.transform.position = new Vector3(desMCursorOldVec.x + 1, desMCursorOldVec.y, desMCursorOldVec.z);
						DesKeyFade(true, desMCursor, desMCursorOldVec, nowDes);
					}
				}
				// 元に戻す
				else
				{
					DesKeyFade(false, desMCursor, desMCursorOldVec, nowDes);
				}
			break;
		}
	}

	// 撮影----------------------------------------------------------------------------------------------------------------------
	private void DesPhoto()
	{
		// 初期化
		if (resetFlag == true)
		{
			desMoveSet.SetActive(false);
			desPhotoSet.SetActive(true);
			desChangeSet.SetActive(false);
			desAttackSet.SetActive(false);
			desMainTexts[0].text = "2";
			desMainTexts[1].text = "オバケの撮影";
			keyPColor.enabled = false;
			resetFlag = false;
		}

		// メモリの表示リセット
		if (memoryCnt == 0)
		{
			for(int i = 0; i < 3; ++i)
			{
				desPMemorys[i].enabled = false;
			}
		}

		// 移動してフェードアウト
		if (returnFlag == false)
		{
			// カーソルの移動
			if (desPCursor.transform.position.y <= desPGhost.transform.position.y)
			{
				desPCursor.transform.position += new Vector3(0, 0.03f, 0);
			}
			else if (desPCursor.transform.position.x <= desPGhost.transform.position.x)
			{
				desPCursor.transform.position += new Vector3(0.03f, 0, 0);
			}
			// 移動しきったら
			else
			{
				keyPColor.enabled = true;
				// オバケを消す＋メモリを増やす
				if (deletePFlag == false)
				{
					desPGhost.enabled = false;
					Instantiate(deletePrefab, desPGhost.transform.position, Quaternion.identity);
					++memoryCnt;
					if (memoryCnt >= 4)
					{
						memoryCnt = 1;
					}
					for (int i = 0; i < 3; ++i)
					{
						desPMemorys[i].enabled = false;
						desPConcentrationTexts[i].enabled = true;
						desPGaugeArrow.enabled = true;
						if (i < memoryCnt)
						{
							desPMemorys[i].enabled = true;
						}
					}
					deletePFlag = true;
				}
				// メモリの点滅やオバケを再表示させる準備
				else
				{
					++desDelay;
					if (desDelay == 1)						// オバケを再表示させる準備
					{
						desPGhost.enabled = true;
						desPGhost.color = new Color(desPGhost.color.r, desPGhost.color.g, desPGhost.color.b, 0);
					}
					else if (desDelay == 10)
					{
						for (int i = 0; i < 3; ++i)			// 以下集中線などの点滅
						{
							desPConcentrationTexts[i].enabled = false;
						}
					}
					else if (desDelay == 15)
					{
						for (int i = 0; i < 3; ++i)
						{
							desPConcentrationTexts[i].enabled = true;
						}
					}
					else if (desDelay == 25)
					{
						for (int i = 0; i < 3; ++i)
						{
							desPConcentrationTexts[i].enabled = false;
						}
					}
					else if (desDelay == 30)
					{
						for (int i = 0; i < 3; ++i)
						{
							desPConcentrationTexts[i].enabled = true;
						}
					}
					else if (desDelay == 40)
					{
						for (int i = 0; i < 3; ++i)
						{
							desPConcentrationTexts[i].enabled = false;
						}
					}
					else if (desDelay <= 60)				// オバケを再表示させる(関数内で元に戻せるようにフラグを変更)
					{
						DesKeyFade(true, desPCursor, desPCursorOldVec, nowDes);
					}
				}
			}
		}
		// 元に戻す
		else
		{
			DesKeyFade(false, desPCursor, desPCursorOldVec, nowDes);
			keyPColor.enabled = false;
			deletePFlag = false;
			desDelay = 0;
			for (int i = 0; i < 3; ++i)
			{
				desPGaugeArrow.enabled = false;
				desPConcentrationTexts[i].enabled = false;
			}
		}
	}

	// カーソル切り替え----------------------------------------------------------------------------------------------------------
	private void DesChange()
	{
		// 初期化
		if (resetFlag == true)
		{
			desMoveSet.SetActive(false);
			desPhotoSet.SetActive(false);
			desChangeSet.SetActive(true);
			desAttackSet.SetActive(false);
			desMainTexts[0].text = "3";
			desMainTexts[1].text = "カーソルの切り替え";
			resetFlag = false;
		}

		// 雑魚を震えさせる(攻撃のカーソルの時のみ)
		if (desCCursor.sprite.name == desCcursorImgs[1].name)
		{
			if (one == true)
			{
				sweatPer.Play();
				one = false;
			}
			if (zakoTremble == true)
			{
				desCZako.transform.position -= new Vector3(0.05f, 0, 0);
				if (desCZako.transform.position.x <= desCZakoOldVec.x - 0.1f)
				{
					zakoTremble = false;
				}
			}
			else
			{
				desCZako.transform.position += new Vector3(0.05f, 0, 0);
				if (desCZako.transform.position.x >= desCZakoOldVec.x + 0.1f)
				{
					zakoTremble = true;
				}
			}
		}
		// ザコ敵を止める、汗のパーティクルを停止
		else
		{
			desCZako.transform.position = desCZakoOldVec;
			sweatPer.Stop();
			one = true;
		}

		// 一定フレーム後カーソルの種類を変更させる
		++desDelay;
		if (desDelay == 60)
		{
			if (desCCursor.sprite.name == desCcursorImgs[0].name)
			{
				desCCursor.sprite = desCcursorImgs[1];
			}
			else
			{
				desCCursor.sprite = desCcursorImgs[0];
			}
			keyCColor.enabled = true;
		}
		else if (desDelay == 120)
		{
			keyCColor.color = new Color(keyCColor.color.r, keyCColor.color.g, keyCColor.color.b, 1);
			keyCColor.enabled = false;
			desDelay = 0;
		}
	}

	// 攻撃----------------------------------------------------------------------------------------------------------------------
	private void DesAttack()
	{
		// 初期化
		if (resetFlag == true)
		{
			desMoveSet.SetActive(false);
			desPhotoSet.SetActive(false);
			desChangeSet.SetActive(false);
			desAttackSet.SetActive(true);
			desMainTexts[0].text = "4";
			desMainTexts[1].text = "敵への攻撃";
			resetFlag = false;
		}

		// 移動してフェードアウト
		if (returnFlag == false)
		{
			// カーソルの移動
			if (desACursor.transform.position.y <= desAZako.transform.position.y)
			{
				desACursor.transform.position += new Vector3(0, 0.03f, 0);
			}
			else if (desACursor.transform.position.x <= desAZako.transform.position.x)
			{
				desACursor.transform.position += new Vector3(0.03f, 0, 0);
			}
			// 移動しきったら
			else
			{
				// ザコ敵の表情をやられに変更、一定フレーム後消す＋メモリを減らす
				keyAColor.enabled = true;
				if (deleteAFlag == false)
				{
					desAZakos[2].sprite = desAZakoBeatens[1];
					++desABeatenCnt;
					if (desABeatenCnt >= 20)
					{
						for (int i = 0; i < 3; ++i)
						{
							desAZakos[i].enabled = false;
						}
						--memoryACnt;
						if (memoryACnt <= 0)
						{
							memoryACnt = 3;
						}
						for (int i = 0; i < 3; ++i)
						{
							desAMemorys[i].enabled = false;
							desAConcentrationTexts[i].enabled = true;
							desAGaugeArrow.enabled = true;
							if (i < memoryACnt)
							{
								desAMemorys[i].enabled = true;
							}
						}
						deleteAFlag = true;
						desABeatenCnt = 0;
					}
				}
				// メモリの点滅やザコ敵を再表示させる準備
				else
				{
					++desDelay;
					if (desDelay == 1)					// ザコ敵を再表示させる準備
					{
						desAZakos[2].sprite = desAZakoBeatens[0];
						for (int i = 0; i < 3; ++i)
						{
							desAZakos[i].enabled = true;
							desAZakos[i].color = new Color(desAZakos[i].color.r, desAZakos[i].color.g, desAZakos[i].color.b, 0);
						}
					}
					else if (desDelay == 10)			// 以下集中線などの点滅
					{
						for (int i = 0; i < 3; ++i)
						{
							desAConcentrationTexts[i].enabled = false;
						}
					}
					else if (desDelay == 15)
					{
						for (int i = 0; i < 3; ++i)
						{
							desAConcentrationTexts[i].enabled = true;
						}
					}
					else if (desDelay == 25)
					{
						for (int i = 0; i < 3; ++i)
						{
							desAConcentrationTexts[i].enabled = false;
						}
					}
					else if (desDelay == 30)
					{
						for (int i = 0; i < 3; ++i)
						{
							desAConcentrationTexts[i].enabled = true;
						}
					}
					else if (desDelay == 40)
					{
						for (int i = 0; i < 3; ++i)
						{
							desAConcentrationTexts[i].enabled = false;
						}
					}
					else if (desDelay <= 60)				// ザコ敵を再表示させる(関数内で元に戻せるようにフラグを変更)
					{
						DesKeyFade(true, desACursor, desACursorOldVec, nowDes);
					}
				}
			}
		}
		// 元に戻す
		else
		{
			DesKeyFade(false, desACursor, desACursorOldVec, nowDes);
			keyAColor.enabled = false;
			deleteAFlag = false;
			desDelay = 0;
			for(int i = 0; i < 3; ++i)
			{
				desAGaugeArrow.enabled = false;
				desAConcentrationTexts[i].enabled = false;
			}
		}
	}

	// カーソルのフェードアウトとイン--------------------------------------------------------------------------------------------
	private void DesKeyFade(bool Flag, Image Cursor, Vector3 OldVec, int Des)
	{
		// カーソルのフェードアウト
		if (Flag == true)
		{
			Cursor.color -= new Color(0, 0, 0, 0.05f);
			//keyColors[nowKey].color -= new Color(0, 0, 0, 0.05f);
			if (Cursor.color.a <= 0)
			{
				Cursor.transform.position = OldVec;
				returnFlag = true;
				if (Des == 0)
				{
					++nextCnt;
					// キーの変化
					if (nextCnt > 1)
					{
						++nowKey;
						if (nowKey >= 4)
						{
							nowKey = 0;
						}
						nextCnt = 0;
					}
				}
			}
		}
		// カーソルのフェードイン(オバケやザコ敵のフェードインもセットで)
		else
		{
			Cursor.color += new Color(0, 0, 0, 0.05f);
			//keyColors[nowKey].color += new Color(0, 0, 0, 0.05f);
			if (Cursor.color.a >= 1 && Des != 1 && Des != 3)
			{
				returnFlag = false;
			}
			if (Des == 1)
			{
				desPGhost.color += new Color(0, 0, 0, 0.03f);
				if (desPGhost.color.a >= 1)
				{
					//DesKeyFade(true, desPCursor, desPCursorOldVec, nowDes);
					returnFlag = false;
				}
			}
			if (Des == 3)
			{
				for (int i = 0; i < 3; ++i)
				{
					desAZakos[i].color += new Color(0, 0, 0, 0.03f);
					if (desAZakos[i].color.a >= 1)
					{
						returnFlag = false;
					}
				}
			}
		}
	}
}

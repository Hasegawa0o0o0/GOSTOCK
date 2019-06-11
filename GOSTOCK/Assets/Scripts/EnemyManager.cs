/*
 * EnemyManager.cs
 * 2018.10.13 作成
 *		2018.10.14 ザコ親追加
 *		2018.10.20 引数にPlayerColtrolを追加
 *				   ボスの追加
 *				   ザコ親からラウンドを進める
 *				   エラー防止の修正
 *		2018.11.02 演出
 *		2018.11.08 ボスのデバッグアクションを追加
 *		2018.12.02 出現方法の変更と演出追加
 *		2018.12.04 中ボス出現の演出追加
 *		2018.12.08 ボス出現の演出追加
 *		2018.12.20 ザコの体当たりの追加
 *		2018.12.24 エラー防止追加
 *		2018.12.27 ポーズ中は動かないように修正
 *		2018.12.31 ザコのリベンジ
 *		2019.01.03 時計が壊されたら動きを遅くする(失敗)
 *		2019.01.04 時計が壊されたら動きを遅くする(成功)
 *		2019.01.16 プレイイヤーのゲージがたまった時に初めて開始する
 *		2019.01.31 ザコを重ならないようにする
 *		2019.02.21 ボスの入場音
 *		2019.03.02 ボスの出現中はプレイヤーを操作できないようにする
 *		2019.03.10 ボスがやられたことがわかるようにする
 *		2019.03.16 ボスでBGMを変える
 *		2019.04.03 ボスの登場でも背景を暗くする
 */
/* エネミー全形態の管理(主にラウンド) */
using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
	public static bool isOnParticle = true;
	public bool isDebug = false;
	// 対戦するエネミーのラウンド
	// 0:ザコ共、1:ザコ親、2:ボス
	public int enemyRound = 0;
	public GameObject blackholeObject;					// ブラックホール 2018.12.02
	bool isPlayerPowerLow = true;						// プレイヤーの力がたまったかどうか 2019.01.16
	public static bool bossDefeat;						// ボスが倒されたかどうか 2019.03.10
	public static bool bossStaging;						// ボスが出現している最中か 2019.04.03

	int zakoRound = 0;												// ザコのラウンド 2019.01.16
	public GameObject zakoPrefab;									// ザコのプレハブ
	List<ZakoAction> zakoObj = new List<ZakoAction>();				// ザコたち(最大10) リストにしました 2018.12.02
	List<Vector2Int> zakoTargetPosList = new List<Vector2Int>();	// ザコたちが目指す位置 2019.01.31
	bool isZakoAttacking = false;									// 誰かザコが体当たりをしているか 2018.12.20
	int attackingZakoNum = 0;										// 体当たりしているザコの要素番号 2018.12.20
	GameObject playerCamera;										// プレイヤーカメラの位置 2018.12.20
	float shake;													// 縦の揺れ幅
	float addShake = 0.0005f;										//
	float zakoInsCheckTime = 0f;									// ザコの生成タイミングを記録する 2019.01.16
	Vector3[] zakoTargetPos = new Vector3[10]						// ザコたちが目指す位置一覧(ワールド座標)
		{
			new Vector3(-8.4f - 33.0f,5.5f,2.6f - 10.0f),
			new Vector3(-4.2f - 33.0f,5.5f,2.6f - 10.0f),
			new Vector3(-33.0f,5.5f,2.6f - 10.0f),
			new Vector3(4.2f - 33.0f,5.5f,2.6f - 10.0f),
			new Vector3(8.4f - 33.0f,5.5f,2.6f - 10.0f),
			new Vector3(-8.4f - 33.0f,5.5f,-2.6f - 10.0f),
			new Vector3(-4.2f - 33.0f,5.5f,-2.6f - 10.0f),
			new Vector3(-33.0f,5.5f,-2.6f - 10.0f),
			new Vector3(4.2f - 33.0f,5.5f,-2.6f - 10.0f),
			new Vector3(8.4f - 33.0f,5.5f,-2.6f - 10.0f),
		};
	bool isZakoRevenge = false;					// ザコがリベンジしたか 2018.12.31

	public GameObject zakoParentPrefab;         // ザコ親のプレハブ
	ZakoParentAction zakoParentObj;             // ザコ親

	// 2018.10.20
	public GameObject bossPrefab;							// ボスプレハブ
	BossAction bossObj;										// ボス
	int blinkFrame = 0;										// 点滅のために使う 2018.11.02	2019.02.12
	int bossSlowFrame = 0;									// 動きを遅くするのに使う 2019.01.03
	List<ZakoAction> stagingZako = new List<ZakoAction>();	// ボス出現の演出用ザコ 2018.12.08	型をGameObject -> ZakoAction 2019.04.03
	List<float> inholeSpeedList = new List<float>();		// 吸い込みスピード 2018.12.08
	GameObject stagingBlackhole;							// ボス出現の演出用ブラックホール 2018.12.08

	AudioSource masterAudioSource;		// GameMasterのAudioSource 2019.03.16
	public AudioClip prelude;			// ボスが出てくるときの音 2019.03.16
	public AudioClip bossBGM;			// ボス戦BGM 2019.03.16

	void Start ()
	{
		// ボスが負けていない状態にする 2019.03.10
		bossDefeat = false;
		bossStaging = false;
		// 2018.11.02
		blinkFrame = 120;
		// 2018.12.02
		blackholeObject.transform.localScale = Vector3.zero;
		// 2018.12.20
		playerCamera = GameObject.Find("Camera02");
		// 2019.01.16
		zakoRound = 0;
		zakoInsCheckTime = 0f;
		// 2019.03.16
		masterAudioSource = FindObjectOfType<MainBGM>().audioSource;
	}

	void Update ()
	{
		// デバッグ用-------------------------------------------
		if (isDebug)
		{
			if (Input.GetKeyDown(KeyCode.F))
			{
				enemyRound++;
				// 2018.11.15
				if (enemyRound == 1 && zakoParentObj && !zakoParentObj.isActiveAndEnabled)
				{
					zakoParentObj.gameObject.SetActive(true);
					zakoParentObj.hp = 3;
				}
				else if (enemyRound >= 2 && bossObj && !bossObj.isActiveAndEnabled)
				{
					bossObj.gameObject.SetActive(true);
				}
				else if (enemyRound < 2 && bossObj)
				{
					bossObj.gameObject.SetActive(false);
				}
			}
			// 2018.11.15
			if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.F))
			{
				enemyRound = 0;
				zakoRound = 0;  // 2019.01.16
				for (int i = 0; i < zakoObj.Count; ++i)
				{
					if (!zakoObj[i].isActiveAndEnabled)
					{
						zakoObj[i].gameObject.SetActive(true);
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
			{
				isOnParticle = !isOnParticle;
			}
			// 2018.11.08
			if (bossObj)
			{
				bossObj.DebugAction();
				// 2018.11.15
				if (enemyRound < 2)
				{
					bossObj.gameObject.SetActive(false);
				}
			}
		}
		//-----------------------------------------------------
		// 2018.12.27
		if (Time.timeScale == 0) { return; }
		// ブラックホールを回しておく 2018.12.02
		blackholeObject.transform.eulerAngles -= new Vector3(0.0f, 0.0f, 2.0f);
		if (blackholeObject.transform.eulerAngles.z < -360)
		{
			blackholeObject.transform.eulerAngles += new Vector3(0.0f, 0.0f, 360.0f);
		}
	}

	// 2018.10.20
	public void EnemyControl(PlayerControl playerControl, bool clockworkFlag)
	{
		// プレイヤーのゲージがはじめて貯まるまで始めない 2019.01.16
		if (playerControl.power >= 5 && isPlayerPowerLow)
		{
			isPlayerPowerLow = false;
			zakoRound = 1;
		}
		if (isPlayerPowerLow) { return; }
		// 2018.12.27
		if (Time.timeScale == 0) { return; }
		// ザコ----------------------------------------------------------------
		if (enemyRound == 0)
		{
			// 処理をかなり変更 2019.01.16
			if (zakoObj.Count != 0)
			{
				// 操作を関数に 2018.12.02 引数追加 2018.12.20
				ManipulateZako2(playerControl, playerCamera);
			}
			// ザコの生成
			if (zakoRound == 1 && zakoObj.Count == 0)
			{
				// ブラックホールを広げる
				if (blackholeObject.transform.localScale.x <= 1.0f)
				{
					blackholeObject.transform.localScale = Vector3.Lerp(blackholeObject.transform.localScale,
						Vector3.one * 1.005f, 2.5f * Time.deltaTime);
				}
				// 広がったら生成 2018.12.02
				else
				{
					InsZako2(1);
				}
			}
			else if (zakoRound == 2 && zakoObj.Count < 2)
			{
				// ブラックホールを広げる
				if (blackholeObject.transform.localScale.x <= 1.0f)
				{
					blackholeObject.transform.localScale = Vector3.Lerp(blackholeObject.transform.localScale,
						Vector3.one * 1.005f, 2.5f * Time.deltaTime);
				}
				// 広がり、タイミングをずらして生成 2018.12.02
				else if (Time.time > zakoInsCheckTime + 2f)
				{
					InsZako2(1);
				}
			}
			else if (zakoRound == 3 && zakoObj.Count < 5)
			{
				// ブラックホールを広げる
				if (blackholeObject.transform.localScale.x <= 1.0f)
				{
					blackholeObject.transform.localScale = Vector3.Lerp(blackholeObject.transform.localScale,
						Vector3.one * 1.005f, 2.5f * Time.deltaTime);
				}
				// 広がり、タイミングをずらして生成 2018.12.02
				else if (Time.time > zakoInsCheckTime + 0.5f)
				{
					InsZako2(1);
				}
			}
			else if (zakoRound == 4)
			{
				enemyRound = 1;
			}
			// ブラックホールを縮小する 2019.01.16
			else
			{
				Shrink(blackholeObject);
			}
		}
		// ザコ親 2018.10.14--------------------------------------------------------
		else if (enemyRound == 1)
		{
			// ブラックホールを広げる 2018.12.04
			if (blackholeObject.transform.localScale.x <= 1.0f && zakoParentObj == null)
			{
				blackholeObject.transform.localScale = Vector3.Lerp(blackholeObject.transform.localScale,
					Vector3.one * 1.005f, 2.5f * Time.deltaTime);
			}
			// 広がったら生成 2018.12.02
			else if (zakoParentObj == null)
			{
				zakoParentObj = Instantiate(zakoParentPrefab, blackholeObject.transform.position, zakoParentPrefab.transform.localRotation)
					.GetComponent<ZakoParentAction>();
			}
			// 2018.10.20 変更 2018.12.04
			else
			{
				// 2018.12.20
				zakoParentObj.Action(playerControl, playerCamera);
				// ブラックホールを縮める 2018.12.04
				Shrink(blackholeObject);
				// ザコ親が倒されたらラウンドを進める 2018.10.20 場所をelse文の中にした
				if (!zakoParentObj.isActiveAndEnabled)
				{
					enemyRound = 2;
				}
			}
		}
		// ボス 2018.10.20------------------------------------------------------------------
		else if (enemyRound >= 2)
		{
			// 点滅する 2018.11.02 ブラックホールと点滅する 2018.12.08
			if (bossObj && blinkFrame++ < 120)
			{
				if (blinkFrame < 85)
				{
					if (blinkFrame % 6 < 3)
					{
						bossObj.gameObject.SetActive(false);
						stagingBlackhole.SetActive(true);   // 2018.12.08
					}
					else
					{
						bossObj.gameObject.SetActive(true);
						stagingBlackhole.SetActive(false);  // 2018.12.08
					}
				}
				// 手を広げる
				else
				{
					// 音 2019.02.21
					if (blinkFrame == 85)
					{
						EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossEntering);
						// プレイヤーを操作可能にする 2019.03.02
						// 2019.04.03
						GameMaster.isOperationPlayer = true;
						bossStaging = false;
					}
					bossObj.gameObject.SetActive(true);
					stagingBlackhole.SetActive(false);      // 2018.12.08
					bossObj.MoveReturn();
					bossObj.SetParticle(false);
				}
			}
			// ボスの処理
			else if (bossObj && bossObj.isActiveAndEnabled)
			{
				if(!masterAudioSource.isPlaying)
				{
					masterAudioSource.Play();
				}
				// BGMを変える 2019.03.16
				if (masterAudioSource.clip == bossBGM && !bossDefeat)
				{
					masterAudioSource.volume += 0.05f;
				}
				else
				{
					masterAudioSource.volume -= 0.05f;
					if (masterAudioSource.volume <= 0f) { masterAudioSource.clip = bossBGM; }
				}
				// 引数追加 2019.01.04
				bossObj.Action(playerControl, clockworkFlag);
			}
			// ボスを出すブラックホールを出す 2018.12.08
			else if (!bossObj && !stagingBlackhole)
			{
				// 2019.04.03
				bossStaging = true;
				// プレイヤーを操作不可にする 2019.03.02
				GameMaster.isOperationPlayer = false;
				stagingBlackhole = Instantiate(blackholeObject, blackholeObject.transform.position, blackholeObject.transform.localRotation);
				// 吸い込むザコの生成
				for (int i = 0; i < 10; ++i)
				{
					Vector3 insPos = new Vector3(Random.Range(-40.0f, -25.0f),
						Random.Range(-4.0f, 2.5f), Random.Range(-22.0f, -16.0f));
					stagingZako.Add(Instantiate(zakoPrefab, insPos, zakoPrefab.transform.localRotation).GetComponent<ZakoAction>());
					inholeSpeedList.Add(Random.Range(2.0f, 5.0f));
				}
			}
			// 画面外から吸い込む 2018.12.08
			else if (!bossObj && stagingBlackhole && stagingZako.Count != 0)
			{
				if (!masterAudioSource.isPlaying)
				{
					masterAudioSource.Play();
				}
				// BGMを変える 2019.03.16
				if (masterAudioSource.clip == prelude)
				{
					masterAudioSource.volume += 0.05f;
					if (masterAudioSource.volume > 0.7f) { masterAudioSource.volume = 0.7f; }
				}
				else
				{
					masterAudioSource.volume -= 0.05f;
					if (masterAudioSource.volume <= 0f) { masterAudioSource.clip = prelude; }
				}
				int removeNum = 10;
				for (int i = 0; i < stagingZako.Count; ++i)
				{
					// 下に加速
					stagingZako[i].transform.position += new Vector3(0.0f, 0.0f,
						(stagingBlackhole.transform.position.z - stagingZako[i].transform.position.z) / 5.0f);
					// アニメーションをさせる 2019.04.03
					stagingZako[i].ZakoAnimation();
					// ブラックホールに向かう
					stagingZako[i].transform.position = Vector3.MoveTowards(stagingZako[i].transform.position, stagingBlackhole.transform.position,
						inholeSpeedList[i] * Time.deltaTime);
					if (IsNear(stagingZako[i].transform.position, stagingBlackhole.transform.position, 0.2f))
					{
						removeNum = i;
					}
				}
				// ザコが近づいたら消す
				if (removeNum < 10)
				{
					ZakoAction removeObj = stagingZako[removeNum];
					stagingZako.RemoveAt(removeNum);
					inholeSpeedList.RemoveAt(removeNum);
					Destroy(removeObj.gameObject);
				}
				// 吸い込んだ数に応じて大きくする
				stagingBlackhole.transform.localScale = Vector3.Lerp(stagingBlackhole.transform.localScale,
					new Vector3(7.0f, 7.0f, 7.0f) / (stagingZako.Count + 1), 5.0f * Time.deltaTime);
				// ボスの位置に移動する
				stagingBlackhole.transform.position = Vector3.MoveTowards(stagingBlackhole.transform.position,
					bossPrefab.transform.position, 2.0f * Time.deltaTime);
			}
			// ボスの生成
			else if (!bossObj && stagingBlackhole && stagingZako.Count == 0)
			{
				bossObj = Instantiate(bossPrefab, bossPrefab.transform.position, bossPrefab.transform.localRotation)
					.GetComponent<BossAction>();
				// ブラックホールの設定 2018.12.04
				bossObj.blackholeObject = Instantiate(blackholeObject, Vector3.zero, blackholeObject.transform.localRotation);
				blinkFrame = 0;
			}
			// ボスを生成するブラックホールを回しておく 2018.12.08
			if (stagingBlackhole)
			{
				stagingBlackhole.transform.eulerAngles -= new Vector3(0.0f, 0.0f, 2.0f);
				if (stagingBlackhole.transform.eulerAngles.z < -360)
				{
					stagingBlackhole.transform.eulerAngles += new Vector3(0.0f, 0.0f, 360.0f);
				}
			}
		}
	}

	// ザコの生成----------------------------------------------------------
	void InsZako()
	{
		// ザコの生成
		for (int i = 0; i < 10; ++i)
		{
			// 1/3の確率か、六回目までに3体生成されていなかったら、生成 2018.12.02
			if (Random.Range(0, 2) == 0 || (i > 6 && zakoObj.Count < 3))
			{
				ZakoAction za = Instantiate(zakoPrefab, blackholeObject.transform.position, zakoPrefab.transform.localRotation)
					.GetComponent<ZakoAction>();
				zakoObj.Add(za);
				za.targetPos = zakoTargetPos[i];
			}
		}
	}

	// 新しいザコの生成 2019.01.16------------------------------------------
	void InsZako2(int zakoNum)
	{
		// 引数で渡された分だけ生成する
		for (int i = 0; i < zakoNum && zakoObj.Count < 21; ++i)
		{
			ZakoAction za = Instantiate(zakoPrefab, blackholeObject.transform.position, zakoPrefab.transform.localRotation)
				.GetComponent<ZakoAction>();
			zakoObj.Add(za);
			// ターゲットの設定
			Vector2Int targetPos = Vector2Int.zero;
			targetPos.x = Random.Range(-3, 3 + 1) * 2 - 33;
			targetPos.y = Random.Range(-5, -3 + 1) * 2 - 1;
			// ザコが重ならないようにする
			while (zakoTargetPosList.Contains(targetPos))
			{
				targetPos.x = Random.Range(-3, 3 + 1) * 2 - 33;
				targetPos.y = Random.Range(-5, -3 + 1) * 2 - 1;
			}
			za.targetPos = new Vector3(targetPos.x, Random.Range(4.5f, 5.5f), targetPos.y);
			zakoTargetPosList.Add(targetPos);
		}
		zakoInsCheckTime = Time.time;
	}

	// ザコの操作 引数を追加 2018.12.20----------------------------------------
	void ManipulateZako(PlayerControl playerControl, GameObject playerCamera)
	{
		// 確率で一人の行動を変える 2018.12.20
		if (!isZakoAttacking && Random.Range(0, 128) == 0)
		{
			SwitchZakoAction();
		}
		for (int i = 0, cnt = 0; i < zakoObj.Count; ++i)
		{
			// オブジェクトがアクティブで有効なときは操作してカウント 2018.10.14
			if (zakoObj[i].isActiveAndEnabled)
			{
				// 普通のアクションとそうでないアクションで操作を分ける 2018.12.20 条件追加 2018.12.24
				if (isZakoAttacking && i == attackingZakoNum)
				{
					isZakoAttacking = !zakoObj[attackingZakoNum].Attack(playerControl, playerCamera);
				}
				// 攻撃しているオブジェクトがアクティブでなくなったらフラグをfalse 2018.12.24
				else if (!zakoObj[attackingZakoNum].isActiveAndEnabled)
				{
					isZakoAttacking = false;
				}
				if (!isZakoAttacking || i != attackingZakoNum)
				{
					zakoObj[i].NormalAction();
					// ザコを揺らす 2018.11.02
					shake -= addShake;
					if (Mathf.Abs(shake) > 0.02f) { addShake *= -1; }
					zakoObj[i].transform.position += new Vector3(0, 0, shake);
				}
				++cnt;
			}
			// 全て倒されたらラウンドを進める リストの影響 2018.12.02 条件追加 2018.12.31
			if (i + 1 == zakoObj.Count && cnt == 0 && isZakoRevenge)
			{
				enemyRound = 1;
			}
			// ザコのリベンジ 2018.12.31
			else if (i + 1 == zakoObj.Count && cnt == 0)
			{
				// 目指す位置をランダムに入れ替え
				ReplaceZakoTargetPos();
				// 再度アクティブにする
				for (int j = 0; j < zakoObj.Count; ++j)
				{
					if (!zakoObj[j].isActiveAndEnabled)
					{
						zakoObj[j].gameObject.SetActive(true);
					}
				}
				// フラグオン
				isZakoRevenge = true;
			}
		}
	}

	// 新しいザコの操作 2019.01.16--------------------------------------------------------------------
	void ManipulateZako2(PlayerControl playerControl, GameObject playerCamera)
	{
		blinkFrame++;
		// 確率で一人の行動を変える 2018.12.20 条件追加 2019.01.16
		if (!isZakoAttacking && Random.Range(0, 128) == 0)
		{
			SwitchZakoAction();
		}
		for (int i = 0, cnt = 0; i < zakoObj.Count; ++i)
		{
			// オブジェクトがアクティブで有効なときは操作してカウント 2018.10.14
			if (zakoObj[i].isActiveAndEnabled)
			{
				// 普通のアクションとそうでないアクションで操作を分ける 2018.12.20 条件追加 2018.12.24
				if (isZakoAttacking && i == attackingZakoNum)
				{
					isZakoAttacking = !zakoObj[attackingZakoNum].Attack(playerControl, playerCamera);
				}
				// 攻撃しているオブジェクトがアクティブでなくなったらフラグをfalse 2018.12.24
				else if (!zakoObj[attackingZakoNum].isActiveAndEnabled)
				{
					isZakoAttacking = false;
				}
				if (!isZakoAttacking || i != attackingZakoNum)
				{
					zakoObj[i].NormalAction();
					// ザコを揺らす 2018.11.02	2019.02.12
					if (!zakoObj[i].isInvincible)
					{
						shake -= addShake;
						if (Mathf.Abs(shake) > 0.02f) { addShake *= -1; }
						zakoObj[i].transform.position += new Vector3(0, 0, shake);
					}
				}
				if (zakoObj[i].isInvincible)
				{
					if (blinkFrame % 6 < 3)
					{
						zakoObj[i].SetSpriteEnabled(false);
					}
					else
					{
						zakoObj[i].SetSpriteEnabled(true);
					}
				}
				else
				{
					zakoObj[i].SetSpriteEnabled(true);
				}
				++cnt;
			}
			// ザコがすべてアクティブでなければ全て削除
			if (i + 1 == zakoObj.Count && cnt == 0)
			{
				ClearZakoObj();
				attackingZakoNum = 0;
				++zakoRound;
				break;
			}
		}
	}

	// ザコをすべて開放する 2019.01.16---------------------------------------------------
	void ClearZakoObj()
	{
		for (int i = 0; i < zakoObj.Count; ++i)
		{
			GameObject o = zakoObj[i].gameObject;
			Destroy(o);
		}
		zakoObj.Clear();
		zakoTargetPosList.Clear();
		isZakoAttacking = false;
		attackingZakoNum = 0;
	}

	// ザコたちが目指す位置をランダムに入れ替える 2018.12.31---------------------------------------------
	void ReplaceZakoTargetPos()
	{
		List<int> previouslyElemNum = new List<int>();	// 既出の要素番号
		for (int i = 0; i < zakoObj.Count; ++i)
		{
			int elem = Random.Range(0, 10);
			// 既出ならば番号を変える
			while(previouslyElemNum.Contains(elem))
			{
				elem = elem + 1 >= 10 ? 0 : elem + 1;
			}
			// それぞれ反映
			previouslyElemNum.Add(elem);
			zakoObj[i].targetPos = zakoTargetPos[elem];
		}
	}

	// ザコの状態を体当たりにする 2018.12.20--------------------------
	void SwitchZakoAction()
	{
		isZakoAttacking = true;
		int cnt = 0;
		attackingZakoNum = Random.Range(0, zakoObj.Count);
		for (; !zakoObj[attackingZakoNum].isActiveAndEnabled || zakoObj[attackingZakoNum].isInvincible; ++attackingZakoNum)
		{
			// エラー防止 2018.12.27
			if (attackingZakoNum + 1 >= zakoObj.Count) { attackingZakoNum = -1; }
			++cnt;
			if (cnt == zakoObj.Count)
			{
				isZakoAttacking = false;
				attackingZakoNum = 0;
				break;
			}
		}
	}

	// ゲームオブジェクトを縮小する-----------------------------------
	void Shrink(GameObject shrinkObj)
	{
		if (shrinkObj.transform.localScale.x > 0.0f)
		{
			shrinkObj.transform.localScale = Vector3.Lerp(shrinkObj.transform.localScale,
				Vector3.zero, 1.0f * Time.deltaTime);
			if (shrinkObj.transform.localScale.x < 0.0f)
			{
				shrinkObj.transform.localScale = Vector3.zero;
			}
		}
	}
	// BossAction.cs からコピー 2018.12.08 -------------------------------------------------------
	bool IsNear(Vector3 a, Vector3 b, float value)
	{
		return Mathf.Abs(a.x - b.x) < value && Mathf.Abs(a.y - b.y) < value && Mathf.Abs(a.z - b.z) < value;
	}
}

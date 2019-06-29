/*
 * ZaokParentAction.cs
 * 2018.10.14 作成
 *		2018.12.01 ザコ放出の追加(のためのいろいろ)
 *		2018.12.20 ザコの体当たりを追加
 *		2018.12.25 数値いじり
 *		2019.01.08 音を鳴らす
 *		2019.01.15 アニメーション追加
 *		2019.01.19 色々な処理を追加
 *		2019.01.26 ガード、カウンター（ザコ生成）追加
 *		2019.02.03 ザコの位置調整
 *		2019.02.20 消えるときのパーティクルを追加
 *		2019.03.02 おばけを吸い込むときにスプライトを変える
 *		2019.03.16 吸い込むときにおばけを震わせる
 *		2019.03.17 スプライトを変えた
 *		2019.03.19 目パチのタイミングをずらす
 *		2019.03.20 本を追加
 *		2019.03.22 ガードを本に変更
 *		2019.03.26 SE追加
 *		2019.03.30 召喚、吸い込み中は弾をすり抜けるように
 *				   カウンターのバグ修正
 *				   行動の割合調整
 *				   HP変更
 *				   本の位置調整
 *		2019.04.02 オーラ付きの本を追加、各行動ごとに色を変更
 *		2019.04.03 不自然な動作を修正
 *		2019.04.06 微調整
 *		2019.04.12 行動割合調整
 */
using UnityEngine;
using System.Collections.Generic;

public class ZakoParentAction : MonoBehaviour
{
	enum ZakoParentConduct
	{
		NORMAL,
		INHOLE,
		DELIVER,
		GUARD,
		NONE
	};

	public int hp = 5;
	Anim anim;												// アニメーション用 2019.01.15
	public Sprite[] bodySprite = new Sprite[8];				// 本体のアニメーション 2019.01.15 →目パチをゆっくりにするため要素数変更 2019.03.19
	public Sprite[] dieSpriteList = new Sprite[4];			// 倒された時のスプライト 2019.03.17
	public Sprite[] damageSpriteList = new Sprite[4];		// ダメージを受けた時のスプライト 2019.03.17
	public Sprite[] deliverSpriteList_1 = new Sprite[4];	// ザコを召喚するときのスプライトリスト 2019.03.17
	public Sprite[] deliverSpriteList_2 = new Sprite[4];	// 2019.03.17
	public Sprite[] deliverSpriteList_3 = new Sprite[4];	// 2019.03.17
	public Sprite[] deliverSpriteList_4 = new Sprite[4];	// 2019.03.17
	Anim maskAnim;											// 仮面のアニメーション 2019.03.17
	public Sprite[] maskSpriteList = new Sprite[4];			// 仮面のスプライトリスト 2019.03.17
	Vector3 deadMaskVelocity;								// 倒された時に仮面を動かす 2019.03.17
	Anim bookAnim;											// 本のアニメーション 2019.03.20
	public Sprite[] bookInhaleSpriteList = new Sprite[4];	// 吸い込み時の本のスプライトリスト 2019.03.20
	public Sprite[] bookSummonSpriteList = new Sprite[4];	// 召喚時の本のスプライトリスト 2019.04.02
	public Sprite[] bookGuardSpriteList = new Sprite[4];	// ガードするときの本のスプライトリスト 2019.03.22
	Transform bookBlackhole;								// 本が持っているブラックホール 2019.03.20
	Vector3 returnPos = Vector3.zero;						// 戻る位置
	float speedX = 0.05f;									// x軸のスピード 2019.01.19
	ZakoParentConduct nowAction;
	int actionFrame = 0;
	// 2018.12.01
	public GameObject zakoPrefab;									// ザコのプレハブ
	List<ZakoAction> zakoObjectList = new List<ZakoAction>();		// ザコのリスト 2019.01.26
	bool isAttackingZako = false;									// ザコが体当たりをしているか 2018.12.20
	ZakoAction attackingZako = null;								// 体当たり中のザコ 2019.01.26
	List<Vector2Int> zakoTargetPosList = new List<Vector2Int>();	// ザコの目指す位置 2019.01.26
	ParticleSystem deadParticle;									// 消えるときのパーティクル 2019.02.20
	public GameObject deadEffect;									// 消えた時のぱっとなるやつ 2019.02.20

	List<GameObject> targetGhostList = new List<GameObject>();	// 吸い込むおばけ
	bool endChangeGhostSprite = false;							// 吸い込むおばけのスプライトを変えたか
	GameObject hitBullet;										// 判定内の弾
	bool isInhole = false;										// 吸い込みの最中か

	bool endPointing = false;			// ザコを繰り出したか

	int zakoInsFrame = 50;										// 弾が当たった時のactionFrame
	SpriteRenderer shieldSR;									// シールドのアニメーション 2019.01.26
	const int guardFrameMax = 180;								// ガードするフレーム数

	// ダメージを受けた時に使う 2019.01.19
	int damageFrame = 0;				// ダメージを受けてからのフレーム数
	const int damageFrameMax = 45;		// ダメージ時のアクションのフレーム数
	int invincibleFrame = 0;			// 無敵フレーム

	int sign = 1;						// 左右に揺らすときの符号
	float nockbackSpeed = -0.2f;		// ノックバックするスピード
	float shake;						// 縦にどれくらい動いたか 2018.01.19
	float addShake = 0.00025f;			// 揺れる速さ 2019.01.19

	int blinkFrame = 0;					// ザコを点滅させるに使う 2019.02.20

	// 2019.02.26 弾の被弾エフェクトを追加するために変更
	public ParticleSystem fireworkPar;				// 被弾エフェクト用パーティクル
	PlayerControl playerControl;					// パーティクルのオンオフに使用

	void Start ()
	{
		nowAction = ZakoParentConduct.NORMAL;   // 2018.12.01
		// 2019.03.30
		hp = 5;
		// 2019.01.15
		anim = GetComponent<Anim>();
		speedX = 0.01f;
		// シールドのアニメーションを取得 2019.01.26
		shieldSR = transform.Find("Shield").GetComponent<SpriteRenderer>();
		shieldSR.color = new Color(1f, 1f, 1f, 0f);
		// パーティクルの情報を取得 2019.02.20
		deadParticle = transform.Find("DeadParticle").GetComponent<ParticleSystem>();
		deadParticle.Stop(true);
		// 仮面の情報を取得 2019.03.17
		maskAnim = transform.Find("Mask").GetComponent<Anim>();
		maskAnim.drawingSpriteFrameMax = anim.drawingSpriteFrameMax;
		deadMaskVelocity = new Vector3(-0.05f, 0f, -0.08f);
		// 本の情報を取得 2019.03.20
		bookAnim = transform.Find("Book").GetComponent<Anim>();
		bookAnim.drawingSpriteFrameMax = 4;
		bookAnim.transform.localScale = Vector3.zero;
		bookAnim.transform.localPosition = new Vector3(0f, -0.35f, 0f);
		bookBlackhole = bookAnim.transform.Find("Blackhole");
		bookBlackhole.localScale = Vector3.zero;
		// 2019.02.26
		playerControl = FindObjectOfType<PlayerControl>();
	}

	// 引数を追加 2018.12.20------------------------------------
	public void Action(PlayerControl player, GameObject playerCamera)
	{
		sign *= -1;
		// 体力が無くなったら行動しないようにする
		if (hp <= 0)
		{
			if (EnemyManager.isOnParticle)
			{
				deadParticle.Play(true);
			}
			// 仮面を回しながら落とす
			maskAnim.transform.Translate(deadMaskVelocity, Space.World);
			deadMaskVelocity.z += 0.004f;
			maskAnim.transform.Rotate(Vector3.up, 10f, Space.World);
			// 色を変える 2019.03.30
			anim.spriteRenderer.color = Color.Lerp(anim.spriteRenderer.color, new Color(200f / 255f, 245f / 255f, 1f, anim.spriteRenderer.color.a), 1f * Time.deltaTime);
			// 下降
			if (maskAnim.transform.localPosition.y < -3f)
			{
				transform.Translate(Vector3.forward * 0.5f * Time.deltaTime);
				ShakeObj(gameObject);
				anim.spriteRenderer.color -= new Color(0f, 0f, 0f, 0.01f);
			}
			anim.Animation(dieSpriteList, 4);
			// 本を落とす
			if (deadMaskVelocity.z > 0f)
			{
				bookAnim.transform.Translate(new Vector3(0f, 0f, deadMaskVelocity.z), Space.World);
			}
			bookBlackhole.gameObject.SetActive(false);
			if (anim.spriteRenderer.color.a <= 0f)
			{
				gameObject.SetActive(false);
				Instantiate(deadEffect, transform.position, deadEffect.transform.localRotation);
			}
			for (int i = 0; i < zakoObjectList.Count; ++i)
			{
				if (zakoObjectList[i].isActiveAndEnabled)
				{
					zakoObjectList[i].DeadAction();
				}
			}
			return;
		}
		// 本のブラックホールを回しておく 2019.03.20
		bookBlackhole.Rotate(Vector3.back, 1f);
		// シールドが張られていれば薄くしていく 2019.03.22
		shieldSR.color -= new Color(0f, 0f, 0f, 0.5f / 60f);
		if (shieldSR.color.a < 0f)
		{
			shieldSR.color = new Color(1f, 1f, 1f, 0f);
		}
		// ダメージを受けた時はダメージのアクションをする2019.01.19
		if (damageFrame > 0)
		{
			DamageAction();
			anim.Animation(damageSpriteList, 4);
		}
		else if (nowAction != ZakoParentConduct.DELIVER)
		{
			// アニメーションさせる 2019.01.15
			anim.Animation(bodySprite, 8);			// 2019.03.19
			maskAnim.Animation(maskSpriteList, 4);	// 2019.03.17
		}
		if (damageFrame <= 0 && invincibleFrame > 0)
		{
			anim.spriteRenderer.enabled = invincibleFrame % 4 < 2;
			maskAnim.spriteRenderer.enabled = invincibleFrame % 4 < 2;
		}
		damageFrame -= damageFrame - 1 < 0 ? 0 : 1;
		invincibleFrame -= invincibleFrame - 1 < 0 ? 0 : 1;
		++blinkFrame;
		// かなり変更 2018.12.01	2019.01.26
		// ザコを生成していたら操作する
		for (int i = 0; i < zakoObjectList.Count; ++i)
		{
			// ザコを大きくする
			zakoObjectList[i].transform.localScale = Vector3.Lerp(zakoObjectList[i].transform.localScale, Vector3.one, 1f * Time.deltaTime);
			// ザコの行動を変える 2018.12.20
			if (!isAttackingZako && Random.Range(0,128) == 0 && !zakoObjectList[i].isInvincible)
			{
				isAttackingZako = true;
				attackingZako = zakoObjectList[i];
			}
			// ザコがアクティブであれば操作する
			if (zakoObjectList[i].isActiveAndEnabled)
			{
				// 攻撃 2018.12.20
				if (attackingZako && attackingZako == zakoObjectList[i])
				{
					isAttackingZako = !zakoObjectList[i].Attack(player, playerCamera);
					if (!isAttackingZako)
					{
						attackingZako = null;
					}
				}
				else
				{
					zakoObjectList[i].NormalAction();
				}
				// 無敵状態であれば点滅させる
				if (zakoObjectList[i].isInvincible)
				{
					if (blinkFrame % 6 < 3)
					{
						zakoObjectList[i].SetSpriteEnabled(false);
					}
					else
					{
						zakoObjectList[i].SetSpriteEnabled(true);
					}
				}
				else
				{
					zakoObjectList[i].SetSpriteEnabled(true);
				}
			}
			// ザコがアクティブでなかったら削除
			else
			{
				ZakoAction zakoObject = zakoObjectList[i];
				zakoTargetPosList.RemoveAt(i);
				zakoObjectList.RemoveAt(i);
				Destroy(zakoObject.gameObject);
			}
		}
		// 判定内に弾があったら本体に当たったか判定する
		if (hitBullet)
		{
			if (invincibleFrame <= 0 && IsNear(transform.position, hitBullet.transform.position, 1.0f))
			{
				// ガードをしていたらザコを生成する 2019.01.26
				if (nowAction == ZakoParentConduct.GUARD && zakoObjectList.Count < 21)
				{
					Destroy(hitBullet);
					hitBullet = null;
					// 2019.02.26
					if (playerControl.parFlag == true)
					{
						Instantiate(fireworkPar, transform.position, fireworkPar.transform.localRotation);
					}
					// ザコを生成するためにフレームを設定 2019.03.30
					zakoInsFrame = 0;
					// シールドを張る 2019.03.22
					shieldSR.color = Color.white;
					// カウンター時のSE再生 2019.03.26
					EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossGuardHit);
				}
				// ガード状態でなければダメージを受ける
				else if (nowAction == ZakoParentConduct.NORMAL)
				{
					Destroy(hitBullet);
					hitBullet = null;
					// 2019.02.26
					if (playerControl.parFlag == true)
					{
						Instantiate(fireworkPar, transform.position, fireworkPar.transform.localRotation);
					}
					--hp;
					damageFrame = damageFrameMax;
					invincibleFrame = damageFrameMax + 90;
					nockbackSpeed = 0.15f;
				}
			}
		}
		// カウンターザコの生成 2019.03.30
		if (++zakoInsFrame == 50)
		{
			InsZako();
		}
		// 格納しているおばけの整理
		for (int i = 0; i < targetGhostList.Count; ++i)
		{
			if (!targetGhostList[i])
			{
				targetGhostList.RemoveAt(i);
			}
		}
		// 行動
		if (nowAction == ZakoParentConduct.NORMAL || nowAction == ZakoParentConduct.GUARD)
		{
			// シールドを広げておく
			if (nowAction == ZakoParentConduct.GUARD)
			{
				GuardAction();
				// 揺らす 2019.01.19
				shake += addShake;
				if (Mathf.Abs(shake) > 0.02f) { addShake *= -1; }
				// スピード補正 2019.01.19
				if (transform.position.x >= 4.5f - 33f)
				{
					speedX -= 0.0005f;
					if (speedX < -0.01f) { speedX = -0.01f; }
				}
				else if (transform.position.x <= -4.5f - 33.0f)
				{
					speedX += 0.0005f;
					if (speedX > 0.01f) { speedX = 0.01f; }
				}
			}
			// 通常状態なら移動しながら行動を変える
			else if (nowAction == ZakoParentConduct.NORMAL)
			{
				shake += addShake;
				if (Mathf.Abs(shake) > 0.02f) { addShake *= -1; }
				// 通常状態ならさらに移動する
				if (transform.position.x >= 4.5f - 33f)
				{
					speedX -= 0.0005f;
					if (speedX < -0.02f) { speedX = -0.02f; }
				}
				else if (transform.position.x <= -4.5f - 33.0f)
				{
					speedX += 0.0005f;
					if (speedX > 0.02f) { speedX = 0.02f; }
				}
				transform.position += new Vector3(speedX, 0f, 0f);
				// 行動変化
				if ((actionFrame++ > 400 || Random.Range(0, 96) == 0) && damageFrame <= 0)
				{
					RandomSwitchAction();
					actionFrame = 0;
				}
				else if (Random.Range(0, 192) == 0)
				{
					nowAction = ZakoParentConduct.GUARD;
					actionFrame = 0;
				}
			}
			// 移動
			transform.position += new Vector3(speedX, 0f, shake);
		}
		else if (nowAction == ZakoParentConduct.INHOLE)
		{
			InholeAction();
		}
		else if (nowAction == ZakoParentConduct.DELIVER)
		{
			DeliverZako();
		}
	}

	// 二つの位置をどれくらい近いか比べる 2018.12.01----------------------------------------
	bool IsNear(Vector3 a, Vector3 b, float value)
	{
		return Mathf.Abs(a.x - b.x) < value && Mathf.Abs(a.y - b.y) < value && Mathf.Abs(a.z - b.z) < value;
	}

	// 行動選択 2018.12.01---------------------------------------
	void RandomSwitchAction()
	{
		returnPos = transform.position;
		int ran = Random.Range(1, 6);	// 2019.04.12
		// 行動選択	2019.04.12
		if (ran > hp)
		{
			nowAction = ZakoParentConduct.INHOLE;
			isInhole = true;
			// 2019.03.02
			if (targetGhostList.Count != 0)
			{
				for (int i = 0; i < targetGhostList.Count; ++i)
				{
					targetGhostList[i].GetComponent<GhostAction>().ChangeSpriteDeleteInhole();
				}
			}
		}
		else
		{
			if (zakoObjectList.Count != 0) { return; }
			nowAction = ZakoParentConduct.DELIVER;
		}
	}

	// 吸い込み一連の行動 2018.12.01----------------------------------------------------------
	void InholeAction()
	{
		// 情報設定
		Vector3 waitPos = new Vector3(returnPos.x + 1.15f, returnPos.y + -1f, returnPos.z + -1.65f);
		// 全て吸い取ったら元の位置に戻り動き始める
		if (targetGhostList.Count == 0)
		{
			// 本体の色を戻す 2019.03.30
			anim.spriteRenderer.color = Color.Lerp(anim.spriteRenderer.color, Color.white, 1f * Time.deltaTime);
			maskAnim.spriteRenderer.color = Color.Lerp(maskAnim.spriteRenderer.color, Color.white, 1f * Time.deltaTime);
			// 本を戻す 2019.03.20
			bookAnim.transform.localPosition = Vector3.Lerp(bookAnim.transform.localPosition, new Vector3(0f, -0.35f, 0f), 3f * Time.deltaTime);
			// 本を閉じる 2019.03.20
			if (bookAnim.spriteRenderer.sprite != bookInhaleSpriteList[0])
			{
				bookAnim.Animation(bookInhaleSpriteList, 4, false);
			}
			// ブラックホールを縮める 2019.03.20
			bookBlackhole.localScale = Vector3.Lerp(bookBlackhole.localScale, Vector3.zero, 10f * Time.deltaTime);
			if (bookBlackhole.localScale.x < 0.1f) { bookBlackhole.localScale = Vector3.zero; }
			// 本を縮める 2019.03.20
			bookAnim.transform.localScale = Vector3.Lerp(bookAnim.transform.localScale, Vector3.zero, 3f * Time.deltaTime);
			if (bookAnim.transform.localScale.x < 0.1f) { bookAnim.transform.localScale = Vector3.zero; }
			transform.position = Vector3.MoveTowards(transform.position, returnPos, 5.0f * Time.deltaTime);
			if (IsNear(transform.position, returnPos, 0.01f) && bookAnim.transform.localScale.x < 0.1f)
			{
				nowAction = ZakoParentConduct.NORMAL;
				transform.position = returnPos;
				isInhole = false;
				// 2019.03.30
				anim.spriteRenderer.color = Color.white;
				maskAnim.spriteRenderer.color = Color.white;
			}
		}
		// おばけを吸い込む
		else
		{
			// 本体の色を変える 2019.03.30
			anim.spriteRenderer.color = Color.Lerp(anim.spriteRenderer.color, new Color(1f, 100f / 255f, 100f / 255f, 190 / 255f), 5f * Time.deltaTime);
			maskAnim.spriteRenderer.color = Color.Lerp(maskAnim.spriteRenderer.color, new Color(1f, 100f / 255f, 100f / 255f, 190 / 255f), 5f * Time.deltaTime);
			// 本を特定の位置に置く 2019.03.20
			bookAnim.transform.localPosition = Vector3.Lerp(bookAnim.transform.localPosition, new Vector3(-0.8f, 0f, -0.312f), 3f * Time.deltaTime);
			// 本を開く 2019.03.20
			if (bookAnim.spriteRenderer.sprite != bookInhaleSpriteList[3])
			{
				bookAnim.Animation(bookInhaleSpriteList, 4);
			}
			// ブラックホールを広げる 2019.03.20
			bookBlackhole.localScale = Vector3.Lerp(bookBlackhole.localScale, Vector3.one, 1f * Time.deltaTime);
			// 本を広げる 2019.03.20
			bookAnim.transform.localScale = Vector3.Lerp(bookAnim.transform.localScale, Vector3.one, 5f * Time.deltaTime);
			// 音を鳴らす 2019.03.31
			if(actionFrame == 10)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eBossInhale);
			}
			// 震わせる 2019.03.16
			if (++actionFrame < 60)
			{
				for (int i = 0; i < targetGhostList.Count; ++i)
				{
					ShakeObj(targetGhostList[i]);
				}
			}
			else
			{
				for (int i = 0; i < targetGhostList.Count; ++i)
				{
					targetGhostList[i].transform.position = Vector3.MoveTowards(targetGhostList[i].transform.position, bookBlackhole.position, 5.0f * Time.deltaTime);
					// 近づいたら消す
					if (IsNear(targetGhostList[i].transform.position, bookBlackhole.position, 0.05f))
					{
						GameObject remove = targetGhostList[i];
						targetGhostList.Remove(remove);
						Destroy(remove);
					}
				}
			}
		}
	}

	// ザコを繰り出す 2018.12.01-------------------------------------------------------------
	void DeliverZako()
	{
		// 終わったら元の位置に戻る
		if (endPointing)
		{
			// 本体の色を戻す 2019.03.30
			anim.spriteRenderer.color = Color.Lerp(anim.spriteRenderer.color, Color.white, 1f * Time.deltaTime);
			maskAnim.spriteRenderer.color = Color.Lerp(maskAnim.spriteRenderer.color, Color.white, 1f * Time.deltaTime);
			// 本を閉じる 2019.03.20
			if (bookAnim.spriteRenderer.sprite != bookSummonSpriteList[0])
			{
				bookAnim.Animation(bookSummonSpriteList, 4, false);
			}
			bookAnim.spriteRenderer.color -= new Color(0f, 0f, 0f, 1.2f / 60f);
			bookAnim.transform.Translate(Vector3.down * Time.deltaTime, Space.Self);
			// 本体のアニメーション 2019.03.18
			if (anim.spriteRenderer.sprite == deliverSpriteList_3[0] || anim.spriteRenderer.sprite == deliverSpriteList_3[1]
				|| anim.spriteRenderer.sprite == deliverSpriteList_3[2])
			{
				anim.Animation(deliverSpriteList_3, 4);
			}
			else if (anim.spriteRenderer.sprite == deliverSpriteList_1[0] || anim.spriteRenderer.sprite == deliverSpriteList_1[1]
				|| anim.spriteRenderer.sprite == deliverSpriteList_1[2] || anim.spriteRenderer.sprite == deliverSpriteList_1[3]
				|| anim.spriteRenderer.sprite == deliverSpriteList_4[3])
			{
				anim.Animation(deliverSpriteList_1, 4);
			}
			else
			{
				anim.Animation(deliverSpriteList_4, 4);
			}
			transform.position = Vector3.Lerp(transform.position, returnPos, 1.0f * Time.deltaTime);
			// 近くまで来ていたら動き出す
			if (IsNear(transform.position, returnPos, 0.05f) && bookAnim.spriteRenderer.color.a < 0f)
			{
				endPointing = false;
				nowAction = ZakoParentConduct.NORMAL;
				transform.position = returnPos;
				actionFrame = 0;
				bookAnim.transform.localPosition = new Vector3(0f, -0.35f, 0f);
				bookAnim.transform.localScale = Vector3.zero;
				bookAnim.spriteRenderer.color = Color.white;
				// 2019.03.30
				anim.spriteRenderer.color = Color.white;
				maskAnim.spriteRenderer.color = Color.white;
			}
		}
		// ザコを繰り出す
		else if (actionFrame++ > 60)
		{
			// 本体のアニメーション 2019.03.20
			if (anim.spriteRenderer.sprite == deliverSpriteList_1[0] || anim.spriteRenderer.sprite == deliverSpriteList_1[1]
				|| anim.spriteRenderer.sprite == deliverSpriteList_1[2])
			{
				anim.Animation(deliverSpriteList_1, 4);
			}
			else if (actionFrame < 61 + anim.drawingSpriteFrameMax * 4)
			{
				anim.Animation(deliverSpriteList_2, 4);
			}
			else
			{
				anim.Animation(deliverSpriteList_3, 4);
			}
			// 移動させる
			transform.position = Vector3.Lerp(transform.position, returnPos, 1.0f * Time.deltaTime);
			// 生成する
			if (IsNear(transform.position, returnPos, 0.25f))
			{
				for (int i = 0; i < 2; ++i)
				{
					InsZako();
				}
				endPointing = true;
			}
		}
		// ちょっと後ろにためる
		else
		{
			// 本体の色を変える 2019.03.30
			anim.spriteRenderer.color = Color.Lerp(anim.spriteRenderer.color, new Color(1f, 100f / 255f, 100f / 255f, 190 / 255f), 5f * Time.deltaTime);
			maskAnim.spriteRenderer.color = Color.Lerp(maskAnim.spriteRenderer.color, new Color(1f, 100f / 255f, 100f / 255f, 190 / 255f), 5f * Time.deltaTime);
			// 本を開くSEの再生 2019.03.26
			if (actionFrame == 1)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eZakoParentBook);
			}
			// 本を特定の位置に置く 2019.03.20
			bookAnim.transform.localPosition = Vector3.Lerp(bookAnim.transform.localPosition, new Vector3(0.8f, 0f, -0.312f), 3f * Time.deltaTime);
			// 本を広げる 2019.03.20
			bookAnim.transform.localScale = Vector3.Lerp(bookAnim.transform.localScale, Vector3.one, 5f * Time.deltaTime);
			// 本を開く 2019.03.20
			if (bookAnim.spriteRenderer.sprite != bookSummonSpriteList[3])
			{
				bookAnim.Animation(bookSummonSpriteList, 4);
			}
			anim.Animation(deliverSpriteList_1, 4);	// 2019.03.17
			Vector3 waitPos = returnPos;
			waitPos.y += 1.0f;
			waitPos.z -= 0.5f;
			transform.position = Vector3.Lerp(transform.position, waitPos, 1.0f * Time.deltaTime);
		}
	}

	// ガードする 2019.01.26----------------------------------------------------------------------
	void GuardAction()
	{
		// 規定フレームまでシールドを広げておく
		if (actionFrame++ < guardFrameMax)
		{
			// 本を開くSEの再生 2019.03.26
			if(actionFrame == 1)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eZakoParentBook);
			}
			// 本を所定の位置に置く 2019.03.22
			bookAnim.transform.localPosition = Vector3.Lerp(bookAnim.transform.localPosition, new Vector3(0f, 0f, -0.5f), 5f * Time.deltaTime);
			// 本を広げる 2019.03.22
			bookAnim.transform.localScale = Vector3.Lerp(bookAnim.transform.localScale, Vector3.one * 1.5f, 4f * Time.deltaTime);
			// 本を開く 2019.03.22
			if (bookAnim.spriteRenderer.sprite != bookGuardSpriteList[3])
			{
				bookAnim.Animation(bookGuardSpriteList, 4);
			}
		}
		// フレームを過ぎたら本を小さくして終わる
		else
		{
			bookAnim.transform.localScale = Vector3.Lerp(bookAnim.transform.localScale, Vector3.zero, 3f * Time.deltaTime);
			// 本を小さくする 2019.03.22
			if (bookAnim.transform.localScale.x <= 0.25f && bookAnim.spriteRenderer.sprite == bookGuardSpriteList[0])
			{
				bookAnim.transform.localScale = new Vector3(0f, 0.25f, 0f);
				nowAction = ZakoParentConduct.NORMAL;
				actionFrame = 0;
			}
			// 本を閉じる 2019.03.22
			if (bookAnim.spriteRenderer.sprite != bookGuardSpriteList[0])
			{
				bookAnim.Animation(bookGuardSpriteList, 4, false);
			}
			// 本を戻す 2019.03.22
			bookAnim.transform.localPosition = Vector3.Lerp(bookAnim.transform.localPosition, new Vector3(0f, -0.35f, 0f), 10f * Time.deltaTime);
		}
	}

	// ザコを生成する 2019.02.03--------------------------------------------------------
	void InsZako()
	{
		ZakoAction zakoObject = Instantiate(zakoPrefab, bookAnim.transform.position, zakoPrefab.transform.localRotation).GetComponent<ZakoAction>();
		Vector2Int targetPos = Vector2Int.zero;
		targetPos.x = Random.Range(-3, 3 + 1) * 2 - 33;
		targetPos.y = Random.Range(-5, -3 + 1) * 2 - 1;
		// 重ならないようにする
		while (zakoTargetPosList.Contains(targetPos))
		{
			targetPos.x = Random.Range(-3, 3 + 1) * 2 - 33;
			targetPos.y = Random.Range(-5, -3 + 1) * 2 - 1;
		}
		zakoObject.targetPos = new Vector3(targetPos.x, Random.Range(4.5f, 5.5f), targetPos.y);
		zakoObject.transform.localScale = Vector3.one * 0.1f;
		zakoTargetPosList.Add(targetPos);
		zakoObjectList.Add(zakoObject);
	}

	// 渡されたオブジェクトを呼ばれている間震えさせる--------------------------------------
	void ShakeObj(GameObject shake)
	{
		// 震える
		shake.transform.position += new Vector3(0.1f * sign, 0, 0);
	}

	// ダメージを受けたときのアクション---------------------------------------------------
	void DamageAction()
	{
		// 震える
		ShakeObj(gameObject);
		Vector3 nockbackPos = new Vector3(0, nockbackSpeed, 0);
		transform.position += nockbackPos;
		if (nockbackSpeed > -0.15f)
		{
			nockbackSpeed -= 0.00675f;
			if (nockbackSpeed < -0.15f)
			{
				nockbackSpeed = -0.15f;
			}
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Bullet")
		{
			// 判定を大きくするので一度格納する 2018.12.01
			hitBullet = col.gameObject;
		}
		// 2018.12.01
		else if (col.tag == "Goast")
		{
			if (isInhole) { return; }
			targetGhostList.Add(col.gameObject);
			col.tag = "TargetGhost";
		}
	}

	// 2018.12.01
	void OnTriggerExit(Collider col)
	{
		if (col.tag == "Bullet")
		{
			// 当たらなかったので解放
			hitBullet = null;
		}
		else if (col.tag == "TargetGhost")
		{
			if (isInhole) { return; }
			col.tag = "Goast";
			targetGhostList.Remove(col.gameObject);
		}
	}
}

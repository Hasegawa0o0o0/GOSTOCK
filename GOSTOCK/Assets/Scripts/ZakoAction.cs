/*
 * ZakoAction.cs
 * 2018.10.13 作成
 *		2018.12.20 体当たりの追加
 *		2018.12.27 待っているときに震えさせる
 *				   数値いじり
 *		2018.12.28 震えを遅くする、数値いじり
 *		2019.01.08 音を鳴らす
 *		2019.01.15 アニメーション追加
 *		2019.02.12 無敵時間追加
 *		2019.02.20 パーティクル追加
 *		2019.02.26 倒れるときに震える
 *		2019.04.03 ボス出現の演出でもアニメーションができるよう関数を追加
 */
 /* ザコが目的地に向かう */
using UnityEngine;

public class ZakoAction : MonoBehaviour
{
	public Vector3 targetPos;	// 目的地

	// 2019.01.15
	Anim anim;										// アニメーション用
	public Sprite[] bodySpriteList = new Sprite[4];	// 本体のスプライト
	public Sprite deadSprite;						// 倒れるときのスプライト
	Anim[] wingAnim = new Anim[2];					// 翼のアニメーション用
	public Sprite[] wingSpriteList = new Sprite[4];	// 翼のスプライト
	ParticleSystem deadParticle;					// 消えるときのパーティクル 2019.02.20
	public GameObject deadEffect;					// 消えた時のぱっとなるやつ 2019.02.20

	const int attackWaitFrame = 270;	// 攻撃で待つフレーム数 数値いじり 2018.12.28
	int actionFrame = 0;				// アクション管理用フレーム 2018.12.20
	bool endAttack = false;				// 2018.12.20
	int sign = 1;						// 左右に揺らすときの符号 2018.12.27
	int shakeFrame = 0;					// 左右に揺らすときのディレイ 2018.12.28
	public bool isInvincible = true;	// 
	bool isHit = false;					// 弾に当たったかどうか 2019.02.20

	// 2019.02.26 弾の被弾エフェクトを追加するために変更
	public ParticleSystem fireworkPar;				// 被弾エフェクト用パーティクル
	PlayerControl playerControl;					// パーティクルのオンオフに使用

	void Start ()
	{
		// 2019.01.15
		anim = GetComponent<Anim>();
		wingAnim[0] = transform.Find("RightWing").GetComponent<Anim>();
		wingAnim[1] = transform.Find("LeftWing").GetComponent<Anim>();
		// 2019.02.20
		deadParticle = transform.Find("DeadParticle").GetComponent<ParticleSystem>();
		deadParticle.Stop(true);
		// 2019.02.26
		playerControl = FindObjectOfType<PlayerControl>();
	}

	// 名前を変更 Action -> NormalAction 2018.12.20
	public void NormalAction()
	{
		// 弾に当たっていたら消えるときの動作をしてreturn 2019.02.20
		if (isHit)
		{
			DeadAction();
			return;
		}
		// 本体と翼をアニメーションさせる 2019.01.15
		anim.Animation(bodySpriteList, 8);
		for (int i = 0; i < 2; ++i)
		{
			wingAnim[i].Animation(wingSpriteList, 4);
		}
		transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f * Time.deltaTime);
		// 2019.02.12
		if (isInvincible)
		{
			isInvincible = !IsNear(transform.position, targetPos, 3f);
		}
	}

	// 二つの位置をどれくらい近いか比べる 2018.12.20------------------------------------------
	bool IsNear(Vector3 a, Vector3 b, float value)
	{
		return Mathf.Abs(a.x - b.x) < value && Mathf.Abs(a.y - b.y) < value && Mathf.Abs(a.z - b.z) < value;
	}

	// 体当たり 2018.12.20------------------------------------------------
	// 戻り値：攻撃が終わって元の位置に戻ったか
	public bool Attack(PlayerControl player, GameObject playerCamera)
	{
		// 2019.02.20
		if (isHit) { return true; }
		// 本体と翼をアニメーションさせる 2019.01.15
		anim.Animation(bodySpriteList, 4);
		// 羽ばたきをを早くする 2019.01.15
		for (int i = 0; i < 2; ++i)
		{
			wingAnim[i].drawingSpriteFrameMax = 2;
			wingAnim[i].Animation(wingSpriteList, 4);
		}
		// 攻撃が終わったらもとの位置に戻す
		if (endAttack)
		{
			// 数値いじり(第三引数:2f -> 2.5f) 2018.12.27
			transform.position = Vector3.Lerp(transform.position, targetPos, 2.5f * Time.deltaTime);
			if (IsNear(transform.position, targetPos, 0.01f))
			{
				// 羽ばたきをを元の早さに戻す 2019.01.15
				for (int i = 0; i < 2; ++i)
				{
					wingAnim[i].drawingSpriteFrameMax = 4;
				}
				actionFrame = 0;
				endAttack = false;
				return true;
			}
		}
		// 待っているときは所定の位置に
		else if (actionFrame++ < attackWaitFrame)
		{
			// 音を鳴らす 2019.01.08
			if (actionFrame == 1)
			{
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eZakoAttack);
			}
			// 待つ 数値いじり(第三引数:0.1f -> 5f) 2018.12.27
			transform.position = Vector3.Lerp(transform.position,
				new Vector3(targetPos.x, targetPos.y + 1.0f, targetPos.z + 0.5f), 5f * Time.deltaTime);
			// 震えさせる 2018.12.27
			ShakeObj(gameObject);
		}
		else
		{
			if (actionFrame == attackWaitFrame + 1)
			{
				// 音を鳴らす 2019.01.08
				EnemySounds.instance.SoundOneShot(EnemySounds.SoundArray.eZakoAttack);
			}
			// プレイヤーに向かって体当たり 数値いじり(第三引数:6.5f -> 7.5f) 2018.12.28
			transform.position = Vector3.MoveTowards(transform.position,
				playerCamera.transform.position,
				7.5f * Time.deltaTime);
			if (IsNear(transform.position, playerCamera.transform.position, 0.01f))
			{
				player.hp--;
				player.damageCameraNum = 1;      // プレイヤーのほうを変えるべき
				player.isDamage = true;
				endAttack = true;
			}
		}
		return false;
	}

	// 渡されたオブジェクトを呼ばれている間震えさせる 2018.12.27---------------------------
	void ShakeObj(GameObject shake)
	{
		// 震える 2018.12.28
		if (shakeFrame++ == 3)
		{
			shake.transform.position += new Vector3(0.1f * sign, 0, 0);
			sign *= -1;
			shakeFrame = 0;
		}
	}

	// スプライトのオンオフを設定する 2019.02.12-----------------------------------------
	public void SetSpriteEnabled(bool set)
	{
		anim.spriteRenderer.enabled = set;
		for (int i = 0; i < 2; ++i) { wingAnim[i].spriteRenderer.enabled = set; }
	}

	// 本体と翼のアニメーションをする 2019.04.03-------------------------------
	public void ZakoAnimation()
	{
		// 本体と翼をアニメーションさせる 2019.01.15
		anim.Animation(bodySpriteList, 8);
		for (int i = 0; i < 2; ++i)
		{
			wingAnim[i].Animation(wingSpriteList, 4);
		}
	}

	// HPが0になった時の動作 2019.02.20--------------------------------------
	public void DeadAction()
	{
		if (EnemyManager.isOnParticle)
		{
			deadParticle.Play(true);
		}
		transform.Translate(Vector3.forward * Time.deltaTime);
		ShakeObj(gameObject);
		anim.spriteRenderer.sprite = deadSprite;
		anim.spriteRenderer.color -= new Color(0f, 0f, 0f, 0.02f);
		for (int i = 0; i < 2; ++i) { wingAnim[i].spriteRenderer.color -= new Color(0f, 0f, 0f, 0.021f); }
		if (anim.spriteRenderer.color.a <= 0f)
		{
			gameObject.SetActive(false);
			Instantiate(deadEffect, transform.position, deadEffect.transform.localRotation);
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Bullet" && !isInvincible)
		{
			isHit = true;
			if (playerControl.parFlag == true)			// 2019.02.26
			{
				Instantiate(fireworkPar, col.transform.position, fireworkPar.transform.localRotation);
			}
		}
	}
}

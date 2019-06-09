/*
 * BossHand.cs
 * 2018.10.17 作成
 *		2018.10.24 ちょっとバグ防止
 *		2018.10.25 バグ防止
 *		2018.10.26 体力作成
 *		2018.10.27 アニメーション変数を追加
 *		2018.11.01 ブラックホールのためのおばけ格納方法の変更
 *		2018.11.27 吸い込んでいるおばけのスプライトを変えたかどうか
 *		2018.12.08 バグ防止
 *		2019.01.03 回復パンチのスパークルを出す
 *		2019.01.13 ばんそうこう追加
 *		2019.01.19 攻撃用スパークル追加
 *		2019.02.16 微調整
 *		2019.03.30 ばんそうこうの位置を外部から変えられるように
 */
/* ボスの手の操作 */
using UnityEngine;
using System.Collections.Generic;

public class BossHand : MonoBehaviour
{
	Rigidbody rb;
	public Anim anim;							// 2018.10.27
	public int hp = 0;
	public Vector3 speed;												// スピード
	public GameObject targetGhost;										// つかみに行くおばけ格納用
	public List<GameObject> targetGhostList = new List<GameObject>();   // 吸い込むおばけの格納リスト 2018.11.01
	public List<bool> endChangeGhostSprite = new List<bool>();			// スプライトを変えたか 2018.11.27
	public int inholeLevel = 15;										// 吸い込みレベル(HPで変動) 2018.11.01
	public bool isPush = false;											// つかみにいっているかどうか 2018.10.25
	public bool isInhole = false;										// 吸い込みをしているか 2018.11.01
	bool hitColBullet = false;											// 判定に弾が触れた
	public bool isHitBullet = false;									// 手自体に当たる
	GameObject hitBullet;												// 当たった弾
	public GameObject sparcklePrefab;									// 回復パンチのときに出すスパークル 2019.01.03
	List<SpriteRenderer> sparckleObject = new List<SpriteRenderer>();	// スパークルを管理するリスト 2019.01.03
	List<Vector3> sparckleVelocity = new List<Vector3>();				// スパークを動かす方向とスピード 2019.01.03
	bool isGreenSparckle = true;										// 回復スパークルかどうか 2019.01.19
	List<SpriteRenderer> bandaidSRList = new List<SpriteRenderer>();	// ばんそうこう 2019.01.13
	public int bandaidFrame = 0;										// ばんそうこうを表示する時間 2019.01.13
	List<float> defaultBandaidPosY = new List<float>();					// ばんそうこうのyの本来の位置(動かすのに使う) 2019.03.30

	// 2019.02.26 弾の被弾エフェクトを追加するために変更
	public ParticleSystem fireworkPar;				// 被弾エフェクト用パーティクル
	PlayerControl playerControl;					// パーティクルのオンオフに使用

	void Start ()
	{
		rb = GetComponent<Rigidbody>();
		anim = GetComponent<Anim>();	// 2018.10.27
		// ばんそうこう取得 2019.01.13
		foreach(Transform o in transform)
		{
			SpriteRenderer s = o.GetComponent<SpriteRenderer>();
			if (s)
			{
				bandaidSRList.Add(s);
				defaultBandaidPosY.Add(s.transform.localPosition.y);	// 2019.03.30
			}
		}
		// 2019.02.26
		playerControl = FindObjectOfType<PlayerControl>();
	}
	
	void Update ()
	{
		// リストにおばけがいなかったら要素を取り除く 2018.11.01
		GameObject removeObj = null;
		int removeBNum = 0;		// 2018.11.27
		foreach(GameObject o in targetGhostList)
		{
			if (o == null)
			{
				// 2018.12.08
				targetGhostList.Remove(o);
				endChangeGhostSprite.RemoveAt(removeBNum);
				break;			// 2018.11.27
			}
			else if (o.tag != "TargetGhost")
			{
				if (isInhole) { break; }
				removeObj = o;
				break;			// 2018.11.27
			}
			++removeBNum;	// 2018.11.27
		}
		// 2018.11.27
		if (removeObj)
		{
			targetGhostList.Remove(removeObj);
			endChangeGhostSprite.RemoveAt(removeBNum);	// 2018.11.27
		}
		// スパークルの処理、アルファチャンネルが0になったものから排除 2019.01.03
		for (int i = 0; i < sparckleObject.Count; ++i)
		{
			sparckleObject[i].transform.position += sparckleVelocity[i];
			sparckleObject[i].color -= new Color(0f, 0f, 0f, 0.75f / 60f);
			// 点滅させる 条件追加 2019.01.19
			if (isGreenSparckle)
			{
				sparckleObject[i].color = new Color(Random.Range(20f / 255f, 120f / 255f), Random.Range(120f / 255f, 1f),
					Random.Range(20f / 255f, 120f / 255f), sparckleObject[i].color.a);
			}
			else
			{
				sparckleObject[i].color = new Color(1f, 0f, 0f, sparckleObject[i].color.a);
			}
			// アルファチャンネルが0以下になったら削除する
			if (sparckleObject[i].color.a < 0f)
			{
				sparckleVelocity.RemoveAt(i);
				GameObject o = sparckleObject[i].gameObject;
				sparckleObject.RemoveAt(i);
				Destroy(o);
			}
		}
		// ばんそうこうの処理 2019.01.13 少し処理を変更
		for (int i = 0; i < bandaidSRList.Count; ++i)
		{
			bandaidSRList[i].enabled = bandaidFrame > 0;
		}
		bandaidFrame -= bandaidFrame - 1 >= 0 ? 1 : 0;
		// 速度更新
		rb.velocity = speed;
		// 手事態に弾が当たったか確認する
		if (hitColBullet && hitBullet)
		{
			isHitBullet = Mathf.Abs(hitBullet.transform.position.x - transform.position.x) < 1.5f &&
				Mathf.Abs(hitBullet.transform.position.y - transform.position.y) < 1.5f &&
				Mathf.Abs(hitBullet.transform.position.z - transform.position.z) < 1.5f;
		}
		if (isHitBullet && hitBullet)
		{
			// 2019.02.26
			if (playerControl.parFlag == true)
			{
				Instantiate(fireworkPar, hitBullet.transform.position, fireworkPar.transform.localRotation);
			}
			Destroy(hitBullet);
		}
	}

	// 回復スパークルを生成する 2019.01.03------------------------------------------------
	public void InsGreenSparckle()
	{
		isGreenSparckle = true;
		sparckleObject.Add(Instantiate(sparcklePrefab, transform.position,
			sparcklePrefab.transform.localRotation).GetComponent<SpriteRenderer>());
		float randomX = Random.Range(-0.05f, 0.05f);
		float randomZ = Random.Range(-0.05f, 0.05f);
		sparckleVelocity.Add(new Vector3(randomX, -0.001f, randomZ));
	}

	// 攻撃スパークルを生成する 2019.01.19-----------------------------------------------
	public void InsRedSparckle()
	{
		isGreenSparckle = false;
		for (int i = 0; i < 10; ++i)
		{
			float angle_rad = Random.Range(0f, 359f) * Mathf.Deg2Rad;
			Vector3 insPos = new Vector3(transform.position.x + 4f * Mathf.Cos(angle_rad), transform.position.y + 0.5f, transform.position.z + 4f * Mathf.Sin(angle_rad));
			Vector3 velocity = new Vector3(-0.05f * Mathf.Cos(angle_rad), 0f, -0.05f * Mathf.Sin(angle_rad));
;			sparckleObject.Add(Instantiate(sparcklePrefab, insPos,
				sparcklePrefab.transform.localRotation).GetComponent<SpriteRenderer>());
			sparckleVelocity.Add(velocity);
		}
	}

	// スピードを0にする-------------------------------------------------------------
	public void InitSpeed()
	{
		speed = Vector3.zero;
	}

	// ばんそうこうのローカル位置を移動させるする 2019.03.30-----------------------------------
	public void OffsetBandaidLocalPos(Vector3 pos)
	{
		for (int i = 0; i < bandaidSRList.Count; ++i)
		{
			bandaidSRList[i].transform.localPosition += pos;
			if (bandaidSRList[i].transform.localPosition.y > defaultBandaidPosY[i])
			{
				bandaidSRList[i].transform.localPosition = new Vector3(bandaidSRList[i].transform.localPosition.x, defaultBandaidPosY[i], bandaidSRList[i].transform.localPosition.z);
			}
			else if (bandaidSRList[i].transform.localPosition.y < defaultBandaidPosY[i] - 0.13f)
			{
				bandaidSRList[i].transform.localPosition = new Vector3(bandaidSRList[i].transform.localPosition.x, defaultBandaidPosY[i] - 0.13f, bandaidSRList[i].transform.localPosition.z);
			}
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Goast")
		{
			// 2018.11.01
			if (isInhole) { return; }
			// 当たったおばけを格納する
			// !!!!!!CountとinholeLevelの数値を比べる!!!!!!
			int i;
			for (i = 0; i < targetGhostList.Count; ++i) ;
			if (i < inholeLevel)
			{
				targetGhostList.Add(col.gameObject);
				endChangeGhostSprite.Add(false);        // 2018.11.27
				col.tag = "TargetGhost";
			}
		}
		else if (col.tag == "Bullet")
		{
			hitColBullet = true;
			hitBullet = col.gameObject;
		}
	}

	void OnTriggerStay(Collider col)
	{
		if (col.tag == "Goast")
		{
			// 2018.11.01
			if (isInhole) { return; }
			// 当たったおばけを格納する
			// !!!!!!CountとinholeLevelの数値を比べる!!!!!!
			int i;
			for (i = 0; i < targetGhostList.Count; ++i) ;
			if (i < inholeLevel)
			{
				targetGhostList.Add(col.gameObject);
				endChangeGhostSprite.Add(false);        // 2018.11.27
				col.tag = "TargetGhost";
			}
		}
	}

	void OnTriggerExit(Collider col)
	{
		if (col.tag == "TargetGhost")
		{
			// おばけを開放してタグも戻す 2018.11.01
			if (isInhole) { return; }
			GameObject removeObj = null;
			int removeBNum = 0;	// 2018.11.27
			foreach(GameObject o in targetGhostList)
			{
				if(o == col.gameObject)
				{
					removeObj = o;
					break;	// 2018.11.27
				}
				++removeBNum;	// 2018.11.27
			}
			if (removeObj)
			{
			targetGhostList.Remove(removeObj);
				endChangeGhostSprite.RemoveAt(removeBNum);  // 2018.11.27
			}
			col.tag = "Goast";
		}
		else if (col.tag == "Bullet")
		{
			hitColBullet = false;
			isHitBullet = false;
			hitBullet = null;
		}
	}
}

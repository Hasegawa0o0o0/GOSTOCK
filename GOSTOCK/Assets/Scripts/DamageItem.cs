using UnityEngine;

public class DamageItem : MonoBehaviour
{
	public float speed;
	public int frame;
	float addScale;
	int posNum = 0;					// プレイヤーとの距離の番号
	SpriteRenderer spriteRenderer;	// 自身のスプライトレンダラー
	public int nc;					// nowCamera
	public PlayerControl pc;
	GameObject camera1;
	GameObject camera2;
	GameObject camera3;

	void Start()
	{
		pc = FindObjectOfType<PlayerControl>();
		if (nc == 0)
		{
			camera1 = GameObject.Find("Camera01");
		}
		else if (nc == 1)
		{
			camera2 = GameObject.Find("Camera02");
		}
		else if (nc == 2)
		{
			camera3 = GameObject.Find("Camera03");
		}
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.color = new Color(1, 1, 1, 0.7f);
		addScale = 0.5f;
	}

	void Update()
	{
	}

	public void MovePos(Vector3 enemyPos, int frameMax = 15)
	{
		if (frame >= frameMax)
		{
			Vector3 nextPos = transform.position;
			// カメラの位置に合わせる
			if (nc == 0)
			{
				if (transform.position.x > camera1.transform.position.x)
				{
					nextPos.x -= 0.5f;
					if (nextPos.x < camera1.transform.position.x) { nextPos.x = camera1.transform.position.x; }
				}
				else if (transform.position.x < camera1.transform.position.x)
				{
					nextPos.x += 0.5f;
					if (nextPos.x > camera1.transform.position.x) { nextPos.x = camera1.transform.position.x; }
				}
				if (transform.position.y > camera1.transform.position.y)
				{
					nextPos.y -= 0.5f;
					if (nextPos.y < camera1.transform.position.y) { nextPos.y = camera1.transform.position.y; }
				}
				else if (transform.position.y < camera1.transform.position.y)
				{
					nextPos.y += 0.5f;
					if (nextPos.y > camera1.transform.position.y) { nextPos.y = camera1.transform.position.y; }
				}
			}
			else if (nc == 1)
			{
				if (transform.position.x > camera2.transform.position.x)
				{
					nextPos.x -= 2;
					// 比較対象がちがう、と思う
					// ↑たぶん直した
					if (nextPos.x < camera2.transform.position.x) { nextPos.x = camera2.transform.position.x; }
				}
				else if (transform.position.x < camera2.transform.position.x)
				{
					nextPos.x += 2;
					if (nextPos.x > camera2.transform.position.x) { nextPos.x = camera2.transform.position.x; }
				}
				if (transform.position.z > camera2.transform.position.z)
				{
					nextPos.z -= 2;
					if (nextPos.z < camera2.transform.position.z) { nextPos.z = camera2.transform.position.z; }
				}
				else if (transform.position.z < camera2.transform.position.z)
				{
					nextPos.z += 2;
					if (nextPos.z > camera2.transform.position.z) { nextPos.z = camera2.transform.position.z; }
				}
			}
			else if (nc == 2)
			{
				if (transform.position.x > camera3.transform.position.x)
				{
					nextPos.x -= 2;
					// 比較対象がちがう、と思う
					// ↑たぶん直した
					if (nextPos.x < camera3.transform.position.x) { nextPos.x = camera3.transform.position.x; }
				}
				else if (transform.position.x < camera3.transform.position.x)
				{
					nextPos.x += 2;
					if (nextPos.x > camera3.transform.position.x) { nextPos.x = camera3.transform.position.x; }
				}
				if (transform.position.z > camera3.transform.position.z)
				{
					nextPos.z -= 2;
					if (nextPos.z < camera3.transform.position.z) { nextPos.z = camera3.transform.position.z; }
				}
				else if (transform.position.z < camera3.transform.position.z)
				{
					nextPos.z += 2;
					if (nextPos.z > camera3.transform.position.z) { nextPos.z = camera3.transform.position.z; }
				}
			}
			float bringNum = enemyPos.y;
			// カメラに近づける
			if (nc == 2)
			{
				bringNum -= camera3.transform.position.y;
				bringNum /= 8.4f;
				nextPos.y += bringNum;
			}
			else if (nc == 1)
			{
				bringNum -= camera2.transform.position.y;
				bringNum /= 8.4f;
				nextPos.y -= bringNum;
				++posNum;
				if (posNum >= 8)
				{
					spriteRenderer.color = new Color(1, 0, 0);
				}
			}
			else if (nc == 0)
			{
				transform.localScale += new Vector3(addScale, addScale, 0);
			}
			transform.position = nextPos;
			addScale += 0.5f;
			frame = 0;
		}
		frame++;
	}

	//-----------------
	// ダメージを与える
	//-----------------
	public void ColcDamage()
	{
		BGMMaster.instance.SoundEffect (11);
		pc.hp--;
		pc.isDamage = true;
		pc.damageCameraNum = nc;
	}

	//----------------------------------
	// プレイヤーが当たっていない状態にする
	//----------------------------------
	public void OutDamageItem()
	{
		pc.inDamageItem = false;
	}

	void OnTriggerEnter(Collider oth)
	{
		if (oth.tag == "Bullet")
		{
			Bullet b = oth.GetComponent<Bullet>();
			if (posNum >= 8 || b.throughDamageItem)
			{
				Destroy(gameObject);
				b.throughDamageItem = true;
			}
			else
			{
				Destroy(oth.gameObject);
			}
		}
		if (oth.tag == "Player")
		{
			pc.inDamageItem = true;
		}
	}
	void OnTriggerExit(Collider oth)
	{
		if (oth.tag == "Player")
		{
			OutDamageItem();
		}
	}
}

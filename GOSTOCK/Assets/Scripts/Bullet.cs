using UnityEngine;

public class Bullet : MonoBehaviour
{
	public enum BulletStatus
	{
		Normal,
		SecondPower,	// 第二形態
		ThirdPower,		// 第三形態
	}
	Rigidbody rb;
	public float speed;
	public Rigidbody2D rigidbody2d1;
	SpriteRenderer spriteRenderer;
	public BulletStatus status = BulletStatus.Normal;	// バレットの強化状態
	public bool throughDamageItem = false;				// 敵の物理攻撃を貫通したか
	GameObject enemy2;									// 2カメのエネミーオブジェクト

	void Start ()
	{
		rb = GetComponent<Rigidbody>();
		rigidbody2d1 = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (status != BulletStatus.Normal)
		{
			enemy2 = GameObject.Find("Enemy2");
		}
	}
	
	void Update ()
	{
		if (rb && status != BulletStatus.ThirdPower)
		{
			rb.velocity = new Vector3(0, speed, 0);
		}
		else if (rigidbody2d1)
		{
			rigidbody2d1.velocity = new Vector2(0, speed);
		}
		if (enemy2)
		{
			// 強化状態の時のスピード補正
			if (status == BulletStatus.SecondPower)
			{
				// 第二形態の時は敵に少し合わせる
				Vector3 nextPos = transform.position;
				// x
				if (nextPos.x > enemy2.transform.position.x)
				{
					nextPos.x -= 0.035f;
					if (nextPos.x < enemy2.transform.position.x)
					{
						nextPos.x = enemy2.transform.position.x;
					}
				}
				else if (nextPos.x < enemy2.transform.position.x)
				{
					nextPos.x += 0.035f;
					if (nextPos.x > enemy2.transform.position.x)
					{
						nextPos.x = enemy2.transform.position.x;
					}
				}
				// z
				if (nextPos.z > enemy2.transform.position.z)
				{
					nextPos.z -= 0.035f;
					if (nextPos.z < enemy2.transform.position.z)
					{
						nextPos.z = enemy2.transform.position.z;
					}
				}
				else if (nextPos.z < enemy2.transform.position.z)
				{
					nextPos.z += 0.035f;
					if (nextPos.z > enemy2.transform.position.z)
					{
						nextPos.z = enemy2.transform.position.z;
					}
				}
				transform.position = nextPos;
			}
			else if (status == BulletStatus.ThirdPower)
			{
				// 第三形態の時は敵に合わせる
				//rb.velocity = Vector3.MoveTowards(rb.velocity, enemy2.transform.position, 2 * Time.deltaTime);
				Vector3 nextPos = Vector3.MoveTowards(transform.position, enemy2.transform.position, 15 * Time.deltaTime);
				transform.position = nextPos;
			}
		}
		// 座標を過ぎたら黒くしていく
		if (transform.position.y > 60)
		{
			spriteRenderer.color -= new Color(0.01f, 0.01f, 0.01f, 0);
			// 真っ黒になったら消滅
			if (spriteRenderer.color.r <= 0)
			{
				Destroy(gameObject);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.tag == "Enemy")
		{
			Destroy(gameObject);
		}
	}

	void OnTriggerEnter(Collider oth)
	{
		if (oth.tag == "Enemy")
		{
			Destroy(gameObject);
		}
	}
}

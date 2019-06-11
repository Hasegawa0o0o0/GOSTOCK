using UnityEngine;

public class Enemy2Status : MonoBehaviour
{
	public int Hp;
	public bool isDamage = false;
	PlayerControl playerControl;        // プレイヤー情報
	public int cnt = 0;
	void Start()
	{
		playerControl = FindObjectOfType<PlayerControl>();
	}

	void OnTriggerEnter(Collider oth)
	{
		if (oth.tag == "Bullet")
		{
			Bullet b = oth.GetComponent<Bullet>();
			int damage = 1;
			//// バレットの状態でダメージを変える
			//if (b.status == Bullet.BulletStatus.SecondPower)
			//{
			//	damage = 2;
			//}
			//else if (b.status == Bullet.BulletStatus.ThirdPower)
			//{
			//	damage = 3;
			//}
			// バレットが物理攻撃を貫通していたらダメージを加算
			if (b.throughDamageItem)
			{
				damage = 2;
			}
			isDamage = true;
			Hp -= damage;
		}
	}
	void OnTriggerStay(Collider oth)
	{
		
		if (oth.tag == "Camera")
		{
			// バレットのパワーアップ
			playerControl.PowerUpBullet();
			//if (cameramaster.nowCamera == 1) {
			//	BGMMaster.instance.SoundEffect3 (17);
			//}
		}
	}
	void OnTriggerExit(Collider oth)
	{
		if (oth.tag == "Camera")
		{
			// バレットの強化状態のリセット
			playerControl.InitBullet();
			BGMMaster.instance.frameResette ();
		}
	}
}

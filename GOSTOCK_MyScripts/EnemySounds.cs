/*
 * 2019.01.08 作成
*/
/* 敵関係のサウンドを管理する */
using System.Collections.Generic;
using UnityEngine;

public class EnemySounds : MonoBehaviour
{
	public enum SoundArray
	{
		eZakoAttack = 0,		// ザコが体当たりしてくるとき
		eBossEntering = 1,		// ボス入場音
		eBossDamage = 2,		// ボスがダメージを受けたとき
		eBossPunch = 3,			// ボスがパンチをするとき
		eBossGuard = 4,			// ボスがガードをするとき
		eBossWarp = 5,			// ボスがワープしたとき
		eBossInhale = 6,		// ボスがオバケを吸い込むとき
		eBossGuardHit = 7,		// ボスがガード後、プレイヤーに攻撃を受けた(=カウンター)時 2019.03.26
		eZakoParentBook = 8,	// 中ボスの本が開いた時
		eBossLetterGet = 9,		// ボスが手紙を落とした時の音
		eBossDie = 10,			// ボスが死んだときの(ゴゴゴゴゴ的な)音
	}

	AudioSource audioSource;
	public List<AudioClip> soundsList = new List<AudioClip>();
	public static EnemySounds instance;

	void Start ()
	{
		audioSource = GetComponent<AudioSource>();
		instance = FindObjectOfType<EnemySounds>();
	}
	
	public void SoundOneShot(SoundArray sa)
	{
		audioSource.PlayOneShot(soundsList[(int)sa]);
	}
}

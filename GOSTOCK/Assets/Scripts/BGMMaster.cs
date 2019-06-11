/*
 * 2018.06.26　SE再生システム確立
 * 2018.06.29 StartからAwakeに変更
 *            インスタンスの初期化を変更
 * 
 * 2019.03.03 現在時点で使ってないものは×、使っているものには○を。
 * 
 * 音入ってる番号
 * ×0		BGM
 * ○1		cameraSE
 * 2		cannon2 つかわない
 * 3		shot1 使わない
 * 4		お化け照準
 * ○5		オバケ消滅
 * 6		カメラ切り替え
 * 7		ゲームオーバー
 * 8		タイトル		使わない
 * 9		タイトルスポット 後
 * 10		ブザー		後
 * ○11		プレイヤーダメージ
 * ○12		プレイヤー攻撃1		エネルギーがある状態
 * ○13		プレイヤー攻撃2		エネルギーがない状態
 * 14		モード選択	後
 * 15			リザルト
 * 16		幕開け		後
 * 17		敵 ロックオン
 * 18		敵攻撃１		使わない
 * 19		敵攻撃２
 * 20		魂回収
 * 21		bell
 * 22		halloween
 * ×23		カウント(ぽっ)
 * ×24		カウント(ピー)
 * ○25		ゲージ上昇(メモリVer)
 * ○26		ゲージコア
 */
using UnityEngine;

public class BGMMaster : MonoBehaviour {
	public AudioSource AS;
	public AudioClip[] audio;
	public static BGMMaster instance;
	int frame = 0;
	void Awake () 
	{
		if (instance) { Destroy(instance); }
		instance = FindObjectOfType<BGMMaster>();
		AS = gameObject.GetComponent<AudioSource> ();
	}

	public void SoundEffect(int i)
	{
		AS.PlayOneShot(audio[i]);
	}
	public void SoundEffect3 (int i)
	{
		if (frame == 0 || frame == 60 || frame == 240|| frame == 70 || frame == 250 || frame == 260)
		{
			AS.PlayOneShot(audio[i]);
		}
		frame++;
	}
	public void frameResette()
	{
		frame = 0;
	}
}

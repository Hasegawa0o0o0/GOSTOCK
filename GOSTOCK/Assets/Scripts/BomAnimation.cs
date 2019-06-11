// 爆発アニメーション
//		2018.12.06 要素数を変更
using UnityEngine;

public class BomAnimation : MonoBehaviour
{
	Anim anim;
	public Sprite[] bomSpriteList = new Sprite[9];
	int frame = 0;
	public float destroyFrame;

	void Start ()
	{
		anim = GetComponent<Anim>();
	}
	
	void Update ()
	{
		if (frame > destroyFrame)
		{
			Destroy(gameObject);
		}
		anim.Animation(bomSpriteList, 13);
		++frame;
	}
}

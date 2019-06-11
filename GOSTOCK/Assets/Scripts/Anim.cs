//-----------------------------
// Anim.cs
// アニメーションクラス
//-----------------------------
using UnityEngine;

public class Anim : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;
	int drawingSpriteIndex = 0;
	int drawingSpriteFrame = 0;
	public int drawingSpriteFrameMax;

	void Start ()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	void Update ()
	{
	}

	// 取得したスプライトでアニメーションさせる関数
	// spriteList	アニメーションさせるスプライト
	// indexMax		配列の大きさ
	public void Animation(Sprite[] spriteList, int indexMax, bool increment = true)
	{
		// 2018.07.18
		if (Time.timeScale == 0)
		{
			return;
		}
		spriteRenderer.enabled = true;
		// 表示しているフレームを更新
		++drawingSpriteFrame;
		// 表示するフレームを超えたらスプライトを変更
		if (drawingSpriteFrame > drawingSpriteFrameMax)
		{
			// 表示しているフレームの初期化
			drawingSpriteFrame = 0;
			// 表示するスプライトを変える
			if (increment)
			{
				++drawingSpriteIndex;
			}
			else
			{
				--drawingSpriteIndex;
			}
			// スプライトの最大値を超えたら0に戻す
			if (drawingSpriteIndex >= indexMax)
			{
				drawingSpriteIndex = 0;
			}
			else if (drawingSpriteIndex < 0)
			{
				drawingSpriteIndex = indexMax - 1;
			}
			spriteRenderer.sprite = spriteList[drawingSpriteIndex];
		}
	}

	// スプライトを非表示にする関数
	public void OffSprite()
	{
		spriteRenderer.enabled = false;
	}
}

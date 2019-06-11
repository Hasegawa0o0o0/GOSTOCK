/*---------------------------------------------------------------------------------------------------
// タイトル画面演出03
//
* FileName		: TitleLight.cpp
* Author		: 17CU0116 杉田優帆
* Log			: 2018.06.01 作成
*				: 2018.06.11 スポットライト風のやつに手直し  長谷川
---------------------------------------------------------------------------------------------------*/
using UnityEngine;

public class TitleLight : MonoBehaviour
{
	// 変数宣言
	public Rigidbody2D rigidbody2D;
	SpriteRenderer spriteRenderer;
	public SpriteRenderer darkSpriteRenderer;	// バックのスプライトレンダラー(unity側設定)
	public Vector2 speed;
	public bool isCalcX = true;
	public bool isCalcY = true;
	public int stayFrameX = 0;
	public int stayFrameY = 0;
	public int activeFrame = 0;

	void Start()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	void Update ()
	{
	}
	// タイトルが表示されている時の動き
	public void MoveTitleScene()
	{
		if (isCalcX && stayFrameX == 0)
		{
			speed.x += Random.Range(0, 0.3f);
			if (speed.x >= 2.5f)
			{
				isCalcX = false;
				stayFrameX = 350;
			}
		}
		else if (!isCalcX && stayFrameX == 0)
		{
			speed.x -= Random.Range(0, 0.3f);
			if (speed.x <= -2.5f)
			{
				isCalcX = true;
				stayFrameX = 350;
			}
		}
		else
		{
			stayFrameX--;
		}
		if (isCalcY && stayFrameY == 0)
		{
			speed.y += Random.Range(0, 0.1f);
			if (speed.y >= 2.5f)
			{
				isCalcY = false;
				stayFrameY = 300;
			}
		}
		else if (!isCalcY && stayFrameY == 0)
		{
			speed.y -= Random.Range(0, 0.1f);
			if (speed.y <= -2.5f)
			{
				isCalcY = true;
				stayFrameY = 300;
			}
		}
		else
		{
			stayFrameY--;
		}
		// 移動制限
		if (transform.localPosition.x >= 5.5f)
		{
			isCalcX = false;
			speed.x += -0.2f;
		}
		else if (transform.localPosition.x <= -5.5f)
		{
			isCalcX = true;
			speed.x += 0.2f;
		}
		if (transform.localPosition.y >= 1.4f)
		{
			isCalcY = false;
			speed.y += -0.2f;
		}
		else if (transform.localPosition.y <= -1)
		{
			isCalcY = true;
			speed.y += 0.2f;
		}
		if (rigidbody2D)
		{
			rigidbody2D.velocity = speed;
		}
	}
	// 選択画面が表示されているときの動き
	public void MoveSelect()
	{
		transform.position = Vector2.MoveTowards(transform.position, Vector2.zero, 1 * Time.deltaTime);
		transform.localScale += new Vector3(0.085f, 0.085f, 0.085f);
		if (spriteRenderer)
		{
			spriteRenderer.color -= new Color(0, 0, 0, 0.005f);
		}
		darkSpriteRenderer.color -= new Color(0, 0, 0, 0.005f);
		if (rigidbody2D)
		{
			rigidbody2D.velocity = Vector2.zero;
		}
	}
	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.tag == "Goast")
		{
			Destroy(col.gameObject);
		}
	}
}

//----------------------------------
// 2カメの背景のスクロール
// Scroll02.cs
// 17CU0116 杉田
// 2018.05.23 スクロールを2カメに対応
// ?????????? 画面奥に行くにつれて暗くなる
// 2018.07.12 蜘蛛の巣がある場合の処理を追加
// 2018.07.13 Pause中は動かないように
// 2018.11.20 デバックキーの追加(正方向:true、逆方向:false)
// 2018.12.31 シャンデリアを揺らす
// 2019.03.10 親子関係にあるオブジェクトのSpriteRendererの取得、管理を変更
//----------------------------------
using UnityEngine;
using System.Collections.Generic;

public class Scroll02 : MonoBehaviour
{
	// 変数
	public float scrollSpeed = 0.05f;
	private string tagName;
	SpriteRenderer spriteRenderer;
	SpriteRenderer spiderwebSp;
	List<SpriteRenderer> allChildrenSRList = new List<SpriteRenderer>();	// 全ての子どものSpriteRenderer 2019.03.10
	// [Re]2018.12.31
	Transform chandelier;			// シャンデリア
	float side;						// 傾いている角度
	float addSide = 0.25f;			// 傾ける度合い

	// [Re]
	private bool reversalFlag = true;
	private bool stopFlag = false;

	void Start ()
	{
		tagName = gameObject.tag;
		spriteRenderer = GetComponent<SpriteRenderer>();
		GetAllChildrenComponent<SpriteRenderer>(transform, allChildrenSRList);
		//2018.07.12
		Transform t = transform.Find("Spiderweb");
		//GameObject o = transform.Find("Spiderweb").gameObject;
		if (t)
		{
			spiderwebSp = t.GetComponent<SpriteRenderer>();
		}
		chandelier = transform.Find("Chandelier");
	}
	
	void Update ()
	{
		// [Re]2018.11.20
		if (Input.GetKeyDown(KeyCode.Home))
		{
			if (reversalFlag == true)
			{
				reversalFlag = false;
			}
			else
			{
				reversalFlag = true;
			}
		}
		// [Re]2018.11.20
		if (Input.GetKeyDown(KeyCode.End))
		{
			if (stopFlag == true)
			{
				stopFlag = false;
			}
			else
			{
				stopFlag = true;
			}
		}

		// Pause中は動かないように 2018.07.13
		if (Time.timeScale == 0)
		{
			return;
		}
		// シャンデリアがあったら揺らす [Re]2018.12.31
		if (chandelier)
		{
			chandelier.RotateAround(new Vector3(chandelier.position.x, chandelier.position.y, chandelier.position.z - 4f),
				new Vector3(0f, side, 0f), 3f * Time.deltaTime);
			side -= addSide;
			if (side > 5 || side < -5) { addSide *= -1; }
		}

		//[Re]2018.11.20
		if (stopFlag == true)
		{
			transform.position += new Vector3(0, 0, 0);
		}
		else
		{
			if (reversalFlag == true)
			{
				transform.position -= new Vector3(0, scrollSpeed, 0);

				if (tagName == "RightWall")
				{
					if (transform.position.y < -71f)
					{
						// X,Z軸はいじるべからずf Y軸の細かな編集
						transform.position = new Vector3(-19.0f, 86.3f, -9.5f);
						//spriteRenderer.color = new Color(0, 0, 0);
					}
				}
				if (tagName == "LeftWall")
				{
					if (transform.position.y < -71f)
					{
						// X,Z軸はいじるべからずf Y軸の細かな編集
						transform.position = new Vector3(-47.0f, 86.3f, -9.5f);
					}
				}
				if (tagName == "Untagged")
				{
					if (transform.position.y < -71f)
					{
						transform.position = new Vector3(-33.0f, 86.3f, 0.0f);
					}
				}
				if (tagName == "Roof")
				{
					if (transform.position.y < -71f)
					{
						transform.position = new Vector3(-33.0f, 86.3f, -19f);
					}
				}
			}
			else
			{
				transform.position += new Vector3(0, scrollSpeed, 0);

				if (tagName == "RightWall")
				{
					if (transform.position.y > 86.3f)
					{
						// X,Z軸はいじるべからずf Y軸の細かな編集
						transform.position = new Vector3(-19.0f, -71, -9.5f);
						//spriteRenderer.color = new Color(0, 0, 0);
					}
				}
				if (tagName == "LeftWall")
				{
					if (transform.position.y > 86.3f)
					{
						// X,Z軸はいじるべからずf Y軸の細かな編集
						transform.position = new Vector3(-47.0f, -71, -9.5f);
					}
				}
				if (tagName == "Untagged")
				{
					if (transform.position.y > 86.3f)
					{
						transform.position = new Vector3(-33.0f, -71, 0.0f);
					}
				}
				if (tagName == "Roof")
				{
					if (transform.position.y > 86.3f)
					{
						transform.position = new Vector3(-33.0f, -71, -19f);
					}
				}
			}
		}

		// 2018.07.12
		Color color = Color.white;
		// 色を黒くする
		if (transform.position.y > -23 && transform.position.y < 50.4f)
		{
			color = Color.white;
		}
		else if (transform.position.y > -33.5f && transform.position.y < 60.9f)
		{
			color = new Color(0.75f, 0.75f, 0.75f);
		}
		else if (transform.position.y > -44 && transform.position.y < 71.4f)
		{
			color = new Color(0.5f, 0.5f, 0.5f);
		}
		else if (transform.position.y > -54.5f && transform.position.y < 81.9f)
		{
			color = new Color(0.25f, 0.25f, 0.25f);
		}
		else
		{
			color = new Color(0, 0, 0);
		}
		// ボスが撃破されていたらスピードを落とし暗くする 2019.03.10	2019.04.03
		if (EnemyManager.bossDefeat || EnemyManager.bossStaging)
		{
			color /= 1.5f;
			color.a = 1f;
			// 条件追加 2019.04.03
			if (EnemyManager.bossDefeat)
			{
				scrollSpeed = Mathf.Lerp(scrollSpeed, 0f, 1.5f * Time.deltaTime);
			}
		}
		spriteRenderer.color = color;
		if (spiderwebSp) { spiderwebSp.color = color; }	// 2018.07.12
		// 2019.03.10
		for (int i = 0; i < allChildrenSRList.Count; ++i)
		{
			allChildrenSRList[i].color = color;
		}
	}

	// 全ての子どもの情報を取得する再帰関数 2019.03.10-----------------------------------
	void GetAllChildrenComponent<T>(Transform t, List<T> list)where T : Component
	{
		foreach(Transform child in t)
		{
			T item = child.GetComponent<T>();
			if (item != null) { list.Add(item); }
			GetAllChildrenComponent<T>(child, list);
		}
	}
}

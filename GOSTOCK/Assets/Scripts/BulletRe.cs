/* -----------------------------------------------------------------------------------------
 * ■ BulletRe.cs
 *		2018.11.25 作成(本来のBullet.csから分離)
 *		2018.11.25 画面中央から発射してターゲットへ向かうのに変更
 *		2018.11.27 当たった回数をカウント
 *		2019.01.29 弾道を修正
 *		2019.01.31 消える感じを修正、スコアの仕様変更
 *		2019.02.25 被弾エフェクトの追加
 * ---------------------------------------------------------------------------------------
 *		moveJudgeの仕様(0は未使用)→未使用
 *		┌─┬─┬─┐
 *		│１│２│３│
 *		├─┼─┼─┤
 *		│４│５│６│
 *		├─┼─┼─┤
 *		│７│８│９│
 *		└─┴─┴─┘
 ----------------------------------------------------------------------------------------- */
using UnityEngine;

public class BulletRe : MonoBehaviour
{
	public float speed;
	private Rigidbody rb;
	private SpriteRenderer spriteRenderer;

	private PlayerControl playerControl;
	private GameObject cameraMain;
	public Vector3 cursorVec;
	public Vector3 cursorVec2nd;

	public ParticleSystem fireworkPar;				// 被弾エフェクト用パーティクル

	//public int moveJudge = 0;

	void Start ()
	{
		rb = GetComponent<Rigidbody>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		playerControl = FindObjectOfType<PlayerControl>();
		cursorVec = playerControl.cursorobj.transform.position;
		cursorVec2nd = cursorVec;
		//// moveJudgeの判断、次の行き先決定
		//if (cursorVec.z >= -7)
		//{
		//	if (cursorVec.x <= -37)
		//	{
		//		moveJudge = 7;
		//		cursorVec2nd = new Vector3(cursorVec.x - 10, cursorVec.y + 10, cursorVec.z + 5);
		//	}
		//	else if (cursorVec.x >= -30)
		//	{
		//		moveJudge = 9;
		//		cursorVec2nd = new Vector3(cursorVec.x + 10, cursorVec.y + 10, cursorVec.z + 5);
		//	}
		//	else
		//	{
		//		moveJudge = 8;
		//		cursorVec2nd = new Vector3(cursorVec.x, cursorVec.y + 10, cursorVec.z + 5);
		//	}
		//}
		//else if(cursorVec.z <= -12)
		//{
		//	if (cursorVec.x <= -37)
		//	{
		//		moveJudge = 1;
		//		cursorVec2nd = new Vector3(cursorVec.x - 10, cursorVec.y + 10, cursorVec.z - 5);
		//	}
		//	else if (cursorVec.x >= -30)
		//	{
		//		moveJudge = 3;
		//		cursorVec2nd = new Vector3(cursorVec.x + 10, cursorVec.y + 10, cursorVec.z - 5);
		//	}
		//	else
		//	{
		//		moveJudge = 2;
		//		cursorVec2nd = new Vector3(cursorVec.x, cursorVec.y + 10, cursorVec.z - 5);
		//	}
		//}
		//else
		//{
		//	if (cursorVec.x <= -37)
		//	{
		//		moveJudge = 4;
		//		cursorVec2nd = new Vector3(cursorVec.x - 10, cursorVec.y + 10, cursorVec.z);
		//	}
		//	else if (cursorVec.x >= -30)
		//	{
		//		moveJudge = 6;
		//		cursorVec2nd = new Vector3(cursorVec.x + 10, cursorVec.y + 10, cursorVec.z);
		//	}
		//	else
		//	{
		//		moveJudge = 5;
		//		cursorVec2nd = new Vector3(cursorVec.x, cursorVec.y + 10, cursorVec.z);
		//	}
		//}
		//cursorVec2nd = new Vector3(cursorVec.x, cursorVec.y + 10, cursorVec.z);			// これがしっくり？
	}
	
	void Update ()
	{
		// カーソルの指す場所へ移動
		if (transform.position.y > 6.5)
		{
			//// 2個めの行き先へ行く
			//Vector3 nextPos = Vector3.Lerp(transform.position, cursorVec2nd, Time.deltaTime * 5);
			//transform.position = nextPos;
			// ぼや―って消えるように
			//transform.localScale -= new Vector3(0.1f, 0, 0);

			Vector3 nextPos = Vector3.Lerp(transform.position, cursorVec, Time.deltaTime * 5);
			transform.position = nextPos;

			spriteRenderer.color -= new Color(0f, 0f, 0f, 0.1f);
			// 透明になったら消滅
			if (spriteRenderer.color.a <= 0)
			{
				Destroy(gameObject);
				playerControl.shotNoHit++;			// 2019.01.31
				//Debug.Log("NoHit");
			}
		}
		else
		{
			Vector3 nextPos = Vector3.Lerp(transform.position, cursorVec, Time.deltaTime * 5);
			transform.position = nextPos;
		}
		//// 座標を過ぎたら黒くしていく
		//if (transform.position.y > 60)
		//{
		//	spriteRenderer.color -= new Color(0.01f, 0.01f, 0.01f, 0);
		//	// 真っ黒になったら消滅
		//	if (spriteRenderer.color.r <= 0)
		//	{
		//		Destroy(gameObject);
		//	}
		//}
	}

	//void OnTriggerEnter2D(Collider2D col)
	//{
	//	if (col.tag == "Enemy")
	//	{
	//		Destroy(gameObject);
	//	}
	//}

	void OnTriggerEnter(Collider oth)
	{
		if (oth.tag == "Enemy")
		{
			//playerControl.shotHit++;			// 2018.11.27
			Destroy(gameObject);
			//if (playerControl.parFlag == true)
			//{
			//	Instantiate(fireworkPar, transform.position, fireworkPar.transform.localRotation);
			//}
		}
		//if (oth.tag == "Clockwork")				// 2018.12.06
		//{
		//	Debug.Log("時計");
		//	Destroy(gameObject);
		//	oth.gameObject.SetActive(false);
		//}
	}
}

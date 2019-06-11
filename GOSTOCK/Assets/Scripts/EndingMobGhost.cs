/* -----------------------------------------------------------------------------------------
 * EndingMobGhost.cs
 * 2018.12.23 作成
 *		2018.12.25 動きの修正
 ----------------------------------------------------------------------------------------- */
using UnityEngine;

public class EndingMobGhost : MonoBehaviour
{
	// モブ(座ったオバケ、ついでに司会(?)のオバケも)の動き
	public enum MobMove
	{
		HOST,			// 司会者
		NORMAL,			// 特に動きなし、ノーマル
		SIDE,			// 横移動
		LEN,			// 縦移動
	}

	public MobMove mobMove;						// 座ったオバケ達の動き
	private MobMove oldMove;					// 以前の動きを保存

	// 動き関連
	private Vector3 originVec;					// 元の位置
	public float amountMove = 0.01f;			// ふわふわする移動量
	private bool exchange = false;				// 移動先変更

	void Start ()
	{
		oldMove = mobMove;
		originVec = transform.position;
	}
	
	void Update ()
	{
		if (mobMove == MobMove.HOST)
		{
			HostMove();
		}
		else if (mobMove == MobMove.NORMAL)
		{
			transform.position = Vector3.Lerp(transform.position, originVec, Time.deltaTime * 5);
			// 150分の1
			if (Random.Range(0, 150) == 1)
			{
				mobMove = oldMove;
			}
		}
		else if (mobMove == MobMove.SIDE)
		{
			ChairSide();
		}
		else if(mobMove == MobMove.LEN)
		{
			ChairLen();
		}
	}

	// ホスト(司会者)の動き----------------------------------------------------------------
	private void HostMove()
	{
		// 縦にふわふわ？
		if (exchange == true)
		{
			if (transform.position.y >= originVec.y + 1)
			{
				exchange = false;
			}
			else
			{
				transform.position += new Vector3(0, amountMove, 0);
			}
		}
		else
		{
			if (transform.position.y <= originVec.y - 1)
			{
				exchange = true;
			}
			else
			{
				transform.position -= new Vector3(0, amountMove, 0);
			}
		}
	}

	// 椅子での横移動の動き---------------------------------------------------------------
	private void ChairSide()
	{
		// 横にゆらゆら？
		if (exchange == true)
		{
			if (transform.position.x >= originVec.x + 0.3f)
			{
				exchange = false;
			}
			else
			{
				transform.position += new Vector3(amountMove, 0, 0);
			}
		}
		else
		{
			if (transform.position.x <= originVec.x - 0.3f)
			{
				exchange = true;
			}
			else
			{
				transform.position -= new Vector3(amountMove, 0, 0);
			}
		}
		// 255分の1
		if (Random.Range(0, 255) == 1)
		{
			mobMove = MobMove.NORMAL;
		}
	}

	// 椅子での縦移動の動き---------------------------------------------------------------
	private void ChairLen()
	{
		// 縦にゆらゆら？
		if (exchange == true)
		{
			if (transform.position.y >= originVec.y + 0.1f)
			{
				exchange = false;
			}
			else
			{
				transform.position += new Vector3(0, amountMove, 0);
			}
		}
		else
		{
			if (transform.position.y <= originVec.y - 0.1f)
			{
				exchange = true;
			}
			else
			{
				transform.position -= new Vector3(0, amountMove, 0);
			}
		}
		// 255分の1
		if (Random.Range(0, 255) == 1)
		{
			mobMove = MobMove.NORMAL;
		}
	}
}

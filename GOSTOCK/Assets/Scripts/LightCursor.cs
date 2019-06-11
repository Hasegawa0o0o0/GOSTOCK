using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCursor : MonoBehaviour
{
	public PlayerControl pc;			// unity側設定
	public Rigidbody rigidbody;
	public SpriteRenderer spriteRenderer;
	public Sprite lightOn;
	public Sprite lightOff;
	public Vector3 speed;
	public bool isCalcX = true;
	public bool isCalcZ = true;
	public int stayFrameX = 0;
	public int stayFrameZ = 0;
	public int activeFrame = 0;
	
	void Start ()
	{
		rigidbody = GetComponent<Rigidbody>();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	void Update ()
	{
	}

	// 動かす処理
	public void MoveCursor()
	{
		// X軸の処理
		// 増やす処理
		if (isCalcX && stayFrameX == 0)
		{
			speed.x += Random.Range(0,0.3f);
			// スピードが1.5を超えないように制限し、減らす処理に移る
			if (speed.x >= 1.5f)
			{
				isCalcX = false;
				stayFrameX = 120;
			}
		}
		// 減らす処理
		else if (!isCalcX && stayFrameX == 0)
		{
			speed.x -= Random.Range(0, 0.3f);
			// スピードが-1.5を下回らないように制限し、減らす処理に移る
			if (speed.x <= -1.5f)
			{
				isCalcX = true;
				stayFrameX = 120;
			}
		}
		else
		{
			stayFrameX--;
		}
		// Z軸の処理
		// 増やす処理
		if (isCalcZ && stayFrameZ == 0)
		{
			speed.z += Random.Range(0, 0.1f);
			// スピードが1を超えないように制限し、減らす処理に移る
			if (speed.z >= 1)
			{
				isCalcZ = false;
				stayFrameZ = 180;
			}
		}
		// 減らす処理
		else if (!isCalcZ && stayFrameZ == 0)
		{
			speed.z -= Random.Range(0, 0.1f);
			// スピードが-1を下回らないように制限し、減らす処理に移る
			if (speed.z <= -1)
			{
				isCalcZ = true;
				stayFrameZ = 180;
			}
		}
		else
		{
			stayFrameZ--;
		}
		SetSpeed();
	}

	public void CheckPos()
	{
		// 画面外に行かないように
		// なんか違う↓
		// ↑解決
		if (transform.localPosition.x >= 10.5f)
		{
			speed.x = -1;
			isCalcX = false;
			stayFrameX = 360;
		}
		else if (transform.localPosition.x <= -10.5f)
		{
			speed.x = 1;
			isCalcX = true;
			stayFrameX = 360;
		}
		if (transform.localPosition.y >= 4.5f)
		{
			speed.z = 1;
			isCalcZ = false;
			stayFrameZ = 360;
		}
		else if (transform.localPosition.y <= -4.5f)
		{
			speed.z = -1;
			isCalcZ = true;
			stayFrameZ = 360;
		}
	}

	public void SetSpeed()
	{
		rigidbody.velocity = speed;
	}
}

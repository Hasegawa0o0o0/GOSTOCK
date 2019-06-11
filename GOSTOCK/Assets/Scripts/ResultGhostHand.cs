using UnityEngine;
using UnityEngine.UI;

public class ResultGhostHand : MonoBehaviour
{
	// 変数
	public Image Left;                  // 右手
	public Image Right;                 // 左手
	private int frame = 0;              // カウント用
	public int MaxFrame;                // 手を叩く速度(Unity上で変更可)
	private float move = 7.5f;          // 叩く用の移動距離
	public bool LRFlag = false;         // 左右反転

	void Update()
	{
		++frame;
		if (LRFlag == false)
		{
			if (frame == MaxFrame)
			{
				Left.transform.position += new Vector3(move, 0.0f, 0.0f);
				Right.transform.position += new Vector3(-move, 0.0f, 0.0f);
			}
			if (frame == MaxFrame + 10)
			{
				Left.transform.position += new Vector3(-move, 0.0f, 0.0f);
				Right.transform.position += new Vector3(move, 0.0f, 0.0f);
				frame = 0;
			}
		}
		else
		{
			if (frame == MaxFrame)
			{
				Left.transform.position += new Vector3(-move, 0.0f, 0.0f);
				Right.transform.position += new Vector3(move, 0.0f, 0.0f);
			}
			if (frame == MaxFrame + 10)
			{
				Left.transform.position += new Vector3(move, 0.0f, 0.0f);
				Right.transform.position += new Vector3(-move, 0.0f, 0.0f);
				frame = 0;
			}
		}
	}
}

//----------------------------------
// 背景のスクロール
// Scroll.cs
// 17CU0126 長谷川勇太
// 2018.05.18
// 2018.07.13 Pause中は動かないように
//----------------------------------
using UnityEngine;

public class Scroll : MonoBehaviour
{
	// 変数
	public float scrollSpeed = 0.05f;

	void Start ()
	{
		
	}
	
	void Update ()
	{
		// Pause中は動かないように 2018.07.13
		if (Time.timeScale == 0)
		{
			return;
		}
		transform.localPosition += new Vector3(0, scrollSpeed, 0);
		if (transform.localPosition.y > 15)
		{
			transform.localPosition = new Vector3(0, -14.9f, transform.localPosition.z);
		}
	}
}

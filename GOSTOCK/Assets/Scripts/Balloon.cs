using UnityEngine;
using UnityEngine.UI;

public class Balloon : MonoBehaviour
{
	// 変数
	private int Frame = 0;
	private Image balloon;
	RectTransform balloonRect;

	void Start ()
	{
		balloon= GetComponent<Image>();
		balloonRect = GetComponent<RectTransform>();
		balloonRect.sizeDelta = new Vector2(10.0f, 10.0f);
	}
	
	void Update ()
	{
		++Frame;
		if (Frame <= 30)
		{
			balloonRect.sizeDelta += new Vector2(22.0f, 18.0f);
		}
		if (Frame == 31)
		{
			balloonRect.sizeDelta = new Vector2(758.6f, 629.1f);
		}
	}
}

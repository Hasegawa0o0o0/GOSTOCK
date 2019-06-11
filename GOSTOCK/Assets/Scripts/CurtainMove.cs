using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurtainMove : MonoBehaviour
{
	public GameObject topCurtain;
	public GameObject rightCurtain;
	public GameObject leftCurtain;

	void Start ()
	{
		
	}
	
	void Update ()
	{
		
	}

	// カーテンを動かす関数
	public void Move(Vector3 topTarget, Vector3 rightTarget, Vector3 leftTarget, float topSpeed, float sideSpeed)
	{
		topCurtain.transform.localPosition = Vector3.Lerp(topCurtain.transform.localPosition, topTarget, topSpeed * Time.deltaTime);
		rightCurtain.transform.localPosition = Vector3.Lerp(rightCurtain.transform.localPosition, rightTarget, sideSpeed * Time.deltaTime);
		leftCurtain.transform.localPosition = Vector3.Lerp(leftCurtain.transform.localPosition, leftTarget, sideSpeed * Time.deltaTime);
	}
}

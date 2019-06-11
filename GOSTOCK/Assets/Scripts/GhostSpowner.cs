using UnityEngine;
using System.Collections;

public class GhostSpowner : MonoBehaviour
{
	// 各オブジェクト宣言
	public GameObject ghostPrefab;	// ゴーストプレハブ
	private int frame = 0;          // フレーム数管理
	public int lateFrame;			// 初めての生成までの遅延フレーム
	public int frameMax = 0;
	public float MinSpeed;	// 最低速度
	public float maxSpeed;	// 最大速度
	public bool isLeft;     // 左回りかどうか
	public bool isTop;		// 始点が上かどうか
	public float curve;		// カーブの大きさ
	public float zSpeed;
	//public static GhostSpowner instance;

	void Start ()
	{
		//instance = FindObjectOfType<GhostSpowner>();
		frame = -lateFrame;
	}

	void Update ()
	{
		// Pause中は動かないように 2018.07.13
		if (Time.timeScale == 0)
		{
			return;
		}
		// フレームカウント
		frame++;
		// ゴースト発射
		if(frame>=frameMax)
		{
			InsGhost();
			frame = 0;
		}
	}

	public void InsGhost()
	{
		// 生成したおばけに情報を渡すための変数
		GameObject ghostObj;
		// 生成
		ghostObj = Instantiate(ghostPrefab, this.transform.position, ghostPrefab.transform.rotation);
		GhostAction ga = ghostObj.GetComponent<GhostAction>();
		// 情報を代入
		// スピード
		float randomSpeed = Random.Range(MinSpeed, maxSpeed);	// 最低から最大までのランダムなスピードを求める
		ga.speed = randomSpeed;
		// 左回りかどうか
		ga.isLeft = isLeft;
		// 円の頂点かどうか
		if ((isTop && !isLeft) || (!isTop && isLeft)) { ga.direction = 0;}
		else if ((!isTop && !isLeft) || (isTop && isLeft)) { ga.direction = 180; }
		// カーブの大きさ?
		ga.curve = curve;
		// Zスピード
		ga.zSpeed = zSpeed;
		// おばけのタイプを設定する
		// デフォルトでNormal
		ga.ghostType = GhostAction.GhostType.Normal;
		// 50%の確率でNormalではなくなる
		int selectGhost = Random.Range(0, 2);
		if (selectGhost < 1)
		{
			ga.ghostType = GhostAction.GhostType.GoldFish;
			// 25%の確率でレアなおばけになる
			selectGhost = Random.Range(0, 2);
			if (selectGhost < 1)
			{
				// 12.5%の確率で反射状態になる
				selectGhost = Random.Range(0, 2);
				if (selectGhost < 1)
				{
					ga.ghostType = GhostAction.GhostType.Flip;
				}
				// 12.5%の確率で波形状態になる
				else
				{
					ga.ghostType = GhostAction.GhostType.Wave;
				}
			}
		}
		//ga.ghostType = GhostAction.GhostType.Flip;
		// どこのスポナー産か
		//GhostMaster.instance.GetMyNumber(gameObject, ga.data);
	}
}

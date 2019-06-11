// ゴーストスポナーを追加 2018.06.30
using UnityEngine;

public class GhostMaster : MonoBehaviour
{
	const int cSpownerNum = 100;
	public int spownerNum;
	public GameObject[,,] ghostObj = new GameObject[2, cSpownerNum, 100];	// 左から、1,2カメどちらか、何番目のスポナーか、何番目のおばけか
	GameObject[,] spowners = new GameObject[cSpownerNum, 2];                // 左から、何番目のスポナーか、1,2カメどちらか
	public GameObject[] inSpowner = new GameObject[cSpownerNum * 2];        // スポナー代入用
	public GhostSpowner[] ghostSpowner = new GhostSpowner[100];// 2018.06.30
	public static GhostMaster instance;

	void Start ()
	{
		instance = FindObjectOfType<GhostMaster>();
		int inNum = 0;
		// 2次元配列にUnity側で入れたスポナーを代入
		for (int i = 0; i < cSpownerNum; ++i)
		{
			for (int j = 0; j < 2; ++j, ++inNum)
			{
				spowners[i, j] = inSpowner[inNum];
				ghostSpowner[inNum] = inSpowner[inNum].GetComponent<GhostSpowner>();
				++spownerNum;
			}
			// 入ってなかったら終了
			if (inSpowner[inNum] == null)
			{
				break;
			}
		}
	}
	
	void Update ()
	{
	}
	//------------------------------------------
	// spowner		どこのスポナーか
	// data			おばけに渡すデータポインタ変数
	//------------------------------------------
	public void GetMyNumber(GameObject spowner, int[] data)
	{
		int i;
		// どこのスポナーか検索する
		for (i = 0; i < cSpownerNum; ++i)
		{
			// 見つけたらデータにどちらのカメラか代入し検索終了
			if (spowner == spowners[i,0])
			{
				data[GhostAction.cIsCamera] = 0;
				break;
			}
			else if (spowner == spowners[i,1])
			{
				data[GhostAction.cIsCamera] = 1;
				break;
			}
		}
		// どこのスポナーかを代入
		data[GhostAction.cWhereSpowner] = i;
		// 特定されたスポナーの何番目のおばけか確かめる
		for (int j = 0; j < 100; ++j)
		{
			// 開いていたら代入
			if (ghostObj[0, i, j] == null)
			{
				data[GhostAction.cNumber] = j;
				break;
			}
		}
	}
}

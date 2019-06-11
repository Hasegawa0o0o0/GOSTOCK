/* -----------------------------------------------------------------------------------------
 * PaintngGhost.cs
 * 2018.12.27 作成
 *		2018.12.28 動きの修正
 *		2019.01.02 スクリプトの大幅修正
 *		2019.01.03 デバック用の追加
 *		2019.01.10 フェードイン、アウトの作成(出来なさそうなのでアニメーションで強行？)
 *		2019.01.12 フェードイン、アウト出来た！！！！！！！！
 *		2019.01.20 エネルギー依存による出現率の変更
 *		2019.03.19 出現率の変更
 ----------------------------------------------------------------------------------------- */
using UnityEngine;

public class PaintingGhost : MonoBehaviour
{
	// オブジェクト系
	public GameObject painting;							// 絵画のゲームオブジェクト(自分？)
	public SpriteRenderer paintingSR;					// ↑のスプライト
	//public GameObject paintingFront;					// メインの全面においてあるオブジェクト
	public SpriteRenderer frontSR;						// ↑のスプライト(ポルターガイスト的な動きに使う)
	public Sprite[] paintingSprite = new Sprite[2];		// 絵画のスプライト([0]オバケなし [1]オバケなし)
	public GameObject pGhostPrefab;						// 絵画用のオバケのプレハブ

	// 変数系
	private bool leaveFlag = false;						// 家出(絵画出？)のフラグ
	public int leaveRand;								// 家出の確立
	private bool fadeFlag = true;						// フェードイン、フェードアウト
	public float[] adjustmentVec = new float[2];		// オバケの出現位置の調整([0]X [1]Y *Zは弄らないのでなし)

	// スクリプト系
	public EnergyRe energyRe;

	// デバック用
	public bool debugF = false;							// trueでデバック中

	void Start ()
	{
		//leaveRand = Random.Range(1, 5);
	}

	void Update ()
	{
		//// デバック用
		//if (Input.GetKeyDown(KeyCode.PageUp))
		//{
		//	leaveRand = 1;
		//}
		//if (Input.GetKeyDown(KeyCode.PageDown))
		//{
		//	leaveRand = 0;
		//}

		// フェードイン←→フェードアウト
		if (leaveRand != 1)
		{
			if (fadeFlag == false)
			{
				frontSR.color -= new Color(0, 0, 0, 0.01f);
				if (frontSR.color.a <= 0)
				{
					fadeFlag = true;
				}
			}
			else
			{
				frontSR.color += new Color(0, 0, 0, 0.01f);
				if (frontSR.color.a >= 1)
				{
					fadeFlag = false;
				}
			}
		}
		// 出現するときのモーション
		else
		{
			// 前に来ている画像(frontSR)の不透明度を0にする
			frontSR.color = new Color(frontSR.color.r, frontSR.color.g, frontSR.color.b, 0);
		}
	}

	// Destroyの範囲内に入った時だけオバケを家出(撮影の有効範囲内)
	private void OnTriggerEnter(Collider col)
	{
		if (col.tag == "DestroyArea")
		{
			if (leaveFlag == false)
			{
				if (debugF == false)
				{
					// デバックじゃないとき(ランダム化)
					if (leaveRand == 1)
					{
						// 座標取得＋位置の修正
						Vector3 vec3 = new Vector3(transform.position.x - adjustmentVec[0], transform.position.y - adjustmentVec[1], transform.position.z);
						Instantiate(pGhostPrefab, vec3, pGhostPrefab.transform.rotation);
						// オバケを家出(絵画出？)させる
						paintingSR.sprite = paintingSprite[0];
						leaveFlag = true;
						frontSR.color = new Color(frontSR.color.r, frontSR.color.g, frontSR.color.b, 1);
					}
				}
				else
				{
					// 座標取得＋位置の修正
					Vector3 vec3 = new Vector3(transform.position.x - adjustmentVec[0], transform.position.y - adjustmentVec[1], transform.position.z);
					Instantiate(pGhostPrefab, vec3, pGhostPrefab.transform.rotation);
					// オバケを家出(絵画出？)させる
					paintingSR.sprite = paintingSprite[0];
					leaveFlag = true;
					frontSR.color = new Color(frontSR.color.r, frontSR.color.g, frontSR.color.b, 1);
				}
				// ランダム値の再定
				RandDec();
			}
		}
	}

	// ランダム値の設定(エネルギー依存)
	private void RandDec()
	{
		// エネルギーが満タンの時(出現無し)
		if (energyRe.energyFull == true)
		{
			leaveRand = 0;
		}
		// コアが3個あるとき(1/100)
		else if(energyRe.coreFull == true)
		{
			leaveRand = Random.Range(1, 100);
		}
		// コアが1個も無いとき(1/2)
		else if (energyRe.gageMax == 0)
		{
			leaveRand = Random.Range(1, 2);
		}
		// コアが1個あるとき(1/20)
		else if (energyRe.gageMax == 1)
		{
			leaveRand = Random.Range(1, 20);
		}
		// コアが2個あるとき(1/35)
		else if (energyRe.gageMax == 2)
		{
			leaveRand = Random.Range(1, 35);
		}
	}

	// 家出中
	private void OnTriggerExit(Collider col)
	{
		if (col.tag == "DestroyArea")
		{
			if (leaveFlag == true)
			{
				paintingSR.sprite = paintingSprite[1];
				leaveFlag = false;
			}
		}
	}
}

/*---------------------------------------------------------------------------------------------------
// スコア管理
//
 * FileName		: Score.cpp
 * Author		: 17CU0116 杉田優帆
 * Log			: 2018.10.18 作成(既存のものから大幅に変更)
 *				: 2018.11.14 リザルト画面作成にあたり、変数を受け渡す。
 *				: 2018.12.11 クリアタイムの計測に伴いタイマーの設置
 *				: 2019.01.31 時間のボーナスに、撃破有り無しでポイントが大幅に変更するように
---------------------------------------------------------------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour 
{
	// 各スコア----------------------------------------------------------------
	public int noDamageBonus;					// ノーダメージボーナス(変更可)
	public int defeatBonus = 0;					// 撃破ボーナス(変更可)
	public int bonusMax;						// クリアタイムや命中率の得点の最大値(変更可)
	public static int[] points = new int[5];	// [0]撮影 [1]ノーダメージボーナス [2]命中率 [3]クリアタイム [4]撃破ボーナス
	public bool defeatFlag = false;
	public bool noDamageFlag = true;
	public bool finishFlag = false;

	// 加点数値(撮影での加点)
	public int adds = 0;

	// 各スクリプト取得
	public PlayerControl pc;

	// 仮系
	//public Text text;
	public float rate;

	// [Re]2018.12.11 時間計測
	private int minute;
	private float seconds;
	private float startTime;		// 最初の時間


	// ------------------------------------------------------------------------

	void Start ()
	{
		// 初期化
		for(int i = 0; i < 5; ++i)
		{
			points[i] = 0;
		}
		// 2018.12.11
		startTime = Time.realtimeSinceStartup + 3;
	}

	void Update ()
	{
		// 撃破ボーナスの判定
		if (defeatFlag == true)
		{
			points[4] = defeatBonus;
		}
		// ノーダメージの判定
		if (noDamageFlag == true)
		{
			points[1] = noDamageBonus;
		}
		else
		{
			points[1] = 0;
		}
		// 値が入っている時だけ処理する
		if (defeatFlag == false && finishFlag == false && pc.shot != 0 && pc.shotNoHit != 0)
		{
			HitRate(false);
		}
		else
		{
			HitRate(true);
		}

		// 2018.12.11 Time.timeでの時間計測
		if (defeatFlag == false && finishFlag == false)
		{
			seconds = Time.time - startTime;
			minute = (int)seconds / 60;
		}
		else
		{
			ClearTime();
		}
	}

	// 2018.11.27 命中率計算及び得点の決定
	private void HitRate(bool rateFlag)
	{
		if (rateFlag == false)
		{
			rate = (pc.shot - pc.shotNoHit) / pc.shot * 100.0f;
			if (rate >= 100)
			{
				rate = 100.0f;
			}
			//text.text = rate.ToString() + " %";
		}
		else
		{
			if (rate >= 90.0f)
			{
				points[2] = bonusMax;
			}
			else if(rate >=60.0f)
			{
				points[2] = bonusMax - 2000;
			}
			else if(rate >= 30.0f)
			{
				points[2] = 1000;
			}
			else
			{
				points[2] = 500;
			}
		}
	}

	// クリアタイム評価→時間に応じた得点を＋撃破の有り無しによって変動するように変更
	private void ClearTime()
	{
		if (defeatFlag == true)
		{
			if (minute <= 1)
			{
				points[3] = bonusMax;
			}
			else if (minute <= 3)
			{
				points[3] = bonusMax - 2000;
			}
			else if (minute <= 5)
			{
				points[3] = 1000;
			}
			else
			{
				points[3] = 500;
			}
		}
		else
		{
			points[3] = 500;
		}
	}

	// 撮影での加点、他スクリプトから呼び出し可
	public void PhotoAdd()
	{
		points[0] = points[0] + adds;
	}

	// 別シーン間での値の受け渡し(得点の入った配列)
	public static int[] GetPoints()
	{
		return points;
	}
}

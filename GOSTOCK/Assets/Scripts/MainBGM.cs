/* -----------------------------------------------------------------------------------------
 * TitleBGM.cs
 * 2019.03.11 作成
 *		2019.03.11 TitleBGMを流用、mainProsuctionからmainへ
 *		2019.03.19 AudioSourceの変数名変更
 *		2019.04.02 リプレイの機構を変更(必ずフェードインするように)
 ----------------------------------------------------------------------------------------- */
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainBGM: MonoBehaviour
{
	public AudioSource audioSource;				// BGMで流したいクリップが入ったオーディオソース

	public static bool only = true;				// 一度きりの処理に使用
	public bool rePlay = false;					// リプレイ

	private bool fadeFlag = false;

	void Start()
	{
		if (only == true)
		{
			// Sceneを遷移してもオブジェクトが消えないようにする
			DontDestroyOnLoad(this);
			audioSource.Play();
			only = false;
			rePlay = true;
		}
	}

	void Update()
	{
		if (SceneManager.GetActiveScene().name == "clear")
		{
			audioSource.Stop();
		}

		if(rePlay == true)
		{
			//audioSource.volume = 1;
			//audioSource.Play();
			//rePlay = false;

			if(fadeFlag == true)
			{
				audioSource.volume += 0.01f;
				if(audioSource.volume >= 1f)
				{
					audioSource.volume = 1f;
					rePlay = false;
				}
			}
			else
			{
				audioSource.volume = 0;
				fadeFlag = true;
			}

			if(!audioSource.isPlaying)
			{
				audioSource.Play();
			}
		}
		else
		{
			fadeFlag = false;
		}
	}
}
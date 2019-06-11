/* -----------------------------------------------------------------------------------------
 * TitleBGM.cs
 * 2019.02.25 作成
 *		2019.02.25 BGMをシーンが跨いでも途切れない様にする
 ----------------------------------------------------------------------------------------- */
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleBGM: MonoBehaviour
{
	public bool DontDestroyEnabled = true;

	public AudioSource source;					// BGMで流したいクリップが入ったオーディオソース

	public static bool one = true;				// 一度きりの処理に使用
	public bool rePlay = false;					// リプレイ

	public TitleAnimation_Re re;

	void Start()
	{
		if (one == true)
		{
			if (DontDestroyEnabled == true)
			{
				// Sceneを遷移してもオブジェクトが消えないようにする
				DontDestroyOnLoad(this);
				source.Play();
			}
			one = false;
		}
		re = FindObjectOfType<TitleAnimation_Re>();
	}

	void Update()
	{
		if (re == null && SceneManager.GetActiveScene().name == "re_title")
		{
			re = FindObjectOfType<TitleAnimation_Re>();
		}

		if (re != null && re.fadeBGM == true)
		{
			source.volume -= 0.40f / 60f;
			if (source.volume == 0)
			{
				source.Stop();
			}
		}

		if (rePlay == true)
		{
			source.volume = 1;
			source.Play();
			rePlay = false;
		}
	}
}
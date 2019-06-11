using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
	int frame;
	// unity側設定
	public GameObject game;
	SpriteRenderer gameSp;
	public GameObject over;
	SpriteRenderer overSp;
	public SpriteRenderer soul;					// 魂の周り
	public GameObject centerSoul;               // 魂の中枢
	bool isBack = false;
	int backFrame = 0;
	const int backFrameMax = 60;
	int forcedFrame = 0;
	public SpriteRenderer titleSp;

	SpriteRenderer centerSoulSpriteRenderer;
	public AudioSource audioSource;
	void Start ()
	{
		this.audioSource.Play ();
		gameSp = game.GetComponent<SpriteRenderer>();
		overSp = over.GetComponent<SpriteRenderer>();
		// 魂を非表示
		soul.color = new Color(1, 1, 1, 0);
		centerSoulSpriteRenderer = centerSoul.GetComponent<SpriteRenderer>();
		centerSoulSpriteRenderer.color = new Color(1, 1, 1, 0);
		titleSp.color = new Color(1, 1, 1, 0);
	}
	
	void Update ()
	{
		// シーン移行
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button5))
		{
			isBack = true;
		}
		if (isBack)
		{
			TitleAnimation.afterEnding = true;
			audioSource.volume -= 0.0075f;
			backFrame++;
			if (backFrame >= backFrameMax + 240)
			{
				SceneManager.LoadScene("title");
			}
			// タイトルの表示
			if (backFrame >= backFrameMax)
			{
				titleSp.color += new Color(0, 0, 0, 0.005f);
			}
			// スプライトを消す
			gameSp.color -= new Color(0, 0, 0, 0.02f);
			overSp.color -= new Color(0, 0, 0, 0.02f);
			soul.color -= new Color(0, 0, 0, 0.02f);
			centerSoulSpriteRenderer.color -= new Color(0, 0, 0, 0.02f);
		}
		frame++;
		if (frame > 170)
		{
			++forcedFrame;
			if (forcedFrame > 600)
			{
				isBack = true;
			}
			if (!isBack)
			{
				soul.color += new Color(0, 0, 0, 0.006f);
				centerSoulSpriteRenderer.color += new Color(0, 0, 0, 0.006f);
				if (soul.color.a > 1)
				{
					soul.color = new Color(1, 1, 1, 1);
					centerSoulSpriteRenderer.color = new Color(1, 1, 1, 1);
				}
			}
			// フレーム数によって位置を変える
			// ↑
			if (frame % 120 > 90)
			{
				centerSoul.transform.position = new Vector3(2.94f, -0.92f);
			}
			// 右
			else if (frame % 120 > 60)
			{
				centerSoul.transform.position = new Vector3(2.97f, -0.95f);
			}
			// ↓
			else if (frame % 120 > 30)
			{
				centerSoul.transform.position = new Vector3(2.94f, -0.98f);
			}
			// 左
			else
			{
				centerSoul.transform.position = new Vector3(2.91f, -0.95f);
			}
		}
		else if (frame > 60)
		{
			over.transform.position = Vector2.MoveTowards(over.transform.position, new Vector2(0, 0), 0.175f);
			game.transform.position = Vector2.MoveTowards(game.transform.position, new Vector2(0, 0), 0.175f);
		}
		// game,overを移動
		else
		{
			game.transform.position = Vector2.MoveTowards(game.transform.position, new Vector2(0, 0), 0.2f);
		}
	}
}

/*
 * 2018.07.17 作成
 *		2019.01.13 調整
 */
using UnityEngine;
using UnityEngine.UI;

public class Select : MonoBehaviour
{
	public GameObject cursorPrefab;
	SpriteRenderer spriteRenderer;
	private float inputX;
	int moveDeley = 0;
	const int moveDeleyMax = 30;
	// Playコマンドの取得
	private TextCmd cmd;
	// アイコン系(unity側設定)
	public Image[] icons = new Image[5];
	int iconNum = 0;
	// ディレイ
	int sceneCnt = 0;

	void Start ()
	{
		// Playの文字のスクリプト取得
		cmd = FindObjectOfType<TextCmd>();
		spriteRenderer = cursorPrefab.GetComponent<SpriteRenderer>();
		// カーソル赤にして非表示
		spriteRenderer.color = new Color(1, 0, 0, 0);
		// カーソルの位置合わせ 2019.01.13
		cursorPrefab.transform.position = new Vector3(-3.5f, 5f, cursorPrefab.transform.position.z);
		// アイコンの数をカウント
		for (int i = 0; i < 5; ++i)
		{
			// 入っていなければ終了
			if (!icons[i]) { break; }
			++iconNum;
		}
	}

	void Update ()
	{
		// 選択画面が表示されて2秒以降
		if (sceneCnt > 120)
		{
			// 決定されたらカーソルを消す
			if (cmd.TT.decision)
			{
				spriteRenderer.color -= new Color(0, 0, 0, 0.01f);
			}
			else
			{
				// カーソルの表示
				spriteRenderer.color += new Color(0, 0, 0, 0.015f);
				if (spriteRenderer.color.a >= 1) { spriteRenderer.color = new Color(1, 0, 0, 1); }
				// 左右の入力を取得（X軸）
				inputX = Input.GetAxisRaw("Horizontal");
			}
			// カーソルの位置合わせ
			if (cursorPrefab.transform.position.y > -1f)
			{
				cursorPrefab.transform.position = new Vector3(cursorPrefab.transform.position.x, -1f, cursorPrefab.transform.position.z);
			}
			// 入力がされていたら位置の変更
			// 右移動
			if (inputX > 1.0e-10)
			{
				// 設定した時間が過ぎたら移動
				if (moveDeley > moveDeleyMax)
				{
					if (cursorPrefab.transform.position.x > 0f)
					{
						cursorPrefab.transform.position = new Vector3(-3.5f, -1f, cursorPrefab.transform.position.z);
					}
					else
					{
						cursorPrefab.transform.position = new Vector3(3.5f, -1f, cursorPrefab.transform.position.z);
					}
					moveDeley = 0;
				}
				else
				{
					++moveDeley;
				}
			}
			// 左移動
			else if (inputX < -1.0e-10)
			{
				if (moveDeley > moveDeleyMax)
				{
					if (cursorPrefab.transform.position.x > 0f)
					{
						cursorPrefab.transform.position = new Vector3(-3.5f, -1f, cursorPrefab.transform.position.z);
					}
					else
					{
						cursorPrefab.transform.position = new Vector3(3.5f, -1f, cursorPrefab.transform.position.z);
					}
					moveDeley = 0;
				}
				else
				{
					++moveDeley;
				}
			}
			// 入力がなかったらすぐ移動できるように調整
			else
			{
				moveDeley = moveDeleyMax + 1;
			}
		}
		else
		{
			++sceneCnt;
		}
	}
}

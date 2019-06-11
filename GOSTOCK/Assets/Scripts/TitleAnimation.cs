// タイトルのアニメーション全般を管理

using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleAnimation : MonoBehaviour
{
	// 全体で使う
	int frame = 0;
	public static bool afterEnding = false;
	AudioSource audioSource;

	// ランプを左右に揺らすのに使用
	public GameObject lamp;
	public float side;
	float addSide = 0.25f;

	// シュッと出すのに使用
	public SpriteRenderer title;

	// 目をぱちぱちさせるところで使用
	public SpriteRenderer[] eyes = new SpriteRenderer[2];
	public Sprite[] eyeSpriteList = new Sprite[2];
	int pcpcCnt = 0;                                        // ぱちぱちした回数
	public GameObject titleLightObj;
	TitleLight titleLight;
	public SpriteRenderer pressSp;
	Color addColor = new Color(0, 0, 0, 0.01f);

	// おばけがやってくるのに使用
	public GameObject ghost;
	Anim ghostAnim;
	public Sprite[] ghostSpriteList = new Sprite[4];
	public float shake;         // 縦の揺れ幅
	float addShake = 0.0005f;	//
	public Anim candleAnim;     // 手に持っている燭台
	public Sprite[] candleSpriteList = new Sprite[4];
	public Sprite candleOff;	// ついてないキャンドル

	// 火がつくのに使用
	public Anim[] fire = new Anim[2];
	public Sprite[] fireSpriteList = new Sprite[5];
	bool ignition = false;              // 点火
	int ignitionFrame;

	// 入力された後に使う
	bool isInput = false;
	int changeFrame = 0;
	SpriteRenderer lampSp;
	public SpriteRenderer lampPoleSp;
	Vector3 ghostMove = new Vector3(-0.01f, 0.005f, 0);

	void Awake()
	{
		Application.targetFrameRate = 60;
		Screen.SetResolution(1920, 1080, true);
	}

	void Start ()
	{
		titleLightObj.SetActive(false);
		// 非表示
		title.color = new Color(1, 1, 1, 0);
		pressSp.enabled = false;
		for (int i = 0; i < 2; ++i)
		{
			eyes[i].enabled = false;
		}
		// 取得
		ghostAnim = ghost.GetComponent<Anim>();
		lampSp = lamp.GetComponent<SpriteRenderer>();
		titleLight = titleLightObj.GetComponent<TitleLight>();
		audioSource = GetComponent<AudioSource>();
		// クリアに行った後なら初期化
		if (afterEnding)
		{
			frame = 340;
			title.color = Color.white;
			pcpcCnt = 4;
			titleLightObj.SetActive(true);
			pressSp.enabled = true;
			ghost.transform.localPosition = new Vector2(2.71f, ghost.transform.localPosition.y);
			ignitionFrame = frame;
			ignition = true;
			audioSource.Play();
		}
		else
		{
			BGMMaster.instance.SoundEffect(21);
		}
	}
	
	void Update ()
	{
		// 入力されたら
		if (isInput)
		{
			if (changeFrame > 120)
			{
				//SceneManager.LoadScene("select");
				// 2018.11.14
				SceneManager.LoadScene("Main");
			}
			// ライトを近づけて穴に通す表現
			titleLight.MoveSelect();
			// すーっと消す
			// タイトル
			soo(title);
			// 目と炎
			for (int i = 0; i < 2; ++i)
			{
				soo(eyes[i]);
				soo(fire[i].spriteRenderer);
			}
			// ランタン
			soo(lampSp);
			// ランタンのポール
			soo(lampPoleSp);
			soo(pressSp);
			//// おばけをピャーっと移動させる
			//ghost.transform.localPosition += ghostMove;
			//ghostMove.x *= 1.05f;
			//ghostMove.y *= 1.06f;
			// 落とす
			ghost.transform.localPosition -= ghostMove;
			ghostMove.x = 0;
			ghostMove.y *= 1.05f;
			audioSource.volume -= 0.01f;
			++changeFrame;
		}
		// 火の非表示
		if (frame == 0)
		{
			for (int i = 0; i < 2; ++i)
			{
				fire[i].OffSprite();
			}
		}
		// ランプを揺らす
		side -= addSide;
		if (side < -30 || side > 9) { addSide *= -1; }
		// ↓かくかくする
		//lamp.transform.rotation = Quaternion.Euler(0, 0, side);
		lamp.transform.eulerAngles = new Vector3(0, 0, side);
		// おばけを揺らす
		shake -= addShake;
		if (Mathf.Abs(shake) > 0.02f) { addShake *= -1; }
		// ライトを動かす
		titleLight.MoveTitleScene();
		ghost.transform.localPosition += new Vector3(0, shake, 0);
		// 点火
		if (ignition)
		{
			if (frame > ignitionFrame)
			{
				for (int i = 0; i < 2; ++i)
				{
					if (fire[i].spriteRenderer.sprite == fireSpriteList[3])
					{
						fire[i].Animation(fireSpriteList, 5);
					}
					else if (fire[i].spriteRenderer.sprite == fireSpriteList[4])
					{
						fire[i].drawingSpriteFrameMax = 20;
						fire[i].Animation(fireSpriteList, 5, false);
					}
					else
					{
						fire[i].drawingSpriteFrameMax = 5;
						fire[i].Animation(fireSpriteList, 5);
					}
				}
				// おばけが持っているろうそくの火を消す
				candleAnim.spriteRenderer.sprite = candleOff;
			}
		}
		// 目をぱちぱち
		if (frame > 220)
		{
			// 目の表示
			//if (!eyes[0].enabled)
			//{
			//	for (int i = 0; i < 2; ++i)
			//	{
			//		eyes[i].enabled = true;
			//	}
			//}
			// ぱちぱち
			if (frame % 10 == 0 && (frame - 221) % 250 < 40)
			{
				for (int i = 0; i < 2; ++i)
				{
					if (eyes[i].sprite == eyeSpriteList[0])
					{
						eyes[i].enabled = true;
						eyes[i].sprite = eyeSpriteList[1];
					}
					else if (eyes[i].sprite == eyeSpriteList[1])
					{
						eyes[i].enabled = false;
						eyes[i].sprite = eyeSpriteList[0];
					}
				}
				pcpcCnt++;
			}
			// pressを点滅
			if (pcpcCnt >= 4)
			{
				if ((frame - 221) % 250 == 80)
				{
					// ここでスポットライトの表示
					titleLightObj.SetActive(true);
					pressSp.enabled = true;
				}
				if (!isInput)
				{
					if (pressSp.color.a < 0 || pressSp.color.a > 1)
					{
						addColor *= -1;
					}
					pressSp.color += addColor;
				}
			}
			if (frame == 340)
			{
				audioSource.Play();
			}
			// おばけの移動
			if (frame > 300)
			{
				// アニメーション
				ghostAnim.Animation(ghostSpriteList, 4);
				if (!ignition)
				{
					candleAnim.Animation(candleSpriteList, 4);
				}
				if (isInput == false)
				{
					// 移動
					if (ghost.transform.localPosition.x > 2.71f)
					{
						ghost.transform.localPosition += new Vector3(-0.06f, 0, 0);
						// 位置補正
						if (ghost.transform.localPosition.x < 2.71f)
						{
							ghost.transform.localPosition = new Vector2(2.71f, ghost.transform.localPosition.y);
							ignitionFrame = frame + 40;
							ignition = true;
						}
					}
				}
				if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button5) || Input.anyKeyDown)
				{
					isInput = true;
				}
			}
		}
		// タイトルたちを表示
		else if (frame > 120)
		{
			title.color += new Color(0, 0, 0, 0.01f);
			if (title.color.a > 1)
			{
				title.color = Color.white;
			}
		}
		++frame;
	}

	// すーっと消すための関数
	void soo(SpriteRenderer sp)
	{
		sp.color -= new Color(0, 0, 0, 0.01f);
	}
}

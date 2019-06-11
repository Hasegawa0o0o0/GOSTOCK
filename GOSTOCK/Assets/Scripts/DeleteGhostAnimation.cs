using UnityEngine;

public class DeleteGhostAnimation : MonoBehaviour
{
	int frame = 0;
	Anim anim;
	public Sprite[] spriteList = new Sprite[4];
	public bool shunFlag = false;					// 通常に消えるのと縦にシュンって消えるのの違いのため

	// エネルギーの情報を取得
	private EnergyRe energyRe;
	private PlayerControl playerControl;

	// ゲージへ戻るのに使用
	public bool returnFlag = false;					// ゲージへ移動するように 2018.01.10
	public GameObject EGage;						// エネルギーゲージ
	public SpriteRenderer spriteRenderer;

	void Start ()
	{
		anim = GetComponent<Anim>();
		energyRe = FindObjectOfType<EnergyRe>();
		playerControl = FindObjectOfType<PlayerControl>();
	}
	
	void Update ()
	{
		// ゲージの量を確認する エラーのための条件追加 2019.01.18
		if ((energyRe && playerControl) && energyRe.gageMax == 3 && playerControl.power >= 15)
		{
			returnFlag = false;
		}

		++frame;
		anim.Animation(spriteList, 4);
		// 2018.12.03
		if (shunFlag == true)
		{
			// 上にシュンって消えるように
			transform.localScale -= new Vector3(0.2f, 0, 0);
			transform.position += new Vector3(0, 0, -0.2f);
		}
		if (frame > 8)
		{
			if (returnFlag == false)
			{
				Destroy(gameObject);
			}
			else		// ゲージへ移動
			{
				//Vector3 finiPos = new Vector3(-37.13f, -1.11f, -10.24f);		// UI縦置きVer
				Vector3 finiPos = new Vector3(-29.8f, 7.28f, -3.55f);			// UI横置きVer
				Vector3 nextPos = Vector3.Lerp(transform.position, finiPos, Time.deltaTime * 5);
				transform.position = nextPos;
				if (frame > 24)
				{
					spriteRenderer.color -= new Color(0.05f, 0.05f, 0.05f, 0.02f);
					// 真っ黒になったら消滅
					if (spriteRenderer.color.r <= 0)
					{
						Destroy(gameObject);
					}
				}
			}
		}
	}
}

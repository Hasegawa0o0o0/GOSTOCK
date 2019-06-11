/* -----------------------------------------------------------------------------------------
 * AutoDestoryPartcle.cs
 * 2019.02.13 作成
 *		参考サイト:http://dadapo.hatenablog.com/entry/2018/03/01/123238
 ----------------------------------------------------------------------------------------- */
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoDestoryPartcle : MonoBehaviour
{
	void Start()
	{
		ParticleSystem partcleSystem = GetComponent<ParticleSystem>();
		//Delete object after duration.
		Destroy(gameObject, (float)partcleSystem.main.duration);
	}
}

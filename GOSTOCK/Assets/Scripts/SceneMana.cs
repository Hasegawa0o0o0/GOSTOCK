using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMana : MonoBehaviour
{
	public string nextSceneName;
	
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SceneManager.LoadScene(nextSceneName);
		}
	}
}

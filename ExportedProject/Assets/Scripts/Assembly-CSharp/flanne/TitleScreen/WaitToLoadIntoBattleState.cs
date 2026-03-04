using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace flanne.TitleScreen
{
	public class WaitToLoadIntoBattleState : TitleScreenState
	{
		public override void Enter()
		{
			StartCoroutine(WaitToLoadCR());
			AudioManager.Instance.FadeOutMusic(1f);
		}

		private IEnumerator WaitToLoadCR()
		{
			yield return new WaitForSeconds(1f);
			SceneManager.LoadSceneAsync("Battle", LoadSceneMode.Single);
		}
	}
}

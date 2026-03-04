using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using flanne.UI;

namespace flanne.Core
{
	public class GunEvoMenuState : GameState
	{
		private List<Powerup> powerupChoices;

		private void OnEvoClicked(object sender, InfoEventArgs<int> e)
		{
			GunEvolution data = base.gunEvoMenu.GetEntry(e.info).GetComponent<GunEvoUI>().data;
			StartCoroutine(EndGunEvoAnimationCR(data));
		}

		public override void Enter()
		{
			base.pauseController.Pause();
			AudioManager.Instance.SetLowPassFilter(isOn: true);
			StartCoroutine(PlayGunEvoAnimationCR());
		}

		public override void Exit()
		{
			base.gunEvoMenu.ClickEvent -= OnEvoClicked;
			AudioManager.Instance.SetLowPassFilter(isOn: false);
			base.pauseController.UnPause();
		}

		private void GenerateEvolutions()
		{
			int num = 3;
			List<GunEvolution> list = new List<GunEvolution>();
			List<GunEvolution> list2 = ((!(Loadout.GunSelection != null)) ? PlayerController.Instance.gun.gunData.gunEvolutions : Loadout.GunSelection.gunEvolutions);
			for (int i = 0; i < num; i++)
			{
				GunEvolution gunEvolution = null;
				while (gunEvolution == null)
				{
					GunEvolution gunEvolution2 = list2[Random.Range(0, list2.Count)];
					if (!list.Contains(gunEvolution2))
					{
						gunEvolution = gunEvolution2;
					}
				}
				base.gunEvoMenu.GetEntry(i).GetComponent<GunEvoUI>().Set(gunEvolution);
				list.Add(gunEvolution);
			}
		}

		private IEnumerator PlayGunEvoAnimationCR()
		{
			base.screenFlash.Flash(1);
			PlayerController.Instance.gun.PlayGunEvoAnimation();
			base.gunEvoStartSFX.Play();
			yield return new WaitForSecondsRealtime(0.5f);
			base.screenFlash.Flash(1);
			GenerateEvolutions();
			base.gunEvoPanel.Show();
			base.gunEvoMenuSFX.Play();
			yield return new WaitForSecondsRealtime(0.2f);
			base.gunEvoMenu.ClickEvent += OnEvoClicked;
		}

		private IEnumerator EndGunEvoAnimationCR(GunEvolution evo)
		{
			base.gunEvoPanel.Hide();
			yield return new WaitForSecondsRealtime(0.2f);
			PlayerController.Instance.gun.EndGunEvoAnimation();
			base.gunEvoEndSFX.Play();
			yield return null;
			PlayerController.Instance.playerPerks.Equip(evo);
			yield return new WaitForSecondsRealtime(1f);
			owner.ChangeState<CombatState>();
		}
	}
}

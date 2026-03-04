using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class PlayerHealth : MonoBehaviour
	{
		[SerializeField]
		private PlayerController player;

		public UnityIntEvent onHealthChangedTo;

		public UnityIntEvent onMaxHPChangedTo;

		public UnityIntEvent onSoulHPChangedTo;

		public UnityEvent onDamagePrevented;

		public UnityEvent onDodged;

		public UnityEvent onHealed;

		public UnityEvent onGainedSHP;

		public UnityEvent onLostSHP;

		public UnityEvent onHurt;

		public UnityEvent onDeath;

		public List<string> vulnerableTags;

		public int maxSHP = 3;

		private int _baseMaxHP;

		private int _maxHP;

		private int _hp;

		private int _shp;

		public BoolToggle isInvincible;

		public bool isProtected;

		private DodgeRoller dodger;

		public int baseMaxHP
		{
			get
			{
				return _baseMaxHP;
			}
			set
			{
				_baseMaxHP = value;
				maxHP = _baseMaxHP;
			}
		}

		public int maxHP
		{
			get
			{
				return _maxHP;
			}
			set
			{
				int num = Mathf.Clamp(value, 0, 20);
				shp = Mathf.Clamp(shp, 0, 20 - num);
				int num2 = num - _maxHP;
				_maxHP = num;
				if (num2 > 0)
				{
					hp += num2;
				}
				else
				{
					hp = Mathf.Clamp(hp, 1, maxHP);
				}
				onMaxHPChangedTo.Invoke(_maxHP);
			}
		}

		public int hp
		{
			get
			{
				return _hp;
			}
			set
			{
				_hp = Mathf.Clamp(value, 0, maxHP);
				onHealthChangedTo.Invoke(_hp);
			}
		}

		public int shp
		{
			get
			{
				return _shp;
			}
			set
			{
				if (value < _shp && _shp != 0)
				{
					onLostSHP.Invoke();
				}
				else if (value > _shp && _shp < 20 - maxHP)
				{
					onGainedSHP.Invoke();
				}
				_shp = Mathf.Clamp(value, 0, 20 - maxHP);
				_shp = Mathf.Clamp(value, 0, maxSHP);
				onSoulHPChangedTo.Invoke(_shp);
			}
		}

		private void Start()
		{
			isInvincible = new BoolToggle(b: false);
			dodger = new DodgeRoller(player);
			vulnerableTags = new List<string> { "Enemy", "HarmfulToPlayer", "EProjectile" };
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			CheckCollision(other.gameObject);
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			CheckCollision(other.gameObject);
		}

		public void Heal(int amount)
		{
			if (amount < 0)
			{
				Debug.LogWarning("Player should not be healed a negative amount");
			}
			hp += amount;
			onHealed.Invoke();
		}

		public void AutoKill()
		{
			hp = 0;
			onDeath.Invoke();
		}

		public void RemoveVulnerability(string tag)
		{
			vulnerableTags.Remove(tag);
		}

		private void CheckCollision(GameObject other)
		{
			string text = other.tag;
			if (!CheckIfVulnerable(text) || isInvincible.value)
			{
				return;
			}
			if (isProtected)
			{
				isProtected = false;
				onDamagePrevented.Invoke();
				return;
			}
			if (dodger.Roll())
			{
				onDodged.Invoke();
				return;
			}
			AIComponent component = other.gameObject.GetComponent<AIComponent>();
			if (component == null)
			{
				TakeDamage(1);
			}
			else
			{
				TakeDamage(component.damageToPlayer);
			}
		}

		private void TakeDamage(int damage)
		{
			for (int i = 0; i < damage; i++)
			{
				if (shp == 0)
				{
					hp--;
				}
				else
				{
					shp--;
				}
			}
			StartCoroutine(InvicibilityCR(1f));
			onHurt.Invoke();
			if (hp == 0)
			{
				onDeath.Invoke();
			}
		}

		private bool CheckIfVulnerable(string tag)
		{
			bool result = false;
			foreach (string vulnerableTag in vulnerableTags)
			{
				if (tag.Contains(vulnerableTag))
				{
					result = true;
				}
			}
			return result;
		}

		private IEnumerator InvicibilityCR(float duration)
		{
			isInvincible.Flip();
			yield return new WaitForSeconds(duration);
			isInvincible.UnFlip();
		}
	}
}

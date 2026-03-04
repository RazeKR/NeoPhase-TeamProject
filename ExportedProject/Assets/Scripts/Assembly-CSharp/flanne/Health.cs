using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class Health : MonoBehaviour
	{
		public static string DeathEvent = "Health.DeathEvent";

		public static string TweakDamageEvent = "Health.TweakDamageEvent";

		public static string TookDamageEvent = "Health.TookDamageEvent";

		[SerializeField]
		private int _maxHP;

		public BoolToggle isInvincible;

		public UnityIntEvent onHealthChange;

		public UnityIntEvent onMaxHealthChange;

		public UnityIntEvent onHurt;

		public UnityIntEvent onHeal;

		public UnityEvent onDeath;

		public UnityEvent onDamagePrevented;

		public int maxHP
		{
			get
			{
				return _maxHP;
			}
			set
			{
				int num = value - _maxHP;
				_maxHP = value;
				onMaxHealthChange.Invoke(_maxHP);
				if (num > 0)
				{
					HP += num;
					onHealthChange.Invoke(HP);
					return;
				}
				HP = Mathf.Clamp(HP, 0, maxHP);
				onHealthChange.Invoke(HP);
				if (_maxHP <= 0)
				{
					onDeath.Invoke();
				}
			}
		}

		public bool isDead => HP <= 0;

		public int HP { get; private set; }

		private void Awake()
		{
			isInvincible = new BoolToggle(b: false);
		}

		private void OnEnable()
		{
			HP = maxHP;
			onMaxHealthChange.Invoke(maxHP);
			onHealthChange.Invoke(HP);
		}

		public void TakeDamage(DamageType damageType, int damage, float finalMultiplier = 1f)
		{
			if (HP <= 0)
			{
				return;
			}
			if (damage < 0)
			{
				Debug.LogWarning("Cannot take negative damage.");
				return;
			}
			if (isInvincible.value)
			{
				onDamagePrevented.Invoke();
				return;
			}
			int value = damage.NotifyModifiers($"Health.Tweak{damageType.ToString()}Damage", this);
			value = value.NotifyModifiers(TweakDamageEvent, this);
			value = Mathf.FloorToInt((float)value * finalMultiplier);
			int hP = HP;
			HP = Mathf.Clamp(HP - value, 0, maxHP);
			if (hP != 0 && HP == 0)
			{
				onDeath.Invoke();
				this.PostNotification($"Health.{damageType.ToString()}DamageKill", null);
				this.PostNotification(DeathEvent, null);
			}
			this.PostNotification(TookDamageEvent, value);
			onHealthChange.Invoke(HP);
			onHurt.Invoke(value);
		}

		public void HPChange(int change)
		{
			if (HP <= 0)
			{
				return;
			}
			if (isInvincible.value && change < 0)
			{
				onDamagePrevented.Invoke();
				return;
			}
			int num = change.NotifyModifiers(TweakDamageEvent, this);
			int hP = HP;
			HP = Mathf.Clamp(HP + num, 0, maxHP);
			onHealthChange.Invoke(HP);
			if (change < 0)
			{
				onHurt.Invoke(num);
			}
			else if (change > 0)
			{
				onHeal.Invoke(num);
			}
			if (hP != 0 && HP == 0)
			{
				onDeath.Invoke();
				this.PostNotification(DeathEvent, null);
			}
		}

		public void AutoKill(bool notify = true)
		{
			if (HP > 0)
			{
				HP = 0;
				onDeath.Invoke();
				if (notify)
				{
					this.PostNotification(DeathEvent, null);
				}
			}
		}
	}
}

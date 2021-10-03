using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class SeinBashAttack : CharacterState, ISeinReceiver
{
	static SeinBashAttack()
	{
		SeinBashAttack.OnBashAttackEvent = delegate(Vector2 A_0)
		{
		};
		SeinBashAttack.OnBashBegin = delegate
		{
		};
		SeinBashAttack.OnBashEnemy = delegate(EntityTargetting A_0)
		{
		};
	}

	public static event Action<Vector2> OnBashAttackEvent;

	public static event Action OnBashBegin;

	public static event Action<EntityTargetting> OnBashEnemy;

	public Component TargetAsComponent
	{
		get
		{
			return this.Target as Component;
		}
	}

	public CharacterAirNoDeceleration AirNoDeceleration
	{
		get
		{
			return this.Sein.PlatformBehaviour.AirNoDeceleration;
		}
	}

	public SeinDoubleJump DoubleJump
	{
		get
		{
			return this.Sein.Abilities.DoubleJump;
		}
	}

	public CharacterApplyFrictionToSpeed ApplyFrictionToSpeed
	{
		get
		{
			return this.Sein.PlatformBehaviour.ApplyFrictionToSpeed;
		}
	}

	public CharacterGravity Gravity
	{
		get
		{
			return this.Sein.PlatformBehaviour.Gravity;
		}
	}

	public CharacterLeftRightMovement CharacterLeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	public PlayerAbilities PlayerAbilities
	{
		get
		{
			return this.Sein.PlayerAbilities;
		}
	}

	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	public SeinController SeinController
	{
		get
		{
			return this.Sein.Controller;
		}
	}

	public TextureAnimationWithTransitions BashChargeAnimation
	{
		get
		{
			Vector2 vector = this.m_directionToTarget;
			float num = Mathf.Cos(0.3926991f);
			SeinBashAttack.DirectionalAnimationSet directionalAnimationSet = (!this.Sein.Controller.IsSwimming) ? this.BashChargeAnimationSet : this.SwimBashChargeAnimationSet;
			vector.x = Mathf.Abs(vector.x);
			if (Vector3.Dot(Vector3.up, vector) > num)
			{
				return directionalAnimationSet.Up;
			}
			Vector3 vector2 = new Vector3(1f, 1f);
			if (Vector3.Dot(vector2.normalized, vector) > num)
			{
				return directionalAnimationSet.UpDiagonal;
			}
			if (Vector3.Dot(Vector3.right, vector) > num)
			{
				return directionalAnimationSet.Horizontal;
			}
			Vector3 vector3 = new Vector3(1f, -1f);
			if (Vector3.Dot(vector3.normalized, vector) > num)
			{
				return directionalAnimationSet.DownDiagonal;
			}
			if (Vector3.Dot(Vector3.down, vector) > num)
			{
				return directionalAnimationSet.Down;
			}
			return directionalAnimationSet.Up;
		}
	}

	public TextureAnimationWithTransitions BashJumpAnimation
	{
		get
		{
			Vector2 vector = MoonMath.Angle.VectorFromAngle(this.m_bashAngle + 90f);
			float num = Mathf.Cos(0.3926991f);
			SeinBashAttack.DirectionalAnimationSet directionalAnimationSet = (!this.Sein.Controller.IsSwimming) ? this.BashJumpAnimationSet : this.SwimBashJumpAnimationSet;
			vector.x = Mathf.Abs(vector.x);
			if (Vector3.Dot(Vector3.up, vector) > num)
			{
				return directionalAnimationSet.Up;
			}
			Vector3 vector2 = new Vector3(1f, 1f);
			if (Vector3.Dot(vector2.normalized, vector) > num)
			{
				return directionalAnimationSet.UpDiagonal;
			}
			if (Vector3.Dot(Vector3.right, vector) > num)
			{
				return directionalAnimationSet.Horizontal;
			}
			Vector3 vector3 = new Vector3(1f, -1f);
			if (Vector3.Dot(vector3.normalized, vector) > num)
			{
				return directionalAnimationSet.DownDiagonal;
			}
			if (Vector3.Dot(Vector3.down, vector) > num)
			{
				return directionalAnimationSet.Down;
			}
			return directionalAnimationSet.Up;
		}
	}

	public bool SpriteMirrorLock
	{
		get
		{
			return this.m_spriteMirrorLock;
		}
		set
		{
			if (this.m_spriteMirrorLock != value)
			{
				this.m_spriteMirrorLock = value;
				int @lock;
				if (value)
				{
					CharacterSpriteMirror spriteMirror = this.Sein.PlatformBehaviour.Visuals.SpriteMirror;
					@lock = spriteMirror.Lock;
					spriteMirror.Lock = @lock + 1;
					return;
				}
				CharacterSpriteMirror spriteMirror2 = this.Sein.PlatformBehaviour.Visuals.SpriteMirror;
				@lock = spriteMirror2.Lock;
				spriteMirror2.Lock = @lock - 1;
			}
		}
	}

	public bool CanBash
	{
		get
		{
			return this.PlayerAbilities.Bash.HasAbility && !(this.TargetAsComponent == null) && this.TargetAsComponent.gameObject.activeInHierarchy && (!(this.Sein != null) || this.Sein.Active) && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities);
		}
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.m_seinTransform = this.Sein.transform;
		this.Sein.Abilities.Bash = this;
	}

	public void Start()
	{
		this.m_hasStarted = true;
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent += this.ModifyHorizontalPlatformMovementSettings;
		this.Gravity.ModifyGravityPlatformMovementSettingsEvent += this.ModifyGravityPlatformMovementSettings;
	}

	public new void OnDestroy()
	{
		base.OnDestroy();
		if (this.m_hasStarted)
		{
			Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
			this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent -= this.ModifyHorizontalPlatformMovementSettings;
			this.Gravity.ModifyGravityPlatformMovementSettingsEvent -= this.ModifyGravityPlatformMovementSettings;
		}
	}

	public void ModifyGravityPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (this.IsBashing)
		{
			settings.GravityStrength = 0f;
		}
	}

	public void ModifyHorizontalPlatformMovementSettings(HorizontalPlatformMovementSettings settings)
	{
		if (this.IsBashing)
		{
			settings.LockInput = true;
		}
	}

	public void OnRestoreCheckpoint()
	{
		if (this.IsBashing)
		{
			this.ExitBash();
		}
		this.ApplyFrictionToSpeed.SpeedFactor = 0f;
		this.m_spriteMirrorLock = false;
	}

	public void OnDisable()
	{
		if (this.IsBashing)
		{
			this.ExitBash();
		}
	}

	public void ExitBash()
	{
		if (GameController.Instance)
		{
			GameController.Instance.ResumeGameplay();
		}
		this.ApplyFrictionToSpeed.SpeedFactor = 0f;
		this.IsBashing = false;
	}

	public void MovePlayerToTargetAndCreateEffect()
	{
		Component component = this.Target as Component;
		Vector3 vector = (!InstantiateUtility.IsDestroyed(component)) ? component.transform.position : this.PlatformMovement.Position;
		GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.BashFromFx);
		gameObject.transform.position = vector;
		Vector3 localScale = gameObject.transform.localScale;
		localScale.x = (vector - this.PlatformMovement.Position).magnitude;
		gameObject.transform.localScale = localScale;
		gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, MoonMath.Angle.AngleFromVector(this.PlatformMovement.Position - vector));
		if (!this.PlatformMovement.IsOnGround)
		{
			this.PlatformMovement.Position2D = vector;
		}
	}

	public void BeginBash()
	{
		this.m_timeRemainingOfBashButtonPress = 0f;
		this.IsBashing = true;
		this.Target.OnEnterBash();
		Transform transform = this.TargetAsComponent.transform;
		Sound.Play((!this.Sein.PlayerAbilities.BashBuff.HasAbility) ? this.BashStartSound.GetSound(null) : this.UpgradedBashStartSound.GetSound(null), this.m_seinTransform.position, null);
		if (GameController.Instance)
		{
			GameController.Instance.SuspendGameplay();
		}
		if (UI.Cameras.Current != null)
		{
			SuspensionManager.GetSuspendables(this.m_bashSuspendables, UI.Cameras.Current.GameObject);
			SuspensionManager.Resume(this.m_bashSuspendables);
			this.m_bashSuspendables.Clear();
		}
		this.PlatformMovement.LocalSpeed = Vector2.zero;
		GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.BashAttackGamePrefab);
		this.m_bashAttackGame = gameObject.GetComponent<BashAttackGame>();
		this.m_bashAttackGame.SendDirection(transform.position - this.PlatformMovement.Position);
		this.m_bashAttackGame.BashGameComplete += this.BashGameComplete;
		this.m_bashAttackGame.transform.position = transform.position;
		Vector3 b = Vector3.ClampMagnitude(transform.position - this.PlatformMovement.Position, 2f);
		this.m_playerTargetPosition = transform.position - b;
		this.m_directionToTarget = b.normalized;
		SeinBashAttack.OnBashBegin();
		this.Sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.BashChargeAnimation, 10, new Func<bool>(this.ShouldBashChargeAnimationKeepPlaying), false);
	}

	public void BashGameComplete(float angle)
	{
		this.JumpOffTarget(angle);
		this.AttackTarget();
		this.ExitBash();
	}

	public void JumpOffTarget(float angle)
	{
		if (GameController.Instance)
		{
			GameController.Instance.ResumeGameplay();
		}
		Vector2 vector = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
		Vector2 vector2 = vector * (this.BashVelocity  + this.BashVelocity * .10f * RandomizerBonus.Velocity());
		this.PlatformMovement.WorldSpeed = vector2;
		this.AirNoDeceleration.NoDeceleration = true;
		this.Sein.ResetAirLimits();
		this.m_frictionTimeRemaining = this.FrictionDuration;
		this.ApplyFrictionToSpeed.SpeedToSlowDown = this.PlatformMovement.LocalSpeed;
		this.MovePlayerToTargetAndCreateEffect();
		Component component = this.Target as Component;
		Vector3 position = (!InstantiateUtility.IsDestroyed(component)) ? component.transform.position : this.Sein.Position;
		GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.BashOffFx);
		gameObject.transform.position = position;
		Vector3 localScale = gameObject.transform.localScale;
		localScale.x = vector2.magnitude * 0.1f;
		gameObject.transform.localScale = localScale;
		gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, MoonMath.Angle.AngleFromVector(vector));
		if (this.BashReleaseEffect)
		{
			((GameObject)InstantiateUtility.Instantiate(this.BashReleaseEffect)).transform.position = position;
		}
		SeinBashAttack.OnBashAttackEvent(vector2);
		this.m_timeRemainingTillNextBash = this.DelayTillNextBash;
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.BashJumpAnimation, 10, new Func<bool>(this.ShouldBashJumpAnimationKeepPlaying));
		characterAnimationState.OnStartPlaying = new Action(this.OnAnimationStart);
		characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
		this.Sein.PlatformBehaviour.Visuals.SpriteMirror.FaceLeft = (vector2.x > 0f);
		if (this.Sein.Abilities.Swimming)
		{
			this.Sein.Abilities.Swimming.OnBash(angle);
		}
	}

	public void OnAnimationStart()
	{
		this.SpriteMirrorLock = true;
	}

	public void AttackTarget()
	{
		Component component = this.Target as Component;
		if (!InstantiateUtility.IsDestroyed(component))
		{
			Vector2 force = -MoonMath.Angle.VectorFromAngle(this.m_bashAngle + 90f) * (4f + (float)RandomizerBonus.Velocity());
			new Damage(RandomizerBonusSkill.AbilityDamage((!this.Sein.PlayerAbilities.BashBuff.HasAbility) ? this.Damage : this.UpgradedDamage), force, Characters.Sein.Position, DamageType.Bash, base.gameObject).DealToComponents(component.gameObject);			EntityTargetting component2 = component.gameObject.GetComponent<EntityTargetting>();
			if (component2 && component2.Entity is Enemy)
			{
				SeinBashAttack.OnBashEnemy(component2);
			}
			if (this.Sein.PlayerAbilities.BashBuff.HasAbility)
			{
				this.BeginBashThroughEnemies();
			}
		}
	}

	public void BeginBashThroughEnemies()
	{
		this.m_bashThroughEnemiesRemainingTime = 0.5f;
		this.Sein.Mortality.DamageReciever.MakeInvincibleToEnemies(this.m_bashThroughEnemiesRemainingTime);
		this.m_enemiesBashedThrough.Clear();
	}

	public void UpdateBashThroughEnemies()
	{
		if (this.m_bashThroughEnemiesRemainingTime > 0f)
		{
			this.m_bashThroughEnemiesRemainingTime -= Time.deltaTime;
			for (int i = 0; i < Targets.Attackables.Count; i++)
			{
				IAttackable attackable = Targets.Attackables[i];
				if (attackable.CanBeSpiritFlamed() && !this.m_enemiesBashedThrough.Contains(attackable))
				{
					Vector3 vector = attackable.Position - this.Sein.PlatformBehaviour.PlatformMovement.Position;
					if (vector.magnitude < 3f && Vector2.Dot(vector.normalized, this.PlatformMovement.LocalSpeed.normalized) > 0f)
					{
						Damage damage = new Damage(this.UpgradedDamage, this.PlatformMovement.WorldSpeed.normalized, this.Sein.Position, DamageType.SpiritFlame, base.gameObject);
						GameObject gameObject = ((Component)attackable).gameObject;
						damage.DealToComponents(gameObject);
						this.m_enemiesBashedThrough.Add(attackable);
						break;
					}
				}
			}
			if (this.m_bashThroughEnemiesRemainingTime <= 0f)
			{
				this.m_bashThroughEnemiesRemainingTime = 0f;
				this.FinishBashThroughEnemies();
			}
		}
	}

	public void FinishBashThroughEnemies()
	{
		this.m_enemiesBashedThrough.Clear();
	}

	public void UpdateBashingState()
	{
		this.HandleBashAngle();
		this.Sein.Mortality.DamageReciever.MakeInvincibleToEnemies(0.2f);
		this.HandleMovingTowardsBashTarget();
		this.Sein.PlatformBehaviour.Visuals.SpriteMirror.FaceLeft = (this.m_directionToTarget.x < 0f);
	}

	public void BashFailed()
	{
		if (this.NoBashTargetEffect)
		{
			((GameObject)InstantiateUtility.Instantiate(this.NoBashTargetEffect, base.transform.position, Quaternion.identity)).transform.parent = this.m_seinTransform;
		}
	}

	public void UpdateNormalState()
	{
		Randomizer.BashWasQueued = Randomizer.QueueBash;
		if (Core.Input.Bash.OnPressed || Randomizer.QueueBash)
		{
			Randomizer.QueueBash = false;
			this.m_timeRemainingOfBashButtonPress = 0.5f;
			if (this.Sein.IsOnGround && this.Sein.Speed.x == 0f && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && !this.Sein.Abilities.Carry.IsCarrying)
			{
				this.Sein.Animation.Play(this.BackFlipAnimation, 10, null);
				this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedY = this.BackFlipSpeed;
				if ((!this.Sein.PlayerAbilities.BashBuff.HasAbility) ? this.StationaryBashSound : this.UpgradedStationaryBashSound)
				{
					Sound.Play((!this.Sein.PlayerAbilities.BashBuff.HasAbility) ? this.StationaryBashSound.GetSound(null) : this.UpgradedStationaryBashSound.GetSound(null), base.transform.position, null);
				}
			}
		}
		if (this.m_timeRemainingOfBashButtonPress > 0f)
		{
			this.m_timeRemainingOfBashButtonPress -= Time.deltaTime;
			if ((Core.Input.Bash.OnReleased || ((double)this.m_timeRemainingOfBashButtonPress <= 0.4 && (double)this.m_timeRemainingOfBashButtonPress >= 0.4 - (double)Time.deltaTime)) && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && !this.Sein.Abilities.Carry.IsCarrying)
			{
				this.BashFailed();
			}
			if (Core.Input.Bash.Released || this.m_timeRemainingOfBashButtonPress <= 0f)
			{
				this.m_timeRemainingOfBashButtonPress = 0f;
			}
		}
		if ((this.m_timeRemainingOfBashButtonPress > 0f || Randomizer.BashWasQueued) && this.CanBash)
		{
			this.BeginBash();
		}
		this.HandleFindingTarget();
		this.UpdateTargetHighlight(this.Target);
	}

	public override void UpdateCharacterState()
	{
		if (this.Sein.IsSuspended)
		{
			return;
		}
		if (!this.Sein.PlayerAbilities.Bash.HasAbility)
		{
			return;
		}
		if (!this.Sein.Active)
		{
			this.ExitBash();
			return;
		}
		if (this.m_timeRemainingTillNextBash > 0f)
		{
			this.m_timeRemainingTillNextBash -= Time.deltaTime;
		}
		this.UpdateBashThroughEnemies();
		if (this.m_frictionTimeRemaining > 0f)
		{
			this.m_frictionTimeRemaining -= Time.deltaTime;
			float time = this.FrictionDuration - this.m_frictionTimeRemaining;
			this.ApplyFrictionToSpeed.SpeedFactor = this.FrictionCurve.Evaluate(time);
		}
		if (this.m_frictionTimeRemaining + this.NoAirDecelerationDuration - this.FrictionDuration > 0f)
		{
			this.AirNoDeceleration.NoDeceleration = true;
		}
		if (this.IsBashing)
		{
			this.UpdateBashingState();
			return;
		}
		this.UpdateNormalState();
	}

	public void HandleMovingTowardsBashTarget()
	{
		Vector3 a = this.m_playerTargetPosition - this.PlatformMovement.Position;
		this.PlatformMovement.WorldSpeed = a / Time.deltaTime * 0.1f;
	}

	public void HandleBashAngle()
	{
		if (!InstantiateUtility.IsDestroyed(this.m_bashAttackGame))
		{
			this.m_bashAngle = this.m_bashAttackGame.Angle;
		}
	}

	public void HandleFindingTarget()
	{
		if (this.Sein.Controller.IsCarrying)
		{
			this.Target = null;
			return;
		}
		if (this.m_timeRemainingTillNextBash > 0f)
		{
			this.Target = null;
			return;
		}
		if (this.PlayerAbilities.Bash.HasAbility)
		{
			this.Target = this.FindClosestAttackHandler();
			return;
		}
		this.Target = null;
	}

	public void UpdateTargetHighlight(IBashAttackable target)
	{
		if (this.m_lastTarget == target)
		{
			return;
		}
		if (!InstantiateUtility.IsDestroyed(this.m_lastTarget as Component))
		{
			this.m_lastTarget.OnBashDehighlight();
		}
		this.m_lastTarget = target;
		if (!InstantiateUtility.IsDestroyed(this.m_lastTarget as Component))
		{
			this.m_lastTarget.OnBashHighlight();
		}
	}

	public IBashAttackable FindClosestAttackHandler()
	{
		IBashAttackable result = null;
		float num = float.MaxValue;
		int num2 = int.MinValue;
		Vector3 position = this.Sein.Position;
		for (int i = 0; i < Targets.Attackables.Count; i++)
		{
			IAttackable attackable = Targets.Attackables[i];
			if (attackable.CanBeBashed())
			{
				float magnitude = (attackable.Position - position).magnitude;
				if (magnitude <= this.Range)
				{
					IBashAttackable bashAttackable = attackable as IBashAttackable;
					if (bashAttackable != null)
					{
						int bashPriority = bashAttackable.BashPriority;
						if ((bashPriority > num2 || (magnitude <= num && bashPriority == num2)) && this.Sein.Controller.RayTest(((Component)bashAttackable).gameObject))
						{
							num = magnitude;
							num2 = bashPriority;
							result = bashAttackable;
						}
					}
				}
			}
		}
		return result;
	}

	public bool ShouldBashChargeAnimationKeepPlaying()
	{
		return this.IsBashing;
	}

	public bool ShouldBashJumpAnimationKeepPlaying()
	{
		return !this.PlatformMovement.IsOnGround;
	}

	public void OnAnimationEnd()
	{
		this.SpriteMirrorLock = false;
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_timeRemainingOfBashButtonPress);
		ar.Serialize(ref this.m_frictionTimeRemaining);
		ar.Serialize(ref this.m_timeRemainingTillNextBash);
		ar.Serialize(ref this.m_spriteMirrorLock);
		base.Serialize(ar);
		if (ar.Reading && !InstantiateUtility.IsDestroyed(this.m_bashAttackGame))
		{
			InstantiateUtility.Destroy(this.m_bashAttackGame.gameObject);
		}
	}

	public SeinBashAttack.DirectionalAnimationSet BashChargeAnimationSet;

	public SeinBashAttack.DirectionalAnimationSet BashJumpAnimationSet;

	public SeinBashAttack.DirectionalAnimationSet SwimBashChargeAnimationSet;

	public SeinBashAttack.DirectionalAnimationSet SwimBashJumpAnimationSet;

	public TextureAnimationWithTransitions BackFlipAnimation;

	public GameObject BashAttackGamePrefab;

	public SoundProvider BashEndSound;

	public SoundProvider BashLoopSound;

	public SoundProvider BashStartSound;

	public SoundProvider StationaryBashSound;

	public SoundProvider UpgradedBashEndSound;

	public SoundProvider UpgradedBashLoopSound;

	public SoundProvider UpgradedBashStartSound;

	public SoundProvider UpgradedStationaryBashSound;

	public GameObject BashFromFx;

	public GameObject BashOffFx;

	public GameObject BashReleaseEffect;

	public float BashVelocity = 56.568f;

	public float Damage = 2f;

	public float UpgradedDamage = 5f;

	public float DelayTillNextBash  = 0.2f;

	public AnimationCurve FrictionCurve;

	public float FrictionDuration;

	public float NoAirDecelerationDuration = 0.2f;

	public float Range = 4f;

	public SeinCharacter Sein;

	public IBashAttackable Target;

	public Vector3 m_directionToTarget;

	public float m_bashAngle;

	public Vector3 m_playerTargetPosition;

	public BashAttackGame m_bashAttackGame;

	public float m_frictionTimeRemaining;

	public IBashAttackable m_lastTarget;

	public Transform m_seinTransform;

	public bool m_spriteMirrorLock;

	public float m_timeRemainingTillNextBash;

	public float m_timeRemainingOfBashButtonPress;

	public readonly HashSet<ISuspendable> m_bashSuspendables = new HashSet<ISuspendable>();

	public GameObject NoBashTargetEffect;

	public bool IsBashing;

	public float m_bashThroughEnemiesRemainingTime;

	public HashSet<IAttackable> m_enemiesBashedThrough = new HashSet<IAttackable>();

	public bool m_hasStarted;

	public float BackFlipSpeed = 5f;

	[Serializable]
	public class DirectionalAnimationSet
	{
		public TextureAnimationWithTransitions Down;

		public TextureAnimationWithTransitions DownDiagonal;

		public TextureAnimationWithTransitions Horizontal;

		public TextureAnimationWithTransitions Up;

		public TextureAnimationWithTransitions UpDiagonal;
	}
}

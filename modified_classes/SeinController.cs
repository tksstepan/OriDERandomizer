using System;
using Core;
using Game;
using UnityEngine;

public class SeinController : SaveSerialize, IDamageReciever, ISeinReceiver, ISuspendable, ICanActivateStompers
{
	public event Action OnTriggeredAnimationFinished = delegate()
	{
	};

	public bool InputLocked
	{
		get
		{
			return (this.Sein.Abilities.Lever && this.Sein.Abilities.Lever.InputLocked) || GameController.Instance.LockInput || GameController.Instance.LockInputByAction;
		}
	}

	public bool CanMove
	{
		get
		{
			return !this.InputLocked && !this.IsPlayingAnimation;
		}
	}

	public bool FaceLeft
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement.SpriteMirror.FaceLeft;
		}
		set
		{
			this.Sein.PlatformBehaviour.LeftRightMovement.SpriteMirror.FaceLeft = value;
		}
	}

	public Transform Transform
	{
		get
		{
			return this.m_transform;
		}
	}

	public bool IsCrouching
	{
		get
		{
			return this.Sein.Abilities.Crouch && this.Sein.Abilities.Crouch.IsCrouching;
		}
	}

	private bool IsGrabbingBlock
	{
		get
		{
			return this.Sein.Abilities.GrabBlock && this.Sein.Abilities.GrabBlock.IsGrabbing;
		}
	}

	public bool IsGrabbingWall
	{
		get
		{
			return this.Sein.Abilities.GrabWall && this.Sein.Abilities.GrabWall.IsGrabbing;
		}
	}

	public bool IsGrabbingLever
	{
		get
		{
			return this.Sein.Abilities.Lever && this.Sein.Abilities.Lever.IsUsingLever;
		}
	}

	public bool IsGliding
	{
		get
		{
			return this.Sein.Abilities.Glide && this.Sein.Abilities.Glide.IsGliding;
		}
	}

	public bool IsPushPulling
	{
		get
		{
			return this.Sein.Abilities.GrabBlock && this.Sein.Abilities.GrabBlock.IsGrabbing;
		}
	}

	public bool IsSwimming
	{
		get
		{
			return this.Sein.Abilities.Swimming && this.Sein.Abilities.Swimming.IsSwimming;
		}
	}

	public bool IsBashing
	{
		get
		{
			return this.Sein.Abilities.Bash && this.Sein.Abilities.Bash.IsBashing;
		}
	}

	public bool IsAimingGrenade
	{
		get
		{
			return this.Sein.Abilities.Grenade && this.Sein.Abilities.Grenade.IsAiming;
		}
	}

	public bool IsInsideSoulFlame
	{
		get
		{
			return this.Sein.SoulFlame.InsideCheckpointMarker;
		}
	}

	public bool IsCarrying
	{
		get
		{
			return this.Sein.Abilities.Carry && (this.Sein.Abilities.Carry.IsCarrying || this.Sein.Abilities.Carry.IsPickingUp);
		}
	}

	public bool IsStomping
	{
		get
		{
			return this.Sein.Abilities.Stomp && this.Sein.Abilities.Stomp.IsStomping;
		}
	}

	public bool IsCharging
	{
		get
		{
			return this.Sein.Abilities.ChargeFlame && this.Sein.Abilities.ChargeFlame.IsCharging;
		}
	}

	public bool IsChargingJump
	{
		get
		{
			return this.Sein.Abilities.ChargeJumpCharging && this.Sein.Abilities.ChargeJumpCharging.IsCharging;
		}
	}

	public bool IsSuspended { get; set; }

	public Component[] Suspendables
	{
		get
		{
			return this.m_suspendables;
		}
	}

	public bool AnimationHasMetaData
	{
		get
		{
			return this.IsPlayingAnimation && this.Sein.Animation.Animator.CurrentAnimation.AnimationMetaData != null;
		}
	}

	public bool IsDashing
	{
		get
		{
			return this.Sein.Abilities.Dash && this.Sein.Abilities.Dash.IsDashingOrChangeDashing;
		}
	}

	public bool IsStandingOnEdge
	{
		get
		{
			return this.Sein.Abilities.StandingOnEdge && this.Sein.Abilities.StandingOnEdge.StandingOnEdge;
		}
	}

	public void EnterPlayingAnimation()
	{
		this.IsPlayingAnimation = true;
		if (this.Sein.PlatformBehaviour.PlatformMovement)
		{
			Vector2 localSpeed = this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeed;
			localSpeed.x = 0f;
			if (localSpeed.y > 0f)
			{
				localSpeed.y = 0f;
			}
			this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeed = localSpeed;
		}
	}

	public bool CanActivateSwitch(GameObject theSwitch)
	{
		return true;
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
	}

	public void HandleControllerInput()
	{
		if (this.Sein.PlatformBehaviour.LeftRightMovement == null)
		{
			return;
		}
		if (!this.IgnoreControllerInput)
		{
			if (this.CanMove && !this.LockMovementInput)
			{
				this.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput = (float)this.Sein.Input.NormalizedHorizontal;
				if (this.Sein.Abilities.Run.Active && this.Sein.PlatformBehaviour.PlatformMovement.IsOnGround)
				{
					float num = this.Sein.Controller.InputCurve.Evaluate(Mathf.Abs(this.Sein.Input.Horizontal)) * Mathf.Sign(this.Sein.Input.Horizontal);
					this.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput = 0f;
					if (num == 0f)
					{
						this.m_horizontalInputDelay = 0.06666667f;
					}
					if (Mathf.Abs(num) > this.Sein.Controller.InputSettings.JogThreshold)
					{
						this.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput = num;
					}
					this.m_horizontalInputDelay = Mathf.Max(0f, this.m_horizontalInputDelay - Time.deltaTime);
					if (this.m_horizontalInputDelay == 0f)
					{
						this.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput = num;
					}
				}
				else
				{
					this.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput = (float)Core.Input.NormalizedHorizontal;
				}
			}
			else
			{
				this.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput = 0f;
			}
		}
		this.OnHorizontalInputPostCalculate();
	}

	[UberBuildMethod]
	private void ProvideComponents()
	{
		this.m_suspendables = base.gameObject.FindComponentsInChildren<ISuspendable>();
	}

	public override void Awake()
	{
		this.m_transform = base.transform;
		this.ProvideComponents();
		SuspensionManager.Register(this);
		UI.Cameras.Current.ChangeTargetToCurrentCharacter();
		base.Awake();
	}

	public override void OnDestroy()
	{
		SuspensionManager.Unregister(this);
		base.OnDestroy();
		PlatformMovementPortalVisitor component = this.Sein.GetComponent<PlatformMovementPortalVisitor>();
		if (component)
		{
			PlatformMovementPortalVisitor platformMovementPortalVisitor = component;
			platformMovementPortalVisitor.OnGoThroughPortalAction = (Action)Delegate.Remove(platformMovementPortalVisitor.OnGoThroughPortalAction, new Action(this.OnGoThroughPortal));
		}
	}

	public void OnGoThroughPortal()
	{
		this.Sein.ResetAirLimits();
	}

	public void Start()
	{
		this.Sein.PlatformBehaviour.PlatformMovement.PlaceOnGround(0.5f, 0f);
		UI.Cameras.Current.MoveCameraToTargetInstantly(true);
		PlatformMovementPortalVisitor component = this.Sein.GetComponent<PlatformMovementPortalVisitor>();
		if (component)
		{
			PlatformMovementPortalVisitor platformMovementPortalVisitor = component;
			platformMovementPortalVisitor.OnGoThroughPortalAction = (Action)Delegate.Combine(platformMovementPortalVisitor.OnGoThroughPortalAction, new Action(this.OnGoThroughPortal));
		}
	}

	public void HandleJumping()
	{
		if (this.IgnoreControllerInput || this.LockMovementInput || !this.CanMove)
		{
			return;
		}

		bool grenadeJumpPressed = false;
		bool grenadeJumpHeld = false;
		if (RandomizerSettings.GrenadeJump == RandomizerSettings.GrenadeJumpMode.Free)
		{
			grenadeJumpPressed = RandomizerRebinding.FreeGrenadeJump.OnPressed;
			grenadeJumpHeld = RandomizerRebinding.FreeGrenadeJump.Pressed;
		}

		if (Randomizer.GrenadeJumpQueued)
		{
			Randomizer.GrenadeJumpQueued = false;
			if (grenadeJumpHeld && CharacterState.IsActive(this.Sein.Abilities.WallChargeJump) && this.Sein.Abilities.GrabWall && this.Sein.Abilities.WallChargeJump.CanChargeJump && this.IsAimingGrenade)
			{
				Core.Input.LeftShoulder.IsPressed = true;
				Core.Input.Jump.IsPressed = true;
			}
		}

		if (grenadeJumpPressed && CharacterState.IsActive(this.Sein.Abilities.WallChargeJump) && this.Sein.Abilities.GrabWall && this.Sein.Abilities.WallChargeJump.CanChargeJump && this.Sein.Abilities.Grenade && this.Sein.Abilities.Grenade.CanAim)
		{
			Randomizer.GrenadeJumpQueued = true;
			Core.Input.LeftShoulder.IsPressed = true;
			Core.Input.Jump.IsPressed = false;
		}

		if (Core.Input.Jump.OnPressed)
		{
			this.PerformJump();
		}
	}

	public void PerformJump()
	{
		if (CharacterState.IsActive(this.Sein.Abilities.WallChargeJump) && this.Sein.Abilities.GrabWall && this.Sein.Abilities.WallChargeJump.CanChargeJump)
		{
			this.Sein.Abilities.WallChargeJump.PerformChargeJump();
		}
		else if (CharacterState.IsActive(this.Sein.Abilities.WallJump) && this.Sein.Abilities.WallJump.CanPerformWallJump)
		{
			this.Sein.Abilities.WallJump.PerformWallJump();
		}
		else if (!this.IsGrabbingBlock)
		{
			if (CharacterState.IsActive(this.Sein.Abilities.ChargeJump) && this.Sein.Abilities.ChargeJump.CanChargeJump)
			{
				this.Sein.Abilities.ChargeJump.PerformChargeJump();
			}
			else if (CharacterState.IsActive(this.Sein.Abilities.Jump) && this.Sein.Abilities.Jump.CanJump)
			{
				this.Sein.Abilities.Jump.PerformJump();
			}
			else if (CharacterState.IsActive(this.Sein.Abilities.DoubleJump) && this.Sein.Abilities.DoubleJump.CanDoubleJump)
			{
				if (this.Sein.Controller.IsGliding)
				{
					this.Sein.Abilities.Glide.Exit();
				}
				this.Sein.Abilities.DoubleJump.PerformDoubleJump();
			}
		}
	}

	public bool RayTest(GameObject target)
	{
		return this.RayTest(target, Vector2.zero, Vector2.zero);
	}

	public bool RayTest(GameObject target, Vector2 startOffset, Vector2 endOffset)
	{
		Vector3 vector = this.m_transform.position + (Vector3)startOffset;
		Vector3 a = target.transform.position + (Vector3)endOffset;
		Vector3 vector2 = a - vector;
		Rigidbody component = target.GetComponent<Rigidbody>();
		RaycastHit raycastHit;
		return !Physics.Raycast(vector, vector2.normalized, out raycastHit, vector2.magnitude, this.RayTestLayerMask) || !(raycastHit.collider.gameObject != target) || (component && !(component != raycastHit.collider.attachedRigidbody)) || raycastHit.collider.isTrigger;
	}

	public bool RayTest(Vector3 position, Vector3 delta, out RaycastHit hitInfo)
	{
		float magnitude = delta.magnitude;
		return Physics.Raycast(position, delta / magnitude, out hitInfo, magnitude);
	}

	public void StopAnimation()
	{
		this.IsPlayingAnimation = false;
	}

	public void PlayAnimation(TextureAnimationWithTransitions animation)
	{
		Characters.Sein.Controller.EnterPlayingAnimation();
		if (animation.Animation.Loop)
		{
			this.Sein.PlatformBehaviour.Visuals.Animation.PlayLoop(animation, 200, new Func<bool>(this.ShouldAnimationKeepPlaying), false);
		}
		else
		{
			this.Sein.PlatformBehaviour.Visuals.Animation.Play(animation, 200, new Func<bool>(this.ShouldAnimationKeepPlaying));
			this.Sein.PlatformBehaviour.Visuals.Animation.Animator.OnAnimationEndEvent += this.OnAnimationEndEvent;
		}
	}

	private void OnAnimationEndEvent(TextureAnimation textureAnimation)
	{
		this.Sein.PlatformBehaviour.Visuals.Animation.Animator.OnAnimationEndEvent -= this.OnAnimationEndEvent;
		if (this.IsPlayingAnimation)
		{
			this.IsPlayingAnimation = false;
			this.OnTriggeredAnimationFinished();
		}
	}

	public bool ShouldAnimationKeepPlaying()
	{
		return this.IsPlayingAnimation;
	}

	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		if (this.IsPlayingAnimation)
		{
			TextureAnimation currentAnimation = this.Sein.Animation.Animator.CurrentAnimation;
			if (currentAnimation)
			{
				AnimationMetaData animationMetaData = currentAnimation.AnimationMetaData;
				if (animationMetaData)
				{
					Vector3 deltaPositionAtTime = animationMetaData.CameraData.GetDeltaPositionAtTime(this.Sein.Animation.Animator.CurrentAnimationTime);
					Vector3 a = Vector3.Scale(deltaPositionAtTime, this.Sein.PlatformBehaviour.Visuals.Sprite.transform.lossyScale);
					if (this.FaceLeft)
					{
						a.x *= -1f;
					}
					this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeed = a / Time.deltaTime;
				}
				else
				{
					this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeed = Vector2.zero;
				}
			}
		}
		this.HandleControllerInput();
		this.HandleJumping();
		this.UpdateOriActiveState();
	}

	public void HandleOffscreenIssue()
	{
		if (Scenes.Manager.PositionInsideSceneStillLoading(this.Sein.Position))
		{
			this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeed = Vector2.zero;
			this.Sein.Mortality.DamageReciever.MakeInvincible(0.1f);
		}
	}

	public void UpdateOriActiveState()
	{
		if (Characters.Ori && Characters.Ori.gameObject.activeSelf != this.Sein.PlayerAbilities.SpiritFlame.HasAbility)
		{
			Characters.Ori.gameObject.SetActive(this.Sein.PlayerAbilities.SpiritFlame.HasAbility);
		}
	}

	public void UpdateMovementStuff()
	{
		this.Sein.Controller.HandleJumping();
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_horizontalInputDelay);
		if (ar.Reading)
		{
			this.IsPlayingAnimation = false;
			this.LockMovementInput = false;
		}
	}

	public void OnRecieveDamage(Damage damage)
	{
		this.Sein.Mortality.DamageReciever.OnRecieveDamage(damage);
	}

	public SeinAnimationSpeedSettings AnimationSpeedSettings;

	public bool IgnoreControllerInput;

	public bool LockMovementInput;

	public AnimationCurve InputCurve;

	public SeinInputSettings InputSettings;

	public LayerMask RayTestLayerMask;

	public SeinCharacter Sein;

	public bool IsPlayingAnimation;

	public Action OnHorizontalInputPostCalculate = delegate()
	{
	};

	private Transform m_transform;

	public Transform GetItemTransform;

	[SerializeField]
	[HideInInspector]
	private Component[] m_suspendables;

	private float m_horizontalInputDelay;
}

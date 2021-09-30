using System;
using System.Collections;
using Core;
using Game;
using UnityEngine;

public class GetAbilityPedestal : SaveSerialize
{
	public bool SeinInRange
	{
		get
		{
			return !(Characters.Sein == null) && Vector3.Distance(this.m_transform.position, Characters.Sein.Position) < this.Radius;
		}
	}

	private void ChangeState(GetAbilityPedestal.States state)
	{
		if (this.CurrentState == GetAbilityPedestal.States.InRange)
		{
			this.ExitInRangeState();
		}
		this.CurrentState = state;
	}

	public void UpdateStates()
	{
		GetAbilityPedestal.States currentState = this.CurrentState;

		if (currentState != GetAbilityPedestal.States.Completed && RandomizerLocationManager.IsPickupCollected(this.MoonGuid))
		{
			this.ChangeState(GetAbilityPedestal.States.Completed);
			this.ActivatePedestalSequence.PerformInstantly(null);
			return;
		}

		if (currentState != GetAbilityPedestal.States.OutOfRange)
		{
			if (currentState == GetAbilityPedestal.States.InRange)
			{
				this.UpdateInRangeState();
			}
		}
		else
		{
			this.UpdateOutOfRange();
		}
	}

	private void UpdateOutOfRange()
	{
		if (this.SeinInRange)
		{
			this.ChangeState(GetAbilityPedestal.States.InRange);
		}
	}

	private void ExitInRangeState()
	{
		if (this.m_message != null)
		{
			this.m_message.HideMessageScreen();
		}
	}

	public void UpdateInRangeState()
	{
		if (Characters.Sein.PlatformBehaviour.PlatformMovement.IsOnGround)
		{
			if (this.m_message == null && !SeinUI.DebugHideUI)
			{
				this.m_message = UI.Hints.Show(this.PressUpToActivatePedestalMessage, HintLayer.Gameplay, float.PositiveInfinity);
			}
			if (!Characters.Sein.IsSuspended && Characters.Sein.Controller.CanMove && Core.Input.SpiritFlame.OnPressed)
			{
				Core.Input.SpiritFlame.Used = true;
				this.ActivatePedestal();
				return;
			}
		}
		if (!this.SeinInRange)
		{
			this.ChangeState(GetAbilityPedestal.States.OutOfRange);
		}
	}

	public void FixedUpdate()
	{
		this.UpdateStates();
	}

	public void ActivatePedestal()
	{
		base.StartCoroutine(this.MoveSeinToCenterSmoothly());
		if (Characters.Sein.Abilities.Carry && Characters.Sein.Abilities.Carry.CurrentCarryable != null)
		{
			Characters.Sein.Abilities.Carry.CurrentCarryable.Drop();
		}
		Characters.Sein.Mortality.Health.RestoreAllHealth();
		Characters.Sein.Energy.RestoreAllEnergy();
		Characters.Sein.Controller.PlayAnimation(this.GetAbilityAnimation);
		RandomizerLocationManager.GivePickup(this.MoonGuid);
		this.ChangeState(GetAbilityPedestal.States.Completed);
		this.ActivatePedestalSequence.Perform(null);
		GameWorld.Instance.CurrentArea.DirtyCompletionAmount();
	}

	public IEnumerator MoveSeinToCenterSmoothly()
	{
		PlatformMovement seinPlatformMovement = Characters.Sein.PlatformBehaviour.PlatformMovement;
		for (int i = 0; i < 10; i++)
		{
			seinPlatformMovement.PositionX = Mathf.Lerp(seinPlatformMovement.PositionX, base.transform.position.x, 0.2f);
			yield return new WaitForFixedUpdate();
		}
		seinPlatformMovement.PositionX = base.transform.position.x;
		yield break;
	}

	public override void Serialize(Archive ar)
	{
		if (ar.Reading)
		{
			int state = ar.Serialize(0);
			this.ChangeState((GetAbilityPedestal.States)state);
		}
		else
		{
			ar.Serialize((int)this.CurrentState);
		}
	}

	public override void Awake()
	{
		base.Awake();
		this.m_transform = base.transform;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	public GetAbilityPedestal.States CurrentState;

	public AbilityType Ability;

	public PerformingAction ActivatePedestalSequence;

	public float ActivationDuration = 6f;

	public TextureAnimationWithTransitions GetAbilityAnimation;

	public Texture2D PressUpToActivatePedestal;

	public MessageProvider PressUpToActivatePedestalMessage;

	private MessageBox m_message;

	public float Radius = 1.5f;

	private Transform m_transform;

	public enum States
	{
		OutOfRange,
		InRange,
		Completed
	}
}

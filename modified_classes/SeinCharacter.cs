using System;
using Game;
using UnityEngine;

public class SeinCharacter : MonoBehaviour, ICharacter
{
	public Vector2 PhysicsSpeed
	{
		get
		{
			PlatformMovement platformMovement = this.PlatformBehaviour.PlatformMovement;
			return (!platformMovement.IsOnGround) ? platformMovement.WorldSpeed : (platformMovement.GroundNormal * platformMovement.LocalSpeedY + platformMovement.GroundBinormal * platformMovement.LocalSpeedX);
		}
	}

	public CharacterAnimationSystem Animation
	{
		get
		{
			return this.PlatformBehaviour.Visuals.Animation;
		}
	}

	public bool IsSuspended
	{
		get
		{
			return this.PlatformBehaviour.PlatformMovement.IsSuspended;
		}
	}

	public Vector3 Position
	{
		get
		{
			return this.PlatformBehaviour.PlatformMovement.Position;
		}
		set
		{
			this.PlatformBehaviour.PlatformMovement.Position = value;
		}
	}

	public bool Active
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public void Awake()
	{
		Characters.Sein = this;
		Characters.Current = this;
		this.Input = new SeinInput(this);
		this.MakeBelongToSein(base.gameObject);
	}

	public void OnDestroy()
	{
		if (Characters.Sein == this)
		{
			Characters.Sein = null;
		}
		if (object.ReferenceEquals(Characters.Current, this))
		{
			Characters.Current = null;
		}
	}

	public void MakeBelongToSein(GameObject go)
	{
		go.BroadcastMessage("SetReferenceToSein", this, SendMessageOptions.DontRequireReceiver);
	}

	public void FixedUpdate()
	{
		this.Input.Update();
	}

	public void Activate(bool active)
	{
		base.gameObject.SetActive(active);
		if (active)
		{
			base.gameObject.BroadcastMessage("SetReferenceToSein", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	public GameObject GameObject
	{
		get
		{
			return base.gameObject;
		}
	}

	public bool FaceLeft
	{
		get
		{
			return this.Animation.SpriteMirror.FaceLeft;
		}
		set
		{
			this.Animation.SpriteMirror.FaceLeft = value;
		}
	}

	public Vector3 Speed
	{
		get
		{
			return this.PlatformBehaviour.PlatformMovement.LocalSpeed;
		}
		set
		{
			this.PlatformBehaviour.PlatformMovement.LocalSpeed = value;
		}
	}

	public Transform Transform
	{
		get
		{
			return base.transform;
		}
	}

	public bool IsOnGround
	{
		get
		{
			return this.PlatformBehaviour.PlatformMovement.IsOnGround;
		}
	}

	public void PlaceOnGround()
	{
		this.PlatformBehaviour.PlatformMovement.PlaceOnGround(0.5f, 0f);
	}

	public void ResetAirLimits()
	{
		if (this.Abilities.DoubleJump)
		{
			this.Abilities.DoubleJump.ResetDoubleJump();
		}
		if (this.Abilities.Dash)
		{
			this.Abilities.Dash.ResetDashLimit();
		}
	}

	public SeinAbilities Abilities;

	public CloneOfSeinForPortals CloneOfSeinForPortals;

	public SeinController Controller;

	public SeinCutsceneBlocked CutsceneBlocked;

	public SeinCutsceneMovement CutsceneMovement;

	public SeinDoorHandler DoorHandler;

	public SeinSoulFlame SoulFlame;

	public SeinInventory Inventory;

	public SeinEnvironmentForceController ForceController;

	public SeinInput Input;

	public SeinLevel Level;

	public SeinEnergy Energy;

	public SeinMortality Mortality;

	public SeinPickupProcessor PickupHandler;

	public PlatformBehaviour PlatformBehaviour;

	public PlayerAbilities PlayerAbilities;

	public SeinPrefabFactory Prefabs;
}

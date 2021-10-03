using System;
using Game;
using UnityEngine;

public class Naru : MonoBehaviour, ICharacter
{
	public void Awake()
	{
		Characters.Naru = this;
		Characters.Current = this;
	}

	public void OnDestroy()
	{
		Randomizer.onNaruDestroyed();
		if (Characters.Naru == this)
		{
			Characters.Naru = null;
		}
		if (Characters.Current == this)
		{
			Characters.Current = null;
		}
	}

	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public void Activate(bool active)
	{
		base.gameObject.SetActive(active);
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

	public CharacterAnimationSystem Animation;

	public NaruController Controller;

	public PlatformBehaviour PlatformBehaviour;

	public bool SeinNaruComboEnabled;

	public NaruSounds Sounds;
}

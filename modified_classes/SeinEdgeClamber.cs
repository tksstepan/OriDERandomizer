using System;
using Core;
using UnityEngine;

public class SeinEdgeClamber : CharacterState, ISeinReceiver
{
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	public CharacterLeftRightMovement LeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.EdgeClamber = this;
	}

	public override void UpdateCharacterState()
	{
		if (!base.Active)
		{
			return;
		}
		if (this.m_isEdgeClambering)
		{
			if (!this.PlatformMovement.IsOnWall)
			{
				this.m_isEdgeClambering = false;
			}
		}
		else if (this.PlatformMovement.IsOnWall && !this.PlatformMovement.HeadAgainstWall && this.PlatformMovement.FeetAgainstWall && ((this.PlatformMovement.HasWallLeft && this.Sein.Input.NormalizedHorizontal < 0) || (this.PlatformMovement.HasWallRight && this.Sein.Input.NormalizedHorizontal > 0)) && this.PlatformMovement.LocalSpeedY > 0f)
		{
			if (this.PlatformMovement.HasWallLeft && this.Sein.PlatformBehaviour.PlatformMovementListOfColliders.WallLeftCollider && this.Sein.PlatformBehaviour.PlatformMovementListOfColliders.WallLeftCollider.GetComponent<NonEdgeClamberble>())
			{
				return;
			}
			if (this.PlatformMovement.HasWallRight && this.Sein.PlatformBehaviour.PlatformMovementListOfColliders.WallRightCollider && this.Sein.PlatformBehaviour.PlatformMovementListOfColliders.WallRightCollider.GetComponent<NonEdgeClamberble>())
			{
				return;
			}
			this.PerformEdgeClamber();
		}
		base.UpdateCharacterState();
	}

	public void PerformEdgeClamber()
	{
		this.PerformEdgeClamber(0.65f);
	}

	public void PerformEdgeClamber(float minSpeedFactor)
	{
		this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.EdgeClamberAnimation, 10, new Func<bool>(this.ShouldAnimationKeepPlaying));
		this.m_isEdgeClambering = true;
		if (this.PlatformMovement.LocalSpeedY < 9f)
		{
			this.PlatformMovement.LocalSpeedY = 9f;
		}
		if (this.PlatformMovement.HasWallLeft)
		{
			this.PlatformMovement.LocalSpeedX = Mathf.Min(this.PlatformMovement.LocalSpeedX, this.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.MaxSpeed * -minSpeedFactor);
		}
		else
		{
			this.PlatformMovement.LocalSpeedX = Mathf.Max(this.PlatformMovement.LocalSpeedX, this.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.MaxSpeed * minSpeedFactor);
		}
		if (this.EdgeClamberSound)
		{
			Sound.Play(this.EdgeClamberSound.GetSound(null), base.transform.position, null);
		}
		this.Sein.PlatformBehaviour.AirNoDeceleration.NoDeceleration = true;
	}

	public bool ShouldAnimationKeepPlaying()
	{
		return !this.PlatformMovement.IsOnGround;
	}

	public TextureAnimationWithTransitions EdgeClamberAnimation;

	public SoundProvider EdgeClamberSound;

	public SeinCharacter Sein;

	private bool m_isEdgeClambering;
}

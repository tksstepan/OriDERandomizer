using System;
using UnityEngine;

public class WormMortarShootingState : WormState
{
	public WormMortarShootingState(WormEnemy worm, MortarWormDirectionalAnimations shoot, PrefabSpawner shootEffect, SoundSource shootSound, ProjectileSpawner projectileSpawner, float shootDelay, float projectileDamage) : base(worm)
	{
		this.m_shoot = shoot;
		this.m_shootEffect = shootEffect;
		this.m_shootSound = shootSound;
		this.m_projectileSpawner = projectileSpawner;
		this.m_shootDelay = shootDelay;
		this.m_projectileDamage = projectileDamage;
	}

	public override void OnEnter()
	{
		MortarWormEnemy mortarWormEnemy = (MortarWormEnemy)this.Worm;
		Vector3 direction = (this.m_projectileSpawner.Speed * this.m_projectileSpawner.Direction + 0.5f * this.m_projectileSpawner.Gravity * this.m_shootDelay * this.m_shootDelay * Vector3.down).normalized;
		direction = mortarWormEnemy.transform.InverseTransformDirection(direction);
		if (mortarWormEnemy.FaceLeft)
		{
			direction.x *= -1f;
		}
		this.Worm.Animation.Play(this.m_shoot.PickWithDirection(direction), 0, null);
		this.m_projectileAnimationPosition = mortarWormEnemy.Spawn.FindPosition(direction);
	}

	public override void OnExit()
	{
		this.m_hasShot = false;
		base.OnExit();
	}

	public override void UpdateState()
	{
		if (base.CurrentStateTime >= this.m_shootDelay && !this.m_hasShot)
		{
			this.m_hasShot = true;
			if (this.m_shootEffect)
			{
				this.m_shootEffect.Spawn(null);
			}
			if (this.m_shootSound)
			{
				this.m_shootSound.Play();
			}
			Projectile projectile = this.m_projectileSpawner.SpawnProjectile();
			Vector3 b = RandomizerBonusSkill.TimeScale(projectile.Direction * projectile.Speed * this.m_shootDelay + Vector3.down * projectile.Gravity * this.m_shootDelay * this.m_shootDelay * 0.5f);
			projectile.Position += b;
			projectile.SpeedVector += Vector3.down * projectile.Gravity * this.m_shootDelay;
			projectile.GetComponent<DamageDealer>().Damage = this.m_projectileDamage;
			Vector3 vector = this.m_projectileAnimationPosition - projectile.Position;
			vector.z = 0f;
			projectile.Position += vector;
			projectile.Displacement = vector;
		}
		base.UpdateState();
	}

	private readonly MortarWormDirectionalAnimations m_shoot;

	private readonly PrefabSpawner m_shootEffect;

	private readonly SoundSource m_shootSound;

	private readonly ProjectileSpawner m_projectileSpawner;

	private readonly float m_shootDelay;

	private readonly float m_projectileDamage;

	private Vector3 m_projectileAnimationPosition;

	private bool m_hasShot;
}

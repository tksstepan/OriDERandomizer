using System;
using UnityEngine;

// Token: 0x020004FE RID: 1278
public class WormMortarShootingState : WormState
{
	// Token: 0x06001B7F RID: 7039 RVA: 0x00018064 File Offset: 0x00016264
	public WormMortarShootingState(WormEnemy worm, MortarWormDirectionalAnimations shoot, PrefabSpawner shootEffect, SoundSource shootSound, ProjectileSpawner projectileSpawner, float shootDelay, float projectileDamage) : base(worm)
	{
		this.m_shoot = shoot;
		this.m_shootEffect = shootEffect;
		this.m_shootSound = shootSound;
		this.m_projectileSpawner = projectileSpawner;
		this.m_shootDelay = shootDelay;
		this.m_projectileDamage = projectileDamage;
	}

	// Token: 0x06001B80 RID: 7040 RVA: 0x000872A0 File Offset: 0x000854A0
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

	// Token: 0x06001B81 RID: 7041 RVA: 0x0001809B File Offset: 0x0001629B
	public override void OnExit()
	{
		this.m_hasShot = false;
		base.OnExit();
	}

	// Token: 0x06001B82 RID: 7042
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

	// Token: 0x040018B0 RID: 6320
	private readonly MortarWormDirectionalAnimations m_shoot;

	// Token: 0x040018B1 RID: 6321
	private readonly PrefabSpawner m_shootEffect;

	// Token: 0x040018B2 RID: 6322
	private readonly SoundSource m_shootSound;

	// Token: 0x040018B3 RID: 6323
	private readonly ProjectileSpawner m_projectileSpawner;

	// Token: 0x040018B4 RID: 6324
	private readonly float m_shootDelay;

	// Token: 0x040018B5 RID: 6325
	private readonly float m_projectileDamage;

	// Token: 0x040018B6 RID: 6326
	private Vector3 m_projectileAnimationPosition;

	// Token: 0x040018B7 RID: 6327
	private bool m_hasShot;
}

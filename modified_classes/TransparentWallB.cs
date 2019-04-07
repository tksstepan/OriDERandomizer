using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x020008E4 RID: 2276
public class TransparentWallB : SaveSerialize, ISuspendable
{
	// Token: 0x060031F0 RID: 12784 RVA: 0x00027E31 File Offset: 0x00026031
	public TransparentWallB()
	{
		this.IsSuspended = false;
	}

	// Token: 0x060031F1 RID: 12785 RVA: 0x000030B0 File Offset: 0x000012B0
	public new void Awake()
	{
		SuspensionManager.Register(this);
	}

	// Token: 0x060031F2 RID: 12786 RVA: 0x000029E2 File Offset: 0x00000BE2
	public new void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	// Token: 0x060031F3 RID: 12787 RVA: 0x00027E40 File Offset: 0x00026040
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_hasBeenShown);
	}

	// Token: 0x170007DB RID: 2011
	// (get) Token: 0x060031F4 RID: 12788 RVA: 0x00027E4E File Offset: 0x0002604E
	public float SenseTime
	{
		get
		{
			return this.Animator.Duration / 2f;
		}
	}

	// Token: 0x060031F5 RID: 12789 RVA: 0x000D03F8 File Offset: 0x000CE5F8
	public void Start()
	{
		AnimatorDriver animatorDriver = this.Animator.AnimatorDriver;
		if (this.WallVisible)
		{
			this.Animator.Initialize();
			animatorDriver.GoToEnd();
		}
		else if (this.HasSense)
		{
			this.Animator.Initialize();
			animatorDriver.CurrentTime = this.SenseTime;
			animatorDriver.Pause();
			animatorDriver.Sample();
		}
		else
		{
			this.Animator.Initialize();
			animatorDriver.GoToStart();
		}
	}

	// Token: 0x060031F6 RID: 12790 RVA: 0x00027E61 File Offset: 0x00026061
	public void OnTriggerEnter(Collider other)
	{
		this.OnEnterTrigger(other);
		this.OnTrigger(other);
	}

	// Token: 0x060031F7 RID: 12791 RVA: 0x00027E71 File Offset: 0x00026071
	public void OnTriggerStay(Collider other)
	{
		this.OnTrigger(other);
	}

	// Token: 0x060031F8 RID: 12792 RVA: 0x000D0478 File Offset: 0x000CE678
	private void OnEnterTrigger(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			if (!this.m_hasBeenShown)
			{
				if (SeinTransparentWallHandler.Instance)
				{
					Sound.Play(SeinTransparentWallHandler.Instance.EnterTransparentWallFirstTimeSoundProvider.GetSound(null), base.transform.position, null);
				}
			}
			else if (SeinTransparentWallHandler.Instance)
			{
				Sound.Play(SeinTransparentWallHandler.Instance.EnterTransparentWallSoundProvider.GetSound(null), base.transform.position, null);
			}
		}
	}

	// Token: 0x060031F9 RID: 12793 RVA: 0x00027E7A File Offset: 0x0002607A
	public void OnTrigger(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			this.m_beingTriggered = true;
			if (!this.m_hasBeenShown)
			{
				this.m_hasBeenShown = true;
				AchievementsLogic.Instance.RevealTransparentWall();
			}
		}
	}

	// Token: 0x060031FA RID: 12794 RVA: 0x000D050C File Offset: 0x000CE70C
	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		AnimatorDriver animatorDriver = this.Animator.AnimatorDriver;
		if (this.WallVisible)
		{
			if (animatorDriver.IsReversed || !animatorDriver.IsPlaying)
			{
				animatorDriver.SetForward();
				animatorDriver.Resume();
			}
		}
		else if (this.m_lastVisiable)
		{
			animatorDriver.SetBackwards();
			animatorDriver.Resume();
			if (SeinTransparentWallHandler.Instance)
			{
				Sound.Play(SeinTransparentWallHandler.Instance.LeaveTransparentWallSoundProvider.GetSound(null), base.transform.position, null);
			}
		}
		this.m_lastVisiable = this.WallVisible;
		if (animatorDriver.CurrentTime < this.SenseTime && this.HasSense)
		{
			animatorDriver.Pause();
			animatorDriver.CurrentTime = this.SenseTime;
			animatorDriver.Sample();
		}
		this.m_beingTriggered = false;
	}

	// Token: 0x170007DC RID: 2012
	// (get) Token: 0x060031FB RID: 12795
	public bool HasSense
	{
		get
		{
			return !(Characters.Sein == null);
		}
	}

	// Token: 0x170007DD RID: 2013
	// (get) Token: 0x060031FC RID: 12796 RVA: 0x00027EDC File Offset: 0x000260DC
	public bool WallVisible
	{
		get
		{
			return this.m_beingTriggered;
		}
	}

	// Token: 0x170007DE RID: 2014
	// (get) Token: 0x060031FD RID: 12797 RVA: 0x00027EE4 File Offset: 0x000260E4
	// (set) Token: 0x060031FE RID: 12798 RVA: 0x00027EEC File Offset: 0x000260EC
	public bool IsSuspended { get; set; }

	// Token: 0x04002D3A RID: 11578
	private bool m_hasBeenShown;

	// Token: 0x04002D3B RID: 11579
	private bool m_lastVisiable;

	// Token: 0x04002D3C RID: 11580
	private bool m_beingTriggered;

	// Token: 0x04002D3D RID: 11581
	public BaseAnimator Animator;
}

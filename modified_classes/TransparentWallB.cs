using System;
using Core;
using Game;
using UnityEngine;

public class TransparentWallB : SaveSerialize, ISuspendable
{
	public TransparentWallB()
	{
		this.IsSuspended = false;
	}

	public new void Awake()
	{
		SuspensionManager.Register(this);
	}

	public new void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_hasBeenShown);
	}

	public float SenseTime
	{
		get
		{
			return this.Animator.Duration / 2f;
		}
	}

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

	public void OnTriggerEnter(Collider other)
	{
		this.OnEnterTrigger(other);
		this.OnTrigger(other);
	}

	public void OnTriggerStay(Collider other)
	{
		this.OnTrigger(other);
	}

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

	public bool HasSense
	{
		get
		{
			return !(Characters.Sein == null);
		}
	}

	public bool WallVisible
	{
		get
		{
			return this.m_beingTriggered;
		}
	}

	public bool IsSuspended { get; set; }

	private bool m_hasBeenShown;

	private bool m_lastVisiable;

	private bool m_beingTriggered;

	public BaseAnimator Animator;
}

using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

// Token: 0x0200089A RID: 2202
public class Lever : SaveSerialize, ISuspendable, IDynamicGraphicHierarchy
{
	// Token: 0x06002FCD RID: 12237 RVA: 0x000CA530 File Offset: 0x000C8730
	public Lever()
	{
	}

	// Token: 0x06002FCE RID: 12238 RVA: 0x0002662D File Offset: 0x0002482D
	static Lever()
	{
	}

	// Token: 0x1700078D RID: 1933
	// (get) Token: 0x06002FCF RID: 12239 RVA: 0x00026639 File Offset: 0x00024839
	// (set) Token: 0x06002FD0 RID: 12240 RVA: 0x00026641 File Offset: 0x00024841
	public bool InRange { get; private set; }

	// Token: 0x1700078E RID: 1934
	// (get) Token: 0x06002FD1 RID: 12241 RVA: 0x0002664A File Offset: 0x0002484A
	// (set) Token: 0x06002FD2 RID: 12242 RVA: 0x00026652 File Offset: 0x00024852
	public bool IsGrabbed { get; private set; }

	// Token: 0x1700078F RID: 1935
	// (get) Token: 0x06002FD3 RID: 12243 RVA: 0x00003D16 File Offset: 0x00001F16
	public SeinCharacter Sein
	{
		get
		{
			return Characters.Sein;
		}
	}

	// Token: 0x17000790 RID: 1936
	// (get) Token: 0x06002FD4 RID: 12244 RVA: 0x0002665B File Offset: 0x0002485B
	public bool NeedsToBeOnGround
	{
		get
		{
			return this.LeverType != Lever.LeverMode.LeftRightToggle;
		}
	}

	// Token: 0x06002FD5 RID: 12245 RVA: 0x00026669 File Offset: 0x00024869
	public void OnEnable()
	{
		Lever.All.Add(this);
	}

	// Token: 0x06002FD6 RID: 12246 RVA: 0x00026676 File Offset: 0x00024876
	public void OnDisable()
	{
		Lever.All.Remove(this);
	}

	// Token: 0x06002FD7 RID: 12247 RVA: 0x00026684 File Offset: 0x00024884
	public override void Awake()
	{
		base.Awake();
		this.Transform = base.transform;
		SuspensionManager.Register(this);
	}

	// Token: 0x06002FD8 RID: 12248 RVA: 0x0000512E File Offset: 0x0000332E
	public override void OnDestroy()
	{
		base.OnDestroy();
		SuspensionManager.Unregister(this);
	}

	// Token: 0x06002FD9 RID: 12249 RVA: 0x000CA6E8 File Offset: 0x000C88E8
	public void OnEnterLever()
	{
		this.InRange = true;
		this.LeverEnterEvent();
		foreach (LegacyAnimator legacyAnimator in this.HighlightAnimators)
		{
			if (legacyAnimator)
			{
				legacyAnimator.ContinueForward();
			}
		}
	}

	// Token: 0x06002FDA RID: 12250 RVA: 0x000CA738 File Offset: 0x000C8938
	public void OnExitLever()
	{
		this.InRange = false;
		this.LeverExitEvent();
		foreach (LegacyAnimator legacyAnimator in this.HighlightAnimators)
		{
			if (legacyAnimator)
			{
				legacyAnimator.ContinueBackward();
			}
		}
	}

	// Token: 0x06002FDB RID: 12251 RVA: 0x0002669E File Offset: 0x0002489E
	public void OnGrabLever()
	{
		this.IsGrabbed = true;
		this.GrabLeverEvent();
	}

	// Token: 0x06002FDC RID: 12252 RVA: 0x000266B2 File Offset: 0x000248B2
	public void OnReleaseLever()
	{
		if (this.LeverType == Lever.LeverMode.LeftMiddleRightSpring && this.Direction != Lever.LeverDirections.Middle)
		{
			this.OnPushLeverMiddle();
		}
		this.IsGrabbed = false;
		this.ReleaseLeverEvent();
	}

	// Token: 0x06002FDD RID: 12253 RVA: 0x000CA788 File Offset: 0x000C8988
	public void OnPushLeverLeft()
	{
		if (this.CanLeverLeft())
		{
			this.Direction = Lever.LeverDirections.Left;
			this.LeverLeftEvent();
			if (this.LeftSound)
			{
				Sound.Play(this.LeftSound.GetSound(null), base.transform.position, null);
			}
		}
		else
		{
			this.LeverLeftFailedEvent();
		}
	}

	// Token: 0x06002FDE RID: 12254 RVA: 0x000CA7F8 File Offset: 0x000C89F8
	public void OnPushLeverRight()
	{
		if (this.CanLeverRight())
		{
			this.Direction = Lever.LeverDirections.Right;
			this.LeverRightEvent();
			if (this.RightSound)
			{
				Sound.Play(this.RightSound.GetSound(null), base.transform.position, null);
			}
		}
		else
		{
			this.LeverRightFailedEvent();
		}
	}

	// Token: 0x06002FDF RID: 12255 RVA: 0x000CA868 File Offset: 0x000C8A68
	public void OnPushLeverMiddle()
	{
		this.Direction = Lever.LeverDirections.Middle;
		this.LeverMiddleEvent();
		if (this.MiddleSound)
		{
			Sound.Play(this.MiddleSound.GetSound(null), base.transform.position, null);
		}
	}

	// Token: 0x06002FE0 RID: 12256 RVA: 0x000266E4 File Offset: 0x000248E4
	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		this.HackyRotatingHandle();
	}

	// Token: 0x06002FE1 RID: 12257 RVA: 0x000CA8B8 File Offset: 0x000C8AB8
	private void HackyRotatingHandle()
	{
		if (this.RotatingHandle)
		{
			if (this.Direction == Lever.LeverDirections.Left)
			{
				this.m_handleRotationTime = Mathf.Max(-1f, this.m_handleRotationTime - this.HandleRotationSpeed * Time.deltaTime);
			}
			else if (this.Direction == Lever.LeverDirections.Right)
			{
				this.m_handleRotationTime = Mathf.Min(1f, this.m_handleRotationTime + this.HandleRotationSpeed * Time.deltaTime);
			}
			else
			{
				if (this.m_handleRotationTime > 0f)
				{
					this.m_handleRotationTime = Mathf.Max(0f, this.m_handleRotationTime - this.HandleRotationSpeed * Time.deltaTime);
				}
				if (this.m_handleRotationTime < 0f)
				{
					this.m_handleRotationTime = Mathf.Min(0f, this.m_handleRotationTime + this.HandleRotationSpeed * Time.deltaTime);
				}
			}
			this.RotatingHandle.localEulerAngles = new Vector3(0f, 0f, -this.HandleRotation.Evaluate(this.m_handleRotationTime) * this.HandleRotationAmount);
		}
	}

	// Token: 0x06002FE2 RID: 12258 RVA: 0x000266F8 File Offset: 0x000248F8
	public bool PlayLeverAnimation()
	{
		return this.InRange && this.IsGrabbed;
	}

	// Token: 0x17000791 RID: 1937
	// (get) Token: 0x06002FE3 RID: 12259 RVA: 0x0002670E File Offset: 0x0002490E
	public Vector3 Position
	{
		get
		{
			return this.Transform.position;
		}
	}

	// Token: 0x17000792 RID: 1938
	// (get) Token: 0x06002FE4 RID: 12260 RVA: 0x0002671B File Offset: 0x0002491B
	public float SeinPositionOffset
	{
		get
		{
			return 0.8f;
		}
	}

	// Token: 0x06002FE5 RID: 12261 RVA: 0x000CA9D8 File Offset: 0x000C8BD8
	public void SetLeverDirection(Lever.LeverDirections leverDirection)
	{
		if (this.Direction == leverDirection)
		{
			return;
		}
		switch (leverDirection)
		{
		case Lever.LeverDirections.Left:
			this.OnPushLeverLeft();
			break;
		case Lever.LeverDirections.Middle:
			this.OnPushLeverMiddle();
			break;
		case Lever.LeverDirections.Right:
			this.OnPushLeverRight();
			break;
		}
	}

	// Token: 0x06002FE6 RID: 12262 RVA: 0x00026722 File Offset: 0x00024922
	public override void Serialize(Archive ar)
	{
		this.Direction = (Lever.LeverDirections)ar.Serialize((int)this.Direction);
		if (ar.Reading)
		{
			this.IsGrabbed = false;
			this.InRange = false;
		}
	}

	// Token: 0x17000793 RID: 1939
	// (get) Token: 0x06002FE7 RID: 12263 RVA: 0x0002674F File Offset: 0x0002494F
	// (set) Token: 0x06002FE8 RID: 12264 RVA: 0x00026757 File Offset: 0x00024957
	public bool IsSuspended { get; set; }

	// Token: 0x17000794 RID: 1940
	// (get) Token: 0x06002FE9 RID: 12265
	public bool CanBeGrabbed
	{
		get
		{
			return (!Randomizer.OpenWorld || !(Scenes.Manager.CurrentScene.Scene == "westGladesFireflyAreaA")) && (this.CanGrabCondition == null || this.CanGrabCondition.Validate(null));
		}
	}

	// Token: 0x04002B45 RID: 11077
	public static List<Lever> All = new List<Lever>();

	// Token: 0x04002B46 RID: 11078
	public float Radius = 2f;

	// Token: 0x04002B47 RID: 11079
	public Transform RotatingHandle;

	// Token: 0x04002B48 RID: 11080
	public Lever.LeverDirections Direction = Lever.LeverDirections.Middle;

	// Token: 0x04002B49 RID: 11081
	public LegacyAnimator[] HighlightAnimators;

	// Token: 0x04002B4A RID: 11082
	public Varying2DSoundProvider LeftSound;

	// Token: 0x04002B4B RID: 11083
	public Varying2DSoundProvider MiddleSound;

	// Token: 0x04002B4C RID: 11084
	public Varying2DSoundProvider RightSound;

	// Token: 0x04002B4D RID: 11085
	public Condition CanGrabCondition;

	// Token: 0x04002B4E RID: 11086
	public Lever.LeverMode LeverType = Lever.LeverMode.LeftMiddleRightSpring;

	// Token: 0x04002B4F RID: 11087
	public Transform Transform;

	// Token: 0x04002B50 RID: 11088
	public AnimationCurve HandleRotation;

	// Token: 0x04002B51 RID: 11089
	private float m_handleRotationTime;

	// Token: 0x04002B52 RID: 11090
	public float HandleRotationSpeed;

	// Token: 0x04002B53 RID: 11091
	public float HandleRotationAmount = 40f;

	// Token: 0x04002B54 RID: 11092
	public Action GrabLeverEvent = delegate()
	{
	};

	// Token: 0x04002B55 RID: 11093
	public Action ReleaseLeverEvent = delegate()
	{
	};

	// Token: 0x04002B56 RID: 11094
	public Action LeverLeftEvent = delegate()
	{
	};

	// Token: 0x04002B57 RID: 11095
	public Action LeverRightEvent = delegate()
	{
	};

	// Token: 0x04002B58 RID: 11096
	public Action LeverLeftFailedEvent = delegate()
	{
	};

	// Token: 0x04002B59 RID: 11097
	public Action LeverRightFailedEvent = delegate()
	{
	};

	// Token: 0x04002B5A RID: 11098
	public Action LeverMiddleEvent = delegate()
	{
	};

	// Token: 0x04002B5B RID: 11099
	public Action LeverEnterEvent = delegate()
	{
	};

	// Token: 0x04002B5C RID: 11100
	public Action LeverExitEvent = delegate()
	{
	};

	// Token: 0x04002B5D RID: 11101
	public Func<bool> CanLeverLeft = () => true;

	// Token: 0x04002B5E RID: 11102
	public Func<bool> CanLeverRight = () => true;

	// Token: 0x0200089B RID: 2203
	public enum LeverDirections
	{
		// Token: 0x04002B6E RID: 11118
		Left,
		// Token: 0x04002B6F RID: 11119
		Middle,
		// Token: 0x04002B70 RID: 11120
		Right
	}

	// Token: 0x0200089C RID: 2204
	public enum LeverMode
	{
		// Token: 0x04002B72 RID: 11122
		LeftRightToggle,
		// Token: 0x04002B73 RID: 11123
		LeftRightGrab,
		// Token: 0x04002B74 RID: 11124
		LeftMiddleRightSpring,
		// Token: 0x04002B75 RID: 11125
		LeftMiddleRightStay
	}
}

using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x0200001B RID: 27
internal class BashAttackGame : Suspendable, IPooled
{
	// Token: 0x06000096 RID: 150 RVA: 0x000029B5 File Offset: 0x00000BB5
	public BashAttackGame()
	{
	}

	// Token: 0x14000001 RID: 1
	// (add) Token: 0x06000097 RID: 151 RVA: 0x0002D30C File Offset: 0x0002B50C
	// (remove) Token: 0x06000098 RID: 152 RVA: 0x0002D344 File Offset: 0x0002B544
	public event Action<float> BashGameComplete;

	// Token: 0x1700001E RID: 30
	// (get) Token: 0x06000099 RID: 153 RVA: 0x000029CF File Offset: 0x00000BCF
	// (set) Token: 0x0600009A RID: 154 RVA: 0x000029D7 File Offset: 0x00000BD7
	public override bool IsSuspended { get; set; }

	// Token: 0x0600009B RID: 155 RVA: 0x0002D37C File Offset: 0x0002B57C
	public void OnPoolSpawned()
	{
		this.m_bashLoopingAudioSource = null;
		this.m_keyboardSpeed = 0f;
		this.m_keyboardAngle = 0f;
		this.m_keyboardClockwise = false;
		this.m_mode = BashAttackGame.Modes.Keyboard;
		this.m_currentState = BashAttackGame.State.Appearing;
		this.Angle = 0f;
		this.m_stateCurrentTime = 0f;
		this.m_nextBashLoopPlayedTime = 0f;
		this.BashAttackCritical.enabled = true;
		this.IsSuspended = false;
		this.BashGameComplete = null;
	}

	// Token: 0x0600009C RID: 156 RVA: 0x0002D3F8 File Offset: 0x0002B5F8
	public void ChangeState(BashAttackGame.State state)
	{
		this.m_currentState = state;
		this.m_stateCurrentTime = 0f;
		switch (state)
		{
		case BashAttackGame.State.Appearing:
			this.BashAttackCritical.enabled = false;
			return;
		case BashAttackGame.State.Playing:
			this.BashAttackCritical.enabled = true;
			return;
		case BashAttackGame.State.Disappearing:
			this.BashAttackCritical.enabled = false;
			if (this.m_bashLoopingAudioSource)
			{
				InstantiateUtility.Destroy(this.m_bashLoopingAudioSource.gameObject);
			}
			return;
		default:
			return;
		}
	}

	// Token: 0x0600009D RID: 157 RVA: 0x0002D470 File Offset: 0x0002B670
	public void UpdateMode()
	{
		if (Core.Input.AnalogAxisLeft.magnitude > 0.2f)
		{
			this.m_mode = BashAttackGame.Modes.Controller;
			return;
		}
		if (Core.Input.CursorMoved || GameSettings.Instance.CurrentControlScheme == ControlScheme.KeyboardAndMouse)
		{
			this.m_mode = BashAttackGame.Modes.Mouse;
			return;
		}
		if (Core.Input.DigiPadAxis.magnitude > 0.2f && this.m_mode != BashAttackGame.Modes.Mouse)
		{
			this.m_mode = BashAttackGame.Modes.Keyboard;
		}
	}

	// Token: 0x0600009E RID: 158
	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		if (this.m_currentState != BashAttackGame.State.Disappearing)
		{
			this.UpdateMode();
			switch (this.m_mode)
			{
			case BashAttackGame.Modes.Mouse:
			{
				Vector2 v = UI.Cameras.Current.Camera.WorldToScreenPoint(base.transform.position);
				Vector2 b = UI.Cameras.System.GUICamera.ScreenToWorldPoint(v);
				Vector2 vector = Core.Input.CursorPositionUI - b;
				if (vector.magnitude > 0.001f)
				{
					vector.Normalize();
					this.Angle = Mathf.LerpAngle(this.Angle, Mathf.Atan2(-vector.x, vector.y) * 57.29578f, 0.5f);
				}
				break;
			}
			case BashAttackGame.Modes.Keyboard:
			{
				Vector2 digiPadAxis = Core.Input.DigiPadAxis;
				if ((double)digiPadAxis.magnitude > 0.2)
				{
					float target = MoonMath.Angle.AngleFromVector(digiPadAxis) - 90f;
					float f = Mathf.DeltaAngle(this.m_keyboardAngle, target);
					if (Mathf.Sign(f) != (float)((!this.m_keyboardClockwise) ? -1 : 1))
					{
						this.m_keyboardClockwise = (Mathf.Sign(f) > 0f);
						this.m_keyboardSpeed = 0f;
					}
					this.m_keyboardSpeed += Mathf.Min(Mathf.Abs(f), Time.deltaTime * 2000f);
					this.m_keyboardAngle = Mathf.MoveTowardsAngle(this.m_keyboardAngle, target, this.m_keyboardSpeed * Time.deltaTime);
				}
				else
				{
					this.m_keyboardSpeed = 0f;
				}
				this.Angle = Mathf.LerpAngle(this.Angle, this.m_keyboardAngle, 0.5f);
				break;
			}
			case BashAttackGame.Modes.Controller:
			{
				Vector2 vector2 = Core.Input.AnalogAxisLeft;
				float sqrMagnitude = vector2.sqrMagnitude;
				if (sqrMagnitude > RandomizerSettings.BashDeadzone)
				{
					vector2 /= Mathf.Sqrt(sqrMagnitude);
					this.Angle = Mathf.LerpAngle(this.Angle, Mathf.Atan2(-vector2.x, vector2.y) * 57.29578f, 0.5f);
				}
				break;
			}
			}
		}
		this.ArrowSprite.transform.parent.rotation = Quaternion.Euler(0f, 0f, this.Angle);
		this.UpdateState();
		if (Characters.Sein && !Characters.Sein.Active)
		{
			InstantiateUtility.Destroy(base.gameObject);
		}
	}

	// Token: 0x0600009F RID: 159 RVA: 0x000029E0 File Offset: 0x00000BE0
	public void SendDirection(Vector2 direction)
	{
		this.m_keyboardAngle = MoonMath.Angle.AngleFromVector(direction) - 90f;
	}

	// Token: 0x060000A0 RID: 160 RVA: 0x0002D73C File Offset: 0x0002B93C
	public void UpdateState()
	{
		switch (this.m_currentState)
		{
		case BashAttackGame.State.Appearing:
			this.UpdateAppearingState();
			break;
		case BashAttackGame.State.Playing:
			this.UpdatePlayingState();
			break;
		case BashAttackGame.State.Disappearing:
			this.UpdateDisappearingState();
			break;
		}
		this.m_stateCurrentTime += Time.deltaTime;
	}

	// Token: 0x060000A1 RID: 161 RVA: 0x0002D78C File Offset: 0x0002B98C
	private void UpdateDisappearingState()
	{
		float time = Mathf.Clamp01(this.m_stateCurrentTime / this.DisappearTime);
		this.ArrowSprite.localScale = this.m_originalArrowScale * this.ArrowDisappearScaleCurve.Evaluate(time);
		InstantiateUtility.Destroy(base.gameObject, 1f);
	}

	// Token: 0x060000A2 RID: 162 RVA: 0x0002D7E0 File Offset: 0x0002B9E0
	private void UpdatePlayingState()
	{
		if (this.m_nextBashLoopPlayedTime <= this.m_stateCurrentTime)
		{
			this.m_bashLoopingAudioSource = Sound.Play((!Characters.Sein.PlayerAbilities.BashBuff.HasAbility) ? Characters.Sein.Abilities.Bash.BashLoopSound.GetSound(null) : Characters.Sein.Abilities.Bash.UpgradedBashLoopSound.GetSound(null), base.transform.position, delegate()
			{
				this.m_bashLoopingAudioSource = null;
			});
			if (!InstantiateUtility.IsDestroyed(this.m_bashLoopingAudioSource))
			{
				this.m_nextBashLoopPlayedTime = this.m_stateCurrentTime + this.m_bashLoopingAudioSource.Length;
			}
		}
		if (this.BashAttackCritical.CurrentState == BashAttackCritical.State.Finished)
		{
			this.GameFinished();
		}
		RandomizerRebinding.DoubleBash.wasPressed = false;
		if (this.ButtonBash.Released || (RandomizerRebinding.DoubleBash.IsPressed() && Randomizer.BashTap))
		{
			this.GameFinished();
		}
	}

	// Token: 0x060000A3 RID: 163 RVA: 0x0002D8D8 File Offset: 0x0002BAD8
	private void UpdateAppearingState()
	{
		float num = Mathf.Clamp01(this.m_stateCurrentTime / this.AppearTime);
		this.ArrowSprite.localScale = this.m_originalArrowScale * this.ArrowAppearScaleCurve.Evaluate(num);
		if (num == 1f)
		{
			this.ChangeState(BashAttackGame.State.Playing);
		}
	}

	// Token: 0x060000A4 RID: 164 RVA: 0x000029F4 File Offset: 0x00000BF4
	public new void Awake()
	{
		base.Awake();
		this.m_originalArrowScale = this.ArrowSprite.localScale;
	}

	// Token: 0x060000A5 RID: 165 RVA: 0x00002A0D File Offset: 0x00000C0D
	public void Start()
	{
		this.ChangeState(this.m_currentState);
		this.ArrowSprite.localScale = Vector3.zero;
	}

	// Token: 0x060000A6 RID: 166 RVA: 0x0002D92C File Offset: 0x0002BB2C
	private void GameFinished()
	{
		Sound.Play((!Characters.Sein.PlayerAbilities.BashBuff.HasAbility) ? Characters.Sein.Abilities.Bash.BashEndSound.GetSound(null) : Characters.Sein.Abilities.Bash.UpgradedBashEndSound.GetSound(null), base.transform.position, null);
		this.BashGameComplete(this.Angle);
		this.ChangeState(BashAttackGame.State.Disappearing);
		RandomizerRebinding.DoubleBash.wasPressed = false;
		if (RandomizerRebinding.DoubleBash.IsPressed() && !Randomizer.BashWasQueued)
		{
			Randomizer.QueueBash = true;
		}
		Randomizer.BashWasQueued = false;
	}

	// Token: 0x1700001F RID: 31
	// (get) Token: 0x060000A7 RID: 167 RVA: 0x00002A2B File Offset: 0x00000C2B
	public Core.Input.InputButtonProcessor ButtonBash
	{
		get
		{
			return Core.Input.Bash;
		}
	}

	// Token: 0x040000C6 RID: 198
	public float Angle;

	// Token: 0x040000C7 RID: 199
	public float ArrowSpeed = 45f;

	// Token: 0x040000C8 RID: 200
	public Transform ArrowSprite;

	// Token: 0x040000C9 RID: 201
	public BashAttackCritical BashAttackCritical;

	// Token: 0x040000CA RID: 202
	public float AppearTime;

	// Token: 0x040000CB RID: 203
	public float DisappearTime;

	// Token: 0x040000CC RID: 204
	public AnimationCurve ArrowAppearScaleCurve;

	// Token: 0x040000CD RID: 205
	public AnimationCurve ArrowDisappearScaleCurve;

	// Token: 0x040000CE RID: 206
	private BashAttackGame.State m_currentState;

	// Token: 0x040000CF RID: 207
	private float m_stateCurrentTime;

	// Token: 0x040000D0 RID: 208
	private float m_nextBashLoopPlayedTime;

	// Token: 0x040000D1 RID: 209
	private Vector3 m_originalArrowScale;

	// Token: 0x040000D2 RID: 210
	private SoundPlayer m_bashLoopingAudioSource;

	// Token: 0x040000D3 RID: 211
	private float m_keyboardSpeed;

	// Token: 0x040000D4 RID: 212
	private float m_keyboardAngle;

	// Token: 0x040000D5 RID: 213
	private bool m_keyboardClockwise;

	// Token: 0x040000D6 RID: 214
	private BashAttackGame.Modes m_mode = BashAttackGame.Modes.Keyboard;

	// Token: 0x0200001C RID: 28
	public enum State
	{
		// Token: 0x040000DA RID: 218
		Appearing,
		// Token: 0x040000DB RID: 219
		Playing,
		// Token: 0x040000DC RID: 220
		Disappearing
	}

	// Token: 0x0200001D RID: 29
	public enum Modes
	{
		// Token: 0x040000DE RID: 222
		Mouse,
		// Token: 0x040000DF RID: 223
		Keyboard,
		// Token: 0x040000E0 RID: 224
		Controller
	}
}

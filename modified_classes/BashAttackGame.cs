using System;
using Core;
using Game;
using UnityEngine;

internal class BashAttackGame : Suspendable, IPooled
{
	public BashAttackGame()
	{
	}

	public event Action<float> BashGameComplete;

	public override bool IsSuspended { get; set; }

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
				if (sqrMagnitude > RandomizerSettings.Controls.BashDeadzone)
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

	public void SendDirection(Vector2 direction)
	{
		this.m_keyboardAngle = MoonMath.Angle.AngleFromVector(direction) - 90f;
	}

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

	private void UpdateDisappearingState()
	{
		float time = Mathf.Clamp01(this.m_stateCurrentTime / this.DisappearTime);
		this.ArrowSprite.localScale = this.m_originalArrowScale * this.ArrowDisappearScaleCurve.Evaluate(time);
		InstantiateUtility.Destroy(base.gameObject, 1f);
	}

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
		if (this.ButtonBash.Released || (RandomizerRebinding.DoubleBash.OnPressed && Randomizer.BashTap))
		{
			this.GameFinished();
		}
	}

	private void UpdateAppearingState()
	{
		float num = Mathf.Clamp01(this.m_stateCurrentTime / this.AppearTime);
		this.ArrowSprite.localScale = this.m_originalArrowScale * this.ArrowAppearScaleCurve.Evaluate(num);
		if (num == 1f)
		{
			this.ChangeState(BashAttackGame.State.Playing);
		}
	}

	public new void Awake()
	{
		base.Awake();
		this.m_originalArrowScale = this.ArrowSprite.localScale;
	}

	public void Start()
	{
		this.ChangeState(this.m_currentState);
		this.ArrowSprite.localScale = Vector3.zero;
	}

	private void GameFinished()
	{
		Sound.Play((!Characters.Sein.PlayerAbilities.BashBuff.HasAbility) ? Characters.Sein.Abilities.Bash.BashEndSound.GetSound(null) : Characters.Sein.Abilities.Bash.UpgradedBashEndSound.GetSound(null), base.transform.position, null);
		this.BashGameComplete(this.Angle);
		this.ChangeState(BashAttackGame.State.Disappearing);
		if (RandomizerRebinding.DoubleBash.Pressed && !Randomizer.BashWasQueued)
		{
			Randomizer.QueueBash = true;
		}
		Randomizer.BashWasQueued = false;
	}

	public Core.Input.InputButtonProcessor ButtonBash
	{
		get
		{
			return Core.Input.Bash;
		}
	}

	public float Angle;

	public float ArrowSpeed = 45f;

	public Transform ArrowSprite;

	public BashAttackCritical BashAttackCritical;

	public float AppearTime;

	public float DisappearTime;

	public AnimationCurve ArrowAppearScaleCurve;

	public AnimationCurve ArrowDisappearScaleCurve;

	private BashAttackGame.State m_currentState;

	private float m_stateCurrentTime;

	private float m_nextBashLoopPlayedTime;

	private Vector3 m_originalArrowScale;

	private SoundPlayer m_bashLoopingAudioSource;

	private float m_keyboardSpeed;

	private float m_keyboardAngle;

	private bool m_keyboardClockwise;

	private BashAttackGame.Modes m_mode = BashAttackGame.Modes.Keyboard;

	public enum State
	{
		Appearing,
		Playing,
		Disappearing
	}

	public enum Modes
	{
		Mouse,
		Keyboard,
		Controller
	}
}

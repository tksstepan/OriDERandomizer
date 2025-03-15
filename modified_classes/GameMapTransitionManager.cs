using System;
using Core;
using UnityEngine;

public class GameMapTransitionManager : MonoBehaviour
{
	public bool IsTransitioning
	{
		get
		{
			return this.m_zoomTime != 0f && this.m_zoomTime < 1f;
		}
	}

	public bool InWorldMapMode
	{
		get
		{
			return Mathf.Approximately(this.m_zoomTime, 0f);
		}
	}

	public bool InAreaMapMode
	{
		get
		{
			return this.m_zoomTime >= 1f;
		}
	}

	public void Awake()
	{
		GameMapTransitionManager.Instance = this;
	}

	public void OnDestroy()
	{
		if (GameMapTransitionManager.Instance == this)
		{
			GameMapTransitionManager.Instance = null;
		}
	}

	public float ZoomTime
	{
		get
		{
			return this.m_zoomTime;
		}
	}

	public void ZoomToWorldMap()
	{
        if (!GameMapUI.Instance.ShowingTeleporters)
		{
			return;
		}
		if (this.ZoomOutSound)
		{
			this.ZoomOutSound.Play();
		}
		if (this.InAreaMapZoomOutSound)
		{
			this.InAreaMapZoomOutSound.Stop();
		}
		this.GoToWorldMap();
	}

	public void ZoomToAreaMap()
	{
		if (this.ZoomInSound)
		{
			this.ZoomInSound.Play();
		}
		this.GoToAreaMap();
	}

	public void Update()
	{
		this.m_mouseWheel += UnityEngine.Input.GetAxis("Mouse ScrollWheel");
	}

	public void Advance()
	{
		if (!GameMapUI.Instance.ShowingObjective && !GameMapUI.Instance.RevealingMap)
		{
			bool flag = Core.Input.ZoomOut.Pressed;
			bool flag2 = Core.Input.ZoomIn.Pressed;
			float num = this.m_mouseWheel * 50f;
			this.m_mouseWheel = 0f;
			this.m_zoomSpeed = Mathf.Lerp(this.m_zoomSpeed, num, 0.5f);
			if (flag || flag2)
			{
				this.m_zoomSpeed = (float)(((!flag2) ? 0 : 1) - ((!flag) ? 0 : 1));
				this.m_zeroZoom = true;
			}
			else if (this.m_zeroZoom)
			{
				this.m_zoomSpeed = 0f;
				this.m_zeroZoom = false;
			}
			if (num > 0f)
			{
				flag2 = true;
			}
			else if (num < 0f)
			{
				flag = true;
			}
			if (flag)
			{
				if (this.m_areaMode && this.m_zoomTime <= 1f)
				{
					this.ZoomToWorldMap();
				}
			}
			else if (this.m_zoomSpeed >= 0.05f && this.InAreaMapZoomOutSound)
			{
				this.InAreaMapZoomOutSound.Stop();
			}
			if (flag2)
			{
				if (!this.m_areaMode)
				{
					this.ZoomToAreaMap();
				}
			}
			else if (this.m_zoomSpeed <= -0.05f && this.InAreaMapZoomInSound)
			{
				this.InAreaMapZoomInSound.Stop();
			}
			if (this.m_areaMode)
			{
				if (this.m_zoomTime >= 1f)
				{
					if (this.m_zoomSpeed < -0.05f)
					{
						if (this.InAreaMapZoomOutSound && !this.InAreaMapZoomOutSound.IsPlaying)
						{
							this.InAreaMapZoomOutSound.Play();
						}
						this.m_zoomTime += Time.deltaTime * this.m_zoomSpeed;
					}
					else if (this.m_zoomSpeed > 0.05f)
					{
						if (this.InAreaMapZoomInSound && !this.InAreaMapZoomInSound.IsPlaying)
						{
							this.InAreaMapZoomInSound.Play();
						}
						this.m_zoomTime += Time.deltaTime * this.m_zoomSpeed;
						this.m_zoomTime = Mathf.Clamp(this.m_zoomTime, 1f, 2f);
					}
				}
			}
			else if (Core.Input.ActionButtonA.OnPressed && !Core.Input.ActionButtonA.Used)
			{
				Core.Input.ActionButtonA.Used = true;
				this.ZoomToAreaMap();
			}
		}
		if (this.m_areaMode && this.m_zoomTime < 1f)
		{
			this.m_zoomTime += 1f / this.ZoomDuration * Time.deltaTime;
			this.m_zoomTime = Mathf.Clamp01(this.m_zoomTime);
			if (this.m_zoomTime == 1f)
			{
				WorldMapUI.Instance.Deactivate();
                return;
			}
		}
		else if (!this.m_areaMode)
		{
			this.m_zoomTime -= 1f / this.ZoomDuration * Time.deltaTime;
			this.m_zoomTime = Mathf.Clamp01(this.m_zoomTime);
			if (this.m_zoomTime == 0f)
			{
				AreaMapUI.Instance.Hide();
			}
		}
	}

	public void GoToWorldMap()
	{
		WorldMapUI.Instance.Activate();
		this.m_areaMode = false;
		AreaMapUI.Instance.FadeOutAnimator.Initialize();
		AreaMapUI.Instance.FadeOutAnimator.AnimatorDriver.ContinueForward();
		WorldMapUI.Instance.CrossFade.Initialize();
		WorldMapUI.Instance.CrossFade.AnimatorDriver.ContinueForward();
	}

	public void GoToAreaMap()
	{
		AreaMapUI.Instance.ResetMaps();
		this.m_areaMode = true;
		AreaMapUI.Instance.Show();
		AreaMapUI.Instance.Init();
		AreaMapUI.Instance.FadeOutAnimator.Initialize();
		AreaMapUI.Instance.FadeOutAnimator.AnimatorDriver.ContinueBackwards();
		WorldMapUI.Instance.CrossFade.Initialize();
		WorldMapUI.Instance.CrossFade.AnimatorDriver.ContinueBackwards();
	}

	public void GoToAreaMapInstantly()
	{
		this.m_areaMode = true;
		this.m_zoomTime = 1f;
		WorldMapUI.Instance.Deactivate();
		WorldMapUI.Instance.CrossFade.Initialize();
		WorldMapUI.Instance.CrossFade.AnimatorDriver.GoToStart();
		WorldMapUI.Instance.CrossFade.AnimatorDriver.Pause();
		AreaMapUI.Instance.FadeOutAnimator.Initialize();
		AreaMapUI.Instance.FadeOutAnimator.AnimatorDriver.GoToStart();
		AreaMapUI.Instance.FadeOutAnimator.AnimatorDriver.Pause();
		AreaMapUI.Instance.Show();
		AreaMapUI.Instance.Init();
	}

	public void GoToWorldMapInstantly()
	{
		this.m_areaMode = false;
		this.m_zoomTime = 0f;
		AreaMapUI.Instance.Hide();
		WorldMapUI.Instance.Activate();
		WorldMapUI.Instance.CrossFade.Initialize();
		WorldMapUI.Instance.CrossFade.AnimatorDriver.GoToEnd();
		WorldMapUI.Instance.CrossFade.AnimatorDriver.Pause();
		AreaMapUI.Instance.FadeOutAnimator.Initialize();
		AreaMapUI.Instance.FadeOutAnimator.AnimatorDriver.GoToEnd();
		AreaMapUI.Instance.FadeOutAnimator.AnimatorDriver.Pause();
	}

	public static GameMapTransitionManager Instance;

	private float m_zoomTime = 1f;

	public SoundSource ZoomInSound;

	public SoundSource ZoomOutSound;

	public SoundSource InAreaMapZoomInSound;

	public SoundSource InAreaMapZoomOutSound;

	private bool m_areaMode = true;

	public float ZoomDuration = 1f;

	private float m_mouseWheelSmooth;

	private float m_zoomSpeed;

	private bool m_zeroZoom;

	private float m_mouseWheel;
}
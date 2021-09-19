using System;
using UnityEngine;

public class CameraShakeLogic : MonoBehaviour, ISuspendable
{
	public void Awake()
	{
		SuspensionManager.Register(this);
	}

	public void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	public void UpdateOffset()
	{
		Vector3 shakeOffset = Vector3.zero;
		Vector3 shakeRotation = Vector3.zero;
		
		for (int i = 0; i < CameraShake.All.Count; i++)
		{
			CameraShake cameraShake = CameraShake.All[i];
			float modifiedStrength = cameraShake.ModifiedStrength;
			shakeOffset += cameraShake.CurrentOffset * modifiedStrength;
			shakeRotation += cameraShake.CurrentRotation * modifiedStrength;
		}

		if (RandomizerSettings.Accessibility.ReduceCameraShake)
		{
			shakeOffset *= (1f - RandomizerSettings.Accessibility.CameraShakeReduction);
			shakeRotation *= (1f - RandomizerSettings.Accessibility.CameraShakeReduction);
		}

		this.Target.localPosition = shakeOffset;
		this.Target.localEulerAngles = shakeRotation;
	}

	public bool IsSuspended { get; set; }

	public Transform Target;
}

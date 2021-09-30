using System;
using UnityEngine;

[Category("General")]
public class ActivateAction : ActionMethod
{
	public void OnValidate()
	{
		if (this.Save && this.Target && this.Target.GetComponent<GameObjectActivator>())
		{
			this.Save = false;
		}
	}

	public override void Perform(IContext context)
	{
		this.Target.SetActive(this.Activate);
	}

	public override void PerformInstantly(IContext context)
	{
		this.Perform(context);
	}

	public override void Serialize(Archive ar)
	{
		if (this.Save)
		{
			if (ar.Reading)
			{
				bool active = ar.Serialize(true);
				if (this.Target)
				{
					this.Target.SetActive(active);
				}
			}
			if (ar.Writing)
			{
				if (this.Target == null)
				{
					ar.Serialize(false);
				}
				else
				{
					ar.Serialize(this.Target.activeSelf);
				}
			}
		}
	}

	private string TargetName
	{
		get
		{
			return (!(this.Target != null)) ? "unkown" : this.Target.name;
		}
	}

	public override string GetNiceName()
	{
		return ((!this.Activate) ? "Deactivate " : "Activate ") + this.TargetName;
	}

	[NotNull]
	public GameObject Target;

	public bool Activate = true;

	public bool Save = true;
}

using System;

public abstract class ActionMethod : SaveSerialize, IAction
{
	public void Start()
	{
	}

	public override void Serialize(Archive ar)
	{
	}

	public virtual string GetNiceName()
	{
		return StringUtility.AddSpaces(base.GetType().Name);
	}

	public abstract void Perform(IContext context);

	public virtual void PerformInstantly(IContext context)
	{
	}
}

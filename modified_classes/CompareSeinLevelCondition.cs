using System;
using Game;

// Token: 0x0200016F RID: 367
public class CompareSeinLevelCondition : Condition
{
	// Token: 0x06000836 RID: 2102
	public override bool Validate(IContext context)
	{
		return Characters.Sein.Level.Current % 128 == this.Value;
	}

	// Token: 0x04000819 RID: 2073
	public int Value;
}

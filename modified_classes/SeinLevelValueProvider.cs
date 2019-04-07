using System;
using Game;

// Token: 0x0200050A RID: 1290
public class SeinLevelValueProvider : FloatValueProvider
{
	// Token: 0x06001B9E RID: 7070
	public override float GetFloatValue()
	{
		return (float)(Characters.Sein.Level.Current % 128 + 1);
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;

// Token: 0x020009EF RID: 2543
public class RandomizerMessageProvider : MessageProvider
{
	// Token: 0x0600373C RID: 14140 RVA: 0x0002B54E File Offset: 0x0002974E
	public RandomizerMessageProvider()
	{
		this.messages = new MessageDescriptor[1];
	}

	public RandomizerMessageProvider(string message)
	{
		this.messages = new MessageDescriptor[1];
		this.messages[0] = new MessageDescriptor(message);
	}

	// Token: 0x0600373D RID: 14141 RVA: 0x0002B562 File Offset: 0x00029762
	[DebuggerHidden]
	public override IEnumerable<MessageDescriptor> GetMessages()
	{
		return this.messages;
	}

	// Token: 0x0600373E RID: 14142 RVA: 0x0002B56A File Offset: 0x0002976A
	public void SetMessage(string message)
	{
		this.messages[0] = new MessageDescriptor(message);
	}

	// Token: 0x04003228 RID: 12840
	public MessageDescriptor[] messages;
}

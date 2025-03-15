using System;
using System.Collections.Generic;
using System.Diagnostics;

public class RandomizerMessageProvider : MessageProvider
{
	public RandomizerMessageProvider()
	{
		this.messages = new MessageDescriptor[1];
	}

	public RandomizerMessageProvider(string message)
	{
		this.messages = new MessageDescriptor[1];
		this.messages[0] = new MessageDescriptor(message);
	}

	[DebuggerHidden]
	public override IEnumerable<MessageDescriptor> GetMessages()
	{
		return this.messages;
	}

	public void SetMessage(string message)
	{
		this.messages[0] = new MessageDescriptor(message);
	}

	public MessageDescriptor[] messages;
}

using System;

public class RandomizerPickupAction : ActionMethod
{
	public void Awake()
	{
		if (this.LocationName != null)
		{
			return;
		}

		if (RandomizerLocationManager.LocationsByGuid.ContainsKey(this.MoonGuid))
		{
			this.LocationName = RandomizerLocationManager.LocationsByGuid[this.MoonGuid].Name;
		}
		
		if (this.LocationName == null)
		{
			this.LocationName = "Unknown";
		}
	}

	public override void Perform(IContext context)
	{
		if (!this.Granted)
		{
			RandomizerLocationManager.GivePickup(this.MoonGuid);
			this.Granted = true;
		}
	}

	public override string GetNiceName()
	{
		return "Give randomized pickup " + this.LocationName;
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.Granted);
	}

	public string LocationName;

	public bool Granted;
}
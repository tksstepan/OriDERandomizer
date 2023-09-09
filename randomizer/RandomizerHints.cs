using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class RandomizerHints {

	public delegate String StringMaker();
	public static List<StringMaker> NewPlayerTips = new List<StringMaker>() {
		new StringMaker(() => $"If you're unable to proceed, Warp ({RandomizerRebinding.ReturnToStart.FirstBindName()}) elsewhere and come back later"),
		new StringMaker(() => $"*Charge Flame*, *Grenade*, and the #Charge Dash Ability# can all break open *Blue* #Walls#, #Floors#, and #Petrified Plants#. Collectively, these are known as *Blue Breakage*"),
		new StringMaker(() => $"*Stomp* has a shockwave can break some #Walls# from the side"),
		new StringMaker(() => "Some enemies are immune to some spikes, allowing them to be lured surprising distances"),
		new StringMaker(() => "The *Blue* Ability Tree is by far the most important when playing Ori Rando"),
		new StringMaker(() => "The @Red@ Ability Tree is rarely worth investing in. (*Stomp*, *Charge Jump*, and #Charge Dash# are extremely effective against enemies)"),
		new StringMaker(() => $"You can Warp ({RandomizerRebinding.ReturnToStart.FirstBindName()}) to avoid damage from enemies, poison water, or spikes"),
		new StringMaker(() => $"You can Warp ({RandomizerRebinding.ReturnToStart.FirstBindName()}) out of #Kuro's Nest# to avoid getting chased"),
		new StringMaker(() => $"Hold {RandomizerRebinding.DoubleBash.FirstBindName()} while Bashing to *Double Bash*."),

		// common to both (may refactor how this is done)
		new StringMaker(() => "Extra Double Jump lets you jump an additional time in the air.\nRequires *Double Jump*; stacks with #Triple Jump# in the *Blue* #Ability Tree#\n(See all Bonus Item Descriptions in the Bonus Item Glossary at orirando.com/faq)"),
		new StringMaker(() => "Extra Air Dash lets you dash an additional time in the air.\nRequires *Dash* and #Air Dash# in the *Blue* #Ability Tree#\n(See all Bonus Item Descriptions in the Bonus Item Glossary at orirando.com/faq)"),
		new StringMaker(() => "Health Regeneration restores your Health slowly over time\n(See all Bonus Item Descriptions in the Bonus Item Glossary at orirando.com/faq)"),
		new StringMaker(() => "Energy Regeneration restores your Energy slowly over time\n(See all Bonus Item Descriptions in the Bonus Item Glossary at orirando.com/faq)"),
		new StringMaker(() => "Join the Ori community at orirando.com/discord"),
		new StringMaker(() => "Use the *Logic Helper* map filter to check which pickups are currently in logic"),
		new StringMaker(() => "Some enemies are immune to some spikes, allowing them to be lured surprising distances"),
		new StringMaker(() => "The top of the Ginso escape will always teleport you to Thornfelt Swamp"),
		new StringMaker(() => "Tips can be disabled using RandomizerSettings.txt"),
			};

	public static HashSet<int> SeenNPTs = new HashSet<int>();

	public static List<StringMaker> MiscTips = new List<StringMaker>() {
		new StringMaker(() => "Extra Double Jump lets you jump an additional time in the air.\nRequires *Double Jump*; stacks with #Triple Jump# in the *Blue* #Ability Tree#\n(See all Bonus Item Descriptions in the Bonus Item Glossary at orirando.com/faq)"),
		new StringMaker(() => "Extra Air Dash lets you dash an additional time in the air.\nRequires *Dash* and #Air Dash# in the *Blue* #Ability Tree#\n(See all Bonus Item Descriptions in the Bonus Item Glossary at orirando.com/faq)"),
		new StringMaker(() => "Health Regeneration restores your Health slowly over time\n(See all Bonus Item Descriptions in the Bonus Item Glossary at orirando.com/faq)"),
		new StringMaker(() => "Energy Regeneration restores your Energy slowly over time\n(See all Bonus Item Descriptions in the Bonus Item Glossary at orirando.com/faq)"),
		new StringMaker(() => "Once you've cleared the fog in #Misty Woods#\nyou can change the layout by interacting with the orb pedestal"),
		new StringMaker(() => "If you start the #Forlorn# escape and Warp away immediately,\nyou can retry it later by returning to the start room and jumping.\nBe sure not to Warp out halfway through!"),
		new StringMaker(() => "Join the Ori community at orirando.com/discord"),
		new StringMaker(() => "Use the *Logic Helper* map filter to check which pickups are currently in logic"),
		new StringMaker(() => "Some enemies are immune to some spikes, allowing them to be lured surprising distances"),
		new StringMaker(() => "The top of the Ginso escape will always teleport you to Thornfelt Swamp"),
		new StringMaker(() => "Tips can be disabled using RandomizerSettings.txt"),
		new StringMaker(() => "Report bugs and discuss upcoming rando features in the *dev* discord (orirando.com/discord/dev)"),	};

	public static HashSet<int> SeenMiscs = new HashSet<int>();

	private static Random hintRandom = new Random();

	public static void TryShowSenseHint() {
		try {
			if(File.GetLastWriteTime("RandomizerSettings.txt") > new DateTime(2023,9,9,0,0,0)) return;
			Randomizer.Print("In patch 4.0.7 onwards, #Sense# only changes Ori's color from the default (or custom) color when it is detecting an item.\nIf you previously set your Sense #cold color# to be similar to your normal color, consider changing one or both.\n(Don't want to see this message anymore? Add a blank line to the bottom of your RandomizerSettings.txt file and it will stop showing up.)", 15, false, false, false, true);
		} catch(Exception e) { Randomizer.LogError($"TryShowSenseHint: {e.Message}"); }
	}

	public static void ShowTip() {
		var hl = RandomizerSettings.Customization.HintLevel.Value;
		if(hl == RandomizerSettings.HintLevels.Disabled) return;
		if(hl == RandomizerSettings.HintLevels.Skilled) {
			if(SeenMiscs.Count == MiscTips.Count) SeenMiscs.Clear();
			var h = hintRandom.Next(MiscTips.Count);
			while(SeenMiscs.Contains(h)) h = (h + 1) % MiscTips.Count;
			Randomizer.Print(MiscTips[h](), 9, false, false, false, true);
			SeenMiscs.Add(h);
		} else {
			if(SeenNPTs.Count == NewPlayerTips.Count) SeenNPTs.Clear();
			var h = hintRandom.Next(NewPlayerTips.Count);
			while(SeenNPTs.Contains(h)) h = (h + 1) % NewPlayerTips.Count;
			Randomizer.Print(NewPlayerTips[h](), 9, false, false, false, true);
			SeenNPTs.Add(h);
			return;

		}

	}
	
}
using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class RandomizerBootstrap
{
	public static void Initialize()
	{
		Events.Scheduler.OnSceneRootPreEnabled.Add(new Action<SceneRoot>(RandomizerBootstrap.BootstrapScene));
	}

	public static void FixedUpdate()
	{
		for (int i = 0; i < RandomizerBootstrap.s_bootstrappedScenes.Count;)
		{
			if (Core.Scenes.Manager.GetSceneManagerScene(RandomizerBootstrap.s_bootstrappedScenes[i]) != null)
			{
				i++;
			}
			else
			{
				RandomizerBootstrap.s_bootstrappedScenes.RemoveAt(i);
			}
		}
	}

	private static void BootstrapScene(SceneRoot sceneRoot)
	{
		if (RandomizerBootstrap.s_bootstrappedScenes.Contains(sceneRoot.name))
		{
			return;
		}

		if (RandomizerBootstrap.s_bootstrap.ContainsKey(sceneRoot.name))
		{
			RandomizerBootstrap.s_bootstrappedScenes.Add(sceneRoot.name);
			RandomizerBootstrap.s_bootstrap[sceneRoot.name].Invoke(sceneRoot);
		}
	}

	private static void TwiddleGuidAndSave(SceneRoot sceneRoot, GuidOwner owner)
	{
		// this is a horrendous hack but it works well enough to place new, serializable objects without maintaining a giant GUID store
		// MoonGuid is a v4 UUID, which is a 16-byte identifier with two special bit sequences:
		//		* in the 16-bit identifier spanning bytes 6-7, the four most significant bits must be 0100 (4), indicating a v4 UUID
		//		* in byte 8, the two most significant bits must be 10, a special sequence indicating UUID "variant 2"
		// we can ensure "unique" "UUIDs" by just abusing the four version bits, incrementing the "version" by 1 for each clone
		// this results in an invalid UUID but fortunately this literally only matters for differentiating saved object data
		MoonGuid originalGuid = owner.MoonGuid;
		owner.MoonGuid = new MoonGuid(originalGuid.A, originalGuid.B + 268435456, originalGuid.C, originalGuid.D);

		if (owner is SaveSerialize)
		{
			(owner as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		}
	}

	private static Transform CloneObject(SceneRoot sceneRoot, Transform obj, string name=null, bool sibling=true)
	{
		// temporarily fiddle with the original object's active status to prevent the clone from instantly awaking if it shouldn't
		bool originalActive = obj.gameObject.activeSelf;
		if (!obj.gameObject.activeInHierarchy)
		{
			obj.gameObject.SetActive(false);
		}

		Transform clone = UnityEngine.Object.Instantiate<Transform>(obj);
		if (name != null)
		{
			clone.gameObject.name = name;
		}
		if (sibling)
		{
			clone.parent = obj.parent;
		}

		// reinstate active status after the clone is part of the hierarchy
		obj.gameObject.SetActive(originalActive);
		clone.gameObject.SetActive(originalActive);

		foreach (GuidOwner owner in clone.gameObject.FindComponentsInChildren<GuidOwner>())
		{
			TwiddleGuidAndSave(sceneRoot, owner);
		}

		return clone;
	}

	private static void BootstrapTitleScreen(SceneRoot sceneRoot)
	{
		SaveSlotsItemsUI itemsUI = sceneRoot.transform.FindChild("ui").GetComponent<TitleScreenManager>().SaveSlotsScreen.ItemsUI;
		foreach (SaveSlotUI saveSlotUI in new UnityEngine.Object[2]{ itemsUI.SaveSlotUI, itemsUI.SaveSlotCompletedUI })
		{
			saveSlotUI.EasyTextMessageProvider = RandomizerText.DifficultyOverrides.Easy.NameOverride;
			saveSlotUI.NormalTextMessageProvider = RandomizerText.DifficultyOverrides.Normal.NameOverride;
			saveSlotUI.HardTextMessageProvider = RandomizerText.DifficultyOverrides.Hard.NameOverride;
			saveSlotUI.OneLifeTestMessageProvider = RandomizerText.DifficultyOverrides.OneLife.NameOverride;
		}

		CleverMenuItemSelectionManager difficultyManager = itemsUI.SaveSlotUI.DifficultyScreen.GetComponent<CleverMenuItemSelectionManager>();
		difficultyManager.MenuItems[0].GetComponentInChildren<MessageBox>(true).SetMessageProvider(RandomizerText.DifficultyOverrides.Easy.NameOverride);
		difficultyManager.MenuItems[0].GetComponentInChildren<CleverMenuItemTooltip>(true).Tooltip = RandomizerText.DifficultyOverrides.Easy.DescriptionOverride;
		difficultyManager.MenuItems[1].GetComponentInChildren<MessageBox>(true).SetMessageProvider(RandomizerText.DifficultyOverrides.Normal.NameOverride);
		difficultyManager.MenuItems[1].GetComponentInChildren<CleverMenuItemTooltip>(true).Tooltip = RandomizerText.DifficultyOverrides.Normal.DescriptionOverride;
		difficultyManager.MenuItems[2].GetComponentInChildren<MessageBox>(true).SetMessageProvider(RandomizerText.DifficultyOverrides.Hard.NameOverride);
		difficultyManager.MenuItems[2].GetComponentInChildren<CleverMenuItemTooltip>(true).Tooltip = RandomizerText.DifficultyOverrides.Hard.DescriptionOverride;
		difficultyManager.MenuItems[3].GetComponentInChildren<MessageBox>(true).SetMessageProvider(RandomizerText.DifficultyOverrides.OneLife.NameOverride);
		difficultyManager.MenuItems[3].GetComponentInChildren<CleverMenuItemTooltip>(true).Tooltip = RandomizerText.DifficultyOverrides.OneLife.DescriptionOverride;
		difficultyManager.Index = 0;
	}

	private static void BootstrapBlackrootLanternRoom(SceneRoot sceneRoot)
	{
		Transform darkPlatforms = sceneRoot.transform.FindChild("*lightDarkPlatforms").FindChild("darkPlatforms");
		Transform physicsManager = darkPlatforms.FindChild("physicsManager");

		// add difficulty condition to the platform container; use this to toggle all the platforms
		DifficultyCondition condition = darkPlatforms.gameObject.AddComponent<DifficultyCondition>();
		condition.Easy = true;
		condition.Normal = false;
		condition.Hard = false;
		condition.OneLife = false;
		
		// make a clone of the physics system and set it as a sibling of the original
		Transform alternateManager = CloneObject(sceneRoot, physicsManager, "physicsManagerRelaxed");

		// activate the clone if the condition is met, else activate the original
		ActivationBasedOnCondition activation = darkPlatforms.gameObject.AddComponent<ActivationBasedOnCondition>();
		activation.Condition = condition;
		activation.TargetTrue = alternateManager.gameObject;
		activation.TargetFalse = physicsManager.gameObject;

		// force the scene to re-validate since we've added things
		sceneRoot.OnValidate();

		// modify platforms in the clone
		foreach (Transform child in alternateManager)
		{
			if (child.position.x < 125f)
			{
				child.rotation *= Quaternion.Euler(0f, 0f, 60f);

				// "unrotate" the swaying movement of the platforms back to Y-axis alignment
				SinMovement componentInChildren = child.GetComponentInChildren<SinMovement>();
				SinMovement.Affect affectorY = componentInChildren.Affectors[0];

				Vector2 rotatedRange = MoonMath.Angle.Unrotate(new Vector2(0f, affectorY.Range / 2f), 60f);
				Vector2 rotatedRangeRandom = MoonMath.Angle.Unrotate(new Vector2(0f, affectorY.Range / 2f), 60f);

				affectorY.Range = rotatedRange.y;
				affectorY.RangeRandom = rotatedRangeRandom.y;

				SinMovement.Affect affectorX = new SinMovement.Affect();
				affectorX.Type = SinMovement.Affect.AffectType.X;
				affectorX.Offset = affectorY.Offset;
				affectorX.OffsetRandom = affectorY.OffsetRandom;
				affectorX.Period = affectorY.Period;
				affectorX.PeriodRandom = affectorY.PeriodRandom;
				affectorX.Range = rotatedRange.x;
				affectorX.RangeRandom = rotatedRangeRandom.x;
				componentInChildren.Affectors.Add(affectorX);
			}
			else if (child.position.x < 135f)
			{
				child.GetComponentInChildren<SinMovement>().Affectors[0].Range /= 2f;
			}
			else
			{
				child.position -= new Vector3(0f, 0.2f, 0f);
				child.GetComponentInChildren<SinMovement>().Affectors[0].Range /= 2f;
			}
		}
	}

	private static void BootstrapSpiritTree(SceneRoot sceneRoot)
	{
		// Unlike most other pickups, which are permanent placeholders that spawn an object with a DestroyOnRestoreCheckpoint component,
		// this one is *just* an object with a DestroyOnRestoreCheckpoint component. Disable that to prevent its untimely demise.
		sceneRoot.transform.FindChild("mediumExpOrb").GetComponent<DestroyOnRestoreCheckpoint>().enabled = false;
	}

	private static void BootstrapValleyThreeBirdArea(SceneRoot sceneRoot)
	{
		if (Characters.Sein && Characters.Sein.Inventory && Characters.Sein.Inventory.GetRandomizerItem(800) > 0)
		{
			Transform leverSetup = sceneRoot.transform.FindChild("*leverSetup");

			// Just disconnect the lever from the door; leave the lever itself interact for Ori to play with if they want
			ActionLeverSystem leverSystem = leverSetup.GetComponentInChildren<ActionLeverSystem>();
			leverSystem.LeverLeftAction = null;
			leverSystem.LeverRightAction = null;

			// Force the door open
			LegacyTranslateAnimator doorAnimator = leverSetup.FindChild("platformBranchSetup").FindChild("sunkenGladesStompTree").GetComponent<LegacyTranslateAnimator>();
			doorAnimator.TimeOffset = doorAnimator.TimeOfLastCurvePoint;
		}
	}

	private static void BootstrapThornfeltSwampMain(SceneRoot sceneRoot)
	{
		// force the music to start up, dang it
		ActionSequence musicSequence = sceneRoot.transform.FindChild("musicZones").FindChild("musicActivation").GetComponent<ActionSequence>();
		OnSceneStartRunAction runAction = musicSequence.gameObject.AddComponent<OnSceneStartRunAction>();
		runAction.ActionToRun = musicSequence;
		runAction.TriggerOnce = true;
		runAction.MoonGuid = new MoonGuid(560691571, 1097907217, -1524861543, 276788056);
		(runAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);

		// patch the post-Ginso cutscene to fix softlock when Sein's dialogue is auto-skipped
		ActionSequence seinAnimationSequence = sceneRoot.transform.FindChild("*objectiveSetup").FindChild("objectiveSetupTrigger").FindChild("seinSpriteAction").GetComponent<ActionSequence>();
		WaitAction waitAction = seinAnimationSequence.Actions[1] as WaitAction;
		waitAction.Duration = 5.0f;
	}

	private static void BootstrapMoonGrottoBridge(SceneRoot sceneRoot)
	{
		if (RandomizerSettings.Game.FixGrottoBridgeDrop)
		{
			// add an ActionSequenceSerializer to the bridge so that the sequence continues and activates the final colliders even after glitching it
			GameObject bridgeSequence = sceneRoot.transform.FindChild("*gumoBridgeSetup").FindChild("group").FindChild("action").gameObject;
			ActionSequenceSerializer serializer = bridgeSequence.AddComponent<ActionSequenceSerializer>();
			serializer.OnValidate();
			serializer.MoonGuid = new MoonGuid(1360931587, 1176121670, -1051255642, 855352030);
			(serializer as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		}
	}

	private static void BootstrapMountHoruHub(SceneRoot sceneRoot)
	{
		// add randomized pickup actions for each end of room cutscene
		Transform lavaDrainParent = sceneRoot.transform.FindChild("*doorSetups").FindChild("lavaDrainSetups");

		// door1LavaDrain - (L3) mountHoruBreakyPathTop
		ActionSequence doorSequence = lavaDrainParent.FindChild("*door1LavaDrains").FindChild("*door1LavaDrain").GetComponent<ActionSequence>();
		RandomizerPickupAction pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruL3");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door2LavaDrain - (R1) mountHoruStomperSystemsR
		doorSequence = lavaDrainParent.FindChild("*door2LavaDrains").FindChild("*door2LavaDrain").GetComponent<ActionSequence>();
		pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruR1");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door3LavaDrain - (R2) mountHoruProjectileCorridor
		doorSequence = lavaDrainParent.FindChild("*door3LavaDrains").FindChild("*door3LavaDrain").GetComponent<ActionSequence>();
		pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruR2");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door5LavaDrain - (R3) mountHoruMovingPlatform
		doorSequence = lavaDrainParent.FindChild("*door5LavaDrains").FindChild("*door5LavaDrain").GetComponent<ActionSequence>();
		pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruR3");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door7LavaDrain - (L2) mountHoruBigPushBlock
		doorSequence = lavaDrainParent.FindChild("*door7LavaDrains").FindChild("*door7LavaDrain").GetComponent<ActionSequence>();
		pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruL2");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door8LavaDrain - (L1) mountHoruBlockableLasers
		doorSequence = lavaDrainParent.FindChild("*door8LavaDrains").FindChild("*door8LavaDrain").GetComponent<ActionSequence>();
		pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruL1");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// special cases for L4/R4
		RandomizerPickupAction leftPickupAction = RandomizerLocationManager.AddPickupAction(lavaDrainParent.gameObject, "HoruL4", "giveLeftPickup");
		(leftPickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);

		RandomizerPickupAction rightPickupAction = RandomizerLocationManager.AddPickupAction(lavaDrainParent.gameObject, "HoruR4", "giveRightPickup");
		(rightPickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);

		// door4LavaDrain - L4/R4, whichever comes first
		doorSequence = lavaDrainParent.FindChild("*door4LavaDrains").FindChild("*door4LavaDrain").GetComponent<ActionSequence>();
		GameObject obj = new GameObject("pickupAction");
		obj.transform.parent = doorSequence.transform;

		RunActionCondition conditionPickupAction = obj.AddComponent<RunActionCondition>();
		conditionPickupAction.MoonGuid = new MoonGuid(-1261986975, 1336041250, 1663544246, -817715174);
		(conditionPickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		conditionPickupAction.Action = leftPickupAction;
		conditionPickupAction.ElseAction = rightPickupAction;
		conditionPickupAction.Condition = (doorSequence.Actions[2] as RunActionCondition).Condition;

		doorSequence.Actions.Insert(3, conditionPickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door6LavaDrain - L4/R4, whichever comes second
		doorSequence = lavaDrainParent.FindChild("*door6LavaDrains").FindChild("*door6LavaDrain").GetComponent<ActionSequence>();
		obj = new GameObject("pickupAction");
		obj.transform.parent = doorSequence.transform;

		conditionPickupAction = obj.AddComponent<RunActionCondition>();
		conditionPickupAction.MoonGuid = new MoonGuid(-300318401, 1327879929, 1536957364, -1500614911);
		(conditionPickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		conditionPickupAction.Action = rightPickupAction;
		conditionPickupAction.ElseAction = leftPickupAction;
		conditionPickupAction.Condition = (doorSequence.Actions[2] as RunActionCondition).Condition;

		doorSequence.Actions.Insert(3, conditionPickupAction);
		ActionSequence.Rename(doorSequence.Actions);
	}

	private static void BootstrapSunkenGladesSpiritWell(SceneRoot sceneRoot)
	{
		// forcibly prevent the cutscene for the spirit well, instead of basing it on whether or not you have spirit flame
		ActivateBasedOnCondition activate = sceneRoot.transform.FindChild("*activatedBySpiritFlame").GetComponent<ActivateBasedOnCondition>();
		activate.enabled = false;
		activate.Target.SetActive(false);
	}

	private static Dictionary<string, Action<SceneRoot>> s_bootstrap = new Dictionary<string, Action<SceneRoot>>
	{
		{ "moonGrottoRopeBridge", new Action<SceneRoot>(RandomizerBootstrap.BootstrapMoonGrottoBridge) },
		{ "mountHoruHubMid", new Action<SceneRoot>(RandomizerBootstrap.BootstrapMountHoruHub) },
		{ "northMangroveFallsLanternIntro", new Action<SceneRoot>(RandomizerBootstrap.BootstrapBlackrootLanternRoom) },
		{ "spiritTreeRefined", new Action<SceneRoot>(RandomizerBootstrap.BootstrapSpiritTree) },
		{ "sunkenGladesIntroSplitB", new Action<SceneRoot>(RandomizerBootstrap.BootstrapSunkenGladesSpiritWell) },
		{ "thornfeltSwampActTwoStart", new Action<SceneRoot>(RandomizerBootstrap.BootstrapThornfeltSwampMain) },
		{ "titleScreenSwallowsNest", new Action<SceneRoot>(RandomizerBootstrap.BootstrapTitleScreen) },
		{ "westGladesFireflyAreaA", new Action<SceneRoot>(RandomizerBootstrap.BootstrapValleyThreeBirdArea) }
	};

	private static List<string> s_bootstrappedScenes = new List<string>();
}
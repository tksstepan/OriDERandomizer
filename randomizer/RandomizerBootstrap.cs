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

	private static void BootstrapScene(SceneRoot sceneRoot)
	{
		if (RandomizerBootstrap.s_bootstrap.ContainsKey(sceneRoot.name))
		{
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
			((SaveSerialize)owner).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
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
		sceneRoot.transform.FindChild("mediumExpOrb").GetComponent<DestroyOnRestoreCheckpoint>().enabled = false;
	}

	private static Dictionary<string, Action<SceneRoot>> s_bootstrap = new Dictionary<string, Action<SceneRoot>>
	{
		{ "titleScreenSwallowsNest", new Action<SceneRoot>(RandomizerBootstrap.BootstrapTitleScreen) },
		{ "northMangroveFallsLanternIntro", new Action<SceneRoot>(RandomizerBootstrap.BootstrapBlackrootLanternRoom) },
		{ "spiritTreeRefined", new Action<SceneRoot>(RandomizerBootstrap.BootstrapSpiritTree) },
	};
}
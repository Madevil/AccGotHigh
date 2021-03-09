using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UniRx;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;

#if AI || HS2
using AIChara;
#endif

namespace AccGotHigh
{
	[BepInPlugin(GUID, Name, Version)]
	[BepInDependency("marco.kkapi")]
	[BepInDependency("com.deathweasel.bepinex.materialeditor", "2.5")]
	[BepInProcess(Constants.MainGameProcessName)]
#if KK
	[BepInProcess(Constants.MainGameProcessNameSteam)]
#endif
	public partial class AccGotHigh : BaseUnityPlugin
	{
		public const string Name = "AccGotHigh";
#if AI
		public const string GUID = "madevil.ai.AccGotHigh";
#elif HS2
		public const string GUID = "madevil.hs2.AccGotHigh";
#else
		public const string GUID = "madevil.kk.AccGotHigh";
#endif
		public const string Version = "1.7.0.0";

		internal static new ManualLogSource Logger;
		internal static MonoBehaviour Instance;
		internal static Harmony HooksInstance = null;
		internal static object BodyMapInstance = null;

		internal static ConfigEntry<bool> CfgEnable { get; set; }
		internal static ConfigEntry<Color> CfgColor { get; set; }
		internal static ConfigEntry<string> CfgUsingMaterialName { get; set; }
		internal static ConfigEntry<bool> CfgApplyMasking { get; set; }
		internal static SidebarToggle SidebarToggleEnable;

		internal static Dictionary<string, Material> LoadedEffectMaterials = new Dictionary<string, Material>();
		internal static List<Transform> EffectClones = new List<Transform>();
		internal static ChaControl chaCtrl;

		private void Start()
		{
			Logger = base.Logger;
			Instance = this;

			CfgEnable = Config.Bind("General", "Enable", true, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 10 }));
			CfgEnable.SettingChanged += (sender, args) =>
			{
				if (MakerAPI.InsideMaker)
				{
					if (CfgEnable.Value)
						EnableHarmonyPatch();
					else
						DisableHarmonyPatch();

					SidebarToggleEnable.SetValue(CfgEnable.Value, false);
				}
			};
			CfgColor = Config.Bind("General", "Effect Color", new Color(0, 1f, 1f, 0.2f), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 0 }));
			CfgColor.SettingChanged += (sender, args) =>
			{
				if (LoadedEffectMaterials.ContainsKey("standard"))
					LoadedEffectMaterials["standard"].SetColor("_Color", CfgColor.Value);
				if (LoadedEffectMaterials.ContainsKey("glitch"))
					LoadedEffectMaterials["glitch"].SetColor("_Color4", CfgColor.Value);
#if KK
				if (LoadedEffectMaterials.ContainsKey("bonelyfans"))
					LoadedEffectMaterials["bonelyfans"].SetColor("_Color", CfgColor.Value);
#endif
			};
#if KK
			CfgUsingMaterialName = Config.Bind("General", "Effect Use", "bonelyfans", new ConfigDescription("", new AcceptableValueList<string>("bonelyfans", "glitch", "standard", "numbers"), new ConfigurationManagerAttributes { Order = 1 }));
#else
			CfgUsingMaterialName = Config.Bind("General", "Effect Use", "bonelyfans", new ConfigDescription("", new AcceptableValueList<string>("glitch", "standard"), new ConfigurationManagerAttributes { Order = 1 }));
#endif
			CfgApplyMasking = Config.Bind("General", "Apply masking", true, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 9 }));

			MakerAPI.MakerBaseLoaded += (sender, args) =>
			{
				chaCtrl = MakerAPI.GetCharacterControl();
				Hooks.current = -1;
				if (CfgEnable.Value)
					EnableHarmonyPatch();
			};
			MakerAPI.RegisterCustomSubCategories += (sender, e) =>
			{
				SidebarToggleEnable = e.AddSidebarControl(new SidebarToggle("AccGotHigh", CfgEnable.Value, this));
				SidebarToggleEnable.ValueChanged.Subscribe(value => CfgEnable.Value = value);
			};
			MakerAPI.MakerExiting += (sender, args) =>
			{
				DisableHarmonyPatch();
				SidebarToggleEnable = null;
			};
		}

		internal static void EnableHarmonyPatch()
		{
			LoadedMaterial_standard();
			LoadedMaterial_glitch();
#if KK
			LoadedMaterial_bonelyfans();
			LoadedMaterial_numbers();
#endif
			HooksInstance = Harmony.CreateAndPatchAll(typeof(Hooks));

			BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.materialeditor", out PluginInfo PluginInfo);
			if (PluginInfo?.Instance != null)
			{
				Type MaterialAPI = PluginInfo.Instance.GetType().Assembly.GetType("MaterialEditorAPI.MaterialAPI");
				HooksInstance.Patch(MaterialAPI.GetMethod("GetRendererList", AccessTools.all, null, new[] { typeof(GameObject) }, null), prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.MaterialAPI_GetRendererList_Prefix)));
			}
		}

		internal static void DisableHarmonyPatch()
		{
			HooksInstance.UnpatchAll(HooksInstance.Id);
			HooksInstance = null;

			RemoveEffect();
		}

		internal static void RemoveEffect()
		{
			for (int i = 0; i < EffectClones.Count; i++)
				Destroy(EffectClones[i].gameObject);
			EffectClones.Clear();
		}

		internal static void AddEffect(List<GameObject> groupie)
		{
			for (int i = 0; i < groupie.Count; i++)
			{
				if (groupie[i].name.StartsWith("AccGotHigh_")) continue;

				Transform origin = groupie[i].transform;
				Transform copy = Instantiate(origin, origin.parent, false);
				copy.name = "AccGotHigh_" + origin.name;
				copy.GetComponent<Renderer>().material = LoadedEffectMaterials[CfgUsingMaterialName.Value];
				EffectClones.Add(copy);

				if (CfgApplyMasking.Value)
				{
					if (CfgUsingMaterialName.Value == "bonelyfans")
					{
						Material[] materials = origin.GetComponent<Renderer>().sharedMaterials;
						Texture mask = materials[0].GetTexture("_AlphaMask");
						copy.GetComponent<Renderer>().material.SetTexture("_AlphaMask", mask);
						Texture tex = materials[0].GetTexture("_MainTex");
						copy.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
					}
					else if (CfgUsingMaterialName.Value == "numbers")
					{
						Material[] materials = origin.GetComponent<Renderer>().sharedMaterials;
						Texture mask = materials[0].GetTexture("_AlphaMask");
						copy.GetComponent<Renderer>().material.SetTexture("_AlphaMask", mask);
					}
				}
			}
		}

		internal static void CtrlEffect(int slot, bool show)
		{
			if (slot < 0) return;
#if KK
			ChaAccessoryComponent chaAccessory = chaCtrl.GetAccessoryObject(slot)?.GetComponent<ChaAccessoryComponent>();
#else
			CmpAccessory chaAccessory = chaCtrl.GetAccessoryObject(slot)?.GetComponent<CmpAccessory>();
#endif
			if (chaAccessory == null) return;

			if (show)
				AddEffect(chaAccessory.GetComponentsInChildren<Renderer>().Select(x => x.gameObject).ToList());
			else
				RemoveEffect();
		}

		internal sealed class ConfigurationManagerAttributes
		{
			public int? Order;
		}
	}
}

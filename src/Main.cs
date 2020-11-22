using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UniRx;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;

namespace AccGotHigh
{
	[BepInPlugin(GUID, Name, Version)]
	public partial class AccGotHigh : BaseUnityPlugin
	{
		public const string Name = "AccGotHigh";
		public const string GUID = "madevil.kk.AccGotHigh";
		public const string Version = "1.1.0.0";

		internal static new ManualLogSource Logger;
		internal static MonoBehaviour Instance;
		internal static Harmony HooksInstance = null;

		internal static ConfigEntry<bool> ConfigEnable { get; set; }
		internal static ConfigEntry<int> ConfigRenderQueue { get; set; }
		internal static ConfigEntry<Color> ConfigColor { get; set; }
		internal static SidebarToggle SidebarToggleEnable;

		internal static Material bonelyfans;
		internal static List<Transform> EffectClones = new List<Transform>();
		internal static ChaControl chaCtrl;

		private void Start()
		{
			Logger = base.Logger;
			Instance = this;

			ConfigEnable = Config.Bind("General", "Enable", true);
			ConfigEnable.SettingChanged += (sender, args) =>
			{
				if (MakerAPI.InsideMaker)
				{
					if (ConfigEnable.Value)
						EnableHarmonyPatch();
					else
						DisableHarmonyPatch();
				}
			};
			ConfigRenderQueue = Config.Bind("General", "Render Queue", 5000);
			ConfigRenderQueue.SettingChanged += (sender, args) =>
			{
				bonelyfans.renderQueue = ConfigRenderQueue.Value;
			};
			ConfigColor = Config.Bind("General", "Effect Color", Color.cyan);
			ConfigColor.SettingChanged += (sender, args) =>
			{
				bonelyfans.color = ConfigColor.Value;
			};

			bonelyfans = new Material(Shader.Find("Standard"));
			bonelyfans.renderQueue = ConfigRenderQueue.Value;
			bonelyfans.color = ConfigColor.Value;

			MakerAPI.MakerBaseLoaded += (s, e) =>
			{
				if (ConfigEnable.Value)
					EnableHarmonyPatch();
				chaCtrl = MakerAPI.GetCharacterControl();
			};
			MakerAPI.RegisterCustomSubCategories += (sender, e) =>
			{
				SidebarToggleEnable = e.AddSidebarControl(new SidebarToggle("AccGotHigh", ConfigEnable.Value, this));
				SidebarToggleEnable.ValueChanged.Subscribe(value => ConfigEnable.Value = value);
			};
			MakerAPI.MakerExiting += (s, e) =>
			{
				DisableHarmonyPatch();
				chaCtrl = null;
			};
		}

		internal static void EnableHarmonyPatch()
		{
			if (HooksInstance == null)
				HooksInstance = Harmony.CreateAndPatchAll(typeof(Hooks));
		}

		internal static void DisableHarmonyPatch()
		{
			if (HooksInstance != null)
			{
				HooksInstance.UnpatchAll(HooksInstance.Id);
				HooksInstance = null;
			}
			RemoveEffect();
		}

		internal static void AddEffect(List<GameObject> groupie)
		{
			for (int i = 0; i < groupie.Count; i++)
			{
				if (groupie[i].name.StartsWith("AccGotHigh_")) continue;
				if (GameObject.Find("AccGotHigh_" + groupie[i].gameObject.name) != null) continue;

				Transform origin = groupie[i].transform;
				Transform copy = Instantiate(origin, origin.parent, false);
				copy.name = "AccGotHigh_" + origin.name;
				copy.GetComponent<Renderer>().material = bonelyfans;
				EffectClones.Add(copy);
			}
		}

		internal static void RemoveEffect()
		{
			for (int i = 0; i < EffectClones.Count; i++)
				Destroy(EffectClones[i].gameObject);
			EffectClones.Clear();
		}

		internal static void CtrlEffect(int slot, bool show)
		{
			if (slot < 0) return;

			ChaAccessoryComponent chaAccessory = chaCtrl.GetAccessory(slot);
			if (chaAccessory == null) return;

			if (show)
			{
				AddEffect(chaAccessory.GetComponentsInChildren<MeshRenderer>().Select(x => x.gameObject).ToList());
				AddEffect(chaAccessory.GetComponentsInChildren<SkinnedMeshRenderer>().Select(x => x.gameObject).ToList());
			}
			else
				RemoveEffect();
		}
	}
}

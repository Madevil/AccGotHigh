using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Utilities;

namespace AccGotHigh
{
	[BepInPlugin(GUID, Name, Version)]
	public partial class AccGotHigh : BaseUnityPlugin
	{
		public const string Name = "AccGotHigh";
		public const string GUID = "madevil.kk.AccGotHigh";
		public const string Version = "1.0.0.0";

		internal static new ManualLogSource Logger;
		internal static MonoBehaviour Instance;
		internal static Harmony HooksInstance;

		internal static Material bonelyfans;
		internal static List<Transform> Copies = new List<Transform>();
		internal static ChaControl chaCtrl;

		private void Start()
		{
			Logger = base.Logger;
			Instance = this;
			bonelyfans = LoadShader();

			MakerAPI.MakerBaseLoaded += (s, e) =>
			{
				HooksInstance = Harmony.CreateAndPatchAll(typeof(Hooks));
				chaCtrl = MakerAPI.GetCharacterControl();
			};

			MakerAPI.MakerExiting += (s, e) =>
			{
				HooksInstance.UnpatchAll(HooksInstance.Id);
				HooksInstance = null;
				chaCtrl = null;
			};
		}

		private static Material LoadShader()
		{
			AssetBundle ab = null;
			try
			{
				var res = ResourceUtils.GetEmbeddedResource("bonelyfans.unity3d") ?? throw new ArgumentNullException("GetEmbeddedResource");
				ab = AssetBundle.LoadFromMemory(res) ?? throw new ArgumentNullException("LoadFromMemory");
				var sha = ab.LoadAsset<Shader>("assets/bonelyfans 1.shader") ?? throw new ArgumentNullException("LoadAsset");
				ab.Unload(false);
				return new Material(sha);
			}
			catch (Exception)
			{
				if (ab != null) ab.Unload(true);
				throw;
			}
		}

		internal static void AddEffect(List<GameObject> lalala)
		{
			for (int i = 0; i < lalala.Count; i++)
			{
				if (lalala[i].name.StartsWith("AccGotHigh_"))
					continue;

				if (GameObject.Find("AccGotHigh_" + lalala[i].gameObject.name) != null)
					continue;

				Transform origin = lalala[i].transform;
				Transform copy = Instantiate(origin, origin.parent, false);
				copy.name = "AccGotHigh_" + origin.name;
				copy.GetComponent<Renderer>().material = bonelyfans;
				Copies.Add(copy);
			}
		}

		internal static void CtrlEffect(int slot, bool show)
		{
			if (slot > -1)
			{
				ChaAccessoryComponent chaAccessory = chaCtrl.GetAccessory(slot);
				if (chaAccessory != null)
				{
					if (show)
					{
						AddEffect(chaAccessory.GetComponentsInChildren<MeshRenderer>().Select(x => x.gameObject).ToList());
						AddEffect(chaAccessory.GetComponentsInChildren<SkinnedMeshRenderer>().Select(x => x.gameObject).ToList());
					}
					else
					{
						for (int i = 0; i < Copies.Count; i++)
							Destroy(Copies[i].gameObject);
						Copies.Clear();
					}
				}
			}
		}
	}
}

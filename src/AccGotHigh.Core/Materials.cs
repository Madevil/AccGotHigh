using System;
using System.Linq;

using UnityEngine;
using UniRx;

using KKAPI.Utilities;

namespace AccGotHigh
{
	public partial class AccGotHigh
	{
		internal static void LoadedMaterial_standard()
		{
			if (LoadedEffectMaterials.ContainsKey("standard")) return;
			LoadedEffectMaterials["standard"] = new Material(Shader.Find("Standard"));
			LoadedEffectMaterials["standard"].SetColor("_Color", CfgColor.Value);
			LoadedEffectMaterials["standard"].renderQueue = 9999;
		}

		internal static void LoadedMaterial_bonelyfans()
		{
			if (LoadedEffectMaterials.ContainsKey("bonelyfans")) return;

			AssetBundle ab = null;
			try
			{
				byte[] res = ResourceUtils.GetEmbeddedResource("bonelyfans.unity3d") ?? throw new ArgumentNullException("GetEmbeddedResource");
				ab = AssetBundle.LoadFromMemory(res) ?? throw new ArgumentNullException("LoadFromMemory");
				string assetName = ab.GetAllAssetNames().First(x => x.Contains("bonelyfans"));
				Logger.LogWarning($"assetName: {assetName}");
				Shader sha = ab.LoadAsset<Shader>(assetName) ?? throw new ArgumentNullException("LoadAsset");
				ab.Unload(false);

				LoadedEffectMaterials["bonelyfans"] = new Material(sha);
				LoadedEffectMaterials["bonelyfans"].SetColor("_Color", CfgColor.Value);
				LoadedEffectMaterials["bonelyfans"].SetInt("_UseMaterialColor", 1);
			}
			catch (Exception)
			{
				if (ab != null) ab.Unload(true);
				throw;
			}
		}

		internal static void LoadedMaterial_numbers()
		{
			if (LoadedEffectMaterials.ContainsKey("numbers")) return;

			AssetBundle ab = null;
			try
			{
				byte[] res = ResourceUtils.GetEmbeddedResource("numbers.unity3d") ?? throw new ArgumentNullException("GetEmbeddedResource");
				ab = AssetBundle.LoadFromMemory(res) ?? throw new ArgumentNullException("LoadFromMemory");
				string assetName = ab.GetAllAssetNames().First(x => x.Contains("numbers"));
				Logger.LogWarning($"assetName: {assetName}");
				LoadedEffectMaterials["numbers"] = ab.LoadAsset<Material>(assetName) ?? throw new ArgumentNullException("LoadAsset");
				ab.Unload(false);

				LoadedEffectMaterials["numbers"].renderQueue = 9999;
			}
			catch (Exception)
			{
				if (ab != null) ab.Unload(true);
				throw;
			}
		}

		internal static void LoadedMaterial_glitch()
		{
			if (LoadedEffectMaterials.ContainsKey("glitch")) return;

			AssetBundle ab = null;
			try
			{
				byte[] res = ResourceUtils.GetEmbeddedResource("glitch.unity3d") ?? throw new ArgumentNullException("GetEmbeddedResource");
				ab = AssetBundle.LoadFromMemory(res) ?? throw new ArgumentNullException("LoadFromMemory");
				string assetName = ab.GetAllAssetNames().First(x => x.Contains("glitch"));
				Logger.LogWarning($"assetName: {assetName}");
				LoadedEffectMaterials["glitch"] = ab.LoadAsset<Material>(assetName) ?? throw new ArgumentNullException("LoadAsset");
				ab.Unload(false);

				LoadedEffectMaterials["glitch"].SetColor("_Color4", CfgColor.Value);
				/*
				bonelyfans.SetFloat("_BlockAmount", 0.125f);
				bonelyfans.SetFloat("_BlockSize", 32f);
				bonelyfans.SetFloat("_BlockSpeed", 2f);
				*/
				LoadedEffectMaterials["glitch"].SetFloat("_FlickerAmount", 0.125f);
				LoadedEffectMaterials["glitch"].SetFloat("_FlickerSpeed", 20f);
				/*
				bonelyfans.SetFloat("_GlitchTime", 1f);
				bonelyfans.SetFloat("_GlitchWidth", 0.2f);
				bonelyfans.SetColor("_GlitchRun", new Color(0, 1f, 0, 0));
				bonelyfans.SetColor("_GlitchDirection", new Color(0, 1f, 0, 0));
				*/
			}
			catch (Exception)
			{
				if (ab != null) ab.Unload(true);
				throw;
			}
		}
	}
}

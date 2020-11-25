using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using HarmonyLib;

using KK_Plugins.MaterialEditor;

namespace AccGotHigh
{
	public partial class AccGotHigh
	{
		internal class Hooks
		{
			internal static int current = -1;

			[HarmonyPostfix, HarmonyPatch(typeof(Selectable), nameof(Selectable.OnPointerEnter))]
			private static void Selectable_OnPointerEnter(Selectable __instance, PointerEventData eventData)
			{
#if KK
				if (__instance is Toggle && eventData.pointerEnter.name == "imgOff" && eventData.pointerEnter.transform.parent.name.StartsWith("tglSlot"))
#else
				if (__instance is UI_ButtonEx && eventData.pointerEnter.name.StartsWith("Slot"))
#endif
				{
#if KK
					int slot = int.Parse(eventData.pointerEnter.transform.parent.name.Replace("tglSlot", "")) - 1;
#else
					int slot = -1;
					Transform self = eventData.pointerEnter.transform;
					Transform parent = self.parent;
					for (int i = 0; i < parent.childCount - 1; i++)
					{
						if (parent.GetChild(i) == self)
							slot = i;
					}
					if (slot == -1)
						return;
#endif
					if (current == slot)
						return;
					if (current > -1)
						CtrlEffect(current, false);
					current = slot;
					CtrlEffect(current, true);
				}
			}

			[HarmonyPostfix, HarmonyPatch(typeof(Selectable), nameof(Selectable.OnPointerExit))]
			private static void Selectable_OnPointerExit(Selectable __instance, PointerEventData eventData)
			{
				if (current == -1)
					return;
				CtrlEffect(current, false);
				current = -1;
			}

			[HarmonyPrefix, HarmonyPatch(typeof(MaterialAPI), nameof(MaterialAPI.GetRendererList))]
			private static bool MaterialAPI_GetRendererList(ref List<Renderer> __result, GameObject gameObject)
			{
				if (gameObject == null)
					return true;

				List<Renderer> rendList = gameObject.GetComponentsInChildren<Renderer>(true).ToList();

				if (!rendList.Any(x => x.name.StartsWith("AccGotHigh_")))
					return true;

				rendList.RemoveAll(x => x.name.StartsWith("AccGotHigh_"));
				__result = rendList;
				return false;
			}
		}
	}
}

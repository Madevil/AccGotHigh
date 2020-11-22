using UnityEngine.EventSystems;
using UnityEngine.UI;

using HarmonyLib;

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
				if (__instance is Toggle && eventData.pointerEnter.name == "imgOff" && eventData.pointerEnter.transform.parent.name.StartsWith("tglSlot"))
				{
					int slot = int.Parse(eventData.pointerEnter.transform.parent.name.Replace("tglSlot", "")) - 1;
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
		}
	}
}

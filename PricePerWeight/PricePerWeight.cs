using HarmonyLib;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using System.Collections.Generic;
using System;
using static Kingmaker.UI.Common.ItemsFilter;

namespace OmegaTweaks.PricePerWeight
{
    [HarmonyPatch(typeof(ItemsFilter))]
    static class PricePerWeight
    {
        [HarmonyPostfix]
        [HarmonyPatch("ItemSorter")]
        public static void ItemSorterPostfix(ItemsFilter.SorterType type, List<ItemEntity> items, ItemsFilter.FilterType filter, ref List<ItemEntity> __result)
        {
            if (OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.NOT_USE_THIS || type == ItemsFilter.SorterType.NotSorted)
            {
                return;
            }

            if ((OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.Type && type == SorterType.TypeUp) ||
                (OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.Price && type == SorterType.PriceUp) ||
                (OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.Name && type == SorterType.NameUp) ||
                (OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.Date && type == SorterType.DateUp) ||
                (OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.Weight && type == SorterType.WeightUp))
            {
                items.Sort((ItemEntity a, ItemEntity b) => CompareByPricePerWeight(a, b, filter));
                __result = items;
            }
            else
            if ((OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.Type && type == SorterType.TypeDown) ||
                (OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.Price && type == SorterType.PriceDown) ||
                (OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.Name && type == SorterType.NameDown) ||
                (OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.Date && type == SorterType.DateDown) ||
                (OmegaTweaksModMain.settings.PricePerWeight == Settings.PricePerWeightOverrideTarget.Weight && type == SorterType.WeightDown))
            {
                items.Sort((ItemEntity a, ItemEntity b) => CompareByPricePerWeight(a, b, filter));
                items.Reverse();
                __result = items;
            }
        }

        private static int CompareByPricePerWeight(ItemEntity a, ItemEntity b, ItemsFilter.FilterType filter)
        {
            int other1 = ItemsFilter.GetItemType(a, filter).CompareTo(ItemsFilter.ItemType.Other);
            int other2 = ItemsFilter.GetItemType(b, filter).CompareTo(ItemsFilter.ItemType.Other);
            if (other1 != -1 || other2 != -1)
            {
                return ItemsFilter.GetItemType(a, filter).CompareTo(ItemsFilter.GetItemType(b, filter));
            }

            float per1 = a.Blueprint.Cost == 0 ? 0f : a.Blueprint.Weight / a.Blueprint.Cost;
            float per2 = b.Blueprint.Cost == 0 ? 0f : b.Blueprint.Weight / b.Blueprint.Cost;
            int per3 = per1.CompareTo(per2) * -1;

            if (per3 == 0)
            {
                per3 = string.Compare(a.Name, b.Name);
            }

            return per3;
        }
    }
}

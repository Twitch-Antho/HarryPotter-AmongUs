using HarmonyLib;

namespace HarryPotter.Classes.Hats.Patches;

[HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.CheckUnlockedItems))]
public class InventoryManager_Patches
{
    public static void Prefix()
    {
        HatLoader.LoadHatsRoutine();
    }
}
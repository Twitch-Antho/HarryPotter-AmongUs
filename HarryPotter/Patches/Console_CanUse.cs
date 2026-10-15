using HarmonyLib;
using HarryPotter.Classes;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
public class Console_CanUse
{
    private static void Postfix(NetworkedPlayerInfo __0, ref bool __1, ref bool __2)
    {
        var moddedPlayer = Main.Instance.ModdedPlayerById(__0.PlayerId);

        if (moddedPlayer == null)
            return;

        if (moddedPlayer.CanUseConsoles)
            return;

        __1 = false;
        __2 = false;
    }
}
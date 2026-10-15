using HarmonyLib;
using HarryPotter.Classes;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public class PlayerControl_FixedUpdate
{
    private static void Postfix(PlayerControl __instance)
    {
        if (__instance.AmOwner)
        {
            foreach (var wItem in Main.Instance.AllItems)
            {
                wItem.DrawWorldIcon();
                wItem.Update();
            }

            Main.Instance.AllItems.RemoveAll(x => x.IsPickedUp);
        }
    }
}
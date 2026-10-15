using HarmonyLib;
using HarryPotter.Classes;
using System.Linq;

namespace HarryPotter.Patches
{
    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.MoveTowardNextPoint))]
    class CustomNetworkTransform_MoveTowardNextPoint_Patch
    {
        static void Prefix(CustomNetworkTransform __instance)
        {
            var targetPlayer = Main.Instance.AllPlayers
                .FirstOrDefault(x =>
                    x._Object.NetTransform == __instance &&
                    x.ControllerOverride == Main.Instance.GetLocalModdedPlayer());

            if (targetPlayer != null)
            {
                // 保存原始修正值并应用速度系数
                __instance.rubberbandModifier *= targetPlayer.SpeedMultiplier;
            }
        }

        static void Postfix(CustomNetworkTransform __instance)
        {
            var targetPlayer = Main.Instance.AllPlayers
                .FirstOrDefault(x =>
                    x._Object.NetTransform == __instance &&
                    x.ControllerOverride == Main.Instance.GetLocalModdedPlayer());

            if (targetPlayer != null)
            {
                // 还原原始修正值
                __instance.rubberbandModifier /= targetPlayer.SpeedMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.ShouldExtendCurrentFrame))]
    class CustomNetworkTransform_ShouldExtendCurrentFrame_Patch
    {
        static void Postfix(CustomNetworkTransform __instance, ref bool __result)
        {
            var isControlled = Main.Instance.AllPlayers
                .Any(x =>
                    x._Object.NetTransform == __instance &&
                    x.ControllerOverride == Main.Instance.GetLocalModdedPlayer());

            if (isControlled)
            {
                __result = false; // 禁用位置跳跃
            }
        }
    }
}
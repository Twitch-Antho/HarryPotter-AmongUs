using HarmonyLib;
using HarryPotter.Classes;
using UnityEngine;

namespace HarryPotter.Patches
{
    [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
    public static class PassiveButton_ReceiveClickDown
    {
        public static void Postfix(PassiveButton __instance)
        {
            if (__instance == null)
                return;

            var local = Main.Instance.GetLocalModdedPlayer();
            if (local == null)
                return;

            // Sécurise le nom pour IL2CPP
            string btnName = __instance?.gameObject?.name;
            if (btnName == null)
                return;

            // Récupération sécurisée du PlayerVoteArea
            var voteArea = __instance.transform.GetComponentInParent<PlayerVoteArea>();
            if (voteArea == null)
                return;

            byte targetId = voteArea.TargetPlayerId;

            // SNITCH BUTTON
            if (btnName.Equals("SnitchButton") && local.HasItem(3))
            {
                Main.Instance.RpcForceAllVotes(targetId);
                return;
            }

            // SORT BUTTON
            if (btnName.Equals("SortButton") && local.HasItem(8))
            {
                Main.Instance.RpcRevealRole(targetId);
                return;
            }
        }
    }
}

using HarmonyLib;
using HarryPotter.Classes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public class PlayerControl_Exiled
{
    private static bool Prefix(PlayerControl __instance)
    {
        __instance.Visible = false;
        if (__instance.AmOwner)
        {
            if (Main.Instance.ModdedPlayerById(__instance.PlayerId).ShouldRevive)
            {
                Main.Instance.RpcFakeKill(__instance);
            }
            else
            {
                Main.Instance.PlayerDie(__instance);

                var instance = ExileController.Instance;
                var timesEjected = instance.Duration;
                instance.Duration = timesEjected + 1U;
                DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
                var importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
                importantTextTask.transform.SetParent(__instance.transform, false);
                if (__instance.Data.Role.IsImpostor)
                {
                    __instance.ClearTasks();
                    importantTextTask.Text =
                        DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostImpostor,
                            new Il2CppReferenceArray<Object>(0));
                }
                else if (!GameOptionsManager.Instance.currentNormalGameOptions.GhostsDoTasks)
                {
                    __instance.ClearTasks();
                    importantTextTask.Text =
                        DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostIgnoreTasks,
                            new Il2CppReferenceArray<Object>(0));
                }
                else
                {
                    importantTextTask.Text =
                        DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostDoTasks,
                            new Il2CppReferenceArray<Object>(0));
                }

                __instance.myTasks.Insert(0, importantTextTask);

                var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.FinallyDie);
                writer.Write(__instance.PlayerId);
                writer.EndMessage();
            }
        }

        return false;
    }
}
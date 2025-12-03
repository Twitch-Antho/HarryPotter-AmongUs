using HarmonyLib;
using HarryPotter.Classes;
using Hazel;
using Il2CppSystem;
using UnhollowerBaseLib;
using UnityEngine;
using Object = Il2CppSystem.Object;
using System.Collections;

namespace HarryPotter.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public class PlayerControl_Exiled
    {
        static bool Prefix(PlayerControl __instance)
        {
            if (__instance == null) return true;

            __instance.Visible = false;

            if (!__instance.AmOwner) return false;

            var modPlayer = Main.Instance.ModdedPlayerById(__instance.PlayerId);
            if (modPlayer == null) return true;

            if (modPlayer.ShouldRevive)
            {
                // Démarre la routine de résurrection
                Main.Instance.StartCoroutine(RevivePlayerRoutine(__instance, modPlayer));
            }
            else
            {
                HandlePermanentDeath(__instance);
            }

            return false; // Bloque l'Exiled natif
        }

        private static IEnumerator RevivePlayerRoutine(PlayerControl player, ModdedPlayerClass modPlayer)
        {
            float reviveDelay = 3f; // délai avant de revenir
            float elapsed = 0f;

            // Effet visuel simple: clignotement du joueur
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color original = sr.color;
                while (elapsed < reviveDelay)
                {
                    sr.color = Color.clear;
                    yield return new WaitForSeconds(0.3f);
                    sr.color = original;
                    yield return new WaitForSeconds(0.3f);
                    elapsed += 0.6f;
                }
            }
            else
            {
                yield return new WaitForSeconds(reviveDelay);
            }

            // Réactive le joueur
            player.Visible = true;
            modPlayer.ShouldRevive = false;

            // Reset des timers et rôles si nécessaire
            player.Revive();
            player.SetKillTimer(PlayerControl.GameOptions.KillCooldown);

            Main.Instance.RpcRevivePlayer(player); // RPC pour les autres clients
        }

        private static void HandlePermanentDeath(PlayerControl __instance)
        {
            Main.Instance.PlayerDie(__instance);

            var stats = StatsManager.Instance;
            if (stats != null)
                stats.TimesEjected += 1;

            var hud = DestroyableSingleton<HudManager>.Instance;
            if (hud != null)
                hud.ShadowQuad.gameObject.SetActive(false);

            // Important text
            var importantGO = new GameObject($"_Player_{__instance.PlayerId}");
            var importantTextTask = importantGO.AddComponent<ImportantTextTask>();
            importantTextTask.transform.SetParent(__instance.transform, false);

            var translation = DestroyableSingleton<TranslationController>.Instance;
            if (translation != null)
            {
                if (__instance.Data.IsImpostor)
                {
                    __instance.ClearTasks();
                    importantTextTask.Text = translation.GetString(StringNames.GhostImpostor, new Il2CppReferenceArray<Object>(0));
                }
                else if (!PlayerControl.GameOptions.GhostsDoTasks)
                {
                    __instance.ClearTasks();
                    importantTextTask.Text = translation.GetString(StringNames.GhostIgnoreTasks, new Il2CppReferenceArray<Object>(0));
                }
                else
                {
                    importantTextTask.Text = translation.GetString(StringNames.GhostDoTasks, new Il2CppReferenceArray<Object>(0));
                }
            }

            __instance.myTasks.Insert(0, importantTextTask);

            // RPC kill
            var localPlayer = PlayerControl.LocalPlayer;
            if (AmongUsClient.Instance != null && localPlayer != null)
            {
                var writer = AmongUsClient.Instance.StartRpc(localPlayer.NetId, (byte)Packets.FinallyDie, SendOption.Reliable);
                writer.Write(__instance.PlayerId);
                writer.EndMessage();
            }
        }
    }
}

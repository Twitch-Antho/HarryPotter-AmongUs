using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using HarryPotter.Classes;
using HarryPotter.Classes.Roles;
using hunterlib.Classes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace HarryPotter.Patches
{
    // Nouveau patch : assignation des rôles custom
    // Among Us 2025 utilise maintenant RoleManager.SelectRoles
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static class RoleManager_SelectRoles
    {
        public static void Postfix(RoleManager __instance)
        {
            if (!Main.Instance.Config.SelectRoles)
                return;

            // Listes des joueurs
            var allPlayers = PlayerControl.AllPlayerControls.ToList();

            // Séparer les imposteurs / crew
            List<PlayerControl> imps = allPlayers.Where(p => p.Data.Role.IsImpostor).ToList();
            List<PlayerControl> crew = allPlayers.Where(p => !p.Data.Role.IsImpostor).ToList();

            // Listes de rôles custom disponibles
            List<string> impRoles = new() { "Voldemort", "Bellatrix" };
            List<string> crewRoles = new() { "Harry", "Hermione", "Ron" };

            // 1. | Assignation forcée (demandée par les joueurs)
            foreach (var tuple in Main.Instance.PlayersWithRequestedRoles)
            {
                var player = tuple.Item1;
                var requested = tuple.Item2;

                if (player.Data.Role.IsImpostor && impRoles.Contains(requested))
                {
                    AssignCustomRole(player, requested);
                    impRoles.Remove(requested);
                    imps.Remove(player);
                }
                else if (!player.Data.Role.IsImpostor && crewRoles.Contains(requested))
                {
                    AssignCustomRole(player, requested);
                    crewRoles.Remove(requested);
                    crew.Remove(player);
                }
            }

            // 2. | Compléter les imposteurs restants
            while (impRoles.Count > 0 && imps.Count > 0)
            {
                var target = imps.Random();
                string role = impRoles.Random();
                AssignCustomRole(target, role);

                impRoles.Remove(role);
                imps.Remove(target);
            }

            // 3. | Compléter les crewmates restants
            while (crewRoles.Count > 0 && crew.Count > 0)
            {
                var target = crew.Random();
                string role = crewRoles.Random();
                AssignCustomRole(target, role);

                crewRoles.Remove(role);
                crew.Remove(target);
            }

            Main.Instance.PlayersWithRequestedRoles.Clear();
        }

        // Petite fonction pour rendre ça propre
        private static void AssignCustomRole(PlayerControl player, string role)
        {
            var modded = Main.Instance.ModdedPlayerById(player.PlayerId);

            switch (role)
            {
                case "Voldemort":
                    Main.Instance.RpcAssignRole(modded, new Voldemort(modded));
                    break;

                case "Bellatrix":
                    Main.Instance.RpcAssignRole(modded, new Bellatrix(modded));
                    break;

                case "Harry":
                    Main.Instance.RpcAssignRole(modded, new Harry(modded));
                    break;

                case "Hermione":
                    Main.Instance.RpcAssignRole(modded, new Hermione(modded));
                    break;

                case "Ron":
                    Main.Instance.RpcAssignRole(modded, new Ron(modded));
                    break;
            }
        }
    }
}

using HarmonyLib;
using HarryPotter.Classes;

namespace HarryPotter.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControl_FixedUpdate
    {
        public static void Postfix(PlayerControl __instance)
        {
            // Ne jamais exécuter côté non-owner
            if (!__instance.AmOwner) 
                return;

            // Copie locale pour éviter les problèmes IL2CPP "collection modified"
            var itemsSnapshot = Main.Instance.AllItems.ToList();

            foreach (var wItem in itemsSnapshot)
            {
                if (wItem == null) 
                    continue;

                try
                {
                    // Update logique (OK)
                    wItem.Update();

                    // Dessin visuel : mieux ici que dans Update du monobehaviour
                    wItem.DrawWorldIcon();

                    // Suppression différée
                    if (wItem.IsPickedUp)
                        Main.Instance.MarkForRemoval.Add(wItem);
                }
                catch { }
            }

            // Suppression regroupée (évite les crashs IL2CPP)
            if (Main.Instance.MarkForRemoval.Count > 0)
            {
                foreach (var w in Main.Instance.MarkForRemoval)
                    Main.Instance.AllItems.Remove(w);

                Main.Instance.MarkForRemoval.Clear();
            }
        }
    }
}

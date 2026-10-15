using System;
using System.Collections;
using InnerNet;
using Reactor.Utilities;
using UnityEngine;

namespace HarryPotter.Classes.Items;

public class MaraudersMap : Item
{
    public MaraudersMap(ModdedPlayerClass owner)
    {
        Owner = owner;
        ParentInventory = owner.Inventory;
        Id = 1;
        Icon = Main.Instance.Assets.ItemIcons[Id];
        Name = "Marauder's Map";
        Tooltip = string.Format(ModTranslation.getString("MaraudersMapTooltip"), Main.Instance.Config.MapDuration);
    }

    public override void Use()
    {
        Delete();
        Coroutines.Start(ZoomOut());
    }

    public IEnumerator ZoomOut()
    {
        var now = DateTime.UtcNow;
        Camera.main.orthographicSize *= 4f;

        var oldActive = HudManager.Instance.ShadowQuad.gameObject.active;
        var oldActiveKill = HudManager.Instance.KillButton.gameObject.active;
        var oldActiveUse = HudManager.Instance.UseButton.gameObject.active;
        var oldActiveReport = HudManager.Instance.ReportButton.gameObject.active;
        var oldUseConsoles = Owner.CanUseConsoles;
        HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
        HudManager.Instance.KillButton.gameObject.SetActive(false);
        HudManager.Instance.UseButton.gameObject.SetActive(false);
        HudManager.Instance.ReportButton.gameObject.SetActive(false);
        Owner.CanUseConsoles = false;

        while (true)
        {
            if (Minigame.Instance)
                Minigame.Instance.Close();

            if ((now.AddSeconds(Main.Instance.Config.MapDuration) - DateTime.UtcNow).TotalMilliseconds < 0)
                break;

            if (MeetingHud.Instance)
            {
                oldActiveKill = false;
                oldActiveReport = false;
                oldActiveUse = false;
                break;
            }

            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                break;

            yield return null;
        }

        Camera.main.orthographicSize /= 4f;

        HudManager.Instance.ShadowQuad.gameObject.SetActive(oldActive);
        HudManager.Instance.KillButton.gameObject.SetActive(oldActiveKill);
        HudManager.Instance.UseButton.gameObject.SetActive(oldActiveUse);
        HudManager.Instance.ReportButton.gameObject.SetActive(oldActiveReport);
        Owner.CanUseConsoles = oldUseConsoles;
    }
}
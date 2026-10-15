using System.Linq;
using HarryPotter.Classes.Roles;
using InnerNet;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace HarryPotter.Classes;

public class MindControlMenu
{
    public ChatController MenuObject { get; set; }

    public void OpenMenu()
    {
        if (MenuObject == null)
        {
            HudManager.Instance.Chat.SetVisible(false);
            MenuObject = Object.Instantiate(HudManager.Instance.Chat);

            MenuObject.transform.SetParent(Camera.main.transform);
            MenuObject.SetVisible(true);
            MenuObject.Toggle();

            var aspect = MenuObject.gameObject.AddComponent<AspectPosition>();
            aspect.Alignment = AspectPosition.EdgeAlignments.Center;
            aspect.AdjustPosition();

            MenuObject.GetPooledBubble().enabled = false;
            MenuObject.GetPooledBubble().gameObject.SetActive(false);

            MenuObject.freeChatField.enabled = false;
            MenuObject.freeChatField.gameObject.SetActive(false);

            MenuObject.banButton.MenuButton.enabled = false;
            MenuObject.banButton.MenuButton.gameObject.SetActive(false);

            MenuObject.freeChatField.charCountText.enabled = false;
            MenuObject.freeChatField.charCountText.gameObject.SetActive(false);

            MenuObject.openKeyboardButton.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled =
                false;
            MenuObject.openKeyboardButton.SetActive(false);

            MenuObject.backgroundImage.enabled = false;

            foreach (var rend in MenuObject.chatScreen
                         .GetComponentsInChildren<SpriteRenderer>())
                if (rend.name == "SendButton" || rend.name == "QuickChatButton")
                {
                    rend.enabled = false;
                    rend.gameObject.SetActive(false);
                }

            foreach (var bubble in MenuObject.chatBubblePool.activeChildren)
            {
                bubble.enabled = false;
                bubble.gameObject.SetActive(false);
            }

            MenuObject.chatBubblePool.activeChildren.Clear();

            foreach (var player in PlayerControl.AllPlayerControls.ToArray().Where(
                         x => /* x.PlayerId == ((Bellatrix)Main.Instance.GetLocalModdedPlayer().Role).MindControlledPlayer._Object.PlayerId &&*/
                             x != null && x != PlayerControl.LocalPlayer && !x.Data.Disconnected))
                if (!player.Data.IsDead)
                    MenuObject.AddChat(player, "MindControlMenuClickText".Translate());
                else
                    foreach (var body in Object.FindObjectsOfType<DeadBody>())
                        if (body.ParentId == player.PlayerId)
                        {
                            player.Data.IsDead = false;
                            MenuObject.AddChat(player, "MindControlMenuClickText".Translate());
                            player.Data.IsDead = true;
                        }
        }
    }

    public void CloseMenu()
    {
        MenuObject?.ForceClosed();
        MenuObject?.gameObject.Destroy();
        MenuObject = null;
    }

    public void ToggleMenu()
    {
        if (MenuObject != null && MenuObject.IsOpenOrOpening)
            CloseMenu();
        else
            OpenMenu();
    }

    public void ClickPlayer(PlayerControl target)
    {
        if (((Bellatrix)Main.Instance.GetLocalModdedPlayer().Role).MindControlledPlayer != null)
            return;

        if (((Bellatrix)Main.Instance.GetLocalModdedPlayer().Role).MindControlButton.isCoolingDown)
            return;

        if (Main.Instance.ModdedPlayerById(target.PlayerId).Immortal)
            return;

        if (target.Data.IsDead)
            return;

        if (Main.Instance.GetLocalModdedPlayer()._Object.Data.IsDead)
            return;

        if (Main.Instance.GetLocalModdedPlayer()._Object.inVent)
            return;

        CloseMenu();
        Main.Instance.RpcControlPlayer(PlayerControl.LocalPlayer, target);
    }

    public void Update()
    {
        if (MenuObject == null || !MenuObject.IsOpenOrOpening || MeetingHud.Instance ||
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            CloseMenu();
            return;
        }

        if (Minigame.Instance)
            Minigame.Instance.Close();

        foreach (var bubble in MenuObject.chatBubblePool.activeChildren)
        {
            Vector2 ScreenMin = Camera.main.WorldToScreenPoint(bubble.Cast<ChatBubble>().Background.bounds.min);
            Vector2 ScreenMax = Camera.main.WorldToScreenPoint(bubble.Cast<ChatBubble>().Background.bounds.max);

            if (Input.mousePosition.x < ScreenMin.x || Input.mousePosition.x > ScreenMax.x)
                continue;

            if (Input.mousePosition.y < ScreenMin.y || Input.mousePosition.y > ScreenMax.y)
                continue;

            if (Input.GetMouseButtonUp(0))
            {
                ClickPlayer(PlayerControl.AllPlayerControls.ToArray()
                    .Where(x => x.Data.PlayerName == bubble.Cast<ChatBubble>().NameText.text).FirstOrDefault());
                break;
            }
        }
    }
}
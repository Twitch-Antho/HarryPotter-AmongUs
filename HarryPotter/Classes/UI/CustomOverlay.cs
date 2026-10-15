using System;
using HarmonyLib;
using InnerNet;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HarryPotter.Classes.UI;

[Harmony]
public class CustomOverlay
{
    public static bool overlayShown;
    private static SpriteRenderer roleUnderlay;
    private static Sprite colorBG;
    private static SpriteRenderer meetingUnderlay;
    private static SpriteRenderer infoUnderlay;

    private static SpriteRenderer DelumWorldIcon;
    private static SpriteRenderer MapWorldIcon;
    private static SpriteRenderer KeyWorldIcon;
    private static SpriteRenderer SnitchWorldIcon;
    private static SpriteRenderer GhostStoneWorldIcon;
    private static SpriteRenderer BeerWorldIcon;
    private static SpriteRenderer ElderWandWorldIcon;
    private static SpriteRenderer BasWorldIcon;
    private static SpriteRenderer SortingHatWorldIcon;
    private static SpriteRenderer PhiloStoneWorldIcon;

    private static TextMeshPro DelumWorldText;
    private static TextMeshPro MapWorldText;
    private static TextMeshPro KeyWorldText;
    private static TextMeshPro SnitchWorldText;
    private static TextMeshPro GhostStoneWorldText;
    private static TextMeshPro BeerWorldText;
    private static TextMeshPro ElderWandWorldText;
    private static TextMeshPro BasWorldText;
    private static TextMeshPro SortingHatWorldText;
    private static TextMeshPro PhiloStoneWorldText;


    public static bool initializeOverlays()
    {
        var hudManager = DestroyableSingleton<HudManager>.Instance;
        if (hudManager == null) return false;

        if (meetingUnderlay == null)
        {
            meetingUnderlay = Object.Instantiate(hudManager.FullScreen, hudManager.transform);
            meetingUnderlay.transform.localPosition = new Vector3(0f, 0f, 20f);
            meetingUnderlay.gameObject.SetActive(true);
            meetingUnderlay.enabled = false;
        }

        if (colorBG == null) colorBG = Main.Instance.Assets.colorBG;

        if (infoUnderlay == null)
        {
            infoUnderlay = Object.Instantiate(meetingUnderlay, hudManager.transform);
            infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
            infoUnderlay.gameObject.SetActive(true);
            infoUnderlay.enabled = false;
        }

        return true;
    }

    public static void hideBlackBG()
    {
        if (meetingUnderlay == null) return;
        meetingUnderlay.enabled = false;
    }

    public static void showInfoOverlay()
    {
        if (overlayShown) return;

        var hudManager = DestroyableSingleton<HudManager>.Instance;
        if (ShipStatus.Instance == null || PlayerControl.LocalPlayer == null || hudManager == null ||
            DestroyableSingleton<HudManager>.Instance.IsIntroDisplayed ||
            (!PlayerControl.LocalPlayer.CanMove && MeetingHud.Instance == null))
            return;

        if (!initializeOverlays()) return;

        if (MapBehaviour.Instance != null)
            MapBehaviour.Instance.Close();

        hudManager.SetHudActive(false);

        overlayShown = true;

        Transform parent;
        if (MeetingHud.Instance != null)
            parent = MeetingHud.Instance.transform;
        else
            parent = hudManager.transform;

        infoUnderlay.transform.parent = parent;

        infoUnderlay.sprite = colorBG;
        infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
        infoUnderlay.transform.localScale = new Vector3(7.5f, 5f, 1f);
        infoUnderlay.enabled = true;


        // Show Items
        DelumWorldIcon = Object.Instantiate(infoUnderlay, hudManager.transform);
        DelumWorldIcon.color = Color.white;
        DelumWorldIcon.transform.localPosition = new Vector3(-3f, 2f, -900f);
        DelumWorldIcon.gameObject.SetActive(true);
        DelumWorldIcon.sprite = Main.Instance.Assets.WorldItemIcons[0];
        DelumWorldIcon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        DelumWorldIcon.enabled = true;

        DelumWorldText = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
        DelumWorldText.fontSize = 1.15f;
        DelumWorldText.outlineWidth += 0.02f;
        DelumWorldText.autoSizeTextContainer = false;
        DelumWorldText.enableWordWrapping = false;
        DelumWorldText.alignment = TextAlignmentOptions.TopLeft;
        DelumWorldText.transform.position = Vector3.zero;
        DelumWorldText.transform.position = Vector3.zero;
        DelumWorldText.transform.localPosition = new Vector3(-1.5f, 1f, -910f);
        DelumWorldText.transform.localScale = Vector3.one;
        DelumWorldText.color = Palette.White;
        DelumWorldText.enabled = true;
        DelumWorldText.text = ModTranslation.getString("DeluminatorTooltip");


        MapWorldIcon = Object.Instantiate(infoUnderlay, hudManager.transform);
        MapWorldIcon.color = Color.white;
        MapWorldIcon.transform.localPosition = new Vector3(-3f, 1f, -900f);
        MapWorldIcon.gameObject.SetActive(true);
        MapWorldIcon.sprite = Main.Instance.Assets.WorldItemIcons[1];
        MapWorldIcon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        MapWorldIcon.enabled = true;

        MapWorldText = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
        MapWorldText.fontSize = 1.15f;
        MapWorldText.outlineWidth += 0.02f;
        MapWorldText.autoSizeTextContainer = false;
        MapWorldText.enableWordWrapping = false;
        MapWorldText.alignment = TextAlignmentOptions.TopLeft;
        MapWorldText.transform.position = Vector3.zero;
        MapWorldText.transform.position = Vector3.zero;
        MapWorldText.transform.localPosition = new Vector3(-1.5f, 0f, -910f);
        MapWorldText.transform.localScale = Vector3.one;
        MapWorldText.color = Palette.White;
        MapWorldText.enabled = true;
        MapWorldText.text =
            string.Format(ModTranslation.getString("MaraudersMapTooltip"), Main.Instance.Config.MapDuration);


        KeyWorldIcon = Object.Instantiate(infoUnderlay, hudManager.transform);
        KeyWorldIcon.color = Color.white;
        KeyWorldIcon.transform.localPosition = new Vector3(-3f, 0f, -900f);
        KeyWorldIcon.gameObject.SetActive(true);
        KeyWorldIcon.sprite = /*Main.Instance.Assets.WorldItemIcons[2]*/Main.Instance.Assets.ItemIcons[2];
        KeyWorldIcon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        KeyWorldIcon.enabled = true;

        KeyWorldText = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
        KeyWorldText.fontSize = 1.15f;
        KeyWorldText.outlineWidth += 0.02f;
        KeyWorldText.autoSizeTextContainer = false;
        KeyWorldText.enableWordWrapping = false;
        KeyWorldText.alignment = TextAlignmentOptions.TopLeft;
        KeyWorldText.transform.position = Vector3.zero;
        KeyWorldText.transform.position = Vector3.zero;
        KeyWorldText.transform.localPosition = new Vector3(-1.5f, -1f, -910f);
        KeyWorldText.transform.localScale = Vector3.one;
        KeyWorldText.color = Palette.White;
        KeyWorldText.enabled = true;
        KeyWorldText.text = ModTranslation.getString("PortKeyTooltip");


        SnitchWorldIcon = Object.Instantiate(infoUnderlay, hudManager.transform);
        SnitchWorldIcon.color = Color.white;
        SnitchWorldIcon.transform.localPosition = new Vector3(-3f, -1f, -900f);
        SnitchWorldIcon.gameObject.SetActive(true);
        SnitchWorldIcon.sprite = Main.Instance.Assets.WorldItemIcons[3];
        SnitchWorldIcon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        SnitchWorldIcon.enabled = true;

        SnitchWorldText = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
        SnitchWorldText.fontSize = 1.15f;
        SnitchWorldText.outlineWidth += 0.02f;
        SnitchWorldText.autoSizeTextContainer = false;
        SnitchWorldText.enableWordWrapping = false;
        SnitchWorldText.alignment = TextAlignmentOptions.TopLeft;
        SnitchWorldText.transform.position = Vector3.zero;
        SnitchWorldText.transform.position = Vector3.zero;
        SnitchWorldText.transform.localPosition = new Vector3(-1.5f, -2f, -910f);
        SnitchWorldText.transform.localScale = Vector3.one;
        SnitchWorldText.color = Palette.White;
        SnitchWorldText.enabled = true;
        SnitchWorldText.text = ModTranslation.getString("TheGoldenSnitchTooltip");


        GhostStoneWorldIcon = Object.Instantiate(infoUnderlay, hudManager.transform);
        GhostStoneWorldIcon.color = Color.white;
        GhostStoneWorldIcon.transform.localPosition = new Vector3(-3f, -2f, -900f);
        GhostStoneWorldIcon.gameObject.SetActive(true);
        GhostStoneWorldIcon.sprite = Main.Instance.Assets.WorldItemIcons[4];
        GhostStoneWorldIcon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        GhostStoneWorldIcon.enabled = true;

        GhostStoneWorldText = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
        GhostStoneWorldText.fontSize = 1.15f;
        GhostStoneWorldText.outlineWidth += 0.02f;
        GhostStoneWorldText.autoSizeTextContainer = false;
        GhostStoneWorldText.enableWordWrapping = false;
        GhostStoneWorldText.alignment = TextAlignmentOptions.TopLeft;
        GhostStoneWorldText.transform.position = Vector3.zero;
        GhostStoneWorldText.transform.position = Vector3.zero;
        GhostStoneWorldText.transform.localPosition = new Vector3(-1.5f, -3f, -910f);
        GhostStoneWorldText.transform.localScale = Vector3.one;
        GhostStoneWorldText.color = Palette.White;
        GhostStoneWorldText.enabled = true;
        GhostStoneWorldText.text = ModTranslation.getString("GhostStoneTooltip");


        BeerWorldIcon = Object.Instantiate(infoUnderlay, hudManager.transform);
        BeerWorldIcon.color = Color.white;
        BeerWorldIcon.transform.localPosition = new Vector3(3f, 2f, -900f);
        BeerWorldIcon.gameObject.SetActive(true);
        BeerWorldIcon.sprite = Main.Instance.Assets.WorldItemIcons[5];
        BeerWorldIcon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        BeerWorldIcon.enabled = true;

        BeerWorldText = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
        BeerWorldText.fontSize = 1.15f;
        BeerWorldText.outlineWidth += 0.02f;
        BeerWorldText.autoSizeTextContainer = false;
        BeerWorldText.enableWordWrapping = false;
        BeerWorldText.alignment = TextAlignmentOptions.TopLeft;
        BeerWorldText.transform.position = Vector3.zero;
        BeerWorldText.transform.position = Vector3.zero;
        BeerWorldText.transform.localPosition = new Vector3(1.5f, 1f, -910f);
        BeerWorldText.transform.localScale = Vector3.one;
        BeerWorldText.color = Palette.White;
        BeerWorldText.enabled = true;
        BeerWorldText.text = "ButterBeerTooltip".Translate();


        ElderWandWorldIcon = Object.Instantiate(infoUnderlay, hudManager.transform);
        ElderWandWorldIcon.color = Color.white;
        ElderWandWorldIcon.transform.localPosition = new Vector3(3f, 1f, -900f);
        ElderWandWorldIcon.gameObject.SetActive(true);
        ElderWandWorldIcon.sprite = Main.Instance.Assets.WorldItemIcons[6];
        ElderWandWorldIcon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        ElderWandWorldIcon.enabled = true;

        ElderWandWorldText = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
        ElderWandWorldText.fontSize = 1.15f;
        ElderWandWorldText.outlineWidth += 0.02f;
        ElderWandWorldText.autoSizeTextContainer = false;
        ElderWandWorldText.enableWordWrapping = false;
        ElderWandWorldText.alignment = TextAlignmentOptions.TopLeft;
        ElderWandWorldText.transform.position = Vector3.zero;
        ElderWandWorldText.transform.position = Vector3.zero;
        ElderWandWorldText.transform.localPosition = new Vector3(1.5f, 0f, -910f);
        ElderWandWorldText.transform.localScale = Vector3.one;
        ElderWandWorldText.color = Palette.White;
        ElderWandWorldText.enabled = true;
        ElderWandWorldText.text = ModTranslation.getString("ElderWandTooltip");


        BasWorldIcon = Object.Instantiate(infoUnderlay, hudManager.transform);
        BasWorldIcon.color = Color.white;
        BasWorldIcon.transform.localPosition = new Vector3(3f, 0f, -900f);
        BasWorldIcon.gameObject.SetActive(true);
        BasWorldIcon.sprite = Main.Instance.Assets.WorldItemIcons[7];
        BasWorldIcon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        BasWorldIcon.enabled = true;

        BasWorldText = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
        BasWorldText.fontSize = 1.15f;
        BasWorldText.outlineWidth += 0.02f;
        BasWorldText.autoSizeTextContainer = false;
        BasWorldText.enableWordWrapping = false;
        BasWorldText.alignment = TextAlignmentOptions.TopLeft;
        BasWorldText.transform.position = Vector3.zero;
        BasWorldText.transform.position = Vector3.zero;
        BasWorldText.transform.localPosition = new Vector3(1.5f, -1f, -910f);
        BasWorldText.transform.localScale = Vector3.one;
        BasWorldText.color = Palette.White;
        BasWorldText.enabled = true;
        BasWorldText.text = "BasItemTooltip".Translate();


        SortingHatWorldIcon = Object.Instantiate(infoUnderlay, hudManager.transform);
        SortingHatWorldIcon.color = Color.white;
        SortingHatWorldIcon.transform.localPosition = new Vector3(3f, -1f, -900f);
        SortingHatWorldIcon.gameObject.SetActive(true);
        SortingHatWorldIcon.sprite = Main.Instance.Assets.WorldItemIcons[8];
        SortingHatWorldIcon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        SortingHatWorldIcon.enabled = true;

        SortingHatWorldText = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
        SortingHatWorldText.fontSize = 1.15f;
        SortingHatWorldText.outlineWidth += 0.02f;
        SortingHatWorldText.autoSizeTextContainer = false;
        SortingHatWorldText.enableWordWrapping = false;
        SortingHatWorldText.alignment = TextAlignmentOptions.TopLeft;
        SortingHatWorldText.transform.position = Vector3.zero;
        SortingHatWorldText.transform.position = Vector3.zero;
        SortingHatWorldText.transform.localPosition = new Vector3(1.5f, -2f, -910f);
        SortingHatWorldText.transform.localScale = Vector3.one;
        SortingHatWorldText.color = Palette.White;
        SortingHatWorldText.enabled = true;
        SortingHatWorldText.text = ModTranslation.getString("SortingHatTooltip");


        PhiloStoneWorldIcon = Object.Instantiate(infoUnderlay, hudManager.transform);
        PhiloStoneWorldIcon.color = Color.white;
        PhiloStoneWorldIcon.transform.localPosition = new Vector3(3f, -2f, -900f);
        PhiloStoneWorldIcon.gameObject.SetActive(true);
        PhiloStoneWorldIcon.sprite = Main.Instance.Assets.WorldItemIcons[9];
        PhiloStoneWorldIcon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        PhiloStoneWorldIcon.enabled = true;

        PhiloStoneWorldText = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
        PhiloStoneWorldText.fontSize = 1.15f;
        PhiloStoneWorldText.outlineWidth += 0.02f;
        PhiloStoneWorldText.autoSizeTextContainer = false;
        PhiloStoneWorldText.enableWordWrapping = false;
        PhiloStoneWorldText.alignment = TextAlignmentOptions.TopLeft;
        PhiloStoneWorldText.transform.position = Vector3.zero;
        PhiloStoneWorldText.transform.position = Vector3.zero;
        PhiloStoneWorldText.transform.localPosition = new Vector3(1.5f, -3f, -910f);
        PhiloStoneWorldText.transform.localScale = Vector3.one;
        PhiloStoneWorldText.color = Palette.White;
        PhiloStoneWorldText.enabled = true;
        PhiloStoneWorldText.text = ModTranslation.getString("PhiloStoneTooltip");


        DelumWorldText.enabled = true;
        MapWorldText.enabled = true;
        KeyWorldText.enabled = true;
        SnitchWorldText.enabled = true;
        GhostStoneWorldText.enabled = true;
        BeerWorldText.enabled = true;
        ElderWandWorldText.enabled = true;
        BasWorldText.enabled = true;
        SortingHatWorldText.enabled = true;
        PhiloStoneWorldText.enabled = true;
    }

    public static void hideInfoOverlay()
    {
        if (!overlayShown) return;

        if (MeetingHud.Instance == null) DestroyableSingleton<HudManager>.Instance.SetHudActive(true);

        overlayShown = false;
        var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

        DestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            if (infoUnderlay != null)
            {
                infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                if (t >= 1.0f) infoUnderlay.enabled = false;
            }

            DelumWorldText.enabled = false;
            MapWorldText.enabled = false;
            KeyWorldText.enabled = false;
            SnitchWorldText.enabled = false;
            GhostStoneWorldText.enabled = false;
            BeerWorldText.enabled = false;
            ElderWandWorldText.enabled = false;
            BasWorldText.enabled = false;
            SortingHatWorldText.enabled = false;
            PhiloStoneWorldText.enabled = false;

            DelumWorldIcon.gameObject.SetActive(false);
            MapWorldIcon.gameObject.SetActive(false);
            KeyWorldIcon.gameObject.SetActive(false);
            SnitchWorldIcon.gameObject.SetActive(false);
            GhostStoneWorldIcon.gameObject.SetActive(false);
            BeerWorldIcon.gameObject.SetActive(false);
            ElderWandWorldIcon.gameObject.SetActive(false);
            BasWorldIcon.gameObject.SetActive(false);
            SortingHatWorldIcon.gameObject.SetActive(false);
            PhiloStoneWorldIcon.gameObject.SetActive(false);
        })));
    }

    public static void toggleInfoOverlay()
    {
        if (overlayShown)
            hideInfoOverlay();
        else
            showInfoOverlay();
    }

    public static void resetOverlays()
    {
        hideBlackBG();
        hideInfoOverlay();
        Object.Destroy(meetingUnderlay);
        Object.Destroy(infoUnderlay);
        Object.Destroy(roleUnderlay);

        Object.Destroy(DelumWorldText);
        Object.Destroy(MapWorldText);
        Object.Destroy(KeyWorldText);
        Object.Destroy(SnitchWorldText);
        Object.Destroy(GhostStoneWorldText);
        Object.Destroy(BeerWorldText);
        Object.Destroy(ElderWandWorldText);
        Object.Destroy(BasWorldText);
        Object.Destroy(SortingHatWorldText);
        Object.Destroy(PhiloStoneWorldText);

        overlayShown = false;
        roleUnderlay = null;
        meetingUnderlay = infoUnderlay = null;
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class CustomOverlayKeybinds
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            var cc = DestroyableSingleton<HudManager>.Instance.Chat;
            var isOpen = cc != null && cc?.IsOpenOrOpening == true;
            if (Input.GetKeyDown(KeyCode.H) && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started &&
                !isOpen && !Minigame.Instance && !ExileController.Instance) toggleInfoOverlay();
        }
    }
}
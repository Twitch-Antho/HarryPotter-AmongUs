using System;
using AmongUs.Data;
using Assets.InnerNet;
using HarmonyLib;
using HarryPotter.Classes;
using Il2CppSystem.Collections.Generic;
using Reactor.Patches;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public class MainMenuPatch
{
    private static AnnouncementPopUp popUp;
    private static GameObject CreditsButton;
    private static GameObject DiscordButton;
    public static MainMenuManager Instance { get; private set; }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    public static void Start_Postfix(MainMenuManager __instance)
    {
        Instance = __instance;

        SimpleButton.SetBase(__instance.quitButton);

        var row = 1;
        var col = 0;

        GameObject CreatButton(string text, Action action)
        {
            col++;
            if (col > 2)
            {
                col = 1;
                row++;
            }

            var template = col == 1 ? __instance.creditsButton.gameObject : __instance.quitButton.gameObject;
            var button = Object.Instantiate(template, template.transform.parent);
            button.transform.transform.FindChild("FontPlacer").GetChild(0).gameObject.DestroyTranslator();
            var buttonText = button.transform.FindChild("FontPlacer").GetChild(0).GetComponent<TextMeshPro>();
            buttonText.text = text;
            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(action);
            var aspectPosition = button.GetComponent<AspectPosition>();
            aspectPosition.anchorPoint = new Vector2(col == 1 ? 0.415f : 0.583f, 0.5f - 0.08f * row);
            return button;
        }

        if (CreditsButton == null) CreditsButton = CreatButton("Credits", null);
        CreditsButton.gameObject.SetActive(true);
        CreditsButton.name = "Credits Button";
        var passiveButtoncredits = CreditsButton.GetComponent<PassiveButton>();
        var buttonSpritecredits = CreditsButton.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();

        passiveButtoncredits.OnClick = new Button.ButtonClickedEvent();
        var creditsColor = new Color(0, 1, 1, 0.8f);
        buttonSpritecredits.color = creditsColor;
        passiveButtoncredits.OnMouseOut.AddListener((Action)delegate { buttonSpritecredits.color = creditsColor; });
        //if (CreditsButton == null) CreditsButton = CreatButton("Credits", delegate
        passiveButtoncredits.OnClick.AddListener((Action)delegate
        {
            // do stuff
            if (popUp != null) Object.Destroy(popUp);
            var popUpTemplate = Object.FindObjectOfType<AnnouncementPopUp>(true);
            popUp = Object.Instantiate(popUpTemplate);

            popUp.gameObject.SetActive(true);
            var creditsString = DataManager.Settings.Language.CurrentLanguage == SupportedLangs.SChinese
                ? @"<align=""center"">贡献者:
模组由 FangkuaiYa 续更
原创作者 Hunter101#1337
艺术设计 賣蟑螂NotKomi & PhasmoFireGod

"
                : @"<align=""center"">Contributors:
Modded by FangkuaiYa
Original Design by Hunter101#1337
Art by 賣蟑螂NotKomi & PhasmoFireGod

";
            creditsString += DataManager.Settings.Language.CurrentLanguage == SupportedLangs.SChinese
                ? @"<size=70%> 其他资源和贡献者:
Reactor - 所有版本使用的框架
BepInEx - 用于挂钩游戏函数
TownOfUs-R - 游戏设置部分代码来自 eDonnes124
TheOtherRoles - 哈利波特帽代码来自TheOtherHats
</size>"
                : @"<size=70%> Other Credits & Resources:
Reactor - The framework used for all versions
BepInEx - Used to hook game functions
TownOfUs-R - Custom game options by eDonnes124
TheOtherRoles - Harry Potter Hats is based on the code from TheOtherRoles
</size>";
            creditsString += "</align>";
            Announcement creditsAnnouncement = new()
            {
                Id = "hpCredits",
                Language = 0,
                Number = 500,
                Title = DataManager.Settings.Language.CurrentLanguage == SupportedLangs.SChinese
                    ? "贡献者"
                    : "Credits and Contributors",
                ShortTitle = DataManager.Settings.Language.CurrentLanguage == SupportedLangs.SChinese
                    ? "哈利波特贡献者"
                    : "Harry Potter Credits",
                SubTitle = "",
                PinState = false,
                Date = "03.08.2025",
                Text = creditsString
            };
            __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(p =>
            {
                if (p == 1)
                {
                    var backup = DataManager.Player.Announcements.allAnnouncements;
                    DataManager.Player.Announcements.allAnnouncements = new List<Announcement>();
                    popUp.Init(false);
                    DataManager.Player.Announcements.SetAnnouncements(new[] { creditsAnnouncement });
                    popUp.CreateAnnouncementList();
                    popUp.UpdateAnnouncementText(creditsAnnouncement.Number);
                    popUp.visibleAnnouncements._items[0].PassiveButton.OnClick.RemoveAllListeners();
                    DataManager.Player.Announcements.allAnnouncements = backup;
                }
            })));
        });


        if (DiscordButton == null)
            DiscordButton = CreatButton("GitHub",
                () => Application.OpenURL("https://github.com/FangkuaiYa/HarryPotterAmongUs-R/"));
        DiscordButton.gameObject.SetActive(true);
        DiscordButton.name = "GitHub";
        var passiveDiscordButton = DiscordButton.GetComponent<PassiveButton>();
        var SpriteDiscordButton = DiscordButton.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();
        var DiscordColor = new Color(0.317f, 0, 1, 0.8f);
        SpriteDiscordButton.color = DiscordColor;
        passiveDiscordButton.OnMouseOut.AddListener((Action)delegate { SpriteDiscordButton.color = DiscordColor; });
    }
}

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
internal class VersionShowerStartPatch
{
    private static GameObject VersionShower1;
    public static GameObject OVersionShower;
    private static TextMeshPro VisitText;

    private static void Postfix(VersionShower __instance)
    {
        var credentialsText = "<color=#cdfffd>XtremeWave</color> © 2025";
        credentialsText += "\t\t\t";
        var versionText = $"<color=#FFF319>Harry Potter</color> - {Plugin.Version.ToString()}";
        credentialsText += versionText;
        var friendCode = GameObject.Find("FriendCode");
        if (friendCode != null && VersionShower1 == null)
        {
            VersionShower1 = Object.Instantiate(friendCode, friendCode.transform.parent);
            VersionShower1.name = "HP Version Shower";
            VersionShower1.transform.localPosition = friendCode.transform.localPosition + new Vector3(3.2f, 0f, 0f);
            VersionShower1.transform.localScale *= 1.7f;
            var TMP = VersionShower1.GetComponent<TextMeshPro>();
            TMP.alignment = TextAlignmentOptions.Right;
            TMP.fontSize = 30f;
            TMP.SetText(credentialsText);
        }

        if ((OVersionShower = GameObject.Find("VersionShower")) != null && VisitText == null)
        {
            VisitText = Object.Instantiate(__instance.text);
            VisitText.name = "HP User Counter";
            VisitText.alignment = TextAlignmentOptions.Left;
            VisitText.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            VisitText.transform.localPosition = new Vector3(-3.92f, -2.9f, 0f);
            VisitText.enabled = GameObject.Find("TOR Background") != null;

            __instance.text.text = $"<color=#FFF319>Harry Potter</color> v{Plugin.Version.ToString()}";
            __instance.text.alignment = TextAlignmentOptions.Left;
            OVersionShower.transform.localPosition = new Vector3(-4.92f, -3.3f, 0f);

            var ap1 = OVersionShower.GetComponent<AspectPosition>();
            if (ap1 != null) Object.Destroy(ap1);
            var ap2 = VisitText.GetComponent<AspectPosition>();
            if (ap2 != null) Object.Destroy(ap2);
        }

        ;
    }
}

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
[HarmonyPriority(Priority.First)]
internal class TitleLogoPatch
{
    public static GameObject TOR_Background;
    public static GameObject Ambience;
    public static GameObject Starfield;
    public static GameObject LeftPanel;
    public static GameObject RightPanel;
    public static GameObject CloseRightButton;
    public static GameObject Tint;
    public static GameObject Sizer;
    public static GameObject AULogo;
    public static GameObject BottomButtonBounds;

    public static Vector3 RightPanelOp;

    private static void Postfix(MainMenuManager __instance)
    {
        ReactorVersionShower.Text.Destroy();

        GameObject.Find("BackgroundTexture")?.SetActive(false);

        TOR_Background = new GameObject("TOR Background");
        TOR_Background.transform.position = new Vector3(0, 0, 520f);
        var bgRenderer = TOR_Background.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = ModHelpers.loadSpriteFromResources("HP_BG.png", 179f);

        if (!(Ambience = GameObject.Find("Ambience"))) return;
        if (!(Starfield = Ambience.transform.FindChild("starfield").gameObject)) return;
        var starGen = Starfield.GetComponent<StarGen>();
        starGen.SetDirection(new Vector2(0, -2));
        Starfield.transform.SetParent(TOR_Background.transform);
        Object.Destroy(Ambience);

        if (!(LeftPanel = GameObject.Find("LeftPanel"))) return;
        LeftPanel.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

        static void ResetParent(GameObject obj)
        {
            obj.transform.SetParent(LeftPanel.transform.parent);
        }

        LeftPanel.ForEachChild((Il2CppSystem.Action<GameObject>)ResetParent);
        LeftPanel.SetActive(false);

        Color shade = new(0f, 0f, 0f, 0f);
        var standardActiveSprite = __instance.newsButton.activeSprites.GetComponent<SpriteRenderer>().sprite;
        var minorActiveSprite = __instance.quitButton.activeSprites.GetComponent<SpriteRenderer>().sprite;
        System.Collections.Generic.Dictionary<System.Collections.Generic.List<PassiveButton>, (Sprite, Color, Color,
            Color, Color)> mainButtons = new()
        {
            {
                new System.Collections.Generic.List<PassiveButton>
                    { __instance.playButton, __instance.inventoryButton, __instance.shopButton },
                (standardActiveSprite, new Color(0.06f, 0.5f, 1f, 0.8f), shade, Color.white, Color.white)
            },
            {
                new System.Collections.Generic.List<PassiveButton>
                    { __instance.newsButton, __instance.myAccountButton, __instance.settingsButton },
                (minorActiveSprite, new Color(0.06f, 0.28f, 1f, 0.8f), shade, Color.white, Color.white)
            },
            {
                new System.Collections.Generic.List<PassiveButton> { __instance.creditsButton, __instance.quitButton },
                (minorActiveSprite, new Color(0.06f, 0.56f, 1f, 0.8f), shade, Color.white, Color.white)
            }
        };
        try
        {
            mainButtons.Keys.Flatten().DoIf(x => x != null, x => x.buttonText.color = Color.white);
        }
        catch
        {
        }

        void FormatButtonColor(PassiveButton button, Sprite borderType, Color inActiveColor, Color activeColor,
            Color inActiveTextColor, Color activeTextColor)
        {
            button.activeSprites.transform.FindChild("Shine")?.gameObject?.SetActive(false);
            button.inactiveSprites.transform.FindChild("Shine")?.gameObject?.SetActive(false);
            var activeRenderer = button.activeSprites.GetComponent<SpriteRenderer>();
            var inActiveRenderer = button.inactiveSprites.GetComponent<SpriteRenderer>();
            activeRenderer.sprite = minorActiveSprite;
            inActiveRenderer.sprite = minorActiveSprite;
            activeRenderer.color = activeColor.a == 0f
                ? new Color(inActiveColor.r, inActiveColor.g, inActiveColor.b, 1f)
                : activeColor;
            inActiveRenderer.color = inActiveColor;
            button.activeTextColor = activeTextColor;
            button.inactiveTextColor = inActiveTextColor;
        }

        foreach (var kvp in mainButtons)
            kvp.Key.Do(button => FormatButtonColor(button, kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3,
                kvp.Value.Item4, kvp.Value.Item5));

        GameObject.Find("Divider")?.SetActive(false);

        if (!(RightPanel = GameObject.Find("RightPanel"))) return;
        var rpap = RightPanel.GetComponent<AspectPosition>();
        if (rpap) Object.Destroy(rpap);
        RightPanelOp = RightPanel.transform.localPosition;
        RightPanel.transform.localPosition = RightPanelOp + new Vector3(10f, 0f, 0f);
        RightPanel.GetComponent<SpriteRenderer>().color = new Color(0.06f, 0.36f, 1f, 1f);

        CloseRightButton = new GameObject("CloseRightPanelButton");
        CloseRightButton.transform.SetParent(RightPanel.transform);
        CloseRightButton.transform.localPosition = new Vector3(-4.78f, 1.3f, 1f);
        CloseRightButton.transform.localScale = new Vector3(1f, 1f, 1f);
        CloseRightButton.AddComponent<BoxCollider2D>().size = new Vector2(0.6f, 1.5f);
        var closeRightSpriteRenderer = CloseRightButton.AddComponent<SpriteRenderer>();
        closeRightSpriteRenderer.sprite = Main.Instance.Assets.AbilityIcons[7];
        closeRightSpriteRenderer.color = new Color(0.06f, 0.36f, 1f, 1f);
        var closeRightPassiveButton = CloseRightButton.AddComponent<PassiveButton>();
        closeRightPassiveButton.OnClick = new Button.ButtonClickedEvent();
        closeRightPassiveButton.OnClick.AddListener((Action)MainMenuManagerPatch.HideRightPanel);
        closeRightPassiveButton.OnMouseOut = new UnityEvent();
        closeRightPassiveButton.OnMouseOut.AddListener((Action)(() =>
            closeRightSpriteRenderer.color = new Color(0.06f, 0.36f, 1f, 1f)));
        closeRightPassiveButton.OnMouseOver = new UnityEvent();
        closeRightPassiveButton.OnMouseOver.AddListener((Action)(() =>
            closeRightSpriteRenderer.color = new Color(0.06f, 0.36f, 1f, 1f)));

        Tint = __instance.screenTint.gameObject;
        var ttap = Tint.GetComponent<AspectPosition>();
        if (ttap) Object.Destroy(ttap);
        Tint.transform.SetParent(RightPanel.transform);
        Tint.transform.localPosition = new Vector3(-0.0824f, 0.0513f, Tint.transform.localPosition.z);
        Tint.transform.localScale = new Vector3(1f, 1f, 1f);
        __instance.howToPlayButton.gameObject.SetActive(true);
        __instance.howToPlayButton.transform.parent.Find("FreePlayButton").gameObject.SetActive(true);

        var creditsScreen = __instance.creditsScreen;
        if (creditsScreen)
        {
            var csto = creditsScreen.GetComponent<TransitionOpen>();
            if (csto) Object.Destroy(csto);
            var closeButton = creditsScreen.transform.FindChild("CloseButton");
            closeButton?.gameObject.SetActive(false);
        }

        if (!(Sizer = GameObject.Find("Sizer"))) return;
        if (!(AULogo = GameObject.Find("LOGO-AU"))) return;
        Sizer.transform.localPosition += new Vector3(0f, 0.12f, 0f);
        AULogo.transform.localScale = new Vector3(0.66f, 0.67f, 1f);
        AULogo.transform.position += new Vector3(0f, 0.1f, 0f);
        var logoRenderer = AULogo.GetComponent<SpriteRenderer>();
        logoRenderer.sprite = ModHelpers.loadSpriteFromResources("Banner.png", 300f);

        if (!(BottomButtonBounds = GameObject.Find("BottomButtonBounds"))) return;
        BottomButtonBounds.transform.localPosition -= new Vector3(0f, 0.1f, 0f);
    }
}

[HarmonyPatch(typeof(CreditsScreenPopUp))]
internal class CreditsScreenPopUpPatch
{
    [HarmonyPatch(nameof(CreditsScreenPopUp.OnEnable))]
    public static void Postfix(CreditsScreenPopUp __instance)
    {
        __instance.BackButton.transform.parent.FindChild("Background").gameObject.SetActive(false);
    }
}
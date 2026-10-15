using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AmongUs.Data;
using Assets.InnerNet;
using Twitch;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities;

namespace HarryPotter.Classes.UI;
public class ModUpdater : MonoBehaviour
{
    public const string RepositoryOwner = "Twitch-Antho";
    public const string RepositoryName = "HarryPotter-AmongUs";
    public static ModUpdater Instance { get; private set; }

    private long _lastDownloadBytes;
    private float _lastDownloadTime;

    public ModUpdater(IntPtr ptr) : base(ptr) { }

    private bool _busy;
    private bool showPopUp = true;
    public List<GithubRelease> Releases;

    public void Awake()
    {
        if (Instance) Destroy(Instance);
        Instance = this;
        foreach (var file in Directory.GetFiles(Paths.PluginPath, "*.old"))
        {
            File.Delete(file);
        }
    }

    private void Start()
    {
        if (_busy) return;
        this.StartCoroutine(CoCheckForUpdate());
        SceneManager.add_sceneLoaded((System.Action<Scene, LoadSceneMode>)(OnSceneLoaded));
    }


    [HideFromIl2Cpp]
    public void StartDownloadRelease(GithubRelease release)
    {
        if (_busy) return;
        this.StartCoroutine(CoDownloadRelease(release));
    }

    [HideFromIl2Cpp]
    private IEnumerator CoCheckForUpdate()
    {
        _busy = true;
        var www = new UnityWebRequest();
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        www.SetUrl($"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases");
        www.downloadHandler = new DownloadHandlerBuffer();
        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            yield break;
        }

        Releases = JsonSerializer.Deserialize<List<GithubRelease>>(www.downloadHandler.text);
        www.downloadHandler.Dispose();
        www.Dispose();
        Releases.Sort(SortReleases);
        _busy = false;
    }

    [HideFromIl2Cpp]
    private IEnumerator CoDownloadRelease(GithubRelease release)
    {
        _busy = true;
        _lastDownloadBytes = 0;
        _lastDownloadTime = 0;

        var popup = Instantiate(TwitchManager.Instance.TwitchPopup);
        popup.TextAreaTMP.fontSize *= 0.7f;
        popup.TextAreaTMP.enableAutoSizing = false;

        popup.Show();

        var button = popup.transform.GetChild(2).gameObject;
        button.SetActive(false);
        popup.TextAreaTMP.text = $"updatingText".Translate();

        var asset = release.Assets.Find(FilterPluginAsset);
        var www = new UnityWebRequest();
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        www.SetUrl(asset.DownloadUrl);
        www.downloadHandler = new DownloadHandlerBuffer();
        var operation = www.SendWebRequest();

        _lastDownloadTime = Time.realtimeSinceStartup;

        while (!operation.isDone)
        {
            int stars = Mathf.CeilToInt(www.downloadProgress * 10);

            long currentBytes = (long)www.downloadedBytes;
            float currentTime = Time.realtimeSinceStartup;

            var downloadText = new String((char)0x25A0, stars) + new String((char)0x25A1, 10 - stars);
            if (_lastDownloadTime > 0 && _lastDownloadBytes > 0)
            {
                float timeDelta = currentTime - _lastDownloadTime;
                long bytesDelta = currentBytes - _lastDownloadBytes;

                if (timeDelta > 0)
                {
                    float speed = bytesDelta / timeDelta;
                    string speedText;
                    if (speed > 1048576)
                    {
                        speedText = $"{(speed / 1048576):0.00} MB/s";
                    }
                    else
                    {
                        speedText = $"{(speed / 1024):0.00} KB/s";
                    }
                    popup.TextAreaTMP.text = string.Format("downliadingText".Translate(), downloadText, speedText);
                }
            }
            else
            {
                var speedText = "0.00 KB/s";
                popup.TextAreaTMP.text = string.Format("downliadingText".Translate(), downloadText, speedText);
            }

            _lastDownloadBytes = currentBytes;
            _lastDownloadTime = currentTime;

            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            popup.TextAreaTMP.text = "updateFailedText".Translate();
            yield break;
        }
        popup.TextAreaTMP.text = $"copyingFileText".Translate();

        var filePath = Path.Combine(Paths.PluginPath, asset.Name);

        if (File.Exists(filePath + ".old")) File.Delete(filePath + "old");
        if (File.Exists(filePath)) File.Move(filePath, filePath + ".old");

        var persistTask = File.WriteAllBytesAsync(filePath, www.downloadHandler.data);
        var hasError = false;
        while (!persistTask.IsCompleted)
        {
            if (persistTask.Exception != null)
            {
                hasError = true;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        www.downloadHandler.Dispose();
        www.Dispose();

        if (!hasError)
        {
            popup.TextAreaTMP.text = $"updateEndText".Translate();
        }
        button.SetActive(true);
        _busy = false;
    }

    [HideFromIl2Cpp]
    private static bool FilterLatestRelease(GithubRelease release)
    {
        return release.IsNewer(Plugin.Version) && release.Assets.Any(FilterPluginAsset);
    }

    [HideFromIl2Cpp]
    private static bool FilterPluginAsset(GithubAsset asset)
    {
        return asset.Name == "HarryPotter.dll";
    }

    [HideFromIl2Cpp]
    private static int SortReleases(GithubRelease a, GithubRelease b)
    {
        if (a.IsNewer(b.Version)) return -1;
        if (b.IsNewer(a.Version)) return 1;
        return 0;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_busy || scene.name != "MainMenu") return;
        var latestRelease = Releases.FirstOrDefault();
        if (latestRelease == null || latestRelease.Version <= Plugin.Version)
            return;

        var template = GameObject.Find("ExitGameButton");
        if (!template) return;

        string parsedDescription = ParseLocalizedDescription(latestRelease.Description);

        var button = Instantiate(template, null);
        var buttonTransform = button.transform;
        button.GetComponent<AspectPosition>().anchorPoint = new Vector2(0.458f, 0.124f);

        PassiveButton passiveButton = button.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((Action)(() =>
        {
            StartDownloadRelease(latestRelease);
            button.SetActive(false);
        }));

        var text = button.transform.GetComponentInChildren<TMPro.TMP_Text>();
        string t = "updateButtonText".Translate();
        StartCoroutine(Effects.Lerp(0.1f, (Action<float>)(p => text.SetText(t))));
        passiveButton.OnMouseOut.AddListener((Action)(() => text.color = Color.red));
        passiveButton.OnMouseOver.AddListener((Action)(() => text.color = Color.white));

        var announcement = string.Format("announcementText".Translate(), latestRelease.Tag, parsedDescription);
        var mgr = FindObjectOfType<MainMenuManager>(true);
        if (showPopUp) mgr.StartCoroutine(CoShowAnnouncement(announcement, shortTitle: "HP Update", date: latestRelease.PublishedAt));
        showPopUp = false;
    }

    [HideFromIl2Cpp]
    private string ParseLocalizedDescription(string rawDescription)
    {
        try
        {
            var lang = DataManager.Settings.Language.CurrentLanguage;
            string targetSection = lang == SupportedLangs.SChinese ? "### CN" : "### EN";
            string nextSection = lang == SupportedLangs.SChinese ? "### EN" : "### CN";

            string normalized = rawDescription.Replace("\r\n", "\n");
            int startIndex = normalized.IndexOf(targetSection);

            if (startIndex == -1) return rawDescription;

            startIndex += targetSection.Length;
            int endIndex = normalized.IndexOf("###", startIndex);

            string result = endIndex == -1 ?
                normalized.Substring(startIndex) :
                normalized.Substring(startIndex, endIndex - startIndex);

            return result.Trim('\n', '\r', ' ')
                         .Replace("\n#", "\n##");
        }
        catch
        {
            return rawDescription;
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator CoShowAnnouncement(string announcement, bool show = true, string shortTitle = "HP Update", string title = "", string date = "")
    {
        var mgr = FindObjectOfType<MainMenuManager>(true);
        var popUpTemplate = UnityEngine.Object.FindObjectOfType<AnnouncementPopUp>(true);
        if (popUpTemplate == null)
        {
            PluginSingleton<Plugin>.Instance.Log.LogError("couldnt show credits, popUp is null");
            yield return null;
        }
        var popUp = UnityEngine.Object.Instantiate(popUpTemplate);

        popUp.gameObject.SetActive(true);

        Assets.InnerNet.Announcement creditsAnnouncement = new()
        {
            Id = "hpAnnouncement",
            Language = 0,
            Number = 6969,
            Title = title == "" ? "Harry Potter Announcement" : title,
            ShortTitle = shortTitle,
            SubTitle = "",
            PinState = false,
            Date = date == "" ? DateTime.Now.Date.ToString() : date,
            Text = announcement,
        };
        mgr.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => {
            if (p == 1)
            {
                var backup = DataManager.Player.Announcements.allAnnouncements;
                DataManager.Player.Announcements.allAnnouncements = new();
                popUp.Init(false);
                DataManager.Player.Announcements.SetAnnouncements(new Announcement[] { creditsAnnouncement });
                popUp.CreateAnnouncementList();
                popUp.UpdateAnnouncementText(creditsAnnouncement.Number);
                popUp.visibleAnnouncements[0].PassiveButton.OnClick.RemoveAllListeners();
                DataManager.Player.Announcements.allAnnouncements = backup;
            }
        })));
    }
}

public class GithubRelease
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("tag_name")]
    public string Tag { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("published_at")]
    public string PublishedAt { get; set; }

    [JsonPropertyName("body")]
    public string Description { get; set; }

    [JsonPropertyName("assets")]
    public List<GithubAsset> Assets { get; set; }

    public Version Version => Version.Parse(Tag.Replace("v", string.Empty));

    public bool IsNewer(Version version)
    {
        return Version > version;
    }
}

public class GithubAsset
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; set; }
}

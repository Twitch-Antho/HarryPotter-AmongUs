using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using AmongUs.Data;
using HarryPotter.Classes;
using Newtonsoft.Json.Linq;

namespace HarryPotter;

public class ModTranslation
{
    private const string blankText = "[BLANK]";
    public static int defaultLanguage = (int)SupportedLangs.English;
    public static Dictionary<string, Dictionary<int, string>> stringData;

    public static void Load()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var json = Main.Instance.Assets.lanuageJsonAsset.text;

        stringData = new Dictionary<string, Dictionary<int, string>>();
        var parsed = JObject.Parse(json);

        for (var i = 0; i < parsed.Count; i++)
        {
            var token = parsed.ChildrenTokens[i].TryCast<JProperty>();
            if (token == null) continue;

            var stringName = token.Name;
            var val = token.Value.TryCast<JObject>();

            if (token.HasValues)
            {
                var strings = new Dictionary<int, string>();

                for (var j = 0; j < (int)SupportedLangs.Irish + 1; j++)
                {
                    var key = j.ToString();
                    var text = val[key]?.TryCast<JValue>().Value.ToString();

                    if (text != null && text.Length > 0)
                    {
                        if (text == blankText) strings[j] = "";
                        else strings[j] = text;
                    }
                }

                stringData[stringName] = strings;
            }
        }
    }

    public static string getString(string key, string def = null)
    {
        // Strip out color tags.
        var keyClean = Regex.Replace(key, "<.*?>", "");
        keyClean = Regex.Replace(keyClean, "^-\\s*", "");
        keyClean = keyClean.Trim();

        def ??= key;
        if (!stringData.ContainsKey(keyClean)) return def;

        var data = stringData[keyClean];
        var lang = (int)DataManager.Settings.Language.CurrentLanguage;

        if (data.ContainsKey(lang)) return key.Replace(keyClean, data[lang]);

        if (data.ContainsKey(defaultLanguage)) return key.Replace(keyClean, data[defaultLanguage]);

        return key;
    }
}

internal static class LanguageExtension
{
    internal static string Translate(this string key)
    {
        return ModTranslation.getString(key);
    }
}
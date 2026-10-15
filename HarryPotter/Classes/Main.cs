using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using HarryPotter.Classes.Roles;
using HarryPotter.Classes.UI;
using HarryPotter.Classes.WorldItems;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace HarryPotter.Classes;

public class Pair<T1, T2>
{
    public Pair(T1 first, T2 second)
    {
        Item1 = first;
        Item2 = second;
    }

    public T1 Item1 { get; set; }
    public T2 Item2 { get; set; }
}

internal static class ModHelpers
{
    public static Random rnd = new((int)DateTime.Now.Ticks);
    internal static d_LoadImage iCall_LoadImage;

    public static string cs(Color c, string s)
    {
        return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b),
            ToByte(c.a), s);
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }

    public static bool isCN()
    {
        try
        {
            var name = CultureInfo.CurrentUICulture.Name;
            if (name.StartsWith("zh")) return true;
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static void ModRevive(this PlayerControl player, bool resetRoleIfGhost = true)
    {
        player.Data.IsDead = false;
        player.moveable = true;
        player.gameObject.layer = LayerMask.NameToLayer("Players");
        player.MyPhysics.ResetMoveState();
        player.cosmetics.SetPetSource(player);
        player.cosmetics.SetNameMask(true);
        if (player.AmOwner)
        {
            DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(true);
            DestroyableSingleton<HudManager>.Instance.KillButton.ToggleVisible(player.Data.Role.IsImpostor);
            DestroyableSingleton<HudManager>.Instance.AdminButton.ToggleVisible(player.Data.Role.IsImpostor);
            DestroyableSingleton<HudManager>.Instance.SabotageButton.ToggleVisible(player.Data.Role.IsImpostor);
            DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(player.Data.Role.IsImpostor);
            DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
            DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(false);
        }

        if (!resetRoleIfGhost || !AmongUsClient.Instance.AmHost || !RoleManager.IsGhostRole(player.Data.Role.Role))
            return;
        player.RpcSetRole(RoleTypes.Crewmate);
    }

    public static bool isMira()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 1;
    }

    public static bool isAirship()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 4;
    }

    public static bool isSkeld()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 0;
    }

    public static bool isPolus()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 2;
    }

    public static bool isFungle()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == 5;
    }

    public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit = 100f)
    {
        path = "HarryPotter.Resources." + path;
        try
        {
            var texture = loadTextureFromResources(path);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
        }
        catch
        {
            System.Console.WriteLine("Error loading sprite from path: " + path);
        }

        return null;
    }

    public static Texture2D loadTextureFromResources(string path)
    {
        try
        {
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(path);
            var byteTexture = new byte[stream.Length];
            var read = stream.Read(byteTexture, 0, (int)stream.Length);
            LoadImage(texture, byteTexture, false);
            return texture;
        }
        catch
        {
            System.Console.WriteLine("Error loading texture from resources: " + path);
        }

        return null;
    }

    public static Texture2D loadTextureFromDisk(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                var byteTexture = Il2CppSystem.IO.File.ReadAllBytes(path);
                texture.LoadImage(byteTexture, false);
                return texture;
            }
        }
        catch
        {
            PluginSingleton<Plugin>.Instance.Log.LogError("Error loading texture from disk: " + path);
        }

        return null;
    }

    internal static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
    {
        if (iCall_LoadImage == null)
            iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
        var il2cppArray = (Il2CppStructArray<byte>)data;
        return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
    }

    public static PlayerControl FindClosestTarget(this PlayerControl player)
    {
        return player.Data.Role.FindClosestTarget();
    }

    internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
}

internal class Main
{
    public bool ControlKillUsed;

    public bool isActivateHourglass = false;

    public Main()
    {
        Config = new Config();
        Rpc = new CustomRpc();
        Assets = new Asset();
        AllPlayers = new List<ModdedPlayerClass>();
        AllItems = new List<WorldItem>();
        PlayersWithRequestedRoles = new List<Pair<PlayerControl, string>>();
    }

    public static Main Instance { get; set; }
    public List<ModdedPlayerClass> AllPlayers { get; set; }
    public List<WorldItem> AllItems { get; set; }
    public Config Config { get; set; }
    public CustomRpc Rpc { get; set; }
    public Asset Assets { get; set; }
    public int CurrentStage { get; set; }
    public List<Pair<PlayerControl, string>> PlayersWithRequestedRoles { get; set; }
    public List<Tuple<byte, Vector2>> PossibleItemPositions { get; set; }

    public List<Tuple<byte, Vector2>> DefaultItemPositons { get; } = new()
    {
        new Tuple<byte, Vector2>(2, new Vector2(18.58625f, -21.96028f)),
        new Tuple<byte, Vector2>(2, new Vector2(17.26129f, -19.21864f)),
        new Tuple<byte, Vector2>(2, new Vector2(17.12244f, -17.1841f)),
        new Tuple<byte, Vector2>(2, new Vector2(20.71172f, -21.69597f)),
        new Tuple<byte, Vector2>(2, new Vector2(22.10037f, -25.14912f)),
        new Tuple<byte, Vector2>(2, new Vector2(24.01747f, -24.70225f)),
        new Tuple<byte, Vector2>(2, new Vector2(27.69598f, -20.75172f)),
        new Tuple<byte, Vector2>(2, new Vector2(36.57115f, -21.61535f)),
        new Tuple<byte, Vector2>(2, new Vector2(39.53685f, -10.09214f)),
        new Tuple<byte, Vector2>(2, new Vector2(34.10915f, -10.16738f)),
        new Tuple<byte, Vector2>(2, new Vector2(40.37588f, -7.95609f)),
        new Tuple<byte, Vector2>(2, new Vector2(30.84753f, -7.736305f)),
        new Tuple<byte, Vector2>(2, new Vector2(25.44218f, -7.583571f)),
        new Tuple<byte, Vector2>(2, new Vector2(22.11417f, -8.61549f)),
        new Tuple<byte, Vector2>(2, new Vector2(24.15556f, -3.469771f)),
        new Tuple<byte, Vector2>(2, new Vector2(25.03353f, -12.18799f)),
        new Tuple<byte, Vector2>(2, new Vector2(30.12325f, -16.94627f)),
        new Tuple<byte, Vector2>(2, new Vector2(24.99184f, -17.18198f)),
        new Tuple<byte, Vector2>(2, new Vector2(19.82783f, -14.6528f)),
        new Tuple<byte, Vector2>(2, new Vector2(13.25453f, -12.43384f)),
        new Tuple<byte, Vector2>(2, new Vector2(20.17053f, -11.87447f)),
        new Tuple<byte, Vector2>(2, new Vector2(9.549327f, -9.725276f)),
        new Tuple<byte, Vector2>(2, new Vector2(7.533298f, -12.32464f)),
        new Tuple<byte, Vector2>(2, new Vector2(5.438123f, -11.85916f)),
        new Tuple<byte, Vector2>(2, new Vector2(2.918584f, -11.96187f)),
        new Tuple<byte, Vector2>(2, new Vector2(5.385927f, -16.55273f)),
        new Tuple<byte, Vector2>(2, new Vector2(1.579458f, -17.25811f)),
        new Tuple<byte, Vector2>(2, new Vector2(2.281264f, -24.06277f)),
        new Tuple<byte, Vector2>(2, new Vector2(1.496747f, -20.5509f)),
        new Tuple<byte, Vector2>(2, new Vector2(6.260661f, -24.13944f)),
        new Tuple<byte, Vector2>(2, new Vector2(10.77005f, -20.56692f)),
        new Tuple<byte, Vector2>(2, new Vector2(12.74145f, -23.43479f)),
        new Tuple<byte, Vector2>(2, new Vector2(11.00322f, -17.61416f)),
        new Tuple<byte, Vector2>(2, new Vector2(16.18938f, -24.71703f)),
        new Tuple<byte, Vector2>(2, new Vector2(4.66598f, -4.429569f)),
        new Tuple<byte, Vector2>(2, new Vector2(11.46261f, -7.265319f)),
        new Tuple<byte, Vector2>(2, new Vector2(16.66492f, -2.420291f)),
        new Tuple<byte, Vector2>(0, new Vector2(-4.182233f, 1.07599f)),
        new Tuple<byte, Vector2>(0, new Vector2(-0.7304592f, -2.823687f)),
        new Tuple<byte, Vector2>(0, new Vector2(2.807524f, 0.9642968f)),
        new Tuple<byte, Vector2>(0, new Vector2(-0.930865f, 4.924871f)),
        new Tuple<byte, Vector2>(0, new Vector2(9.511244f, -0.3346088f)),
        new Tuple<byte, Vector2>(0, new Vector2(6.474998f, -3.704194f)),
        new Tuple<byte, Vector2>(0, new Vector2(16.74196f, -4.494635f)),
        new Tuple<byte, Vector2>(0, new Vector2(11.9279f, -6.496367f)),
        new Tuple<byte, Vector2>(0, new Vector2(9.137466f, -12.28679f)),
        new Tuple<byte, Vector2>(0, new Vector2(7.774953f, -14.2921f)),
        new Tuple<byte, Vector2>(0, new Vector2(4.006791f, -15.55052f)),
        new Tuple<byte, Vector2>(0, new Vector2(-0.5833896f, -15.77747f)),
        new Tuple<byte, Vector2>(0, new Vector2(-3.692864f, -14.83466f)),
        new Tuple<byte, Vector2>(0, new Vector2(-9.448769f, -14.59895f)),
        new Tuple<byte, Vector2>(0, new Vector2(-8.734068f, -11.36326f)),
        new Tuple<byte, Vector2>(0, new Vector2(-9.266067f, -8.41872f)),
        new Tuple<byte, Vector2>(0, new Vector2(-12.16772f, -11.86919f)),
        new Tuple<byte, Vector2>(0, new Vector2(-17.45776f, -13.38308f)),
        new Tuple<byte, Vector2>(0, new Vector2(-13.52165f, -5.388005f)),
        new Tuple<byte, Vector2>(0, new Vector2(-20.25068f, -5.388005f)),
        new Tuple<byte, Vector2>(0, new Vector2(-21.6893f, -7.326626f)),
        new Tuple<byte, Vector2>(0, new Vector2(-21.79704f, -3.05468f)),
        new Tuple<byte, Vector2>(0, new Vector2(-16.86277f, -1.031248f)),
        new Tuple<byte, Vector2>(0, new Vector2(-17.02352f, 2.37704f)),
        new Tuple<byte, Vector2>(0, new Vector2(-9.174051f, 0.764464f)),
        new Tuple<byte, Vector2>(0, new Vector2(-9.0562f, -2.353384f)),
        new Tuple<byte, Vector2>(0, new Vector2(-9.0562f, -4.770045f)),
        new Tuple<byte, Vector2>(0, new Vector2(6.228173f, -7.598089f)),
        new Tuple<byte, Vector2>(0, new Vector2(2.062296f, -7.244535f)),
        new Tuple<byte, Vector2>(1, new Vector2(25.60198f, -1.924499f)),
        new Tuple<byte, Vector2>(1, new Vector2(22.01862f, -1.924499f)),
        new Tuple<byte, Vector2>(1, new Vector2(19.5472f, 0.2135701f)),
        new Tuple<byte, Vector2>(1, new Vector2(19.26237f, 4.432643f)),
        new Tuple<byte, Vector2>(1, new Vector2(22.10624f, 2.365375f)),
        new Tuple<byte, Vector2>(1, new Vector2(25.37229f, 4.71474f)),
        new Tuple<byte, Vector2>(1, new Vector2(23.33011f, 6.756915f)),
        new Tuple<byte, Vector2>(1, new Vector2(17.85176f, 11.23526f)),
        new Tuple<byte, Vector2>(1, new Vector2(12.27656f, 6.639064f)),
        new Tuple<byte, Vector2>(1, new Vector2(6.114027f, 0.9975319f)),
        new Tuple<byte, Vector2>(1, new Vector2(9.414569f, 1.145713f)),
        new Tuple<byte, Vector2>(1, new Vector2(6.087941f, 6.03428f)),
        new Tuple<byte, Vector2>(1, new Vector2(2.498136f, 10.72081f)),
        new Tuple<byte, Vector2>(1, new Vector2(6.086607f, 12.19221f)),
        new Tuple<byte, Vector2>(1, new Vector2(8.707928f, 12.92791f)),
        new Tuple<byte, Vector2>(1, new Vector2(15.20495f, 4.131007f)),
        new Tuple<byte, Vector2>(1, new Vector2(14.62f, 0.3439525f)),
        new Tuple<byte, Vector2>(1, new Vector2(12.28571f, -1.344667f)),
        new Tuple<byte, Vector2>(1, new Vector2(8.014729f, -1.344667f)),
        new Tuple<byte, Vector2>(1, new Vector2(-4.464246f, -1.344667f)),
        new Tuple<byte, Vector2>(1, new Vector2(-4.464246f, 2.071996f)),
        new Tuple<byte, Vector2>(1, new Vector2(17.84423f, 17.16991f)),
        new Tuple<byte, Vector2>(1, new Vector2(14.76334f, 20.16747f)),
        new Tuple<byte, Vector2>(1, new Vector2(19.75764f, 20.30775f)),
        new Tuple<byte, Vector2>(1, new Vector2(17.82509f, 22.0234f)),
        new Tuple<byte, Vector2>(1, new Vector2(16.16338f, 24.20611f)),
        new Tuple<byte, Vector2>(4, new Vector2(-0.7782411f, 0.4685913f)),
        new Tuple<byte, Vector2>(4, new Vector2(-7.420381f, 0.4966883f)),
        new Tuple<byte, Vector2>(4, new Vector2(-10.912f, -1.175899f)),
        new Tuple<byte, Vector2>(4, new Vector2(-13.3935f, 1.22228f)),
        new Tuple<byte, Vector2>(4, new Vector2(-17.48869f, -0.9646047f)),
        new Tuple<byte, Vector2>(4, new Vector2(-23.44337f, -1.358598f)),
        new Tuple<byte, Vector2>(4, new Vector2(-14.16418f, -4.727928f)),
        new Tuple<byte, Vector2>(4, new Vector2(-10.07188f, -5.741868f)),
        new Tuple<byte, Vector2>(4, new Vector2(-10.07188f, -7.658527f)),
        new Tuple<byte, Vector2>(4, new Vector2(-14.55895f, -8.608552f)),
        new Tuple<byte, Vector2>(4, new Vector2(-7.496612f, -7.233172f)),
        new Tuple<byte, Vector2>(4, new Vector2(-6.758393f, -11.28371f)),
        new Tuple<byte, Vector2>(4, new Vector2(-2.138819f, -12.1676f)),
        new Tuple<byte, Vector2>(4, new Vector2(1.444511f, -12.33426f)),
        new Tuple<byte, Vector2>(4, new Vector2(7.065756f, -12.64319f)),
        new Tuple<byte, Vector2>(4, new Vector2(-13.58657f, -12.36934f)),
        new Tuple<byte, Vector2>(4, new Vector2(-15.81556f, -12.36934f)),
        new Tuple<byte, Vector2>(4, new Vector2(-13.67749f, -14.22881f)),
        new Tuple<byte, Vector2>(4, new Vector2(10.26638f, -15.4287f)),
        new Tuple<byte, Vector2>(4, new Vector2(10.49246f, -8.517835f)),
        new Tuple<byte, Vector2>(4, new Vector2(10.34679f, -6.517842f)),
        new Tuple<byte, Vector2>(4, new Vector2(13.29307f, -8.638988f)),
        new Tuple<byte, Vector2>(4, new Vector2(16.3192f, -11.1449f)),
        new Tuple<byte, Vector2>(4, new Vector2(16.3192f, -8.728246f)),
        new Tuple<byte, Vector2>(4, new Vector2(16.3192f, -6.394921f)),
        new Tuple<byte, Vector2>(4, new Vector2(12.55694f, -2.757262f)),
        new Tuple<byte, Vector2>(4, new Vector2(19.35949f, -4.450295f)),
        new Tuple<byte, Vector2>(4, new Vector2(22.77165f, -8.549025f)),
        new Tuple<byte, Vector2>(4, new Vector2(24.76921f, -5.884814f)),
        new Tuple<byte, Vector2>(4, new Vector2(29.19261f, -5.933629f)),
        new Tuple<byte, Vector2>(4, new Vector2(32.40412f, -5.814812f)),
        new Tuple<byte, Vector2>(4, new Vector2(32.42162f, -3.183638f)),
        new Tuple<byte, Vector2>(4, new Vector2(33.88465f, -0.9706092f)),
        new Tuple<byte, Vector2>(4, new Vector2(33.85013f, 3.216274f)),
        new Tuple<byte, Vector2>(4, new Vector2(38.45294f, -0.2198919f)),
        new Tuple<byte, Vector2>(4, new Vector2(37.77024f, -3.351321f)),
        new Tuple<byte, Vector2>(4, new Vector2(28.91639f, -1.531849f)),
        new Tuple<byte, Vector2>(4, new Vector2(25.86155f, 0.5424373f)),
        new Tuple<byte, Vector2>(4, new Vector2(24.07702f, 0.09125323f)),
        new Tuple<byte, Vector2>(4, new Vector2(24.05602f, 2.257918f)),
        new Tuple<byte, Vector2>(4, new Vector2(18.72092f, 0.006185418f)),
        new Tuple<byte, Vector2>(4, new Vector2(19.91638f, 6.678555f)),
        new Tuple<byte, Vector2>(4, new Vector2(22.81039f, 8.989212f)),
        new Tuple<byte, Vector2>(4, new Vector2(19.80523f, 11.26535f)),
        new Tuple<byte, Vector2>(4, new Vector2(17.69994f, 9.076731f)),
        new Tuple<byte, Vector2>(4, new Vector2(15.384f, 1.640165f)),
        new Tuple<byte, Vector2>(4, new Vector2(12.27794f, 1.865757f)),
        new Tuple<byte, Vector2>(4, new Vector2(12.31664f, -0.1788689f)),
        new Tuple<byte, Vector2>(4, new Vector2(9.262692f, 1.896087f)),
        new Tuple<byte, Vector2>(4, new Vector2(6.444838f, -2.560592f)),
        new Tuple<byte, Vector2>(4, new Vector2(6.163728f, 2.282847f)),
        new Tuple<byte, Vector2>(4, new Vector2(24.74887f, 7.754529f)),
        new Tuple<byte, Vector2>(4, new Vector2(30.77678f, 7.037311f)),
        new Tuple<byte, Vector2>(4, new Vector2(30.63032f, 5.350429f)),
        new Tuple<byte, Vector2>(4, new Vector2(11.98442f, 8.979587f)),
        new Tuple<byte, Vector2>(4, new Vector2(11.79658f, 6.068962f)),
        new Tuple<byte, Vector2>(4, new Vector2(-0.9279394f, 4.762999f)),
        new Tuple<byte, Vector2>(4, new Vector2(-0.9279395f, 8.512984f)),
        new Tuple<byte, Vector2>(4, new Vector2(-5.042377f, 8.689761f)),
        new Tuple<byte, Vector2>(4, new Vector2(-8.985039f, 12.50144f)),
        new Tuple<byte, Vector2>(4, new Vector2(-12.64849f, 8.643408f)),
        new Tuple<byte, Vector2>(4, new Vector2(-8.8501f, 5.144454f)),
        new Tuple<byte, Vector2>(4, new Vector2(4.531908f, 15.29855f)),
        new Tuple<byte, Vector2>(4, new Vector2(16.36781f, 15.21838f)),
        new Tuple<byte, Vector2>(5, new Vector2(-10.0842f, 13.0026f)),
        new Tuple<byte, Vector2>(5, new Vector2(0.9815f, 6.7968f)),
        new Tuple<byte, Vector2>(5, new Vector2(22.5621f, 3.2779f)),
        new Tuple<byte, Vector2>(5, new Vector2(-1.8699f, -1.3406f)),
        new Tuple<byte, Vector2>(5, new Vector2(12.0036f, 2.6763f)),
        new Tuple<byte, Vector2>(5, new Vector2(21.705f, -7.8691f)),
        new Tuple<byte, Vector2>(5, new Vector2(1.4485f, -1.6105f)),
        new Tuple<byte, Vector2>(5, new Vector2(-4.0766f, -8.7178f)),
        new Tuple<byte, Vector2>(5, new Vector2(2.9486f, 1.1347f)),
        new Tuple<byte, Vector2>(5, new Vector2(-4.2181f, -8.6795f)),
        new Tuple<byte, Vector2>(5, new Vector2(19.5553f, -12.5014f)),
        new Tuple<byte, Vector2>(5, new Vector2(15.2497f, -16.5009f)),
        new Tuple<byte, Vector2>(5, new Vector2(-22.7174f, -7.0523f)),
        new Tuple<byte, Vector2>(5, new Vector2(-16.5819f, -2.1575f)),
        new Tuple<byte, Vector2>(5, new Vector2(9.399f, -9.7127f)),
        new Tuple<byte, Vector2>(5, new Vector2(7.3723f, 1.7373f)),
        new Tuple<byte, Vector2>(5, new Vector2(22.0777f, -7.9315f)),
        new Tuple<byte, Vector2>(5, new Vector2(-15.3916f, -9.3659f)),
        new Tuple<byte, Vector2>(5, new Vector2(-16.1207f, -0.1746f)),
        new Tuple<byte, Vector2>(5, new Vector2(-23.1353f, -7.2472f)),
        new Tuple<byte, Vector2>(5, new Vector2(-20.0692f, -2.6245f)),
        new Tuple<byte, Vector2>(5, new Vector2(-4.2181f, -8.6795f)),
        new Tuple<byte, Vector2>(5, new Vector2(-9.9285f, 12.9848f)),
        new Tuple<byte, Vector2>(5, new Vector2(-8.3475f, 1.6215f)),
        new Tuple<byte, Vector2>(5, new Vector2(-17.7614f, 6.9115f)),
        new Tuple<byte, Vector2>(5, new Vector2(-0.5743f, -4.7235f)),
        new Tuple<byte, Vector2>(5, new Vector2(-20.8897f, 2.7606f))
    };

    public void RpcRequestRole(string roleName)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            if (PlayersWithRequestedRoles.All(x => x.Item1 != PlayerControl.LocalPlayer))
                PlayersWithRequestedRoles.Add(new Pair<PlayerControl, string>(PlayerControl.LocalPlayer, roleName));
        }
        else
        {
            var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.RequestRole);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(roleName);
            writer.EndMessage();
        }
    }

    public List<Vector2> GetAllApplicableItemPositions()
    {
        var positions = new List<Vector2>();
        foreach (var position in PossibleItemPositions)
            if (ShipStatus.Instance != null &&
                position.Item1 == GameOptionsManager.Instance.currentNormalGameOptions.MapId)
                positions.Add(position.Item2);
        return positions;
    }

    public ModdedPlayerClass ModdedPlayerById(byte id)
    {
        var matches = AllPlayers.FindAll(player => player._Object.PlayerId == id);
        return matches.FirstOrDefault();
    }

    public IEnumerator CoStunPlayer(PlayerControl player)
    {
        var durationText = TaskInfoHandler.Instance.AddNewItem(1, "");
        ;

        while (true)
        {
            bool isMeeting = MeetingHud.Instance;
            var isGameEnded = !AmongUsClient.Instance.IsGameStarted;

            if (player.AmOwner)
            {
                var roleColorHex = TaskInfoHandler.Instance.GetRoleHexColor(player);
                durationText.Text = string.Format(ModTranslation.getString("CoStunPlayerDurationText"), roleColorHex);
                player.lightSource.SetViewDistance(Mathf.Lerp(ShipStatus.Instance.MinLightRadius, ShipStatus.Instance.MaxLightRadius, 0) *
                    GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod);
                player.moveable = false;
                player.MyPhysics.body.velocity = Vector2.zero;
            }

            if (isMeeting || isGameEnded)
            {
                TaskInfoHandler.Instance.RemoveItem(durationText);
                player.moveable = true;
                yield break;
            }

            yield return null;
        }
    }

    public IEnumerator CoActivateButterBeer(PlayerControl player)
    {
        var now = DateTime.UtcNow;
        var durationText = TaskInfoHandler.Instance.AddNewItem(1, "");
        ;
        SetSpeedMultiplier(player.PlayerId, 2f);
        ModdedPlayerById(player.PlayerId).ReverseDirectionalControls = true;

        while (true)
        {
            bool isMeeting = MeetingHud.Instance;
            var isGameEnded = !AmongUsClient.Instance.IsGameStarted;
            var hasTimeExpired = now.AddSeconds(Config.BeerDuration) < DateTime.UtcNow;

            if (player.AmOwner)
            {
                var remainingTime = Math.Ceiling(Config.HourglassTimer - (float)(DateTime.UtcNow - now).TotalSeconds);
                var roleColorHex = TaskInfoHandler.Instance.GetRoleHexColor(player);
                durationText.Text = string.Format(ModTranslation.getString("CoActivateButterBeerDurationText"),
                    roleColorHex, remainingTime);
            }

            if (isMeeting || isGameEnded || hasTimeExpired)
            {
                TaskInfoHandler.Instance.RemoveItem(durationText);
                SetSpeedMultiplier(player.PlayerId, 1f);
                ModdedPlayerById(player.PlayerId).ReverseDirectionalControls = false;
                yield break;
            }

            yield return null;
        }
    }

    public IEnumerator CoActivateHourglass(PlayerControl player)
    {
        var now = DateTime.UtcNow;
        Vector2 startPosition = player.transform.position;
        var durationText = new ImportantTextTask();
        ModdedPlayerById(player.PlayerId).CanSeeAllRolesOveridden = true;
        if (player.AmOwner)
            durationText = TaskInfoHandler.Instance.AddNewItem(1,
                string.Format(ModTranslation.getString("CoActivateHourglassDurationText"),
                    TaskInfoHandler.Instance.GetRoleHexColor(player), Config.HourglassTimer));
        while (true)
        {
            if (player.AmOwner)
            {
                durationText.Text = string.Format(ModTranslation.getString("CoActivateHourglassDurationText"),
                    TaskInfoHandler.Instance.GetRoleHexColor(player),
                    Math.Ceiling(Config.HourglassTimer - (float)(DateTime.UtcNow - now).TotalSeconds));
                GetLocalModdedPlayer().Role?.ResetCooldowns();
            }

            if (MeetingHud.Instance ||
                AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
                now.AddSeconds(Config.HourglassTimer) < DateTime.UtcNow)
            {
                TaskInfoHandler.Instance.RemoveItem(durationText);

                if (ModdedPlayerById(player.PlayerId).KilledByCurse)
                    yield break;

                if (player.Data.IsDead)
                    RpcRevivePlayer(player);

                if (MeetingHud.Instance)
                    yield break;

                RpcTeleportPlayer(player, startPosition);
                ModdedPlayerById(player.PlayerId).CanSeeAllRolesOveridden = false;
                yield break;
            }

            yield return null;
        }
    }

    public void SpawnItem(int id, Vector2 pos, Vector2 vel)
    {
        switch (id)
        {
            case 0:
                var deluminator = new DeluminatorWorld(pos);
                AllItems.Add(deluminator);
                break;
            case 1:
                var map = new MaraudersMapWorld(pos);
                AllItems.Add(map);
                break;
            case 2:
                var key = new PortKeyWorld(pos);
                AllItems.Add(key);
                break;
            case 3:
                var snitch = new TheGoldenSnitchWorld(pos, vel);
                AllItems.Add(snitch);
                break;
            case 4:
                var ghostStone = new GhostStoneWorld(pos);
                AllItems.Add(ghostStone);
                break;
            case 5:
                var butterBeer = new ButterBeerWorld(pos);
                AllItems.Add(butterBeer);
                break;
            case 6:
                var elderWand = new ElderWandWorld(pos);
                AllItems.Add(elderWand);
                break;
            case 7:
                var basItem = new BasWorldItem(pos);
                AllItems.Add(basItem);
                break;
            case 8:
                var sortingHat = new SortingHatWorld(pos);
                AllItems.Add(sortingHat);
                break;
            case 9:
                var philoStone = new PhiloStoneWorld(pos);
                AllItems.Add(philoStone);
                break;
        }
    }

    public void SetNameColor(PlayerControl player, Color color)
    {
        player.cosmetics.nameText.color = color;
        if (DestroyableSingleton<HudManager>.Instance && DestroyableSingleton<HudManager>.Instance.Chat)
            foreach (var bubble in DestroyableSingleton<HudManager>.Instance.Chat.chatBubblePool.activeChildren)
                if (bubble.Cast<ChatBubble>().NameText.text == player.cosmetics.nameText.text)
                    bubble.Cast<ChatBubble>().NameText.color = color;
        if (MeetingHud.Instance && MeetingHud.Instance.playerStates != null)
            foreach (var voteArea in MeetingHud.Instance.playerStates)
                if (voteArea.TargetPlayerId == player.PlayerId)
                    voteArea.NameText.color = color;
    }

    public void RpcFakeKill(PlayerControl target)
    {
        Coroutines.Start(CoFakeKill(target));

        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.FakeKill);
        writer.Write(target.PlayerId);
        writer.EndMessage();
    }

    public IEnumerator CoFakeKill(PlayerControl target)
    {
        var now = DateTime.UtcNow;
        var bodyObject = GameObject.Find("body_" + target.PlayerId);

        while (true)
        {
            bool isMeeting = MeetingHud.Instance;
            var isGameEnded = !AmongUsClient.Instance.IsGameStarted;
            var hasTimeExpired = now.AddSeconds(4) < DateTime.UtcNow;

            target.MyPhysics.body.velocity = Vector2.zero;
            target.moveable = false;
            target.Visible = false;
            target.Collider.enabled = false;

            if (isMeeting || isGameEnded || hasTimeExpired)
            {
                if (target.AmOwner)
                {
                    GetLocalModdedPlayer().Inventory.Find(x => x.Id == 9).Delete();
                    PopupTMPHandler.Instance.CreatePopup(ModTranslation.getString("CoFakeKillPopup"), Color.white,
                        Color.black);
                }

                target.moveable = true;
                target.Visible = true;
                target.Collider.enabled = true;

                bodyObject.Destroy();

                yield break;
            }

            yield return null;
        }
    }

    public void GiveGrabbedItem(int id)
    {
        if (!GetLocalModdedPlayer().HasItem(id))
            GetLocalModdedPlayer().GiveItem(id);
        var allMatches = AllItems.FindAll(x => x.Id == id);
        foreach (var item in allMatches)
            item.Delete();
    }

    public void RpcSpawnItem(int id, Vector2 pos)
    {
        var x = UnityEngine.Random.Range(-1.2f, 1.2f);
        var y = UnityEngine.Random.Range(-1.2f, 1.2f);
        var velocity = new Vector2(x, y);
        SpawnItem(id, pos, velocity);

        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.SpawnItem);
        writer.Write(id);
        writer.Write(pos.x);
        writer.Write(pos.y);
        writer.Write(velocity.x);
        writer.Write(velocity.y);
        writer.EndMessage();
    }

    public void RpcRevealRole(byte playerId)
    {
        if (!MeetingHud.Instance) return;

        foreach (var voteArea in MeetingHud.Instance.playerStates)
        {
            var child = voteArea.Buttons.transform.GetChild(voteArea.Buttons.transform.childCount - 1);
            if (child.gameObject.name != "SortButton") child.gameObject.Destroy();
        }

        GetLocalModdedPlayer().Inventory.Find(x => x.Id == 8).Delete();
        RevealRole(playerId);

        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.RevealRole);
        writer.Write(playerId);
        writer.EndMessage();
    }

    public void RevealRole(byte playerId)
    {
        if (!MeetingHud.Instance) return;

        foreach (var voteArea in MeetingHud.Instance.playerStates)
        {
            if (voteArea.TargetPlayerId != playerId) continue;
            var targetPlayer = ModdedPlayerById(playerId);

            PopupTMPHandler.Instance.CreatePopup(
                string.Format(ModTranslation.getString("RevealRolePopup"), targetPlayer._Object.Data.PlayerName),
                Color.white, Color.black);

            var roleHex = TaskInfoHandler.Instance.GetRoleHexColor(targetPlayer._Object);
            var playerName = targetPlayer._Object.Data.PlayerName;
            var roleName = GetPlayerRoleName(targetPlayer);
            voteArea.NameText.text = $"{roleHex}{playerName}\n{roleName}";
            voteArea.NameText.fontSize = 1.8f;
            voteArea.NameText.enableAutoSizing = false;
        }
    }

    public void RpcRevivePlayer(PlayerControl player)
    {
        player.ModRevive();
        foreach (var body in Object.FindObjectsOfType<DeadBody>())
            if (body.ParentId == player.PlayerId)
                Object.Destroy(body.gameObject);

        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
            (byte)Packets.RevivePlayer);
        writer.Write(player.PlayerId);
        writer.EndMessage();
    }

    public void RpcTeleportPlayer(PlayerControl player, Vector2 position)
    {
        player.NetTransform.SnapTo(position);

        var posWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
            (byte)Packets.TeleportPlayer);
        posWriter.Write(player.PlayerId);
        posWriter.Write(position.x);
        posWriter.Write(position.y);
        posWriter.EndMessage();
    }

    public void UseHourglass(PlayerControl player)
    {
        Coroutines.Start(CoActivateHourglass(player));
    }

    public IEnumerator CoDefensiveDuelist(PlayerControl player)
    {
        var now = DateTime.UtcNow;
        var startingPosition = player.transform.position;
        ModdedPlayerById(player.PlayerId).Immortal = true;
        var durationText = new ImportantTextTask();
        if (player.AmOwner)
            durationText = TaskInfoHandler.Instance.AddNewItem(1,
                string.Format(ModTranslation.getString("CoDefensiveDuelistDurationText"),
                    TaskInfoHandler.Instance.GetRoleHexColor(player), Config.DefensiveDuelistDuration));
        while (true)
        {
            if (MeetingHud.Instance ||
                AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
                now.AddSeconds(Config.DefensiveDuelistDuration) < DateTime.UtcNow ||
                ModdedPlayerById(player.PlayerId).ControllerOverride != null)
            {
                ModdedPlayerById(player.PlayerId).Immortal = false;
                TaskInfoHandler.Instance.RemoveItem(durationText);
                yield break;
            }

            if (player.AmOwner)
            {
                player.transform.position = startingPosition;
                player.MyPhysics.body.velocity = new Vector2(0, 0);
                durationText.Text = string.Format(ModTranslation.getString("CoDefensiveDuelistDurationText"),
                    TaskInfoHandler.Instance.GetRoleHexColor(player),
                    Math.Ceiling(Config.DefensiveDuelistDuration - (float)(DateTime.UtcNow - now).TotalSeconds));
                GetLocalModdedPlayer().Role?.ResetCooldowns();
            }

            yield return null;
        }
    }

    public void DefensiveDuelist(PlayerControl player)
    {
        Coroutines.Start(CoDefensiveDuelist(player));
    }

    public void RpcDefensiveDuelist(PlayerControl player)
    {
        DefensiveDuelist(player);
        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.DefensiveDuelist);
        writer.Write(player.PlayerId);
        writer.EndMessage();
    }

    public static void setInvisible(PlayerControl player, Color color, float alpha)
    {
        if (player.cosmetics.currentBodySprite.BodySprite != null)
            player.cosmetics.currentBodySprite.BodySprite.color = color;

        if (player.cosmetics.skin?.layer != null)
            player.cosmetics.skin.layer.color = color;

        if (player.cosmetics.hat != null)
        {
            player.cosmetics.hat.FrontLayer.color = color;
            player.cosmetics.hat.BackLayer.color = color;
        }

        if (player.cosmetics.currentPet != null)
            player.cosmetics.currentPet.SetAlpha(alpha);

        if (player.cosmetics.visor != null)
            player.cosmetics.visor.Image.color = color;

        if (player.cosmetics.colorBlindText != null)
            player.cosmetics.colorBlindText.color = color;

        if (player.cosmetics.PettingHand != null)
            player.cosmetics.PettingHand.SetAlpha(alpha);
    }

    public IEnumerator CoInvisPlayer(PlayerControl target)
    {
        var now = DateTime.UtcNow;
        var durationText = new ImportantTextTask();
        if (target.AmOwner)
            durationText = TaskInfoHandler.Instance.AddNewItem(1,
                string.Format(ModTranslation.getString("CoInvisPlayerDurationText"),
                    TaskInfoHandler.Instance.GetRoleHexColor(target), Config.InvisCloakDuration));

        while (true)
        {
            if (target.AmOwner)
            {
                durationText.Text = string.Format(ModTranslation.getString("CoInvisPlayerDurationText"),
                    TaskInfoHandler.Instance.GetRoleHexColor(target),
                    Math.Ceiling(Config.InvisCloakDuration - (float)(DateTime.UtcNow - now).TotalSeconds));
                GetLocalModdedPlayer().Role?.ResetCooldowns();

                target.Visible = true;

                setInvisible(target, new Color(1f, 1f, 1f, 100f / 255f), 0.0f);
            }
            else
            {
                target.Visible = false;

                if (target.cosmetics.currentPet)
                    target.cosmetics.currentPet.Visible = false;
            }

            if (MeetingHud.Instance ||
                AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
                now.AddSeconds(Config.InvisCloakDuration) < DateTime.UtcNow ||
                ModdedPlayerById(target.PlayerId).ControllerOverride != null)
            {
                target.cosmetics.currentBodySprite.BodySprite.color = Color.white;
                target.cosmetics.hat.SpriteColor = Color.white;
                target.cosmetics.skin.GetComponent<SpriteRenderer>().color = Color.white;

                if (target.cosmetics.currentPet)
                {
                    target.cosmetics.currentPet.GetComponent<SpriteRenderer>().color = Color.white;
                    target.cosmetics.currentPet.Visible = true;
                }

                target.Visible = true;
                TaskInfoHandler.Instance.RemoveItem(durationText);
                yield break;
            }

            yield return null;
        }
    }

    public IEnumerator CoBlindPlayer(PlayerControl target)
    {
        var now = DateTime.UtcNow;
        var num = 0f;
        target.MyPhysics.body.velocity = new Vector2(0, 0);
        var durationText = new ImportantTextTask();
        if (target.AmOwner)
        {
            PopupTMPHandler.Instance.CreatePopup(ModTranslation.getString("CoBlindPlayerPopup"), Color.white,
                Color.black);
            durationText = TaskInfoHandler.Instance.AddNewItem(1,
                string.Format(ModTranslation.getString("CoBlindPlayerDurationText"),
                    TaskInfoHandler.Instance.GetRoleHexColor(target), Config.CrucioDuration));
        }

        while (true)
        {
            if (target.AmOwner)
            {
                durationText.Text = string.Format(ModTranslation.getString("CoBlindPlayerDurationText"),
                    TaskInfoHandler.Instance.GetRoleHexColor(target),
                    Math.Ceiling(Config.CrucioDuration - (float)(DateTime.UtcNow - now).TotalSeconds));
                target.lightSource.SetViewDistance(Mathf.Lerp(ShipStatus.Instance.MinLightRadius, ShipStatus.Instance.MaxLightRadius, num) *
                    GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod);
                target.moveable = false;
            }

            if (MeetingHud.Instance ||
                AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
                now.AddSeconds(Config.CrucioDuration) < DateTime.UtcNow ||
                ModdedPlayerById(target.PlayerId).ControllerOverride != null)
            {
                target.moveable = true;
                TaskInfoHandler.Instance.RemoveItem(durationText);
                yield break;
            }

            yield return null;
        }
    }

    public IEnumerator CoControlPlayer(PlayerControl controller, PlayerControl target)
    {
        var now = DateTime.UtcNow;
        ControlKillUsed = false;

        Instance.ModdedPlayerById(target.PlayerId).ControllerOverride =
            Instance.ModdedPlayerById(controller.PlayerId);

        ((Bellatrix)Instance.ModdedPlayerById(controller.PlayerId).Role).MindControlledPlayer =
            Instance.ModdedPlayerById(target.PlayerId);

        ImportantTextTask durationText = null;

        if (controller.AmOwner)
        {
            target.MyPhysics.body.interpolation = RigidbodyInterpolation2D.Interpolate;
            Camera.main.GetComponent<FollowerCamera>().Target = target;
            Camera.main.GetComponent<FollowerCamera>().shakeAmount = 0;
            durationText = TaskInfoHandler.Instance.AddNewItem(1,
                string.Format(ModTranslation.getString("CoControlPlayerDurationText"),
                    TaskInfoHandler.Instance.GetRoleHexColor(controller), target.Data.PlayerName,
                    Config.ImperioDuration));
        }

        if (target.AmOwner)
            PopupTMPHandler.Instance.CreatePopup(ModTranslation.getString("CoControlPlayerPopup"), Color.white,
                Color.black);

        target.moveable = true;
        controller.moveable = true;

        if (target.AmOwner || controller.AmOwner)
            PlayerControl.LocalPlayer.MyPhysics.body.velocity = new Vector2(0, 0);

        while (true)
        {
            if (target.Data.IsDead ||
                MeetingHud.Instance ||
                AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
                now.AddSeconds(Config.ImperioDuration) < DateTime.UtcNow)
            {
                if (target.AmOwner)
                {
                    target.moveable = true;
                }
                else if (controller.AmOwner)
                {
                    target.MyPhysics.body.interpolation = RigidbodyInterpolation2D.None;
                    TaskInfoHandler.Instance.RemoveItem(durationText);
                    controller.moveable = true;
                    Camera.main.GetComponent<FollowerCamera>().Target = controller;
                    controller.lightSource.transform.position = controller.transform.position;
                    PlayerControl.LocalPlayer.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions
                        .KillCooldown);
                }

                Instance.ModdedPlayerById(target.PlayerId).ControllerOverride = null;
                ((Bellatrix)Instance.ModdedPlayerById(controller.PlayerId).Role).MindControlledPlayer = null;
                yield break;
            }

            if (controller.AmOwner || target.AmOwner)
            {
                if (Minigame.Instance)
                    Minigame.Instance.Close();
                PlayerControl.LocalPlayer.moveable = false;
                if (controller.AmOwner)
                {
                    durationText.Text =
                        string.Format(ModTranslation.getString("CoControlPlayerDurationText"),
                            TaskInfoHandler.Instance.GetRoleHexColor(controller), target.Data.PlayerName,
                            Math.Ceiling(Config.ImperioDuration - (float)(DateTime.UtcNow - now).TotalSeconds));
                    controller.lightSource.transform.position = target.transform.position;
                    DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(0f,
                        GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);

                    if (Input.GetKeyDown(KeyCode.Q))
                        if (target.FindClosestTarget() != null && !ControlKillUsed)
                        {
                            ControlKillUsed = true;
                            RpcKillPlayer(target, target.FindClosestTarget());
                        }

                    if (ControlKillUsed)
                        DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                    else
                        DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target.FindClosestTarget());
                }
            }

            yield return null;
        }
    }

    public void SetSpeedMultiplier(byte playerId, float newSpeed)
    {
        ModdedPlayerById(playerId).SpeedMultiplier = newSpeed;

        var writer =
            AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.UpdateSpeedMultiplier);
        writer.Write(playerId);
        writer.Write(newSpeed);
        writer.EndMessage();
    }

    public void ControlPlayer(PlayerControl controller, PlayerControl target)
    {
        target.MyPhysics.body.velocity = Vector2.zero;
        controller.MyPhysics.body.velocity = Vector2.zero;
        KillAnimation.SetMovement(controller, false);
        KillAnimation.SetMovement(controller, true);
        KillAnimation.SetMovement(target, false);
        KillAnimation.SetMovement(target, true);
        Coroutines.Start(CoControlPlayer(controller, target));
    }

    public void RpcControlPlayer(PlayerControl controller, PlayerControl target)
    {
        ControlPlayer(controller, target);
        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.StartControlling);
        writer.Write(controller.PlayerId);
        writer.Write(target.PlayerId);
        writer.EndMessage();
    }

    public void InvisPlayer(PlayerControl target)
    {
        Coroutines.Start(CoInvisPlayer(target));
    }

    public void RpcInvisPlayer(PlayerControl target)
    {
        InvisPlayer(target);
        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.InvisPlayer);
        writer.Write(target.PlayerId);
        writer.EndMessage();
    }

    public void CrucioBlind(PlayerControl target)
    {
        System.Console.WriteLine("Blinding " + target.Data.PlayerName);
        Coroutines.Start(CoBlindPlayer(target));
    }

    public void RpcCrucioBlind(PlayerControl target)
    {
        CrucioBlind(target);
        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.DeactivatePlayer);
        writer.Write(target.PlayerId);
        writer.EndMessage();
    }

    public void RpcKillPlayer(PlayerControl killer, PlayerControl target, bool isCurseKill = true,
        bool forceShowAnim = false)
    {
        KillPlayer(killer, target, isCurseKill, forceShowAnim);
        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.KillPlayerUnsafe);
        writer.Write(killer.PlayerId);
        writer.Write(target.PlayerId);
        writer.Write(isCurseKill);
        writer.Write(forceShowAnim);
        writer.EndMessage();
    }

    public void KillPlayer(PlayerControl killer, PlayerControl target, bool isCurseKill, bool forceShowAnim)
    {
        if (MeetingHud.Instance || AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
            return;

        if (!target.Collider.enabled)
            return;

        if (ModdedPlayerById(target.PlayerId).Immortal)
        {
            if (killer.AmOwner)
            {
                PopupTMPHandler.Instance.CreatePopup(ModTranslation.getString("KillPlayerPopup"), Color.white,
                    Color.black);
                killer.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
                ModdedPlayerById(killer.PlayerId).Role?.ResetCooldowns();
            }

            return;
        }

        if (target.AmOwner || killer.AmOwner ||
            ModdedPlayerById(killer.PlayerId).ControllerOverride?._Object.AmOwner == true)
        {
            System.Console.WriteLine("Trying to play the kill sound!");
            SoundManager.Instance.StopSound(PlayerControl.LocalPlayer.KillSfx);
            SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false, 0.8f);
        }

        if (!isCurseKill)
        {
            killer.MyPhysics.body.transform.position = target.transform.position;
        }
        else
        {
            ModdedPlayerById(target.PlayerId).KilledByCurse = true;
            if (target.AmOwner && killer.AmOwner)
            {
                PopupTMPHandler.Instance.CreatePopup(ModTranslation.getString("KillPlayerPopup1"), Color.white,
                    Color.black, 3f);
                DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(killer.Data, killer.Data);
            }
            else if (target.AmOwner)
            {
                PopupTMPHandler.Instance.CreatePopup(ModTranslation.getString("KillPlayerPopup2"), Color.white,
                    Color.black);
            }
        }

        /*DeadBody deadBody = DeadBody.Instantiate(GameManager.Instance.DeadBodyPrefab);
        SpriteRenderer rend = deadBody.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
        rend.color = target.Data.Color;
        deadBody.gameObject.name = "body_" + target.PlayerId;
        Vector3 vector = target.transform.position + target.KillAnimations[0].BodyOffset;
        vector.z = vector.y / 1000f;
        deadBody.transform.position = vector;
        deadBody.ParentId = target.PlayerId;
        target.SetPlayerMaterialColors(deadBody.GetComponent<Renderer>());*/
        target.MyPhysics.StartCoroutine(target.KillAnimations.First().CoPerformKill(target, target));

        if (target.AmOwner)
        {
            if (forceShowAnim)
                DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(killer.Data, target.Data);

            if (ModdedPlayerById(target.PlayerId).ShouldRevive)
            {
                RpcFakeKill(target);
            }
            else
            {
                PlayerDie(target);

                var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.FinallyDie);
                writer.Write(target.PlayerId);
                writer.EndMessage();
            }
        }
    }

    public void PlayerDie(PlayerControl target)
    {
        if (target.AmOwner)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(true);
            DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
            AmongUsClient.Instance.gameObject.layer = LayerMask.NameToLayer("Ghost");
        }

        if (target.cosmetics.currentPet)
            target.cosmetics.currentPet.SetMourning();

        target.Data.IsDead = true;
        target.cosmetics.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
        target.gameObject.layer = LayerMask.NameToLayer("Ghost");
    }

    public void RpcForceAllVotes(byte playerId)
    {
        GetLocalModdedPlayer().Inventory.Find(x => x.Id == 3).Delete();
        ForceAllVotes(playerId);
        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.ForceAllVotes);
        writer.Write(playerId);
        writer.EndMessage();
    }

    public void ForceAllVotes(byte playerId)
    {
        PopupTMPHandler.Instance.CreatePopup(ModTranslation.getString("ForceAllVotesPopup"), Color.white, Color.black);

        foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                SoundManager.Instance.PlaySound(MeetingHud.Instance.VoteLockinSound, false);

                playerVoteArea.voteComplete = true;
                playerVoteArea.VotedFor = playerId;
                playerVoteArea.Flag.enabled = true;

                MeetingHud.Instance.SetDirtyBit(
                    1U << MeetingHud.Instance.playerStates.IndexOf(playerVoteArea));
                MeetingHud.Instance.CheckForEndVoting();
            }

            if (playerVoteArea.TargetPlayerId == playerId)
            {
                var confirmButton = playerVoteArea.Buttons.GetComponentsInChildren<PassiveButton>()
                    .FirstOrDefault();
                var snitchIco = new GameObject();
                var snitchIcoR = snitchIco.AddComponent<SpriteRenderer>();
                snitchIco.layer = 5;
                snitchIcoR.sprite = Assets.SmallSnitchSprite;
                snitchIco.transform.SetParent(playerVoteArea.transform);
                snitchIco.transform.localPosition = new Vector3(2.8f, 0.01f);
                snitchIco.transform.localScale = confirmButton.transform.localScale;
            }

            playerVoteArea.ClearButtons();
            playerVoteArea.voteComplete = true;
        }

        MeetingHud.Instance.SkipVoteButton.ClearButtons();
        MeetingHud.Instance.SkipVoteButton.voteComplete = true;
        MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(false);
    }

    public bool IsLightsSabotaged()
    {
        foreach (var task in PlayerControl.LocalPlayer.myTasks)
            if (task.TaskType == TaskTypes.FixLights)
                return true;
        return false;
    }

    public void CreateCrucio(Vector2 pos, ModdedPlayerClass owner)
    {
        var crucioObject = new GameObject();
        crucioObject.name = "_crucio";

        var crucio = crucioObject.AddComponent<Spell>();
        crucio.Owner = owner;
        crucio.MousePostition = pos;
        crucio.SpellSprites = Assets.CrucioSprite.ToArray();
        crucio.OnHit += (spell, player) =>
        {
            var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.DestroyCrucio);
            writer.EndMessage();

            crucioObject.Destroy();

            if (player == null) return;
            if (((Bellatrix)owner.Role).MarkedPlayers.All(x => x.PlayerId != player.PlayerId)) return;
            var moddedPlayer = ModdedPlayerById(player.PlayerId);
            if (moddedPlayer.Immortal) return;
            RpcCrucioBlind(player);
        };
    }

    public void RpcCreateCrucio(Vector2 pos, ModdedPlayerClass owner)
    {
        CreateCrucio(pos, owner);

        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.CreateCrucio);
        writer.Write(owner._Object.PlayerId);
        writer.Write(pos.x);
        writer.Write(pos.y);
        writer.EndMessage();
    }

    public void CreateCurse(Vector2 pos, ModdedPlayerClass owner)
    {
        var curseObject = new GameObject();
        curseObject.name = "_curse";

        var curse = curseObject.AddComponent<Spell>();
        curse.Owner = owner;
        curse.MousePostition = pos;
        curse.SpellSprites = Assets.CurseSprite.ToArray();
        curse.OnHit += (spell, player) =>
        {
            var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.DestroyCurse);
            writer.EndMessage();

            curseObject.Destroy();

            if (player == null) return;
            if (ModdedPlayerById(player.PlayerId).Immortal) return;

            if (GetPlayerRoleName(ModdedPlayerById(player.PlayerId)) == "Harry")
            {
                RpcKillPlayer(owner._Object, owner._Object);
                return;
            }

            RpcKillPlayer(owner._Object, player);
        };
    }

    public void RpcCreateCurse(Vector2 pos, ModdedPlayerClass owner)
    {
        CreateCurse(pos, owner);

        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.CreateCurse);
        writer.Write(owner._Object.PlayerId);
        writer.Write(pos.x);
        writer.Write(pos.y);
        writer.EndMessage();
    }

    public void DestroySpell(string name)
    {
        GameObject.Find(name)?.Destroy();
    }

    public void RpcAssignRole(ModdedPlayerClass player, Role role)
    {
        AssignRole(player, role);

        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.AssignRole);
        writer.Write(player._Object.PlayerId);
        writer.Write(role.RoleName);
        writer.EndMessage();
    }

    public void AssignRole(ModdedPlayerClass player, Role role)
    {
        player.Role = role;
    }

    public ModdedPlayerClass GetLocalModdedPlayer()
    {
        var matches = AllPlayers.FindAll(player => player._Object == PlayerControl.LocalPlayer);
        return matches.FirstOrDefault();
    }

    public ModdedPlayerClass FindPlayerOfRole(string roleName)
    {
        var matches = AllPlayers.FindAll(player => player.Role != null && player.Role.RoleName == roleName);
        return matches.FirstOrDefault();
    }

    public bool IsPlayerRole(ModdedPlayerClass player, string roleName)
    {
        if (player?.Role?.RoleName == roleName)
            return true;
        return false;
    }

    public string GetPlayerRoleName(ModdedPlayerClass player)
    {
        if (player == null) return "Null";
        if (player.Role == null)
            return player._Object.Data.Role.IsImpostor
                ? ModTranslation.getString("TeamImpostor")
                : ModTranslation.getString("TeamMuggle");
        return player.Role.RoleName;
    }

    public PlayerControl GetClosestTarget(PlayerControl player, bool excludeImp, PlayerControl[] exclusions = null)
    {
        PlayerControl result = null;
        var num = LegacyGameOptions.KillDistances[
            Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
        if (!ShipStatus.Instance) return null;
        var truePosition = player.GetTruePosition();
        var allPlayers = GameData.Instance.AllPlayers.ToArray().ToList();
        for (var i = 0; i < allPlayers.Count; i++)
        {
            var playerInfo = allPlayers[i];
            if (!playerInfo.Disconnected && playerInfo.PlayerId != player.PlayerId && !playerInfo.IsDead &&
                (!playerInfo.Role.IsImpostor || !excludeImp) &&
                (exclusions == null || !exclusions.Any(x => x.PlayerId == playerInfo.PlayerId)))
            {
                var @object = playerInfo.Object;
                if (@object && @object.Collider.enabled)
                {
                    var vector = @object.GetTruePosition() - truePosition;
                    var magnitude = vector.magnitude;
                    if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized,
                            magnitude, Constants.ShipAndObjectsMask))
                    {
                        result = @object;
                        num = magnitude;
                    }
                }
            }
        }

        return result;
    }

    /*public string GetTooltipByOptionName(string name)
    {
        switch (name)
        {
            case "Order of the Impostors":
                return "If 'On', when Harry, Ron, and Hermione are dead, Impostors will win";
            case "Can Spells be Used In Vents":
                return "When 'On', spells can be casted from inside vents";
            case "Show Info Popups/Tooltips":
                return "When 'On', informational popups/tooltips will be shown";
            case "Defensive Duelist Cooldown":
                return "The cooldown for Ron's main ability";
            case "Invisibility Cloak Cooldown":
                return "The cooldown for Harry's main ability";
            case "Time Turner Cooldown":
                return "The cooldown for Hermione's main ability";
            case "Crucio Cooldown":
                return "The cooldown for Bellatrix's secondary ability";
            case "Shared Voldemort Cooldowns":
                return "When 'On', the Kill button and the Curse button will share a cooldown";
        }
        return "No tooltip was supplied.";
    }*/
}
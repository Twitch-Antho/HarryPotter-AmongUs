using HarryPotter.CustomOption;
using System;

namespace HarryPotter.Classes;

internal class Config
{
    private static CustomHeaderOption roleSettings;
    private static CustomNumberOption BellatrixOn;
    private static CustomNumberOption HarryOn;
    private static CustomNumberOption HermioneOn;
    private static CustomNumberOption RonOn;
    private static CustomNumberOption VoldemortOn;

    private static CustomHeaderOption HPSettings;
    private static CustomToggleOption enableModRole;
    private static CustomToggleOption spellsInVents;
    private static CustomToggleOption randomGameStartPosition;

    private static CustomHeaderOption ItemSettings;
    private static CustomNumberOption beerDuration;
    private static CustomNumberOption mapDuration;

    private static CustomHeaderOption VoldemortSettings;
    private static CustomToggleOption orderOfTheImp;
    private static CustomToggleOption separateCooldowns;
    private static CustomNumberOption crucioCooldown;
    private static CustomNumberOption crucioDuration;
    private static CustomHeaderOption BellatrixSettings;
    private static CustomNumberOption imperioDuration;
    private static CustomHeaderOption RonSettings;
    private static CustomNumberOption defensiveDuelistCooldown;
    private static CustomNumberOption defensiveDuelistDuration;
    private static CustomHeaderOption HarrySettings;
    private static CustomNumberOption invisCloakCooldown;
    private static CustomNumberOption invisCloakDuration;
    private static CustomHeaderOption HermioneSettings;
    private static CustomNumberOption hourglassCooldown;
    private static CustomNumberOption hourglassTimer;


    public int EnableVoldemort { get; private set; }
    public int EnableBellatrix { get; private set; }
    public int EnableHarry { get; private set; }
    public int EnableHermione { get; private set; }
    public int EnableRon { get; private set; }

    public bool OrderOfTheImp { get; private set; }
    public float MapDuration { get; private set; }
    public float DefensiveDuelistDuration { get; private set; }
    public float InvisCloakDuration { get; private set; }
    public float HourglassTimer { get; private set; }
    public float BeerDuration { get; private set; }
    public float CrucioDuration { get; private set; }
    public float DefensiveDuelistCooldown { get; private set; }
    public float InvisCloakCooldown { get; private set; }
    public float HourglassCooldown { get; private set; }
    public float CrucioCooldown { get; private set; }
    public bool SpellsInVents { get; private set; }
    public float ImperioDuration { get; private set; }
    public bool SeparateCooldowns { get; private set; }
    public bool EnableModRole { get; private set; }
    public bool RandomGameStartPosition { get; private set; }
    public static Func<object, string> PercentFormat { get; } = value => $"{value:0}%";
    private static Func<object, string> CooldownFormat { get; } = value => $"{value:0.0#}s";
    private static Func<object, string> MultiplierFormat { get; } = value => $"{value:0.0#}x";


    public static void LoadOption()
    {
        var num = 0;
        HPSettings = new CustomHeaderOption(num++, "HPSettings");
        enableModRole = new CustomToggleOption(num++, "enableModRole");
        orderOfTheImp = new CustomToggleOption(num++, "orderOfTheImp", false);
        randomGameStartPosition = new CustomToggleOption(num++, "randomGameStartPosition");

        roleSettings = new CustomHeaderOption(num++, "roleSettings");
        VoldemortOn = new CustomNumberOption(num++, ModHelpers.cs(Palette.ImpostorRed, "RoleNameVoldemort"), 0f, 0f, 100f, 10f, PercentFormat);
        BellatrixOn = new CustomNumberOption(num++, ModHelpers.cs(Palette.ImpostorRed, "RoleNameBellatrix"), 0f, 0f, 100f, 10f, PercentFormat);
        HarryOn = new CustomNumberOption(num++, ModHelpers.cs(Palette.Orange, "RoleNameHarry"), 0f, 0f, 100f, 10f, PercentFormat);
        HermioneOn = new CustomNumberOption(num++, ModHelpers.cs(Palette.Orange, "RoleNameHermione"), 0f, 0f, 100f, 10f, PercentFormat);
        RonOn = new CustomNumberOption(num++, ModHelpers.cs(Palette.Orange, "RoleNameRon"), 0f, 0f, 100f, 10f, PercentFormat);

        VoldemortSettings = new CustomHeaderOption(num++, ModHelpers.cs(Palette.ImpostorRed, "RoleNameVoldemort"));
        spellsInVents = new CustomToggleOption(num++, "spellsInVents", false);
        separateCooldowns = new CustomToggleOption(num++, "separateCooldowns");

        BellatrixSettings = new CustomHeaderOption(num++, ModHelpers.cs(Palette.ImpostorRed, "RoleNameBellatrix"));
        imperioDuration = new CustomNumberOption(num++, "imperioDuration", 5f, 5f, 30f, 1f, CooldownFormat);
        crucioCooldown = new CustomNumberOption(num++, "crucioCooldown", 20f, 40f, 10f, 2.5f, CooldownFormat);
        crucioDuration = new CustomNumberOption(num++, "crucioDuration", 5f, 5f, 30f, 1f, CooldownFormat);

        RonSettings = new CustomHeaderOption(num++, ModHelpers.cs(Palette.Orange, "RoleNameRon"));
        defensiveDuelistCooldown = new CustomNumberOption(num++, "defensiveDuelistCooldown", 20f, 40f, 10, 2.5f, CooldownFormat);
        defensiveDuelistDuration = new CustomNumberOption(num++, "defensiveDuelistDuration", 5f, 5f, 30f, 1f, CooldownFormat);

        HarrySettings = new CustomHeaderOption(num++, ModHelpers.cs(Palette.Orange, "RoleNameHarry"));
        invisCloakCooldown = new CustomNumberOption(num++, "invisCloakCooldown", 20f, 40f, 10, 2.5f, CooldownFormat);
        invisCloakDuration = new CustomNumberOption(num++, "invisCloakDuration", 5f, 5f, 30f, 1f, CooldownFormat);

        HermioneSettings = new CustomHeaderOption(num++, ModHelpers.cs(Palette.Orange, "RoleNameHermione"));
        hourglassCooldown = new CustomNumberOption(num++, "hourglassCooldown", 20f, 40f, 10f, 2.5f, CooldownFormat);
        hourglassTimer = new CustomNumberOption(num++, "hourglassTimer", 5f, 5f, 30f, 1f, CooldownFormat);


        ItemSettings = new CustomHeaderOption(num++, "ItemSettings");

        beerDuration = new CustomNumberOption(num++, "beerDuration", 5f, 5f, 30f, 1f, CooldownFormat);
        mapDuration = new CustomNumberOption(num++, "mapDuration", 5f, 5f, 30f, 1f, CooldownFormat);
    }

    public void ReloadSettings()
    {
        EnableModRole = enableModRole.Get();

        EnableVoldemort = (int)VoldemortOn.Get();
        EnableBellatrix = (int)BellatrixOn.Get();
        EnableHarry = (int)HarryOn.Get();
        EnableHermione = (int)HermioneOn.Get();
        EnableRon = (int)RonOn.Get();


        OrderOfTheImp = orderOfTheImp.Get();
        SpellsInVents = spellsInVents.Get();
        DefensiveDuelistCooldown = defensiveDuelistCooldown.Get();
        InvisCloakCooldown = invisCloakCooldown.Get();
        HourglassCooldown = hourglassCooldown.Get();
        CrucioCooldown = crucioCooldown.Get();
        SeparateCooldowns = !separateCooldowns.Get();
        ImperioDuration = imperioDuration.Get();
        RandomGameStartPosition = randomGameStartPosition.Get();
        MapDuration = mapDuration.Get();
        DefensiveDuelistDuration = defensiveDuelistDuration.Get();
        InvisCloakDuration = invisCloakDuration.Get();
        HourglassTimer = hourglassTimer.Get();
        BeerDuration = beerDuration.Get();
        CrucioDuration = crucioDuration.Get();
    }
}
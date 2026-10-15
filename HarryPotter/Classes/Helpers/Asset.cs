using System.Collections.Generic;
using System.Reflection;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace HarryPotter.Classes;

internal class Asset
{
    public Asset()
    {
        var resourceAssetBundleStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("HarryPotter.Resources.harrypotter");
        var bundle = AssetBundle.LoadFromMemory(resourceAssetBundleStream.ReadFully());

        ItemIcons = new List<Sprite>();
        AbilityIcons = new List<Sprite>();
        WorldItemIcons = new List<Sprite>();
        CrucioSprite = new List<Sprite>();
        CurseSprite = new List<Sprite>();

        lanuageJsonAsset = bundle.LoadAsset<TextAsset>("stringData").DontUnload();

        AbilityIcons.Add(bundle.LoadAsset<Sprite>("CurseButton").DontUnload());
        AbilityIcons.Add(bundle.LoadAsset<Sprite>("CrucioButton").DontUnload());
        AbilityIcons.Add(bundle.LoadAsset<Sprite>("ImperioButton").DontUnload());
        AbilityIcons.Add(bundle.LoadAsset<Sprite>("DDButton").DontUnload());
        AbilityIcons.Add(bundle.LoadAsset<Sprite>("InvisButton").DontUnload());
        AbilityIcons.Add(bundle.LoadAsset<Sprite>("HourglassButton").DontUnload());
        AbilityIcons.Add(bundle.LoadAsset<Sprite>("MarkButton").DontUnload());
        AbilityIcons.Add(bundle.LoadAsset<Sprite>("RightPanelCloseButton").DontUnload());

        ItemIcons.Add(bundle.LoadAsset<Sprite>("DelumIco").DontUnload());
        ItemIcons.Add(bundle.LoadAsset<Sprite>("MapIco").DontUnload());
        ItemIcons.Add(bundle.LoadAsset<Sprite>("KeyIco").DontUnload());
        ItemIcons.Add(null); //golden snitch
        ItemIcons.Add(null); //res stone
        ItemIcons.Add(null); //butter beer
        ItemIcons.Add(bundle.LoadAsset<Sprite>("ElderWandIco").DontUnload());
        ItemIcons.Add(null); //basilisk
        ItemIcons.Add(null); //sorting hat
        ItemIcons.Add(null); //philo stone

        WorldItemIcons.Add(bundle.LoadAsset<Sprite>("DelumWorldIcon").DontUnload());
        WorldItemIcons.Add(bundle.LoadAsset<Sprite>("MapWorldIcon").DontUnload());
        WorldItemIcons.Add(bundle.LoadAsset<Sprite>("KeyWorldIcon").DontUnload());
        WorldItemIcons.Add(bundle.LoadAsset<Sprite>("SnitchWorldIcon").DontUnload());
        WorldItemIcons.Add(bundle.LoadAsset<Sprite>("GhostStoneWorldIcon").DontUnload());
        WorldItemIcons.Add(bundle.LoadAsset<Sprite>("BeerWorldIcon").DontUnload());
        WorldItemIcons.Add(bundle.LoadAsset<Sprite>("ElderWandWorldIcon").DontUnload());
        WorldItemIcons.Add(bundle.LoadAsset<Sprite>("BasWorldIcon").DontUnload());
        WorldItemIcons.Add(bundle.LoadAsset<Sprite>("SortingHatWorldIcon").DontUnload());
        WorldItemIcons.Add(bundle.LoadAsset<Sprite>("PhiloStoneWorldIcon").DontUnload());

        CrucioSprite.Add(ModHelpers.loadSpriteFromResources("CrucioF1"));
        CrucioSprite.Add(ModHelpers.loadSpriteFromResources("CrucioF2"));

        CurseSprite.Add(ModHelpers.loadSpriteFromResources("CurseF2"));
        CurseSprite.Add(ModHelpers.loadSpriteFromResources("CurseF2"));

        colorBG = bundle.LoadAsset<Sprite>("White").DontUnload();
        SmallSortSprite = bundle.LoadAsset<Sprite>("SmallSortIco").DontUnload();
        SmallSnitchSprite = bundle.LoadAsset<Sprite>("SmallSnitchIco").DontUnload();
        SnitchMaterial = bundle.LoadAsset<PhysicsMaterial2D>("SnitchMaterial").DontUnload();
    }

    public List<Sprite> ItemIcons { get; }
    public Sprite SmallSnitchSprite { get; }
    public Sprite SmallSortSprite { get; }
    public List<Sprite> AbilityIcons { get; }
    public List<Sprite> WorldItemIcons { get; }
    public List<Sprite> CrucioSprite { get; }
    public List<Sprite> CurseSprite { get; }
    public PhysicsMaterial2D SnitchMaterial { get; }
    public TextAsset lanuageJsonAsset { get; }
    public Sprite colorBG { get; }
}
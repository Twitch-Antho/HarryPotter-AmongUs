namespace HarryPotter.Classes.Items;

public class TheGoldenSnitch : Item
{
    public TheGoldenSnitch(ModdedPlayerClass owner)
    {
        Owner = owner;
        ParentInventory = owner.Inventory;
        Id = 3;
        Icon = Main.Instance.Assets.WorldItemIcons[Id];
        Name = "The Golden Snitch";
        IsSpecial = true;
        Tooltip = ModTranslation.getString("TheGoldenSnitchTooltip");
    }
}
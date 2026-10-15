namespace HarryPotter.Classes.Items;

public class SortingHat : Item
{
    public SortingHat(ModdedPlayerClass owner)
    {
        Owner = owner;
        ParentInventory = owner.Inventory;
        Id = 8;
        Icon = Main.Instance.Assets.WorldItemIcons[Id];
        Name = "Sorting Hat";
        IsSpecial = true;
        Tooltip = ModTranslation.getString("SortingHatTooltip");
    }
}
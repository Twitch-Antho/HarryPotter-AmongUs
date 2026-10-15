using Reactor.Utilities;

namespace HarryPotter.Classes.Items;

public class ButterBeer : Item
{
    public ButterBeer(ModdedPlayerClass owner)
    {
        Owner = owner;
        ParentInventory = owner.Inventory;
        Id = 5;
        Name = "Butter Beer";
        Tooltip = "";
        IsTrap = true;
    }

    public override void Use()
    {
        Delete();
        Coroutines.Start(Main.Instance.CoActivateButterBeer(Owner._Object));
    }
}
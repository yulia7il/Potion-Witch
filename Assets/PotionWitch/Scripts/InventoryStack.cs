using System;

// One row in the inventory: which item, and how many of it the player owns.
// Plain data holder — no logic. InventoryManager owns the list of stacks.
[Serializable]
public class InventoryStack
{
    public InventoryItemData item;
    public int amount;

    public InventoryStack(InventoryItemData item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}

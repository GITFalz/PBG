public class ItemSlot
{
    public ItemData item;
    public int Amount;
    public bool Visible = false;
    public Inventory? Inventory = null;

    public ItemSlot(ItemData item, int amount)
    {
        this.item = item;
        this.Amount = amount;
    }

    public ItemSlot()
    {
        item = ItemDataManager.Empty;
        Amount = 0;
    }

    public void Exchange(ItemSlot slot)
    {
        if (Amount == 0)
        {
            item = slot.item;
            Amount = slot.Amount;
            slot.Empty();
            return;
        }
        if (slot.Amount == 0)
        {
            slot.item = item;
            slot.Amount = Amount;
            Empty();
            return;
        }
        Add(slot);
    }

    public void Add(ItemSlot slot)
    {
        if (SameItem(slot))
        {
            if (Amount + slot.Amount > item.MaxStackSize) 
            {
                int overflow = Amount + slot.Amount - item.MaxStackSize;
                Amount = item.MaxStackSize;
                slot.Amount = overflow;
            }
            else
            {
                Amount += slot.Amount;
                slot.Empty();
            }
        }
        else
        {
            ItemData tempItem = item;
            int tempAmount = Amount;

            item = slot.item;
            Amount = slot.Amount;

            slot.item = tempItem;
            slot.Amount = tempAmount;
        }
    }

    public void Add(ItemData item, ref int count)
    {
        if (SameItem(item))
        {
            if (Amount + count > item.MaxStackSize)
            {
                count = Amount + count - item.MaxStackSize;
                Amount = item.MaxStackSize;
            }
            else
            {
                Amount += count;
                count = 0;
            }
        }
        else if (Amount == 0)
        {
            this.item = item;
            Add(item, ref count);
        }
    }

    // Transfer count from slot to this slot
    public void Transfer(ItemSlot slot, int count = 1)
    {
        if (SameItem(slot))
        {
            if (slot.Amount > count)
            {
                if (Amount + count > item.MaxStackSize)
                {
                    int overflow = Amount + count - item.MaxStackSize;
                    Amount = item.MaxStackSize;
                    slot.Amount -= overflow;
                }
                else
                {
                    Amount += count;
                    slot.Amount -= count;
                }
            }
            else
            {
                Amount += slot.Amount;
                slot.Empty();
            }
        }
        else if (Amount == 0)
        {
            item = slot.item;       // Take the item from the slot
            Transfer(slot, count); // Recursive call to transfer the remaining items
        }
        else if (slot.Amount == 0)
        {
            slot.item = item;
            slot.Transfer(this, (int)Amount / 2);
        }
    }

    public bool Remove(int count = 1)
    {
        if (Amount > count)
        {
            Amount -= count;
            return true;
        }
        else if (Amount == count)
        {
            Empty();
            return true;
        }
        return false;
    }

    public bool SameItem(ItemSlot slot)
    {
        return item == slot.item;
    }

    public bool SameItem(ItemData item)
    {
        return this.item == item;
    }

    public bool HasMaxStackSize()
    {
        return Amount == item.MaxStackSize;
    }

    public void Empty()
    {
        item = ItemDataManager.Empty;
        Amount = 0;
    }

    public void Add(int count = 1)
    {
        Amount += count;
    }

    public string GetAmountToString()
    {
        return Amount == 0 ? "" : Amount.ToString();
    }
}
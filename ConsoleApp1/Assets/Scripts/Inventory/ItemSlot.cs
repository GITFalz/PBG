public class ItemSlot
{
    public ItemData item = new EmptyItemData();
    public int Amount;
    public bool Visible = false;

    public ItemSlot(ItemData item, int amount)
    {
        this.item = item;
        this.Amount = amount;
    }

    public ItemSlot()
    {
        item = new EmptyItemData();
        Amount = 0;
    }

    public ItemSlot(ItemData item)
    {
        this.item = item;
        Amount = 1;
    }

    public ItemData Take(int count = 1)
    {
        if (Amount >= count)
        {
            Amount -= count;
            return item;
        }
        else
        {
            ItemData temp = item;
            item = new EmptyItemData();
            Amount = 0;
            return temp;
        }
    }

    public void Add(int count = 1)
    {
        Amount += count;
    }
}
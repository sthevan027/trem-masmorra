using System;

namespace TremMasmorra.Core.Items
{
    public class Item
    {
        public const int MaxDurability = 100;

        public string Name { get; }
        public Rarity Rarity { get; }
        public ItemKind Kind { get; }
        public int Durability { get; private set; }
        public string AbilityId { get; }

        public bool IsBroken => Durability <= 0;

        public Item(string name, Rarity rarity, ItemKind kind, int durability, string abilityId = null)
        {
            Name = name;
            Rarity = rarity;
            Kind = kind;
            Durability = Math.Max(0, Math.Min(MaxDurability, durability));
            AbilityId = abilityId;
        }

        public void Wear(int amount)
        {
            Durability = Math.Max(0, Durability - amount);
        }
    }
}

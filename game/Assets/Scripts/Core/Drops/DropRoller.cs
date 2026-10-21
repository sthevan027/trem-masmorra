using System;
using TremMasmorra.Core.Items;

namespace TremMasmorra.Core.Drops
{
    public class DropRoller
    {
        public const double RareNewChance = 0.40;
        public const double EpicChance = 0.10;
        public const int EpicMinWagon = 5;
        public const int CommonMaxDurability = 44;

        static readonly ItemKind[] CommonKinds =
        {
            ItemKind.Melee, ItemKind.Melee, ItemKind.Ranged, ItemKind.Magic, ItemKind.Shield,
            ItemKind.ArmorHead, ItemKind.ArmorChest, ItemKind.ArmorArms, ItemKind.ArmorLegs
        };

        static readonly ItemKind[] BossKinds =
        {
            ItemKind.Melee, ItemKind.Ranged, ItemKind.Magic, ItemKind.Shield, ItemKind.Orb,
            ItemKind.ArmorHead, ItemKind.ArmorChest, ItemKind.ArmorArms, ItemKind.ArmorLegs,
            ItemKind.ExtraArm, ItemKind.Utility
        };

        readonly Random rng;

        public DropRoller(Random rng) { this.rng = rng; }

        public Item RollCommon(int wagon)
        {
            var kind = CommonKinds[rng.Next(CommonKinds.Length)];
            int durability = rng.Next(5, CommonMaxDurability + 1);
            return ItemFactory.Make(kind, Rarity.Comum, durability);
        }

        public Item[] RollBossItems(int wagon, int count)
        {
            var items = new Item[count];
            for (int i = 0; i < count; i++) items[i] = RollBossItem(wagon);
            return items;
        }

        Item RollBossItem(int wagon)
        {
            var kind = BossKinds[rng.Next(BossKinds.Length)];
            if (wagon >= EpicMinWagon && rng.NextDouble() < EpicChance)
                return ItemFactory.Make(kind, Rarity.Epico, Item.MaxDurability);

            int durability = rng.NextDouble() < RareNewChance ? Item.MaxDurability : rng.Next(90, 100);
            return ItemFactory.Make(kind, Rarity.Raro, durability);
        }

    }
}

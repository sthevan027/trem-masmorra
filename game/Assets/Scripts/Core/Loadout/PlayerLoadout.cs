using System.Collections.Generic;
using TremMasmorra.Core.Cards;
using TremMasmorra.Core.Items;

namespace TremMasmorra.Core.Loadout
{
    public enum ArmorSlot { Head, Chest, Arms, Legs }

    public class PlayerLoadout
    {
        public const int BackpackCapacity = 20;
        public const int BaseMaxInUse = 2;

        readonly List<Item> backpack = new List<Item>();
        readonly List<Item> inUse = new List<Item>();

        public IReadOnlyList<Item> Backpack => backpack;
        public IReadOnlyList<Item> InUse => inUse;
        public Item[] Armor { get; } = new Item[4];
        public Item ExtraArm { get; private set; }
        public string AbilityQ { get; private set; }
        public string AbilityE { get; private set; }

        public int MaxInUse => BaseMaxInUse + (ExtraArm != null && !ExtraArm.IsBroken ? 1 : 0);

        public bool TryAddToBackpack(Item item)
        {
            if (backpack.Count >= BackpackCapacity) return false;
            backpack.Add(item);
            return true;
        }

        public bool TryEquipInUse(Item item)
        {
            if (!backpack.Contains(item) || !IsUsable(item.Kind) || inUse.Count >= MaxInUse) return false;
            backpack.Remove(item);
            inUse.Add(item);
            return true;
        }

        public bool Swap(Item inUseItem, Item fromBackpack)
        {
            if (!inUse.Contains(inUseItem) || !backpack.Contains(fromBackpack) || !IsUsable(fromBackpack.Kind))
                return false;
            inUse[inUse.IndexOf(inUseItem)] = fromBackpack;
            backpack[backpack.IndexOf(fromBackpack)] = inUseItem;
            return true;
        }

        public bool TryEquipExtraArm(Item item)
        {
            if (item.Kind != ItemKind.ExtraArm || !backpack.Contains(item)) return false;
            backpack.Remove(item);
            if (ExtraArm != null) backpack.Add(ExtraArm);
            ExtraArm = item;
            return true;
        }

        public bool EquipArmor(Item item)
        {
            if (!TryArmorSlot(item.Kind, out var slot) || !backpack.Contains(item)) return false;
            backpack.Remove(item);
            var old = Armor[(int)slot];
            Armor[(int)slot] = item;
            if (old != null) backpack.Add(old);
            return true;
        }

        public void OnItemBroken(Item item)
        {
            backpack.Remove(item);
            inUse.Remove(item);
            for (int i = 0; i < Armor.Length; i++)
                if (Armor[i] == item) Armor[i] = null;
            if (ExtraArm == item)
            {
                ExtraArm = null;
                while (inUse.Count > MaxInUse)
                {
                    var overflow = inUse[inUse.Count - 1];
                    inUse.RemoveAt(inUse.Count - 1);
                    backpack.Add(overflow);
                }
            }
        }

        /// <summary>Aplica a carta escolhida. Retorna false se nao coube (mochila cheia ou Q e E ocupados).</summary>
        public bool Apply(Card card)
        {
            if (card.IsAbility) return TryAssignFirstEmpty(card.AbilityId);
            return TryAddToBackpack(card.Item);
        }

        public void SetAbility(char slot, string abilityId)
        {
            if (slot == 'Q') AbilityQ = abilityId;
            else if (slot == 'E') AbilityE = abilityId;
        }

        bool TryAssignFirstEmpty(string abilityId)
        {
            if (AbilityQ == null) { AbilityQ = abilityId; return true; }
            if (AbilityE == null) { AbilityE = abilityId; return true; }
            return false;
        }

        static bool IsUsable(ItemKind kind) =>
            kind == ItemKind.Melee || kind == ItemKind.Ranged || kind == ItemKind.Magic ||
            kind == ItemKind.Shield || kind == ItemKind.Orb;

        static bool TryArmorSlot(ItemKind kind, out ArmorSlot slot)
        {
            switch (kind)
            {
                case ItemKind.ArmorHead: slot = ArmorSlot.Head; return true;
                case ItemKind.ArmorChest: slot = ArmorSlot.Chest; return true;
                case ItemKind.ArmorArms: slot = ArmorSlot.Arms; return true;
                case ItemKind.ArmorLegs: slot = ArmorSlot.Legs; return true;
                default: slot = ArmorSlot.Head; return false;
            }
        }
    }
}

using TremMasmorra.Core.Items;

namespace TremMasmorra.Core.Drops
{
    public static class ItemFactory
    {
        public static Item Make(ItemKind kind, Rarity rarity, int durability)
        {
            string name = BaseName(kind) + " [" + rarity + "]";
            string ability = rarity == Rarity.Comum ? null : AbilityFor(kind);
            return new Item(name, rarity, kind, durability, ability);
        }

        public static string BaseName(ItemKind kind)
        {
            switch (kind)
            {
                case ItemKind.Melee: return "Espada";
                case ItemKind.Ranged: return "Arco";
                case ItemKind.Magic: return "Cajado";
                case ItemKind.Shield: return "Escudo";
                case ItemKind.Orb: return "Orbe Protetor";
                case ItemKind.ArmorHead: return "Elmo";
                case ItemKind.ArmorChest: return "Peitoral";
                case ItemKind.ArmorArms: return "Braceletes";
                case ItemKind.ArmorLegs: return "Grevas";
                case ItemKind.ExtraArm: return "Braco Mecanico";
                default: return "Capa da Invisibilidade";
            }
        }

        static string AbilityFor(ItemKind kind)
        {
            switch (kind)
            {
                case ItemKind.Melee: return "golpe_giratorio";
                case ItemKind.Ranged: return "chuva_de_flechas";
                case ItemKind.Magic: return "bola_de_fogo";
                case ItemKind.Utility: return "invisibilidade";
                default: return null;
            }
        }
    }
}

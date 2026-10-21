using TremMasmorra.Core.Items;

namespace TremMasmorra.Core.Cards
{
    public enum CardStyle { Magia, FisicoDistancia, FisicoCorpo }

    public class Card
    {
        public CardStyle Style { get; }
        public Item Item { get; }
        public string AbilityId { get; }
        public bool IsAbility => AbilityId != null;

        Card(CardStyle style, Item item, string abilityId)
        {
            Style = style;
            Item = item;
            AbilityId = abilityId;
        }

        public static Card ForAbility(string abilityId, CardStyle style) => new Card(style, null, abilityId);

        public static Card ForItem(Item item, CardStyle style) => new Card(style, item, null);

        public static CardStyle StyleOf(ItemKind kind)
        {
            switch (kind)
            {
                case ItemKind.Ranged: return CardStyle.FisicoDistancia;
                case ItemKind.Magic:
                case ItemKind.Orb: return CardStyle.Magia;
                default: return CardStyle.FisicoCorpo;
            }
        }
    }
}

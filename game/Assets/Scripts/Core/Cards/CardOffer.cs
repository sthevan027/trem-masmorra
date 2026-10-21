using System;
using TremMasmorra.Core.Drops;

namespace TremMasmorra.Core.Cards
{
    public class CardOffer
    {
        public const int CardCount = 3;

        public static readonly (string Id, CardStyle Style)[] Abilities =
        {
            ("bola_de_fogo", CardStyle.Magia),
            ("raio_arcano", CardStyle.Magia),
            ("flecha_perfurante", CardStyle.FisicoDistancia),
            ("chuva_de_flechas", CardStyle.FisicoDistancia),
            ("golpe_giratorio", CardStyle.FisicoCorpo),
            ("investida", CardStyle.FisicoCorpo),
        };

        public Card[] Cards { get; }
        bool picked;

        public CardOffer(DropRoller roller, Random rng, int wagon)
        {
            Cards = new Card[CardCount];
            var items = roller.RollBossItems(wagon, CardCount);
            for (int i = 0; i < CardCount; i++)
            {
                // Um estilo por posicao garante variedade: magia, distancia e corpo a corpo.
                var wanted = (CardStyle)((i + rng.Next(3)) % 3);
                bool ability = rng.NextDouble() < 0.5;
                if (ability)
                {
                    Cards[i] = Card.ForAbility(PickAbility(rng, wanted), wanted);
                }
                else
                {
                    Cards[i] = Card.ForItem(items[i], Card.StyleOf(items[i].Kind));
                }
            }
            EnsureStyleVariety(items);
        }

        public Card Pick(int index)
        {
            if (picked) throw new InvalidOperationException("Uma carta ja foi escolhida.");
            picked = true;
            return Cards[index];
        }

        static string PickAbility(Random rng, CardStyle style)
        {
            var matches = Array.FindAll(Abilities, a => a.Style == style);
            return matches[rng.Next(matches.Length)].Id;
        }

        void EnsureStyleVariety(Core.Items.Item[] items)
        {
            if (Cards[0].Style != Cards[1].Style || Cards[1].Style != Cards[2].Style) return;
            var other = (CardStyle)(((int)Cards[2].Style + 1) % 3);
            Cards[2] = Card.ForAbility(PickAbility(new Random(0), other), other);
        }
    }
}

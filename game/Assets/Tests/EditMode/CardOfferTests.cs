using System;
using System.Linq;
using NUnit.Framework;
using TremMasmorra.Core.Cards;
using TremMasmorra.Core.Drops;

namespace TremMasmorra.Tests
{
    public class CardOfferTests
    {
        static CardOffer NewOffer(int seed = 1, int wagon = 3) =>
            new CardOffer(new DropRoller(new Random(seed)), new Random(seed), wagon);

        [Test]
        public void Offer_HasThreeCards() => Assert.AreEqual(3, NewOffer().Cards.Length);

        [Test]
        public void Pick_ReturnsChosen()
        {
            var offer = NewOffer();
            Assert.AreSame(offer.Cards[1], offer.Pick(1));
        }

        [Test]
        public void Pick_Twice_Throws()
        {
            var offer = NewOffer();
            offer.Pick(0);
            Assert.Throws<InvalidOperationException>(() => offer.Pick(1));
        }

        [Test]
        public void Cards_CoverMoreThanOneStyle()
        {
            for (int seed = 0; seed < 100; seed++)
                Assert.GreaterOrEqual(NewOffer(seed).Cards.Select(c => c.Style).Distinct().Count(), 2,
                    "seed " + seed);
        }

        [Test]
        public void Card_IsExactlyAbilityOrItem()
        {
            foreach (var card in NewOffer().Cards)
                Assert.AreNotEqual(card.IsAbility, card.Item != null);
        }
    }
}

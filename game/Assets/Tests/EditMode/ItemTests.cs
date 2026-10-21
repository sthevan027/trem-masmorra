using NUnit.Framework;
using TremMasmorra.Core.Items;

namespace TremMasmorra.Tests
{
    public class ItemTests
    {
        static Item NewItem(int durability = 100) =>
            new Item("Espada", Rarity.Comum, ItemKind.Melee, durability);

        [Test]
        public void Wear_ReducesDurability()
        {
            var item = NewItem(100);
            item.Wear(10);
            Assert.AreEqual(90, item.Durability);
        }

        [Test]
        public void Wear_ToZero_MarksBroken()
        {
            var item = NewItem(5);
            item.Wear(5);
            Assert.IsTrue(item.IsBroken);
        }

        [Test]
        public void Wear_NeverBelowZero()
        {
            var item = NewItem(3);
            item.Wear(50);
            Assert.AreEqual(0, item.Durability);
        }

        [Test]
        public void NewItem_DurabilityClampedTo100()
        {
            Assert.AreEqual(100, NewItem(150).Durability);
        }
    }
}

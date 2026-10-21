using NUnit.Framework;
using TremMasmorra.Core.Cards;
using TremMasmorra.Core.Items;
using TremMasmorra.Core.Loadout;

namespace TremMasmorra.Tests
{
    public class LoadoutTests
    {
        static Item Sword(Rarity r = Rarity.Comum, int d = 30) => new Item("Espada", r, ItemKind.Melee, d);

        static PlayerLoadout NewLoadout() => new PlayerLoadout();

        [Test]
        public void InUse_MaxTwo()
        {
            var l = NewLoadout();
            var a = Sword(); var b = Sword(); var c = Sword();
            l.TryAddToBackpack(a); l.TryAddToBackpack(b); l.TryAddToBackpack(c);
            Assert.IsTrue(l.TryEquipInUse(a));
            Assert.IsTrue(l.TryEquipInUse(b));
            Assert.IsFalse(l.TryEquipInUse(c));
        }

        [Test]
        public void ExtraArm_AllowsThird()
        {
            var l = NewLoadout();
            var arm = new Item("Braco extra", Rarity.Raro, ItemKind.ExtraArm, 100);
            var a = Sword(); var b = Sword(); var c = Sword();
            foreach (var i in new[] { arm, a, b, c }) l.TryAddToBackpack(i);
            Assert.IsTrue(l.TryEquipExtraArm(arm));
            Assert.IsTrue(l.TryEquipInUse(a));
            Assert.IsTrue(l.TryEquipInUse(b));
            Assert.IsTrue(l.TryEquipInUse(c));
            Assert.AreEqual(3, l.MaxInUse);
        }

        [Test]
        public void TwoSwords_BothEquipped_WearIndependently()
        {
            var l = NewLoadout();
            var a = Sword(); var b = Sword();
            l.TryAddToBackpack(a); l.TryAddToBackpack(b);
            l.TryEquipInUse(a); l.TryEquipInUse(b);
            a.Wear(10);
            Assert.AreEqual(20, a.Durability);
            Assert.AreEqual(30, b.Durability);
            Assert.AreEqual(2, l.InUse.Count);
        }

        [Test]
        public void BrokenItem_RemovedFromInUse()
        {
            var l = NewLoadout();
            var a = Sword(d: 5);
            l.TryAddToBackpack(a); l.TryEquipInUse(a);
            a.Wear(5);
            l.OnItemBroken(a);
            Assert.AreEqual(0, l.InUse.Count);
            CollectionAssert.DoesNotContain(l.Backpack, a);
        }

        [Test]
        public void Apply_AbilityCard_FillsFirstEmptyOfQE()
        {
            var l = NewLoadout();
            Assert.IsTrue(l.Apply(Card.ForAbility("fireball", CardStyle.Magia)));
            Assert.AreEqual("fireball", l.AbilityQ);
            Assert.IsTrue(l.Apply(Card.ForAbility("dash", CardStyle.FisicoCorpo)));
            Assert.AreEqual("dash", l.AbilityE);
        }

        [Test]
        public void Apply_AbilityCard_BothFull_ReturnsFalse()
        {
            var l = NewLoadout();
            l.Apply(Card.ForAbility("a", CardStyle.Magia));
            l.Apply(Card.ForAbility("b", CardStyle.Magia));
            Assert.IsFalse(l.Apply(Card.ForAbility("c", CardStyle.Magia)));
            Assert.AreEqual("a", l.AbilityQ);
        }

        [Test]
        public void Apply_ItemCard_BackpackFull_DoesNotLoseItem()
        {
            var l = NewLoadout();
            for (int i = 0; i < PlayerLoadout.BackpackCapacity; i++) l.TryAddToBackpack(Sword());
            var epic = Sword(Rarity.Epico, 100);
            Assert.IsFalse(l.Apply(Card.ForItem(epic, CardStyle.FisicoCorpo)));
            Assert.AreEqual(PlayerLoadout.BackpackCapacity, l.Backpack.Count);
        }

        [Test]
        public void SwapRustySwordForEpic()
        {
            var l = NewLoadout();
            var rusty = Sword(); var epic = Sword(Rarity.Epico, 100);
            l.TryAddToBackpack(rusty); l.TryAddToBackpack(epic);
            l.TryEquipInUse(rusty);
            Assert.IsTrue(l.Swap(rusty, epic));
            CollectionAssert.Contains(l.InUse, epic);
            CollectionAssert.Contains(l.Backpack, rusty);
        }

        [Test]
        public void Armor_EquipsBySlot_ReplacesAndReturnsOldToBackpack()
        {
            var l = NewLoadout();
            var h1 = new Item("Capacete 1", Rarity.Comum, ItemKind.ArmorHead, 40);
            var h2 = new Item("Capacete 2", Rarity.Raro, ItemKind.ArmorHead, 100);
            l.TryAddToBackpack(h1); l.TryAddToBackpack(h2);
            Assert.IsTrue(l.EquipArmor(h1));
            Assert.IsTrue(l.EquipArmor(h2));
            Assert.AreSame(h2, l.Armor[(int)ArmorSlot.Head]);
            CollectionAssert.Contains(l.Backpack, h1);
        }
    }
}

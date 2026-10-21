using System;
using System.Linq;
using NUnit.Framework;
using TremMasmorra.Core.Drops;
using TremMasmorra.Core.Items;

namespace TremMasmorra.Tests
{
    public class DropRollerTests
    {
        static DropRoller NewRoller() => new DropRoller(new Random(1234));

        [Test]
        public void Common_AlwaysBelow45()
        {
            var roller = NewRoller();
            for (int i = 0; i < 1000; i++)
            {
                var item = roller.RollCommon(2);
                Assert.Less(item.Durability, 45);
                Assert.AreEqual(Rarity.Comum, item.Rarity);
            }
        }

        [Test]
        public void Epic_NeverBeforeWagon5()
        {
            var roller = NewRoller();
            for (int i = 0; i < 1000; i++)
                Assert.IsFalse(roller.RollBossItems(3, 3).Any(x => x.Rarity == Rarity.Epico));
        }

        [Test]
        public void Epic_AppearsAtWagon6()
        {
            var roller = NewRoller();
            bool found = false;
            for (int i = 0; i < 1000 && !found; i++)
                found = roller.RollBossItems(6, 3).Any(x => x.Rarity == Rarity.Epico);
            Assert.IsTrue(found);
        }

        [Test]
        public void BossRare_NearNewDurability()
        {
            var roller = NewRoller();
            for (int i = 0; i < 1000; i++)
                foreach (var item in roller.RollBossItems(3, 3))
                {
                    Assert.AreEqual(Rarity.Raro, item.Rarity);
                    Assert.GreaterOrEqual(item.Durability, 90);
                }
        }
    }
}

using System;
using NUnit.Framework;
using TremMasmorra.Core.Run;

namespace TremMasmorra.Tests
{
    public class RunStateTests
    {
        [Test]
        public void BossWagons_Are3_6_9()
        {
            var r = new RunState();
            for (int w = 1; w <= 9; w++)
            {
                r.Wagon = w;
                Assert.AreEqual(w == 3 || w == 6 || w == 9, r.IsBossWagon, "vagao " + w);
            }
        }

        [Test]
        public void Door_OpensWhenAllDead()
        {
            var r = new RunState();
            r.StartWagon(2);
            Assert.IsFalse(r.DoorOpen);
            r.OnMonsterKilled();
            Assert.IsFalse(r.DoorOpen);
            r.OnMonsterKilled();
            Assert.IsTrue(r.DoorOpen);
        }

        [Test]
        public void NextWagon_ClosedDoor_Throws()
        {
            var r = new RunState();
            r.StartWagon(1);
            Assert.Throws<InvalidOperationException>(() => r.NextWagon());
        }

        [Test]
        public void NextWagon_AdvancesWagon()
        {
            var r = new RunState();
            r.StartWagon(1); r.OnMonsterKilled(); r.NextWagon();
            Assert.AreEqual(2, r.Wagon);
        }

        [Test]
        public void Victory_AfterWagon9()
        {
            var r = new RunState { Wagon = 9 };
            r.StartWagon(1); r.OnMonsterKilled(); r.NextWagon();
            Assert.IsTrue(r.Victory);
        }

        [Test]
        public void PlayerDied_RestartGoesToWagon1()
        {
            var r = new RunState { Wagon = 6 };
            r.StartWagon(3);
            r.OnPlayerDied();
            Assert.IsTrue(r.Dead);
            r.Restart();
            Assert.AreEqual(1, r.Wagon);
            Assert.IsFalse(r.Dead);
        }

        [Test]
        public void Restart_ClearsPreviousRunState()
        {
            var r = new RunState { Wagon = 9 };
            r.StartWagon(1); r.OnMonsterKilled(); r.NextWagon();
            r.Restart();
            Assert.IsFalse(r.Victory);
            Assert.IsFalse(r.DoorOpen);
            Assert.AreEqual(0, r.MonstersAlive);
        }
    }
}

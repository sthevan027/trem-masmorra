using System.Collections;
using System.Linq;
using NUnit.Framework;
using TremMasmorra.Core.Run;
using TremMasmorra.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace TremMasmorra.PlayTests
{
    public class FullRunTests
    {
        GameManager gm;

        [SetUp]
        public void SetUp()
        {
            PlayerController.InputBlocked = false;
            Time.timeScale = 1f;
            gm = new GameObject("GameManager").AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            PlayerController.InputBlocked = false;
            foreach (var root in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.parent == null).ToList())
                if (root != null) Object.Destroy(root.gameObject);
        }

        IEnumerator ClearWagonAndExit()
        {
            yield return null;
            foreach (var e in gm.Alive.ToList()) e.TakeDamage(999999);
            yield return null;
            var cards = Object.FindFirstObjectByType<CardRevealScreen>();
            if (cards != null && cards.IsOpen) cards.ForcePick(0);
            yield return null;
            gm.Player.Teleport(new Vector3(0, 0.1f, 39.2f), Quaternion.identity);
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator FullRun_FromStartToVictory()
        {
            yield return null;
            gm.StartRun();
            for (int w = 1; w <= RunState.TotalWagons; w++)
            {
                Assert.AreEqual(GameState.Playing, gm.State, "vagao " + w);
                Assert.AreEqual(w, gm.Run.Wagon);
                int bosses = gm.Alive.Count(e => e.Def.IsBoss);
                Assert.AreEqual(w % 3 == 0 ? 1 : 0, bosses, "bosses no vagao " + w);
                yield return ClearWagonAndExit();
            }
            Assert.AreEqual(GameState.Victory, gm.State);
        }

        [UnityTest]
        public IEnumerator ThreeDifferentBosses()
        {
            yield return null;
            gm.StartRun();
            var names = new System.Collections.Generic.HashSet<string>();
            for (int w = 1; w <= RunState.TotalWagons; w++)
            {
                foreach (var e in gm.Alive.Where(e => e.Def.IsBoss)) names.Add(e.Def.Name);
                yield return ClearWagonAndExit();
            }
            Assert.AreEqual(3, names.Count);
        }

        [UnityTest]
        public IEnumerator Death_RestartsClean()
        {
            yield return null;
            gm.StartRun();
            yield return ClearWagonAndExit(); // vai para o vagao 2
            Assert.AreEqual(2, gm.Run.Wagon);
            var oldLoadout = gm.Loadout;
            gm.Player.Health.Damage(999999);
            yield return null;
            Assert.AreEqual(GameState.Dead, gm.State);

            gm.StartRun();
            yield return null;
            Assert.AreEqual(1, gm.Run.Wagon);
            Assert.AreNotSame(oldLoadout, gm.Loadout);
            Assert.AreEqual(1, Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(gm.Player.Health.Max, gm.Player.Health.Current);
            Assert.IsNull(gm.Loadout.AbilityQ);
            Assert.IsFalse(gm.Run.DoorOpen);
        }
    }
}

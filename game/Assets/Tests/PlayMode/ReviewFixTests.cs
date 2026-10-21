using System.Collections;
using System.Linq;
using NUnit.Framework;
using TremMasmorra.Core.Character;
using TremMasmorra.Core.Loadout;
using TremMasmorra.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace TremMasmorra.PlayTests
{
    /// <summary>Defeitos achados na revisao final, cada um reproduzido por um teste.</summary>
    public class ReviewFixTests
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

        IEnumerator GoToBossWagon()
        {
            yield return null;
            gm.StartRun();
            for (int w = 1; w <= 2; w++)
            {
                yield return null;
                foreach (var e in gm.Alive.ToList()) e.TakeDamage(999999);
                yield return null;
                gm.Player.Teleport(new Vector3(0, 0.1f, 39.2f), Quaternion.identity);
                yield return null;
                yield return null;
            }
            Assert.AreEqual(3, gm.Run.Wagon);
        }

        [UnityTest]
        public IEnumerator Inventory_Open_UnlocksCursor()
        {
            yield return null;
            var inv = Object.FindFirstObjectByType<InventoryScreen>();
            inv.Bind(new PlayerLoadout());
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inv.Open();
            Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
            Assert.IsTrue(Cursor.visible);
            inv.Close(); // o relock nao e verificavel: o editor em batch ignora CursorLockMode.Locked
        }

        [UnityTest]
        public IEnumerator BossKilledAfterPlayerDeath_DoesNotOpenCards()
        {
            yield return GoToBossWagon();
            var boss = gm.Alive.First(e => e.Def.IsBoss);
            gm.Player.Health.Damage(999999);
            yield return null;
            Assert.AreEqual(GameState.Dead, gm.State);
            boss.TakeDamage(999999);
            yield return null;
            Assert.IsFalse(Object.FindFirstObjectByType<CardRevealScreen>().IsOpen);
        }

        [UnityTest]
        public IEnumerator StartRun_ClosesOpenCardScreen()
        {
            yield return GoToBossWagon();
            gm.Alive.First(e => e.Def.IsBoss).TakeDamage(999999);
            yield return null;
            var cards = Object.FindFirstObjectByType<CardRevealScreen>();
            Assert.IsTrue(cards.IsOpen);
            gm.StartRun();
            yield return null;
            Assert.IsFalse(cards.IsOpen);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator PlayerHitbox_IndependentOfCharacterHeight()
        {
            yield return null;
            gm.Look.Height = CharacterLook.MinHeight;
            gm.StartRun();
            float small = gm.Player.GetComponent<CharacterController>().height;
            gm.Look.Height = CharacterLook.MaxHeight;
            gm.StartRun();
            yield return null;
            float tall = gm.Player.GetComponent<CharacterController>().height;
            Assert.AreEqual(small, tall, 0.001f);
        }

        [UnityTest]
        public IEnumerator BossVolley_HitsDefaultHeightPlayer()
        {
            PlayerController.InputBlocked = true;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 4f);
            floor.transform.localScale = new Vector3(40, 1, 40);
            var go = new GameObject("Jogador", typeof(CharacterController), typeof(Health));
            var cc = go.GetComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.4f; cc.center = new Vector3(0, 0.9f, 0);
            var player = go.AddComponent<PlayerController>();
            player.Setup(new PlayerLoadout(), ProceduralModels.Humanoid(new CharacterLook(), Color.blue, Color.gray));
            player.Teleport(new Vector3(0, 0.1f, 0), Quaternion.identity);
            var boss = Enemy.Spawn(EnemyCatalog.ReiFormiga, new Vector3(0, 0.1f, 8f), player);
            yield return new WaitForSeconds(4f);
            Assert.Less(player.Health.Current, player.Health.Max, "a salva do boss nunca acertou o jogador");
            Object.Destroy(boss.gameObject);
            Object.Destroy(go);
            Object.Destroy(floor);
        }
    }
}

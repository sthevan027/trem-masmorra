using System.Collections;
using NUnit.Framework;
using TremMasmorra.Core.Character;
using TremMasmorra.Core.Items;
using TremMasmorra.Core.Loadout;
using TremMasmorra.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace TremMasmorra.PlayTests
{
    public class CombatTests
    {
        PlayerController player;
        PlayerLoadout loadout;

        [SetUp]
        public void SetUp()
        {
            PlayerController.InputBlocked = true; // testes chamam Attack/TakeHit direto
            Time.timeScale = 1f;
            var go = new GameObject("Jogador", typeof(CharacterController), typeof(Health));
            player = go.AddComponent<PlayerController>();
            loadout = new PlayerLoadout();
            player.Setup(loadout, ProceduralModels.Humanoid(new CharacterLook(), Color.blue, Color.gray));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var e in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None)) Object.Destroy(e.gameObject);
            Object.Destroy(player.gameObject);
            PlayerController.InputBlocked = false;
        }

        Item Equip(ItemKind kind, int durability)
        {
            var item = new Item("Teste", Rarity.Comum, kind, durability);
            loadout.TryAddToBackpack(item);
            if (kind == ItemKind.Melee || kind == ItemKind.Shield) loadout.TryEquipInUse(item);
            return item;
        }

        Enemy EnemyInFront()
        {
            var e = Enemy.Spawn(EnemyCatalog.FormigaCristal, player.transform.position + player.transform.forward * 1.6f, player);
            e.enabled = false; // parado, so serve de alvo
            return e;
        }

        [UnityTest]
        public IEnumerator Attack_WearsWeapon()
        {
            var sword = Equip(ItemKind.Melee, 40);
            EnemyInFront();
            yield return new WaitForFixedUpdate();
            player.Attack();
            Assert.AreEqual(39, sword.Durability);
        }

        [UnityTest]
        public IEnumerator TwoSwords_BothWearIndependently()
        {
            var a = Equip(ItemKind.Melee, 40);
            var b = Equip(ItemKind.Melee, 20);
            EnemyInFront();
            yield return new WaitForFixedUpdate();
            player.Attack();
            Assert.AreEqual(39, a.Durability);
            Assert.AreEqual(19, b.Durability);
        }

        [UnityTest]
        public IEnumerator WeaponBreaks_AtZero_NextAttackSafe()
        {
            var sword = Equip(ItemKind.Melee, 1);
            var enemy = EnemyInFront();
            yield return new WaitForFixedUpdate();
            player.Attack();
            Assert.IsTrue(sword.IsBroken);
            Assert.AreEqual(0, loadout.InUse.Count);
            Assert.DoesNotThrow(() => player.Attack());
            Assert.Less(enemy.Health.Current, enemy.Health.Max);
        }

        [Test]
        public void Block_WearsShield_AndHalvesDamage()
        {
            var shield = Equip(ItemKind.Shield, 50);
            player.TakeHit(10);
            Assert.AreEqual(48, shield.Durability);
            Assert.AreEqual(95, player.Health.Current);
        }

        [Test]
        public void Hit_WearsArmor()
        {
            var helmet = new Item("Elmo", Rarity.Comum, ItemKind.ArmorHead, 50);
            loadout.TryAddToBackpack(helmet);
            loadout.EquipArmor(helmet);
            player.TakeHit(10);
            Assert.AreEqual(49, helmet.Durability);
        }

        [Test]
        public void QEWithoutAbility_DoesNothing()
        {
            Assert.DoesNotThrow(() => { player.Cast('Q'); player.Cast('E'); });
            Assert.AreEqual(player.Health.Max, player.Health.Current);
        }
    }
}

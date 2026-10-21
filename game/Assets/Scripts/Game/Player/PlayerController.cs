using System;
using System.Collections.Generic;
using System.Linq;
using TremMasmorra.Core.Items;
using TremMasmorra.Core.Loadout;
using UnityEngine;

namespace TremMasmorra.Game
{
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        public const float MoveSpeed = 6f;
        public const float JumpSpeed = 7f;
        public const float Gravity = -20f;
        public const float DodgeTime = 0.35f;
        public const float DodgeCooldown = 1f;
        public const float AttackCooldown = 0.45f;

        public static bool InputBlocked;

        public PlayerLoadout Loadout;
        public Health Health;
        public SimpleAnimator Anim;
        public Transform CameraTransform;
        public bool Invisible { get; private set; }

        public event Action<string> Message;
        public event Action<Item> ItemBroken;

        CharacterController cc;
        float verticalVelocity;
        float attackReady;
        float dodgeUntil;
        float dodgeReady;
        Vector3 dodgeDir;
        float invisibleUntil;
        float dashUntil;
        Vector3 dashVelocity;
        int dashDamage;
        readonly HashSet<Enemy> dashHit = new HashSet<Enemy>();
        float cooldownQ, cooldownE;
        float cooldownQMax = 1f, cooldownEMax = 1f;
        string heldSignature = "";
        GameObject heldVisual;

        public Vector3 Position => transform.position;
        public Vector3 Chest => transform.position + Vector3.up * 1.3f + transform.forward * 0.6f;
        public Vector3 Aim { get { var f = transform.forward; f.y = 0f; return f.normalized; } }
        public float DodgeFraction => Mathf.Clamp01(1f - (dodgeReady - Time.time) / DodgeCooldown);

        public void Setup(PlayerLoadout loadout, SimpleAnimator anim)
        {
            Loadout = loadout;
            Anim = anim;
            cc = GetComponent<CharacterController>();
            Health = GetComponent<Health>();
            Health.Init(100);
            anim.transform.SetParent(transform, false);
            ProceduralModels.BlobShadow(transform, 0.55f);
        }

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            Health = GetComponent<Health>();
        }

        public void Teleport(Vector3 pos, Quaternion rot)
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(pos, rot);
            cc.enabled = true;
            verticalVelocity = 0f;
        }

        void Update()
        {
            if (Health.IsDead || Loadout == null) return;
            RefreshHeld();
            if (invisibleUntil > 0f && Time.time > invisibleUntil)
            {
                Invisible = false;
                Anim.Ghost = false;
                invisibleUntil = 0f;
            }
            cooldownQ = Mathf.Max(0f, cooldownQ - Time.deltaTime);
            cooldownE = Mathf.Max(0f, cooldownE - Time.deltaTime);

            Vector3 velocity = Vector3.zero;
            if (!InputBlocked) velocity = HandleInput();
            else Anim.MoveSpeed = 0f;

            if (Time.time < dashUntil)
            {
                velocity = dashVelocity;
                DashDamage();
            }

            if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
            verticalVelocity += Gravity * Time.deltaTime;
            velocity.y = verticalVelocity;
            cc.Move(velocity * Time.deltaTime);
            Health.Invulnerable = Time.time < dodgeUntil;
        }

        Vector3 HandleInput()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 fwd = CameraTransform != null ? CameraTransform.forward : Vector3.forward;
            Vector3 right = CameraTransform != null ? CameraTransform.right : Vector3.right;
            fwd.y = 0f; right.y = 0f;
            Vector3 move = (fwd.normalized * v + right.normalized * h);
            if (move.sqrMagnitude > 1f) move.Normalize();

            if (move.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 15f * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded) verticalVelocity = JumpSpeed;

            if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= dodgeReady)
            {
                dodgeUntil = Time.time + DodgeTime;
                dodgeReady = Time.time + DodgeCooldown;
                dodgeDir = move.sqrMagnitude > 0.01f ? move.normalized : transform.forward;
            }

            if (Input.GetMouseButton(0) && Time.time >= attackReady) Attack();
            if (Input.GetKeyDown(KeyCode.Q)) Cast('Q');
            if (Input.GetKeyDown(KeyCode.E)) Cast('E');

            Anim.MoveSpeed = move.magnitude * MoveSpeed;
            if (Time.time < dodgeUntil) return dodgeDir * (MoveSpeed * 2.4f);
            return move * MoveSpeed;
        }

        // ---------- Ataque ----------

        public static bool IsWeapon(Item item) =>
            item.Kind == ItemKind.Melee || item.Kind == ItemKind.Ranged || item.Kind == ItemKind.Magic;

        static int WeaponDamage(Item w) =>
            w.Rarity == Rarity.Epico ? 28 : w.Rarity == Rarity.Raro ? 18 : 10;

        public void Attack()
        {
            attackReady = Time.time + AttackCooldown;
            Anim.PlayAttack();
            var weapons = Loadout.InUse.Where(IsWeapon).ToList();
            if (weapons.Count == 0)
            {
                foreach (var e in CombatUtil.EnemiesInSphere(Position + Aim * 1.4f + Vector3.up, 1.6f)) e.TakeDamage(5);
                return;
            }
            foreach (var w in weapons) UseWeapon(w);
        }

        void UseWeapon(Item w)
        {
            int damage = WeaponDamage(w);
            bool spent;
            if (w.Kind == ItemKind.Melee)
            {
                var hits = CombatUtil.EnemiesInSphere(Position + Aim * 1.6f + Vector3.up, 1.9f);
                foreach (var e in hits) e.TakeDamage(damage);
                spent = hits.Count > 0;
            }
            else
            {
                var color = w.Kind == ItemKind.Magic ? new Color(0.6f, 0.4f, 1f) : new Color(0.9f, 0.9f, 0.5f);
                Projectile.Spawn(Chest, Aim, damage, color, false, false, 26f);
                spent = true;
            }
            if (spent) WearItem(w, 1);
        }

        public void WearItem(Item item, int amount)
        {
            if (item == null || item.IsBroken) return;
            item.Wear(amount);
            if (!item.IsBroken) return;
            Loadout.OnItemBroken(item);
            Message?.Invoke(item.Name + " quebrou!");
            ItemBroken?.Invoke(item);
        }

        // ---------- Dano recebido ----------

        public void TakeHit(int damage)
        {
            if (Health.IsDead || Health.Invulnerable) return;
            foreach (var shield in Loadout.InUse.Where(i => i.Kind == ItemKind.Shield).ToList())
            {
                damage = Mathf.CeilToInt(damage * 0.5f);
                WearItem(shield, 2);
            }
            foreach (var orb in Loadout.InUse.Where(i => i.Kind == ItemKind.Orb).ToList())
            {
                damage = Mathf.Max(0, damage - 3);
                WearItem(orb, 1);
            }
            var worn = Loadout.Armor.Where(a => a != null).ToList();
            if (worn.Count > 0)
            {
                damage = Mathf.CeilToInt(damage * (1f - 0.08f * worn.Count));
                WearItem(worn[UnityEngine.Random.Range(0, worn.Count)], 1);
            }
            Anim.PlayHurt();
            Health.Damage(Mathf.Max(1, damage));
        }

        // ---------- Habilidades ----------

        public float CooldownFraction(char slot)
        {
            return slot == 'Q' ? cooldownQ / cooldownQMax : cooldownE / cooldownEMax;
        }

        public void Cast(char slot)
        {
            string id = slot == 'Q' ? Loadout.AbilityQ : Loadout.AbilityE;
            var ability = AbilityLibrary.Get(id);
            if (ability == null) return; // Q/E vazios: nada acontece
            if ((slot == 'Q' ? cooldownQ : cooldownE) > 0f) return;
            ability.Cast(this);
            if (slot == 'Q') { cooldownQ = cooldownQMax = ability.Cooldown; }
            else { cooldownE = cooldownEMax = ability.Cooldown; }
        }

        public void BecomeInvisible(float seconds)
        {
            Invisible = true;
            Anim.Ghost = true;
            invisibleUntil = Time.time + seconds;
        }

        public void Dash(Vector3 dir, float distance, float duration, int damage)
        {
            dashUntil = Time.time + duration;
            dashVelocity = dir.normalized * (distance / duration);
            dashDamage = damage;
            dashHit.Clear();
            Anim.PlayAttack();
        }

        void DashDamage()
        {
            foreach (var e in CombatUtil.EnemiesInSphere(Position + Vector3.up, 1.5f))
                if (dashHit.Add(e)) e.TakeDamage(dashDamage);
        }

        // ---------- Visual do item na mao ----------

        void RefreshHeld()
        {
            var first = Loadout.InUse.FirstOrDefault();
            string sig = first == null ? "" : first.Name + first.Kind;
            if (sig == heldSignature) return;
            heldSignature = sig;
            if (heldVisual != null) Destroy(heldVisual);
            if (first != null && Anim.ArmR != null) heldVisual = ProceduralModels.HeldItem(Anim.ArmR, first);
        }
    }
}

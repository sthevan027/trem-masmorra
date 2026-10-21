using System;
using UnityEngine;

namespace TremMasmorra.Game
{
    public enum BossPattern { None, Slam, Volley, ChargeSlam }

    public class EnemyDef
    {
        public string Name;
        public int MaxHp;
        public float Speed;
        public int Damage;
        public float AttackRange;
        public float AttackCooldown;
        public Color Body;
        public Color Accent;
        public float Scale = 1f;
        public bool IsBoss;
        public bool Ranged;
        public BossPattern Pattern;
    }

    public static class EnemyCatalog
    {
        public static readonly EnemyDef FormigaCristal = new EnemyDef
        {
            Name = "Formiga de Cristal", MaxHp = 40, Speed = 3.2f, Damage = 8, AttackRange = 1.8f, AttackCooldown = 1.2f,
            Body = new Color(0.35f, 0.7f, 0.95f), Accent = new Color(0.7f, 0.9f, 1f), Scale = 0.9f,
        };

        public static readonly EnemyDef MonstroDeLama = new EnemyDef
        {
            Name = "Monstro de Lama", MaxHp = 65, Speed = 2.2f, Damage = 12, AttackRange = 2f, AttackCooldown = 1.6f,
            Body = new Color(0.45f, 0.32f, 0.2f), Accent = new Color(0.3f, 0.2f, 0.1f), Scale = 1.1f,
        };

        public static readonly EnemyDef Bruxo = new EnemyDef
        {
            Name = "Lodo Arremessador", MaxHp = 45, Speed = 2f, Damage = 9, AttackRange = 11f, AttackCooldown = 2f,
            Body = new Color(0.4f, 0.55f, 0.25f), Accent = new Color(0.6f, 0.8f, 0.3f), Scale = 1f, Ranged = true,
        };

        public static readonly EnemyDef GuerreiroGeode = new EnemyDef
        {
            Name = "Guerreiro Geode", MaxHp = 300, Speed = 2.6f, Damage = 18, AttackRange = 3f, AttackCooldown = 2.5f,
            Body = new Color(0.55f, 0.3f, 0.7f), Accent = new Color(0.85f, 0.6f, 1f), Scale = 2f,
            IsBoss = true, Pattern = BossPattern.Slam,
        };

        public static readonly EnemyDef ReiFormiga = new EnemyDef
        {
            Name = "Rainha dos Cristais", MaxHp = 450, Speed = 2.4f, Damage = 14, AttackRange = 12f, AttackCooldown = 2.2f,
            Body = new Color(0.2f, 0.55f, 0.85f), Accent = new Color(0.9f, 0.95f, 1f), Scale = 2.2f,
            IsBoss = true, Ranged = true, Pattern = BossPattern.Volley,
        };

        public static readonly EnemyDef Maquinista = new EnemyDef
        {
            Name = "O Maquinista do Trem", MaxHp = 650, Speed = 3f, Damage = 22, AttackRange = 3.2f, AttackCooldown = 3f,
            Body = new Color(0.75f, 0.2f, 0.2f), Accent = new Color(0.95f, 0.8f, 0.3f), Scale = 2.4f,
            IsBoss = true, Pattern = BossPattern.ChargeSlam,
        };

        public static readonly EnemyDef[] Common = { FormigaCristal, MonstroDeLama, Bruxo };
        public static readonly EnemyDef[] Bosses = { GuerreiroGeode, ReiFormiga, Maquinista };
    }

    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        public EnemyDef Def;
        public Health Health;
        public SimpleAnimator Anim;
        public bool IsDead { get; private set; }
        public event Action<Enemy> Killed;

        PlayerController player;
        CharacterController cc;
        float nextAttack;
        float chargeUntil;
        Vector3 chargeDir;
        bool chargeNext;

        public static Enemy Spawn(EnemyDef def, Vector3 pos, PlayerController target)
        {
            var go = new GameObject(def.Name);
            go.transform.position = pos;
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f * def.Scale;
            cc.radius = 0.5f * def.Scale;
            cc.center = new Vector3(0f, cc.height / 2f, 0f);
            var enemy = go.AddComponent<Enemy>();
            enemy.Def = def;
            enemy.player = target;
            enemy.cc = cc;
            enemy.Health = go.GetComponent<Health>();
            enemy.Health.Init(def.MaxHp);
            enemy.Anim = ProceduralModels.Creature(def.Body, def.Accent, def.Scale);
            enemy.Anim.transform.SetParent(go.transform, false);
            enemy.Health.Died += enemy.OnDied;
            enemy.nextAttack = Time.time + 1f;
            return enemy;
        }

        void Update()
        {
            if (IsDead || player == null) return;
            if (player.Invisible || player.Health.IsDead) { Anim.MoveSpeed = 0f; return; }

            Vector3 to = player.Position - transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            Vector3 dir = dist > 0.01f ? to / dist : transform.forward;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);

            if (Time.time < chargeUntil)
            {
                cc.SimpleMove(chargeDir * (Def.Speed * 4f));
                Anim.MoveSpeed = Def.Speed * 4f;
                if (Time.time + Time.deltaTime >= chargeUntil) Slam(4f, Def.Damage);
                return;
            }

            if (dist > Def.AttackRange * 0.9f)
            {
                cc.SimpleMove(dir * Def.Speed);
                Anim.MoveSpeed = Def.Speed;
            }
            else Anim.MoveSpeed = 0f;

            if (dist <= Def.AttackRange && Time.time >= nextAttack)
            {
                nextAttack = Time.time + Def.AttackCooldown;
                Attack(dir, dist);
            }
        }

        void Attack(Vector3 dir, float dist)
        {
            Anim.PlayAttack();
            Vector3 muzzle = transform.position + Vector3.up * (1.2f * Def.Scale) + dir * 0.8f;
            switch (Def.Pattern)
            {
                case BossPattern.Slam:
                    Slam(4.5f, Def.Damage);
                    break;
                case BossPattern.Volley:
                    for (int i = -1; i <= 1; i++)
                        Projectile.Spawn(muzzle, Quaternion.Euler(0, i * 18f, 0) * dir, Def.Damage,
                            new Color(0.5f, 0.85f, 1f), true, false, 14f);
                    break;
                case BossPattern.ChargeSlam:
                    if (chargeNext)
                    {
                        chargeDir = dir;
                        chargeUntil = Time.time + 0.6f;
                    }
                    else Slam(5f, Def.Damage);
                    chargeNext = !chargeNext;
                    break;
                default:
                    if (Def.Ranged)
                        Projectile.Spawn(muzzle, dir, Def.Damage, new Color(0.6f, 0.8f, 0.3f), true, false, 12f);
                    else if (dist <= Def.AttackRange * 1.2f)
                        player.TakeHit(Def.Damage);
                    break;
            }
        }

        void Slam(float radius, int damage)
        {
            var ring = ProceduralModels.Part(PrimitiveType.Cylinder, null, transform.position + Vector3.up * 0.1f,
                new Vector3(radius * 2f, 0.05f, radius * 2f), new Color(1f, 0.3f, 0.2f), "Impacto");
            Destroy(ring, 0.25f);
            if (Vector3.Distance(player.Position, transform.position) <= radius) player.TakeHit(damage);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            Anim.PlayHurt();
            Health.Damage(amount);
        }

        void OnDied()
        {
            IsDead = true;
            Anim.PlayDeath();
            Anim.MoveSpeed = 0f;
            cc.enabled = false;
            Killed?.Invoke(this);
            Destroy(gameObject, 2f);
        }
    }
}

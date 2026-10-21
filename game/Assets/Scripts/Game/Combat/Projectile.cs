using System.Collections.Generic;
using UnityEngine;

namespace TremMasmorra.Game
{
    public class Projectile : MonoBehaviour
    {
        public Vector3 Direction;
        public float Speed = 20f;
        public int Damage;
        public bool Pierce;
        public bool HitsPlayer;
        public float Life = 3f;
        public float Radius = 0.5f;

        readonly HashSet<Object> alreadyHit = new HashSet<Object>();

        public static Projectile Spawn(Vector3 pos, Vector3 dir, int damage, Color color, bool hitsPlayer,
            bool pierce = false, float speed = 20f)
        {
            var go = ProceduralModels.Part(PrimitiveType.Sphere, null, pos, Vector3.one * 0.4f, color, "Projetil");
            var p = go.AddComponent<Projectile>();
            p.Direction = dir.normalized;
            p.Damage = damage;
            p.HitsPlayer = hitsPlayer;
            p.Pierce = pierce;
            p.Speed = speed;
            return p;
        }

        void Update()
        {
            transform.position += Direction * (Speed * Time.deltaTime);
            Life -= Time.deltaTime;
            if (Life <= 0f) { Destroy(gameObject); return; }

            foreach (var col in Physics.OverlapSphere(transform.position, Radius))
            {
                if (HitsPlayer)
                {
                    var player = col.GetComponentInParent<PlayerController>();
                    if (player != null)
                    {
                        player.TakeHit(Damage);
                        Destroy(gameObject);
                        return;
                    }
                }
                else
                {
                    var enemy = col.GetComponentInParent<Enemy>();
                    if (enemy != null && !enemy.IsDead && alreadyHit.Add(enemy))
                    {
                        enemy.TakeDamage(Damage);
                        if (!Pierce) { Destroy(gameObject); return; }
                    }
                }
            }
        }
    }

    public static class CombatUtil
    {
        public static List<Enemy> EnemiesInSphere(Vector3 center, float radius)
        {
            var result = new List<Enemy>();
            foreach (var col in Physics.OverlapSphere(center, radius))
            {
                var e = col.GetComponentInParent<Enemy>();
                if (e != null && !e.IsDead && !result.Contains(e)) result.Add(e);
            }
            return result;
        }
    }
}

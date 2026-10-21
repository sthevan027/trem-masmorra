using System;
using System.Collections.Generic;
using UnityEngine;

namespace TremMasmorra.Game
{
    public class Ability
    {
        public string Id;
        public string Name;
        public float Cooldown;
        public Color Color;
        public Action<PlayerController> Cast;
    }

    public static class AbilityLibrary
    {
        static readonly Dictionary<string, Ability> all = Build();

        public static IEnumerable<string> Ids => all.Keys;

        public static Ability Get(string id) =>
            id != null && all.TryGetValue(id, out var a) ? a : null;

        static Dictionary<string, Ability> Build()
        {
            var d = new Dictionary<string, Ability>();
            void Add(string id, string name, float cd, Color color, Action<PlayerController> cast) =>
                d[id] = new Ability { Id = id, Name = name, Cooldown = cd, Color = color, Cast = cast };

            var fire = new Color(1f, 0.5f, 0.1f);
            var arcane = new Color(0.6f, 0.4f, 1f);
            var arrow = new Color(0.9f, 0.9f, 0.5f);
            var steel = new Color(0.8f, 0.85f, 0.9f);

            Add("bola_de_fogo", "Bola de Fogo", 3f, fire, p =>
                Projectile.Spawn(p.Chest, p.Aim, 35, fire, false, false, 22f));

            Add("raio_arcano", "Raio Arcano", 4f, arcane, p =>
                Blast(p, p.Position + p.Aim * 7f, 3.2f, 28, arcane));

            Add("flecha_perfurante", "Flecha Perfurante", 3f, arrow, p =>
                Projectile.Spawn(p.Chest, p.Aim, 28, arrow, false, true, 32f));

            Add("chuva_de_flechas", "Chuva de Flechas", 5f, arrow, p =>
                Blast(p, p.Position + p.Aim * 8f, 4f, 30, arrow));

            Add("golpe_giratorio", "Golpe Giratorio", 4f, steel, p =>
                Blast(p, p.Position, 3.5f, 30, steel));

            Add("investida", "Investida", 5f, steel, p => p.Dash(p.Aim, 6f, 0.2f, 25));

            Add("invisibilidade", "Invisibilidade", 10f, new Color(0.5f, 0.9f, 0.9f), p => p.BecomeInvisible(5f));
            return d;
        }

        static void Blast(PlayerController p, Vector3 center, float radius, int damage, Color color)
        {
            foreach (var e in CombatUtil.EnemiesInSphere(center, radius)) e.TakeDamage(damage);
            p.Anim.PlayAttack();
            var ring = ProceduralModels.Part(PrimitiveType.Cylinder, null, center + Vector3.up * 0.1f,
                new Vector3(radius * 2f, 0.05f, radius * 2f), color, "Efeito");
            UnityEngine.Object.Destroy(ring, 0.25f);
        }
    }
}

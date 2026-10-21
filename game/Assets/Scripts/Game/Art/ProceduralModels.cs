using System.Collections.Generic;
using TremMasmorra.Core.Character;
using TremMasmorra.Core.Items;
using UnityEngine;

namespace TremMasmorra.Game
{
    /// <summary>Modelos cartoon montados com primitivas arredondadas, cores chapadas e contorno.</summary>
    public static class ProceduralModels
    {
        public const float CharacterOutline = 0.006f;
        public const float WorldOutline = 0.0035f;

        static readonly Dictionary<(Color, float), Material> cache = new Dictionary<(Color, float), Material>();
        static Shader toon, shadow;
        static Material shadowMaterial;

        public static readonly Color[] SkinTones =
        {
            new Color(1f, 0.85f, 0.72f), new Color(0.93f, 0.72f, 0.52f),
            new Color(0.7f, 0.48f, 0.32f), new Color(0.45f, 0.3f, 0.2f),
        };

        public static readonly Color[] HairColors =
        {
            new Color(0.25f, 0.15f, 0.1f), new Color(0.85f, 0.45f, 0.15f), new Color(0.98f, 0.85f, 0.35f),
        };

        static Shader Toon
        {
            get
            {
                // Resources garante o shader no build; Shader.Find sozinho falha fora do editor.
                if (toon == null) toon = Resources.Load<Shader>("TremMasmorra/Toon");
                if (toon == null) toon = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
                return toon;
            }
        }

        public static Material Mat(Color c, float outline = CharacterOutline)
        {
            if (cache.TryGetValue((c, outline), out var m) && m != null) return m;
            m = new Material(Toon) { color = c };
            if (m.HasProperty("_OutlineWidth")) m.SetFloat("_OutlineWidth", outline);
            cache[(c, outline)] = m;
            return m;
        }

        public static GameObject Part(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Color c,
            string name, float outline = CharacterOutline)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Mat(c, outline);
            return go;
        }

        /// <summary>Disco escuro sob os pes. Fica no objeto do personagem, nao no modelo (que gira ao morrer).</summary>
        public static GameObject BlobShadow(Transform parent, float radius)
        {
            if (shadowMaterial == null)
            {
                if (shadow == null) shadow = Resources.Load<Shader>("TremMasmorra/Shadow");
                shadowMaterial = new Material(shadow != null ? shadow : Toon);
                shadowMaterial.color = new Color(0f, 0f, 0f, 0.35f);
            }
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Sombra";
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, 0.03f, 0);
            go.transform.localScale = new Vector3(radius * 2f, 0.005f, radius * 2f);
            go.GetComponent<Renderer>().sharedMaterial = shadowMaterial;
            return go;
        }

        public static SimpleAnimator Humanoid(CharacterLook look, Color shirt, Color pants)
        {
            var model = new GameObject("Model");
            var t = model.transform;
            var skin = SkinTones[look.SkinTone];
            bool fem = look.Gender == Gender.Feminino;
            float bodyW = fem ? 0.42f : 0.5f;
            var dark = new Color(0.12f, 0.1f, 0.15f);

            Part(PrimitiveType.Capsule, t, new Vector3(0, 0.98f, 0), new Vector3(bodyW, 0.34f, 0.4f), shirt, "Corpo");
            Part(PrimitiveType.Sphere, t, new Vector3(0, 1.62f, 0), new Vector3(0.74f, 0.7f, 0.7f), skin, "Cabeca");
            Part(PrimitiveType.Sphere, t, new Vector3(-0.15f, 1.66f, 0.31f), new Vector3(0.09f, 0.14f, 0.05f), dark, "OlhoE");
            Part(PrimitiveType.Sphere, t, new Vector3(0.15f, 1.66f, 0.31f), new Vector3(0.09f, 0.14f, 0.05f), dark, "OlhoD");
            Part(PrimitiveType.Cube, t, new Vector3(0, 1.5f, 0.33f), new Vector3(0.14f, 0.03f, 0.03f), dark, "Boca");
            AddHair(t, look.HairStyle, fem);

            var anim = model.AddComponent<SimpleAnimator>();
            anim.ArmL = Limb(t, new Vector3(-(bodyW / 2 + 0.13f), 1.2f, 0), 0.62f, 0.14f, skin, "BracoE", skin);
            anim.ArmR = Limb(t, new Vector3(bodyW / 2 + 0.13f, 1.2f, 0), 0.62f, 0.14f, skin, "BracoD", skin);
            anim.LegL = Limb(t, new Vector3(-0.14f, 0.64f, 0), 0.62f, 0.18f, pants, "PernaE", dark);
            anim.LegR = Limb(t, new Vector3(0.14f, 0.64f, 0), 0.62f, 0.18f, pants, "PernaD", dark);
            anim.Renderers = model.GetComponentsInChildren<Renderer>();
            t.localScale = Vector3.one * look.Height;
            return anim;
        }

        /// <param name="variant">0 = orelhas, 1 = olho unico com crista, 2 = antenas</param>
        public static SimpleAnimator Creature(Color body, Color accent, float scale, int variant = 0, bool crown = false)
        {
            var model = new GameObject("Model");
            var t = model.transform;
            var dark = new Color(0.1f, 0.08f, 0.12f);
            var belly = Color.Lerp(accent, Color.white, 0.4f);

            Part(PrimitiveType.Sphere, t, new Vector3(0, 0.95f, 0), new Vector3(1.15f, 1.05f, 0.95f), body, "Corpo");
            Part(PrimitiveType.Sphere, t, new Vector3(0, 0.8f, 0.3f), new Vector3(0.7f, 0.6f, 0.45f), belly, "Barriga");

            if (variant == 1)
            {
                Part(PrimitiveType.Sphere, t, new Vector3(0, 1.2f, 0.4f), new Vector3(0.45f, 0.45f, 0.3f), Color.white, "Olho");
                Part(PrimitiveType.Sphere, t, new Vector3(0, 1.2f, 0.52f), new Vector3(0.2f, 0.2f, 0.1f), dark, "Pupila");
                for (int i = -1; i <= 1; i++)
                    Part(PrimitiveType.Sphere, t, new Vector3(i * 0.3f, 1.72f - Mathf.Abs(i) * 0.1f, 0), new Vector3(0.22f, 0.3f, 0.22f), accent, "Crista");
            }
            else
            {
                foreach (float x in new[] { -0.22f, 0.22f })
                {
                    Part(PrimitiveType.Sphere, t, new Vector3(x, 1.22f, 0.38f), new Vector3(0.3f, 0.34f, 0.2f), Color.white, "Olho");
                    Part(PrimitiveType.Sphere, t, new Vector3(x, 1.22f, 0.5f), new Vector3(0.14f, 0.16f, 0.08f), dark, "Pupila");
                }
                if (variant == 0)
                {
                    foreach (float x in new[] { -0.38f, 0.38f })
                        Part(PrimitiveType.Sphere, t, new Vector3(x, 1.72f, 0), new Vector3(0.28f, 0.36f, 0.22f), accent, "Orelha");
                }
                else
                {
                    foreach (float x in new[] { -0.25f, 0.25f })
                    {
                        Part(PrimitiveType.Cylinder, t, new Vector3(x, 1.8f, 0), new Vector3(0.05f, 0.25f, 0.05f), dark, "Antena");
                        Part(PrimitiveType.Sphere, t, new Vector3(x, 2.08f, 0), new Vector3(0.16f, 0.16f, 0.16f), accent, "PontaAntena");
                    }
                }
            }
            Part(PrimitiveType.Cube, t, new Vector3(0, 0.98f, 0.45f), new Vector3(0.3f, 0.04f, 0.04f), dark, "Boca");

            if (crown)
            {
                var gold = new Color(1f, 0.82f, 0.2f);
                Part(PrimitiveType.Cube, t, new Vector3(0, 1.62f, 0), new Vector3(0.6f, 0.12f, 0.4f), gold, "CoroaBase");
                foreach (float x in new[] { -0.24f, 0f, 0.24f })
                    Part(PrimitiveType.Cube, t, new Vector3(x, 1.76f, 0), new Vector3(0.12f, 0.2f, 0.12f), gold, "CoroaPonta");
            }

            var anim = model.AddComponent<SimpleAnimator>();
            anim.ArmL = Limb(t, new Vector3(-0.62f, 1.0f, 0), 0.6f, 0.18f, accent, "BracoE", accent);
            anim.ArmR = Limb(t, new Vector3(0.62f, 1.0f, 0), 0.6f, 0.18f, accent, "BracoD", accent);
            anim.LegL = Limb(t, new Vector3(-0.3f, 0.4f, 0), 0.42f, 0.26f, body * 0.8f, "PernaE", dark);
            anim.LegR = Limb(t, new Vector3(0.3f, 0.4f, 0), 0.42f, 0.26f, body * 0.8f, "PernaD", dark);
            anim.Renderers = model.GetComponentsInChildren<Renderer>();
            t.localScale = Vector3.one * scale;
            return anim;
        }

        /// <summary>Visual simples do item segurado na mao direita.</summary>
        public static GameObject HeldItem(Transform hand, Item item)
        {
            Color c = item.Rarity == Rarity.Epico ? new Color(0.75f, 0.35f, 0.95f)
                : item.Rarity == Rarity.Raro ? new Color(0.35f, 0.65f, 1f) : new Color(0.8f, 0.8f, 0.85f);
            var root = new GameObject("Held");
            root.transform.SetParent(hand, false);
            root.transform.localPosition = new Vector3(0, -0.62f, 0.1f);
            switch (item.Kind)
            {
                case ItemKind.Melee:
                    Part(PrimitiveType.Cube, root.transform, new Vector3(0, 0, 0.5f), new Vector3(0.1f, 0.05f, 0.9f), c, "Lamina");
                    Part(PrimitiveType.Cube, root.transform, new Vector3(0, 0, 0.05f), new Vector3(0.28f, 0.06f, 0.07f), new Color(0.9f, 0.7f, 0.2f), "Guarda");
                    break;
                case ItemKind.Ranged:
                    Part(PrimitiveType.Capsule, root.transform, new Vector3(0, 0.1f, 0.2f), new Vector3(0.07f, 0.45f, 0.07f), new Color(0.6f, 0.38f, 0.15f), "Arco");
                    break;
                case ItemKind.Magic:
                    Part(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.2f, 0.1f), new Vector3(0.07f, 0.45f, 0.07f), new Color(0.55f, 0.35f, 0.15f), "Cajado");
                    Part(PrimitiveType.Sphere, root.transform, new Vector3(0, 0.72f, 0.1f), new Vector3(0.24f, 0.24f, 0.24f), c, "Cristal");
                    break;
                case ItemKind.Shield:
                    Part(PrimitiveType.Sphere, root.transform, new Vector3(0.2f, 0.1f, 0.1f), new Vector3(0.12f, 0.6f, 0.55f), c, "Escudo");
                    break;
                default:
                    Part(PrimitiveType.Sphere, root.transform, new Vector3(0, 0.1f, 0.2f), new Vector3(0.28f, 0.28f, 0.28f), c, "Orbe");
                    break;
            }
            return root;
        }

        /// <summary>Membro articulado: o pivo fica no ombro/quadril e a forma pende para baixo.</summary>
        static Transform Limb(Transform parent, Vector3 pivotPos, float length, float width, Color c, string name, Color tip)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = pivotPos;
            Part(PrimitiveType.Capsule, pivot, new Vector3(0, -length / 2f, 0), new Vector3(width, length / 2f, width), c, "Forma");
            Part(PrimitiveType.Sphere, pivot, new Vector3(0, -length + width * 0.2f, width * 0.35f), new Vector3(width * 1.25f, width * 0.95f, width * 1.6f), tip, "Ponta");
            return pivot;
        }

        static void AddHair(Transform parent, int style, bool fem)
        {
            var c = HairColors[style];
            Part(PrimitiveType.Sphere, parent, new Vector3(0, 1.86f, -0.04f), new Vector3(0.8f, 0.46f, 0.78f), c, "Cabelo");
            if (style == 1 || fem)
                Part(PrimitiveType.Sphere, parent, new Vector3(0, 1.5f, -0.22f), new Vector3(0.62f, 0.78f, 0.34f), c, "CabeloLongo");
            if (style == 2)
            {
                Part(PrimitiveType.Sphere, parent, new Vector3(0, 2.14f, 0), new Vector3(0.2f, 0.3f, 0.2f), c, "Topete");
                Part(PrimitiveType.Sphere, parent, new Vector3(0.12f, 2.1f, 0), new Vector3(0.14f, 0.24f, 0.14f), c, "Topete2");
            }
            if (fem)
                Part(PrimitiveType.Sphere, parent, new Vector3(0.28f, 1.85f, -0.1f), new Vector3(0.22f, 0.16f, 0.16f), new Color(1f, 0.45f, 0.6f), "Laco");
        }
    }
}

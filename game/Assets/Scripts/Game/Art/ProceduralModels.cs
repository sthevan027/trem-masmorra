using System.Collections.Generic;
using TremMasmorra.Core.Character;
using TremMasmorra.Core.Items;
using UnityEngine;

namespace TremMasmorra.Game
{
    /// <summary>Modelos low poly montados com primitivas, cores chapadas.</summary>
    public static class ProceduralModels
    {
        static readonly Dictionary<Color, Material> cache = new Dictionary<Color, Material>();

        public static readonly Color[] SkinTones =
        {
            new Color(0.96f, 0.80f, 0.69f), new Color(0.85f, 0.65f, 0.48f),
            new Color(0.62f, 0.42f, 0.28f), new Color(0.38f, 0.25f, 0.17f),
        };

        public static readonly Color[] HairColors =
        {
            new Color(0.15f, 0.1f, 0.07f), new Color(0.55f, 0.32f, 0.12f), new Color(0.9f, 0.8f, 0.4f),
        };

        public static Material Mat(Color c)
        {
            if (cache.TryGetValue(c, out var m) && m != null) return m;
            var shader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            m = new Material(shader) { color = c };
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.1f);
            cache[c] = m;
            return m;
        }

        public static GameObject Part(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Color c, string name)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Mat(c);
            return go;
        }

        public static SimpleAnimator Humanoid(CharacterLook look, Color shirt, Color pants)
        {
            var model = new GameObject("Model");
            var skin = SkinTones[look.SkinTone];
            bool fem = look.Gender == Gender.Feminino;
            float bodyW = fem ? 0.5f : 0.62f;

            Part(PrimitiveType.Cube, model.transform, new Vector3(0, 1.1f, 0), new Vector3(bodyW, 0.8f, 0.3f), shirt, "Corpo");
            Part(PrimitiveType.Sphere, model.transform, new Vector3(0, 1.75f, 0), new Vector3(0.42f, 0.45f, 0.42f), skin, "Cabeca");
            AddHair(model.transform, look.HairStyle, fem);

            var anim = model.AddComponent<SimpleAnimator>();
            anim.ArmL = Limb(model.transform, new Vector3(-(bodyW / 2 + 0.1f), 1.45f, 0), new Vector3(0.18f, 0.7f, 0.18f), skin, "BracoE");
            anim.ArmR = Limb(model.transform, new Vector3(bodyW / 2 + 0.1f, 1.45f, 0), new Vector3(0.18f, 0.7f, 0.18f), skin, "BracoD");
            anim.LegL = Limb(model.transform, new Vector3(-0.15f, 0.7f, 0), new Vector3(0.22f, 0.7f, 0.22f), pants, "PernaE");
            anim.LegR = Limb(model.transform, new Vector3(0.15f, 0.7f, 0), new Vector3(0.22f, 0.7f, 0.22f), pants, "PernaD");
            anim.Renderers = model.GetComponentsInChildren<Renderer>();
            model.transform.localScale = Vector3.one * look.Height;
            return anim;
        }

        public static SimpleAnimator Creature(Color body, Color accent, float scale)
        {
            var model = new GameObject("Model");
            Part(PrimitiveType.Cube, model.transform, new Vector3(0, 1.0f, 0), new Vector3(0.9f, 0.9f, 0.6f), body, "Corpo");
            Part(PrimitiveType.Cube, model.transform, new Vector3(0, 1.75f, 0.05f), new Vector3(0.6f, 0.5f, 0.55f), accent, "Cabeca");
            Part(PrimitiveType.Cube, model.transform, new Vector3(-0.15f, 1.8f, 0.34f), new Vector3(0.12f, 0.12f, 0.05f), Color.white, "OlhoE");
            Part(PrimitiveType.Cube, model.transform, new Vector3(0.15f, 1.8f, 0.34f), new Vector3(0.12f, 0.12f, 0.05f), Color.white, "OlhoD");
            var anim = model.AddComponent<SimpleAnimator>();
            anim.ArmL = Limb(model.transform, new Vector3(-0.6f, 1.4f, 0), new Vector3(0.25f, 0.8f, 0.25f), accent, "BracoE");
            anim.ArmR = Limb(model.transform, new Vector3(0.6f, 1.4f, 0), new Vector3(0.25f, 0.8f, 0.25f), accent, "BracoD");
            anim.LegL = Limb(model.transform, new Vector3(-0.25f, 0.6f, 0), new Vector3(0.3f, 0.6f, 0.3f), body * 0.8f, "PernaE");
            anim.LegR = Limb(model.transform, new Vector3(0.25f, 0.6f, 0), new Vector3(0.3f, 0.6f, 0.3f), body * 0.8f, "PernaD");
            anim.Renderers = model.GetComponentsInChildren<Renderer>();
            model.transform.localScale = Vector3.one * scale;
            return anim;
        }

        /// <summary>Visual simples do item segurado na mao direita.</summary>
        public static GameObject HeldItem(Transform hand, Item item)
        {
            Color c = item.Rarity == Rarity.Epico ? new Color(0.7f, 0.3f, 0.9f)
                : item.Rarity == Rarity.Raro ? new Color(0.3f, 0.6f, 0.95f) : new Color(0.6f, 0.6f, 0.6f);
            var root = new GameObject("Held");
            root.transform.SetParent(hand, false);
            root.transform.localPosition = new Vector3(0, -0.7f, 0.1f);
            switch (item.Kind)
            {
                case ItemKind.Melee:
                    Part(PrimitiveType.Cube, root.transform, new Vector3(0, 0, 0.45f), new Vector3(0.08f, 0.08f, 0.9f), c, "Lamina");
                    break;
                case ItemKind.Ranged:
                    Part(PrimitiveType.Cube, root.transform, new Vector3(0, 0.1f, 0.2f), new Vector3(0.06f, 0.8f, 0.06f), new Color(0.5f, 0.3f, 0.1f), "Arco");
                    break;
                case ItemKind.Magic:
                    Part(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.2f, 0.1f), new Vector3(0.06f, 0.7f, 0.06f), new Color(0.4f, 0.25f, 0.1f), "Cajado");
                    Part(PrimitiveType.Sphere, root.transform, new Vector3(0, 0.95f, 0.1f), new Vector3(0.2f, 0.2f, 0.2f), c, "Cristal");
                    break;
                case ItemKind.Shield:
                    Part(PrimitiveType.Cube, root.transform, new Vector3(0.2f, 0.1f, 0.1f), new Vector3(0.1f, 0.6f, 0.5f), c, "Escudo");
                    break;
                default:
                    Part(PrimitiveType.Sphere, root.transform, new Vector3(0, 0.1f, 0.2f), new Vector3(0.25f, 0.25f, 0.25f), c, "Orbe");
                    break;
            }
            return root;
        }

        static Transform Limb(Transform parent, Vector3 pivotPos, Vector3 size, Color c, string name)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = pivotPos;
            Part(PrimitiveType.Cube, pivot, new Vector3(0, -size.y / 2, 0), size, c, "Forma");
            return pivot;
        }

        static void AddHair(Transform parent, int style, bool fem)
        {
            var c = HairColors[style];
            Part(PrimitiveType.Cube, parent, new Vector3(0, 1.97f, 0), new Vector3(0.46f, 0.14f, 0.46f), c, "Cabelo");
            if (style == 1 || fem)
                Part(PrimitiveType.Cube, parent, new Vector3(0, 1.7f, -0.2f), new Vector3(0.46f, 0.55f, 0.1f), c, "CabeloLongo");
            if (style == 2)
                Part(PrimitiveType.Cube, parent, new Vector3(0, 2.1f, 0), new Vector3(0.12f, 0.2f, 0.12f), c, "Topete");
        }
    }
}

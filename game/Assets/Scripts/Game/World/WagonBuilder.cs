using System.Collections.Generic;
using UnityEngine;

namespace TremMasmorra.Game
{
    public class WagonData
    {
        public GameObject Root;
        public GameObject Door;
        public List<Vector3> SpawnPoints = new List<Vector3>();
        public Vector3 PlayerStart = new Vector3(0f, 0.1f, 2.5f);
    }

    public static class WagonBuilder
    {
        public const float Width = 14f;
        public const float Height = 5f;
        public const float Length = 40f;
        public const float ExitZ = 38.8f;

        public static WagonData Build(int wagon, bool boss, System.Random rng)
        {
            var data = new WagonData { Root = new GameObject("Vagao " + wagon) };
            var t = data.Root.transform;

            float hue = (wagon * 0.11f) % 1f;
            Color wall = Color.HSVToRGB(hue, boss ? 0.55f : 0.25f, boss ? 0.45f : 0.5f);
            Color floor = Color.HSVToRGB(hue, 0.2f, 0.25f);
            Color trim = Color.HSVToRGB(hue, 0.5f, 0.75f);

            Solid(t, new Vector3(0, -0.25f, Length / 2), new Vector3(Width, 0.5f, Length), floor, "Chao");
            Solid(t, new Vector3(0, Height + 0.25f, Length / 2), new Vector3(Width, 0.5f, Length), wall * 0.7f, "Teto");
            Solid(t, new Vector3(-Width / 2 - 0.25f, Height / 2, Length / 2), new Vector3(0.5f, Height, Length), wall, "ParedeE");
            Solid(t, new Vector3(Width / 2 + 0.25f, Height / 2, Length / 2), new Vector3(0.5f, Height, Length), wall, "ParedeD");
            Solid(t, new Vector3(0, Height / 2, -0.25f), new Vector3(Width, Height, 0.5f), wall, "ParedeTras");

            // Parede da frente com abertura de 4 m para a porta.
            float side = (Width - 4f) / 2f;
            Solid(t, new Vector3(-(2f + side / 2f), Height / 2, Length + 0.25f), new Vector3(side, Height, 0.5f), wall, "FrenteE");
            Solid(t, new Vector3(2f + side / 2f, Height / 2, Length + 0.25f), new Vector3(side, Height, 0.5f), wall, "FrenteD");
            Solid(t, new Vector3(0, Height - 0.5f, Length + 0.25f), new Vector3(4f, 1f, 0.5f), wall, "FrenteCima");
            data.Door = Solid(t, new Vector3(0, 2f, Length + 0.25f), new Vector3(4f, 4f, 0.5f),
                boss ? new Color(0.8f, 0.15f, 0.15f) : new Color(0.7f, 0.5f, 0.2f), "Porta");

            // Faixas decorativas ao longo das paredes.
            for (int z = 4; z < Length; z += 8)
            {
                Deco(t, new Vector3(-Width / 2 + 0.05f, 2.5f, z), new Vector3(0.1f, 0.4f, 3f), trim);
                Deco(t, new Vector3(Width / 2 - 0.05f, 2.5f, z), new Vector3(0.1f, 0.4f, 3f), trim);
                var lightGo = new GameObject("Luz");
                lightGo.transform.SetParent(t, false);
                lightGo.transform.localPosition = new Vector3(0, Height - 0.6f, z + 4f);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 14f;
                light.intensity = 1.2f;
                light.color = boss ? new Color(1f, 0.7f, 0.6f) : new Color(1f, 0.92f, 0.8f);
            }

            // Caixotes junto das paredes (decoracao com colisao).
            for (int i = 0; i < 6; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * (Width / 2 - 1f);
                float z = 7f + i * 5.5f;
                Solid(t, new Vector3(x, 0.6f, z), new Vector3(1.2f, 1.2f, 1.2f), trim * 0.6f, "Caixote");
            }

            // Placa com o numero do vagao sobre a porta.
            var sign = new GameObject("Placa");
            sign.transform.SetParent(t, false);
            sign.transform.localPosition = new Vector3(0, Height - 1.2f, Length - 0.2f);
            sign.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            var text = sign.AddComponent<TextMesh>();
            text.text = boss ? "BOSS  -  VAGAO " + wagon : "VAGAO " + wagon;
            text.characterSize = 0.25f;
            text.fontSize = 48;
            text.anchor = TextAnchor.MiddleCenter;
            text.font = Ui.Font;
            sign.GetComponent<MeshRenderer>().sharedMaterial = Ui.Font.material;

            for (int i = 0; i < 12; i++)
                data.SpawnPoints.Add(new Vector3((float)(rng.NextDouble() * 9 - 4.5), 0.1f, (float)(rng.NextDouble() * 20 + 13)));
            return data;
        }

        public static void OpenDoor(WagonData data)
        {
            if (data.Door != null) data.Door.SetActive(false);
        }

        static GameObject Solid(Transform parent, Vector3 pos, Vector3 size, Color c, string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = ProceduralModels.Mat(c);
            return go;
        }

        static void Deco(Transform parent, Vector3 pos, Vector3 size, Color c)
        {
            var go = Solid(parent, pos, size, c, "Faixa");
            Object.Destroy(go.GetComponent<Collider>());
        }
    }
}

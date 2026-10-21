using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TremMasmorra.Core.Cards;
using TremMasmorra.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace TremMasmorra.PlayTests
{
    /// <summary>Gera capturas de tela de cada fase do jogo. Precisa de GPU: rode com GRAPHICS=1.</summary>
    [Category("Visual")]
    public class VisualSmokeTests
    {
        const string OutDir = "D:/Projetos/trem-masmorra/.superpowers/shots";
        GameManager gm;
        RenderTexture rt;
        Texture2D tex;

        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(OutDir);
            PlayerController.InputBlocked = false;
            Time.timeScale = 1f;
            gm = new GameObject("GameManager").AddComponent<GameManager>();
            rt = new RenderTexture(1280, 720, 24);
            tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            gm.Cam.targetTexture = rt;
            gm.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            gm.Canvas.worldCamera = gm.Cam;
            gm.Canvas.planeDistance = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            PlayerController.InputBlocked = false;
            foreach (var root in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.parent == null).ToList())
                if (root != null) Object.Destroy(root.gameObject);
        }

        void Shot(string name)
        {
            Canvas.ForceUpdateCanvases();
            gm.Cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(OutDir + "/" + name + ".png", tex.EncodeToPNG());
        }

        IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }

        [UnityTest]
        public IEnumerator CaptureEveryStage()
        {
            yield return Frames(3);
            Shot("01-titulo");

            gm.GetType().GetMethod("ShowCreator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(gm, null);
            yield return Frames(5);
            Shot("02-criador");

            gm.StartRun();
            yield return Frames(30);
            Shot("03-vagao1");

            // vagao 3 (boss)
            for (int w = 1; w <= 2; w++)
            {
                foreach (var e in gm.Alive.ToList()) e.TakeDamage(999999);
                yield return null;
                gm.Player.Teleport(new Vector3(0, 0.1f, 39.2f), Quaternion.identity);
                yield return Frames(3);
            }
            yield return Frames(20);
            Shot("04-vagao3-boss");

            var boss = gm.Alive.First(e => e.Def.IsBoss);
            boss.TakeDamage(999999);
            yield return null;
            var cards = Object.FindFirstObjectByType<CardRevealScreen>();
            for (int i = 0; i < 4; i++) yield return null;
            Shot("05-cartas-empilhadas");
            float start = Time.unscaledTime;
            while (Time.unscaledTime - start < 1.0f) yield return null;
            Shot("06-cartas-leque");
            start = Time.unscaledTime;
            while (Time.unscaledTime - start < 1.2f) yield return null;
            Shot("07-cartas-reveladas");
            cards.ForcePick(0);
            yield return Frames(3);

            Object.FindFirstObjectByType<InventoryScreen>().Open();
            yield return Frames(3);
            Shot("08-inventario");
            Object.FindFirstObjectByType<InventoryScreen>().Close();
            yield return Frames(3);
            Shot("09-hud-apos-boss");
            Assert.Pass();
        }
    }
}

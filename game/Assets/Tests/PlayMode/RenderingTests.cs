using System.Collections;
using System.Linq;
using NUnit.Framework;
using TremMasmorra.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace TremMasmorra.PlayTests
{
    /// <summary>Guardas contra os defeitos vistos no executavel (que o editor esconde).</summary>
    public class RenderingTests
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

        [Test]
        public void ToonShader_IsLoadableFromResources_SoItShipsInBuilds()
        {
            Assert.IsNotNull(Resources.Load<Shader>("TremMasmorra/Toon"), "Toon.shader precisa estar em Resources/TremMasmorra");
            Assert.IsNotNull(Resources.Load<Shader>("TremMasmorra/Shadow"), "Shadow.shader precisa estar em Resources/TremMasmorra");
        }

        [Test]
        public void Materials_UseToonShader_NotAnEditorOnlyFallback()
        {
            var mat = ProceduralModels.Mat(Color.red);
            Assert.AreEqual("TremMasmorra/Toon", mat.shader.name);
            Assert.IsFalse(mat.renderQueue >= 3000, "o material nao pode ser transparente");
        }

        [UnityTest]
        public IEnumerator WagonSign_IsNotMirroredForAPlayerInsideTheWagon()
        {
            yield return null;
            gm.StartRun();
            yield return null;
            var sign = GameObject.Find("Placa").transform;
            // Quem esta dentro do vagao olha para +Z. Um TextMesh sem rotacao le certo para quem olha ao longo de +Z;
            // girado 180 graus ele aparece espelhado.
            Assert.Greater(Vector3.Dot(sign.forward, Vector3.forward), 0f,
                "a placa esta virada de costas para quem esta dentro do vagao");
        }

        [UnityTest]
        public IEnumerator Camera_NeverClosesInOnThePlayer_NearWallsAndCorners()
        {
            yield return null;
            gm.StartRun();
            yield return null;
            var cam = gm.Cam.GetComponent<ThirdPersonCamera>();
            var spots = new[]
            {
                new Vector3(0, 0.1f, 0.6f), new Vector3(-6.4f, 0.1f, 0.6f), new Vector3(6.4f, 0.1f, 39f),
                new Vector3(-6.4f, 0.1f, 39f), new Vector3(0, 0.1f, 39f),
            };
            foreach (var spot in spots)
            {
                gm.Player.Teleport(spot, Quaternion.identity);
                foreach (var yaw in new[] { 0f, 90f, 180f, 270f })
                {
                    cam.Yaw = yaw;
                    yield return null;
                    yield return null;
                    float d = Vector3.Distance(gm.Cam.transform.position, gm.Player.Position + Vector3.up * 1.0f);
                    Assert.GreaterOrEqual(d, 3.0f, "camera colada no jogador em " + spot + " yaw " + yaw);
                }
            }
        }
    }
}

using System;
using TremMasmorra.Core.Character;
using UnityEngine;
using UnityEngine.UI;

namespace TremMasmorra.Game
{
    /// <summary>Tela inicial, criacao de personagem e telas de fim (morte e vitoria).</summary>
    public class Menus : MonoBehaviour
    {
        Transform panel;

        public static Menus Create(Transform canvas)
        {
            var go = new GameObject("Menus", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(canvas, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var menus = go.AddComponent<Menus>();
            menus.panel = go.transform;
            return menus;
        }

        public void Hide() => Ui.Clear(panel);

        public void ShowTitle(Action onPlay, Action onQuit)
        {
            Ui.Clear(panel);
            Ui.Stretch(panel, "Fundo", new Color(0.04f, 0.04f, 0.08f, 0.85f));
            var c = new Vector2(0.5f, 0.5f);
            Ui.Label(panel, "TREM MASMORRA", 96, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f), c, new Vector2(-600, 120), new Vector2(1200, 140));
            Ui.Label(panel, "Um vagão. Um monstro. Mais um vagão...", 28, TextAnchor.MiddleCenter, Color.white, c, new Vector2(-600, 30), new Vector2(1200, 40));
            Ui.Button(panel, "Jogar", c, new Vector2(-150, -60), new Vector2(300, 70), onPlay, 32);
            Ui.Button(panel, "Sair", c, new Vector2(-150, -150), new Vector2(300, 70), onQuit, 32);
        }

        public void ShowCreator(CharacterLook look, Action onChanged, Action onStart)
        {
            Ui.Clear(panel);
            var l = new Vector2(0, 0.5f);
            Ui.Box(panel, "Painel", new Color(0.04f, 0.04f, 0.08f, 0.85f), l, new Vector2(60, 0), new Vector2(560, 660));
            Ui.Label(panel, "CRIAR PERSONAGEM", 36, TextAnchor.MiddleLeft, Color.white, l, new Vector2(90, 270), new Vector2(500, 50));
            Ui.Label(panel, "Só muda o visual. Poder vem das cartas dos bosses.", 18, TextAnchor.MiddleLeft, new Color(0.8f, 0.8f, 0.9f), l, new Vector2(90, 220), new Vector2(500, 30));

            Row("Gênero", l, 150, look.Gender.ToString(),
                () => { look.Gender = look.Gender == Gender.Masculino ? Gender.Feminino : Gender.Masculino; onChanged(); Refresh(look, onChanged, onStart); },
                () => { look.Gender = look.Gender == Gender.Masculino ? Gender.Feminino : Gender.Masculino; onChanged(); Refresh(look, onChanged, onStart); });
            Row("Altura", l, 60, look.Height.ToString("0.00") + "x",
                () => { look.Height -= 0.05f; onChanged(); Refresh(look, onChanged, onStart); },
                () => { look.Height += 0.05f; onChanged(); Refresh(look, onChanged, onStart); });
            Row("Cabelo", l, -30, (look.HairStyle + 1) + " / " + CharacterLook.HairStyles,
                () => { look.HairStyle--; onChanged(); Refresh(look, onChanged, onStart); },
                () => { look.HairStyle++; onChanged(); Refresh(look, onChanged, onStart); });
            Row("Pele", l, -120, (look.SkinTone + 1) + " / " + CharacterLook.SkinTones,
                () => { look.SkinTone--; onChanged(); Refresh(look, onChanged, onStart); },
                () => { look.SkinTone++; onChanged(); Refresh(look, onChanged, onStart); });
            Ui.Button(panel, "Começar", l, new Vector2(150, -260), new Vector2(340, 70), onStart, 32);
        }

        void Refresh(CharacterLook look, Action onChanged, Action onStart) => ShowCreator(look, onChanged, onStart);

        void Row(string title, Vector2 anchor, float y, string value, Action prev, Action next)
        {
            Ui.Label(panel, title, 26, TextAnchor.MiddleLeft, Color.white, anchor, new Vector2(90, y + 20), new Vector2(160, 40));
            Ui.Button(panel, "<", anchor, new Vector2(250, y), new Vector2(60, 50), prev);
            Ui.Label(panel, value, 24, TextAnchor.MiddleCenter, Color.white, anchor, new Vector2(315, y + 5), new Vector2(160, 40));
            Ui.Button(panel, ">", anchor, new Vector2(490, y), new Vector2(60, 50), next);
        }

        public void ShowEnd(string title, string subtitle, string buttonLabel, Action onClick)
        {
            Ui.Clear(panel);
            Ui.Stretch(panel, "Fundo", new Color(0, 0, 0, 0.8f));
            var c = new Vector2(0.5f, 0.5f);
            Ui.Label(panel, title, 90, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f), c, new Vector2(-600, 80), new Vector2(1200, 120));
            Ui.Label(panel, subtitle, 30, TextAnchor.MiddleCenter, Color.white, c, new Vector2(-600, -10), new Vector2(1200, 50));
            Ui.Button(panel, buttonLabel, c, new Vector2(-170, -100), new Vector2(340, 70), onClick, 30);
        }
    }
}

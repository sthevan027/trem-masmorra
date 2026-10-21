using System;
using TremMasmorra.Core.Items;
using UnityEngine;
using UnityEngine.UI;

namespace TremMasmorra.Game
{
    /// <summary>Helpers para montar uGUI por codigo.</summary>
    public static class Ui
    {
        static Font font;

        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        public static Color RarityColor(Rarity r) =>
            r == Rarity.Epico ? new Color(0.65f, 0.3f, 0.9f) : r == Rarity.Raro ? new Color(0.3f, 0.6f, 0.95f) : new Color(0.65f, 0.65f, 0.65f);

        public static Color DurabilityColor(int durability)
        {
            if (durability > 60) return new Color(0.35f, 0.8f, 0.35f);
            if (durability >= 30) return new Color(0.95f, 0.75f, 0.2f);
            return new Color(0.9f, 0.25f, 0.2f);
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        /// <summary>Retangulo de tamanho fixo ancorado em um ponto.</summary>
        public static RectTransform Fixed(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = Rect(parent, name, anchor, anchor, Vector2.zero, Vector2.zero);
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static Image Box(Transform parent, string name, Color color, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = Fixed(parent, name, anchor, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Image Stretch(Transform parent, string name, Color color)
        {
            var rt = Rect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Text Label(Transform parent, string text, int size, TextAnchor align, Color color,
            Vector2 anchor, Vector2 pos, Vector2 boxSize)
        {
            var rt = Fixed(parent, "Texto", anchor, pos, boxSize);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, Action onClick,
            int fontSize = 22)
        {
            var img = Box(parent, "Botao " + text, new Color(0.2f, 0.2f, 0.28f, 0.95f), anchor, pos, size);
            var button = img.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.6f, 0.6f, 0.9f);
            colors.pressedColor = new Color(0.4f, 0.4f, 0.7f);
            button.colors = colors;
            button.targetGraphic = img;
            button.onClick.AddListener(() => onClick());
            var label = Label(img.transform, text, fontSize, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), Vector2.zero, size);
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.rectTransform.anchoredPosition = Vector2.zero;
            return button;
        }

        /// <summary>Define o preenchimento horizontal (0..1) de uma barra com ancora esticada.</summary>
        public static void SetFill(RectTransform fill, float fraction)
        {
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }

        public static Image FillBar(Transform parent, string name, Color bg, Color fg, Vector2 anchor, Vector2 pos,
            Vector2 size, out RectTransform fill)
        {
            var back = Box(parent, name, bg, anchor, pos, size);
            var fillImg = Stretch(back.transform, "Preenchimento", fg);
            fill = fillImg.rectTransform;
            return back;
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }
    }
}

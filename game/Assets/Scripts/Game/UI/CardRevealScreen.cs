using System;
using System.Collections.Generic;
using System.Linq;
using TremMasmorra.Core.Cards;
using TremMasmorra.Core.Items;
using TremMasmorra.Core.Loadout;
using UnityEngine;
using UnityEngine.UI;

namespace TremMasmorra.Game
{
    /// <summary>As 3 cartas do boss: aparecem juntas, abrem em leque, viram e o jogador escolhe uma.</summary>
    public class CardRevealScreen : MonoBehaviour
    {
        enum Phase { Stacked, Fanning, Flipping, Choosing, Resolving }

        const float FanTime = 0.7f;
        const float FlipTime = 0.5f;
        static readonly Vector2 CardSize = new Vector2(240, 340);

        CardOffer offer;
        PlayerLoadout loadout;
        Action onDone;
        Transform root;
        Phase phase;
        float phaseStart;
        readonly List<RectTransform> cards = new List<RectTransform>();
        readonly List<Text> faces = new List<Text>();
        readonly List<Image> backs = new List<Image>();
        Text message;
        Card pending;

        public bool IsOpen => root != null && root.gameObject.activeSelf;

        public static CardRevealScreen Create(Transform canvas)
        {
            var go = new GameObject("Cartas", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(canvas, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var screen = go.AddComponent<CardRevealScreen>();
            screen.root = Ui.Stretch(go.transform, "Fundo", new Color(0, 0, 0, 0.8f)).transform;
            screen.root.gameObject.SetActive(false);
            return screen;
        }

        public void Show(CardOffer cardOffer, PlayerLoadout l, Action done)
        {
            offer = cardOffer;
            loadout = l;
            onDone = done;
            pending = null;
            PlayerController.InputBlocked = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            root.gameObject.SetActive(true);
            Ui.Clear(root);
            cards.Clear(); faces.Clear(); backs.Clear();

            Ui.Label(root, "O BOSS CAIU!  Escolha uma carta", 36, TextAnchor.UpperCenter, Color.white,
                new Vector2(0.5f, 1f), new Vector2(-400, -60), new Vector2(800, 50));
            message = Ui.Label(root, "", 24, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f),
                new Vector2(0.5f, 0f), new Vector2(-450, 80), new Vector2(900, 40));

            for (int i = 0; i < offer.Cards.Length; i++) BuildCard(i);
            SetPhase(Phase.Stacked);
        }

        void BuildCard(int index)
        {
            var card = offer.Cards[index];
            var back = Ui.Box(root, "Carta" + index, new Color(0.25f, 0.25f, 0.4f), new Vector2(0.5f, 0.5f),
                new Vector2(-CardSize.x / 2 + index * 6, -CardSize.y / 2 - index * 6), CardSize);
            back.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            back.rectTransform.anchoredPosition = new Vector2(index * 6, -index * 6);
            Color border = card.IsAbility ? StyleColor(card.Style) : Ui.RarityColor(card.Item.Rarity);
            var inner = Ui.Stretch(back.transform, "Miolo", new Color(0.12f, 0.12f, 0.16f));
            inner.rectTransform.offsetMin = new Vector2(8, 8);
            inner.rectTransform.offsetMax = new Vector2(-8, -8);
            inner.raycastTarget = false;
            back.color = border;

            var face = Ui.Label(back.transform, "?", 80, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), Vector2.zero, CardSize - new Vector2(30, 30));
            face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            face.rectTransform.anchoredPosition = Vector2.zero;

            int captured = index;
            var button = back.gameObject.AddComponent<Button>();
            button.targetGraphic = back;
            button.onClick.AddListener(() => Choose(captured));

            cards.Add(back.rectTransform);
            faces.Add(face);
            backs.Add(back);
        }

        void SetPhase(Phase p)
        {
            phase = p;
            phaseStart = Time.unscaledTime;
        }

        void Update()
        {
            if (!IsOpen) return;
            float t = Time.unscaledTime - phaseStart;
            switch (phase)
            {
                case Phase.Stacked:
                    if (t > 0.6f) SetPhase(Phase.Fanning);
                    break;
                case Phase.Fanning:
                {
                    float k = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / FanTime));
                    for (int i = 0; i < cards.Count; i++)
                    {
                        float side = i - 1;
                        cards[i].anchoredPosition = new Vector2(Mathf.Lerp(i * 6, side * 280, k), Mathf.Lerp(-i * 6, -Mathf.Abs(side) * 20, k));
                        cards[i].localRotation = Quaternion.Euler(0, 0, -side * 8f * k);
                    }
                    if (t >= FanTime) SetPhase(Phase.Flipping);
                    break;
                }
                case Phase.Flipping:
                {
                    float k = Mathf.Clamp01(t / FlipTime);
                    float scaleX = Mathf.Abs(Mathf.Cos(k * Mathf.PI));
                    for (int i = 0; i < cards.Count; i++)
                    {
                        cards[i].localScale = new Vector3(Mathf.Max(0.02f, scaleX), 1, 1);
                        if (k >= 0.5f) faces[i].text = FaceText(offer.Cards[i]);
                        if (k >= 0.5f) faces[i].fontSize = 22;
                    }
                    if (t >= FlipTime)
                    {
                        foreach (var c in cards) c.localScale = Vector3.one;
                        SetPhase(Phase.Choosing);
                    }
                    break;
                }
            }
        }

        void Choose(int index)
        {
            if (phase != Phase.Choosing) return;
            var card = pending ?? offer.Pick(index);
            pending = card;
            TryApply(card);
        }

        void TryApply(Card card)
        {
            if (loadout.Apply(card)) { Finish(); return; }
            // Nao coube: mochila cheia ou Q e E ocupados. O jogador nao perde a carta.
            SetPhase(Phase.Resolving);
            foreach (var b in backs) b.GetComponent<Button>().interactable = false;
            if (card.IsAbility)
            {
                message.text = "Q e E ocupados. Qual habilidade substituir?";
                Ui.Button(root, "Substituir Q", new Vector2(0.5f, 0f), new Vector2(-220, 150), new Vector2(200, 50),
                    () => { loadout.SetAbility('Q', card.AbilityId); Finish(); });
                Ui.Button(root, "Substituir E", new Vector2(0.5f, 0f), new Vector2(20, 150), new Vector2(200, 50),
                    () => { loadout.SetAbility('E', card.AbilityId); Finish(); });
            }
            else
            {
                message.text = "Mochila cheia. Descarte o item mais gasto para ficar com este?";
                Ui.Button(root, "Descartar o mais gasto", new Vector2(0.5f, 0f), new Vector2(-150, 150), new Vector2(300, 50), () =>
                {
                    var worst = loadout.Backpack.OrderBy(i => i.Durability).First();
                    loadout.Discard(worst);
                    TryApply(card);
                });
            }
        }

        void Finish()
        {
            root.gameObject.SetActive(false);
            PlayerController.InputBlocked = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            onDone?.Invoke();
        }

        static Color StyleColor(CardStyle s) =>
            s == CardStyle.Magia ? new Color(0.65f, 0.4f, 1f) : s == CardStyle.FisicoDistancia ? new Color(0.4f, 0.85f, 0.4f) : new Color(0.95f, 0.5f, 0.3f);

        static string StyleName(CardStyle s) =>
            s == CardStyle.Magia ? "Magia" : s == CardStyle.FisicoDistancia ? "Fisico a distancia" : "Fisico corpo a corpo";

        public static string FaceText(Card c)
        {
            if (c.IsAbility)
            {
                var a = AbilityLibrary.Get(c.AbilityId);
                return "HABILIDADE\n\n" + (a != null ? a.Name : c.AbilityId) + "\n\n" + StyleName(c.Style) + "\n\nQ ou E";
            }
            var i = c.Item;
            return i.Rarity.ToString().ToUpper() + "\n\n" + i.Name.Split('[')[0].Trim() + "\n\n" + StyleName(c.Style) +
                   "\n\nDurab. " + i.Durability + "/100" + (i.AbilityId != null ? "\n+ " + (AbilityLibrary.Get(i.AbilityId)?.Name ?? "") : "");
        }
    }
}

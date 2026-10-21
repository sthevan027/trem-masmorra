using System.Collections.Generic;
using TremMasmorra.Core.Items;
using TremMasmorra.Core.Run;
using UnityEngine;
using UnityEngine.UI;

namespace TremMasmorra.Game
{
    public class Hud : MonoBehaviour
    {
        const int HotbarSlots = 5;

        class Slot
        {
            public Image Frame;
            public Text Label;
            public RectTransform Bar;
            public Image BarBack;
        }

        class AbilityBox
        {
            public Text Name;
            public RectTransform Cooldown;
        }

        PlayerController player;
        RunState run;
        Enemy boss;

        readonly List<Slot> slots = new List<Slot>();
        readonly Dictionary<char, AbilityBox> abilityBoxes = new Dictionary<char, AbilityBox>();
        RectTransform playerFill, bossFill, dodgeFill;
        GameObject bossRoot;
        Text bossName, wagonLabel, toast, playerLabel;
        float toastUntil;

        public static Hud Create(Transform canvas)
        {
            var go = new GameObject("HUD", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(canvas, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var hud = go.AddComponent<Hud>();
            hud.Build();
            return hud;
        }

        public void Bind(PlayerController p, RunState r)
        {
            player = p;
            run = r;
            p.Message += ShowToast;
        }

        public void SetBoss(Enemy e)
        {
            boss = e;
            bossRoot.SetActive(e != null);
            if (e != null) bossName.text = e.Def.Name;
        }

        public void ShowToast(string message)
        {
            toast.text = message;
            toastUntil = Time.unscaledTime + 3f;
        }

        void Build()
        {
            var topLeft = new Vector2(0, 1);
            wagonLabel = Ui.Label(transform, "", 26, TextAnchor.UpperLeft, Color.white, topLeft, new Vector2(24, -16), new Vector2(500, 36));
            Ui.FillBar(transform, "VidaJogador", new Color(0.15f, 0.05f, 0.05f, 0.9f), new Color(0.85f, 0.2f, 0.2f),
                topLeft, new Vector2(24, -58), new Vector2(340, 22), out playerFill);
            playerLabel = Ui.Label(transform, "Vida", 16, TextAnchor.MiddleLeft, Color.white, topLeft, new Vector2(30, -58), new Vector2(200, 22));

            // Vida do boss: barra larga no topo, estilo Elden Ring.
            var bossGo = Ui.Rect(transform, "Boss", new Vector2(0.15f, 1f), new Vector2(0.85f, 1f), new Vector2(0, -90), new Vector2(0, -20));
            bossRoot = bossGo.gameObject;
            bossName = Ui.Label(bossRoot.transform, "", 24, TextAnchor.LowerLeft, Color.white, new Vector2(0, 1), new Vector2(0, 2), new Vector2(600, 32));
            var bossBack = Ui.Stretch(bossRoot.transform, "FundoBoss", new Color(0.1f, 0.05f, 0.05f, 0.9f));
            bossBack.rectTransform.anchorMin = new Vector2(0, 0);
            bossBack.rectTransform.anchorMax = new Vector2(1, 0.5f);
            bossBack.rectTransform.offsetMin = Vector2.zero;
            bossBack.rectTransform.offsetMax = Vector2.zero;
            bossFill = Ui.Stretch(bossBack.transform, "VidaBoss", new Color(0.7f, 0.12f, 0.12f)).rectTransform;
            bossRoot.SetActive(false);

            // Hotbar estilo Minecraft.
            for (int i = 0; i < HotbarSlots; i++)
            {
                float x = (i - (HotbarSlots - 1) / 2f) * 72f;
                var anchor = new Vector2(0.5f, 0f);
                var frame = Ui.Box(transform, "Slot" + i, new Color(0.15f, 0.15f, 0.18f, 0.9f), anchor, new Vector2(x, 30), new Vector2(56, 56));
                var label = Ui.Label(frame.transform, "", 13, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(54, 54));
                label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                label.rectTransform.anchoredPosition = Vector2.zero;
                var back = Ui.FillBar(transform, "Dur" + i, new Color(0, 0, 0, 0.8f), Color.green, anchor, new Vector2(x, 22), new Vector2(56, 6), out var fill);
                slots.Add(new Slot { Frame = frame, Label = label, Bar = fill, BarBack = back });
            }

            // Habilidades no canto inferior direito.
            string[] keys = { "Q", "E", "Shift" };
            for (int i = 0; i < keys.Length; i++)
            {
                var anchor = new Vector2(1, 0);
                var box = Ui.Box(transform, "Hab" + keys[i], new Color(0.2f, 0.15f, 0.3f, 0.9f), anchor, new Vector2(-(3 - i) * 84 - 10, 24), new Vector2(76, 76));
                Ui.Label(box.transform, keys[i], 16, TextAnchor.UpperLeft, Color.white, new Vector2(0, 1), new Vector2(4, -2), new Vector2(60, 20));
                var name = Ui.Label(box.transform, "", 13, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(72, 50));
                name.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                name.rectTransform.anchoredPosition = Vector2.zero;
                var cd = Ui.Rect(box.transform, "Recarga", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var cdImg = cd.gameObject.AddComponent<Image>();
                cdImg.color = new Color(0, 0, 0, 0.6f);
                cdImg.raycastTarget = false;
                if (keys[i] == "Shift") { name.text = "Desviar"; dodgeFill = cd; }
                else abilityBoxes[keys[i][0]] = new AbilityBox { Name = name, Cooldown = cd };
            }

            toast = Ui.Label(transform, "", 24, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.4f), new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(600, 40));
        }

        void Update()
        {
            if (player == null || player.Loadout == null) return;
            wagonLabel.text = "Vagão " + run.Wagon + " / " + RunState.TotalWagons + (run.IsBossWagon ? "   (BOSS)" : "");
            Ui.SetFill(playerFill, player.Health.Fraction);
            playerLabel.text = player.Health.Current + " / " + player.Health.Max;

            if (boss != null)
            {
                if (boss.IsDead) SetBoss(null);
                else Ui.SetFill(bossFill, boss.Health.Fraction);
            }

            var inUse = player.Loadout.InUse;
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (i < inUse.Count)
                {
                    var item = inUse[i];
                    slot.Frame.color = Color.Lerp(new Color(0.15f, 0.15f, 0.18f), Ui.RarityColor(item.Rarity), 0.55f);
                    slot.Label.text = item.Name.Split('[')[0].Trim();
                    slot.BarBack.gameObject.SetActive(true);
                    slot.Bar.GetComponent<Image>().color = Ui.DurabilityColor(item.Durability);
                    Ui.SetFill(slot.Bar, item.Durability / (float)Item.MaxDurability);
                }
                else
                {
                    slot.Frame.color = new Color(0.15f, 0.15f, 0.18f, 0.6f);
                    slot.Label.text = "";
                    slot.BarBack.gameObject.SetActive(false);
                }
            }

            UpdateAbility('Q', player.Loadout.AbilityQ);
            UpdateAbility('E', player.Loadout.AbilityE);
            SetCooldownOverlay(dodgeFill, 1f - player.DodgeFraction);

            toast.gameObject.SetActive(Time.unscaledTime < toastUntil);
        }

        void UpdateAbility(char key, string id)
        {
            var box = abilityBoxes[key];
            var ability = AbilityLibrary.Get(id);
            box.Name.text = ability != null ? ability.Name : "";
            SetCooldownOverlay(box.Cooldown, ability != null ? player.CooldownFraction(key) : 0f);
        }

        static void SetCooldownOverlay(RectTransform overlay, float fraction)
        {
            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = new Vector2(1, Mathf.Clamp01(fraction));
            overlay.offsetMin = overlay.offsetMax = Vector2.zero;
        }
    }
}

using System.Linq;
using TremMasmorra.Core.Items;
using TremMasmorra.Core.Loadout;
using UnityEngine;
using UnityEngine.UI;

namespace TremMasmorra.Game
{
    /// <summary>Inventario: armadura, mochila, itens em uso e habilidades. Abre com Tab.</summary>
    public class InventoryScreen : MonoBehaviour
    {
        public static KeyCode ToggleKey = KeyCode.Tab;

        PlayerLoadout loadout;
        Transform content;
        Item selected;
        Text detail;

        public bool IsOpen => content != null && content.gameObject.activeSelf;

        public static InventoryScreen Create(Transform canvas)
        {
            var go = new GameObject("Inventario", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(canvas, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var screen = go.AddComponent<InventoryScreen>();
            screen.content = Ui.Stretch(go.transform, "Fundo", new Color(0.05f, 0.05f, 0.1f, 0.92f)).transform;
            screen.content.gameObject.SetActive(false);
            return screen;
        }

        public void Bind(PlayerLoadout l) { loadout = l; selected = null; }

        public void Close()
        {
            if (content == null || !content.gameObject.activeSelf) return;
            content.gameObject.SetActive(false);
            PlayerController.InputBlocked = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Open()
        {
            if (loadout == null) return;
            content.gameObject.SetActive(true);
            PlayerController.InputBlocked = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Redraw();
        }

        void Update()
        {
            if (loadout == null || !Input.GetKeyDown(ToggleKey)) return;
            if (IsOpen) Close();
            else if (!PlayerController.InputBlocked) Open();
        }

        void Redraw()
        {
            Ui.Clear(content);
            var tl = new Vector2(0, 1);
            Ui.Label(content, "INVENTÁRIO   (Tab para fechar)", 34, TextAnchor.UpperLeft, Color.white, tl, new Vector2(60, -30), new Vector2(900, 50));

            // Armadura + personagem.
            Ui.Label(content, "Armadura", 24, TextAnchor.UpperLeft, Color.white, tl, new Vector2(60, -100), new Vector2(300, 30));
            string[] names = { "Cabeça", "Peitoral", "Braços", "Pernas" };
            for (int i = 0; i < 4; i++)
                ItemButton(loadout.Armor[i], names[i], tl, new Vector2(60, -140 - i * 100));
            Ui.Box(content, "Personagem", new Color(0.3f, 0.5f, 0.8f, 0.8f), tl, new Vector2(190, -140), new Vector2(150, 370));
            Ui.Label(content, "Personagem", 20, TextAnchor.MiddleCenter, Color.white, tl, new Vector2(190, -300), new Vector2(150, 40));

            // Mochila.
            Ui.Label(content, "Mochila (" + loadout.Backpack.Count + "/" + PlayerLoadout.BackpackCapacity + ")", 24,
                TextAnchor.UpperLeft, Color.white, tl, new Vector2(420, -100), new Vector2(400, 30));
            for (int i = 0; i < PlayerLoadout.BackpackCapacity; i++)
            {
                var item = i < loadout.Backpack.Count ? loadout.Backpack[i] : null;
                ItemButton(item, "", tl, new Vector2(420 + (i % 5) * 100, -140 - (i / 5) * 100));
            }

            // Em uso.
            Ui.Label(content, "Em uso (máximo " + loadout.MaxInUse + ")", 24, TextAnchor.UpperLeft, Color.white, tl, new Vector2(420, -560), new Vector2(400, 30));
            for (int i = 0; i < loadout.MaxInUse; i++)
                ItemButton(i < loadout.InUse.Count ? loadout.InUse[i] : null, "", tl, new Vector2(420 + i * 100, -600));
            Ui.Label(content, "Braço extra", 20, TextAnchor.UpperLeft, Color.white, tl, new Vector2(760, -560), new Vector2(200, 30));
            ItemButton(loadout.ExtraArm, "", tl, new Vector2(760, -600));

            // Habilidades.
            Ui.Label(content, "Q: " + AbilityName(loadout.AbilityQ) + "     E: " + AbilityName(loadout.AbilityE), 22,
                TextAnchor.UpperLeft, new Color(0.8f, 0.7f, 1f), tl, new Vector2(420, -720), new Vector2(700, 30));

            // Detalhe e acoes.
            var tr = new Vector2(1, 1);
            detail = Ui.Label(content, selected == null ? "Selecione um item." : Describe(selected), 22, TextAnchor.UpperLeft,
                Color.white, tr, new Vector2(-480, -140), new Vector2(430, 200));
            if (selected != null) BuildActions(tr);
        }

        void BuildActions(Vector2 anchor)
        {
            float y = -360;
            bool inBackpack = loadout.Backpack.Contains(selected);
            if (inBackpack)
            {
                string label = IsHand(selected.Kind) ? "Usar na mão" : selected.Kind == ItemKind.ExtraArm ? "Equipar braço" : IsArmor(selected.Kind) ? "Vestir" : null;
                if (label != null) Action(label, anchor, ref y, Equip);
                if (selected.AbilityId != null)
                {
                    Action("Habilidade -> Q", anchor, ref y, () => loadout.SetAbility('Q', selected.AbilityId));
                    Action("Habilidade -> E", anchor, ref y, () => loadout.SetAbility('E', selected.AbilityId));
                }
                Action("Descartar", anchor, ref y, () => { loadout.Discard(selected); selected = null; });
            }
            else
            {
                Action("Guardar na mochila", anchor, ref y, () => { loadout.Stow(selected); selected = null; });
            }
        }

        void Action(string label, Vector2 anchor, ref float y, System.Action act)
        {
            Ui.Button(content, label, anchor, new Vector2(-480, y), new Vector2(300, 48), () => { act(); Redraw(); });
            y -= 58;
        }

        void Equip()
        {
            if (IsHand(selected.Kind))
            {
                if (!loadout.TryEquipInUse(selected) && loadout.InUse.Count > 0)
                    loadout.Swap(loadout.InUse[0], selected);
            }
            else if (selected.Kind == ItemKind.ExtraArm) loadout.TryEquipExtraArm(selected);
            else loadout.EquipArmor(selected);
            selected = null;
        }

        void ItemButton(Item item, string emptyLabel, Vector2 anchor, Vector2 pos)
        {
            var color = item == null ? new Color(0.18f, 0.18f, 0.22f, 0.9f) : Color.Lerp(new Color(0.15f, 0.15f, 0.18f), Ui.RarityColor(item.Rarity), 0.55f);
            var captured = item;
            var btn = Ui.Button(content, item == null ? emptyLabel : item.Name.Split('[')[0].Trim(), anchor, pos, new Vector2(90, 90),
                () => { if (captured != null) { selected = captured; Redraw(); } }, 15);
            btn.GetComponent<Image>().color = color;
            if (item != null)
            {
                var bar = Ui.FillBar(content, "Durabilidade", new Color(0, 0, 0, 0.8f), Ui.DurabilityColor(item.Durability),
                    anchor, new Vector2(pos.x, pos.y - 84), new Vector2(90, 7), out var fill);
                Ui.SetFill(fill, item.Durability / (float)Item.MaxDurability);
            }
        }

        static string Describe(Item i) =>
            i.Name + "\nRaridade: " + i.Rarity + "\nTipo: " + i.Kind + "\nDurabilidade: " + i.Durability + "/100" +
            (i.AbilityId != null ? "\nHabilidade: " + (AbilityLibrary.Get(i.AbilityId)?.Name ?? i.AbilityId) : "");

        static string AbilityName(string id) => AbilityLibrary.Get(id)?.Name ?? "-";

        static bool IsHand(ItemKind k) =>
            k == ItemKind.Melee || k == ItemKind.Ranged || k == ItemKind.Magic || k == ItemKind.Shield || k == ItemKind.Orb;

        static bool IsArmor(ItemKind k) =>
            k == ItemKind.ArmorHead || k == ItemKind.ArmorChest || k == ItemKind.ArmorArms || k == ItemKind.ArmorLegs;
    }
}

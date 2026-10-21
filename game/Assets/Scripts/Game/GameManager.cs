using System;
using System.Collections.Generic;
using System.Linq;
using TremMasmorra.Core.Cards;
using TremMasmorra.Core.Character;
using TremMasmorra.Core.Drops;
using TremMasmorra.Core.Items;
using TremMasmorra.Core.Loadout;
using TremMasmorra.Core.Run;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TremMasmorra.Game
{
    public enum GameState { Title, Creator, Playing, Dead, Victory }

    /// <summary>Orquestra o fluxo: tela inicial, criacao, trem de 9 vagoes, morte e vitoria.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }
        public RunState Run { get; private set; }
        public PlayerLoadout Loadout { get; private set; }
        public PlayerController Player { get; private set; }
        public CharacterLook Look { get; } = new CharacterLook();

        Camera cam;
        ThirdPersonCamera follow;
        Menus menus;
        Hud hud;
        InventoryScreen inventory;
        CardRevealScreen cards;
        System.Random rng;
        DropRoller roller;
        WagonData wagon;
        readonly List<Enemy> alive = new List<Enemy>();
        GameObject preview;
        Enemy currentBoss;

        public Hud Hud => hud;
        public IReadOnlyList<Enemy> Alive => alive;

        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            BuildScene();
            ShowTitle();
        }

        // ---------- Montagem da cena ----------

        void BuildScene()
        {
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.45f);
            var camGo = new GameObject("Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            cam.fieldOfView = 65f;
            follow = camGo.AddComponent<ThirdPersonCamera>();
            follow.enabled = false;

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            hud = Hud.Create(canvas.transform);
            hud.gameObject.SetActive(false);
            inventory = InventoryScreen.Create(canvas.transform);
            cards = CardRevealScreen.Create(canvas.transform);
            menus = Menus.Create(canvas.transform);
        }

        // ---------- Fluxo ----------

        void ShowTitle()
        {
            State = GameState.Title;
            ClearWorld();
            hud.gameObject.SetActive(false);
            inventory.Close();
            Time.timeScale = 1f;
            PlayerController.InputBlocked = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            follow.enabled = false;
            cam.transform.SetPositionAndRotation(new Vector3(0, 2, -6), Quaternion.identity);
            menus.ShowTitle(ShowCreator, Quit);
        }

        void ShowCreator()
        {
            State = GameState.Creator;
            RebuildPreview();
            menus.ShowCreator(Look, RebuildPreview, StartRun);
        }

        void RebuildPreview()
        {
            if (preview != null) Destroy(preview);
            preview = new GameObject("Preview");
            preview.transform.position = new Vector3(0, 0, 0);
            var anim = ProceduralModels.Humanoid(Look, new Color(0.25f, 0.45f, 0.8f), new Color(0.25f, 0.2f, 0.3f));
            anim.transform.SetParent(preview.transform, false);
            preview.transform.rotation = Quaternion.Euler(0, 160f, 0);
            cam.transform.SetPositionAndRotation(new Vector3(2.4f, 1.6f, -4f), Quaternion.identity);
            cam.transform.LookAt(new Vector3(0, 1.1f, 0));
        }

        void Update()
        {
            if (State == GameState.Creator && preview != null)
                preview.transform.Rotate(0, 25f * Time.unscaledDeltaTime, 0);

            if (State != GameState.Playing || Player == null) return;

            if (Player.Health.IsDead) { OnPlayerDied(); return; }
            if (Run.DoorOpen && Player.Position.z >= WagonBuilder.ExitZ && Mathf.Abs(Player.Position.x) < 2.2f)
                AdvanceWagon();
        }

        public void StartRun()
        {
            ClearWorld();
            menus.Hide();
            State = GameState.Playing;
            rng = new System.Random();
            roller = new DropRoller(rng);
            Loadout = new PlayerLoadout();
            // O jogador comeca com uma espada gasta para poder lutar no primeiro vagao.
            var starter = new Item("Espada [Comum]", Rarity.Comum, ItemKind.Melee, 40);
            Loadout.TryAddToBackpack(starter);
            Loadout.TryEquipInUse(starter);
            Run = new RunState();

            var playerGo = new GameObject("Jogador", typeof(CharacterController), typeof(Health));
            var cc = playerGo.GetComponent<CharacterController>();
            cc.height = 1.8f * Look.Height;
            cc.radius = 0.4f;
            cc.center = new Vector3(0, cc.height / 2f, 0);
            Player = playerGo.AddComponent<PlayerController>();
            var anim = ProceduralModels.Humanoid(Look, new Color(0.25f, 0.45f, 0.8f), new Color(0.25f, 0.2f, 0.3f));
            Player.Setup(Loadout, anim);
            Player.CameraTransform = cam.transform;
            Player.ItemBroken += _ => { };

            follow.Target = playerGo.transform;
            follow.Yaw = 0f;
            follow.enabled = true;
            hud.gameObject.SetActive(true);
            hud.Bind(Player, Run);
            inventory.Bind(Loadout);
            PlayerController.InputBlocked = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (preview != null) Destroy(preview);

            LoadWagon();
        }

        void LoadWagon()
        {
            foreach (var e in alive.ToList()) if (e != null) Destroy(e.gameObject);
            alive.Clear();
            if (wagon != null) Destroy(wagon.Root);
            hud.SetBoss(null);
            currentBoss = null;

            bool boss = Run.IsBossWagon;
            wagon = WagonBuilder.Build(Run.Wagon, boss, rng);
            Player.Teleport(wagon.PlayerStart, Quaternion.identity);
            follow.Yaw = 0f;

            var defs = new List<EnemyDef>();
            if (boss)
            {
                defs.Add(EnemyCatalog.Bosses[(Run.Wagon / RunState.BossEvery - 1) % EnemyCatalog.Bosses.Length]);
                defs.Add(EnemyCatalog.FormigaCristal);
                defs.Add(EnemyCatalog.MonstroDeLama);
            }
            else
            {
                int count = 3 + Run.Wagon / 3;
                for (int i = 0; i < count; i++) defs.Add(EnemyCatalog.Common[rng.Next(EnemyCatalog.Common.Length)]);
            }

            Run.StartWagon(defs.Count);
            for (int i = 0; i < defs.Count; i++)
            {
                var enemy = Enemy.Spawn(defs[i], wagon.SpawnPoints[i % wagon.SpawnPoints.Count], Player);
                enemy.Killed += OnEnemyKilled;
                alive.Add(enemy);
                if (defs[i].IsBoss) { currentBoss = enemy; hud.SetBoss(enemy); }
            }
            hud.ShowToast(boss ? "Boss do vagao " + Run.Wagon + "!" : "Vagao " + Run.Wagon + ": derrote os monstros");
        }

        void OnEnemyKilled(Enemy enemy)
        {
            alive.Remove(enemy);
            Run.OnMonsterKilled();

            if (enemy.Def.IsBoss)
            {
                var offer = new CardOffer(roller, rng, Run.Wagon);
                if (Run.DoorOpen) WagonBuilder.OpenDoor(wagon);
                cards.Show(offer, Loadout, () => hud.ShowToast("Porta aberta: siga em frente"));
                return;
            }

            if (rng.NextDouble() < 0.5)
            {
                var drop = roller.RollCommon(Run.Wagon);
                GiveItem(drop);
            }
            if (Run.DoorOpen)
            {
                WagonBuilder.OpenDoor(wagon);
                hud.ShowToast("Porta aberta: siga em frente");
            }
        }

        void GiveItem(Item item)
        {
            if (!Loadout.TryAddToBackpack(item)) { hud.ShowToast("Mochila cheia: " + item.Name + " perdido"); return; }
            string msg = "Drop: " + item.Name + " (" + item.Durability + "%)";
            // Item de mao entra em uso se houver espaco; armadura veste se o slot estiver livre.
            if (PlayerController.IsWeapon(item) && Loadout.TryEquipInUse(item)) msg += " - em uso";
            else if (IsArmor(item.Kind) && SlotFree(item.Kind) && Loadout.EquipArmor(item)) msg += " - vestido";
            hud.ShowToast(msg);
        }

        bool SlotFree(ItemKind kind)
        {
            switch (kind)
            {
                case ItemKind.ArmorHead: return Loadout.Armor[(int)ArmorSlot.Head] == null;
                case ItemKind.ArmorChest: return Loadout.Armor[(int)ArmorSlot.Chest] == null;
                case ItemKind.ArmorArms: return Loadout.Armor[(int)ArmorSlot.Arms] == null;
                default: return Loadout.Armor[(int)ArmorSlot.Legs] == null;
            }
        }

        static bool IsArmor(ItemKind k) =>
            k == ItemKind.ArmorHead || k == ItemKind.ArmorChest || k == ItemKind.ArmorArms || k == ItemKind.ArmorLegs;

        void AdvanceWagon()
        {
            Run.NextWagon();
            if (Run.Victory) { OnVictory(); return; }
            LoadWagon();
        }

        void OnPlayerDied()
        {
            State = GameState.Dead;
            Run.OnPlayerDied();
            PlayerController.InputBlocked = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            menus.ShowEnd("VOCE MORREU", "Vagao " + Run.Wagon + " de " + RunState.TotalWagons, "Recomecar do vagao 1", StartRun);
        }

        void OnVictory()
        {
            State = GameState.Victory;
            PlayerController.InputBlocked = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            menus.ShowEnd("VITORIA!", "Voce atravessou os 9 vagoes do Trem Masmorra.", "Voltar ao inicio", ShowTitle);
        }

        /// <summary>Remove tudo de uma run anterior para que nada vaze para a proxima.</summary>
        void ClearWorld()
        {
            foreach (var e in alive.ToList()) if (e != null) Destroy(e.gameObject);
            alive.Clear();
            if (wagon != null) Destroy(wagon.Root);
            wagon = null;
            if (Player != null) Destroy(Player.gameObject);
            Player = null;
            if (preview != null) Destroy(preview);
            foreach (var p in FindObjectsByType<Projectile>(FindObjectsSortMode.None)) Destroy(p.gameObject);
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

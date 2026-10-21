# Trem Masmorra Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Uma primeira versão jogável do Trem Masmorra: tela inicial, criação de personagem, 9 vagões com 3 bosses, ciclo de drop/cartas/equipamento.

**Architecture:** A lógica de jogo (itens, durabilidade, drops, cartas, equipamento, estado da run) fica em C# puro, numa assembly sem dependência do Unity (`TremMasmorra.Core`), testada com testes EditMode. A camada Unity (`TremMasmorra.Game`) tem controlador do jogador, combate, modelos procedurais e UI, e só consome o Core. Os modelos 3D são montados por código com primitivas (low poly, cores chapadas).

**Tech Stack:** Unity 6000.0.85f1 (instalado em `C:\Program Files\Unity\Hub\Editor\6000.0.85f1`), C#, Unity Test Framework (NUnit), Input System, uGUI.

**Spec:** `docs/superpowers/specs/2026-10-21-trem-masmorra-design.md`

## Global Constraints

- Engine Unity, linguagem C#, single player. Nada de multiplayer, servidor, salvamento online, "virar boss" ou loop infinito.
- Projeto em `D:\Projetos\trem-masmorra`. Nada no `C:`.
- 9 vagões. A cada 3 vagões, 2 comuns e o terceiro com boss. Bosses nos vagões 3, 6 e 9, 3 bosses diferentes.
- Durabilidade de 0 a 100. Ao chegar a 0, o item **quebra** (não fica com dano 0).
- Itens de vagão comum: durabilidade **abaixo de 45**. Raro: quase novo (perto de 100). Épico: melhor que o raro. Épicos só dropam a partir do **vagão 5**.
- Chance de partida: raro novo ~40% no primeiro boss, épico ~10%. Tudo ajustável.
- Boss dropa 3 cartas; o jogador escolhe 1. Carta = habilidade (Q/E) ou item. Estilos: magia, físico à distância, físico corpo a corpo.
- Máximo de **2 itens em uso** na hotbar, mais 1 se houver equipamento de braço extra.
- Armadura: cabeça, peitoral, braços, pernas, com raridade e durabilidade, e pode dropar de boss em carta.
- Criação de personagem só altera o visual (homem/mulher, tamanho, aparência), nenhum bônus.
- Controles: WASD move, mouse ataca, Q/E poderes, Espaço pula, Shift desvia. Câmera 3D por trás.
- HUD: hotbar embaixo (barra de durabilidade colorida sem número), vida do boss em barra larga no topo, vida do jogador no canto superior, habilidades no canto inferior direito. Inventário só abre por botão.
- Ao morrer volta ao vagão 1. Depois do vagão 9, tela de vitória e volta à tela inicial. A porta abre quando todos os monstros morrem; não dá para voltar.

## Review Focus

- Dois itens da mesma arma equipados (duas espadas): ambos funcionam e cada um gasta a própria durabilidade.
- Item quebra durante um golpe enquanto o jogador está equipado: é removido da hotbar sem erro e o ataque seguinte não usa um item inexistente.
- Carta de item escolhida com a mochila cheia: o jogador não perde o item nem fica preso sem poder escolher.
- Aperto de Q/E sem habilidade atribuída: nada acontece, sem erro.
- Morrer e reiniciar a run: estado limpo (vagão 1, inimigos reiniciados), sem itens ou cartas de uma run anterior vazando.

---

### Task 1: Projeto Unity, assemblies e comando de teste

**Files:**
- Create: projeto Unity em `D:\Projetos\trem-masmorra` (Assets, Packages, ProjectSettings)
- Create: `Assets/Scripts/Core/TremMasmorra.Core.asmdef`, `Assets/Scripts/Game/TremMasmorra.Game.asmdef`, `Assets/Tests/EditMode/TremMasmorra.Tests.asmdef`
- Create: `.gitignore` (Unity) incluindo `.superpowers/`, `scripts/run-tests.sh`

**Interfaces:**
- Produces: `scripts/run-tests.sh` roda `Unity.exe -batchmode -runTests -testPlatform EditMode` e sai com código 0 se passar.

- [ ] **Step 1:** `git init` e criar o projeto com `Unity.exe -batchmode -quit -createProject D:\Projetos\trem-masmorra\game`. Se `game/` for o projeto Unity, os caminhos acima ficam sob `game/`. Confirmar a estrutura e usá-la nos tasks seguintes.
- [ ] **Step 2:** Criar as 3 asmdefs (Game referencia Core; Tests referencia Core e usa NUnit).
- [ ] **Step 3:** Escrever um teste `Smoke.Passes` (`Assert.Pass()`) e `scripts/run-tests.sh`.
- [ ] **Step 4:** Rodar `bash scripts/run-tests.sh`. Esperado: 1 teste passando.
- [ ] **Step 5:** Commit.

### Task 2: Item e durabilidade

**Files:** Create `Core/Items/Item.cs`, `Core/Items/Rarity.cs`, `Core/Items/ItemKind.cs`; Test `Tests/EditMode/ItemTests.cs`

**Interfaces:**
- Produces: `enum Rarity { Comum, Raro, Epico }`; `enum ItemKind { Melee, Ranged, Magic, Shield, Orb, ArmorHead, ArmorChest, ArmorArms, ArmorLegs, ExtraArm, Utility }`; `class Item { string Name; Rarity Rarity; ItemKind Kind; int Durability (0-100); bool IsBroken; void Wear(int amount); string AbilityId (nullable) }`

- [ ] **Step 1:** Testes `Wear_ReducesDurability`, `Wear_ToZero_MarksBroken`, `Wear_NeverBelowZero`, `NewItem_DurabilityClampedTo100`.
- [ ] **Step 2:** Rodar e ver falhar.
- [ ] **Step 3:** Implementar `Item` com o contrato acima.
- [ ] **Step 4:** Rodar e ver passar. Commit.

### Task 3: Tabela de drops

**Files:** Create `Core/Drops/DropRoller.cs`; Test `Tests/EditMode/DropRollerTests.cs`

**Interfaces:**
- Consumes: `Item`, `Rarity`.
- Produces: `class DropRoller { DropRoller(System.Random rng); Item RollCommon(int wagon); Item[] RollBossItems(int wagon, int count) }`; durabilidade comum sempre `< 45`; épico só se `wagon >= 5`.

- [ ] **Step 1:** Testes `Common_AlwaysBelow45`, `Epic_NeverBeforeWagon5` (1000 rolagens no vagão 3), `Epic_AppearsAtWagon6` (1000 rolagens, ao menos um épico), `BossRare_NearNewDurability` (raro com durabilidade >= 90).
- [ ] **Step 2:** Rodar e ver falhar.
- [ ] **Step 3:** Implementar com RNG injetado; probabilidades em constantes nomeadas (`RareNewChance = 0.40`, `EpicChance = 0.10`).
- [ ] **Step 4:** Rodar e ver passar. Commit.

### Task 4: Cartas e oferta do boss

**Files:** Create `Core/Cards/Card.cs`, `Core/Cards/CardOffer.cs`; Test `Tests/EditMode/CardOfferTests.cs`

**Interfaces:**
- Consumes: `Item`, `DropRoller`.
- Produces: `enum CardStyle { Magia, FisicoDistancia, FisicoCorpo }`; `class Card { CardStyle Style; Item Item (null se for habilidade); string AbilityId (null se for item); bool IsAbility }`; `class CardOffer { Card[] Cards (3); Card Pick(int index) }` (só um `Pick`; segunda chamada lança `InvalidOperationException`).

- [ ] **Step 1:** Testes `Offer_HasThreeCards`, `Pick_ReturnsChosen`, `Pick_Twice_Throws`, `Cards_CoverMoreThanOneStyle` (em 100 ofertas há pelo menos 2 estilos).
- [ ] **Step 2:** Rodar e ver falhar. **Step 3:** Implementar. **Step 4:** Rodar e ver passar. Commit.

### Task 5: Equipamento e inventário

**Files:** Create `Core/Loadout/Loadout.cs`; Test `Tests/EditMode/LoadoutTests.cs`

**Interfaces:**
- Consumes: `Item`, `Card`.
- Produces: `class Loadout { IReadOnlyList<Item> Backpack; Item[] Armor (4); Item[] InUse; string AbilityQ, AbilityE; int MaxInUse (2, ou 3 com ExtraArm); bool TryAddToBackpack(Item); bool TryEquipInUse(Item); void Unequip(Item); void OnItemBroken(Item); void Apply(Card) }`; a mochila tem 20 slots.

- [ ] **Step 1:** Testes `InUse_MaxTwo`, `ExtraArm_AllowsThird`, `TwoSwords_BothEquipped`, `BrokenItem_RemovedFromInUse`, `Apply_AbilityCard_FillsFirstEmptyOfQE`, `Apply_ItemCard_BackpackFull_DoesNotLoseItem` (devolve ao jogador a decisão de descartar algo: retorna `false` e mantém a oferta pendente), `SwapRustySwordForEpic`.
- [ ] **Step 2:** Rodar e ver falhar. **Step 3:** Implementar. **Step 4:** Rodar e ver passar. Commit.

### Task 6: Estado da run

**Files:** Create `Core/Run/RunState.cs`; Test `Tests/EditMode/RunStateTests.cs`

**Interfaces:**
- Produces: `class RunState { int Wagon (1-9); bool IsBossWagon; int MonstersAlive; bool DoorOpen; bool Victory; bool Dead; void StartWagon(int monsters); void OnMonsterKilled(); void OnPlayerDied(); void NextWagon(); void Restart() }`; `IsBossWagon` verdadeiro em 3, 6 e 9; a porta abre quando `MonstersAlive == 0`; `NextWagon` sem porta aberta lança exceção; depois do 9 marca `Victory`.

- [ ] **Step 1:** Testes `BossWagons_Are3_6_9`, `Door_OpensWhenAllDead`, `NextWagon_ClosedDoor_Throws`, `Victory_AfterWagon9`, `PlayerDied_RestartGoesToWagon1`, `Restart_ClearsPreviousRunState`.
- [ ] **Step 2:** Rodar e ver falhar. **Step 3:** Implementar. **Step 4:** Rodar e ver passar. Commit.

### Task 7: Jogador (movimento, câmera, pulo, desvio)

**Files:** Create `Game/Player/PlayerController.cs`, `Game/Player/ThirdPersonCamera.cs`, `Game/Input/PlayerInputActions` (Input System asset); cena `Assets/Scenes/Train.unity`

**Interfaces:**
- Produces: `PlayerController` com `Vector3 Velocity`, `bool IsDodging`, `event Action BasicAttack, UseQ, UseE`.

- [ ] **Step 1:** Teste PlayMode `Dodge_MakesPlayerInvulnerableForShortWindow` e `Jump_LeavesGround`.
- [ ] **Step 2:** Rodar e ver falhar. **Step 3:** Implementar WASD relativo à câmera, Espaço pula, Shift desvia com janela curta de invulnerabilidade, câmera por trás. **Step 4:** Rodar e ver passar. Commit.

### Task 8: Combate, vida e desgaste de durabilidade

**Files:** Create `Game/Combat/Health.cs`, `Game/Combat/Weapon.cs`, `Game/Combat/EnemyBase.cs`; Test `Tests/PlayMode/CombatTests.cs`

**Interfaces:**
- Consumes: `Loadout`, `Item.Wear`, `RunState`.
- Produces: `Health { int Current, Max; event Action Died; void Damage(int) }`. Golpe dado com arma e golpe bloqueado com escudo gastam durabilidade; golpe recebido gasta a armadura.

- [ ] **Step 1:** Testes `Attack_WearsWeapon`, `Block_WearsShield`, `Hit_WearsArmor`, `WeaponBreaks_AtZero_NextAttackSafe`, `QEWithoutAbility_DoesNothing`.
- [ ] **Step 2:** Rodar e ver falhar. **Step 3:** Implementar. **Step 4:** Rodar e ver passar. Commit.

### Task 9: Modelos procedurais, vagões e monstros comuns

**Files:** Create `Game/Art/ProceduralModels.cs`, `Game/World/WagonBuilder.cs`, `Game/Enemies/CommonMonsters.cs`

**Interfaces:**
- Produces: `GameObject ProceduralModels.Humanoid(CharacterLook look)`; `GameObject WagonBuilder.Build(int wagon)`; 2 tipos de monstro comum, com animação básica (andar, atacar, tomar dano, morrer).

- [ ] **Step 1:** Teste PlayMode `Wagon_SpawnsMonsters_AndDoorStartsClosed`.
- [ ] **Step 2:** Rodar e ver falhar. **Step 3:** Implementar. **Step 4:** Rodar e ver passar. Commit.

### Task 10: Bosses e habilidades

**Files:** Create `Game/Enemies/Bosses/Boss1.cs`, `Boss2.cs`, `Boss3.cs`; `Game/Abilities/AbilityLibrary.cs`

**Interfaces:**
- Produces: `AbilityLibrary.Get(string id) -> Ability`; no mínimo 6 habilidades cobrindo os 3 estilos; item `Capa da Invisibilidade` com efeito que faz inimigos perderem o alvo por alguns segundos; ao morrer, cada boss chama `CardOffer`.

- [ ] **Step 1:** Testes `Boss_Death_OffersThreeCards`, `Ability_ById_Exists_ForEveryCardAbility`, `InvisibilityCape_EnemiesLoseTarget`.
- [ ] **Step 2:** Rodar e ver falhar. **Step 3:** Implementar. **Step 4:** Rodar e ver passar. Commit.

### Task 11: HUD

**Files:** Create `Game/UI/Hud.cs`, `Game/UI/HotbarSlotView.cs`, `Game/UI/BossBar.cs`

**Interfaces:**
- Consumes: `Loadout`, `Health`, `Item.Durability`.
- Produces: hotbar embaixo com barra de durabilidade colorida (verde, amarelo, vermelho) sem número; vida do boss no topo em largura total; vida do jogador no canto superior; Q/E/Shift no canto inferior direito com ícone da habilidade.

- [ ] **Step 1:** Teste PlayMode `DurabilityBar_ColorChangesWithValue` (verde acima de 60, amarelo entre 30 e 60, vermelho abaixo de 30). **Step 2:** Falhar. **Step 3:** Implementar. **Step 4:** Passar. Commit.

### Task 12: Inventário

**Files:** Create `Game/UI/InventoryScreen.cs`

**Interfaces:**
- Consumes: `Loadout`.
- Produces: tela que abre/fecha pelo botão `Tab` (provisório, a decidir pelo Sthevan), com personagem, 4 slots de armadura, mochila e hotbar, e troca por clique.

- [ ] **Step 1:** Teste PlayMode `Inventory_SwapRustyForEpic_UpdatesLoadout`. **Step 2:** Falhar. **Step 3:** Implementar. **Step 4:** Passar. Commit.

### Task 13: Abertura das cartas

**Files:** Create `Game/UI/CardRevealScreen.cs`

**Interfaces:**
- Consumes: `CardOffer`.
- Produces: 3 cartas viradas para baixo aparecem juntas, abrem em leque para os lados, viram e mostram a raridade; o clique escolhe uma e chama `Loadout.Apply`.

- [ ] **Step 1:** Teste PlayMode `CardReveal_ClickPicksCard_AppliesToLoadout`. **Step 2:** Falhar. **Step 3:** Implementar. **Step 4:** Passar. Commit.

### Task 14: Tela inicial e criação de personagem

**Files:** Create `Game/UI/TitleScreen.cs`, `Game/UI/CharacterCreator.cs`, `Core/Character/CharacterLook.cs`; Test `Tests/EditMode/CharacterLookTests.cs`

**Interfaces:**
- Produces: `class CharacterLook { Gender Gender; float Height; int HairStyle; int SkinTone }`; a criação não altera nenhum atributo de combate.

- [ ] **Step 1:** Teste `Look_DoesNotAffectCombatStats`. **Step 2:** Falhar. **Step 3:** Implementar. **Step 4:** Passar. Commit.

### Task 15: Integração e partida completa

**Files:** Modify `Train.unity`; Create `Game/GameManager.cs`

**Interfaces:**
- Consumes: todas as anteriores.
- Produces: fluxo completo: tela inicial → criação → vagão 1 → … → vagão 9 → vitória → tela inicial; morrer reinicia no vagão 1.

- [ ] **Step 1:** Teste PlayMode `FullRun_FromTitleToVictory` (com inimigos dados como já mortos) e `Death_RestartsClean`.
- [ ] **Step 2:** Falhar. **Step 3:** Ligar tudo no `GameManager`. **Step 4:** Rodar toda a suíte (`bash scripts/run-tests.sh`) e jogar uma partida manual. Commit.

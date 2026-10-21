# Trem Masmorra — design da primeira versão jogável

Data: 2026-10-21. Autor da ideia: Sthevan. Este documento descreve a primeira versão jogável. Itens marcados como **[decisão minha]** são escolhas do redator, não do Sthevan.

## Intenção e por quê

O jogo é inspirado no episódio "O Trem Masmorra" (Hora de Aventura, 5ª temporada): um trem circular em que cada vagão é uma masmorra com inimigos novos. Finn e Jake lutam de vagão em vagão e vão se equipando com o que cai.

O que o Sthevan quer que o jogador **sinta**: a diversão de ir de vagão em vagão e se equipar. O tema do episódio sobre ficar preso no loop fica em segundo plano e não é o centro desta versão.

## Objetivos

- Uma versão jogável do ciclo completo: entrar no vagão, lutar, pegar saque, enfrentar o boss, escolher 1 de 3 cartas, equipar e seguir.
- Jogo 3D em **Unity (C#)**, single player.
- Controles e interface claros, no estilo dos jogos de referência (Minecraft para a hotbar e o inventário, Elden Ring para a vida do boss).

## Fora do escopo desta versão

- Multiplayer (a ideia futura é cooperativo para 4 jogadores, com 4 cartas no drop, uma por jogador, e a carta escolhida some para os outros).
- Guardar e usar o personagem de um jogador como boss para outros jogadores. A escolha entre "continuar no loop infinito" e "virar boss" também fica de fora. Sthevan disse que isso será pensado mais para frente.
- Servidor e salvamento online.

## O que é antiaobjetivo

- A criação de personagem **não pode dar poder nenhum**. Toda habilidade e poder vêm só das cartas dos bosses.

## Estrutura do trem

- 9 vagões nesta versão.
- A cada 3 vagões: 2 vagões comuns e o terceiro com boss. Os bosses ficam nos vagões 3, 6 e 9.
- 3 bosses diferentes, um por vagão de boss.
- Vagões comuns têm monstros comuns.

## Itens

- Tipos de item: espada, escudo, arco e outros, incluindo itens com efeito (exemplo do Sthevan: capa de invisibilidade). Ele pediu para ser criativo com esses efeitos.
- Armadura: cabeça, peitoral, braços e pernas. Ela funciona igual às armas: tem raridade e durabilidade e pode dropar de boss como carta.
- Durabilidade: vai de 100 (perfeita) a 0. Quando chega a 0, o item **quebra**. Não fica com dano 0.
- Raridades:
  - **Comum**: dropa nos vagões comuns, com durabilidade abaixo de 45.
  - **Raro**: dropa de boss, quase novo (perto de 100). Chance de cerca de 40% de vir novo no primeiro boss.
  - **Épico**: melhor que o raro. Dropa de boss, com chance baixa (cerca de 10%). **Decisão do Sthevan:** épicos só dropam a partir do vagão 5, o que nesta versão vale para os bosses dos vagões 6 e 9.
- As porcentagens acima são ponto de partida e podem ser ajustadas ao testar.
- Cartas depois do boss: o boss dropa 3 cartas. O jogador escolhe 1. Cada carta é de um de dois tipos:
  - **Habilidade:** o jogador a coloca no Q ou no E, e pode trocar quando quiser.
  - **Item:** ocupa um slot do inventário. Alguns itens já vêm com uma habilidade própria, que o jogador também pode colocar no Q ou no E.
- As cartas puxam para estilos diferentes. **[leitura minha do que o Sthevan listou]** Os três estilos são magia, dano físico à distância e dano físico corpo a corpo.
- Tipos de item que o Sthevan citou:
  - Itens de magia.
  - Dano físico à distância (por exemplo, arco).
  - Dano físico corpo a corpo (espada, machado, facão, espada pequena).
  - Armadura de vestir (os 4 slots).
  - Escudo.
  - Orbe que fica girando em volta do jogador e dá escudo.
- Um item só vai para a hotbar se o jogador for usá-lo. O jogador pode ter **no máximo 2 itens em uso** (por exemplo, arco e espada, ou duas espadas). Um equipamento especial que dê um braço extra permite usar mais um item além desses.

## Telas e interface

Câmera 3D vista por trás do personagem.

**Controles:** WASD move, mouse faz o ataque básico, Q e E são os poderes (primário e secundário), Espaço pula, Shift desvia de habilidade.

**HUD durante o jogo:**
- Hotbar embaixo, no estilo do Minecraft: slots quadrados. Embaixo de cada item há uma barrinha de durabilidade que muda de cor, sem número.
- Vida do boss em uma barra larga no topo da tela, estilo Elden Ring.
- Vida do jogador no canto superior.
- Habilidades no canto inferior direito, mostradas como imagens dos poderes.

**Inventário:** só aparece quando o jogador abre. O botão de abrir ainda será decidido pelo Sthevan. Mostra o personagem com os slots de armadura (cabeça, peitoral, braços, pernas), a mochila e a hotbar. O jogador pode trocar itens, por exemplo a espada ferrujada pela espada épica.

**Cartas do boss:** as 3 cartas aparecem juntas e abrem para os lados, como um leque. **[decisão minha]** Elas começam viradas para baixo e viram depois de abrir, para a raridade só aparecer no fim.

**Tela inicial** e **criação de personagem**: homem ou mulher, com tamanho e aparência. Só visual, sem bônus.

Mockups feitos durante a conversa estão em `.superpowers/brainstorm/` neste projeto.

## Arte e áudio

- O Claude cria os modelos 3D, que ficam por conta dele. Se o resultado não ficar bom, o Sthevan cria os modelos e envia os arquivos.
- **[decisão minha]** O estilo visual será simples e estilizado (low poly, cores chapadas), porque é o que dá para fazer com boa qualidade sem modelos externos.
- Nada foi dito sobre sons ou música. Fica a critério do construtor.

## Restrições

- O projeto fica em `D:\Projetos\trem-masmorra`. Nada é feito no `C:`.
- Engine: Unity, linguagem C#.

## Regras que o redator decidiu **[decisões minhas]**

O Sthevan não falou sobre estas, e o construtor pode mudar se algo não funcionar:
- **Morte:** ao morrer, o jogador volta para o vagão 1 e recomeça a run. Nada persiste entre sessões nesta versão.
- **Fim da run:** depois de derrotar o boss do vagão 9, aparece uma tela de vitória e o jogador volta à tela inicial.
- **Avançar de vagão:** a porta do vagão abre quando todos os monstros do vagão morrem. O boss é obrigatório. Não dá para voltar a um vagão anterior.
- **Durabilidade:** a arma e o escudo perdem durabilidade a cada golpe dado ou bloqueado. A armadura perde a cada golpe recebido.
- **Animação:** o Claude cria modelos humanoides simples com um rig compartilhado e animações básicas (andar, atacar, tomar dano, morrer). Se não ficar bom, o Sthevan envia os arquivos.
- **Q e E vazios:** antes da primeira habilidade, Q e E ficam sem função, e o jogador usa só o ataque básico do mouse.

## O que fica para o construtor

- Versão do Unity, pipeline de renderização e estrutura do projeto.
- Quais monstros comuns, quais 3 bosses e quais ataques e poderes eles têm.
- Quais itens com efeito existem, além da capa de invisibilidade.
- Quantos slots a hotbar e a mochila têm (os mockups usaram 5 na hotbar).
- Números de dano, vida e equilíbrio.
- Como o movimento do trem e a transição entre vagões aparecem.

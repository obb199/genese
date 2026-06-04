# Gênese — Projeto Unity (3D)

Projeto **Unity + C#** que porta o termômetro 3D (`demo3d/`) para o motor final. É o **esqueleto da
Etapa 1**: por enquanto reproduz o *diorama vivo* (criatura paramétrica, biomas, aldeia, nudges,
dia/noite); o motor de simulação real (genética herdável, especiação, cultura) entra em cima desta base.

> **Versão-alvo:** Unity **6000.0.76f1** (Unity 6 LTS). Se a sua for outra, o Hub pede para fazer
> *upgrade* ao abrir — pode aceitar.

---

## Como abrir e rodar

1. Instale o Editor (ver abaixo) e, no **Unity Hub → Open**, selecione a pasta `unity/Genese`.
2. No menu superior do Unity: **Gênese ▸ Criar cena Mundo Vivo** (cria `Assets/Scenes/MundoVivo.unity`
   já com o `Bootstrap`).
3. Aperte **Play**. O diorama 3D aparece, com um HUD de desenvolvimento (canto direito) para nudges,
   troca de bioma/cultura, pausar, ciclo de dia e nova geração.

> Se o mouse/câmera não responder: **Edit ▸ Project Settings ▸ Player ▸ Active Input Handling** →
> deixe em **Input Manager (Old)** ou **Both** (o termômetro usa a API de Input clássica).

### Instalar o Editor (Linux, via Hub headless)

```bash
UNITY_HUB_CLI_NO_DEPRECATION_WARNING=1 xvfb-run -a unityhub --headless install \
  --version 6000.0.76f1 --changeset 6f7f9e1c9e8a \
  --module linux-il2cpp --childModules
```

Depois ative a licença **Personal** (grátis) no Hub (Preferences ▸ Licenses ▸ Add ▸ free personal license).

---

## Arquitetura (espelha a demo modular `demo3d/`)

```
Assets/Genese/
├── Scripts/
│   ├── Palette.cs          Paleta-base e cores (tokens do DV)
│   ├── WorldData.cs        8 biomas + 6 culturas (structs + dicionários)
│   ├── Prim.cs             Fábrica de primitivas low-poly + cone procedural + materiais
│   ├── Genome.cs           Genoma visível + Genome.Random() (eixos fiéis ao creature.js)
│   ├── CreatureView.cs     Refs de animação de uma criatura
│   ├── CreatureBuilder.cs  Monta a criatura 3D a partir do genoma  (porta 10-creature.js)
│   ├── Agent.cs            Vaguear + squash & stretch                (porta 40-sim.js)
│   ├── WorldBuilder.cs     Terreno, biomas, flora, aldeia            (porta 20-world.js)
│   ├── CameraOrbit.cs      Câmera de diorama (órbita/arraste/zoom)
│   ├── SimManager.cs       Estado + nudges + dia/noite + HUD (IMGUI dev)
│   └── Bootstrap.cs        Cria câmera, luzes, mundo e simulação por código
└── Editor/
    └── SetupMundoVivo.cs   Menu "Gênese ▸ Criar cena Mundo Vivo"
```

**Render pipeline:** Built-in (shader `Standard`) — sem dependência de URP, para abrir em qualquer Unity 6.
A direção final (URP + pós-processo) pode ser adotada depois, conforme o `E10_Render_UX_Loja_3D`.

---

## Status e próximos passos

- ✅ **Esqueleto 3D** rodando por código (paridade visual com o termômetro WebGL).
- ⚠️ **Não compilado aqui** (sem Editor neste ambiente) — pode haver ajuste fino no 1º *Play*; reporte
  qualquer erro do Console que eu corrijo.
- **HUD** ainda é IMGUI (dev). No jogo final vira **uGUI/UI Toolkit** conforme o Design Visual.
- **Etapa 1 (a fazer):** genoma herdável + mutação reais, traços de comportamento (M02), decisão
  individual (M04) e **comportamento coletivo emergente** (M05) respondendo a 1 alavanca de influência.

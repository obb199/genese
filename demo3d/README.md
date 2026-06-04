# Gênese — Termômetro 3D (Mundo Vivo)

Diorama vivo **3D** em WebGL (Three.js). É o **alvo de arte** para a reconstrução em **Unity/C#** —
fixa formas, paleta, iluminação, câmera e o loop *observar → influenciar*.

> **Como abrir:** dê duplo-clique em `index.html` (ou arraste para o navegador). É **autossuficiente**
> (Three.js incluso em `vendor/`), **sem instalação e sem internet**.

---

## Estrutura modular

Os módulos são **scripts clássicos** carregados em ordem numérica (mesmo padrão modular do projeto Claude
Design: `icons.js → creature.js → biomes.js → buildings.js …`). Tudo vive no namespace único `window.G3`.

```
demo3d/
├── index.html            Shell: markup do HUD + inclusão dos scripts em ordem
├── css/
│   └── styles.css        Estilos do HUD (tokens de cor do DV)
├── js/
│   ├── 00-tokens.js      Paleta, rampas, cores de corpo/sinal, BIOMES, CULTURES
│   ├── 10-creature.js    G3.Creature — criatura paramétrica 3D (forma-mãe) + randomGenome
│   ├── 20-world.js       G3.World — terreno, biomas, flora/rochas, aldeia (fogueira/cabanas)
│   ├── 30-effects.js     G3.FX — efeitos de nudge (anel, raio, faísca)
│   ├── 40-sim.js         G3.Sim — estado + agentes (vagueio, alerta, mutação)
│   ├── 50-hud.js         G3.HUD — liga os controles do painel
│   └── 90-main.js        Bootstrap: renderer, cena, câmera, luzes, loop
├── vendor/
│   └── three.min.js      Three.js r155 (UMD, global THREE)
└── screenshots/          Capturas de referência
```

Dependências entre módulos (carregar nesta ordem):
`THREE → 00-tokens → 10-creature → 20-world → 30-effects → 40-sim → 50-hud → 90-main`.

---

## Personalização da criatura (fiel ao `creature.js` do Claude Design)

`G3.Creature.build(genoma)` monta a malha; `G3.Creature.randomGenome()` sorteia um genoma plausível.
Eixos implementados em 3D (✓) — ver `G3.Creature.PARAMS` no código:

| Eixo | Valores 3D | |
|---|---|---|
| `shape` | egg, round, tall, squat, pear, bean, blob | ✓ proporção do corpo |
| `color` / `color2` / `blend` | paleta + none/gradient/twotone/belly2/dorsal | ✓ cor e mistura |
| `pattern` | none, belly, spots, stripes | ✓ padrão |
| `eyes` / `pupil` | two/big/small/three/one · round/vertical | ✓ olhos |
| `mouth` | simple, beak, tusks, none | ✓ boca |
| `ears` | none, pointy, round, tuft, fan | ✓ orelhas |
| `tail` | none, short, long, curl, tuft | ✓ cauda |
| `legs` / `arms` | stubby/tall/none · stubby/claws/flippers | ✓ membros |
| `antennae` | none, pair, horns, crest, mane | ✓ cabeça |
| `finish` | matte, satin, metallic, iridescent | ✓ acabamento (material) |
| `emit` | none, eyes, antenna | ✓ emissão de luz |
| `ornament` | none, headband, facepaint, crest, religious | ✓ ornamento cultural |
| `signal` / `glow` | cor de emoção + aura | ✓ estado/sinal |

**Roadmap (modelar na próxima passada):** `snout`, `cheeks`, `neck`, `opacity` (translúcido/cristal/
fantasma), `asym` (cicatriz/queimadura), `temper` (postura por temperamento) e padrões volumétricos
(escamas/penas/pelagem). Estão listados como roadmap em `PARAMS`.

---

## Mundo

- **8 biomas** (`G3.BIOMES`) recoloríveis em runtime — espelham `biomes.js`. Seletor no HUD.
- **6 culturas de aldeia** (`G3.CULTURES`) recolorem cabanas/totem/fogueira — espelham `buildings.js`.
- Aldeia com **fogueira** + **pedra-coração** (cristal ciano), cabanas, totem; flora/rochas low-poly.
- Iluminação: sol direcional com **sombras**, hemisférica, **luz roxa de influência**, fogueira cintilante;
  **ciclo dia/noite** suave.

## Interação

- **Atenção** (recurso que regenera) + **nudges** *Sinal / Faísca / Inspiração* (clicar no chão).
- **Bioma** e **cultura** por seletor; **Pausar**, **Ciclo dia**, **Nova geração** (re-sorteia genomas).
- Câmera: órbita automática; **arraste** gira, **roda** dá zoom.

---

## Verificação headless (opcional, para CI/screenshots)

WebGL não roda no Firefox headless padrão (sem GPU). Para gerar screenshots por software:

```bash
PROF=$(mktemp -d)
printf 'user_pref("webgl.force-enabled",true);\nuser_pref("gfx.webrender.software",true);\nuser_pref("layers.acceleration.disabled",true);\n' > "$PROF/user.js"
LIBGL_ALWAYS_SOFTWARE=1 GALLIUM_DRIVER=llvmpipe \
  firefox --headless --no-remote --profile "$PROF" --window-size=1280,800 \
  --screenshot "$PWD/screenshots/termometro-3d.png" "file://$PWD/index.html"
```

> Este termômetro é **WebGL** por ser leve e instantâneo. O **jogo final é Unity/C#** — a direção de arte
> aqui (formas low-poly, paleta, luz, câmera de diorama) transfere diretamente para o Unity.

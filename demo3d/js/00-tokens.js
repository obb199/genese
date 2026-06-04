/* ============================================================
   GÊNESE 3D — 00 · Tokens de cor e dados de mundo
   ------------------------------------------------------------
   Fonte de verdade visual: tokens.json + biomes.js + buildings.js
   do projeto Claude Design. Aqui esses dados viram constantes 3D.
   Tudo pendurado em window.G3 (namespace único do termômetro).
   ============================================================ */
(function () {
  const G3 = (window.G3 = window.G3 || {});

  // Paleta-base (DV §3) ------------------------------------------------------
  G3.tok = {
    base: {
      fundo: '#1F2933',      // calma, espaço para o mundo respirar
      vida: '#2F6F62',       // vegetação / vitalidade
      influencia: '#8C5BAA', // presença do jogador, a "mão invisível"
      tensao: '#C0563A',     // conflito / alerta
      ui: '#EAF2EF',         // painéis e texto
      fogo: '#E0A24A',       // fogueira
      heart: '#5FE0C2',      // pedra-coração
    },
    // Rampas semânticas (baixo → alto) — cor é dado
    ramps: {
      agressividade: ['#7E8CA0', '#A8806F', '#C0563A', '#8E2D17'],
      vitalidade: ['#C7CDC8', '#7FB29E', '#2F6F62', '#16463C'],
      recursos: ['#8A7C5E', '#B8862F', '#E0C46A', '#F2E2A6'],
      fe: ['#9A93A8', '#A07CC4', '#7C4DBE', '#5E2E9E'],
      densidade: ['#CFE0D8', '#7FB29E', '#2F6F62', '#16463C'],
    },
    // Cores de corpo de criatura (espelha lib.js BODY_COLORS)
    body: ['#7FB29E', '#9FB4C4', '#C9A86A', '#9579B6', '#5FA9B0', '#B0644C', '#8FB36A', '#D292B6', '#7C4DBE', '#E0C46A'],
    // Cores de sinal/emoção (espelha lib.js SIGNAL_COLORS)
    signal: ['#8C5BAA', '#C0563A', '#2F6F62', '#B8862F', '#3E6B8C'],
  };

  // Biomas (espelha biomes.js BIOMES: sky[0..1], ground, foliage, accent) ----
  G3.BIOMES = {
    pradaria:  { nm: 'Pradaria / Savana', sky: ['#A9B36A', '#d9d6b0'], ground: '#6f7a3a', foliage: '#56A06A', accent: '#C9A86A', fog: '#c8c6a6' },
    floresta:  { nm: 'Floresta',          sky: ['#2C5247', '#88a890'], ground: '#274d40', foliage: '#2F6F62', accent: '#6E5638', fog: '#7d9488' },
    deserto:   { nm: 'Deserto',           sky: ['#D9B873', '#f0e2b8'], ground: '#C9A86A', foliage: '#9DBB6A', accent: '#B58A3E', fog: '#e6d2a0' },
    tundra:    { nm: 'Tundra / Gelo',     sky: ['#9FB4C4', '#e3edf2'], ground: '#C2D2DA', foliage: '#9FB4C4', accent: '#7FA0B6', fog: '#d6e2e8' },
    montanha:  { nm: 'Montanha / Rocha',  sky: ['#9A938A', '#cfc7ba'], ground: '#8A8377', foliage: '#7E8A5E', accent: '#5C564B', fog: '#bdb4a6' },
    agua:      { nm: 'Água / Costa',      sky: ['#5FA9B0', '#bfe3e0'], ground: '#5b7e6a', foliage: '#3E8C7A', accent: '#3E6B8C', fog: '#a9d4d2' },
    pantano:   { nm: 'Pântano / Úmido',   sky: ['#3C4A33', '#8a9a72'], ground: '#3a4a2e', foliage: '#4a6a3a', accent: '#6a6a3a', fog: '#7e8c66' },
    vulcanico: { nm: 'Vulcânico',         sky: ['#5A2E26', '#b06a4a'], ground: '#3A2A28', foliage: '#7a4a3a', accent: '#C0563A', fog: '#8a5040' },
  };

  // Culturas da aldeia (espelha buildings.js CULTURES: wall, roof, accent, flame)
  G3.CULTURES = {
    floresta:    { nm: 'Floresta',  wall: '#C9A86A', roof: '#B0644C', accent: '#8C5BAA', flame: '#E0A24A' },
    arido:       { nm: 'Árido',     wall: '#D8C49A', roof: '#B8862F', accent: '#C0563A', flame: '#E0C46A' },
    medieval:    { nm: 'Reino',     wall: '#C7BBA2', roof: '#7E4A3E', accent: '#B7402E', flame: '#E08A3A' },
    arcana:      { nm: 'Arcana',    wall: '#9579B6', roof: '#5E2E9E', accent: '#5FE0C2', flame: '#A07CC4' },
    imperial:    { nm: 'Imperial',  wall: '#D2C0A0', roof: '#8E2D17', accent: '#E0C46A', flame: '#E0A24A' },
    tecnologica: { nm: 'Tecnológica', wall: '#9FB4C4', roof: '#3E6B8C', accent: '#5FE0C2', flame: '#7FD0E0' },
  };

  // Helpers de cor ----------------------------------------------------------
  G3.color = (hex) => new THREE.Color(hex);
  G3.lerpHex = (a, b, t) => '#' + new THREE.Color(a).lerp(new THREE.Color(b), t).getHexString();
  G3.pick = (arr) => arr[Math.floor(Math.random() * arr.length)];
  G3.rng = (a, b) => a + Math.random() * (b - a);
})();

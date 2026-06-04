/* ============================================================
   GÊNESE 3D — 50 · HUD (interface do Mundo Vivo)
   ------------------------------------------------------------
   Liga os controles do painel (Atenção, nudges, bioma, cultura,
   tempo) ao resto. Não conhece Three.js — só DOM + callbacks.
   G3.HUD.init(ctx) → api { mode, updateAttention, setCycle, ... }
   ============================================================ */
(function () {
  const G3 = (window.G3 = window.G3 || {});

  const DEFAULT_FOOT = 'Termômetro de arte 3D (WebGL) · direção-alvo para reconstrução em Unity/C# — formas, paleta e iluminação que o jogo final deve ter';
  const $ = (id) => document.getElementById(id);

  function init(ctx) {
    // selects de bioma e cultura
    const bsel = $('biomeSel');
    bsel.innerHTML = Object.keys(G3.BIOMES).map(k => `<option value="${k}">${G3.BIOMES[k].nm}</option>`).join('');
    bsel.value = ctx.biome;
    bsel.onchange = () => ctx.onBiome(bsel.value);

    const csel = $('cultSel');
    csel.innerHTML = Object.keys(G3.CULTURES).map(k => `<option value="${k}">${G3.CULTURES[k].nm}</option>`).join('');
    csel.value = ctx.culture;
    csel.onchange = () => ctx.onCulture(csel.value);

    // nudges
    const api = { mode: null };
    const modeBtns = [['bSinal', 'sinal'], ['bFaisca', 'faisca'], ['bInsp', 'inspiracao']];
    function setMode(m) {
      api.mode = (api.mode === m ? null : m);
      modeBtns.forEach(([id, k]) => $(id).classList.toggle('on', api.mode === k));
      $('modeHint').textContent = api.mode ? `Nudge ativo: ${api.mode} — clique no chão.` : 'Nenhum nudge ativo.';
    }
    modeBtns.forEach(([id, k]) => { $(id).onclick = () => setMode(k); });

    // tempo
    $('bPlay').onclick = function () { const p = ctx.onPlay(); this.textContent = p ? '⏸ Pausar' : '▶ Rodar'; };
    $('bDay').onclick = () => ctx.onDay();
    $('bRand').onclick = () => ctx.onRandomize();

    // métodos de atualização
    let footT;
    api.updateAttention = (v) => { $('atFill').style.width = v + '%'; $('atNum').textContent = Math.round(v); };
    api.setCycle = (n) => { $('cycle').textContent = Math.max(0, Math.floor(n)); };
    api.setPop = (n) => { $('pop').textContent = n; };
    api.setStage = (s) => { $('stageName').textContent = s; };
    api.flash = (msg) => { const f = $('foot'); f.textContent = '▸ ' + msg; clearTimeout(footT); footT = setTimeout(() => f.textContent = DEFAULT_FOOT, 2800); };
    api.setMode = setMode;
    return api;
  }

  G3.HUD = { init };
})();

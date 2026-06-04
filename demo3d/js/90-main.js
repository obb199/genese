/* ============================================================
   GÊNESE 3D — 90 · Bootstrap (renderer, cena, câmera, loop)
   ------------------------------------------------------------
   Monta a cena, instancia World/Sim/FX/HUD, trata câmera (órbita
   + arraste + zoom), aplica nudges via raycast no chão e roda o
   loop de render. Último script a carregar.
   ============================================================ */
(function () {
  const G3 = window.G3;
  const T = THREE;

  // ---- renderer ----
  const app = document.getElementById('app');
  const renderer = new T.WebGLRenderer({ antialias: true });
  renderer.setSize(innerWidth, innerHeight);
  renderer.setPixelRatio(Math.min(2, devicePixelRatio || 1));
  renderer.shadowMap.enabled = true;
  renderer.shadowMap.type = T.PCFSoftShadowMap;
  renderer.toneMapping = T.ACESFilmicToneMapping;
  renderer.toneMappingExposure = 1.05;
  renderer.outputColorSpace = T.SRGBColorSpace;
  app.appendChild(renderer.domElement);

  // ---- cena + céu ----
  const scene = new T.Scene();
  function skyTexture(top, bot) {
    const c = document.createElement('canvas'); c.width = 8; c.height = 256;
    const ctx = c.getContext('2d');
    const grad = ctx.createLinearGradient(0, 0, 0, 256);
    grad.addColorStop(0, top); grad.addColorStop(1, bot);
    ctx.fillStyle = grad; ctx.fillRect(0, 0, 8, 256);
    const t = new T.CanvasTexture(c); t.colorSpace = T.SRGBColorSpace; return t;
  }
  scene.fog = new T.Fog(0xc8c6a6, 22, 46);

  // ---- câmera (órbita) ----
  const camera = new T.PerspectiveCamera(46, innerWidth / innerHeight, 0.1, 200);
  let camR = 17, camAz = 0.6, camEl = 0.62, userAz = 0;
  function placeCam() {
    const az = camAz + userAz;
    camera.position.set(Math.cos(az) * camR * Math.cos(camEl), Math.sin(camEl) * camR + 1.5, Math.sin(az) * camR * Math.cos(camEl));
    camera.lookAt(0, 1.0, 0);
  }

  // ---- luzes ----
  const hemi = new T.HemisphereLight(0xd7e0b0, 0x39402c, 0.85); scene.add(hemi);
  const sun = new T.DirectionalLight(0xfff0d0, 1.55);
  sun.position.set(9, 14, 6); sun.castShadow = true;
  sun.shadow.mapSize.set(2048, 2048);
  const d = 18; Object.assign(sun.shadow.camera, { left: -d, right: d, top: d, bottom: -d, near: 1, far: 50 });
  sun.shadow.bias = -0.0004; scene.add(sun);
  const influence = new T.PointLight(0x8C5BAA, 0.0, 40, 1.6); influence.position.set(0, 12, 0); scene.add(influence);
  const fire = new T.PointLight(0xE0A24A, 2.2, 14, 2.0); fire.position.set(0, 1.2, 0); scene.add(fire);

  // ---- mundo / sim / fx ----
  const world = new G3.World(scene);
  const sim = new G3.Sim(scene, world);
  const fx = new G3.FX(scene);
  let hud = null; // atribuído após a cena estar pronta (evita TDZ nas funções abaixo)
  let biome = 'pradaria', culture = 'floresta';
  function applyBiome(key) {
    biome = key; const b = world.setBiome(key);
    scene.background = skyTexture(b.sky[1], b.sky[0]); // topo claro → horizonte
    scene.fog.color.set(b.fog);
    hud && hud.flash('Pressão ambiental: bioma → ' + b.nm);
  }
  function applyCulture(key) { culture = key; const c = world.setCulture(key); fire.color.set(c.flame); hud && hud.flash('Cultura da aldeia: ' + c.nm); }
  applyBiome(biome); world.setCulture(culture);
  const popN = sim.spawn(10);

  // ---- HUD ----
  const stages = ['Bando', 'Tribo', 'Aldeia'];
  hud = G3.HUD.init({
    biome, culture,
    onBiome: applyBiome, onCulture: applyCulture,
    onPlay: () => { sim.state.playing = !sim.state.playing; return sim.state.playing; },
    onDay: () => sim.toggleDay(),
    onRandomize: () => { sim.creatures.forEach(c => sim.scene.remove(c)); sim.creatures.length = 0; sim.spawn(10); hud.flash('Nova geração — genomas re-sorteados'); },
  });
  hud.updateAttention(sim.state.attention); hud.setPop(popN);

  // ---- nudges via raycast no chão ----
  const ray = new T.Raycaster(); const ndc = new T.Vector2();
  function groundPoint(ev) {
    ndc.x = (ev.clientX / innerWidth) * 2 - 1; ndc.y = -(ev.clientY / innerHeight) * 2 + 1;
    ray.setFromCamera(ndc, camera);
    const hit = ray.intersectObject(world.ground, false);
    return hit.length ? hit[0].point : null;
  }
  function applyNudge(p, mode) {
    if (sim.state.attention < 14) { hud.flash('Atenção insuficiente'); return; }
    sim.state.attention = Math.max(0, sim.state.attention - 14); hud.updateAttention(sim.state.attention);
    if (mode === 'sinal') {
      fx.beam(p, 0xEAD37A); fx.ring(p, 0x8C5BAA); sim.state.infl = 1.0;
      sim.alertNear(p, 3.2); hud.flash('Sinal — criaturas próximas reagem; fé tende a subir');
    } else if (mode === 'faisca') {
      fx.spark(p, 0xC0563A); sim.mutateNear(p, 4); hud.flash('Faísca de mutação — genoma da criatura mais próxima muda');
    } else if (mode === 'inspiracao') {
      fx.spark(p, 0xB8862F); fx.ring(p, 0xB8862F); hud.flash('Inspiração — chance de uma Figura surgir nesta geração');
    }
  }

  // ---- controles de câmera ----
  let down = null, moved = 0;
  renderer.domElement.addEventListener('pointerdown', e => { down = { x: e.clientX, az: userAz }; moved = 0; });
  addEventListener('pointermove', e => { if (!down) return; moved += Math.abs(e.movementX || 0); userAz = down.az - (e.clientX - down.x) * 0.005; });
  addEventListener('pointerup', e => { if (down && moved < 5 && hud.mode) { const p = groundPoint(e); if (p) applyNudge(p, hud.mode); } down = null; });
  addEventListener('wheel', e => { camR = Math.max(9, Math.min(28, camR + Math.sign(e.deltaY) * 1.1)); }, { passive: true });
  addEventListener('resize', () => { camera.aspect = innerWidth / innerHeight; camera.updateProjectionMatrix(); renderer.setSize(innerWidth, innerHeight); });

  // ---- loop ----
  let last = performance.now();
  function frame(now) {
    const dt = Math.min(0.05, (now - last) / 1000); last = now;
    sim.step(dt, now);

    // HUD / luz / fogo / dia-noite
    hud.setCycle(sim.state.cycle);
    hud.setStage(stages[Math.min(2, Math.floor(sim.state.cycle / 40))]);
    if (sim.state.playing) hud.updateAttention(sim.state.attention);
    const k = sim.state.dayT;
    hemi.intensity = 0.35 + 0.6 * k; sun.intensity = 0.5 + 1.2 * k;
    fire.intensity = 2.0 + Math.sin(now * 0.02) * 0.5 + (1 - k) * 0.8;
    influence.intensity = sim.state.infl * 3.0;
    if (world.flame) world.flame.scale.y = 1 + Math.sin(now * 0.03) * 0.18;
    if (world.heart) { world.heart.rotation.y += dt * 0.8; world.heart.position.y = 1.35 + Math.sin(now * 0.003) * 0.06; }

    camAz += dt * 0.05; // órbita automática
    placeCam();
    fx.update(dt);
    renderer.render(scene, camera);
    requestAnimationFrame(frame);
  }

  const loading = document.getElementById('loading'); if (loading) loading.remove();
  requestAnimationFrame(frame);
})();

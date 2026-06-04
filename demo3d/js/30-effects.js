/* ============================================================
   GÊNESE 3D — 30 · Efeitos de nudge (partículas/feedback)
   ------------------------------------------------------------
   Espelha meta-assets.js (nudgeRipple, lightBeam, signalPulse).
   FX.spawn* criam malhas temporárias; FX.update anima e descarta.
   ============================================================ */
(function () {
  const G3 = (window.G3 = window.G3 || {});
  const T = THREE;

  function FX(scene) { this.scene = scene; this.list = []; }

  FX.prototype.ring = function (pos, color) {
    const m = new T.Mesh(new T.TorusGeometry(0.3, 0.07, 10, 40),
      new T.MeshBasicMaterial({ color, transparent: true, opacity: 0.9 }));
    m.rotation.x = Math.PI / 2; m.position.copy(pos); m.position.y = 0.06;
    this.scene.add(m); this.list.push({ m, life: 1.2, age: 0, type: 'ring' });
  };
  FX.prototype.beam = function (pos, color) {
    const m = new T.Mesh(new T.CylinderGeometry(0.15, 1.1, 12, 20, 1, true),
      new T.MeshBasicMaterial({ color, transparent: true, opacity: 0.5, side: T.DoubleSide, blending: T.AdditiveBlending, depthWrite: false }));
    m.position.set(pos.x, 6, pos.z);
    this.scene.add(m); this.list.push({ m, life: 1.6, age: 0, type: 'beam' });
  };
  FX.prototype.spark = function (pos, color) {
    const m = new T.Mesh(new T.SphereGeometry(0.25, 16, 12),
      new T.MeshBasicMaterial({ color, transparent: true, opacity: 0.95, blending: T.AdditiveBlending, depthWrite: false }));
    m.position.copy(pos); m.position.y = 0.6;
    this.scene.add(m); this.list.push({ m, life: 0.9, age: 0, type: 'spark' });
  };

  FX.prototype.update = function (dt) {
    for (let i = this.list.length - 1; i >= 0; i--) {
      const f = this.list[i]; f.age += dt; const p = f.age / f.life;
      if (f.type === 'ring') { const s = 1 + p * 5; f.m.scale.set(s, s, s); f.m.material.opacity = 0.9 * (1 - p); }
      else if (f.type === 'beam') { f.m.material.opacity = 0.5 * (1 - p); f.m.rotation.y += dt * 2; }
      else if (f.type === 'spark') { const s = 1 + p * 3; f.m.scale.set(s, s, s); f.m.material.opacity = 0.95 * (1 - p); }
      if (p >= 1) { this.scene.remove(f.m); f.m.geometry.dispose(); f.m.material.dispose(); this.list.splice(i, 1); }
    }
  };

  G3.FX = FX;
})();

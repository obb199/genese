/* ============================================================
   GÊNESE 3D — 40 · Simulação (estado + agentes)
   ------------------------------------------------------------
   NOTA: esta é uma simulação-vitrine (termômetro). NÃO contém o
   motor real (genética herdável, especiação, cultura, linguagem)
   — isso é da Etapa 1 em Unity. Aqui os agentes só vagueiam e
   reagem a nudges, para dar vida ao diorama.
   ============================================================ */
(function () {
  const G3 = (window.G3 = window.G3 || {});
  const T = THREE;
  const RAD = 12.5; // raio em que as criaturas circulam

  function Sim(scene, world) {
    this.scene = scene; this.world = world;
    this.creatures = [];
    this.state = { cycle: 0, attention: 62, playing: true, infl: 0, dayT: 0.62 };
  }

  Sim.prototype.spawn = function (n) {
    for (let i = 0; i < n; i++) {
      const c = G3.Creature.build(G3.Creature.randomGenome());
      const a = Math.random() * 7, r = G3.rng(1.8, 11);
      c.position.set(Math.cos(a) * r, 0, Math.sin(a) * r);
      c.userData.heading = Math.random() * 7;
      c.userData.spd = G3.rng(0.5, 1.1);
      c.userData.t = Math.random() * 3;
      c.userData.state = 'walk';
      c.userData.pause = 0;
      c.userData.hop = Math.random() * 7;
      c.userData.pop = 0;
      this.scene.add(c); this.creatures.push(c);
    }
    return this.creatures.length;
  };

  // muta a criatura mais próxima de um ponto (faísca de mutação) -------------
  Sim.prototype.mutateNear = function (point, maxDist) {
    let best = null, bd = 1e9;
    this.creatures.forEach(c => { const d = c.position.distanceTo(point); if (d < bd) { bd = d; best = c; } });
    if (best && bd < (maxDist || 4)) {
      const ng = G3.Creature.randomGenome();
      const np = best.position.clone(), nh = best.userData.heading;
      this.scene.remove(best);
      const idx = this.creatures.indexOf(best);
      const c = G3.Creature.build(ng);
      c.position.copy(np); c.userData.heading = nh; c.userData.spd = G3.rng(0.5, 1.1);
      c.userData.t = 0; c.userData.state = 'walk'; c.userData.pause = 0; c.userData.hop = 0; c.userData.pop = 1.0;
      this.scene.add(c); this.creatures[idx] = c;
      return true;
    }
    return false;
  };

  // deixa criaturas próximas em "alerta" (reação ao Sinal) -------------------
  Sim.prototype.alertNear = function (point, radius) {
    this.creatures.forEach(c => { if (c.position.distanceTo(point) < (radius || 3.2)) { c.userData.state = 'alert'; c.userData.pause = 1.6; } });
  };

  // passo da simulação ------------------------------------------------------
  Sim.prototype.step = function (dt, now) {
    if (!this.state.playing) return;
    this.state.cycle += dt;
    if (this.state.attention < 100) this.state.attention = Math.min(100, this.state.attention + dt * 1.5);
    this.state.infl = Math.max(0, this.state.infl - dt * 0.6);

    this.creatures.forEach(c => {
      const u = c.userData; u.t -= dt; u.hop += dt * 8;
      if (u.pause > 0) {
        u.pause -= dt; if (u.pause <= 0) u.state = 'walk';
        u.body.position.y = 0.5 * u.baseTall + Math.abs(Math.sin(now * 0.04)) * 0.05; // tremor de alerta
      } else {
        c.position.x += Math.cos(u.heading) * u.spd * dt;
        c.position.z += Math.sin(u.heading) * u.spd * dt;
        const r = Math.hypot(c.position.x, c.position.z);
        if (r > RAD) u.heading = Math.atan2(-c.position.z, -c.position.x) + G3.rng(-0.6, 0.6);
        if (u.t <= 0) { u.t = G3.rng(1.4, 3.5); if (Math.random() < 0.5) u.heading += G3.rng(-1.2, 1.2); }
        // hop + squash & stretch
        const hop = Math.abs(Math.sin(u.hop));
        u.body.position.y = 0.5 * u.baseTall + hop * 0.12;
        const sq = 1 + 0.12 * Math.sin(u.hop * 2);
        const wxz = u.wide / Math.sqrt(sq);
        u.body.scale.set(wxz, u.baseTall * sq, wxz);
        c.rotation.y = -u.heading + Math.PI / 2;
      }
      // "pop" de mutação
      if (u.pop > 0) { u.pop -= dt * 2; const s = 1 + Math.max(0, u.pop) * 0.4; c.scale.setScalar((parseFloat(u.genome.size) || 1) * s); }
    });

    // ciclo dia/noite suave
    this.state.dayT = Math.max(0, Math.min(1, this.state.dayT + this._dayDir * dt * 0.06));
  };

  Sim.prototype._dayDir = 1;
  Sim.prototype.toggleDay = function () { this._dayDir *= -1; };

  G3.Sim = Sim;
})();

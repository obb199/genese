/* ============================================================
   GÊNESE 3D — 20 · Mundo (diorama vivo)
   ------------------------------------------------------------
   Terreno em disco, biomas recoloríveis (8), flora/props low-poly
   e a aldeia com fogueira + pedra-coração + cabanas por cultura.
   Espelha biomes.js (cenas por bioma) e buildings.js (village).
   ============================================================ */
(function () {
  const G3 = (window.G3 = window.G3 || {});
  const T = THREE;
  const R = 15; // raio do diorama

  function World(scene) {
    this.scene = scene;
    this.trees = [];
    this.rocks = [];
    this.huts = [];
    this._build();
  }

  World.prototype._build = function () {
    const scene = this.scene;

    // terreno (disco)
    this.groundMat = new T.MeshStandardMaterial({ color: 0x6f7a3a, roughness: 1, metalness: 0 });
    this.ground = new T.Mesh(new T.CylinderGeometry(R, R, 1, 72), this.groundMat);
    this.ground.position.y = -0.5; this.ground.receiveShadow = true; scene.add(this.ground);

    // relevo leve
    this.mounds = [];
    for (let i = 0; i < 7; i++) {
      const m = new T.Mesh(new T.SphereGeometry(G3.rng(1.5, 3.4), 16, 12),
        new T.MeshStandardMaterial({ color: 0x6f7a3a, roughness: 1 }));
      const a = Math.random() * 7, r = G3.rng(4, 12);
      m.position.set(Math.cos(a) * r, G3.rng(-2.6, -1.4), Math.sin(a) * r);
      m.scale.y = 0.4; m.receiveShadow = true; m.castShadow = true; scene.add(m); this.mounds.push(m);
    }

    // borda escura (vinheta de diorama)
    const rim = new T.Mesh(new T.TorusGeometry(R, 0.5, 12, 72),
      new T.MeshStandardMaterial({ color: 0x2a2a1c, roughness: 1 }));
    rim.rotation.x = Math.PI / 2; rim.position.y = -0.2; scene.add(rim);

    // flora + rochas
    for (let i = 0; i < 9; i++) { const a = Math.random() * 7, rr = G3.rng(7, 13.5); this.trees.push(this._tree(Math.cos(a) * rr, Math.sin(a) * rr, G3.rng(0.8, 1.4))); }
    for (let i = 0; i < 10; i++) { const a = Math.random() * 7, rr = G3.rng(5, 13); this.rocks.push(this._rock(Math.cos(a) * rr, Math.sin(a) * rr, G3.rng(0.7, 1.4))); }

    // aldeia
    this._village();
  };

  World.prototype._tree = function (x, z, s) {
    const g = new T.Group();
    const trunk = new T.Mesh(new T.CylinderGeometry(0.12 * s, 0.18 * s, 1.0 * s, 8),
      new T.MeshStandardMaterial({ color: 0x6E5638, roughness: 1 }));
    trunk.position.y = 0.5 * s; trunk.castShadow = true; g.add(trunk);
    const foliage = [];
    for (let i = 0; i < 3; i++) {
      const b = new T.Mesh(new T.IcosahedronGeometry(G3.rng(0.55, 0.8) * s, 0),
        new T.MeshStandardMaterial({ color: 0x56A06A, roughness: 0.9, flatShading: true }));
      b.position.set(G3.rng(-0.25, 0.25) * s, (1.0 + i * 0.45) * s, G3.rng(-0.25, 0.25) * s);
      b.castShadow = true; g.add(b); foliage.push(b);
    }
    g.userData.foliage = foliage;
    g.position.set(x, 0, z); g.rotation.y = Math.random() * 7; this.scene.add(g); return g;
  };

  World.prototype._rock = function (x, z, s) {
    const r = new T.Mesh(new T.DodecahedronGeometry(G3.rng(0.4, 0.8) * s, 0),
      new T.MeshStandardMaterial({ color: 0x8A8377, roughness: 1, flatShading: true }));
    r.position.set(x, 0.2 * s, z); r.rotation.set(Math.random(), Math.random(), Math.random());
    r.castShadow = true; r.receiveShadow = true; this.scene.add(r); return r;
  };

  World.prototype._hut = function (x, z, rot) {
    const g = new T.Group();
    const wall = new T.Mesh(new T.CylinderGeometry(0.7, 0.8, 0.8, 10),
      new T.MeshStandardMaterial({ color: 0xC9A86A, roughness: 1 }));
    wall.position.y = 0.4; wall.castShadow = true; wall.receiveShadow = true; g.add(wall);
    const roof = new T.Mesh(new T.ConeGeometry(1.0, 0.8, 10),
      new T.MeshStandardMaterial({ color: 0xB0644C, roughness: 1 }));
    roof.position.y = 1.1; roof.castShadow = true; g.add(roof);
    g.userData = { wall, roof };
    g.position.set(x, 0, z); g.rotation.y = rot || 0; this.scene.add(g); return g;
  };

  World.prototype._village = function () {
    // fogueira + pedra-coração
    const g = new T.Group();
    for (let i = 0; i < 7; i++) {
      const a = (i / 7) * 7;
      const s = new T.Mesh(new T.DodecahedronGeometry(0.18, 0),
        new T.MeshStandardMaterial({ color: 0x7a7468, roughness: 1, flatShading: true }));
      s.position.set(Math.cos(a) * 0.6, 0.12, Math.sin(a) * 0.6); s.castShadow = true; g.add(s);
    }
    for (let i = 0; i < 4; i++) {
      const a = (i / 4) * Math.PI;
      const l = new T.Mesh(new T.CylinderGeometry(0.06, 0.06, 0.8, 6),
        new T.MeshStandardMaterial({ color: 0x5a4530, roughness: 1 }));
      l.position.y = 0.18; l.rotation.set(Math.PI / 2.4, a, 0); g.add(l);
    }
    const flameMat = new T.MeshStandardMaterial({ color: 0xE0A24A, emissive: 0xE0A24A, emissiveIntensity: 1.4, roughness: 0.4 });
    const flame = new T.Mesh(new T.ConeGeometry(0.22, 0.7, 12), flameMat);
    flame.position.y = 0.5; g.add(flame);
    const heart = new T.Mesh(new T.OctahedronGeometry(0.22, 0),
      new T.MeshStandardMaterial({ color: 0x5FE0C2, emissive: 0x3fbfa6, emissiveIntensity: 1.1, roughness: 0.2, metalness: 0.1 }));
    heart.position.y = 1.35; g.add(heart);
    this.scene.add(g);
    this.fireGroup = g; this.flame = flame; this.flameMat = flameMat; this.heart = heart;

    // cabanas
    this.huts.push(this._hut(-2.4, 1.6, 0.4));
    this.huts.push(this._hut(2.6, 1.2, -0.5));
    this.huts.push(this._hut(0.4, -2.7, 0.1));

    // totem
    this.totemMat = new T.MeshStandardMaterial({ color: 0x8C5BAA, roughness: 0.8 });
    this.totem = new T.Mesh(new T.CylinderGeometry(0.16, 0.2, 1.8, 8), this.totemMat);
    this.totem.position.set(-3.4, 0.9, -1.6); this.totem.castShadow = true; this.scene.add(this.totem);
  };

  // recolore o mundo conforme o bioma (pressão ambiental do jogador) --------
  World.prototype.setBiome = function (key) {
    const b = G3.BIOMES[key]; if (!b) return;
    this.biome = key;
    this.groundMat.color.set(b.ground);
    this.mounds.forEach(m => m.material.color.set(b.ground).offsetHSL(0, 0, G3.rng(-0.04, 0.05)));
    this.trees.forEach(t => t.userData.foliage.forEach(f => f.material.color.set(b.foliage).offsetHSL(0, 0, G3.rng(-0.05, 0.05))));
    return b;
  };

  // recolore a aldeia conforme a cultura ------------------------------------
  World.prototype.setCulture = function (key) {
    const c = G3.CULTURES[key]; if (!c) return;
    this.culture = key;
    this.huts.forEach(h => { h.userData.wall.material.color.set(c.wall); h.userData.roof.material.color.set(c.roof); });
    this.totemMat.color.set(c.accent);
    this.flameMat.color.set(c.flame); this.flameMat.emissive.set(c.flame);
    return c;
  };

  G3.World = World;
})();

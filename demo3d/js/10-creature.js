/* ============================================================
   GÊNESE 3D — 10 · Criatura paramétrica (forma-mãe modular)
   ------------------------------------------------------------
   Porta para 3D o sistema de personalização do creature.js do
   Claude Design (DV §4 / GDD §8.3 / AA §2). Genoma → malhas.
   build(genome) → THREE.Group com userData para animação.
   randomGenome() → genoma aleatório plausível.
   PARAMS documenta cada eixo de personalização e seu status 3D.
   ============================================================ */
(function () {
  const G3 = (window.G3 = window.G3 || {});
  const T = THREE;

  // ---- Eixos de personalização (espelham creature.js) --------------------
  // Implementado em 3D ✓ | aproximado ~ | roadmap ○
  const PARAMS = {
    shape:    ['egg', 'round', 'tall', 'squat', 'pear', 'bean', 'blob'],     // ✓ proporção do corpo
    color:    'hex (paleta body)',                                            // ✓ cor base
    color2:   'hex (paleta body)',                                            // ✓ cor secundária
    blend:    ['none', 'gradient', 'twotone', 'belly2', 'dorsal'],            // ✓ mistura de cor
    pattern:  ['none', 'belly', 'spots', 'stripes'],                          // ✓ textura/padrão
    size:     '0.82 | 1 | 1.18',                                              // ✓ escala
    eyes:     ['two', 'big', 'small', 'three', 'one'],                        // ✓ olhos
    pupil:    ['round', 'vertical'],                                          // ✓ pupila
    mouth:    ['simple', 'beak', 'tusks', 'none'],                            // ✓ boca
    ears:     ['none', 'pointy', 'round', 'tuft', 'fan'],                     // ✓ orelhas
    tail:     ['none', 'short', 'long', 'curl', 'tuft'],                      // ✓ cauda
    legs:     ['stubby', 'tall', 'none'],                                     // ✓ pernas
    arms:     ['none', 'stubby', 'claws', 'flippers'],                        // ✓ braços
    antennae: ['none', 'pair', 'horns', 'crest', 'mane'],                     // ✓ cabeça/antenas
    finish:   ['matte', 'satin', 'metallic', 'iridescent'],                  // ✓ acabamento (material)
    emit:     ['none', 'eyes', 'antenna'],                                    // ✓ emissão de luz
    ornament: ['none', 'headband', 'facepaint', 'crest', 'religious'],        // ✓ ornamento cultural
    signal:   'hex (paleta signal)',                                          // ✓ cor de sinal/emoção
    glow:     'bool',                                                          // ✓ aura de sinal
    // Roadmap (modelar depois): snout, cheeks, neck, opacity (cristal/fantasma),
    // asym (cicatriz/queimadura), temper, padrões scales/feathers/fur volumétricos.
  };

  const DEFAULTS = {
    shape: 'egg', color: '#7FB29E', color2: '#5FA9B0', blend: 'none',
    pattern: 'belly', size: 1, eyes: 'two', pupil: 'round', mouth: 'simple',
    ears: 'none', tail: 'none', legs: 'stubby', arms: 'none', antennae: 'none',
    finish: 'matte', emit: 'none', ornament: 'none', signal: '#8C5BAA', glow: false,
  };

  // proporção por forma: wide (xz) e tall (y), e bottom-heavy (pear)
  const SHAPE = {
    egg:   { wide: 1.00, tall: 1.15, bottom: 0.0 },
    round: { wide: 1.06, tall: 1.00, bottom: 0.0 },
    tall:  { wide: 0.86, tall: 1.42, bottom: 0.0 },
    squat: { wide: 1.20, tall: 0.82, bottom: 0.0 },
    pear:  { wide: 0.92, tall: 1.10, bottom: 0.5 },
    bean:  { wide: 0.96, tall: 1.18, bottom: 0.0, lean: 0.12 },
    blob:  { wide: 1.04, tall: 0.96, bottom: 0.15 },
  };

  function material(col, finish, emitCol, emitI) {
    const o = { color: col, roughness: 0.55, metalness: 0.02 };
    if (finish === 'satin') o.roughness = 0.4;
    else if (finish === 'metallic') { o.roughness = 0.28; o.metalness = 0.85; }
    else if (finish === 'iridescent') { o.roughness = 0.18; o.metalness = 0.5; o.emissive = new T.Color(emitCol || col).multiplyScalar(0.25); }
    if (emitI) { o.emissive = new T.Color(emitCol).multiplyScalar(emitI); }
    return new T.MeshStandardMaterial(o);
  }

  // ---- construção da criatura --------------------------------------------
  function build(gIn) {
    const g = Object.assign({}, DEFAULTS, gIn || {});
    const grp = new T.Group();
    const sh = SHAPE[g.shape] || SHAPE.egg;
    const col = new T.Color(g.color);
    const col2 = new T.Color(g.color2);
    const sig = new T.Color(g.signal);
    const wide = sh.wide, tall = sh.tall;

    const bodyMat = material(col, g.finish, g.signal, g.emit === 'body' ? 0.4 : 0);
    bodyMat.flatShading = false;

    // corpo
    const body = new T.Mesh(new T.SphereGeometry(0.5, 28, 20), bodyMat);
    body.scale.set(wide, tall, wide);
    body.position.y = 0.5 * tall;
    if (sh.lean) body.rotation.z = sh.lean;
    body.castShadow = true;
    grp.add(body);

    // pera/blob: bojo inferior mais largo
    if (sh.bottom) {
      const belly = new T.Mesh(new T.SphereGeometry(0.5, 24, 16), bodyMat);
      belly.scale.set(wide * (1 + sh.bottom), tall * 0.7, wide * (1 + sh.bottom));
      belly.position.y = 0.34 * tall;
      belly.castShadow = true;
      grp.add(belly);
    }

    // mistura de cor: ventre / dorsal / twotone
    const bellyCol = g.blend === 'twotone' || g.blend === 'belly2' || g.blend === 'gradient'
      ? col2.clone() : col.clone().lerp(new T.Color('#ffffff'), 0.45);
    if (g.pattern === 'belly' || g.blend === 'belly2' || g.blend === 'twotone' || g.blend === 'gradient') {
      const belly = new T.Mesh(new T.SphereGeometry(0.5, 22, 16), material(bellyCol, g.finish));
      belly.scale.set(0.64 * wide, 0.66 * tall, 0.52 * wide);
      belly.position.set(0, 0.42 * tall, 0.2 * wide);
      grp.add(belly);
    }
    if (g.blend === 'dorsal') {
      const dorsal = new T.Mesh(new T.SphereGeometry(0.5, 22, 16), material(col2, g.finish));
      dorsal.scale.set(0.5 * wide, 0.7 * tall, 0.5 * wide);
      dorsal.position.set(0, 0.6 * tall, -0.18 * wide);
      grp.add(dorsal);
    }

    // padrão: pintas / listras
    if (g.pattern === 'spots') {
      const spotMat = material(col.clone().multiplyScalar(0.65), g.finish);
      for (let i = 0; i < 7; i++) {
        const s = new T.Mesh(new T.SphereGeometry(G3.rng(0.05, 0.09), 10, 8), spotMat);
        const a = Math.random() * 7, h = G3.rng(0.3, 0.85) * tall;
        const r = 0.5 * wide * Math.sin((h / tall) * Math.PI * 0.9);
        s.position.set(Math.cos(a) * r, h, Math.sin(a) * r);
        grp.add(s);
      }
    } else if (g.pattern === 'stripes') {
      const stripeMat = material(col.clone().multiplyScalar(0.62), g.finish);
      for (let i = 0; i < 3; i++) {
        const ring = new T.Mesh(new T.TorusGeometry(0.5 * wide * 0.92, 0.045, 8, 28), stripeMat);
        ring.rotation.x = Math.PI / 2;
        ring.position.y = (0.32 + i * 0.22) * tall;
        ring.scale.set(1, 1, 0.7 + 0.3 * Math.sin(i));
        grp.add(ring);
      }
    }

    // olhos
    const eyeMat = new T.MeshStandardMaterial({ color: 0xffffff, roughness: 0.25 });
    const pupilEmit = g.emit === 'eyes';
    const pupMat = new T.MeshStandardMaterial({
      color: pupilEmit ? sig : new T.Color(0x16121b),
      emissive: pupilEmit ? sig : new T.Color(0x000000),
      emissiveIntensity: pupilEmit ? 0.9 : 0, roughness: 0.2,
    });
    const eyeR = g.eyes === 'big' ? 0.17 : g.eyes === 'small' ? 0.1 : 0.135;
    const eyePositions = g.eyes === 'one' ? [[0, 0.66 * tall]]
      : g.eyes === 'three' ? [[-0.17, 0.6 * tall], [0.17, 0.6 * tall], [0, 0.78 * tall]]
      : [[-0.17, 0.62 * tall], [0.17, 0.62 * tall]];
    eyePositions.forEach(([sx, sy]) => {
      const e = new T.Mesh(new T.SphereGeometry(eyeR, 16, 12), eyeMat);
      e.position.set(sx, sy, 0.34 * wide); grp.add(e);
      const pr = eyeR * 0.46;
      const p = new T.Mesh(new T.SphereGeometry(pr, 12, 10), pupMat);
      p.scale.set(g.pupil === 'vertical' ? 0.55 : 1, g.pupil === 'vertical' ? 1.4 : 1, 1);
      p.position.set(sx, sy, 0.34 * wide + eyeR * 0.7); grp.add(p);
    });

    // boca
    if (g.mouth === 'beak') {
      const beak = new T.Mesh(new T.ConeGeometry(0.12, 0.24, 8), material(new T.Color(g.signal).lerp(col, 0.3), 'satin'));
      beak.rotation.x = Math.PI / 2; beak.position.set(0, 0.5 * tall, 0.45 * wide); grp.add(beak);
    } else if (g.mouth === 'tusks') {
      const tuskMat = new T.MeshStandardMaterial({ color: 0xefe7d0, roughness: 0.4 });
      for (const sx of [-1, 1]) {
        const tk = new T.Mesh(new T.ConeGeometry(0.04, 0.22, 6), tuskMat);
        tk.position.set(0.1 * sx, 0.4 * tall, 0.4 * wide); tk.rotation.x = Math.PI; grp.add(tk);
      }
    }

    // pernas / pés
    const footMat = material(col.clone().multiplyScalar(0.7), 'satin');
    if (g.legs === 'stubby') {
      for (const sx of [-1, 1]) {
        const f = new T.Mesh(new T.SphereGeometry(0.17, 12, 10), footMat);
        f.scale.set(1, 0.5, 1.25); f.position.set(0.22 * sx, 0.07, 0.06); f.castShadow = true; grp.add(f);
      }
    } else if (g.legs === 'tall') {
      for (const sx of [-1, 1]) {
        const l = new T.Mesh(new T.CylinderGeometry(0.07, 0.07, 0.4, 8), footMat);
        l.position.set(0.2 * sx, 0.2, 0.04); l.castShadow = true; grp.add(l);
        const f = new T.Mesh(new T.SphereGeometry(0.12, 10, 8), footMat);
        f.scale.set(1, 0.5, 1.3); f.position.set(0.2 * sx, 0.04, 0.1); grp.add(f);
      }
    }

    // braços
    if (g.arms === 'stubby') {
      for (const sx of [-1, 1]) {
        const a = new T.Mesh(new T.SphereGeometry(0.12, 10, 8), bodyMat);
        a.scale.set(1, 1.5, 1); a.position.set(0.5 * wide * sx, 0.45 * tall, 0.05); grp.add(a);
      }
    } else if (g.arms === 'claws') {
      for (const sx of [-1, 1]) {
        const a = new T.Mesh(new T.CylinderGeometry(0.05, 0.08, 0.3, 7), bodyMat);
        a.position.set(0.52 * wide * sx, 0.45 * tall, 0.05); a.rotation.z = sx * 0.5; grp.add(a);
        for (let k = -1; k <= 1; k++) {
          const c = new T.Mesh(new T.ConeGeometry(0.03, 0.12, 5), footMat);
          c.position.set(0.6 * wide * sx, 0.32 * tall, 0.05 + k * 0.06); c.rotation.x = -Math.PI / 2; grp.add(c);
        }
      }
    } else if (g.arms === 'flippers') {
      for (const sx of [-1, 1]) {
        const a = new T.Mesh(new T.SphereGeometry(0.18, 10, 8), bodyMat);
        a.scale.set(0.4, 1.1, 0.8); a.position.set(0.52 * wide * sx, 0.4 * tall, 0.05); a.rotation.z = sx * 0.4; grp.add(a);
      }
    }

    // orelhas
    if (g.ears === 'pointy') {
      for (const sx of [-1, 1]) {
        const e = new T.Mesh(new T.ConeGeometry(0.12, 0.34, 10), bodyMat);
        e.position.set(0.24 * sx, 0.96 * tall, 0); e.rotation.z = sx * 0.35; e.castShadow = true; grp.add(e);
      }
    } else if (g.ears === 'round') {
      for (const sx of [-1, 1]) {
        const e = new T.Mesh(new T.SphereGeometry(0.13, 12, 10), bodyMat);
        e.scale.set(1, 1, 0.5); e.position.set(0.3 * sx, 0.92 * tall, 0); grp.add(e);
      }
    } else if (g.ears === 'tuft') {
      for (const sx of [-1, 1]) {
        const e = new T.Mesh(new T.ConeGeometry(0.08, 0.3, 7), material(col2, 'satin'));
        e.position.set(0.22 * sx, 1.0 * tall, 0); e.rotation.z = sx * 0.3; grp.add(e);
      }
    } else if (g.ears === 'fan') {
      for (const sx of [-1, 1]) {
        const e = new T.Mesh(new T.CircleGeometry(0.2, 10, 0, Math.PI), material(col2, 'satin'));
        e.material.side = T.DoubleSide;
        e.position.set(0.34 * sx, 0.85 * tall, 0); e.rotation.set(0, sx * 0.6, sx * -0.3); grp.add(e);
      }
    }

    // cabeça / antenas / chifres / crista / crina
    const tips = [];
    if (g.antennae === 'pair') {
      for (const sx of [-1, 1]) {
        const a = new T.Mesh(new T.CylinderGeometry(0.02, 0.02, 0.4, 6), footMat);
        a.position.set(0.1 * sx, 1.05 * tall, 0); grp.add(a);
        const tip = new T.Mesh(new T.SphereGeometry(0.08, 12, 10),
          new T.MeshStandardMaterial({ color: sig, emissive: sig, emissiveIntensity: 0.7, roughness: 0.3 }));
        tip.position.set(0.1 * sx, 1.26 * tall, 0); grp.add(tip); tips.push(tip);
      }
    } else if (g.antennae === 'horns') {
      for (const sx of [-1, 1]) {
        const h = new T.Mesh(new T.ConeGeometry(0.08, 0.4, 8), material(col.clone().multiplyScalar(0.6), 'satin'));
        h.position.set(0.18 * sx, 1.0 * tall, 0); h.rotation.z = sx * -0.4; h.castShadow = true; grp.add(h);
      }
    } else if (g.antennae === 'crest') {
      for (let i = -1; i <= 1; i++) {
        const c = new T.Mesh(new T.ConeGeometry(0.07, 0.22 + Math.abs(i === 0 ? 0.14 : 0), 6), material(col2, 'satin'));
        c.position.set(0, (0.95 + (i === 0 ? 0.08 : 0)) * tall, -i * 0.14); grp.add(c);
      }
    } else if (g.antennae === 'mane') {
      for (let i = 0; i < 8; i++) {
        const a = (i / 8) * Math.PI * 2;
        const m = new T.Mesh(new T.SphereGeometry(0.08, 8, 6), material(col2, 'satin'));
        m.position.set(Math.cos(a) * 0.3, 0.85 * tall, Math.sin(a) * 0.3 - 0.1); grp.add(m);
      }
    }

    // cauda
    if (g.tail !== 'none') {
      const tailMat = bodyMat;
      if (g.tail === 'short' || g.tail === 'long') {
        const len = g.tail === 'long' ? 0.5 : 0.28;
        const t = new T.Mesh(new T.ConeGeometry(0.12, len, 8), tailMat);
        t.position.set(0, 0.4 * tall, -0.5 * wide - len * 0.3); t.rotation.x = -1.1; grp.add(t);
      } else if (g.tail === 'curl') {
        const t = new T.Mesh(new T.TorusGeometry(0.16, 0.05, 8, 16, Math.PI * 1.4), tailMat);
        t.position.set(0, 0.5 * tall, -0.5 * wide); t.rotation.set(0.4, 0, 0); grp.add(t);
      } else if (g.tail === 'tuft') {
        const t = new T.Mesh(new T.SphereGeometry(0.14, 10, 8), material(col2, 'satin'));
        t.position.set(0, 0.45 * tall, -0.52 * wide); grp.add(t);
      }
    }

    // ornamento cultural
    if (g.ornament === 'headband') {
      const band = new T.Mesh(new T.TorusGeometry(0.42 * wide, 0.04, 8, 24), material(new T.Color(g.signal), 'satin'));
      band.rotation.x = Math.PI / 2; band.position.y = 0.78 * tall; grp.add(band);
    } else if (g.ornament === 'facepaint') {
      const p = new T.Mesh(new T.SphereGeometry(0.5, 16, 12), new T.MeshStandardMaterial({ color: sig, roughness: 0.5, transparent: true, opacity: 0.85 }));
      p.scale.set(0.55 * wide, 0.28 * tall, 0.5 * wide); p.position.set(0, 0.52 * tall, 0.18 * wide); grp.add(p);
    } else if (g.ornament === 'crest') {
      for (let i = 0; i < 3; i++) {
        const c = new T.Mesh(new T.ConeGeometry(0.06, 0.2, 6), material(sig, 'satin'));
        c.position.set(0, (0.92 - i * 0.05) * tall, -0.1 - i * 0.14); grp.add(c);
      }
    } else if (g.ornament === 'religious') {
      const sym = new T.Mesh(new T.OctahedronGeometry(0.1, 0),
        new T.MeshStandardMaterial({ color: G3.tok.base.heart, emissive: G3.tok.base.heart, emissiveIntensity: 0.8, roughness: 0.2 }));
      sym.position.y = 1.25 * tall; grp.add(sym); tips.push(sym);
    }

    // aura de sinal (glow): luz pontual fraca
    if (g.glow) {
      const aura = new T.PointLight(sig.getHex(), 0.5, 3, 2);
      aura.position.y = 0.6 * tall; grp.add(aura);
    }

    // escala global
    const s = parseFloat(g.size) || 1;
    grp.scale.setScalar(s);

    grp.userData = { body, baseTall: tall, wide, mat: bodyMat, tips, genome: g };
    return grp;
  }

  // ---- genoma aleatório plausível (pesos espelham lib.js cRand) -----------
  function randomGenome() {
    const P = G3.pick;
    return {
      shape: P(PARAMS.shape),
      color: P(G3.tok.body), color2: P(G3.tok.body),
      blend: P(['none', 'none', 'gradient', 'twotone', 'belly2', 'dorsal']),
      pattern: P(['none', 'belly', 'belly', 'spots', 'stripes']),
      size: P([0.82, 1, 1, 1.18]),
      eyes: P(['two', 'two', 'big', 'small', 'three', 'one']),
      pupil: P(['round', 'round', 'vertical']),
      mouth: P(['simple', 'simple', 'beak', 'tusks', 'none']),
      ears: P(['none', 'none', 'pointy', 'round', 'tuft', 'fan']),
      tail: P(['none', 'none', 'short', 'long', 'curl', 'tuft']),
      legs: P(['stubby', 'stubby', 'tall', 'none']),
      arms: P(['none', 'none', 'stubby', 'claws', 'flippers']),
      antennae: P(['none', 'none', 'pair', 'horns', 'crest', 'mane']),
      finish: P(['matte', 'matte', 'satin', 'metallic', 'iridescent']),
      emit: P(['none', 'none', 'none', 'eyes', 'antenna']),
      ornament: P(['none', 'none', 'headband', 'facepaint', 'crest', 'religious']),
      signal: P(G3.tok.signal),
      glow: Math.random() < 0.25,
    };
  }

  G3.Creature = { build, randomGenome, PARAMS, DEFAULTS };
})();

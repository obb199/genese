/* ============================================================
   GÊNESE — Biomas: flora & props vetoriais (tintáveis) + cenas
   Estilo: contorno escuro grosso, formas arredondadas — combina
   com as criaturas. Cada prop é um sprite isolado e exportável.
   Espelha AA §3 / DV §5.
   ============================================================ */
(function(){
  const OL = '#23201c', SW = 2.4;

  /* ---- prop registry: cada prop desenha com base-center em (w/2,h) ---- */
  const P = {};

  P.pinheiro = {w:60,h:92,draw:p=>`
    <rect x="26" y="58" width="8" height="30" rx="3" fill="${p.trunk}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M30 6 L48 40 H12 Z" fill="${p.foliage}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M30 24 L52 58 H8 Z" fill="${p.foliage2}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M30 22 L44 44 H16 Z" fill="${p.foliage}" opacity=".5"/>`};

  P.frondosa = {w:74,h:90,draw:p=>`
    <path d="M37 88 C34 70 34 58 37 50" stroke="${p.trunk}" stroke-width="9" stroke-linecap="round" fill="none"/>
    <path d="M37 60 C30 56 24 60 26 50 M37 58 C44 54 50 58 48 49" stroke="${p.trunk}" stroke-width="5" fill="none" stroke-linecap="round"/>
    <g fill="${p.foliage}" stroke="${OL}" stroke-width="${SW}">
      <circle cx="37" cy="30" r="22"/><circle cx="18" cy="40" r="14"/><circle cx="56" cy="40" r="14"/>
    </g>
    <circle cx="30" cy="24" r="9" fill="${p.foliage2}" opacity=".6"/>`};

  P.palmeira = {w:66,h:96,draw:p=>`
    <path d="M34 92 C30 64 30 44 38 26" stroke="${p.trunk}" stroke-width="8" fill="none" stroke-linecap="round"/>
    <g fill="${p.foliage}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round">
      <path d="M38 24 C20 14 8 18 4 28 C18 24 30 26 38 30Z"/>
      <path d="M38 24 C56 12 64 18 64 30 C50 24 44 26 38 30Z"/>
      <path d="M38 24 C30 4 18 2 10 6 C26 10 34 16 38 28Z"/>
      <path d="M38 24 C46 4 58 4 64 10 C48 12 42 16 38 28Z"/>
    </g>
    <circle cx="36" cy="28" r="3.4" fill="${p.accent}" stroke="${OL}" stroke-width="1.6"/>
    <circle cx="42" cy="30" r="3.4" fill="${p.accent}" stroke="${OL}" stroke-width="1.6"/>`};

  P.arvore_morta = {w:58,h:88,draw:p=>`
    <path d="M29 86 C28 64 28 50 29 30" stroke="${p.trunk}" stroke-width="8" fill="none" stroke-linecap="round"/>
    <path d="M29 44 C22 38 16 38 12 30 M29 50 C36 44 44 44 48 34 M29 34 C25 26 20 24 18 16 M29 36 C34 28 40 28 44 20" stroke="${p.trunk}" stroke-width="4.5" fill="none" stroke-linecap="round"/>`};

  P.cogumelos = {w:66,h:58,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}">
      <rect x="13" y="34" width="9" height="18" rx="4" fill="#E9E2D2"/>
      <path d="M5 34 C5 22 30 22 30 34Z" fill="${p.accent}"/>
      <rect x="40" y="40" width="8" height="14" rx="4" fill="#E9E2D2"/>
      <path d="M32 40 C32 30 56 30 56 40Z" fill="${p.foliage2||p.accent}"/>
    </g>
    <circle cx="13" cy="29" r="2.2" fill="#F3EEE0"/><circle cx="22" cy="31" r="1.8" fill="#F3EEE0"/>
    <circle cx="44" cy="36" r="1.8" fill="#F3EEE0"/>`};

  P.arvore_cristal = {w:60,h:92,draw:p=>`
    <rect x="26" y="60" width="8" height="28" rx="3" fill="${p.trunk}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round">
      <path d="M30 6 L46 34 L30 62 L14 34 Z" fill="${p.foliage}"/>
      <path d="M30 6 L30 62 M14 34 L46 34" stroke-width="1.4" opacity=".5"/>
      <path d="M16 30 L8 44 L18 52 Z" fill="${p.foliage2||p.accent}"/>
      <path d="M44 30 L52 44 L42 52 Z" fill="${p.foliage2||p.accent}"/>
    </g>`};

  P.pedra = {w:72,h:50,draw:p=>`
    <path d="M6 46 C2 30 14 16 30 16 C48 12 70 22 66 40 C64 48 50 48 36 48 C24 48 12 48 6 46Z" fill="${p.rock}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M22 22 L34 30 L26 40 M48 22 L44 34" stroke="${OL}" stroke-width="1.4" fill="none" opacity=".5"/>
    <path d="M14 22 C24 18 40 18 52 22" stroke="#fff" stroke-width="2" opacity=".18" fill="none"/>`};

  P.pico = {w:46,h:86,draw:p=>`
    <path d="M23 4 L40 84 H6 Z" fill="${p.rock}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M23 4 L23 84 M23 30 L34 50 M23 44 L12 60" stroke="${OL}" stroke-width="1.4" opacity=".45" fill="none"/>
    <path d="M23 4 L34 58 H23Z" fill="#fff" opacity=".1"/>`};

  P.rochas = {w:76,h:44,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round">
      <path d="M4 42 C2 30 12 24 22 26 C30 28 30 42 22 42Z" fill="${p.rock}"/>
      <path d="M26 42 C24 26 42 20 52 28 C58 34 54 42 44 42Z" fill="${p.rock}"/>
      <path d="M52 42 C52 32 64 30 70 36 C74 40 70 42 64 42Z" fill="${p.rock}"/>
    </g>`};

  P.cacto = {w:50,h:84,draw:p=>`
    <g fill="${p.foliage}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round">
      <rect x="20" y="20" width="12" height="62" rx="6"/>
      <path d="M20 46 C10 46 8 40 8 32 C8 28 12 28 12 32 L12 42 C12 44 16 44 20 44Z"/>
      <path d="M32 40 C42 40 44 34 44 26 C44 22 40 22 40 26 L40 36 C40 38 36 38 32 38Z"/>
    </g>
    <path d="M26 26 v50 M16 36 v6 M38 30 v6" stroke="${OL}" stroke-width="1.2" opacity=".4"/>
    <circle cx="26" cy="18" r="4" fill="${p.accent}" stroke="${OL}" stroke-width="1.6"/>`};

  P.junco = {w:42,h:72,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}" fill="none" stroke-linecap="round">
      <path d="M14 70 C14 44 12 30 14 18"/><path d="M22 70 C22 40 22 26 22 14"/><path d="M30 70 C30 46 32 34 30 22"/>
    </g>
    <rect x="11" y="10" width="6" height="14" rx="3" fill="${p.trunk}" stroke="${OL}" stroke-width="1.8"/>
    <rect x="27" y="14" width="6" height="13" rx="3" fill="${p.trunk}" stroke="${OL}" stroke-width="1.8"/>`};

  P.samambaia = {w:66,h:48,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}" fill="${p.foliage}" stroke-linejoin="round">
      <path d="M33 46 C12 46 6 30 10 18 C20 24 30 32 33 46Z"/>
      <path d="M33 46 C54 46 60 30 56 18 C46 24 36 32 33 46Z"/>
      <path d="M33 46 C30 28 33 14 33 8 C36 18 38 32 33 46Z" fill="${p.foliage2||p.foliage}"/>
    </g>`};

  P.flor = {w:38,h:58,draw:p=>`
    <path d="M19 56 C19 42 18 34 19 28" stroke="${p.foliage}" stroke-width="3.5" fill="none" stroke-linecap="round"/>
    <path d="M19 42 C12 40 9 44 10 38 M19 46 C26 44 29 48 28 42" stroke="${p.foliage}" stroke-width="3" fill="none" stroke-linecap="round"/>
    <g stroke="${OL}" stroke-width="${SW}">
      ${[0,72,144,216,288].map(a=>`<ellipse cx="19" cy="14" rx="5" ry="9" fill="${p.accent}" transform="rotate(${a} 19 22)"/>`).join('')}
      <circle cx="19" cy="22" r="5" fill="${p.foliage2||'#E0C46A'}"/>
    </g>`};

  P.tufo = {w:50,h:36,draw:p=>`
    <g stroke="${p.foliage}" stroke-width="3" fill="none" stroke-linecap="round">
      <path d="M10 34 C10 22 8 16 6 10"/><path d="M18 34 C18 18 18 12 16 6"/><path d="M26 34 C26 20 28 14 30 6"/>
      <path d="M34 34 C34 22 36 16 40 10"/><path d="M42 34 C42 24 44 20 46 14"/>
    </g>`};

  P.cristal = {w:54,h:76,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round">
      <path d="M27 4 L40 36 L30 72 L18 36 Z" fill="${p.accent}"/>
      <path d="M14 30 L22 50 L10 64 L4 44 Z" fill="${p.foliage||p.accent}"/>
      <path d="M40 28 L50 46 L44 66 L34 48 Z" fill="${p.foliage2||p.accent}"/>
    </g>
    <path d="M27 4 L27 72 M18 36 L40 36" stroke="#fff" stroke-width="1.2" opacity=".3"/>`};

  P.gelo = {w:50,h:80,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round" opacity=".96">
      <path d="M25 4 L37 40 L27 76 L15 40 Z" fill="${p.accent}"/>
      <path d="M12 34 L20 54 L10 66 L5 46 Z" fill="${p.foliage||p.accent}"/>
    </g>
    <path d="M25 8 L25 70 M16 40 L34 40" stroke="#fff" stroke-width="1.4" opacity=".45"/>
    <path d="M30 18 L34 24" stroke="#fff" stroke-width="2" opacity=".5"/>`};

  P.coral = {w:66,h:50,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}" fill="none" stroke-linecap="round">
      <path d="M14 48 C14 30 8 24 10 14 M14 30 C8 28 6 22 8 18 M14 34 C20 30 22 24 20 20" stroke="${p.accent}" stroke-width="4"/>
      <path d="M44 48 C44 34 40 26 44 16 C48 24 50 36 44 48Z" fill="${p.foliage}"/>
    </g>
    <circle cx="54" cy="34" r="6" fill="${p.foliage2||p.accent}" stroke="${OL}" stroke-width="${SW}"/>
    <circle cx="54" cy="34" r="2" fill="${OL}" opacity=".4"/>`};

  /* ---- biomas: paleta + props + cores de cena ---- */
  const BIOMES = {
    pradaria:{nm:'Pradaria / Savana', sky:['#A9B36A','#7C8442'], ground:'#7E8A3E', ground2:'#65702F',
      pal:{trunk:'#6E5A3A',foliage:'#7BA042',foliage2:'#A7B95A',rock:'#9A8F76',accent:'#E0C46A'},
      props:['frondosa','pedra','tufo','flor','rochas','cacto']},
    floresta:{nm:'Floresta', sky:['#2C5247','#16332C'], ground:'#274d40', ground2:'#1b3a30',
      pal:{trunk:'#5A4630',foliage:'#2F6F62',foliage2:'#56A06A',rock:'#5C6157',accent:'#C77FB0'},
      props:['pinheiro','frondosa','cogumelos','samambaia','pedra','tufo']},
    deserto:{nm:'Deserto', sky:['#D9B873','#B58A3E'], ground:'#C9A86A', ground2:'#A8843E',
      pal:{trunk:'#8A6A3A',foliage:'#7C9A4A',foliage2:'#9FB95A',rock:'#B89A60',accent:'#E2A24A'},
      props:['cacto','pico','rochas','pedra','tufo']},
    tundra:{nm:'Tundra / Gelo', sky:['#9FB4C4','#6E8496'], ground:'#C2D2DA', ground2:'#9FB4C4',
      pal:{trunk:'#6E7A82',foliage:'#7E96A4',foliage2:'#A9C0CC',rock:'#8A97A0',accent:'#BfeAf2'},
      props:['gelo','pinheiro','pedra','rochas','pico']},
    montanha:{nm:'Montanha / Rocha', sky:['#9A938A','#5C564B'], ground:'#8A8377', ground2:'#5C564B',
      pal:{trunk:'#6E5A3A',foliage:'#5C7A4A',foliage2:'#80A05A',rock:'#9A917F',accent:'#C9A86A'},
      props:['pico','pedra','rochas','pinheiro','arvore_morta']},
    agua:{nm:'Água / Costa', sky:['#5FA9B0','#2E6F8C'], ground:'#3E6B8C', ground2:'#2A5470',
      pal:{trunk:'#7A6A4A',foliage:'#3C8A8A',foliage2:'#5FC0B0',rock:'#7E8C8A',accent:'#E6D2A0'},
      props:['palmeira','coral','junco','pedra','rochas']},
    pantano:{nm:'Pântano / Úmido', sky:['#3C4A33','#222C1E'], ground:'#3a4a2e', ground2:'#2a3620',
      pal:{trunk:'#4A3E28',foliage:'#4A6E3A',foliage2:'#7AA050',rock:'#5C5A48',accent:'#B0A84A'},
      props:['arvore_morta','cogumelos','junco','samambaia','pedra']},
    vulcanico:{nm:'Vulcânico', sky:['#5A2E26','#2A1A18'], ground:'#3A2A28', ground2:'#241818',
      pal:{trunk:'#3A2A24',foliage:'#8E3D2A',foliage2:'#C0563A',rock:'#4A3A36',accent:'#E08A3A'},
      props:['arvore_morta','pico','rochas','cristal','pedra']}
  };

  function propSVG(key, pal, scale){
    const d=P[key]; if(!d) return '';
    const s=scale||1;
    return `<svg viewBox="0 0 ${d.w} ${d.h}" width="${d.w*s}" height="${d.h*s}" xmlns="http://www.w3.org/2000/svg" stroke-linecap="round">${d.draw(pal||{})}</svg>`;
  }
  function place(key, x, baseY, scale, pal){
    const d=P[key]; if(!d) return ''; const s=scale||1;
    return `<g transform="translate(${(x-d.w/2*s).toFixed(1)} ${(baseY-d.h*s).toFixed(1)}) scale(${s})">${d.draw(pal||{})}</g>`;
  }
  // cena-vinheta de bioma (vetor) — não é a arte pintada final, é o preview de produção
  function scene(key, w, h){
    const b=BIOMES[key]; if(!b) return ''; w=w||470; h=h||168;
    const gy=h-30; // linha do chão
    const items=[
      [b.props[0], w*0.20, gy, 1.0],
      [b.props[1]||b.props[0], w*0.80, gy-4, 0.78],
      [b.props[2]||b.props[0], w*0.50, gy+6, 0.62],
      [b.props[3]||b.props[1]||b.props[0], w*0.62, gy+2, 0.5],
      [b.props[4]||b.props[2]||b.props[0], w*0.36, gy+4, 0.46]
    ];
    return `<svg viewBox="0 0 ${w} ${h}" width="100%" preserveAspectRatio="xMidYMax meet" xmlns="http://www.w3.org/2000/svg" style="display:block">
      <defs><linearGradient id="sky_${key}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${b.sky[0]}"/><stop offset="1" stop-color="${b.sky[1]}"/></linearGradient></defs>
      <rect x="0" y="0" width="${w}" height="${h}" rx="12" fill="url(#sky_${key})"/>
      <path d="M0 ${gy} Q${w*0.3} ${gy-14} ${w*0.55} ${gy-4} T${w} ${gy-8} V${h} H0 Z" fill="${b.ground}"/>
      <path d="M0 ${gy+14} Q${w*0.4} ${gy+4} ${w} ${gy+12} V${h} H0 Z" fill="${b.ground2}"/>
      ${items.map(([k,x,y,s])=>place(k,x,y,s,b.pal)).join('')}
    </svg>`;
  }

  window.GeneseBiomes = { PROPS:P, BIOMES, propSVG, place, scene };
})();

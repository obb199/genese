/* ============================================================
   GÊNESE — Biomas fantásticos (extensão de biomes.js)
   Props vetoriais + paletas + cenas com efeitos (esporos, música,
   circuitos, chuva-para-cima, sinapses, arco-íris, etc).
   Mesmo estilo de contorno escuro das criaturas. Espelha AA §3.
   ============================================================ */
(function(){
  const G = window.GeneseBiomes; if(!G) return;
  const OL='#23201c', SW=2.4;
  const P = G.PROPS;
  const dots = (pts,c,r)=>pts.map(([x,y,rr])=>`<circle cx="${x}" cy="${y}" r="${rr||r||2}" fill="${c}"/>`).join('');

  /* ---------------- PROPS ---------------- */

  /* Cogumelos gigantes */
  P.cogumelo_gigante = {w:84,h:128,draw:p=>`
    <rect x="36" y="54" width="14" height="72" rx="7" fill="${p.trunk}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M40 70 q-7 16 -3 50 M47 76 q5 18 1 48" stroke="${OL}" stroke-width="1.3" fill="none" opacity=".3"/>
    <ellipse cx="43" cy="62" rx="20" ry="5" fill="${p.trunk}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M6 54 C6 18 80 18 80 54 C80 64 60 66 43 66 C26 66 6 64 6 54Z" fill="${p.foliage}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M12 50 C20 32 66 32 74 50" stroke="${p.foliage2||p.foliage}" stroke-width="5" fill="none" opacity=".5"/>
    <g fill="${p.accent}">${dots([[22,44,3],[40,36,3.4],[58,44,3],[31,52,2.4],[50,52,2.4],[66,50,2.2]])}</g>`};

  P.cogumelo_andante = {w:78,h:104,draw:p=>`
    <g fill="${p.trunk}" stroke="${OL}" stroke-width="${SW}">
      <rect x="28" y="90" width="8" height="14" rx="4"/><rect x="44" y="88" width="8" height="16" rx="4"/>
    </g>
    <rect x="30" y="52" width="18" height="44" rx="9" fill="${p.trunk}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 54 C8 24 70 24 70 54 C70 64 52 66 39 66 C26 66 8 64 8 54Z" fill="${p.foliage}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <ellipse cx="39" cy="46" rx="22" ry="5" fill="${p.foliage2||p.foliage}" opacity=".45"/>
    <g fill="${p.accent}">${dots([[24,44,2.8],[39,36,3.2],[54,44,2.8]])}</g>
    <circle cx="34" cy="74" r="2.6" fill="${OL}"/><circle cx="45" cy="74" r="2.6" fill="${OL}"/>`};

  P.micelio = {w:70,h:38,draw:p=>`
    <g stroke="${p.accent}" stroke-width="2.4" fill="none" stroke-linecap="round" opacity=".9">
      <path d="M4 34 C18 30 22 18 34 16 C46 14 50 26 66 22"/>
      <path d="M34 16 C32 8 38 6 40 2 M22 24 C16 22 12 26 8 24 M50 24 C56 20 60 24 64 20"/>
    </g>
    <g fill="${p.accent}">${dots([[34,16,3],[8,24,2.2],[64,20,2.2],[40,2,2.4],[50,24,2]])}</g>`};

  /* Mar de cristais */
  P.cristal_planta = {w:56,h:88,draw:p=>`
    <path d="M28 86 C26 64 27 50 28 38" stroke="${p.trunk||p.rock}" stroke-width="4" fill="none" stroke-linecap="round"/>
    <g stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round">
      <path d="M28 40 L40 14 L34 44 Z" fill="${p.foliage}"/>
      <path d="M28 50 L14 28 L24 54 Z" fill="${p.foliage2||p.foliage}"/>
      <path d="M28 30 L30 4 L36 30 Z" fill="${p.accent}"/>
      <path d="M28 60 L44 46 L36 64 Z" fill="${p.foliage2||p.accent}"/>
    </g>
    <path d="M30 8 L30 28 M30 44 L40 18" stroke="#fff" stroke-width="1" opacity=".4"/>`};

  P.cristal_grande = {w:72,h:96,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round">
      <path d="M36 4 L52 44 L40 92 L26 44 Z" fill="${p.foliage}"/>
      <path d="M14 34 L26 60 L12 84 L4 52 Z" fill="${p.foliage2||p.foliage}"/>
      <path d="M52 30 L68 56 L58 88 L44 54 Z" fill="${p.accent}"/>
    </g>
    <path d="M36 8 L36 88 M26 44 L52 44 M14 38 L24 60 M54 36 L62 58" stroke="#fff" stroke-width="1.1" opacity=".35"/>`};

  /* Floresta mecânica */
  P.arvore_metalica = {w:66,h:98,draw:p=>`
    <rect x="29" y="48" width="10" height="48" rx="2" fill="${p.trunk}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M34 72 L20 62 M34 62 L50 52" stroke="${p.trunk}" stroke-width="5" stroke-linecap="round"/>
    <g stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round">
      <path d="M10 38 L40 18 L58 26 L28 46 Z" fill="${p.foliage}"/>
    </g>
    <g stroke="${p.accent}" stroke-width="1" opacity=".55" fill="none">
      <path d="M19 33 L37 41 M28 27 L46 35 M22 41 L31 26 M34 37 L46 24"/>
    </g>
    <circle cx="34" cy="48" r="2.6" fill="${p.accent}"/>`};

  P.inseto_robo = {w:50,h:38,draw:p=>`
    <g stroke="${OL}" stroke-width="2" fill="none" stroke-linecap="round">
      <path d="M16 30 l-8 5 M16 24 l-9 -2 M30 30 l8 6 M30 24 l9 -3"/>
    </g>
    <ellipse cx="23" cy="24" rx="12" ry="8" fill="${p.foliage}" stroke="${OL}" stroke-width="${SW}"/>
    <circle cx="35" cy="20" r="6" fill="${p.trunk}" stroke="${OL}" stroke-width="${SW}"/>
    <circle cx="36" cy="19" r="2" fill="${p.accent}"/>
    <path d="M18 24 h10" stroke="${OL}" stroke-width="1.2" opacity=".4"/>
    <line x1="35" y1="16" x2="42" y2="12" stroke="${OL}" stroke-width="1.2"/><circle cx="43" cy="11" r="1.8" fill="${p.accent}"/>`};

  P.raiz_circuito = {w:74,h:32,draw:p=>`
    <g stroke="${p.accent}" stroke-width="2" fill="none" stroke-linecap="round">
      <path d="M2 16 H20 L26 24 H44 L50 12 H72"/>
      <path d="M20 16 V6 M44 24 V30 M50 12 V4"/>
    </g>
    <g fill="${p.accent}">${dots([[20,16,2.6],[26,24,2.2],[44,24,2.6],[50,12,2.4],[20,6,2],[50,4,2]])}</g>`};

  /* Bioma das nuvens */
  P.nuvem_solida = {w:96,h:54,draw:p=>`
    <path d="M10 40 C2 40 2 26 12 26 C12 14 30 12 34 22 C40 10 64 12 66 24 C80 18 92 28 86 40 C86 48 14 48 10 40Z" fill="${p.foliage2||p.foliage}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M16 36 C30 32 64 32 80 36" stroke="${p.accent}" stroke-width="1.6" fill="none" opacity=".4"/>`};

  P.arvore_nuvem = {w:60,h:90,draw:p=>`
    <path d="M30 88 C28 66 28 52 30 42" stroke="${p.trunk}" stroke-width="7" fill="none" stroke-linecap="round"/>
    <g fill="${p.foliage}" stroke="${OL}" stroke-width="${SW}">
      <circle cx="30" cy="30" r="18"/><circle cx="14" cy="38" r="11"/><circle cx="46" cy="38" r="11"/>
    </g>
    <g fill="${p.accent}" stroke="${OL}" stroke-width="1.4">
      <path d="M18 48 q-3 6 0 8 q3 -2 0 -8Z"/><path d="M42 50 q-3 6 0 8 q3 -2 0 -8Z"/><path d="M30 53 q-3 6 0 8 q3 -2 0 -8Z"/>
    </g>`};

  P.baleia_voadora = {w:116,h:64,draw:p=>`
    <path d="M8 36 C8 18 40 12 64 16 C92 20 104 28 104 36 C104 44 92 50 64 52 C40 54 8 54 8 36Z" fill="${p.foliage}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M100 30 C112 18 116 24 113 32 C116 34 112 42 104 40Z" fill="${p.foliage}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M40 50 C36 60 30 62 26 60 C30 56 32 52 36 50Z" fill="${p.foliage2||p.foliage}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M16 40 C30 46 60 46 80 42" stroke="${p.accent}" stroke-width="2" fill="none" opacity=".5"/>
    <path d="M14 30 C24 24 34 24 42 28" stroke="${OL}" stroke-width="1.6" fill="none" opacity=".4"/>
    <circle cx="26" cy="34" r="3.4" fill="${OL}"/><circle cx="27" cy="33" r="1" fill="#fff"/>`};

  /* Oceano vivo */
  P.escama = {w:84,h:56,draw:p=>`
    <path d="M42 6 C68 6 80 28 80 44 C80 52 60 54 42 54 C24 54 4 52 4 44 C4 28 16 6 42 6Z" fill="${p.rock}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M14 40 C28 30 56 30 70 40 M20 48 C34 42 50 42 64 48" stroke="${OL}" stroke-width="1.4" fill="none" opacity=".35"/>
    <path d="M42 12 C40 24 42 34 42 44" stroke="${p.accent}" stroke-width="2" opacity=".5" fill="none"/>
    <g fill="${p.foliage}">${dots([[30,38,2.4],[54,38,2.4],[42,46,2]])}</g>`};

  P.veia = {w:54,h:82,draw:p=>`
    <g stroke="${p.accent}" stroke-width="4" fill="none" stroke-linecap="round">
      <path d="M27 80 C24 60 30 50 26 32 C24 20 30 12 27 4"/>
      <path d="M27 56 C18 52 14 56 8 52 M26 40 C34 36 40 40 46 36 M27 24 C20 22 16 26 12 24"/>
    </g>
    <g fill="${p.accent}" opacity=".85">${dots([[27,4,3],[8,52,2.2],[46,36,2.4],[12,24,2]])}</g>`};

  P.poro_luminoso = {w:48,h:42,draw:p=>`
    <ellipse cx="24" cy="30" rx="20" ry="11" fill="${p.rock}" stroke="${OL}" stroke-width="${SW}"/>
    <ellipse cx="24" cy="28" rx="11" ry="6" fill="${p.foliage}" stroke="${OL}" stroke-width="2"/>
    <ellipse cx="24" cy="27" rx="4.5" ry="3" fill="${p.accent}"/>`};

  /* Selva bioluminescente */
  P.flor_bio = {w:44,h:66,draw:p=>`
    <path d="M22 64 C22 48 20 40 22 32" stroke="${p.foliage}" stroke-width="3.5" fill="none" stroke-linecap="round"/>
    <path d="M22 48 C14 46 10 50 11 44 M22 52 C30 50 34 54 33 48" stroke="${p.foliage}" stroke-width="3" fill="none" stroke-linecap="round"/>
    <g stroke="${OL}" stroke-width="${SW}">
      ${[0,60,120,180,240,300].map(a=>`<ellipse cx="22" cy="14" rx="5" ry="11" fill="${p.accent}" transform="rotate(${a} 22 26)"/>`).join('')}
      <circle cx="22" cy="26" r="6" fill="${p.foliage2||p.accent}"/>
    </g>
    <circle cx="22" cy="26" r="3" fill="#fff" opacity=".6"/>`};

  P.planta_luz = {w:52,h:80,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}">
      <path d="M16 78 C12 56 14 42 20 30 C26 42 24 56 24 78Z" fill="${p.foliage}"/>
      <path d="M30 78 C28 50 32 36 40 24 C46 36 40 58 40 78Z" fill="${p.foliage2||p.foliage}"/>
    </g>
    <g fill="${p.accent}">${dots([[20,30,3.2],[40,24,3.4],[19,46,2.2],[37,40,2.2]])}</g>`};

  /* Bioma neural */
  P.neuronio = {w:62,h:74,draw:p=>`
    <g stroke="${p.foliage}" stroke-width="2.4" fill="none" stroke-linecap="round">
      <path d="M31 44 C20 40 16 30 10 28 M31 44 C42 40 46 30 52 26 M31 44 C28 32 30 22 28 12 M31 50 C24 58 22 64 18 70 M31 50 C38 58 40 64 44 70"/>
    </g>
    <circle cx="31" cy="46" r="13" fill="${p.foliage2||p.foliage}" stroke="${OL}" stroke-width="${SW}"/>
    <circle cx="31" cy="46" r="5" fill="${p.accent}"/>
    <g fill="${p.accent}">${dots([[10,28,2.2],[52,26,2.2],[28,12,2.2],[18,70,2],[44,70,2]])}</g>`};

  P.planta_neural = {w:54,h:84,draw:p=>`
    <path d="M27 82 C25 60 28 46 27 30" stroke="${p.foliage}" stroke-width="3.4" fill="none" stroke-linecap="round"/>
    <g stroke="${p.foliage}" stroke-width="2.2" fill="none" stroke-linecap="round">
      <path d="M27 58 C18 54 14 46 10 44 M27 48 C36 44 40 38 44 34 M27 38 C20 34 16 30 14 24"/>
    </g>
    <g fill="${p.accent}" stroke="${OL}" stroke-width="1.6">
      <circle cx="27" cy="26" r="6"/><circle cx="10" cy="44" r="4"/><circle cx="44" cy="34" r="4.4"/><circle cx="14" cy="24" r="3.6"/>
    </g>`};

  /* Floresta de vidro */
  P.arvore_vidro = {w:62,h:96,draw:p=>`
    <rect x="27" y="56" width="9" height="38" rx="3" fill="${p.foliage}" fill-opacity=".35" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round" fill="${p.foliage}" fill-opacity=".4">
      <path d="M31 6 L50 30 L40 58 L22 58 L12 30 Z"/>
    </g>
    <g stroke="#fff" stroke-width="1.2" opacity=".55" fill="none">
      <path d="M31 6 L31 58 M12 30 L50 30 M22 58 L31 30 L40 58"/>
    </g>
    <path d="M31 6 L40 30 L31 44Z" fill="${p.accent}" opacity=".3"/>`};

  P.prisma = {w:50,h:62,draw:p=>`
    <path d="M25 6 L44 56 H6 Z" fill="${p.foliage}" fill-opacity=".4" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M25 6 L25 56" stroke="#fff" stroke-width="1.2" opacity=".5"/>
    <g stroke-width="2.4" fill="none" opacity=".85" stroke-linecap="round">
      <path d="M28 22 L48 16" stroke="${p.accent}"/>
      <path d="M30 26 L48 24" stroke="#E0C46A"/>
      <path d="M30 30 L48 32" stroke="#5FA0B0"/>
    </g>
    <path d="M6 30 L24 24" stroke="#fff" stroke-width="2" opacity=".5"/>`};

  /* Bioma fractal */
  P.planta_fractal = {w:66,h:86,draw:p=>{
    const br=(x,y,len,ang,depth)=>{
      if(depth<=0) return '';
      const x2=x+len*Math.sin(ang), y2=y-len*Math.cos(ang);
      return `<line x1="${x.toFixed(1)}" y1="${y.toFixed(1)}" x2="${x2.toFixed(1)}" y2="${y2.toFixed(1)}" stroke="${depth>1?p.foliage:p.accent}" stroke-width="${depth}" stroke-linecap="round"/>`
        + br(x2,y2,len*0.72,ang-0.55,depth-1) + br(x2,y2,len*0.72,ang+0.55,depth-1);
    };
    return `<g>${br(33,84,22,0,4)}</g>`;
  }};

  P.montanha_fractal = {w:92,h:82,draw:p=>`
    <path d="M46 6 L84 78 H8 Z" fill="${p.rock}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M30 42 L44 78 H16 Z" fill="${p.rock}" stroke="${OL}" stroke-width="1.8" stroke-linejoin="round"/>
    <path d="M62 40 L76 78 H48 Z" fill="${p.rock}" stroke="${OL}" stroke-width="1.8" stroke-linejoin="round"/>
    <path d="M38 60 L46 78 H30 Z" fill="${p.foliage}" stroke="${OL}" stroke-width="1.4"/>
    <path d="M46 6 L54 30 L46 40 L40 30 Z" fill="#fff" opacity=".18"/>
    <path d="M46 6 L46 40 M30 42 L30 60" stroke="${OL}" stroke-width="1" opacity=".3"/>`};

  /* Ecossistema de silício */
  P.arvore_silicio = {w:64,h:96,draw:p=>`
    <path d="M30 94 L30 50 M30 66 L18 56 M30 60 L44 50" stroke="${p.trunk}" stroke-width="6" fill="none" stroke-linecap="round"/>
    <g stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round">
      <path d="M30 6 L42 30 L30 52 L18 30 Z" fill="${p.foliage}"/>
      <path d="M14 32 L22 48 L12 60 L6 44 Z" fill="${p.foliage2||p.foliage}"/>
      <path d="M44 28 L54 44 L46 58 L36 44 Z" fill="${p.accent}"/>
    </g>
    <path d="M30 10 L30 50 M18 30 L42 30" stroke="#fff" stroke-width="1" opacity=".3"/>`};

  P.cristal_silicio = {w:66,h:62,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round">
      <path d="M22 14 L32 36 L24 58 L14 36 Z" fill="${p.foliage}"/>
      <path d="M40 24 L50 42 L44 60 L32 42 Z" fill="${p.accent}"/>
      <path d="M50 30 L60 44 L54 58 L46 44 Z" fill="${p.foliage2||p.foliage}"/>
      <path d="M8 34 L16 48 L10 60 L4 46 Z" fill="${p.foliage2||p.foliage}"/>
    </g>
    <g fill="#fff" opacity=".3">${dots([[22,18,1.4],[40,28,1.4],[50,34,1.2]])}</g>`};

  /* ---------------- BIOMAS ---------------- */
  Object.assign(G.BIOMES, {
    cogumelos_gigantes:{nm:'Floresta de Cogumelos Gigantes', sky:['#2A2342','#140E22'], ground:'#2C2440', ground2:'#1E1830', night:true,
      pal:{trunk:'#C7BEDE',foliage:'#8C5BAA',foliage2:'#B274D4',rock:'#5A5270',accent:'#7DF0C8'},
      props:['cogumelo_gigante','cogumelo_andante','cogumelos','micelio','junco'], fx:['stars','spores']},
    cristais_mar:{nm:'Mar de Cristais', sky:['#86B8CC','#3E6F9A'], ground:'#5E86A4', ground2:'#456C8C',
      pal:{trunk:'#9FC4D6',foliage:'#5FC6D8',foliage2:'#9AD8E6',rock:'#7C9AB0',accent:'#E6A8D8'},
      props:['cristal_grande','cristal_planta','cristal','gelo','cristal_silicio'], fx:['music']},
    floresta_mecanica:{nm:'Floresta Mecânica', sky:['#3A4A54','#1C272E'], ground:'#2E3A40', ground2:'#222C31',
      pal:{trunk:'#9AA6AE',foliage:'#3FA89A',foliage2:'#5FC0B0',rock:'#646E74',accent:'#E0C46A'},
      props:['arvore_metalica','inseto_robo','raiz_circuito','arvore_metalica','pedra'], fx:['circuit']},
    nuvens:{nm:'Bioma das Nuvens', sky:['#CFE6F0','#8FB6D2'], ground:'#DCE8F0', ground2:'#BCCCDC',
      pal:{trunk:'#9AA6AE',foliage:'#A7D8C0',foliage2:'#CFE6DA',rock:'#C7D4DE',accent:'#7FB6E0'},
      props:['arvore_nuvem','baleia_voadora','nuvem_solida','tufo','arvore_nuvem'], fx:['rain_up']},
    oceano_vivo:{nm:'Oceano Vivo', sky:['#1E4A5A','#0C2630'], ground:'#244A52', ground2:'#193238', night:true,
      pal:{trunk:'#7A8C90',foliage:'#3C8A8A',foliage2:'#5FC0B0',rock:'#3E5A60',accent:'#C0563A'},
      props:['escama','veia','poro_luminoso','coral','escama'], fx:['pulse']},
    bioluminescente:{nm:'Selva Bioluminescente', sky:['#0C1A18','#04100C'], ground:'#0E2620', ground2:'#0A1A16', night:true,
      pal:{trunk:'#2A4A40',foliage:'#1E6A52',foliage2:'#39C08A',rock:'#243A36',accent:'#5FE0C2'},
      props:['planta_luz','flor_bio','cogumelos','samambaia','flor_bio'], fx:['glow','spores']},
    neural:{nm:'Bioma Neural', sky:['#2A2440','#120E20'], ground:'#241E38', ground2:'#19142A', night:true,
      pal:{trunk:'#8C7AB0',foliage:'#8C5BAA',foliage2:'#B274D4',rock:'#4A4264',accent:'#E6A8F0'},
      props:['neuronio','planta_neural','neuronio','planta_neural','tufo'], fx:['synapse']},
    vidro:{nm:'Floresta de Vidro', sky:['#F2CCA8','#C99AD0'], ground:'#D8C4C0', ground2:'#BCA8B0',
      pal:{trunk:'#BFE0E6',foliage:'#9AD0DA',foliage2:'#CFEAEE',rock:'#B0A8B6',accent:'#E0A8C8'},
      props:['arvore_vidro','prisma','arvore_vidro','cristal','prisma'], fx:['rainbow']},
    fractal:{nm:'Bioma Fractal', sky:['#3A3052','#1A1430'], ground:'#2C2444', ground2:'#1E1834',
      pal:{trunk:'#6E5A8A',foliage:'#5FA0B0',foliage2:'#8C5BAA',rock:'#5A5274',accent:'#E0C46A'},
      props:['montanha_fractal','planta_fractal','planta_fractal','montanha_fractal','planta_fractal'], fx:[]},
    silicio:{nm:'Ecossistema de Silício', sky:['#6A5C7E','#2C2236'], ground:'#3E3448', ground2:'#2E2638',
      pal:{trunk:'#8A8296',foliage:'#A89AC0',foliage2:'#C4B8DA',rock:'#6A6276',accent:'#7DD8E0'},
      props:['arvore_silicio','cristal_silicio','cristal','arvore_silicio','cristal_grande'], fx:['liquid']}
  });

  /* ---------------- CENA COM EFEITOS ---------------- */
  const baseScene = G.scene;
  function mkRand(seed){ let s=seed||1; return ()=>{ s=(s*9301+49297)%233280; return s/233280; }; }

  G.scene = function(key, w, h){
    const b = G.BIOMES[key]; if(!b) return '';
    if(!b.fx && !b.night) return baseScene(key, w, h);   // biomas originais inalterados
    w=w||470; h=h||168; const gy=h-30;
    const items=[
      [b.props[0], w*0.20, gy, 1.0],
      [b.props[1]||b.props[0], w*0.80, gy-4, 0.78],
      [b.props[2]||b.props[0], w*0.50, gy+6, 0.62],
      [b.props[3]||b.props[1]||b.props[0], w*0.62, gy+2, 0.5],
      [b.props[4]||b.props[2]||b.props[0], w*0.36, gy+4, 0.46]
    ];
    const fx=b.fx||[]; const rand=mkRand(key.length*131+17);
    let skyFx='', backFx='', frontFx='';

    if(b.night || fx.includes('stars')){
      for(let i=0;i<26;i++){ const x=rand()*w, y=rand()*(gy-24)+4, r=rand()*1.4+0.5;
        skyFx+=`<circle cx="${x.toFixed(0)}" cy="${y.toFixed(0)}" r="${r.toFixed(1)}" fill="#fff" opacity="${(0.3+rand()*0.5).toFixed(2)}"/>`; }
    }
    if(fx.includes('rainbow')){
      const cols=['#E0708A','#E0A85A','#E8D45A','#6FC07A','#5FA0D8','#9C7AD8'];
      skyFx+=`<g fill="none" stroke-width="6" opacity=".42">${cols.map((c,i)=>`<path d="M-10 ${gy} Q ${w/2} ${(-h*0.45+i*9).toFixed(0)} ${w+10} ${gy}" stroke="${c}"/>`).join('')}</g>`;
    }
    if(fx.includes('glow')){
      backFx+=items.map(([k,x,y,s])=>`<ellipse cx="${x.toFixed(0)}" cy="${(y-22*s).toFixed(0)}" rx="${(40*s).toFixed(0)}" ry="${(34*s).toFixed(0)}" fill="url(#glow_${key})"/>`).join('');
    }
    if(fx.includes('circuit')){
      backFx+=`<g stroke="${b.pal.accent}" stroke-width="1.6" fill="none" opacity=".5" stroke-linecap="round"><path d="M0 ${gy+10} H${(w*0.28).toFixed(0)} L${(w*0.34).toFixed(0)} ${gy+18} H${(w*0.6).toFixed(0)} L${(w*0.66).toFixed(0)} ${gy+8} H${w}"/></g>`;
      backFx+=`<g fill="${b.pal.accent}">${[0.28,0.6,0.82].map(f=>`<circle cx="${(w*f).toFixed(0)}" cy="${gy+12}" r="2.4"/>`).join('')}</g>`;
    }
    if(fx.includes('liquid')){
      backFx+=`<g>${[[0.32,0.5],[0.72,0.4]].map(([f,sw2])=>`<ellipse cx="${(w*f).toFixed(0)}" cy="${gy+15}" rx="${(w*sw2*0.5).toFixed(0)}" ry="9" fill="${b.pal.accent}" opacity=".4"/><ellipse cx="${(w*f).toFixed(0)}" cy="${gy+13}" rx="${(w*sw2*0.4).toFixed(0)}" ry="6" fill="#fff" opacity=".12"/>`).join('')}</g>`;
    }
    if(fx.includes('synapse')){
      const nodes=items.map(([k,x,y,s])=>[x, y-30*s]);
      let lines=''; for(let i=0;i<nodes.length-1;i++){ lines+=`<line x1="${nodes[i][0].toFixed(0)}" y1="${nodes[i][1].toFixed(0)}" x2="${nodes[i+1][0].toFixed(0)}" y2="${nodes[i+1][1].toFixed(0)}" stroke="${b.pal.accent}" stroke-width="1.4" opacity=".45"/>`; }
      backFx+=`<g stroke-linecap="round">${lines}</g>`;
    }

    if(fx.includes('spores')){
      for(let i=0;i<22;i++){ const x=rand()*w, y=rand()*gy+8, r=rand()*1.8+0.8;
        frontFx+=`<circle cx="${x.toFixed(0)}" cy="${y.toFixed(0)}" r="${r.toFixed(1)}" fill="${b.pal.accent}" opacity="${(0.4+rand()*0.5).toFixed(2)}"/>`; }
    }
    if(fx.includes('rain_up')){
      let s=''; for(let i=0;i<22;i++){ const x=rand()*w, y=rand()*gy+12; s+=`<line x1="${x.toFixed(0)}" y1="${(y+8).toFixed(0)}" x2="${x.toFixed(0)}" y2="${y.toFixed(0)}" stroke="${b.pal.accent}" stroke-width="1.4" opacity=".5"/><path d="M${(x-2).toFixed(0)} ${(y+2).toFixed(0)} L${x.toFixed(0)} ${y.toFixed(0)} L${(x+2).toFixed(0)} ${(y+2).toFixed(0)}" stroke="${b.pal.accent}" stroke-width="1.2" fill="none" opacity=".5"/>`; }
      frontFx+=`<g stroke-linecap="round">${s}</g>`;
    }
    if(fx.includes('music')){
      frontFx+=`<g fill="none" stroke="${b.pal.accent}" stroke-linecap="round" opacity=".6">${[0,1,2].map(i=>`<path d="M${(w*0.2-4).toFixed(0)} ${(gy-52-i*11).toFixed(0)} q -12 ${(-8-i*4).toFixed(0)} -22 0" stroke-width="${(2-i*0.3).toFixed(1)}"/>`).join('')}</g>`;
      frontFx+=`<g fill="${b.pal.accent}" opacity=".65">${[[w*0.72,gy-66],[w*0.8,gy-84],[w*0.52,gy-90]].map(([x,y])=>`<circle cx="${x.toFixed(0)}" cy="${y.toFixed(0)}" r="3"/><rect x="${(x+2).toFixed(0)}" y="${(y-12).toFixed(0)}" width="1.6" height="12"/>`).join('')}</g>`;
    }
    if(fx.includes('pulse')){
      frontFx+=`<g fill="none" stroke="${b.pal.accent}" opacity=".4">${[10,18,26].map(r=>`<circle cx="${(w*0.5).toFixed(0)}" cy="${(gy-10).toFixed(0)}" r="${r}" stroke-width="${(3-r/14).toFixed(1)}"/>`).join('')}</g>`;
    }

    return `<svg viewBox="0 0 ${w} ${h}" width="100%" preserveAspectRatio="xMidYMax meet" xmlns="http://www.w3.org/2000/svg" style="display:block">
      <defs>
        <linearGradient id="sky_${key}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${b.sky[0]}"/><stop offset="1" stop-color="${b.sky[1]}"/></linearGradient>
        <radialGradient id="glow_${key}" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="${b.pal.accent}" stop-opacity=".55"/><stop offset="1" stop-color="${b.pal.accent}" stop-opacity="0"/></radialGradient>
      </defs>
      <rect x="0" y="0" width="${w}" height="${h}" rx="12" fill="url(#sky_${key})"/>
      ${skyFx}
      <path d="M0 ${gy} Q${w*0.3} ${gy-14} ${w*0.55} ${gy-4} T${w} ${gy-8} V${h} H0 Z" fill="${b.ground}"/>
      <path d="M0 ${gy+14} Q${w*0.4} ${gy+4} ${w} ${gy+12} V${h} H0 Z" fill="${b.ground2}"/>
      ${backFx}
      ${items.map(([k,x,y,s])=>G.place(k,x,y,s,b.pal)).join('')}
      ${frontFx}
    </svg>`;
  };
})();

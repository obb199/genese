/* ============================================================
   GÊNESE — Construções: Culto & Templos + Costa & Mar (expansão)
   + variante RUÍNAS + novas CULTURAS + montagem final dos estágios.
   Extensão de buildings.js + extra2/3/4. Vetores tintáveis.
   ============================================================ */
(function(){
  const G = window.GeneseBuildings; if(!G) return;
  const OL='#23201c', SW=2.6;
  const B = G.PARTS;
  const win=(x,y,w,h,p)=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>`;
  const door=(x,y,w,h,p)=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="2" fill="${p.dark||'#352a22'}" stroke="${OL}" stroke-width="2"/>`;

  /* ============================================================
     CULTO & TEMPLOS
     ============================================================ */
  B.mosteiro = {w:112,h:84,draw:p=>`
    <rect x="14" y="44" width="84" height="40" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 46 L34 26 L58 46 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${OL}" stroke-width="2" fill="${p.stone}">${[58,72,86].map(x=>`<path d="M${x} 44 V62 C${x} 54 ${x+10} 54 ${x+10} 62 V44Z" fill="${p.dark||'#3a342c'}"/>`).join('')}</g>
    <circle cx="34" cy="38" r="6" fill="${p.accent}" stroke="${OL}" stroke-width="1.8"/>
    <path d="M34 32 v12 M28 38 h12" stroke="${OL}" stroke-width="1.4"/>
    ${door(28,62,14,22,p)}
    <path d="M34 26 V18" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.cemiterio = {w:96,h:62,draw:p=>`
    <ellipse cx="48" cy="56" rx="44" ry="6" fill="${p.dark||'#2a2622'}" opacity=".3"/>
    ${[[16,0.8],[34,1],[54,0.9],[74,1],[88,0.7]].map(([x,s])=>`<g transform="translate(${x} ${56}) scale(${s})"><path d="M-9 0 V-22 C-9 -32 9 -32 9 -22 V0Z" fill="${p.stone}" stroke="${OL}" stroke-width="2"/><path d="M0 -28 v8 M-5 -24 h10" stroke="${OL}" stroke-width="1.6"/></g>`).join('')}
    <g stroke="${OL}" stroke-width="2" fill="${p.wall||'#9aa094'}"><rect x="60" y="44" width="18" height="10" rx="2"/></g>`};

  B.oraculo = {w:84,h:90,draw:p=>`
    <ellipse cx="42" cy="84" rx="36" ry="6" fill="${p.dark||'#2a2622'}" opacity=".3"/>
    <path d="M42 4 C30 4 22 20 22 40 L22 80 H62 L62 40 C62 20 54 4 42 4Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M30 80 C30 56 54 56 54 80 Z" fill="${p.glyph||'#7C4DBE'}" opacity=".4" stroke="${OL}" stroke-width="2"/>
    <g stroke="${p.accent}" stroke-width="1.6" fill="none" opacity=".8"><circle cx="42" cy="34" r="9"/><path d="M42 25 V43 M33 34 H51"/></g>
    <g fill="#fff" opacity=".7">${[[36,66],[48,60],[42,72]].map(([x,y])=>`<circle cx="${x}" cy="${y}" r="1.6"/>`).join('')}</g>`};

  B.labirinto = {w:90,h:72,draw:p=>`
    <rect x="8" y="14" width="74" height="54" rx="2" fill="${p.stone}" opacity=".25" stroke="${OL}" stroke-width="2"/>
    <g stroke="${p.stone}" stroke-width="4" fill="none" stroke-linecap="square">
      <path d="M18 60 V24 H64 V52 H30 V34 H52 V44"/>
      <path d="M72 60 V18 M8 14"/>
    </g>
    <circle cx="44" cy="42" r="3.4" fill="${p.accent}"/>`};

  B.jardim_meditacao = {w:96,h:56,draw:p=>`
    <rect x="8" y="24" width="80" height="30" rx="3" fill="${p.wall||'#D8CDB4'}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${OL}" stroke-width="1" opacity=".3" fill="none">${[28,34,40].map(r=>`<ellipse cx="30" cy="40" rx="${r-12}" ry="${(r-12)*0.5}"/>`).join('')}</g>
    <g stroke="${OL}" stroke-width="1" opacity=".25"><path d="M52 28 H84 M52 34 H84 M52 40 H84 M52 46 H84"/></g>
    <ellipse cx="30" cy="40" rx="5" ry="3" fill="${p.stone}" stroke="${OL}" stroke-width="1.6"/>
    <path d="M68 24 V14" stroke="${p.flame||'#3C6E4A'}" stroke-width="2" stroke-linecap="round"/><circle cx="68" cy="12" r="4" fill="${p.accent}" stroke="${OL}" stroke-width="1.4"/>`};

  B.camara_sacrificio = {w:88,h:74,draw:p=>`
    <path d="M10 72 L20 30 H68 L78 72 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M14 30 L44 8 L74 30 Z" fill="${p.dark||'#3a342c'}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="34" y="52" width="20" height="14" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>
    <path d="M44 52 C40 46 44 40 44 36 C46 40 50 46 44 52Z" fill="${p.flame||'#C0563A'}" stroke="${OL}" stroke-width="1.6"/>
    <g stroke="${p.glyph||'#C0563A'}" stroke-width="1.4" opacity=".7"><path d="M24 40 h6 M58 40 h6 M40 24 l4 -6 4 6"/></g>`};

  B.relicario = {w:62,h:84,draw:p=>`
    <rect x="16" y="44" width="30" height="40" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M12 44 L31 22 L50 44 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="24" y="54" width="14" height="22" rx="2" fill="${p.glyph||'#7C4DBE'}" opacity=".5" stroke="${OL}" stroke-width="2"/>
    <path d="M31 58 C28 62 28 68 31 72 C34 68 34 62 31 58Z" fill="${p.accent}"/>
    <circle cx="31" cy="36" r="5" fill="${p.accent}" stroke="${OL}" stroke-width="1.8"/>
    <path d="M31 22 V14" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>
    <g stroke="${p.accent}" stroke-width="1.4" opacity=".5"><path d="M50 44 l6 -2 M12 44 l-6 -2"/></g>`};

  B.escola_doutrina = {w:100,h:80,draw:p=>`
    <rect x="14" y="40" width="72" height="40" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 42 L50 18 L90 42 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${door(42,58,16,22,p)}${win(22,48,11,11,p)}${win(67,48,11,11,p)}
    <rect x="40" y="24" width="20" height="14" rx="2" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="2"/>
    <g stroke="${p.accent}" stroke-width="1.4" opacity=".8"><path d="M44 30 h12 M44 33 h9"/></g>
    <path d="M50 18 V10" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.mausoleu = {w:90,h:82,draw:p=>`
    <rect x="18" y="46" width="54" height="36" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M14 46 L45 22 L76 46 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${OL}" stroke-width="2" fill="${p.wall||'#C8BBA0'}">${[24,38,52,64].map(x=>`<rect x="${x}" y="50" width="6" height="32"/>`).join('')}</g>
    ${door(38,60,14,22,p)}
    <circle cx="45" cy="38" r="5" fill="${p.accent}" stroke="${OL}" stroke-width="1.8"/>
    <path d="M45 22 V14" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  /* ============================================================
     COSTA & MAR (expansão)
     ============================================================ */
  B.estaleiro = {w:112,h:74,draw:p=>`
    <g stroke="${p.wood}" stroke-width="4" stroke-linecap="round"><path d="M14 70 L30 30 M54 70 L40 30 M30 30 H40"/></g>
    <path d="M20 64 C26 44 60 44 66 64 Z" fill="${p.wall||'#8A6A3A'}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M24 56 H62" stroke="${OL}" stroke-width="1.4" opacity=".4"/>
    <path d="M43 56 V26" stroke="${p.wood}" stroke-width="3" stroke-linecap="round"/>
    <g stroke="${p.wood}" stroke-width="3" stroke-linecap="round"><path d="M78 70 V34 M78 40 L98 34"/></g>
    <path d="M78 34 L70 26" stroke="${p.wood}" stroke-width="2.4" stroke-linecap="round"/>
    <path d="M4 70 q10 -4 16 0 q10 -4 16 0" stroke="${p.accent}" stroke-width="1.6" fill="none" opacity=".5"/>`};

  B.armazem_portuario = {w:100,h:72,draw:p=>`
    <rect x="14" y="34" width="74" height="38" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 36 C10 22 92 22 92 36 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="34" y="46" width="30" height="26" rx="2" fill="${p.dark||'#3a2c20'}" stroke="${OL}" stroke-width="2"/>
    <path d="M49 46 V72" stroke="${p.wood}" stroke-width="1.6" opacity=".6"/>
    <g stroke="${OL}" stroke-width="1.6" fill="${p.wood}"><rect x="70" y="58" width="12" height="12"/><rect x="74" y="50" width="12" height="10"/></g>
    ${win(20,42,10,9,p)}`};

  B.taverna_maritima = {w:92,h:82,draw:p=>`
    <g stroke="${p.wood}" stroke-width="4" stroke-linecap="round"><path d="M18 80 V50 M74 80 V50"/></g>
    <rect x="16" y="36" width="60" height="40" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 38 L46 14 L82 38 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${door(38,52,16,24,p)}${win(24,46,11,11,p)}${win(57,46,11,11,p)}
    <circle cx="46" cy="26" r="6" fill="none" stroke="${p.accent}" stroke-width="2"/><path d="M46 20 V32 M40 26 H52 M43 22 l6 8 M49 22 l-6 8" stroke="${p.accent}" stroke-width="1.4"/>
    <path d="M6 80 q10 -4 16 0 q10 -4 16 0 q10 -4 16 0" stroke="${p.accent}" stroke-width="1.6" fill="none" opacity=".5"/>`};

  B.posto_pesca = {w:84,h:72,draw:p=>`
    <g stroke="${p.wood}" stroke-width="3.4" stroke-linecap="round"><path d="M16 70 V44 M64 70 V44"/></g>
    <rect x="12" y="30" width="56" height="16" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 30 L40 14 L72 30 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M68 44 L82 60" stroke="${p.wood}" stroke-width="2.4" stroke-linecap="round"/>
    <path d="M70 48 q8 2 10 10 q-8 -2 -10 -10Z" fill="none" stroke="${p.accent}" stroke-width="1.4"/>
    <g stroke="${OL}" stroke-width="1" opacity=".3"><path d="M20 60 L28 52 36 60 44 52 52 60 60 52" fill="none"/></g>
    <path d="M4 70 q9 -4 14 0 q9 -4 14 0" stroke="${p.accent}" stroke-width="1.6" fill="none" opacity=".5"/>`};

  B.vigia_costeira = {w:62,h:102,draw:p=>`
    <path d="M16 100 L22 40 H40 L46 100 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="12" y="26" width="38" height="16" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    ${[14,24,34].map(x=>`<rect x="${x}" y="18" width="7" height="8" fill="${p.wall}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <rect x="22" y="30" width="9" height="9" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="1.6"/>
    <path d="M22 60 H40 M21 78 H41" stroke="${OL}" stroke-width="1" opacity=".25"/>
    <g stroke="${p.accent}" stroke-width="2" opacity=".5"><path d="M50 32 l10 -3 M50 36 l10 3"/></g>`};

  B.mercado_peixe = {w:92,h:68,draw:p=>`
    <g stroke="${p.wood}" stroke-width="4" stroke-linecap="round"><path d="M16 66 V28 M76 66 V28"/></g>
    <path d="M8 28 H84 L84 20 Q46 10 8 20 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${[20,34,46,58,70].map(x=>`<path d="M${x} 19 V28" stroke="${OL}" stroke-width="1.2" opacity=".4"/>`).join('')}
    <rect x="18" y="42" width="56" height="9" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${OL}" stroke-width="1.4" fill="${p.accent}"><path d="M26 40 q6 -4 12 0 q-2 4 -6 4 q-4 0 -6 -4Z M34 40 l3 -3"/><path d="M46 40 q6 -4 12 0 q-2 4 -6 4 q-4 0 -6 -4Z"/></g>
    <path d="M18 51 V66 M74 51 V66" stroke="${p.wood}" stroke-width="2" opacity=".5"/>`};

  B.quarentena = {w:80,h:78,draw:p=>`
    <rect x="16" y="36" width="48" height="42" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M12 38 L40 16 L68 38 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${door(33,56,14,22,p)}
    <rect x="22" y="44" width="14" height="14" rx="2" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="2"/>
    <g stroke="${p.accent}" stroke-width="2"><path d="M22 44 l14 14 M36 44 l-14 14"/></g>
    <path d="M40 16 V8" stroke="${p.flame||'#C0563A'}" stroke-width="2.4" stroke-linecap="round"/>
    <path d="M40 9 L52 11 L40 14Z" fill="${p.flame||'#C0563A'}" stroke="${OL}" stroke-width="1.2"/>`};

  B.fortaleza_costeira = {w:118,h:86,draw:p=>`
    <rect x="20" y="40" width="78" height="46" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="8" y="28" width="24" height="58" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="86" y="28" width="24" height="58" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[8,19,86,97].map(x=>`<rect x="${x}" y="20" width="9" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    ${[24,40,56,72].map(x=>`<rect x="${x}" y="32" width="9" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <path d="M48 86 V58 C48 48 70 48 70 58 V86Z" fill="${p.dark||'#34302a'}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="14" y="44" width="9" height="13" rx="2" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="2"/>
    <rect x="91" y="44" width="9" height="13" rx="2" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="2"/>
    <path d="M13 20 V12 M101 20 V12" stroke="${p.accent}" stroke-width="2" stroke-linecap="round"/>
    <path d="M2 84 q10 -4 16 0" stroke="${p.accent}" stroke-width="1.6" fill="none" opacity=".5"/>`};

  B.cais_flutuante = {w:104,h:54,draw:p=>`
    <rect x="8" y="28" width="40" height="9" rx="3" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="54" y="32" width="40" height="9" rx="3" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M48 32 H54" stroke="${p.wood}" stroke-width="4"/>
    <g fill="${p.stone}" stroke="${OL}" stroke-width="1.6">${[14,30,60,80].map(x=>`<circle cx="${x}" cy="${x<50?42:46}" r="4"/>`).join('')}</g>
    <path d="M24 28 V16" stroke="${p.wood}" stroke-width="3" stroke-linecap="round"/><path d="M24 18 h10 v6 h-10z" fill="${p.accent}" stroke="${OL}" stroke-width="1.4"/>
    <path d="M2 48 q10 -4 16 0 q10 -4 16 0 q10 -4 16 0 q10 -4 16 0 q10 -4 16 0" stroke="${p.accent}" stroke-width="1.6" fill="none" opacity=".5"/>`};

  /* ============================================================
     RUÍNAS — gerador de variante destruída de qualquer estrutura
     ============================================================ */
  function ruinSVG(key, pal, scale){
    const d=B[key]; if(!d) return ''; const s=scale||1;
    const w=d.w, h=d.h;
    // máscara que "come" o topo e um canto + sobreposição de rachaduras e entulho
    const mid=h*0.42;
    const cracks=`<g stroke="${OL}" stroke-width="1.6" fill="none" opacity=".55" stroke-linecap="round">
        <path d="M${w*0.5} ${h*0.5} l${w*0.08} ${h*0.12} l-${w*0.05} ${h*0.1}"/>
        <path d="M${w*0.3} ${h*0.55} l${w*0.06} ${h*0.14}"/></g>`;
    const rubble=`<g stroke="${OL}" stroke-width="1.6">
        ${[0.2,0.5,0.8].map(f=>`<path d="M${(w*f-9).toFixed(0)} ${h-2} q9 -10 18 0Z" fill="${pal.stone||'#8A8377'}"/>`).join('')}
        ${[0.32,0.66].map(f=>`<rect x="${(w*f).toFixed(0)}" y="${(h-7).toFixed(0)}" width="9" height="6" fill="${pal.stone||'#8A8377'}"/>`).join('')}</g>`;
    return `<svg viewBox="0 0 ${w} ${h}" width="${(w*s).toFixed(0)}" height="${(h*s).toFixed(0)}" xmlns="http://www.w3.org/2000/svg" stroke-linecap="round">
      <defs><clipPath id="ruin_${key}">
        <path d="M0 ${mid} L${w*0.18} ${h*0.32} L${w*0.34} ${mid} L${w*0.5} ${h*0.28} L${w*0.62} ${mid} L${w*0.8} ${h*0.36} L${w} ${mid*0.9} L${w} ${h} L0 ${h} Z"/>
      </clipPath></defs>
      <g clip-path="url(#ruin_${key})" opacity=".92">${d.draw(pal||{})}</g>
      ${cracks}${rubble}
      <g opacity=".5" fill="${pal.foliage||'#6E8F4A'}">${[0.15,0.45,0.85].map(f=>`<path d="M${(w*f).toFixed(0)} ${h-3} q-2 -8 2 -12 q4 4 0 12Z"/>`).join('')}</g>
    </svg>`;
  }

  /* ============================================================
     NOVAS CULTURAS
     ============================================================ */
  Object.assign(G.CULTURES, {
    aquatica:    {nm:'Cultura Aquática',    wall:'#5E8C8C', roof:'#2E5A66', wood:'#3E5A52', stone:'#6E9298', accent:'#7FD8C8', flame:'#5FE0C2', flame2:'#A8F0DC', dark:'#22343a', glyph:'#2E7A78'},
    nomade:      {nm:'Cultura Nômade',      wall:'#C26A4A', roof:'#8A3E2E', wood:'#7A5638', stone:'#B89A60', accent:'#E8C24A', flame:'#E08A3A', flame2:'#F0C46A', dark:'#3a261c', glyph:'#8A3E2E'},
    subterranea: {nm:'Cultura Subterrânea', wall:'#5A5466', roof:'#3E3A4A', wood:'#4A4236', stone:'#6A6276', accent:'#7DF0C8', flame:'#7DF0C8', flame2:'#B6F0E0', dark:'#1f1c28', glyph:'#5FA0B0'},
    arcana:      {nm:'Cultura Arcana',      wall:'#5E4E86', roof:'#3E2E66', wood:'#4A3A6A', stone:'#6A5C94', accent:'#C9A0FF', flame:'#9C6BE0', flame2:'#D8B8FF', dark:'#241c38', glyph:'#C9A0FF'},
    imperial:    {nm:'Cultura Imperial',    wall:'#E0D6C0', roof:'#A8443A', wood:'#9A7A52', stone:'#D2C8AE', accent:'#C9A04A', flame:'#E08A3A', flame2:'#F0C46A', dark:'#4a4234', glyph:'#A8443A'},
    tecnologica: {nm:'Cultura Tecnológica', wall:'#7A8088', roof:'#4A5258', wood:'#5A4E42', stone:'#8A9098', accent:'#E0A030', flame:'#E0A030', flame2:'#F0D070', dark:'#2a2e32', glyph:'#3FA89A'}
  });

  /* ============================================================
     REGISTRO DOS ITENS NOS ESTÁGIOS + REORDENAÇÃO FINAL
     ============================================================ */
  const add=(id,items)=>{ const s=G.STAGES.find(s=>s.id===id); if(s) items.forEach(k=>{ if(!s.items.includes(k)) s.items.push(k); }); };
  add('bando',  ['armadilha','altar_improvisado','deposito_ossos','poste_sinal','curral','compostagem']);
  add('tribal', ['fumeiro','viveiro','campo_treino','poco','celeiro_palha','jardim_medicinal','vigia_arvore','doca_canoas']);
  add('aldeia', ['taverna','biblioteca_pequena','enfermaria','praca_central','estabulo','banheiro_publico','jardim_comunitario','prisao_local','guilda','torre_sino','armazem','casa_prefeito']);
  add('estado', ['forum','prisao_central','alfandega','banco','arquivo','coliseu','embaixada','farol_terrestre']);
  add('medieval', ['masmorra','catapulta','aqueduto','palacio','biblioteca_real','tesouro','quartel','arena','celeiro_real','casa_moeda','mirante','portal_magico']);
  add('culto',  ['mosteiro','cemiterio','oraculo','labirinto','jardim_meditacao','camara_sacrificio','relicario','escola_doutrina','torre_sino','mausoleu']);
  add('costa',  ['estaleiro','armazem_portuario','taverna_maritima','posto_pesca','vigia_costeira','mercado_peixe','quarentena','fortaleza_costeira','cais_flutuante']);

  // novos estágios
  const PEDRA={id:'pedra', nm:'Pré-histórico / Pedra', items:['caverna','dolmen','pintura_rupestre','fogueira_central']};
  const NOMADE={id:'nomade', nm:'Nômade avançado', items:['tenda_grande','carroca','circulo_fogueiras','altar_portatil']};
  const RENASC={id:'renascimento', nm:'Renascimento / Mercantil', items:['banco_mercantil','tipografia','atelie','porto_comercial','fabrica_tecidos']};
  const INDUS={id:'industrial', nm:'Industrial', items:['fabrica','ferrovia','armazem_carvao','torre_relogio','sindicato']};

  // ordem cronológica final
  const order=['pedra','bando','nomade','tribal','aldeia','renascimento','medieval','industrial','estado','culto','costa'];
  const lookup={}; G.STAGES.forEach(s=>lookup[s.id]=s);
  lookup.pedra=PEDRA; lookup.nomade=NOMADE; lookup.renascimento=RENASC; lookup.industrial=INDUS;
  G.STAGES.length=0;
  order.forEach(id=>{ if(lookup[id]) G.STAGES.push(lookup[id]); });

  G.ruinSVG = ruinSVG;
  // estruturas-âncora para a vitrine de ruínas
  G.RUIN_SHOWCASE = ['castelo','templo','casa_pedra','torre','muralha','coliseu'];

  window.GeneseBuildings = G;
})();

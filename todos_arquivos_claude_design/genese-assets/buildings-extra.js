/* ============================================================
   GÊNESE — Construções: expansão + estágio Medieval (extensão de buildings.js)
   Novas peças por estágio + um reino medieval completo (castelo, torre,
   muralha, igreja, moinho, forja, ponte de pedra, mercado) + paleta cultural.
   Mesmo estilo de contorno escuro, tintável. Espelha AA §4 / GDD §5.
   ============================================================ */
(function(){
  const G = window.GeneseBuildings; if(!G) return;
  const OL='#23201c', SW=2.6;
  const B = G.PARTS;

  /* ---- expansão: Bando ---- */
  B.abrigo = {w:82,h:56,draw:p=>`
    <ellipse cx="42" cy="52" rx="30" ry="5" fill="${p.dark||'#2a221a'}" opacity=".35"/>
    <path d="M10 54 L70 18 L74 26 L18 54 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M70 18 L70 8 M50 30 L50 54 M30 42 L30 54" stroke="${p.wood}" stroke-width="3" stroke-linecap="round"/>
    <path d="M16 50 L66 22" stroke="${OL}" stroke-width="1.2" opacity=".3"/>`};

  B.secador = {w:72,h:66,draw:p=>`
    <g stroke="${p.wood}" stroke-width="4" stroke-linecap="round"><path d="M14 64 L14 8 M58 64 L58 8 M10 12 L62 12"/></g>
    <path d="M22 16 C20 30 22 44 30 50 C40 52 50 44 50 30 C50 22 46 16 40 16Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${p.wood}" stroke-width="1.4" opacity=".6"><path d="M24 16 V12 M36 16 V12 M48 18 V12"/></g>
    <circle cx="30" cy="30" r="1.6" fill="${OL}" opacity=".5"/><circle cx="40" cy="36" r="1.6" fill="${OL}" opacity=".5"/>`};

  /* ---- expansão: Tribal ---- */
  B.cerca = {w:92,h:48,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}">
      ${[8,22,36,50,64,78].map(x=>`<path d="M${x} 46 L${x} 12 L${x+5} 6 L${x+10} 12 L${x+10} 46Z" fill="${p.wood}"/>`).join('')}
    </g>
    <path d="M6 22 H88 M6 34 H88" stroke="${p.wood}" stroke-width="3" opacity=".85"/>`};

  B.monte_oferenda = {w:56,h:58,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}">
      <ellipse cx="28" cy="50" rx="20" ry="7" fill="${p.stone}"/>
      <ellipse cx="28" cy="40" rx="15" ry="6" fill="${p.stone}"/>
      <ellipse cx="28" cy="31" rx="11" ry="5" fill="${p.stone}"/>
      <ellipse cx="28" cy="23" rx="7" ry="4" fill="${p.stone}"/>
    </g>
    <path d="M28 18 C24 12 28 8 28 4 C30 8 32 12 28 18Z" fill="${p.accent}" stroke="${OL}" stroke-width="1.6"/>
    <path d="M16 48 H40 M19 39 H37" stroke="${p.glyph||'#7C4DBE'}" stroke-width="1.2" opacity=".55"/>`};

  /* ---- expansão: Aldeia ---- */
  B.poco = {w:66,h:80,draw:p=>`
    <path d="M14 24 L18 8 H48 L52 24" fill="none" stroke="${p.wood}" stroke-width="4" stroke-linecap="round"/>
    <path d="M10 6 H56 L52 14 H14 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="12" y="44" width="42" height="32" rx="3" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <ellipse cx="33" cy="44" rx="21" ry="7" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M12 58 H54 M20 44 V76 M46 44 V76" stroke="${OL}" stroke-width="1.2" opacity=".3"/>
    <rect x="28" y="30" width="10" height="9" rx="2" fill="${p.wood}" stroke="${OL}" stroke-width="2"/>
    <path d="M33 24 V30" stroke="${OL}" stroke-width="2"/>`};

  B.casa_pedra = {w:90,h:80,draw:p=>`
    <rect x="16" y="36" width="58" height="42" rx="3" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 38 L45 12 L80 38 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="38" y="54" width="16" height="24" rx="2" fill="${p.dark||'#352a22'}" stroke="${OL}" stroke-width="2"/>
    <rect x="22" y="46" width="12" height="12" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <rect x="56" y="46" width="12" height="12" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <path d="M16 50 H74 M16 62 H74 M30 36 V78 M52 36 V78" stroke="${OL}" stroke-width="1" opacity=".25"/>
    <rect x="58" y="14" width="9" height="16" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`};

  B.forno = {w:66,h:60,draw:p=>`
    <path d="M8 58 C6 32 26 20 38 20 C54 20 62 34 60 58 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M22 58 C22 42 44 42 44 58 Z" fill="${p.dark||'#2a221a'}" stroke="${OL}" stroke-width="2"/>
    <path d="M14 40 C26 34 46 34 56 40 M12 50 C28 44 44 44 58 50" stroke="${OL}" stroke-width="1.2" fill="none" opacity=".3"/>
    <rect x="30" y="6" width="9" height="16" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>
    <path d="M30 50 C28 44 32 42 33 38 C36 42 38 46 34 50Z" fill="${p.flame||'#E08A3A'}" stroke="${OL}" stroke-width="1.4"/>`};

  /* ---- expansão: Estado ---- */
  B.portao = {w:76,h:86,draw:p=>`
    <rect x="10" y="20" width="14" height="64" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="52" y="20" width="14" height="64" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="6" y="10" width="22" height="12" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="48" y="10" width="22" height="12" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M24 84 V44 C24 32 52 32 52 44 V84Z" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M38 32 V84 M28 50 H48 M28 64 H48" stroke="${OL}" stroke-width="1.6" opacity=".4"/>
    <circle cx="33" cy="58" r="1.8" fill="${p.accent}"/><circle cx="43" cy="58" r="1.8" fill="${p.accent}"/>`};

  B.ponte = {w:102,h:52,draw:p=>`
    <path d="M6 44 C30 20 72 20 96 44" fill="none" stroke="${p.wood}" stroke-width="7" stroke-linecap="round"/>
    <path d="M6 50 C30 26 72 26 96 50" fill="none" stroke="${p.wood}" stroke-width="4" stroke-linecap="round" opacity=".7"/>
    <g stroke="${p.wood}" stroke-width="3" stroke-linecap="round">
      ${[18,34,51,68,84].map((x,i)=>{const y=[34,26,23,26,34][i];return `<path d="M${x} ${y} V${y+10}"/>`;}).join('')}
    </g>
    <path d="M6 44 L18 44 M84 44 L96 44" stroke="${OL}" stroke-width="1.2" opacity=".3"/>`};

  B.estatua = {w:56,h:98,draw:p=>`
    <rect x="14" y="80" width="28" height="16" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="18" y="72" width="20" height="10" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M28 24 C20 24 20 38 22 50 L20 72 H36 L34 50 C36 38 36 24 28 24Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <circle cx="28" cy="16" r="9" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M22 40 L14 30 M34 40 L42 30" stroke="${p.wall}" stroke-width="6" stroke-linecap="round"/>
    <path d="M28 24 v44" stroke="${OL}" stroke-width="1" opacity=".25"/>
    <circle cx="28" cy="16" r="12" fill="none" stroke="${p.accent}" stroke-width="1.6" opacity=".6"/>`};

  /* ---- expansão: Culto ---- */
  B.menir = {w:82,h:76,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}">
      <path d="M10 74 L8 26 C8 18 22 18 22 26 L20 74Z" fill="${p.stone}"/>
      <path d="M62 74 L60 26 C60 18 74 18 74 26 L72 74Z" fill="${p.stone}"/>
      <path d="M4 24 C4 14 78 14 78 24 C78 32 60 32 41 32 C22 32 4 32 4 24Z" fill="${p.stone}"/>
    </g>
    <path d="M12 40 V70 M66 40 V70" stroke="${OL}" stroke-width="1" opacity=".3"/>
    <g stroke="${p.glyph||'#7C4DBE'}" stroke-width="1.4" opacity=".6"><path d="M37 20 h8 M39 24 h6"/></g>`};

  B.santuario = {w:66,h:80,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}">
      <rect x="14" y="40" width="8" height="38" fill="${p.wood}"/>
      <rect x="44" y="40" width="8" height="38" fill="${p.wood}"/>
      <path d="M6 40 L33 20 L60 40 Z" fill="${p.roof}" stroke-linejoin="round"/>
      <rect x="2" y="38" width="62" height="6" rx="2" fill="${p.roof}"/>
    </g>
    <circle cx="33" cy="58" r="9" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <path d="M33 51 C29 55 29 61 33 65 C37 61 37 55 33 51Z" fill="${p.flame||'#7C4DBE'}"/>
    <path d="M33 14 V20" stroke="${p.accent}" stroke-width="3" stroke-linecap="round"/>`};

  /* ---- expansão: Costa ---- */
  B.barco = {w:92,h:66,draw:p=>`
    <path d="M8 44 C14 60 78 60 84 44 Z" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M14 48 H78" stroke="${OL}" stroke-width="1.4" opacity=".3"/>
    <path d="M46 44 V8" stroke="${p.wood}" stroke-width="4" stroke-linecap="round"/>
    <path d="M46 10 C64 14 66 30 46 38 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M46 14 H58 M46 24 H62 M46 32 H56" stroke="${OL}" stroke-width="1.2" opacity=".3"/>
    <path d="M46 8 L40 4" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.farol = {w:62,h:102,draw:p=>`
    <path d="M18 98 L22 40 H40 L44 98 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M22 56 H40 M21 72 H43 M20 88 H44" stroke="${p.roof}" stroke-width="5" opacity=".85"/>
    <rect x="20" y="26" width="22" height="14" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="23" y="28" width="16" height="10" fill="${p.accent}" stroke="${OL}" stroke-width="1.6"/>
    <path d="M18 26 H44 L40 20 H22 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M31 20 V12" stroke="${p.accent}" stroke-width="3" stroke-linecap="round"/>
    <g stroke="${p.accent}" stroke-width="2" opacity=".5"><path d="M42 33 L56 28 M42 36 L56 41"/></g>`};

  B.casa_pesca = {w:86,h:82,draw:p=>`
    <ellipse cx="43" cy="80" rx="40" ry="4" fill="${p.accent}" opacity=".22"/>
    <g stroke="${p.wood}" stroke-width="4" stroke-linecap="round"><path d="M22 80 V52 M44 80 V52 M64 80 V52"/></g>
    <rect x="16" y="34" width="54" height="22" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 36 L43 14 L76 36 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="36" y="40" width="14" height="16" rx="2" fill="${p.dark||'#2a221a'}" stroke="${OL}" stroke-width="2"/>
    <path d="M64 52 L78 74" stroke="${p.wood}" stroke-width="2.4" stroke-linecap="round"/>
    <path d="M78 74 l-3 -2 m3 2 l-2 3" stroke="${p.accent}" stroke-width="1.6" stroke-linecap="round"/>`};

  /* ============================================================
     MEDIEVAL / REINO
     ============================================================ */
  B.castelo = {w:122,h:106,draw:p=>`
    <rect x="24" y="38" width="74" height="64" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="14" y="22" width="22" height="80" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="86" y="22" width="22" height="80" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[14,24,86,96].map(x=>`<rect x="${x}" y="14" width="9" height="9" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    ${[26,40,54,68,82].map(x=>`<rect x="${x}" y="30" width="8" height="9" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <path d="M48 102 V64 C48 50 74 50 74 64 V102Z" fill="${p.dark||'#34302a'}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M61 50 V102 M48 76 H74" stroke="${OL}" stroke-width="1.4" opacity=".4"/>
    <rect x="20" y="42" width="9" height="14" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <rect x="93" y="42" width="9" height="14" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <path d="M19 14 V4 M97 14 V4" stroke="${p.wood}" stroke-width="2"/>
    <path d="M19 4 L31 7 L19 10Z M97 4 L85 7 L97 10Z" fill="${p.flame||p.accent}" stroke="${OL}" stroke-width="1.2"/>
    <path d="M24 60 H98 M24 80 H98" stroke="${OL}" stroke-width="1" opacity=".2"/>`};

  B.torre = {w:56,h:108,draw:p=>`
    <rect x="14" y="32" width="28" height="74" rx="3" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 34 L28 6 L48 34 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="22" y="46" width="12" height="16" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <rect x="24" y="76" width="8" height="16" rx="4" fill="${p.dark||'#34302a'}" stroke="${OL}" stroke-width="2"/>
    <path d="M14 68 H42 M14 90 H42" stroke="${OL}" stroke-width="1" opacity=".25"/>
    <path d="M28 6 V0" stroke="${p.wood}" stroke-width="2"/>
    <path d="M28 1 L40 3 L28 6Z" fill="${p.flame||p.accent}" stroke="${OL}" stroke-width="1.2"/>`};

  B.muralha = {w:122,h:68,draw:p=>`
    <rect x="20" y="30" width="82" height="36" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[24,40,56,72,88].map(x=>`<rect x="${x}" y="22" width="11" height="9" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <rect x="4" y="14" width="22" height="52" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="96" y="14" width="22" height="52" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[4,15,96,107].map(x=>`<rect x="${x}" y="7" width="9" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <path d="M50 66 V44 C50 34 72 34 72 44 V66Z" fill="${p.dark||'#34302a'}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M61 34 V66" stroke="${OL}" stroke-width="1.4" opacity=".4"/>
    <rect x="10" y="26" width="9" height="13" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <rect x="103" y="26" width="9" height="13" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <path d="M20 46 H102 M30 30 V66 M92 30 V66" stroke="${OL}" stroke-width="1" opacity=".2"/>`};

  B.igreja = {w:98,h:110,draw:p=>`
    <rect x="40" y="44" width="50" height="64" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M40 44 L65 22 L90 44 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="12" y="40" width="26" height="68" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 40 L25 6 L40 40 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M25 0 V12 M20 5 H30" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>
    <path d="M58 108 V72 C58 60 72 60 72 72 V108Z" fill="${p.dark||'#34302a'}" stroke="${OL}" stroke-width="${SW}"/>
    <circle cx="65" cy="40" r="8" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <path d="M65 32 V48 M57 40 H73" stroke="${OL}" stroke-width="1.4" opacity=".5"/>
    <rect x="18" y="52" width="14" height="22" rx="7" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <path d="M40 64 H90 M40 86 H90" stroke="${OL}" stroke-width="1" opacity=".2"/>`};

  B.moinho = {w:82,h:110,draw:p=>`
    <path d="M22 108 L30 36 H52 L60 108 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M28 30 L41 16 L54 30 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="34" y="84" width="14" height="24" rx="2" fill="${p.dark||'#34302a'}" stroke="${OL}" stroke-width="2"/>
    <path d="M26 56 H56 M25 72 H57" stroke="${OL}" stroke-width="1" opacity=".25"/>
    <g stroke="${OL}" stroke-width="2" fill="${p.wall}">
      <path d="M41 40 L41 8 L47 8 L43 40Z"/>
      <path d="M41 40 L73 40 L73 34 L41 36Z"/>
      <path d="M41 40 L41 72 L35 72 L39 40Z"/>
      <path d="M41 40 L9 40 L9 46 L41 44Z"/>
    </g>
    <circle cx="41" cy="40" r="4.5" fill="${p.wood}" stroke="${OL}" stroke-width="2"/>
    <circle cx="41" cy="40" r="1.8" fill="${p.accent}"/>`};

  B.forja = {w:90,h:80,draw:p=>`
    <rect x="14" y="40" width="62" height="38" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 42 L45 18 L82 42 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="22" y="50" width="26" height="28" rx="2" fill="${p.dark||'#241c18'}" stroke="${OL}" stroke-width="2"/>
    <path d="M35 76 C31 68 36 64 35 58 C40 64 42 70 35 76Z" fill="${p.flame||'#E08A3A'}" stroke="${OL}" stroke-width="1.4"/>
    <rect x="56" y="14" width="12" height="28" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <g fill="${p.flame2||'#F0C46A'}" opacity=".85"><circle cx="62" cy="10" r="2"/><circle cx="60" cy="4" r="1.6"/><circle cx="65" cy="6" r="1.4"/></g>
    <path d="M55 60 H71 M62 56 V68" stroke="${p.stone}" stroke-width="2.6" stroke-linecap="round"/>
    <circle cx="62" cy="62" r="3" fill="${p.stone}" stroke="${OL}" stroke-width="1.6"/>`};

  B.ponte_pedra = {w:106,h:62,draw:p=>`
    <path d="M4 56 C4 34 30 22 53 22 C76 22 102 34 102 56 L88 56 C88 38 70 32 53 32 C36 32 18 38 18 56 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="2" y="14" width="102" height="10" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[8,22,36,68,82,96].map(x=>`<rect x="${x}" y="6" width="8" height="9" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <path d="M53 22 C40 22 30 28 24 38 M53 22 C66 22 76 28 82 38" stroke="${OL}" stroke-width="1.2" fill="none" opacity=".3"/>`};

  B.mercado = {w:90,h:72,draw:p=>`
    <g stroke="${p.wood}" stroke-width="4" stroke-linecap="round"><path d="M16 70 V30 M74 70 V30"/></g>
    <path d="M8 30 H82 L82 22 Q45 12 8 22 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${[20,32,45,57,69].map(x=>`<path d="M${x} 21 V30" stroke="${OL}" stroke-width="1.4" opacity=".4"/>`).join('')}
    <rect x="18" y="44" width="54" height="8" rx="2" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${OL}" stroke-width="1.6">
      <circle cx="28" cy="40" r="4" fill="${p.accent}"/><circle cx="38" cy="40" r="4" fill="${p.flame||'#E08A3A'}"/><circle cx="48" cy="40" r="4" fill="${p.roof}"/>
      <rect x="56" y="34" width="10" height="10" rx="2" fill="${p.stone}"/>
    </g>
    <path d="M18 52 V70 M72 52 V70" stroke="${p.wood}" stroke-width="2" opacity=".5"/>`};

  /* ---- expandir estágios existentes + adicionar Medieval ---- */
  const add=(id,items)=>{ const s=G.STAGES.find(s=>s.id===id); if(s) items.forEach(k=>{ if(!s.items.includes(k)) s.items.push(k); }); };
  add('bando',  ['abrigo','secador']);
  add('tribal', ['cerca','monte_oferenda']);
  add('aldeia', ['poco','casa_pedra','forno']);
  add('estado', ['portao','ponte','estatua']);
  add('culto',  ['menir','santuario']);
  add('costa',  ['barco','farol','casa_pesca']);

  // inserir estágio Medieval entre 'estado' e 'culto'
  const medieval={id:'medieval', nm:'Medieval / Reino', items:['castelo','torre','muralha','igreja','moinho','forja','ponte_pedra','mercado']};
  const idx=G.STAGES.findIndex(s=>s.id==='estado');
  if(idx>=0) G.STAGES.splice(idx+1,0,medieval); else G.STAGES.push(medieval);

  /* ---- paleta cultural medieval ---- */
  G.CULTURES.medieval={nm:'Cultura Medieval (Reino)', wall:'#C7BBA2', roof:'#7E4A3E', wood:'#6E5638', stone:'#A9A294', accent:'#B7402E', flame:'#E08A3A', flame2:'#F0C46A', dark:'#34302a', glyph:'#7E4A3E'};

  /* ---- vinheta de reino medieval (sobrepõe village p/ 'medieval') ---- */
  const baseVillage = G.village;
  G.village = function(ck, w, h){
    if(ck!=='medieval') return baseVillage(ck, w, h);
    const pal=G.CULTURES.medieval; w=w||470; h=h||180; const gy=h-22;
    return `<svg viewBox="0 0 ${w} ${h}" width="100%" preserveAspectRatio="xMidYMax meet" xmlns="http://www.w3.org/2000/svg" style="display:block">
      <defs><linearGradient id="medsky" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#B9C2CC"/><stop offset="1" stop-color="#8A93A0"/></linearGradient></defs>
      <rect x="0" y="0" width="${w}" height="${h}" rx="12" fill="url(#medsky)"/>
      <ellipse cx="${w*0.5}" cy="${gy+4}" rx="${w*0.5}" ry="22" fill="#6E7A72" opacity=".5"/>
      ${G.place('muralha', w*0.5, gy+4, 0.78, pal)}
      ${G.place('torre', w*0.16, gy, 0.78, pal)}
      ${G.place('igreja', w*0.8, gy, 0.66, pal)}
      ${G.place('moinho', w*0.93, gy, 0.5, pal)}
      ${G.place('castelo', w*0.49, gy-2, 0.74, pal)}
      ${G.place('casa_pedra', w*0.3, gy, 0.5, pal)}
      ${G.place('mercado', w*0.66, gy, 0.5, pal)}
    </svg>`;
  };
})();

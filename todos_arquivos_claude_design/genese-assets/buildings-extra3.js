/* ============================================================
   GÊNESE — Construções: Aldeia/Cidade + Estruturas de Estado (2º doc)
   Extensão de buildings.js. Vetores tintáveis.
   ============================================================ */
(function(){
  const G = window.GeneseBuildings; if(!G) return;
  const OL='#23201c', SW=2.6;
  const B = G.PARTS;
  const win=(x,y,w,h,p)=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>`;
  const door=(x,y,w,h,p)=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="2" fill="${p.dark||'#352a22'}" stroke="${OL}" stroke-width="2"/>`;

  /* ============================================================
     ALDEIA / CIDADE
     ============================================================ */
  B.taverna = {w:96,h:84,draw:p=>`
    <rect x="14" y="40" width="68" height="42" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 42 L48 12 L88 42 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${door(40,58,16,24,p)}${win(20,50,12,12,p)}${win(64,50,12,12,p)}
    <path d="M14 52 H82 M30 40 V82 M66 40 V82" stroke="${OL}" stroke-width="1" opacity=".22"/>
    <rect x="60" y="14" width="9" height="18" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>
    <path d="M82 50 l14 -2" stroke="${p.wood}" stroke-width="2.4" stroke-linecap="round"/>
    <circle cx="96" cy="48" r="5" fill="${p.flame||'#E08A3A'}" stroke="${OL}" stroke-width="1.6"/><path d="M93 46 q3 3 6 0" stroke="${OL}" stroke-width="1.2" fill="none"/>`};

  B.biblioteca_pequena = {w:82,h:82,draw:p=>`
    <rect x="14" y="34" width="54" height="48" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 36 L41 12 L72 36 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${door(34,58,14,24,p)}
    <g stroke="${OL}" stroke-width="1.6" fill="${p.accent}"><rect x="20" y="44" width="28" height="9"/><rect x="20" y="44" width="9" height="9" fill="${p.flame||'#C0563A'}"/><rect x="50" y="44" width="14" height="9" fill="${p.roof}"/></g>
    <path d="M41 12 V6" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.enfermaria = {w:84,h:80,draw:p=>`
    <rect x="14" y="36" width="56" height="44" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="12" y="28" width="60" height="10" rx="2" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}"/>
    ${door(34,56,16,24,p)}${win(20,46,11,11,p)}${win(53,46,11,11,p)}
    <g stroke="#fff" stroke-width="3"><path d="M42 16 V30 M35 23 H49"/></g>
    <rect x="36" y="14" width="12" height="16" rx="2" fill="${p.flame||'#C0563A'}" stroke="${OL}" stroke-width="2"/>
    <g stroke="#fff" stroke-width="2.4"><path d="M42 18 V26 M38 22 H46"/></g>`};

  B.praca_central = {w:104,h:64,draw:p=>`
    <ellipse cx="52" cy="52" rx="48" ry="10" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <ellipse cx="52" cy="48" rx="20" ry="7" fill="${p.dark||'#3a5a6a'}" stroke="${OL}" stroke-width="2"/>
    <rect x="48" y="26" width="8" height="20" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>
    <ellipse cx="52" cy="26" rx="12" ry="4" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>
    <path d="M52 20 C49 24 55 24 52 20Z M46 28 q-4 6 0 10 M58 28 q4 6 0 10" stroke="${p.accent}" stroke-width="2" fill="none" stroke-linecap="round" opacity=".7"/>
    <g fill="${p.accent}" opacity=".6">${[44,52,60].map(x=>`<circle cx="${x}" cy="38" r="1.6"/>`).join('')}</g>
    <g stroke="${OL}" stroke-width="1" opacity=".25">${[20,38,66,84].map(x=>`<path d="M${x} 46 V58"/>`).join('')}</g>`};

  B.estabulo = {w:96,h:74,draw:p=>`
    <rect x="14" y="36" width="68" height="38" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 38 L48 14 L88 38 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M24 74 V48 C24 40 40 40 40 48 V74 M56 74 V48 C56 40 72 40 72 48 V74" fill="${p.dark||'#3a2c20'}" stroke="${OL}" stroke-width="2"/>
    <path d="M32 40 V74 M64 40 V74" stroke="${OL}" stroke-width="1" opacity=".25"/>
    <ellipse cx="32" cy="62" rx="6" ry="4" fill="${p.wall||'#8A6A3A'}" stroke="${OL}" stroke-width="1.6"/>
    <path d="M28 58 q-3 -4 -1 -7" stroke="${OL}" stroke-width="1.6" fill="none"/>
    <rect x="42" y="16" width="12" height="8" fill="${p.wood}" stroke="${OL}" stroke-width="1.6"/>`};

  B.banheiro_publico = {w:78,h:66,draw:p=>`
    <rect x="14" y="30" width="50" height="36" rx="3" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="10" y="22" width="58" height="10" rx="2" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${OL}" stroke-width="2"><path d="M39 30 V66"/></g>
    ${door(20,46,12,20,p)}${door(46,46,12,20,p)}
    <path d="M24 38 q4 -6 8 0 M48 38 q4 -6 8 0" stroke="${p.accent}" stroke-width="1.8" fill="none" opacity=".6"/>
    <path d="M30 16 C28 22 32 22 30 16Z" fill="${p.accent}" opacity=".5"/>`};

  B.jardim_comunitario = {w:96,h:54,draw:p=>`
    <ellipse cx="48" cy="46" rx="44" ry="8" fill="${p.flame||'#3C6E4A'}" opacity=".25"/>
    <g stroke="${p.wood}" stroke-width="2" fill="none">${[14,80].map(x=>`<path d="M${x} 46 V30"/>`).join('')}<path d="M14 32 H80" /></g>
    ${[24,40,56,72].map((x,i)=>`<g><path d="M${x} 46 V26" stroke="${p.flame||'#3C6E4A'}" stroke-width="2.4" stroke-linecap="round"/><circle cx="${x}" cy="22" r="5" fill="${[p.accent,'#E0708A','#C9A0FF','#F0C46A'][i]}" stroke="${OL}" stroke-width="1.4"/></g>`).join('')}
    <rect x="40" y="38" width="16" height="8" rx="2" fill="${p.wood}" stroke="${OL}" stroke-width="1.6"/>`};

  B.prisao_local = {w:82,h:78,draw:p=>`
    <rect x="14" y="30" width="54" height="48" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[18,30,42,54].map(x=>`<rect x="${x}" y="22" width="9" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    ${door(32,54,16,24,p)}
    <rect x="20" y="40" width="16" height="16" rx="2" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="2"/>
    <g stroke="${p.accent}" stroke-width="2"><path d="M24 40 V56 M28 40 V56 M32 40 V56"/></g>
    <path d="M14 44 H68 M40 30 V78" stroke="${OL}" stroke-width="1" opacity=".22"/>`};

  B.guilda = {w:90,h:84,draw:p=>`
    <rect x="14" y="38" width="62" height="44" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 40 L45 12 L82 40 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${door(36,58,16,24,p)}${win(22,48,12,12,p)}${win(56,48,12,12,p)}
    <rect x="38" y="20" width="14" height="16" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <g stroke="${OL}" stroke-width="1.8"><path d="M42 24 l6 8 M48 24 l-6 8"/></g>
    <path d="M45 12 V6" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.torre_sino = {w:60,h:108,draw:p=>`
    <rect x="18" y="36" width="24" height="70" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M12 38 L30 14 L48 38 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M22 44 C22 36 38 36 38 44 L38 56 L22 56 Z" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="2"/>
    <path d="M26 44 C26 50 34 50 34 44 L33 54 L27 54Z" fill="${p.accent}" stroke="${OL}" stroke-width="1.6"/>
    <path d="M30 42 V40" stroke="${OL}" stroke-width="1.6"/>
    ${win(25,68,10,12,p)}<path d="M18 88 H42" stroke="${OL}" stroke-width="1" opacity=".25"/>
    <path d="M30 14 V6" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.armazem = {w:100,h:74,draw:p=>`
    <rect x="12" y="34" width="76" height="40" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 36 C8 22 92 22 92 36 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="34" y="46" width="32" height="28" rx="2" fill="${p.dark||'#3a2c20'}" stroke="${OL}" stroke-width="2"/>
    <path d="M50 46 V74 M34 60 H66" stroke="${p.wood}" stroke-width="1.6" opacity=".6"/>
    ${win(18,44,12,10,p)}${win(70,44,12,10,p)}
    <rect x="74" y="62" width="12" height="12" fill="${p.wood}" stroke="${OL}" stroke-width="1.6"/>`};

  B.casa_prefeito = {w:96,h:88,draw:p=>`
    <rect x="16" y="42" width="64" height="46" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 44 L48 14 L86 44 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${OL}" stroke-width="2" fill="${p.stone}"><rect x="24" y="60" width="6" height="28"/><rect x="66" y="60" width="6" height="28"/></g>
    ${door(40,62,16,26,p)}${win(28,50,10,10,p)}${win(58,50,10,10,p)}
    <rect x="40" y="22" width="16" height="14" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <path d="M48 14 V6" stroke="${p.flame||p.accent}" stroke-width="2.4" stroke-linecap="round"/>
    <path d="M48 7 L60 9 L48 12Z" fill="${p.flame||p.accent}" stroke="${OL}" stroke-width="1.2"/>`};

  /* ============================================================
     ESTRUTURAS DE ESTADO
     ============================================================ */
  B.forum = {w:116,h:80,draw:p=>`
    <path d="M58 8 L108 30 H8 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="12" y="30" width="92" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[18,34,50,66,82,96].map(x=>`<rect x="${x}" y="38" width="9" height="32" fill="${p.wall}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <rect x="8" y="70" width="100" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M40 22 H76" stroke="${p.accent}" stroke-width="2" opacity=".6"/>`};

  B.prisao_central = {w:106,h:84,draw:p=>`
    <rect x="14" y="30" width="78" height="54" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[18,32,46,60,74,86].map(x=>`<rect x="${x}" y="22" width="10" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <g stroke="${p.accent}" stroke-width="2">${[26,38,50,68,80].map(x=>`<path d="M${x} 42 V62"/>`).join('')}</g>
    ${[20,62].map(x=>`<rect x="${x}" y="42" width="26" height="22" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <path d="M53 30 V84 M14 70 H92" stroke="${OL}" stroke-width="1" opacity=".2"/>
    <rect x="46" y="66" width="14" height="18" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="2"/>`};

  B.alfandega = {w:92,h:78,draw:p=>`
    <rect x="14" y="38" width="64" height="40" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="10" y="30" width="72" height="10" rx="2" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}"/>
    ${door(36,56,16,22,p)}${win(22,48,11,11,p)}${win(56,48,11,11,p)}
    <g stroke="${p.wood}" stroke-width="3" stroke-linecap="round"><path d="M84 30 V72 M84 36 H68"/></g>
    <rect x="64" y="34" width="6" height="6" fill="${p.flame||'#C0563A'}" stroke="${OL}" stroke-width="1.4"/>
    <path d="M84 32 l-4 -6 8 0z" fill="${p.accent}" stroke="${OL}" stroke-width="1.2"/>`};

  B.banco = {w:96,h:80,draw:p=>`
    <path d="M48 12 L90 32 H6 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="12" y="32" width="72" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[18,32,46,60,74].map(x=>`<rect x="${x}" y="40" width="8" height="32" fill="${p.wall}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <rect x="8" y="72" width="80" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <circle cx="48" cy="24" r="6" fill="${p.accent}" stroke="${OL}" stroke-width="1.8"/>
    <path d="M48 21 v6 M45 24 h6" stroke="${OL}" stroke-width="1.4"/>`};

  B.arquivo = {w:88,h:82,draw:p=>`
    <rect x="16" y="32" width="56" height="50" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="12" y="24" width="64" height="10" rx="2" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${OL}" stroke-width="1.8" fill="${p.dark||'#3a2c20'}">${[0,1,2].map(r=>[22,40,58].map(x=>`<rect x="${x}" y="${42+r*13}" width="12" height="9"/>`).join('')).join('')}</g>
    <g stroke="${p.accent}" stroke-width="1.4">${[0,1,2].map(r=>[27,45,63].map(x=>`<path d="M${x} ${46+r*13} h2"/>`).join('')).join('')}</g>`};

  B.coliseu = {w:118,h:84,draw:p=>`
    <ellipse cx="59" cy="46" rx="55" ry="36" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <ellipse cx="59" cy="46" rx="34" ry="20" fill="${p.dark||'#3a2c20'}" stroke="${OL}" stroke-width="2"/>
    <g stroke="${OL}" stroke-width="1.8" fill="${p.accent}">${[0,1].map(ring=>Array.from({length:10},(_,i)=>{const a=i/10*Math.PI*2;const rx=46-ring*9,ry=29-ring*6;return `<rect x="${(59+Math.cos(a)*rx-3).toFixed(0)}" y="${(46+Math.sin(a)*ry-5).toFixed(0)}" width="6" height="10" rx="3"/>`;}).join('')).join('')}</g>
    <ellipse cx="59" cy="46" rx="55" ry="36" fill="none" stroke="${OL}" stroke-width="${SW}"/>`};

  B.embaixada = {w:92,h:88,draw:p=>`
    <rect x="16" y="40" width="60" height="48" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="12" y="32" width="68" height="10" rx="2" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}"/>
    ${door(38,62,16,26,p)}${win(24,50,10,10,p)}${win(58,50,10,10,p)}${win(24,68,10,10,p)}${win(58,68,10,10,p)}
    <g stroke="${p.wood}" stroke-width="2.4" stroke-linecap="round"><path d="M28 32 V14 M64 32 V14"/></g>
    <path d="M28 14 h14 v8 h-14z" fill="${p.flame||'#C0563A'}" stroke="${OL}" stroke-width="1.4"/>
    <path d="M64 14 h14 v8 h-14z" fill="${p.accent}" stroke="${OL}" stroke-width="1.4"/>`};

  B.farol_terrestre = {w:56,h:104,draw:p=>`
    <path d="M18 100 L22 40 H34 L38 100 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="18" y="30" width="20" height="12" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="21" y="32" width="14" height="8" fill="${p.accent}" stroke="${OL}" stroke-width="1.6"/>
    <path d="M16 30 H40 L36 22 H20 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M28 22 V14" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>
    <g stroke="${p.accent}" stroke-width="2" opacity=".5"><path d="M40 33 l14 -4 M40 38 l14 4 M16 33 l-12 -4 M16 38 l-12 4"/></g>
    <path d="M22 56 H34 M21 72 H35 M20 88 H36" stroke="${OL}" stroke-width="1" opacity=".25"/>`};

  window.GeneseBuildings = G;
})();

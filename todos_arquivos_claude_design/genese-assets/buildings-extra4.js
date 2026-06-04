/* ============================================================
   GÊNESE — Construções: Renascimento/Mercantil + Medieval (expansão)
   + Industrial. Extensão de buildings.js. Vetores tintáveis.
   ============================================================ */
(function(){
  const G = window.GeneseBuildings; if(!G) return;
  const OL='#23201c', SW=2.6;
  const B = G.PARTS;
  const win=(x,y,w,h,p)=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>`;
  const door=(x,y,w,h,p)=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="2" fill="${p.dark||'#352a22'}" stroke="${OL}" stroke-width="2"/>`;

  /* ============================================================
     RENASCIMENTO / MERCANTIL
     ============================================================ */
  B.banco_mercantil = {w:92,h:86,draw:p=>`
    <rect x="16" y="40" width="60" height="46" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 42 L46 16 L82 42 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${door(38,60,16,26,p)}${win(24,50,11,12,p)}${win(57,50,11,12,p)}
    <ellipse cx="46" cy="30" rx="9" ry="7" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <path d="M46 25 v10 M42 30 h8" stroke="${OL}" stroke-width="1.4"/>
    <path d="M46 16 V8" stroke="${p.flame||p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.tipografia = {w:88,h:82,draw:p=>`
    <rect x="14" y="38" width="60" height="44" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 40 L44 14 L80 40 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${win(20,48,12,12,p)}${door(34,58,16,24,p)}
    <g stroke="${OL}" stroke-width="2" fill="${p.stone}"><rect x="54" y="46" width="16" height="18"/><rect x="50" y="42" width="24" height="5"/><path d="M62 64 V72 M56 72 H68" stroke="${p.wood}" stroke-width="2.4"/></g>
    <path d="M44 14 V6" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.atelie = {w:86,h:84,draw:p=>`
    <rect x="14" y="40" width="58" height="42" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M14 40 L43 18 L72 40 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${OL}" stroke-width="1.8" fill="${p.accent}"><path d="M18 40 L30 24 L42 40Z" fill="${p.dark||'#2a2622'}"/><path d="M44 40 L56 24 L68 40Z" fill="${p.accent}"/></g>
    ${door(34,60,16,22,p)}
    <g stroke="${p.wood}" stroke-width="2.4" stroke-linecap="round"><path d="M58 82 L70 56 M70 56 l-4 1 m4 -1 l1 4"/></g>
    <circle cx="70" cy="54" r="3" fill="${p.flame||'#C0563A'}"/>`};

  B.porto_comercial = {w:108,h:72,draw:p=>`
    <rect x="6" y="34" width="64" height="8" rx="2" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${p.wood}" stroke-width="3" stroke-linecap="round">${[14,32,50,64].map(x=>`<path d="M${x} 42 V64"/>`).join('')}</g>
    <rect x="16" y="20" width="20" height="14" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="2"/>
    <rect x="40" y="24" width="16" height="10" rx="2" fill="${p.roof}" stroke="${OL}" stroke-width="2"/>
    <path d="M72 36 C78 56 104 56 100 36 Z" fill="${p.wall||'#8A6A3A'}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M86 36 V8" stroke="${p.wood}" stroke-width="3" stroke-linecap="round"/>
    <path d="M86 10 C100 14 100 26 86 30Z" fill="${p.accent}" stroke="${OL}" stroke-width="2" stroke-linejoin="round"/>
    <path d="M2 64 q8 -4 14 0 q8 -4 14 0 q8 -4 14 0" stroke="${p.accent}" stroke-width="1.6" fill="none" opacity=".5"/>`};

  B.fabrica_tecidos = {w:98,h:80,draw:p=>`
    <rect x="14" y="40" width="70" height="40" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M14 40 L26 28 L38 40 L50 28 L62 40 L74 28 L84 40Z" fill="${p.roof}" stroke="${OL}" stroke-width="2" stroke-linejoin="round"/>
    ${[20,38,56].map(x=>`<rect x="${x}" y="30" width="10" height="8" fill="${p.accent}" stroke="${OL}" stroke-width="1.6"/>`).join('')}
    ${door(40,58,16,22,p)}${win(20,48,11,9,p)}${win(64,48,11,9,p)}
    <rect x="74" y="14" width="8" height="26" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>
    <g fill="${p.accent}" opacity=".5"><circle cx="78" cy="10" r="2.4"/><circle cx="75" cy="5" r="1.8"/></g>`};

  /* ============================================================
     MEDIEVAL / REINO (expansão)
     ============================================================ */
  B.masmorra = {w:88,h:72,draw:p=>`
    <path d="M10 70 C8 44 26 30 44 30 C62 30 80 44 78 70 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M30 70 C30 50 58 50 58 70 Z" fill="${p.dark||'#1a1714'}" stroke="${OL}" stroke-width="2"/>
    <g stroke="${p.accent}" stroke-width="2">${[38,44,50].map(x=>`<path d="M${x} 56 V70"/>`).join('')}</g>
    ${[16,28,60,72].map(x=>`<rect x="${x}" y="24" width="9" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <path d="M14 48 C30 40 58 40 74 48" stroke="${OL}" stroke-width="1" opacity=".25" fill="none"/>`};

  B.catapulta = {w:96,h:72,draw:p=>`
    <g stroke="${p.wood}" stroke-width="4" stroke-linecap="round"><path d="M16 66 L40 66 M22 66 L30 40 M40 66 L30 40"/></g>
    <circle cx="20" cy="66" r="7" fill="none" stroke="${OL}" stroke-width="${SW}"/><circle cx="44" cy="66" r="7" fill="none" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M30 40 L82 22" stroke="${p.wood}" stroke-width="5" stroke-linecap="round"/>
    <path d="M82 22 q8 -2 8 6 q-8 2 -10 -4Z" fill="${p.wall}" stroke="${OL}" stroke-width="2" stroke-linejoin="round"/>
    <circle cx="86" cy="20" r="5" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>
    <path d="M30 40 L22 56" stroke="${p.accent}" stroke-width="2" opacity=".6"/>`};

  B.aqueduto = {w:124,h:80,draw:p=>`
    <rect x="4" y="20" width="116" height="12" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="8" y="14" width="108" height="6" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>
    ${[10,44,78].map(x=>`<g stroke="${OL}" stroke-width="${SW}" fill="${p.stone}"><rect x="${x}" y="32" width="10" height="44"/><rect x="${x+26}" y="32" width="10" height="44"/><path d="M${x+10} 40 C${x+10} 32 ${x+26} 32 ${x+26} 40 L${x+26} 56 C${x+26} 48 ${x+10} 48 ${x+10} 56Z" fill="${p.dark||'#3a342c'}"/></g>`).join('')}
    <path d="M8 18 H116" stroke="${p.accent}" stroke-width="2" opacity=".5"/>`};

  B.palacio = {w:130,h:96,draw:p=>`
    <rect x="26" y="44" width="78" height="52" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="12" y="36" width="24" height="60" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="94" y="36" width="24" height="60" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M12 36 L24 18 L36 36Z M94 36 L106 18 L118 36Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M40 44 L65 22 L90 44Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M52 96 V62 C52 50 78 50 78 62 V96Z" fill="${p.dark||'#34302a'}" stroke="${OL}" stroke-width="${SW}"/>
    ${win(18,48,10,12,p)}${win(100,48,10,12,p)}${win(38,56,9,11,p)}${win(83,56,9,11,p)}
    <path d="M24 18 V10 M106 18 V10 M65 22 V12" stroke="${p.flame||p.accent}" stroke-width="2.4" stroke-linecap="round"/>
    <path d="M65 13 L77 15 L65 18Z" fill="${p.flame||p.accent}" stroke="${OL}" stroke-width="1.2"/>`};

  B.biblioteca_real = {w:104,h:88,draw:p=>`
    <rect x="16" y="40" width="72" height="48" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M52 14 L92 40 H12 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${door(44,62,16,26,p)}
    <g stroke="${OL}" stroke-width="1.8">${[0,1].map(r=>[22,40,62,80].map(x=>`<rect x="${x}" y="${46+r*16}" width="14" height="12" fill="${[p.accent,p.flame||'#C0563A',p.roof,p.accent][(x/18|0)%4]}"/>`).join('')).join('')}</g>
    <circle cx="52" cy="28" r="6" fill="${p.accent}" stroke="${OL}" stroke-width="1.8"/>`};

  B.tesouro = {w:84,h:72,draw:p=>`
    <path d="M12 68 C10 44 28 32 42 32 C56 32 74 44 72 68 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="28" y="48" width="28" height="20" rx="3" fill="${p.wood}" stroke="${OL}" stroke-width="2"/>
    <path d="M28 48 C28 40 56 40 56 48" fill="${p.wood}" stroke="${OL}" stroke-width="2"/>
    <rect x="38" y="52" width="8" height="8" fill="${p.accent}" stroke="${OL}" stroke-width="1.6"/>
    <g fill="${p.accent}">${[34,42,50].map(x=>`<circle cx="${x}" cy="46" r="2.4"/>`).join('')}</g>
    <path d="M42 26 q-3 -5 1 -9" stroke="${p.accent}" stroke-width="2" fill="none" stroke-linecap="round" opacity=".6"/>`};

  B.quartel = {w:104,h:78,draw:p=>`
    <rect x="14" y="36" width="76" height="42" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[18,32,46,60,74,86].map(x=>`<rect x="${x}" y="28" width="9" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>`).join('')}
    ${door(44,54,16,24,p)}${win(22,44,11,10,p)}${win(70,44,11,10,p)}
    <g stroke="${OL}" stroke-width="1.8"><path d="M30 50 l8 10 M38 50 l-8 10"/></g>
    <rect x="74" y="48" width="14" height="14" rx="2" fill="${p.flame||'#C0563A'}" stroke="${OL}" stroke-width="2"/>
    <path d="M81 51 l4 5 -8 0z" fill="${p.accent}"/>`};

  B.arena = {w:118,h:80,draw:p=>`
    <ellipse cx="59" cy="48" rx="54" ry="32" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <ellipse cx="59" cy="48" rx="36" ry="20" fill="${p.wall||'#C9A86A'}" stroke="${OL}" stroke-width="2"/>
    <ellipse cx="59" cy="48" rx="36" ry="20" fill="${p.dark||'#8A6A3A'}" opacity=".3"/>
    <g stroke="${OL}" stroke-width="1.6" fill="${p.accent}" opacity=".85">${Array.from({length:12},(_,i)=>{const a=i/12*Math.PI*2;return `<rect x="${(59+Math.cos(a)*45-2.5).toFixed(0)}" y="${(48+Math.sin(a)*27-4).toFixed(0)}" width="5" height="8" rx="2"/>`;}).join('')}</g>
    <g stroke="${OL}" stroke-width="1.8"><path d="M50 44 l8 8 M58 44 l-8 8"/></g>`};

  B.celeiro_real = {w:92,h:82,draw:p=>`
    <path d="M16 32 C16 18 76 18 76 32 L76 78 C76 82 16 82 16 78 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <ellipse cx="46" cy="32" rx="30" ry="10" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M16 48 C46 54 46 54 76 48 M16 62 C46 68 46 68 76 62" stroke="${OL}" stroke-width="1.4" fill="none" opacity=".3"/>
    <rect x="36" y="58" width="20" height="22" rx="2" fill="${p.dark||'#5a4a30'}" stroke="${OL}" stroke-width="2"/>
    <circle cx="46" cy="30" r="6" fill="${p.accent}" stroke="${OL}" stroke-width="1.8"/>`};

  B.casa_moeda = {w:88,h:80,draw:p=>`
    <rect x="16" y="40" width="56" height="40" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M44 16 L80 40 H8 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${door(36,58,16,22,p)}
    <g fill="${p.accent}" stroke="${OL}" stroke-width="1.6"><circle cx="30" cy="52" r="7"/><circle cx="54" cy="52" r="7"/></g>
    <path d="M30 49 v6 M27 52 h6 M54 49 v6 M51 52 h6" stroke="${OL}" stroke-width="1.2"/>
    <path d="M44 16 V8" stroke="${p.flame||p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.mirante = {w:54,h:106,draw:p=>`
    <path d="M18 104 L22 36 H32 L36 104 Z" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${p.wood}" stroke-width="2"><path d="M22 60 L32 60 M21 80 L33 80 M20 100 L34 100"/><path d="M22 50 L32 70 M32 50 L22 70"/></g>
    <rect x="10" y="22" width="34" height="16" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 22 L27 6 L46 22 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    ${win(16,26,9,9,p)}${win(29,26,9,9,p)}
    <path d="M27 6 V0" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.portal_magico = {w:72,h:96,draw:p=>`
    <path d="M14 92 L14 44 C14 16 58 16 58 44 L58 92" fill="none" stroke="${p.stone}" stroke-width="9" stroke-linejoin="round"/>
    <path d="M20 92 L20 46 C20 24 52 24 52 46 L52 92Z" fill="${p.glyph||'#7C4DBE'}" opacity=".4" stroke="${p.accent}" stroke-width="2"/>
    <g stroke="${p.accent}" stroke-width="1.6" fill="none" opacity=".8">${[0.3,0.55,0.8].map(t=>`<ellipse cx="36" cy="${(92-(92-30)*t).toFixed(0)}" rx="${(15*(1-t*0.3)).toFixed(0)}" ry="5"/>`).join('')}</g>
    <g fill="#fff" opacity=".7">${[[28,50],[44,62],[34,74],[42,40]].map(([x,y])=>`<circle cx="${x}" cy="${y}" r="1.6"/>`).join('')}</g>
    <g stroke="${p.glyph||'#7C4DBE'}" stroke-width="1.4" opacity=".7"><path d="M12 40 l-5 -5 M60 40 l5 -5"/></g>`};

  /* ============================================================
     INDUSTRIAL
     ============================================================ */
  B.fabrica = {w:110,h:84,draw:p=>`
    <rect x="12" y="42" width="80" height="42" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M12 42 L24 30 L36 42 L48 30 L60 42 L72 30 L84 42Z" fill="${p.roof}" stroke="${OL}" stroke-width="2" stroke-linejoin="round"/>
    ${[18,42,66].map(x=>`<rect x="${x}" y="32" width="10" height="9" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="1.6"/>`).join('')}
    ${door(40,62,16,22,p)}
    <rect x="80" y="10" width="12" height="34" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="78" y="8" width="16" height="5" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>
    <g fill="#9aa0a4" opacity=".6"><circle cx="86" cy="4" r="4"/><circle cx="80" cy="-2" r="3"/><circle cx="92" cy="-1" r="2.6"/></g>`};

  B.ferrovia = {w:116,h:50,draw:p=>`
    <g stroke="${p.stone}" stroke-width="3"><path d="M6 38 H110 M6 44 H110"/></g>
    <g stroke="${p.wood}" stroke-width="3">${[12,28,44,60,76,92,106].map(x=>`<path d="M${x} 34 V48"/>`).join('')}</g>
    <rect x="40" y="14" width="34" height="22" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="30" y="22" width="12" height="14" rx="2" fill="${p.roof}" stroke="${OL}" stroke-width="2"/>
    <rect x="46" y="6" width="9" height="10" fill="${p.stone}" stroke="${OL}" stroke-width="2"/>
    ${win(58,20,10,9,p)}
    <circle cx="36" cy="38" r="5" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="2"/><circle cx="66" cy="38" r="5" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="2"/>
    <g fill="#9aa0a4" opacity=".5"><circle cx="50" cy="2" r="2.6"/><circle cx="45" cy="-2" r="1.8"/></g>`};

  B.armazem_carvao = {w:96,h:72,draw:p=>`
    <rect x="14" y="36" width="68" height="36" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M10 38 L48 18 L86 38 Z" fill="${p.dark||'#2a2622'}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="34" y="48" width="28" height="24" rx="2" fill="${p.dark||'#1a1714'}" stroke="${OL}" stroke-width="2"/>
    <g fill="${p.dark||'#1a1714'}" stroke="${OL}" stroke-width="1.4"><path d="M16 72 q8 -12 18 -8 q10 -10 22 -2 q12 -6 24 4 V72Z"/></g>
    <g fill="#3a3a3a"><circle cx="28" cy="66" r="3"/><circle cx="40" cy="68" r="2.4"/><circle cx="60" cy="66" r="3"/></g>`};

  B.torre_relogio = {w:62,h:112,draw:p=>`
    <rect x="18" y="40" width="26" height="72" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M12 42 L31 16 L50 42 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <circle cx="31" cy="58" r="11" fill="${p.wall||'#E6DCC4'}" stroke="${OL}" stroke-width="${SW}"/>
    <circle cx="31" cy="58" r="1.8" fill="${OL}"/>
    <path d="M31 58 V51 M31 58 L37 60" stroke="${OL}" stroke-width="2" stroke-linecap="round"/>
    ${win(24,80,14,14,p)}
    <path d="M18 100 H44" stroke="${OL}" stroke-width="1" opacity=".25"/>
    <path d="M31 16 V8" stroke="${p.accent}" stroke-width="2.4" stroke-linecap="round"/>`};

  B.sindicato = {w:92,h:82,draw:p=>`
    <rect x="14" y="38" width="64" height="44" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="10" y="30" width="72" height="10" rx="2" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}"/>
    ${door(36,60,16,22,p)}${win(22,48,11,11,p)}${win(57,48,11,11,p)}
    <g stroke="${p.wood}" stroke-width="2.4" stroke-linecap="round"><path d="M46 30 V12"/></g>
    <path d="M46 12 h22 v10 h-22z" fill="${p.flame||'#C0563A'}" stroke="${OL}" stroke-width="1.6"/>
    <g stroke="#fff" stroke-width="1.6"><path d="M52 17 l4 0 M58 14 l0 6"/></g>`};

  window.GeneseBuildings = G;
})();

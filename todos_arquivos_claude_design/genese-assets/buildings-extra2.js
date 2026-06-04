/* ============================================================
   GÊNESE — Construções: expansão completa (extensão de buildings.js
   + buildings-extra.js). Adiciona TODAS as estruturas do catálogo por
   estágio (2º doc), novos estágios (Pré-histórico, Nômade, Renascimento,
   Industrial), variante Ruínas e novas culturas (Aquática, Nômade,
   Subterrânea, Arcana, Imperial, Tecnológica). Vetores tintáveis,
   mesmo contorno escuro. Espelha AA §4 / GDD §5.
   ============================================================ */
(function(){
  const G = window.GeneseBuildings; if(!G) return;
  const OL='#23201c', SW=2.6;
  const B = G.PARTS;
  const dot=(x,y,r,c)=>`<circle cx="${x}" cy="${y}" r="${r}" fill="${c}"/>`;

  /* ============================================================
     PRÉ-HISTÓRICO / PEDRA
     ============================================================ */
  B.caverna = {w:96,h:74,draw:p=>`
    <path d="M6 72 C2 36 24 14 48 14 C72 14 94 36 90 72 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M30 72 C28 48 68 48 66 72 Z" fill="${p.dark||'#1a1714'}" stroke="${OL}" stroke-width="2"/>
    <path d="M16 40 C30 30 66 30 80 40 M12 54 C30 46 66 46 84 54" stroke="${OL}" stroke-width="1.4" fill="none" opacity=".3"/>
    <g stroke="${p.glyph||'#C0563A'}" stroke-width="1.6" opacity=".7" fill="none"><path d="M22 58 l4 -6 l4 6 M30 60 v-7 M70 56 a3 3 0 1 0 0.1 0"/></g>`};

  B.dolmen = {w:88,h:72,draw:p=>`
    <rect x="12" y="30" width="14" height="42" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="62" y="30" width="14" height="42" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M4 30 L84 24 L82 12 L8 18 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M16 44 V68 M66 44 V68" stroke="${OL}" stroke-width="1" opacity=".3"/>`};

  B.pintura_rupestre = {w:78,h:72,draw:p=>`
    <path d="M8 70 C6 30 20 12 39 12 C58 12 72 30 70 70 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${p.glyph||'#C0563A'}" stroke-width="2" fill="none" stroke-linecap="round" opacity=".85">
      <circle cx="28" cy="34" r="3"/><path d="M28 37 v9 M24 41 h8 M28 46 l-4 7 M28 46 l4 7"/>
      <path d="M46 40 q8 -8 16 0 q-3 8 -8 8 q-6 0 -8 -8Z M50 52 l-3 6 M58 52 l3 6"/>
    </g>`};

  B.fogueira_central = {w:84,h:64,draw:p=>`
    <ellipse cx="42" cy="58" rx="36" ry="6" fill="${p.dark||'#2a221a'}" opacity=".4"/>
    <g stroke="${OL}" stroke-width="${SW}">${[8,24,42,60,76].map(x=>`<ellipse cx="${x}" cy="56" rx="8" ry="5" fill="${p.stone}"/>`).join('')}</g>
    <path d="M24 56 L60 44 M60 56 L24 44 M42 58 L42 42" stroke="${p.wood}" stroke-width="5" stroke-linecap="round"/>
    <path d="M42 10 C30 26 36 38 42 50 C52 40 54 26 46 14 C52 24 52 34 46 44 C58 36 58 18 42 10Z" fill="${p.flame||'#E08A3A'}" stroke="${OL}" stroke-width="2" stroke-linejoin="round"/>
    <path d="M42 26 C37 34 39 42 43 46 C48 42 48 34 43 28Z" fill="${p.flame2||'#F0C46A'}"/>`};

  /* ============================================================
     EXPANSÃO BANDO (2º doc)
     ============================================================ */
  B.armadilha = {w:64,h:48,draw:p=>`
    <ellipse cx="32" cy="40" rx="24" ry="7" fill="${p.dark||'#2a221a'}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${p.wood}" stroke-width="2.2" stroke-linecap="round">${[12,22,32,42,52].map(x=>`<path d="M${x} 40 L${x+ (x<32?-4:x>32?4:0)} 22"/>`).join('')}</g>
    <path d="M14 24 L50 24" stroke="${p.wood}" stroke-width="2" opacity=".5"/>
    <path d="M22 40 l8 -6 8 6" stroke="${p.accent}" stroke-width="1.6" fill="none"/>`};

  B.altar_improvisado = {w:58,h:60,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}">
      <ellipse cx="29" cy="52" rx="20" ry="6" fill="${p.stone}"/>
      <ellipse cx="29" cy="42" rx="14" ry="5" fill="${p.stone}"/>
      <ellipse cx="29" cy="33" rx="9" ry="4" fill="${p.stone}"/>
    </g>
    <path d="M29 28 C25 22 29 16 29 12 C31 16 33 22 29 28Z" fill="${p.flame||'#C0563A'}" stroke="${OL}" stroke-width="1.4"/>
    ${dot(29,18,2.4,p.accent)}`};

  B.deposito_ossos = {w:66,h:58,draw:p=>`
    <ellipse cx="33" cy="52" rx="28" ry="6" fill="${p.dark||'#2a221a'}" opacity=".35"/>
    <g stroke="${OL}" stroke-width="2" fill="${p.wall||'#E6DCC4'}">
      <path d="M16 50 q-4 -4 0 -8 q5 -2 5 3 q4 -3 7 1 q-2 5 -6 4 q1 4 -3 4 q-5 0 -3 -8Z"/>
      <circle cx="46" cy="34" r="9"/><rect x="42" y="42" width="8" height="8"/>
    </g>
    <g stroke="${OL}" stroke-width="1.4"><circle cx="43" cy="33" r="1.6" fill="${OL}"/><circle cx="49" cy="33" r="1.6" fill="${OL}"/></g>
    <path d="M22 50 L52 50" stroke="${p.wood}" stroke-width="3" stroke-linecap="round"/>`};

  B.poste_sinal = {w:46,h:90,draw:p=>`
    <rect x="20" y="14" width="7" height="76" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M27 22 L42 26 L27 32 Z" fill="${p.accent}" stroke="${OL}" stroke-width="2" stroke-linejoin="round"/>
    <path d="M20 40 L6 44 L20 50 Z" fill="${p.flame||'#C0563A'}" stroke="${OL}" stroke-width="2" stroke-linejoin="round"/>
    <g stroke="${p.glyph||'#7C4DBE'}" stroke-width="1.4" opacity=".7"><path d="M30 26 h6 M11 44 h6"/></g>
    <path d="M23 14 l-4 -6 8 0 z" fill="${p.wood}" stroke="${OL}" stroke-width="1.4"/>`};

  B.curral = {w:100,h:58,draw:p=>`
    <g stroke="${OL}" stroke-width="${SW}">${[8,26,44,62,80,92].map(x=>`<rect x="${x}" y="22" width="5" height="34" fill="${p.wood}"/>`).join('')}</g>
    <path d="M6 32 H96 M6 44 H96" stroke="${p.wood}" stroke-width="3.4"/>
    <ellipse cx="40" cy="48" rx="12" ry="7" fill="${p.wall||'#B59A6E'}" stroke="${OL}" stroke-width="2"/>
    <circle cx="30" cy="46" r="2" fill="${OL}"/><path d="M28 42 q-3 -3 -1 -5" stroke="${OL}" stroke-width="2" fill="none"/>`};

  B.compostagem = {w:62,h:52,draw:p=>`
    <path d="M8 48 C8 34 54 34 54 48 Z" fill="${p.dark||'#3a2e1e'}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${OL}" stroke-width="2" fill="${p.wood}">${[10,20,42,52].map(x=>`<rect x="${x}" y="30" width="4" height="20"/>`).join('')}</g>
    <g fill="${p.accent}" opacity=".8">${dot(24,40,2.2,p.accent)}${dot(34,42,2,p.flame||'#6E8F4A')}${dot(40,38,1.8,p.accent)}</g>
    <path d="M20 32 q4 -6 10 -3 q5 -5 10 1" stroke="${p.flame||'#6E8F4A'}" stroke-width="2" fill="none" stroke-linecap="round"/>`};

  /* ============================================================
     NÔMADE AVANÇADO
     ============================================================ */
  B.tenda_grande = {w:108,h:78,draw:p=>`
    <path d="M54 6 L100 72 H8 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M54 6 L30 72 M54 6 L78 72 M22 50 L86 50" stroke="${OL}" stroke-width="1.6" opacity=".35"/>
    <path d="M54 72 L46 38 L62 38 Z" fill="${p.dark||'#2a2420'}" stroke="${OL}" stroke-width="2"/>
    <g stroke="${p.accent}" stroke-width="2" opacity=".7"><path d="M34 58 h12 M62 58 h12 M44 64 h20"/></g>
    <path d="M50 6 L44 -4 M58 6 L64 -4" stroke="${p.wood}" stroke-width="3" stroke-linecap="round"/>
    <path d="M44 -4 l-5 3 5 2 z M64 -4 l5 3 -5 2 z" fill="${p.flame||p.accent}"/>`};

  B.carroca = {w:104,h:72,draw:p=>`
    <rect x="20" y="26" width="64" height="26" rx="3" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M22 26 C28 4 76 4 82 26 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M34 12 V40 M52 8 V44 M70 12 V40" stroke="${OL}" stroke-width="1.4" opacity=".3"/>
    <circle cx="34" cy="58" r="11" fill="none" stroke="${OL}" stroke-width="${SW}"/><circle cx="34" cy="58" r="2.4" fill="${p.wood}"/>
    <circle cx="72" cy="58" r="11" fill="none" stroke="${OL}" stroke-width="${SW}"/><circle cx="72" cy="58" r="2.4" fill="${p.wood}"/>
    <g stroke="${OL}" stroke-width="1.4">${[0,60,120,180,240,300].map(a=>`<path d="M34 58 L${(34+9*Math.cos(a*Math.PI/180)).toFixed(1)} ${(58+9*Math.sin(a*Math.PI/180)).toFixed(1)}"/>`).join('')}</g>
    <path d="M84 34 L98 30" stroke="${p.wood}" stroke-width="3" stroke-linecap="round"/>`};

  B.circulo_fogueiras = {w:96,h:60,draw:p=>`
    <ellipse cx="48" cy="42" rx="42" ry="14" fill="none" stroke="${p.wood}" stroke-width="2" opacity=".4" stroke-dasharray="4 5"/>
    ${[[16,44,0.7],[48,30,0.6],[80,44,0.7],[48,52,0.8]].map(([x,y,s])=>`
      <g transform="translate(${x} ${y}) scale(${s})"><ellipse cx="0" cy="6" rx="11" ry="3.4" fill="${p.dark||'#2a221a'}"/>
      <path d="M-6 6 L6 -2 M6 6 L-6 -2" stroke="${p.wood}" stroke-width="3" stroke-linecap="round"/>
      <path d="M0 -14 C-5 -6 -2 0 0 4 C5 -2 6 -10 1 -16 C5 -10 5 -4 1 0 C8 -6 6 -16 0 -14Z" fill="${p.flame||'#E08A3A'}" stroke="${OL}" stroke-width="1.4"/></g>`).join('')}`};

  B.altar_portatil = {w:60,h:66,draw:p=>`
    <g stroke="${p.wood}" stroke-width="4" stroke-linecap="round"><path d="M16 64 L22 36 M44 64 L38 36"/></g>
    <rect x="14" y="30" width="32" height="10" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M30 30 C26 24 30 16 30 12 C32 16 36 24 30 30Z" fill="${p.flame||'#8C5BAA'}" stroke="${OL}" stroke-width="1.6"/>
    ${dot(30,18,2.6,p.accent)}
    <path d="M14 35 h32" stroke="${OL}" stroke-width="1" opacity=".3"/>
    <g stroke="${p.glyph||'#7C4DBE'}" stroke-width="1.2" opacity=".6"><path d="M18 46 h8 M34 46 h8"/></g>`};

  /* ============================================================
     EXPANSÃO TRIBAL (2º doc)
     ============================================================ */
  B.fumeiro = {w:62,h:78,draw:p=>`
    <path d="M14 74 C12 44 24 28 31 28 C40 28 50 44 48 74 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${OL}" stroke-width="1.4" opacity=".35" fill="none"><path d="M16 50 C28 44 36 44 46 50 M14 62 C28 56 36 56 48 62"/></g>
    <rect x="24" y="56" width="14" height="18" rx="2" fill="${p.dark||'#2a221a'}" stroke="${OL}" stroke-width="2"/>
    <g fill="${p.accent}" opacity=".7">${dot(28,24,2.4,p.accent)}${dot(34,18,2,p.accent)}${dot(31,12,1.6,p.accent)}</g>
    <path d="M20 50 q6 4 12 0 q6 4 12 0" stroke="${p.flame||'#E08A3A'}" stroke-width="1.6" fill="none"/>`};

  B.viveiro = {w:92,h:62,draw:p=>`
    <rect x="10" y="26" width="72" height="34" rx="3" fill="${p.dark||'#2a221a'}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${p.wood}" stroke-width="2" opacity=".85">${[10,22,34,46,58,70,82].map(x=>`<path d="M${x} 26 V60"/>`).join('')}<path d="M10 38 H82 M10 50 H82"/></g>
    <path d="M8 26 L46 14 L84 26 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <ellipse cx="30" cy="52" rx="8" ry="5" fill="${p.wall||'#C9A86A'}" stroke="${OL}" stroke-width="1.6"/>
    <ellipse cx="58" cy="52" rx="8" ry="5" fill="${p.wall||'#B59A6E'}" stroke="${OL}" stroke-width="1.6"/>`};

  B.campo_treino = {w:96,h:64,draw:p=>`
    <ellipse cx="48" cy="56" rx="44" ry="7" fill="${p.dark||'#4a3826'}" opacity=".3"/>
    <g stroke="${OL}" stroke-width="2" fill="${p.wood}">
      <rect x="14" y="20" width="6" height="38"/><circle cx="17" cy="16" r="6" fill="${p.wall||'#C9A86A'}"/>
      <rect x="44" y="14" width="6" height="44"/><path d="M47 20 h16" stroke="${p.wood}" stroke-width="4"/>
    </g>
    <rect x="62" y="18" width="14" height="14" rx="2" fill="${p.wall||'#C9A86A'}" stroke="${OL}" stroke-width="2"/>
    <circle cx="69" cy="25" r="4" fill="none" stroke="${p.flame||'#C0563A'}" stroke-width="2"/>
    <path d="M80 58 l6 -16" stroke="${p.wood}" stroke-width="2.4" stroke-linecap="round"/><path d="M86 42 l-4 4 m4 -4 l-1 5" stroke="${p.accent}" stroke-width="1.6"/>`};

  B.celeiro_palha = {w:80,h:72,draw:p=>`
    <path d="M14 68 C12 40 26 24 40 24 C54 24 68 40 66 68 Z" fill="${p.wall||'#D8B85A'}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${OL}" stroke-width="1.2" opacity=".3" fill="none"><path d="M18 40 C32 34 48 34 62 40 M16 52 C32 46 48 46 64 52 M16 62 C32 58 48 58 64 62"/></g>
    <rect x="32" y="50" width="16" height="18" rx="2" fill="${p.dark||'#5a4a30'}" stroke="${OL}" stroke-width="2"/>
    <path d="M40 24 q-3 -6 2 -10" stroke="${p.wood}" stroke-width="2.4" fill="none" stroke-linecap="round"/>`};

  B.jardim_medicinal = {w:90,h:56,draw:p=>`
    <rect x="8" y="34" width="74" height="18" rx="3" fill="${p.dark||'#3a2e1e'}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${p.wood}" stroke-width="2"><path d="M8 43 H82"/></g>
    ${[18,30,42,54,66,76].map((x,i)=>`<g><path d="M${x} 34 V22" stroke="${p.flame||'#6E8F4A'}" stroke-width="2" stroke-linecap="round"/>${dot(x,18,3.2,[p.accent,'#C9A0FF','#E0708A','#F0C46A','#5FE0C2','#E08A3A'][i])}<path d="M${x} 28 l-4 -2 M${x} 28 l4 -2" stroke="${p.flame||'#6E8F4A'}" stroke-width="1.6"/></g>`).join('')}`};

  B.vigia_arvore = {w:78,h:104,draw:p=>`
    <path d="M36 102 C32 70 38 44 40 18" stroke="${p.wood}" stroke-width="8" fill="none" stroke-linecap="round"/>
    <g fill="${p.flame||'#3C6E4A'}" stroke="${OL}" stroke-width="2"><circle cx="40" cy="20" r="16"/><circle cx="22" cy="30" r="10"/><circle cx="58" cy="30" r="10"/></g>
    <rect x="20" y="54" width="40" height="20" rx="2" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M16 54 L40 40 L64 54 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <g stroke="${p.wood}" stroke-width="2"><path d="M24 74 V96 M56 74 V96"/></g>
    <path d="M22 60 h36" stroke="${OL}" stroke-width="1.2" opacity=".3"/>
    <rect x="34" y="58" width="12" height="10" fill="${p.dark||'#2a221a'}" stroke="${OL}" stroke-width="1.6"/>`};

  B.doca_canoas = {w:98,h:54,draw:p=>`
    <rect x="6" y="26" width="58" height="7" rx="2" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${p.wood}" stroke-width="3" stroke-linecap="round">${[14,30,46,60].map(x=>`<path d="M${x} 33 V48"/>`).join('')}</g>
    <path d="M64 38 C70 50 96 50 92 38 Z" fill="${p.wall||'#8A6A3A'}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M68 40 H88" stroke="${OL}" stroke-width="1.4" opacity=".4"/>
    <path d="M78 40 L84 26" stroke="${p.wood}" stroke-width="2.4" stroke-linecap="round"/>
    <path d="M2 48 q8 -4 14 0 q8 -4 14 0" stroke="${p.accent}" stroke-width="1.6" fill="none" opacity=".5"/>`};

  /* register vinheta-friendly aliases used later */
  window.GeneseBuildings = G;
})();

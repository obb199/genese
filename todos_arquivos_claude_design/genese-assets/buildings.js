/* ============================================================
   GÊNESE — Construções modulares (vetor, tintáveis)
   Por estágio sociopolítico + paleta cultural. Cada estrutura é
   um sprite isolado, base-center em (w/2,h). Espelha AA §4 / GDD §5.
   ============================================================ */
(function(){
  const OL = '#23201c', SW = 2.6;
  const B = {};

  /* ---- P0: bando ---- */
  B.fogueira = {w:72,h:62,draw:p=>`
    <ellipse cx="36" cy="56" rx="30" ry="8" fill="${p.stone}" opacity=".35"/>
    <g stroke="${OL}" stroke-width="${SW}">
      ${[6,20,34,48,62].map(x=>`<ellipse cx="${x}" cy="54" rx="7" ry="5" fill="${p.stone}"/>`).join('')}
    </g>
    <path d="M22 54 L50 44 M50 54 L22 44" stroke="${p.wood}" stroke-width="5" stroke-linecap="round"/>
    <path d="M36 14 C26 26 30 36 36 46 C44 38 46 28 40 18 C44 26 44 34 40 42 C50 36 50 22 36 14Z" fill="${p.flame||'#E08A3A'}" stroke="${OL}" stroke-width="2" stroke-linejoin="round"/>
    <path d="M36 28 C32 34 34 40 37 44 C41 40 41 34 37 30Z" fill="${p.flame2||'#F0C46A'}"/>`};

  B.tenda = {w:78,h:72,draw:p=>`
    <path d="M39 6 L72 66 H6 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M39 6 L26 66 M39 6 L52 66" stroke="${OL}" stroke-width="1.6" opacity=".4"/>
    <path d="M39 66 L33 30 L45 30 Z" fill="${p.dark||'#2a2420'}" stroke="${OL}" stroke-width="2"/>
    <path d="M35 6 L30 -2 M43 6 L48 -2" stroke="${p.wood}" stroke-width="3" stroke-linecap="round"/>`};

  /* ---- tribal ---- */
  B.choca = {w:86,h:68,draw:p=>`
    <path d="M8 64 C6 36 26 18 43 18 C60 18 80 36 78 64 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M14 40 C24 32 62 32 72 40 M11 52 C26 46 60 46 75 52" stroke="${OL}" stroke-width="1.6" fill="none" opacity=".4"/>
    <path d="M31 64 C31 48 55 48 55 64 Z" fill="${p.dark||'#241c16'}" stroke="${OL}" stroke-width="2"/>
    <path d="M43 18 C40 10 46 8 48 4" stroke="${p.wood}" stroke-width="3" stroke-linecap="round" fill="none"/>`};

  B.totem = {w:42,h:94,draw:p=>`
    <rect x="14" y="20" width="14" height="70" rx="3" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${OL}" stroke-width="2" fill="none">
      <circle cx="21" cy="34" r="5" fill="${p.accent}"/><path d="M17 34h8"/>
      <path d="M16 54 L26 54 L24 62 L18 62 Z" fill="${p.wall}"/>
      <circle cx="21" cy="76" r="4.5" fill="${p.flame||'#E08A3A'}"/>
    </g>
    <path d="M8 22 L34 22 L28 10 L14 10 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>`};

  /* ---- aldeia ---- */
  B.casa_madeira = {w:88,h:80,draw:p=>`
    <rect x="16" y="38" width="56" height="40" rx="3" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M8 40 L44 10 L80 40 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M16 50 H72 M16 62 H72" stroke="${OL}" stroke-width="1.4" opacity=".3"/>
    <rect x="36" y="54" width="16" height="24" rx="2" fill="${p.dark||'#3a2c20'}" stroke="${OL}" stroke-width="2"/>
    <rect x="22" y="48" width="11" height="11" rx="2" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>
    <path d="M44 10 L44 4" stroke="${p.wood}" stroke-width="3" stroke-linecap="round"/>`};

  B.celeiro = {w:80,h:80,draw:p=>`
    <path d="M14 30 C14 18 66 18 66 30 L66 70 C66 78 14 78 14 70 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}"/>
    <ellipse cx="40" cy="30" rx="26" ry="9" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M14 44 C40 50 40 50 66 44 M14 58 C40 64 40 64 66 58" stroke="${OL}" stroke-width="1.4" fill="none" opacity=".3"/>
    <rect x="32" y="56" width="16" height="20" rx="2" fill="${p.dark||'#5a4a30'}" stroke="${OL}" stroke-width="2"/>`};

  /* ---- estado ---- */
  B.muro = {w:94,h:56,draw:p=>`
    <rect x="6" y="22" width="82" height="32" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[6,22,38,54,70].map(x=>`<rect x="${x}" y="14" width="12" height="10" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>`).join('')}
    <path d="M40 54 L40 34 C40 26 54 26 54 34 L54 54Z" fill="${p.dark||'#3a342c'}" stroke="${OL}" stroke-width="2"/>
    <path d="M6 36 H88 M24 22 V54 M62 22 V54" stroke="${OL}" stroke-width="1.2" opacity=".3"/>`};

  B.obelisco = {w:46,h:102,draw:p=>`
    <path d="M16 96 L30 96 L26 18 L20 18 Z" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M20 18 L23 6 L26 18 Z" fill="${p.accent}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="10" y="96" width="26" height="6" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <g stroke="${p.glyph||'#7C4DBE'}" stroke-width="1.6" fill="none" opacity=".8">
      <path d="M22 34 h4 M23 42 l2 4 M22 54 h4 M24 62 v5 M22 74 h4"/>
    </g>`};

  /* ---- culto / templos ---- */
  B.altar = {w:70,h:58,draw:p=>`
    <rect x="14" y="38" width="42" height="18" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <rect x="10" y="32" width="50" height="8" rx="2" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M35 32 C28 26 30 18 35 12 C40 18 42 26 35 32Z" fill="${p.flame||'#7C4DBE'}" stroke="${OL}" stroke-width="2" stroke-linejoin="round"/>
    <circle cx="35" cy="20" r="3" fill="${p.accent}"/>
    <path d="M20 56 V40 M50 56 V40" stroke="${OL}" stroke-width="1.4" opacity=".3"/>`};

  B.templo = {w:104,h:94,draw:p=>`
    <path d="M52 8 L96 38 H8 Z" fill="${p.roof}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <rect x="14" y="38" width="76" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    ${[18,34,50,66,82].map(x=>`<rect x="${x}" y="46" width="8" height="36" fill="${p.wall}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <rect x="10" y="82" width="84" height="8" fill="${p.stone}" stroke="${OL}" stroke-width="${SW}"/>
    <path d="M52 8 L52 0" stroke="${p.accent}" stroke-width="3" stroke-linecap="round"/>
    <circle cx="52" cy="26" r="4" fill="${p.accent}" stroke="${OL}" stroke-width="2"/>`};

  /* ---- costa ---- */
  B.doca = {w:96,h:58,draw:p=>`
    <rect x="6" y="30" width="64" height="8" rx="2" fill="${p.wood}" stroke="${OL}" stroke-width="${SW}"/>
    ${[12,28,44,60].map(x=>`<rect x="${x}" y="38" width="5" height="16" fill="${p.wood}" stroke="${OL}" stroke-width="2"/>`).join('')}
    <path d="M70 34 C74 26 92 26 94 34 L88 42 H76 Z" fill="${p.wall}" stroke="${OL}" stroke-width="${SW}" stroke-linejoin="round"/>
    <path d="M82 34 V18 L92 28 Z" fill="${p.accent}" stroke="${OL}" stroke-width="2" stroke-linejoin="round"/>
    <path d="M82 34 V20" stroke="${OL}" stroke-width="2"/>`};

  /* ---- estágios + paletas culturais ---- */
  const STAGES = [
    {id:'bando',  nm:'Bando (P0)',        items:['fogueira','tenda']},
    {id:'tribal', nm:'Assentamento tribal', items:['choca','totem']},
    {id:'aldeia', nm:'Aldeia / Cidade',   items:['casa_madeira','celeiro']},
    {id:'estado', nm:'Estruturas de estado', items:['muro','obelisco']},
    {id:'culto',  nm:'Culto & Templos',   items:['altar','templo']},
    {id:'costa',  nm:'Costa & Mar',       items:['doca']}
  ];
  const CULTURES = {
    floresta:{nm:'Cultura da Floresta', wall:'#7C6A48', roof:'#3C6E4A', wood:'#5A4630', stone:'#7E8478', accent:'#C9A04A', flame:'#5FE0C2', flame2:'#A8F0DC', dark:'#2c241c', glyph:'#2F6F62'},
    ordem:{nm:'Cultura da Ordem', wall:'#C8BBA0', roof:'#9A5A4A', wood:'#8A6A4A', stone:'#B6AC96', accent:'#8C5BAA', flame:'#8C5BAA', flame2:'#C9A0FF', dark:'#3a342c', glyph:'#8C5BAA'},
    arido:{nm:'Cultura Árida', wall:'#C9A86A', roof:'#A8843E', wood:'#8A6A3A', stone:'#B89A60', accent:'#C0563A', flame:'#E08A3A', flame2:'#F0C46A', dark:'#4a3826', glyph:'#C0563A'}
  };

  function buildingSVG(key, pal, scale){
    const d=B[key]; if(!d) return ''; const s=scale||1;
    return `<svg viewBox="0 0 ${d.w} ${d.h}" width="${(d.w*s).toFixed(0)}" height="${(d.h*s).toFixed(0)}" xmlns="http://www.w3.org/2000/svg" stroke-linecap="round">${d.draw(pal||{})}</svg>`;
  }
  function place(key, x, baseY, scale, pal){
    const d=B[key]; if(!d) return ''; const s=scale||1;
    return `<g transform="translate(${(x-d.w/2*s).toFixed(1)} ${(baseY-d.h*s).toFixed(1)}) scale(${s})">${d.draw(pal||{})}</g>`;
  }
  // vinheta de aldeia: fogueira central + estruturas em volta (vetor preview)
  function village(cultureKey, w, h){
    const pal=CULTURES[cultureKey]||CULTURES.floresta; w=w||470; h=h||180;
    const gy=h-26;
    return `<svg viewBox="0 0 ${w} ${h}" width="100%" preserveAspectRatio="xMidYMax meet" xmlns="http://www.w3.org/2000/svg" style="display:block">
      <rect x="0" y="0" width="${w}" height="${h}" rx="12" fill="#231C30"/>
      <ellipse cx="${w/2}" cy="${gy}" rx="${w*0.46}" ry="26" fill="#2e2742"/>
      <radialGradient id="vg_${cultureKey}" cx="50%" cy="40%" r="42%"><stop offset="0" stop-color="${pal.flame}" stop-opacity=".4"/><stop offset="1" stop-color="${pal.flame}" stop-opacity="0"/></radialGradient>
      <ellipse cx="${w/2}" cy="${gy-30}" rx="${w*0.4}" ry="60" fill="url(#vg_${cultureKey})"/>
      ${place('choca', w*0.18, gy, 0.9, pal)}
      ${place('tenda', w*0.78, gy-2, 0.82, pal)}
      ${place('totem', w*0.64, gy, 0.7, pal)}
      ${place('fogueira', w*0.46, gy+4, 1.0, pal)}
      ${place('casa_madeira', w*0.85, gy, 0.62, pal)}
    </svg>`;
  }

  window.GeneseBuildings = { PARTS:B, STAGES, CULTURES, buildingSVG, place, village };
})();

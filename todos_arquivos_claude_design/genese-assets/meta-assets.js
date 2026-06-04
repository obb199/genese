/* ============================================================
   GÊNESE — Meta-assets: glifos procedurais, logotipo, molduras,
   efeitos. v0.2
   ============================================================ */
(function(){
  /* ---------- PRNG determinístico ---------- */
  function prng(seed){ let s=(seed*2654435761)%2147483647; if(s<=0)s+=2147483646; return ()=> (s=s*16807%2147483647)/2147483647; }

  /* ---------- Glifos procedurais ----------
     Grade 3×3 de nós; o estilo define como os traços conectam. */
  const NODES = (()=>{ const xs=[9,18,27], ys=[9,22,35], a=[]; ys.forEach(y=>xs.forEach(x=>a.push([x,y]))); return a; })();
  const STYLES = ['angular','organic','radial','runic','cuneiform','spiral'];
  const STYLE_SEED = {angular:100,organic:200,radial:300,runic:400,cuneiform:500,spiral:600};

  function glyph(style, seed){
    const r = prng(seed + (STYLE_SEED[style]||0));
    const pick = ()=> Math.floor(r()*9);
    let used = new Set(), parts=[];

    if(style==='runic'){
      // haste vertical + 2..4 ramos diagonais
      const stemX = 12 + Math.floor(r()*12);
      parts.push(`<path d="M${stemX} 6 L${stemX} 38"/>`);
      const nb = 2 + Math.floor(r()*3);
      for(let i=0;i<nb;i++){ const y=10+Math.floor(r()*24), dir=r()>0.5?1:-1, len=6+Math.floor(r()*8);
        parts.push(`<path d="M${stemX} ${y} l${dir*len} ${(r()>0.5?1:-1)*(4+Math.floor(r()*6))}"/>`); }
      if(r()>0.5) parts.push(`<circle cx="${stemX}" cy="${r()>0.5?6:38}" r="1.7" fill="currentColor" stroke="none"/>`);
      return parts.join('');
    }
    if(style==='cuneiform'){
      // cunhas (triângulos) em posições da grade
      const n = 3 + Math.floor(r()*3);
      for(let i=0;i<n;i++){ const [x,y]=NODES[pick()]; const dir=Math.floor(r()*4);
        const w=3.4, h=6.5; let d;
        if(dir===0) d=`M${x} ${y} l${w} ${h} l${-w*2} 0 Z`;
        else if(dir===1) d=`M${x} ${y} l${w} ${-h} l${-w*2} 0 Z`;
        else if(dir===2) d=`M${x} ${y} l${h} ${w} l0 ${-w*2} Z`;
        else d=`M${x} ${y} l${-h} ${w} l0 ${-w*2} Z`;
        parts.push(`<path d="${d}" fill="currentColor" stroke="none"/>`); }
      return parts.join('');
    }
    if(style==='spiral'){
      // espiral + traço de saída
      const cx2=18, cy2=22, turns=0.7+r()*0.7, rmax=8+r()*5, steps=20; let d='';
      for(let i=0;i<=steps;i++){ const t=i/steps, a=t*turns*2*Math.PI, rr=2+(rmax-2)*t;
        d+=(i?'L':'M')+(cx2+Math.cos(a)*rr).toFixed(1)+' '+(cy2+Math.sin(a)*rr*0.9).toFixed(1); }
      parts.push(`<path d="${d}"/>`);
      const ex=cx2+Math.cos(turns*2*Math.PI)*rmax, ey=cy2+Math.sin(turns*2*Math.PI)*rmax*0.9;
      parts.push(`<path d="M${ex.toFixed(1)} ${ey.toFixed(1)} l${(r()>0.5?7:-7)} ${(r()>0.5?-8:8)}"/>`);
      parts.push(`<circle cx="${cx2}" cy="${cy2}" r="1.6" fill="currentColor" stroke="none"/>`);
      return parts.join('');
    }

    // angular / organic / radial — conexões entre nós
    const nStroke = 2 + Math.floor(r()*3);
    for(let i=0;i<nStroke;i++){
      let a=pick(), b=pick(); if(b===a) b=(b+1+Math.floor(r()*7))%9;
      used.add(a); used.add(b);
      const [x1,y1]=NODES[a], [x2,y2]=NODES[b];
      if(style==='angular'){
        if(r()>0.6){ const [mx,my]=NODES[pick()]; parts.push(`<path d="M${x1} ${y1} L${mx} ${my} L${x2} ${y2}"/>`); }
        else parts.push(`<path d="M${x1} ${y1} L${x2} ${y2}"/>`);
      } else if(style==='organic'){
        const mx=(x1+x2)/2 + (r()-0.5)*16, my=(y1+y2)/2 + (r()-0.5)*16;
        parts.push(`<path d="M${x1} ${y1} Q${mx.toFixed(1)} ${my.toFixed(1)} ${x2} ${y2}"/>`);
      } else { // radial
        parts.push(`<path d="M18 22 L${x1} ${y1}"/>`);
        if(r()>0.4) parts.push(`<path d="M${x1} ${y1} A6 6 0 0 1 ${x2} ${y2}"/>`);
      }
    }
    [...used].slice(0,2).forEach(n=>{ const [x,y]=NODES[n]; parts.push(`<circle cx="${x}" cy="${y}" r="1.6" fill="currentColor" stroke="none"/>`); });
    return parts.join('');
  }
  function glyphSVG(style, seed, size){
    return `<svg viewBox="0 0 36 44" width="${size||36}" height="${size||44}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${glyph(style,seed)}</svg>`;
  }
  // uma "palavra" = fileira de glifos do mesmo estilo/cultura
  function word(style, seed, n){
    let out=''; for(let i=0;i<(n||4);i++) out+=`<span class="gl">${glyphSVG(style, seed*31+i*7, 30)}</span>`;
    return `<span class="glword">${out}</span>`;
  }

  /* ---------- Logotipo / marca ----------
     Marca = "a semente que ramifica": uma espiral nasce de um ponto
     (criação) e termina numa bifurcação que brota (evolução / divergência). */
  function mark(size, color){
    const c=color||'currentColor', cx=32, cy=32;
    const turns=1.32, rMax=19, steps=72, start=-Math.PI/2;
    let d='', ex=cx, ey=cy, pang=start;
    for(let i=0;i<=steps;i++){ const t=i/steps, ang=start+turns*2*Math.PI*t, r=2.6+(rMax-2.6)*t;
      const x=cx+Math.cos(ang)*r, y=cy+Math.sin(ang)*r;
      d+=(i?'L':'M')+x.toFixed(2)+' '+y.toFixed(2);
      if(i===steps){ ex=x; ey=y; pang=ang; } }
    const tang=pang+Math.PI/2, bl=8.5;
    const x1=ex+Math.cos(tang-0.55)*bl, y1=ey+Math.sin(tang-0.55)*bl;
    const x2=ex+Math.cos(tang+0.55)*bl, y2=ey+Math.sin(tang+0.55)*bl;
    return `<svg viewBox="0 0 64 64" width="${size||64}" height="${size||64}" fill="none" stroke="${c}" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
      <path d="${d}"/>
      <path d="M${ex.toFixed(1)} ${ey.toFixed(1)} L${x1.toFixed(1)} ${y1.toFixed(1)}M${ex.toFixed(1)} ${ey.toFixed(1)} L${x2.toFixed(1)} ${y2.toFixed(1)}"/>
      <circle cx="${cx}" cy="${cy}" r="3.4" fill="${c}" stroke="none"/>
      <circle cx="${x1.toFixed(1)}" cy="${y1.toFixed(1)}" r="2.5" fill="${c}" stroke="none"/>
      <circle cx="${x2.toFixed(1)}" cy="${y2.toFixed(1)}" r="2.5" fill="${c}" stroke="none"/>
    </svg>`;
  }
  function wordmark(){
    return `<div class="logo-lockup">${mark(46,'var(--influencia)')}<span class="logo-word">GÊNESE</span></div>`;
  }

  /* ---------- Molduras de HUD (9-slice como SVG) ---------- */
  function panelFrame(w,h){
    w=w||220; h=h||96;
    return `<svg viewBox="0 0 ${w} ${h}" width="100%" preserveAspectRatio="none" fill="none">
      <rect x="3" y="3" width="${w-6}" height="${h-6}" rx="11" fill="#FCFAF4" stroke="#2B2926" stroke-width="3"/>
      <rect x="8" y="8" width="${w-16}" height="${h-16}" rx="7" fill="none" stroke="#B9B0A0" stroke-width="1.4"/>
      ${[[12,12],[w-12,12],[12,h-12],[w-12,h-12]].map(([x,y])=>`<circle cx="${x}" cy="${y}" r="3" fill="#8C5BAA"/>`).join('')}
    </svg>`;
  }
  function barFrame(){
    return `<svg viewBox="0 0 220 26" width="100%" fill="none">
      <rect x="2" y="2" width="216" height="22" rx="11" fill="#FCFAF4" stroke="#2B2926" stroke-width="3"/>
      <rect x="8" y="8" width="160" height="10" rx="5" fill="#fff" stroke="#2B2926" stroke-width="1.2"/>
      <rect x="8" y="8" width="96" height="10" rx="5" fill="#8C5BAA"/>
    </svg>`;
  }
  function roundBtn(active){
    return `<svg viewBox="0 0 56 56" width="56" height="56" fill="none">
      <circle cx="28" cy="28" r="24" fill="#FCFAF4" stroke="#2B2926" stroke-width="3"/>
      <circle cx="28" cy="28" r="20" fill="none" stroke="${active?'#8C5BAA':'#B9B0A0'}" stroke-width="1.4"/>
    </svg>`;
  }

  /* ---------- Efeitos & partículas ---------- */
  // pedra-coração / fogueira central (o ponto de vida da aldeia, visto nos concepts)
  function heartstone(color){
    const c=color||'#5FE0C2';
    return `<svg viewBox="0 0 80 90" width="80" height="90" fill="none" xmlns="http://www.w3.org/2000/svg">
      <defs><radialGradient id="hs" cx="50%" cy="42%" r="55%"><stop offset="0" stop-color="${c}" stop-opacity=".55"/><stop offset="1" stop-color="${c}" stop-opacity="0"/></radialGradient></defs>
      <ellipse cx="40" cy="44" rx="38" ry="40" fill="url(#hs)"/>
      <path d="M40 16 L52 42 L40 70 L28 42 Z" fill="${c}" stroke="#1F2933" stroke-width="2.5" stroke-linejoin="round"/>
      <path d="M40 16 L40 70 M28 42 L52 42" stroke="#1F2933" stroke-width="1.6" opacity=".5"/>
      <ellipse cx="40" cy="80" rx="20" ry="5" fill="#1F2933" opacity=".18"/>
    </svg>`;
  }
  // ondulação de nudge (toque do jogador)
  function nudgeRipple(color){
    const c=color||'#8C5BAA';
    return `<svg viewBox="0 0 80 80" width="80" height="80" fill="none">
      ${[20,14,8].map((r,i)=>`<circle cx="40" cy="40" r="${r+10}" stroke="${c}" stroke-width="${2.6-i*0.5}" opacity="${0.3+i*0.25}"/>`).join('')}
      <circle cx="40" cy="40" r="4" fill="${c}"/>
    </svg>`;
  }
  // pulso de sinal/emoção sobre a criatura
  function signalPulse(color){
    const c=color||'#C0563A';
    return `<svg viewBox="0 0 60 60" width="60" height="60" fill="none">
      <circle cx="30" cy="30" r="22" fill="${c}" opacity=".12"/>
      <circle cx="30" cy="30" r="22" stroke="${c}" stroke-width="2" stroke-dasharray="4 5" opacity=".7"/>
      <circle cx="30" cy="30" r="8" fill="${c}" opacity=".9"/>
    </svg>`;
  }
  // raio divino (a influência do observador descendo do céu)
  function lightBeam(color){
    const c=color||'#EAD37A';
    return `<svg viewBox="0 0 80 100" width="80" height="100" fill="none">
      <defs><linearGradient id="lb" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${c}" stop-opacity=".0"/><stop offset="1" stop-color="${c}" stop-opacity=".55"/></linearGradient></defs>
      <path d="M30 0 H50 L66 86 H14 Z" fill="url(#lb)"/>
      <ellipse cx="40" cy="86" rx="26" ry="7" fill="${c}" opacity=".35"/>
      <path d="M40 6 v74" stroke="${c}" stroke-width="1.2" opacity=".5"/>
    </svg>`;
  }
  // clima: chuva / neve / brasas (3 variantes do mesmo módulo)
  function weather(kind){
    const sets={
      rain:{c:'#6FA8C4', mk:(x,y)=>`<path d="M${x} ${y} l-2 7" stroke="#6FA8C4" stroke-width="2" stroke-linecap="round"/>`},
      snow:{c:'#CFE0DE', mk:(x,y)=>`<circle cx="${x}" cy="${y+3}" r="2" fill="#E7EFEC"/>`},
      ember:{c:'#E08A3A', mk:(x,y)=>`<circle cx="${x}" cy="${y}" r="1.8" fill="#E08A3A"/><circle cx="${x+1}" cy="${y-4}" r="1" fill="#F0C46A" opacity=".7"/>`}
    };
    const s=sets[kind]||sets.rain; const pts=[[14,8],[30,4],[46,12],[62,6],[22,24],[40,20],[56,26],[12,36],[34,40],[52,40],[66,32]];
    return `<svg viewBox="0 0 80 56" width="80" height="56" fill="none">${pts.map(([x,y])=>s.mk(x,y)).join('')}</svg>`;
  }

  window.GeneseMeta = { glyphSVG, word, mark, wordmark, panelFrame, barFrame, roundBtn,
    STYLES, heartstone, nudgeRipple, signalPulse, lightBeam, weather };
})();

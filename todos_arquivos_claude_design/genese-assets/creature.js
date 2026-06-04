/* ============================================================
   GÊNESE — Criatura paramétrica (forma-mãe modular) — v0.2
   Monta uma criatura a partir de genoma (cor, mistura de cor,
   forma, textura, tamanho, olhos, antenas, pernas, braços) +
   cultura (ornamentos) + estado (cor de sinal / postura).
   Tudo em SVG simples e exportável. Espelha DV §4 / GDD §8.3 / AA §2.
   ============================================================ */
(function(){
  let UID = 0;
  const RX = 38, CX = 60;

  /* ---- color helpers ---- */
  function hx(h){ h=h.replace('#',''); if(h.length===3)h=h.split('').map(c=>c+c).join(''); return [parseInt(h.slice(0,2),16),parseInt(h.slice(2,4),16),parseInt(h.slice(4,6),16)]; }
  function rgb(a){ return '#'+a.map(v=>Math.max(0,Math.min(255,Math.round(v))).toString(16).padStart(2,'0')).join(''); }
  function shade(hex,amt){ const a=hx(hex); return rgb(a.map(v=> amt<0 ? v*(1+amt) : v+(255-v)*amt )); }
  function mix(a,b,t){ const x=hx(a),y=hx(b); return rgb(x.map((v,i)=>v+(y[i]-v)*t)); }

  const DEFAULTS = {
    shape:'egg',        // egg | round | tall | squat | pear | bean | blob
    color:'#7FB29E',
    color2:'#5FA9B0',   // cor secundária (mistura)
    blend:'none',       // none | gradient | twotone | belly2 | dorsal | speckle2
    size:1,             // 0.7 .. 1.3
    pattern:'belly',    // none | belly | spots | stripes | scales | speckle | feathers | spines | fur | moss | mosaic
    eyes:'two',         // two | big | small | three | one | none | sleepy | compound | side
    pupil:'round',      // round | vertical | horizontal | star | none
    legs:'stubby',      // none | stubby | tall | back | webbed
    arms:'none',        // none | stubby | long | claws | flippers | tentacles | wings | double | fins
    antennae:'none',    // none | pair | single | horns | crest_bone | hood | mane
    ears:'none',        // none | pointy | round | drooping | tuft | fan | multiple
    snout:'none',       // none | long | flat | horn (nariz/focinho)
    mouth:'simple',     // simple | beak | tusks | trunk | suction | lips | none
    cheeks:'none',      // none | pouch | barbels | jowls (bochechas/queixo)
    neck:'none',        // none | short | long
    tail:'none',        // none | short | long | curl | tuft | fan | sting | fork
    finish:'matte',     // matte | satin | metallic | iridescent | pearl | mirror
    opacity:'solid',    // solid | translucent | crystal | ghost
    emit:'none',        // none | veins | body | eyes | trail
    asym:'none',        // none | eye | scar | burn
    temper:'neutro',    // neutro | curioso | timido | agressivo | preguicoso | brincalhao | paranoico
    ornament:'none',    // none | headband | facepaint | religious | crest | scar_ritual | piercing
    signal:'#8C5BAA',   // cor de sinal (emoção)
    glow:false,
    pose:'idle'         // idle | walk | forrage | alert
  };

  /* ---- shape geometry ---- */
  const SHAPE = {
    egg:  {ry:43, wTop:0.80, wMid:1.00, wBot:0.92, cy:80},
    round:{ry:39, wTop:1.00, wMid:1.03, wBot:1.00, cy:84},
    tall: {ry:50, wTop:0.72, wMid:0.90, wBot:0.84, cy:76},
    squat:{ry:31, wTop:0.94, wMid:1.12, wBot:1.04, cy:93},
    pear: {ry:44, wTop:0.50, wMid:0.84, wBot:1.08, cy:80},
    bean: {ry:44, wTop:0.84, wMid:0.96, wBot:0.92, cy:80, lean:7},
    blob: {ry:42, wMid:0.98, cy:82, wobble:true}
  };
  function metrics(shape){
    const s = SHAPE[shape]||SHAPE.egg;
    return {cx:CX, cy:s.cy, ry:s.ry, rx:RX*(s.wMid||1)};
  }
  function blobPath(cx,cy,rx,ry){
    const wob=[1,0.95,1.07,0.96,1.0,0.96,1.07,0.95], N=wob.length, p=[];
    for(let i=0;i<N;i++){ const a=-Math.PI/2 + i/N*2*Math.PI; p.push([cx+Math.cos(a)*rx*wob[i], cy+Math.sin(a)*ry*wob[i]]); }
    let d=`M${p[0][0].toFixed(1)} ${p[0][1].toFixed(1)}`;
    for(let i=0;i<N;i++){ const p0=p[(i-1+N)%N],p1=p[i],p2=p[(i+1)%N],p3=p[(i+2)%N];
      const c1=[p1[0]+(p2[0]-p0[0])/6, p1[1]+(p2[1]-p0[1])/6], c2=[p2[0]-(p3[0]-p1[0])/6, p2[1]-(p3[1]-p1[1])/6];
      d+=`C${c1[0].toFixed(1)} ${c1[1].toFixed(1)} ${c2[0].toFixed(1)} ${c2[1].toFixed(1)} ${p2[0].toFixed(1)} ${p2[1].toFixed(1)}`; }
    return d+'Z';
  }
  function bodyPath(shape){
    const s=SHAPE[shape]||SHAPE.egg, cx=CX, cy=s.cy, ry=s.ry;
    if(s.wobble) return blobPath(cx,cy,RX*s.wMid,ry);
    const top=cy-ry, bot=cy+ry, mid=cy, lean=s.lean||0;
    const wt=RX*s.wTop, wm=RX*s.wMid, wb=RX*s.wBot;
    return `M${cx+lean} ${top}`
      +`C${cx+lean+wt} ${top} ${cx+wm} ${mid-ry*0.5} ${cx+wm} ${mid}`
      +`C${cx+wm} ${mid+ry*0.5} ${cx+wb} ${bot} ${cx} ${bot}`
      +`C${cx-wb} ${bot} ${cx-wm} ${mid+ry*0.5} ${cx-wm} ${mid}`
      +`C${cx-wm} ${mid-ry*0.5} ${cx-lean-wt} ${top} ${cx+lean} ${top}Z`;
  }

  function pose(p){
    switch(p){
      case 'walk':    return {rot:-4, sx:1.0,  sy:0.98, footL:6,  footR:-4, dy:0};
      case 'forrage': return {rot:14, sx:1.02, sy:0.95, footL:0,  footR:0,  dy:6};
      case 'alert':   return {rot:0,  sx:0.93, sy:1.08, footL:-3, footR:3,  dy:-4};
      default:        return {rot:0,  sx:1,    sy:1,    footL:0,  footR:0,  dy:0};
    }
  }

  /* ---- limbs: feet/legs ---- */
  function feetSVG(o,m,dark,P){
    if(o.legs==='none') return '';
    const cx=m.cx, footY=m.cy+m.ry-2, rx=m.rx;
    if(o.legs==='tall'){
      const lh=11;
      return `<path d="M${cx-rx*0.42} ${footY-3} v${lh}" stroke="${dark}" stroke-width="5" stroke-linecap="round"/>`
        +`<path d="M${cx+rx*0.42} ${footY-3} v${lh}" stroke="${dark}" stroke-width="5" stroke-linecap="round"/>`
        +`<ellipse cx="${cx-rx*0.42+P.footL}" cy="${footY+lh}" rx="7" ry="4.5" fill="${dark}"/>`
        +`<ellipse cx="${cx+rx*0.42+P.footR}" cy="${footY+lh}" rx="7" ry="4.5" fill="${dark}"/>`;
    }
    if(o.legs==='back'){ // articuladas para trás (tipo ave)
      const W=`stroke="${dark}" fill="none" stroke-linecap="round" stroke-width="5"`;
      return [-1,1].map(s=>`<path d="M${cx+s*rx*0.4} ${footY-4} l${s*4} 9 l${s*-7} 8" ${W}/><path d="M${cx+s*rx*0.4-s*3} ${footY+13} l${s*8} 2 m${s*-8} -2 l${s*-3} 4" stroke="${dark}" stroke-width="2.6" fill="none" stroke-linecap="round"/>`).join('');
    }
    if(o.legs==='webbed'){
      return [-1,1].map(s=>`<path d="M${cx+s*rx*0.42} ${footY-2} v8" stroke="${dark}" stroke-width="4.5" stroke-linecap="round"/><path d="M${cx+s*rx*0.42-6} ${footY+6} q6 6 12 0 q0 4 -6 5 q-6 -1 -6 -5Z" fill="${dark}"/>`).join('');
    }
    return `<ellipse cx="${cx-rx*0.45+P.footL}" cy="${footY}" rx="7" ry="5" fill="${dark}"/>`
      +`<ellipse cx="${cx+rx*0.45+P.footR}" cy="${footY}" rx="7" ry="5" fill="${dark}"/>`;
  }

  /* ---- limbs: arms (mirrored per side) ---- */
  function armSVG(side,o,m,dark){
    if(o.arms==='none') return '';
    const s=side, ax=m.cx+s*m.rx*0.84, ay=m.cy+m.ry*0.02;
    const hand=(x,y,r)=>`<circle cx="${x}" cy="${y}" r="${r}" fill="${dark}"/>`;
    const W='stroke="'+dark+'" fill="none" stroke-linecap="round"';
    switch(o.arms){
      case 'stubby':    return `<path d="M${ax} ${ay} q${s*7} 3 ${s*9} 10" ${W} stroke-width="6"/>`+hand(ax+s*9,ay+10,4.5);
      case 'long':      return `<path d="M${ax} ${ay} q${s*15} 7 ${s*12} 23" ${W} stroke-width="5.5"/>`+hand(ax+s*12,ay+23,5);
      case 'claws':{    let cl=''; const ex=ax+s*11, ey=ay+13;
                        for(let k=-1;k<=1;k++) cl+=`<path d="M${ex} ${ey} l${s*5.5} ${5+k*4.5}" ${W} stroke-width="2.6"/>`;
                        return `<path d="M${ax} ${ay} q${s*10} 4 ${s*11} 13" ${W} stroke-width="5.5"/>`+cl; }
      case 'flippers':  return `<path d="M${ax} ${ay-2} q${s*19} 3 ${s*16} 21 q${s*-11} 2 ${s*-15} -7 Z" fill="${dark}"/>`;
      case 'tentacles': return `<path d="M${ax} ${ay} q${s*15} 6 ${s*8} 18 q${s*-7} 11 ${s*7} 17" ${W} stroke-width="4.5"/>`
                        +hand(ax+s*7+s*7,ay+35,2.6);
      case 'wings':     return `<path d="M${ax} ${ay-7} q${s*24} -7 ${s*26} 15 q${s*-13} -2 ${s*-24} 3 Z" fill="${dark}" opacity=".82"/>`;
      case 'double':    return `<path d="M${ax} ${ay-6} q${s*12} 4 ${s*11} 13" ${W} stroke-width="5"/>`+hand(ax+s*11,ay+7,4)
                        + `<path d="M${ax} ${ay+8} q${s*12} 5 ${s*10} 16" ${W} stroke-width="5"/>`+hand(ax+s*10,ay+24,4);
      case 'fins':      return `<path d="M${ax-s*2} ${ay-6} q${s*16} -2 ${s*20} 10 q${s*4} 8 ${s*-2} 14 q${s*-10} -6 ${s*-16} -16 Z" fill="${dark}" opacity=".7" stroke="${darker}" stroke-width="1.6" stroke-linejoin="round"/><g stroke="${darker}" stroke-width="1" opacity=".4"><path d="M${ax+s*2} ${ay} l${s*14} 8 M${ax+s*2} ${ay+6} l${s*12} 10"/></g>`;
    }
    return '';
  }

  /* ---- tail (drawn behind body) ---- */
  function tailSVG(o,m,dark,darker){
    if(o.tail==='none') return '';
    const bx=m.cx+m.rx*0.55, by=m.cy+m.ry*0.55;
    const W=`stroke="${dark}" fill="none" stroke-linecap="round"`;
    switch(o.tail){
      case 'short':  return `<path d="M${bx} ${by} q14 2 16 12" ${W} stroke-width="7"/>`;
      case 'long':   return `<path d="M${bx} ${by} q26 0 30 -18 q2 -10 -4 -16" ${W} stroke-width="6"/>`;
      case 'curl':   return `<path d="M${bx} ${by} q24 4 24 -12 q0 -12 -12 -12 q-9 0 -9 8 q0 6 6 6" ${W} stroke-width="5.5"/>`;
      case 'tuft':   return `<path d="M${bx} ${by} q22 2 26 -14" ${W} stroke-width="6"/>`
                          + `<g stroke="${dark}" stroke-width="3" stroke-linecap="round"><path d="M${bx+26} ${by-14} l8 -6 M${bx+26} ${by-14} l9 1 M${bx+26} ${by-14} l5 8"/></g>`;
      case 'fan':{   let f=''; for(let k=-2;k<=2;k++) f+=`<path d="M${bx+8} ${by-2} q${14+Math.abs(k)*2} ${k*8} ${20} ${k*16}" ${W} stroke-width="3"/><circle cx="${bx+28}" cy="${by-2+k*16}" r="4" fill="${o.signal}" stroke="${darker}" stroke-width="1.4"/>`;
                     return f; }
      case 'sting':  return `<path d="M${bx} ${by} q22 4 22 -14" ${W} stroke-width="6"/>`
                          + `<path d="M${bx+22} ${by-14} l-3 -10 6 4Z" fill="${darker}" stroke="${darker}" stroke-width="1"/>`;
      case 'fork':   return `<path d="M${bx} ${by} q18 2 24 -10" ${W} stroke-width="6"/>`
                          + `<path d="M${bx+24} ${by-10} l10 -8 M${bx+24} ${by-10} l10 2" ${W} stroke-width="4"/>`;
    }
    return '';
  }

  /* ---- ears (separate from antennae) ---- */
  function earsSVG(o,m,dark,darker){
    if(o.ears==='none') return '';
    const cx=m.cx, ty=m.cy-m.ry, ex=m.rx*0.62;
    const E=(s,inner)=>`<g transform="translate(${cx+s*ex} ${ty+6})">${inner}</g>`;
    switch(o.ears){
      case 'pointy':   return [-1,1].map(s=>E(s,`<path d="M0 6 l${s*4} -20 l${s*9} 14Z" fill="${o.color}" stroke="${darker}" stroke-width="2" stroke-linejoin="round"/>`)).join('');
      case 'round':    return [-1,1].map(s=>E(s,`<circle cx="${s*4}" cy="-6" r="9" fill="${o.color}" stroke="${darker}" stroke-width="2"/><circle cx="${s*4}" cy="-6" r="4" fill="${dark}"/>`)).join('');
      case 'drooping': return [-1,1].map(s=>E(s,`<path d="M0 0 q${s*6} 4 ${s*5} 26 q${s*-6} 4 ${s*-11} -4 Z" fill="${o.color}" stroke="${darker}" stroke-width="2" stroke-linejoin="round"/>`)).join('');
      case 'tuft':     return [-1,1].map(s=>E(s,`<path d="M0 4 l${s*3} -16 l${s*8} 12Z" fill="${o.color}" stroke="${darker}" stroke-width="2" stroke-linejoin="round"/><g stroke="${dark}" stroke-width="2" stroke-linecap="round"><path d="M${s*3} -12 l${s*1} -7 M${s*5} -10 l${s*4} -5"/></g>`)).join('');
      case 'fan':      return [-1,1].map(s=>E(s,`<path d="M0 4 q${s*22} -10 ${s*24} 6 q${s*-12} 6 ${s*-22} 2 Z" fill="${o.color}" stroke="${darker}" stroke-width="2" stroke-linejoin="round"/><g stroke="${darker}" stroke-width="1" opacity=".4"><path d="M${s*4} 2 l${s*16} 4 M${s*4} 6 l${s*14} 2"/></g>`)).join('');
      case 'multiple': return [-1,1].map(s=>[0,1,2].map(k=>E(s,`<path d="M${s*k*5} ${2+k*3} l${s*3} -${14-k*3} l${s*7} ${10-k*2}Z" fill="${o.color}" stroke="${darker}" stroke-width="1.8" stroke-linejoin="round"/>`)).join('')).join('');
    }
    return '';
  }

  /* ---- snout / nose ---- */
  function snoutSVG(o,m,dark,darker){
    if(o.snout==='none') return '';
    const cx=m.cx, my=m.cy-m.ry*0.04;
    switch(o.snout){
      case 'long': return `<path d="M${cx-7} ${my-4} q-14 6 -16 16 q8 6 18 2 q4 -8 -2 -18Z" fill="${dark}" stroke="${darker}" stroke-width="1.8" stroke-linejoin="round"/><circle cx="${cx-19}" cy="${my+12}" r="1.6" fill="${darker}"/>`;
      case 'flat': return `<ellipse cx="${cx}" cy="${my+4}" rx="13" ry="8" fill="${dark}" stroke="${darker}" stroke-width="1.8"/><circle cx="${cx-4}" cy="${my+4}" r="1.6" fill="${darker}"/><circle cx="${cx+4}" cy="${my+4}" r="1.6" fill="${darker}"/>`;
      case 'horn': return `<path d="M${cx} ${my-6} l-4 -22 q4 -4 8 0Z" fill="${m.light||dark}" stroke="${darker}" stroke-width="1.8" stroke-linejoin="round"/>`;
    }
    return '';
  }

  /* ---- neck (drawn behind body, lifts a small head knob) ---- */
  function neckSVG(o,m,dark,darker){
    if(o.neck==='none'||o.neck==='short') {
      if(o.neck==='short') return `<path d="M${m.cx-m.rx*0.4} ${m.cy-m.ry*0.78} q${m.rx*0.4} 6 ${m.rx*0.8} 0" stroke="${darker}" stroke-width="2" fill="none" opacity=".4"/>`;
      return '';
    }
    // long: a column rising above the body
    const cx=m.cx, ty=m.cy-m.ry;
    return `<path d="M${cx-9} ${ty+8} C${cx-11} ${ty-18} ${cx+11} ${ty-18} ${cx+9} ${ty+8} Z" fill="${dark}" stroke="${darker}" stroke-width="2"/>`
         + `<path d="M${cx-9} ${ty-2} q9 4 18 0 M${cx-8} ${ty-10} q8 3 16 0" stroke="${darker}" stroke-width="1.4" fill="none" opacity=".4"/>`;
  }

  /* ---- bochechas / queixo ---- */
  function cheeksSVG(o,m,dark,darker){
    if(o.cheeks==='none') return '';
    const cx=m.cx, my=m.cy-m.ry*0.04, by=m.cy+m.ry*0.18;
    switch(o.cheeks){
      case 'pouch':   return `<path d="M${cx-12} ${my+3} Q${cx} ${by+14} ${cx+12} ${my+3} Q${cx} ${my+10} ${cx-12} ${my+3}Z" fill="${mix(o.color,o.signal,0.35)}" stroke="${darker}" stroke-width="1.8" stroke-linejoin="round" opacity=".9"/>`;
      case 'barbels': return `<g stroke="${dark}" stroke-width="2.2" fill="none" stroke-linecap="round"><path d="M${cx-6} ${my+2} q-10 6 -12 16 M${cx+6} ${my+2} q10 6 12 16 M${cx-3} ${my+4} q-5 8 -5 14 M${cx+3} ${my+4} q5 8 5 14"/></g>`;
      case 'jowls':   return `<path d="M${cx-rx(m)*0.5} ${my} q-6 14 4 18 q6 2 8 -4 M${cx+rx(m)*0.5} ${my} q6 14 -4 18 q-6 2 -8 -4" stroke="${darker}" stroke-width="1.8" fill="${dark}" opacity=".5" stroke-linejoin="round"/>`;
    }
    return '';
  }
  function rx(m){ return m.rx; }

  /* ---- temperamento (expressão de repouso: sobrancelhas) ---- */
  function temperSVG(o,m){
    if(!o.temper||o.temper==='neutro') return '';
    if(o.eyes==='none'||o.eyes==='sleepy') return '';
    const cx=m.cx, ry=m.ry, by=m.cy-ry*0.30-8, ex=m.rx*0.32;
    const C='stroke="#23202b" stroke-width="2" fill="none" stroke-linecap="round"';
    const brow=(s,d)=>`<path d="${d}" ${C}/>`;
    switch(o.temper){
      case 'curioso':    return [-1,1].map(s=>brow(s,`M${cx+s*ex-5} ${by+1} q5 -4 10 -1`)).join('');
      case 'timido':     return [-1,1].map(s=>brow(s,`M${cx+s*ex-5} ${by-1} q5 2 10 0`)).join('');
      case 'agressivo':  return [brow(-1,`M${cx-ex-5} ${by-2} l11 5`), brow(1,`M${cx+ex+5} ${by-2} l-11 5`)].join('');
      case 'preguicoso': return [-1,1].map(s=>brow(s,`M${cx+s*ex-6} ${by+3} h12`)).join('');
      case 'brincalhao': return [brow(-1,`M${cx-ex-5} ${by+1} q5 -5 10 -2`), brow(1,`M${cx+ex-5} ${by-1} h10`)].join('');
      case 'paranoico':  return [-1,1].map(s=>brow(s,`M${cx+s*ex-6} ${by-3} q6 -3 12 0`)).join('')
                         + `<circle cx="${cx}" cy="${by-9}" r="1.4" fill="#23202b" opacity=".5"/>`;
    }
    return '';
  }

  function build(opts){
    const o = Object.assign({}, DEFAULTS, opts||{});
    const id = 'cr'+(++UID);
    const m = metrics(o.shape);
    const cx=m.cx, cy=m.cy, rx=m.rx, ry=m.ry;
    const P = pose(o.pose);
    const dark  = shade(o.color,-0.34);
    const darker= shade(o.color,-0.5);
    const light = shade(o.color, 0.38);
    const eyeY  = cy - ry*0.30;
    const bp = bodyPath(o.shape);

    /* ---- color mixing / fills ---- */
    let defs = `<clipPath id="${id}c"><path d="${bp}"/></clipPath>`;
    let bodyFill = o.color, extra='';
    if(o.blend==='gradient'){
      defs += `<linearGradient id="${id}g" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${o.color}"/><stop offset="1" stop-color="${o.color2}"/></linearGradient>`;
      bodyFill = `url(#${id}g)`;
    } else if(o.blend==='twotone'){
      defs += `<linearGradient id="${id}g" x1="0" y1="0" x2="0" y2="1"><stop offset="0.46" stop-color="${o.color}"/><stop offset="0.54" stop-color="${o.color2}"/></linearGradient>`;
      bodyFill = `url(#${id}g)`;
    } else if(o.blend==='belly2'){
      extra += `<ellipse cx="${cx}" cy="${cy+ry*0.30}" rx="${rx*0.62}" ry="${ry*0.52}" fill="${o.color2}"/>`;
    } else if(o.blend==='dorsal'){
      extra += `<path d="M${cx-rx} ${cy-ry*0.1} Q${cx} ${cy-ry*1.05} ${cx+rx} ${cy-ry*0.1} Q${cx} ${cy-ry*0.35} ${cx-rx} ${cy-ry*0.1}Z" fill="${o.color2}"/>`;
    } else if(o.blend==='speckle2'){
      const pts=[[-0.45,-0.2],[0.3,0.1],[-0.15,0.4],[0.2,-0.35],[-0.35,0.32],[0.42,0.42],[0,-0.05]];
      extra += pts.map(([dx,dy])=>`<circle cx="${cx+dx*rx}" cy="${cy+dy*ry}" r="${rx*0.12}" fill="${o.color2}"/>`).join('');
    }

    /* ---- finish / brilho ---- */
    let finishOverlay='';
    if(o.finish==='metallic'){
      defs += `<linearGradient id="${id}mf" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="${shade(o.color,0.5)}"/><stop offset="0.45" stop-color="${o.color}"/><stop offset="0.6" stop-color="${shade(o.color,0.3)}"/><stop offset="1" stop-color="${shade(o.color,-0.4)}"/></linearGradient>`;
      bodyFill = `url(#${id}mf)`;
      finishOverlay += `<path d="M${cx-rx*0.7} ${cy-ry*0.6} L${cx-rx*0.2} ${cy+ry*0.6} L${cx-rx*0.45} ${cy+ry*0.6} L${cx-rx*0.95} ${cy-ry*0.4}Z" fill="#fff" opacity=".22"/>`;
    } else if(o.finish==='iridescent'){
      defs += `<linearGradient id="${id}ir" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="${mix(o.color,'#7FD8E0',0.5)}"/><stop offset="0.5" stop-color="${mix(o.color,'#C9A0FF',0.45)}"/><stop offset="1" stop-color="${mix(o.color,'#E0A8C8',0.5)}"/></linearGradient>`;
      finishOverlay += `<path d="${bp}" fill="url(#${id}ir)" opacity=".5"/>`;
    } else if(o.finish==='pearl'){
      defs += `<radialGradient id="${id}pl" cx="40%" cy="32%" r="62%"><stop offset="0" stop-color="#fff" stop-opacity=".6"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></radialGradient>`;
      finishOverlay += `<path d="${bp}" fill="url(#${id}pl)"/>`;
    } else if(o.finish==='satin'){
      finishOverlay += `<ellipse cx="${cx-rx*0.3}" cy="${cy-ry*0.4}" rx="${rx*0.5}" ry="${ry*0.34}" fill="#fff" opacity=".16"/>`;
    } else if(o.finish==='mirror'){
      defs += `<linearGradient id="${id}mr" x1="0" y1="0" x2="0.6" y2="1"><stop offset="0" stop-color="#fff"/><stop offset="0.35" stop-color="${shade(o.color,0.4)}"/><stop offset="0.55" stop-color="${o.color}"/><stop offset="0.75" stop-color="${shade(o.color,0.5)}"/><stop offset="1" stop-color="${shade(o.color,-0.3)}"/></linearGradient>`;
      bodyFill = `url(#${id}mr)`;
      finishOverlay += `<path d="M${cx-rx*0.6} ${cy-ry*0.5} q${rx*0.4} ${ry*0.3} 0 ${ry} q${-rx*0.25} ${-ry*0.4} 0 ${-ry}Z" fill="#fff" opacity=".35"/>`;
    }

    /* ---- transparência ---- */
    let bodyOpacity=1;
    if(o.opacity==='translucent'){ bodyOpacity=0.6; finishOverlay+=`<ellipse cx="${cx}" cy="${cy+ry*0.18}" rx="${rx*0.4}" ry="${ry*0.34}" fill="${darker}" opacity=".25"/>`; }
    else if(o.opacity==='crystal'){ bodyOpacity=0.5; finishOverlay+=`<g stroke="#fff" stroke-width="1" opacity=".4" fill="none"><path d="M${cx} ${cy-ry} L${cx-rx*0.5} ${cy+ry*0.4} M${cx} ${cy-ry} L${cx+rx*0.5} ${cy+ry*0.4} M${cx-rx*0.6} ${cy} L${cx+rx*0.6} ${cy*1.0}"/></g>`; }
    else if(o.opacity==='ghost'){ defs += `<linearGradient id="${id}gh" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${o.color}" stop-opacity=".55"/><stop offset="1" stop-color="${o.color}" stop-opacity=".05"/></linearGradient>`; bodyFill=`url(#${id}gh)`; bodyOpacity=1; }

    /* ---- emissão de luz ---- */
    let emitLayer='';
    if(o.emit==='veins'){
      emitLayer = `<g stroke="${o.signal}" stroke-width="1.8" fill="none" stroke-linecap="round" opacity=".85"><path d="M${cx} ${cy-ry*0.6} q-${rx*0.4} ${ry*0.4} -${rx*0.2} ${ry*0.9} M${cx} ${cy-ry*0.4} q${rx*0.4} ${ry*0.3} ${rx*0.3} ${ry*0.8} M${cx-rx*0.2} ${cy} q-${rx*0.3} ${ry*0.2} -${rx*0.5} ${ry*0.1}"/></g>`;
    } else if(o.emit==='body'){
      defs += `<radialGradient id="${id}em" cx="50%" cy="50%" r="55%"><stop offset="0" stop-color="${o.signal}" stop-opacity=".55"/><stop offset="1" stop-color="${o.signal}" stop-opacity="0"/></radialGradient>`;
      emitLayer = `<ellipse cx="${cx}" cy="${cy}" rx="${rx*0.9}" ry="${ry*0.9}" fill="url(#${id}em)"/>`;
    } else if(o.emit==='trail'){
      emitLayer = `<g fill="${o.signal}">${[0.9,0.65,0.4].map((op,i)=>`<ellipse cx="${cx+rx*0.7+i*rx*0.5}" cy="${cy+ry*0.2}" rx="${(rx*0.5*(1-i*0.22)).toFixed(1)}" ry="${(ry*0.5*(1-i*0.22)).toFixed(1)}" opacity="${(0.28*op).toFixed(2)}"/>`).join('')}</g>`;
    }
    const emitEyes = (o.emit==='eyes');

    /* ---- pattern overlay (clipped) ---- */
    let pat = '';
    if(o.pattern==='spots'){
      const pts=[[-0.4,-0.1],[0.35,0.15],[-0.1,0.45],[0.15,-0.35],[-0.35,0.4]];
      pat = pts.map(([dx,dy])=>`<circle cx="${cx+dx*rx}" cy="${cy+dy*ry}" r="${rx*0.16}" fill="${dark}"/>`).join('');
    } else if(o.pattern==='stripes'){
      pat = [0.55,0.15,-0.25].map(f=>`<rect x="${cx-rx}" y="${cy+f*ry}" width="${rx*2}" height="${ry*0.16}" fill="${dark}" opacity=".8"/>`).join('');
    } else if(o.pattern==='belly'){
      pat = `<ellipse cx="${cx}" cy="${cy+ry*0.28}" rx="${rx*0.6}" ry="${ry*0.5}" fill="${light}"/>`;
    } else if(o.pattern==='scales'){
      let rows=''; for(let r=0;r<3;r++){ const yy=cy-ry*0.2+r*ry*0.34; for(let c=-1;c<=1;c++){ const xx=cx+c*rx*0.5+(r%2?rx*0.25:0); rows+=`<path d="M${xx-rx*0.22} ${yy} a${rx*0.22} ${rx*0.2} 0 0 1 ${rx*0.44} 0" stroke="${dark}" stroke-width="1.6" fill="none"/>`; } }
      pat = rows;
    } else if(o.pattern==='speckle'){
      const pts=[[-0.5,-0.15],[-0.2,0.2],[0.1,-0.25],[0.35,0.3],[-0.32,0.45],[0.42,-0.05],[0.05,0.5],[-0.1,-0.45]];
      pat = pts.map(([dx,dy])=>`<circle cx="${cx+dx*rx}" cy="${cy+dy*ry}" r="${rx*0.07}" fill="${dark}"/>`).join('');
    } else if(o.pattern==='feathers'){
      let f=''; for(let r=0;r<3;r++){ const yy=cy-ry*0.35+r*ry*0.4; for(let c=-2;c<=2;c++){ const xx=cx+c*rx*0.36+(r%2?rx*0.18:0); f+=`<path d="M${xx} ${yy} q${rx*0.18} ${ry*0.18} 0 ${ry*0.3} q${-rx*0.18} ${-ry*0.12} 0 ${-ry*0.3}Z" fill="${dark}" opacity=".55"/>`; } } pat=f;
    } else if(o.pattern==='spines'){
      let f=''; for(let i=-2;i<=2;i++){ const xx=cx+i*rx*0.34; f+=`<path d="M${xx} ${cy-ry*0.6} l-3 12 6 0Z" fill="${dark}"/>`; } pat=f;
    } else if(o.pattern==='fur'){
      const seedPts=[[-0.6,0.1],[-0.4,0.4],[-0.2,-0.2],[0,0.3],[0.2,-0.1],[0.4,0.35],[0.6,0.05],[-0.5,-0.3],[0.1,-0.4],[0.5,-0.3],[-0.1,0.55],[0.3,0.5]];
      pat = seedPts.map(([dx,dy])=>`<path d="M${cx+dx*rx} ${cy+dy*ry} l3 8 l3 -8" stroke="${dark}" stroke-width="1.6" fill="none" stroke-linecap="round" opacity=".6"/>`).join('');
    } else if(o.pattern==='moss'){
      const pts=[[-0.45,-0.1],[0.3,0.15],[-0.1,0.45],[0.2,-0.3],[-0.3,0.35],[0.4,0.4],[0.45,-0.1],[-0.5,0.2]];
      pat = pts.map(([dx,dy])=>`<circle cx="${cx+dx*rx}" cy="${cy+dy*ry}" r="${rx*0.13}" fill="${mix(o.color,'#4A7A3A',0.6)}" opacity=".7"/>`).join('');
    } else if(o.pattern==='mosaic'){
      let f=''; for(let r=0;r<3;r++){ const yy=cy-ry*0.4+r*ry*0.36; for(let c=-2;c<=2;c++){ const xx=cx+c*rx*0.36; f+=`<rect x="${xx-rx*0.14}" y="${yy}" width="${rx*0.26}" height="${ry*0.26}" fill="${(r+c)%2?dark:light}" opacity=".5"/>`; } } pat=f;
    } else if(o.pattern==='bumps'){
      const pts=[[-0.4,-0.05],[0.3,0.1],[-0.1,0.4],[0.2,-0.3],[-0.35,0.35],[0.4,0.4],[0.05,0.05]];
      pat = pts.map(([dx,dy])=>`<circle cx="${cx+dx*rx}" cy="${cy+dy*ry}" r="${rx*0.13}" fill="${light}" stroke="${dark}" stroke-width="1.2"/><circle cx="${cx+dx*rx-rx*0.04}" cy="${cy+dy*ry-ry*0.03}" r="${rx*0.04}" fill="#fff" opacity=".5"/>`).join('');
    } else if(o.pattern==='needles'){
      let f=''; for(let r=0;r<3;r++){ const yy=cy-ry*0.4+r*ry*0.32; for(let c=-2;c<=2;c++){ const xx=cx+c*rx*0.34+(r%2?rx*0.17:0); f+=`<path d="M${xx} ${yy} l-1.5 ${ry*0.16} 3 0Z" fill="${dark}"/>`; } } pat=f;
    } else if(o.pattern==='plates'){
      let f=''; for(let r=0;r<3;r++){ const yy=cy-ry*0.42+r*ry*0.34; for(let c=-1;c<=1;c++){ const xx=cx+c*rx*0.46+(r%2?rx*0.23:0); f+=`<rect x="${xx-rx*0.2}" y="${yy}" width="${rx*0.4}" height="${ry*0.26}" rx="2" fill="${mix(o.color,'#9aa0a4',0.45)}" stroke="${darker}" stroke-width="1.4"/><line x1="${xx-rx*0.16}" y1="${yy+ry*0.05}" x2="${xx+rx*0.16}" y2="${yy+ry*0.05}" stroke="#fff" stroke-width="1" opacity=".4"/>`; } } pat=f;
    }

    /* ---- eyes ---- */
    let eyes='';
    const pupilShape=(ex,ey,r)=>{
      const pc = emitEyes ? o.signal : '#23202b';
      switch(o.pupil){
        case 'vertical':   return `<ellipse cx="${ex+r*0.1}" cy="${ey+r*0.1}" rx="${r*0.28}" ry="${r*0.62}" fill="${pc}"/>`;
        case 'horizontal': return `<ellipse cx="${ex+r*0.1}" cy="${ey+r*0.1}" rx="${r*0.62}" ry="${r*0.28}" fill="${pc}"/>`;
        case 'star':       return `<path d="M${ex} ${ey-r*0.6} l${r*0.18} ${r*0.36} ${r*0.4} 0 -${r*0.32} ${r*0.26} ${r*0.14} ${r*0.4} -${r*0.36} -${r*0.24} -${r*0.36} ${r*0.24} ${r*0.14} -${r*0.4} -${r*0.32} -${r*0.26} ${r*0.4} 0Z" fill="${pc}"/>`;
        case 'none':       return '';
        default:           return `<circle cx="${ex+r*0.15}" cy="${ey+r*0.12}" r="${r*0.5}" fill="${pc}"/>`;
      }
    };
    const eye=(ex,r,ey)=>{ ey=(ey==null?eyeY:ey);
      const sclera = emitEyes ? mix(o.signal,'#ffffff',0.55) : '#fbf9f3';
      const eg = emitEyes ? `<circle cx="${ex}" cy="${ey}" r="${r+3}" fill="${o.signal}" opacity=".4"/>` : '';
      return `${eg}<circle cx="${ex}" cy="${ey}" r="${r}" fill="${sclera}"/>${pupilShape(ex,ey,r)}<circle cx="${ex-r*0.2}" cy="${ey-r*0.25}" r="${r*0.18}" fill="#fff"/>`; };
    const ab = (o.asym==='eye'); // um olho maior
    if(o.eyes==='none'){ eyes=''; }
    else if(o.eyes==='big'){ eyes = eye(cx-rx*0.30,ab?9.5:7.5)+eye(cx+rx*0.30,7.5); }
    else if(o.eyes==='small'){ eyes = eye(cx-rx*0.3,ab?5.4:3.6)+eye(cx+rx*0.3,3.6); }
    else if(o.eyes==='three'){ eyes = eye(cx-rx*0.38,4.2)+eye(cx+rx*0.38,4.2)+eye(cx,4.2,eyeY-11); }
    else if(o.eyes==='one'){ eyes = eye(cx,9,eyeY); }
    else if(o.eyes==='compound'){
      eyes = [-0.32,0.32].map(f=>{ const ox=cx+f*rx; let g=`<circle cx="${ox}" cy="${eyeY}" r="6.5" fill="${emitEyes?o.signal:'#3a3340'}"/>`;
        for(let a=0;a<6;a++){ const an=a/6*Math.PI*2; g+=`<circle cx="${(ox+Math.cos(an)*3).toFixed(1)}" cy="${(eyeY+Math.sin(an)*3).toFixed(1)}" r="1.5" fill="#fff" opacity=".5"/>`; } return g; }).join('');
    }
    else if(o.eyes==='side'){ eyes = eye(cx-rx*0.62,5,eyeY+2)+eye(cx+rx*0.62,5,eyeY+2); }
    else if(o.eyes==='sleepy'){ eyes = [-0.32,0.32].map(f=>`<path d="M${cx+f*rx-5} ${eyeY} q5 4 10 0" stroke="#23202b" stroke-width="2.2" fill="none" stroke-linecap="round"/>`).join(''); }
    else { eyes = eye(cx-rx*0.32,ab?7:5.3)+eye(cx+rx*0.32,5.3); }

    /* ---- mouth ---- */
    /* ---- mouth / mandíbula ---- */
    const my = cy - ry*0.04;
    let mouth;
    if(o.mouth==='beak'){
      mouth = `<path d="M${cx-7} ${my-2} L${cx+7} ${my-2} L${cx} ${my+9}Z" fill="${o.signal==='#fbf9f3'?'#C9A04A':shade('#C9A04A',0)}" stroke="${darker}" stroke-width="1.8" stroke-linejoin="round"/><path d="M${cx-7} ${my-1} L${cx+7} ${my-1}" stroke="${darker}" stroke-width="1.4"/>`;
    } else if(o.mouth==='tusks'){
      mouth = `<path d="M${cx-5} ${my} Q${cx} ${my+4} ${cx+5} ${my}" stroke="${darker}" stroke-width="1.8" fill="none" stroke-linecap="round"/>`
            + `<path d="M${cx-6} ${my} q-1 7 -3 10" stroke="#F1ECE2" stroke-width="3" stroke-linecap="round"/><path d="M${cx+6} ${my} q1 7 3 10" stroke="#F1ECE2" stroke-width="3" stroke-linecap="round"/>`;
    } else if(o.mouth==='trunk'){
      mouth = `<path d="M${cx} ${my-2} q-2 14 -8 20 q-3 5 2 9" stroke="${dark}" stroke-width="7" fill="none" stroke-linecap="round"/><circle cx="${cx-6}" cy="${my+27}" r="2" fill="${darker}"/>`;
    } else if(o.mouth==='suction'){
      mouth = `<circle cx="${cx}" cy="${my+2}" r="6" fill="${darker}"/><circle cx="${cx}" cy="${my+2}" r="6" fill="none" stroke="${dark}" stroke-width="2"/><circle cx="${cx}" cy="${my+2}" r="2.4" fill="${shade(o.color,-0.2)}"/>`;
    } else if(o.mouth==='lips'){
      mouth = `<path d="M${cx-8} ${my+1} Q${cx} ${my-4} ${cx+8} ${my+1} Q${cx} ${my+7} ${cx-8} ${my+1}Z" fill="${mix(o.signal,'#C0563A',0.4)}" stroke="${darker}" stroke-width="1.6" stroke-linejoin="round"/><path d="M${cx-8} ${my+1} H${cx+8}" stroke="${darker}" stroke-width="1.2"/>`;
    } else if(o.mouth==='none'){
      mouth = '';
    } else {
      mouth = o.pose==='alert'
        ? `<ellipse cx="${cx}" cy="${my+1}" rx="3" ry="3.4" fill="${darker}"/>`
        : `<path d="M${cx-5} ${my} Q${cx} ${my+4} ${cx+5} ${my}" stroke="${darker}" stroke-width="1.8" fill="none" stroke-linecap="round"/>`;
    }

    /* ---- antennae / head ---- */
    let ant='';
    const topY = cy-ry;
    if(o.antennae==='pair'){
      ant = `<path d="M${cx-7} ${topY+4} q-3 -10 -6 -14" stroke="${dark}" stroke-width="2.4" fill="none" stroke-linecap="round"/><circle cx="${cx-13}" cy="${topY-11}" r="3.4" fill="${o.signal}"/>`
          + `<path d="M${cx+7} ${topY+4} q3 -10 6 -14" stroke="${dark}" stroke-width="2.4" fill="none" stroke-linecap="round"/><circle cx="${cx+13}" cy="${topY-11}" r="3.4" fill="${o.signal}"/>`;
    } else if(o.antennae==='single'){
      ant = `<path d="M${cx} ${topY+3} q0 -10 0 -15" stroke="${dark}" stroke-width="2.6" fill="none" stroke-linecap="round"/><circle cx="${cx}" cy="${topY-13}" r="4" fill="${o.signal}"/>`;
    } else if(o.antennae==='horns'){
      ant = `<path d="M${cx-rx*0.3} ${topY+6} l-3 -13 6 4Z" fill="${dark}"/>`
          + `<path d="M${cx+rx*0.3} ${topY+6} l3 -13 -6 4Z" fill="${dark}"/>`;
    } else if(o.antennae==='ears'){
      ant = `<ellipse cx="${cx-rx*0.5}" cy="${topY+8}" rx="6" ry="11" fill="${o.color}" stroke="${darker}" stroke-width="2" transform="rotate(-18 ${cx-rx*0.5} ${topY+8})"/>`
          + `<ellipse cx="${cx+rx*0.5}" cy="${topY+8}" rx="6" ry="11" fill="${o.color}" stroke="${darker}" stroke-width="2" transform="rotate(18 ${cx+rx*0.5} ${topY+8})"/>`;
    } else if(o.antennae==='crest_bone'){
      ant = `<path d="M${cx-rx*0.5} ${topY+4} Q${cx} ${topY-22} ${cx+rx*0.5} ${topY+4} Q${cx} ${topY-10} ${cx-rx*0.5} ${topY+4}Z" fill="${shade(o.color,0.3)}" stroke="${darker}" stroke-width="2" stroke-linejoin="round"/>`
          + `<path d="M${cx-rx*0.2} ${topY-6} L${cx} ${topY-16} L${cx+rx*0.2} ${topY-6}" stroke="${darker}" stroke-width="1.2" fill="none" opacity=".5"/>`;
    } else if(o.antennae==='hood'){
      ant = `<path d="M${cx-rx*0.9} ${topY+10} Q${cx} ${topY-20} ${cx+rx*0.9} ${topY+10} Q${cx} ${topY+2} ${cx-rx*0.9} ${topY+10}Z" fill="${o.color2}" stroke="${darker}" stroke-width="2" stroke-linejoin="round" opacity=".9"/>`
          + `<path d="M${cx} ${topY-14} v10" stroke="${darker}" stroke-width="1.4" opacity=".5"/>`;
    } else if(o.antennae==='mane'){
      let mn=''; for(let k=-3;k<=3;k++){ const sx=cx+k*rx*0.28; mn+=`<path d="M${sx} ${topY+8} q${k*2} -${16-Math.abs(k)*2} ${k*5} -${20-Math.abs(k)*3}" stroke="${shade(o.color,-0.2)}" stroke-width="3.2" fill="none" stroke-linecap="round"/>`; }
      ant = mn;
    }

    /* ---- ornament (culture) ---- */
    let orn='';
    if(o.ornament==='headband'){
      const by = cy-ry*0.60;
      orn = `<path d="M${cx-rx*0.82} ${by} Q${cx} ${by-7} ${cx+rx*0.82} ${by}" stroke="${o.signal}" stroke-width="5" fill="none" stroke-linecap="round"/><circle cx="${cx}" cy="${by-4}" r="3" fill="${shade(o.signal,-0.2)}"/>`;
    } else if(o.ornament==='facepaint'){
      orn = `<path d="M${cx-rx*0.3} ${eyeY-7} l3 -4 M${cx+rx*0.3} ${eyeY-7} l-3 -4" stroke="${o.signal}" stroke-width="2.4" stroke-linecap="round"/><path d="M${cx-6} ${cy+ry*0.22} h12" stroke="${o.signal}" stroke-width="2.2" stroke-linecap="round"/>`;
    } else if(o.ornament==='religious'){
      const sy=topY-10;
      orn = `<path d="M${cx} ${sy-7} v14 M${cx-6} ${sy} h12" stroke="${o.signal}" stroke-width="2.6" stroke-linecap="round"/><circle cx="${cx}" cy="${sy}" r="9" fill="none" stroke="${o.signal}" stroke-width="1.6" opacity=".6"/>`;
    } else if(o.ornament==='crest'){
      const by=topY+2; let cr='';
      for(let k=-1;k<=1;k++) cr+=`<path d="M${cx+k*7} ${by} l${k*2} -${10-Math.abs(k)*3}" stroke="${o.signal}" stroke-width="3.4" stroke-linecap="round"/>`;
      orn = cr;
    } else if(o.ornament==='scar_ritual'){
      orn = `<g stroke="${o.signal}" stroke-width="1.8" stroke-linecap="round" opacity=".8"><path d="M${cx-rx*0.5} ${cy} h${rx*0.3} M${cx-rx*0.5} ${cy+6} h${rx*0.22} M${cx+rx*0.5} ${cy} h-${rx*0.3} M${cx+rx*0.5} ${cy+6} h-${rx*0.22}"/></g>`;
    } else if(o.ornament==='piercing'){
      orn = `<line x1="${cx-3}" y1="${my-2}" x2="${cx+3}" y2="${my-2}" stroke="${shade(o.signal,0.3)}" stroke-width="2.4" stroke-linecap="round"/><circle cx="${cx-4}" cy="${my-2}" r="1.8" fill="${o.signal}"/><circle cx="${cx+4}" cy="${my-2}" r="1.8" fill="${o.signal}"/>`;
    }

    /* ---- assimetria / marcas de vida ---- */
    let asymMark='';
    if(o.asym==='scar'){
      asymMark = `<path d="M${cx+rx*0.3} ${cy-ry*0.3} l6 14" stroke="${darker}" stroke-width="1.8" stroke-linecap="round"/><path d="M${cx+rx*0.3+1} ${cy-ry*0.3+3} l-3 1 M${cx+rx*0.3+3} ${cy-ry*0.3+8} l-3 1" stroke="${darker}" stroke-width="1.2"/>`;
    } else if(o.asym==='burn'){
      asymMark = `<ellipse cx="${cx-rx*0.4}" cy="${cy+ry*0.2}" rx="${rx*0.22}" ry="${ry*0.2}" fill="${darker}" opacity=".5"/><ellipse cx="${cx-rx*0.4}" cy="${cy+ry*0.2}" rx="${rx*0.12}" ry="${ry*0.1}" fill="${shade(darker,-0.3)}" opacity=".6"/>`;
    }

    const glow = o.glow ? `<ellipse cx="${cx}" cy="${cy}" rx="${rx+12}" ry="${ry+12}" fill="${o.signal}" opacity=".16"/>` : '';
    const bodyTransform = `rotate(${P.rot} ${cx} ${cy}) translate(0 ${P.dy}) scale(${P.sx} ${P.sy})`;
    m.light = light;

    const inner = `
      <defs>${defs}</defs>
      ${glow}
      ${emitLayer}
      <ellipse cx="${cx}" cy="${cy+ry+6}" rx="${rx*0.8}" ry="6" fill="#1F2933" opacity=".14"/>
      ${feetSVG(o,m,dark,P)}
      ${armSVG(-1,o,m,dark)}${armSVG(1,o,m,dark)}
      <g transform="${bodyTransform}" transform-origin="${cx}px ${cy}px">
        ${neckSVG(o,m,dark,darker)}
        ${tailSVG(o,m,dark,darker)}
        ${earsSVG(o,m,dark,darker)}
        ${ant}
        <path d="${bp}" fill="${bodyFill}" fill-opacity="${bodyOpacity}" stroke="${darker}" stroke-width="2"/>
        <g clip-path="url(#${id}c)">
          ${extra}${pat}
          <ellipse cx="${cx-rx*0.35}" cy="${cy-ry*0.45}" rx="${rx*0.3}" ry="${ry*0.22}" fill="${light}" opacity=".45"/>
          ${finishOverlay}
          ${asymMark}
        </g>
        ${snoutSVG(o,m,dark,darker)}
        ${cheeksSVG(o,m,dark,darker)}
        ${eyes}
        ${temperSVG(o,m)}
        ${mouth}
        ${orn}
      </g>`;
    return `<svg viewBox="0 0 120 160" width="120" height="160" xmlns="http://www.w3.org/2000/svg" style="transform:scale(${o.size});transform-origin:bottom center">${inner}</svg>`;
  }

  // forma-mãe + ramos divergentes (especiação)
  const VARIATIONS = [
    {name:'Forma-mãe',  o:{shape:'egg',  color:'#7FB29E', pattern:'belly',  eyes:'two',   legs:'stubby'}},
    {name:'Clima frio', o:{shape:'round',color:'#9FB4C4', color2:'#CFE0D8', blend:'belly2', pattern:'spots', eyes:'small', legs:'stubby', size:1.12}},
    {name:'Árido',      o:{shape:'tall', color:'#C9A86A', color2:'#8A6A3A', blend:'gradient', pattern:'stripes', eyes:'two', legs:'tall', size:0.9}},
    {name:'Caverna',    o:{shape:'pear', color:'#9579B6', pattern:'none',   eyes:'big',   legs:'stubby', antennae:'single'}},
    {name:'Aquático',   o:{shape:'squat',color:'#5FA9B0', color2:'#3E6B8C', blend:'twotone', pattern:'belly', eyes:'two', legs:'none', arms:'flippers', antennae:'pair'}},
    {name:'Predador',   o:{shape:'tall', color:'#B0644C', pattern:'spots',  eyes:'three', legs:'tall', arms:'claws'}},
    {name:'Pântano',    o:{shape:'blob', color:'#6E8F4A', color2:'#2F6F62', blend:'speckle2', pattern:'speckle', eyes:'sleepy', legs:'stubby', arms:'tentacles'}},
    {name:'Voador',     o:{shape:'round',color:'#D292B6', pattern:'scales', eyes:'two', legs:'stubby', arms:'wings', antennae:'ears'}},
    {name:'Místico',    o:{shape:'bean', color:'#7C4DBE', color2:'#C9A0FF', blend:'dorsal', eyes:'one', legs:'tall', arms:'long', ornament:'crest', glow:true, signal:'#C9A0FF'}}
  ];
  const POSES = ['idle','walk','forrage','alert'];
  const POSE_LABELS = {idle:'Ocioso', walk:'Locomoção', forrage:'Forragear', alert:'Alerta'};

  window.GeneseCreature = { build, DEFAULTS, VARIATIONS, POSES, POSE_LABELS, shade, mix };
})();

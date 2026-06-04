/* ============================================================
   GÊNESE Wireframes — builder helpers (vanilla)
   Keeps markup compact & data-driven.
   ============================================================ */
(function(){
  const esc = s => String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');

  // priority pill
  function pri(p){
    if(!p) return '';
    const k = p.toLowerCase();
    return `<span class="pill ${k} s-pri">${p}</span>`;
  }
  function tag(t){ return t ? `<span class="pill mod s-tag">${t}</span>` : ''; }

  // placeholder slot: {label, cap, tint, h, p (priority), mod}
  function slot(o){
    o = o||{};
    const tint = o.tint ? 't-'+o.tint : 't-neutral';
    return `<div class="slot ${tint}" style="--h:${o.h||120}px">
      ${tag(o.mod)}${pri(o.p)}
      ${o.glyph?`<div class="ic">${o.glyph}</div>`:''}
      <div class="s-label">${o.label||''}</div>
      ${o.cap?`<div class="s-cap">${o.cap}</div>`:''}
    </div>`;
  }

  // grid wrapper
  function grid(items, cls){
    return `<div class="grid ${cls||'cols-4'}">${items.join('')}</div>`;
  }

  // block: titled section
  function block(label, desc, bodyHTML){
    return `<section class="block">
      <h3 class="blabel">${label}</h3>
      ${desc?`<p class="bdesc">${desc}</p>`:''}
      ${bodyHTML}
    </section>`;
  }

  // sheet head
  function head(o){
    return `<header class="sheet-head">
      <div class="eyebrow">${o.eyebrow||''}</div>
      <h2>${o.title}</h2>
      ${o.desc?`<p>${o.desc}</p>`:''}
      ${o.refs?`<div class="refs">${o.refs}</div>`:''}
    </header>`;
  }

  // annotation callout
  function annot(text, kind){ return `<div class="annot ${kind||''}">${text}</div>`; }
  function callouts(arr){ return `<div class="callouts">${arr.map(a=>annot(a.t,a.k)).join('')}</div>`; }

  // UI panel mock
  function panel(title, bodyHTML, opts){
    opts = opts||{};
    const dots = opts.dots ? `<span class="dotrow"><i></i><i></i><i></i></span>` : '';
    return `<div class="panel">
      <div class="panel-bar">${title}${dots}</div>
      <div class="panel-body">${bodyHTML}</div>
    </div>`;
  }

  // stat line {nm,val,tint}
  function stat(o){
    const tint = o.tint?'s-'+o.tint:'';
    return `<div class="statline ${tint}">
      <span class="nm">${o.nm}</span>
      <span class="track"><span class="fill" style="width:${o.val}%"></span></span>
      <span class="vv">${o.val}</span>
    </div>`;
  }
  function stats(arr){ return arr.map(stat).join(''); }

  // color ramp {label, from, to, stops:[]}
  function ramp(o){
    const cols = o.stops || [o.from,o.to];
    return `<div class="ramp-row">
      <div class="rl"><span>${o.label}</span><span>${o.lo||'baixo'} → ${o.hi||'alto'}</span></div>
      <div class="ramp">${cols.map(c=>`<span style="background:${c}"></span>`).join('')}</div>
    </div>`;
  }

  // icon chip {glyph, name}
  function chip(o){
    return `<div class="chip"><div class="glyph">${o.glyph||''}</div><div class="cn">${o.name}</div></div>`;
  }

  // storyboard frame {n, img, name, desc}
  function frame(o){
    return `<div class="frame">
      <div class="fimg"><span>${o.img}</span></div>
      <div class="fcap"><div class="fnum">${o.n||''}</div><div class="fn">${o.name}</div>
      ${o.desc?`<div class="fd">${o.desc}</div>`:''}</div>
    </div>`;
  }
  function flow(arr){ return `<div class="flow">${arr.map(frame).join('')}</div>`; }

  // checklist
  function checklist(arr){ return `<ul class="chk">${arr.map(i=>`<li>${i}</li>`).join('')}</ul>`; }

  // legend
  function legend(arr){
    return `<div class="legend">${arr.map(a=>`<span><i style="background:${a.c}"></i>${a.t}</span>`).join('')}</div>`;
  }

  window.WF = { esc, slot, grid, block, head, annot, callouts, panel, stat, stats, ramp, chip, frame, flow, checklist, legend, pri, tag };
})();

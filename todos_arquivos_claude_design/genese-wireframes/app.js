/* ============================================================
   GÊNESE Wireframes — app shell: nav, routing, tweaks
   ============================================================ */
(function(){
  const SHEETS = [].concat(window.SHEETS_UI, window.SHEETS_ASSETS, window.SHEETS_META);
  const byId = Object.fromEntries(SHEETS.map(s=>[s.id,s]));

  /* ---- Tweaks state ---- */
  const TW_DEFAULTS = { toolbar:'tb-bottom', annot:true, density:'regular', dark:false };
  let tw;
  try{ tw = Object.assign({}, TW_DEFAULTS, JSON.parse(localStorage.getItem('genese_tw')||'{}')); }
  catch(e){ tw = Object.assign({}, TW_DEFAULTS); }
  function saveTw(){ try{ localStorage.setItem('genese_tw', JSON.stringify(tw)); }catch(e){} }

  /* ---- Build nav rail ---- */
  function buildNav(){
    const groups = {};
    SHEETS.forEach(s=>{ (groups[s.group]=groups[s.group]||[]).push(s); });
    const rail = document.getElementById('rail');
    let html = `<h1>Gênese</h1><div class="sub">Wireframes · v0.1</div>`;
    Object.keys(groups).forEach(g=>{
      html += `<div class="rail-group"><span class="grp-label">${g}</span>`;
      groups[g].forEach((s,i)=>{
        html += `<button class="nav-item" data-id="${s.id}">
          <span class="dot" style="background:${s.dot}"></span>${s.title}
          <span class="num">${String(SHEETS.indexOf(s)+1).padStart(2,'0')}</span>
        </button>`;
      });
      html += `</div>`;
    });
    rail.innerHTML = html;
    rail.querySelectorAll('.nav-item').forEach(b=>{
      b.addEventListener('click',()=>go(b.dataset.id));
    });

    // mobile select
    const sel = document.getElementById('mobileSel');
    sel.innerHTML = Object.keys(groups).map(g=>
      `<optgroup label="${g}">`+groups[g].map(s=>`<option value="${s.id}">${s.title}</option>`).join('')+`</optgroup>`
    ).join('');
    sel.addEventListener('change',()=>go(sel.value));
  }

  /* ---- Routing ---- */
  function go(id){
    const s = byId[id] || SHEETS[0];
    location.hash = s.id;
  }
  function render(){
    const id = location.hash.replace('#','') || SHEETS[0].id;
    const s = byId[id] || SHEETS[0];
    const stage = document.getElementById('stage');
    stage.innerHTML = `<div class="sheet">${s.build()}</div>` + footer();
    stage.scrollTop = 0; window.scrollTo(0,0);
    document.querySelectorAll('.nav-item').forEach(b=>b.classList.toggle('active', b.dataset.id===s.id));
    const sel = document.getElementById('mobileSel'); if(sel) sel.value = s.id;
    applyTw();
  }
  function footer(){
    return `<div class="foot">GÊNESE · simulação evolutiva de civilização emergente — wireframes de UI, UX e folhas de produção de arte.<br>
    Fonte de verdade: GDD v0.1 · DV (Design Visual) · AA (Catálogo de Assets) · E10. Placeholders tracejados = arte a produzir; cor = dado (paletas semânticas).<br>
    Navegue pela barra lateral. Tweaks: alterne pela barra do editor (ícone de ajustes) — posição da barra de ferramentas, anotações e densidade.</div>`;
  }

  /* ---- Tweaks panel (vanilla, host protocol) ---- */
  function buildTweaks(){
    const p = document.getElementById('tweaks');
    p.innerHTML = `
      <div class="tw-head"><span>Tweaks</span><button id="twClose" aria-label="fechar">✕</button></div>
      <div class="tw-body">
        <div class="tw-sec">Barra de ferramentas (HUD)</div>
        <div class="tw-seg" data-key="toolbar">
          <button data-v="tb-bottom">Inferior</button>
          <button data-v="tb-bottom-c">Centro</button>
          <button data-v="tb-left">Esquerda</button>
          <button data-v="tb-right">Direita</button>
        </div>
        <div class="tw-hint">Visível na folha “Mundo Vivo (HUD)”.</div>
        <div class="tw-sec">Densidade</div>
        <div class="tw-seg" data-key="density">
          <button data-v="compact">Compacta</button>
          <button data-v="regular">Regular</button>
        </div>
        <div class="tw-sec">Anotações</div>
        <div class="tw-seg" data-key="annot">
          <button data-v="true">Mostrar</button>
          <button data-v="false">Ocultar</button>
        </div>
        <div class="tw-sec">Tom do papel</div>
        <div class="tw-seg" data-key="dark">
          <button data-v="false">Claro</button>
          <button data-v="true">Escuro</button>
        </div>
      </div>`;
    p.querySelectorAll('.tw-seg').forEach(seg=>{
      const key = seg.dataset.key;
      seg.querySelectorAll('button').forEach(b=>{
        b.addEventListener('click',()=>{
          let v = b.dataset.v;
          if(v==='true') v=true; else if(v==='false') v=false;
          tw[key]=v; saveTw(); applyTw();
        });
      });
    });
    document.getElementById('twClose').addEventListener('click',dismiss);
  }
  function applyTw(){
    // segmented active states
    document.querySelectorAll('#tweaks .tw-seg').forEach(seg=>{
      const key=seg.dataset.key;
      seg.querySelectorAll('button').forEach(b=>{
        let v=b.dataset.v; if(v==='true')v=true; else if(v==='false')v=false;
        b.classList.toggle('on', v===tw[key]);
      });
    });
    // toolbar
    const tb = document.getElementById('hudToolbar');
    if(tb){ tb.className = 'toolbar '+tw.toolbar; }
    // annotations
    document.body.classList.toggle('hideoff', !tw.annot);
    // density
    document.body.classList.toggle('compact', tw.density==='compact');
    // dark
    document.body.classList.toggle('dark', !!tw.dark);
  }

  /* ---- Host protocol ---- */
  function openPanel(){ document.getElementById('tweaks').classList.add('open'); }
  function closePanel(){ document.getElementById('tweaks').classList.remove('open'); }
  function dismiss(){ closePanel(); try{ window.parent.postMessage({type:'__edit_mode_dismissed'},'*'); }catch(e){} }
  window.addEventListener('message', e=>{
    const t = e && e.data && e.data.type;
    if(t==='__activate_edit_mode') openPanel();
    else if(t==='__deactivate_edit_mode') closePanel();
  });

  /* ---- init ---- */
  buildNav();
  buildTweaks();
  window.addEventListener('hashchange', render);
  render();
  try{ window.parent.postMessage({type:'__edit_mode_available'},'*'); }catch(e){}
})();

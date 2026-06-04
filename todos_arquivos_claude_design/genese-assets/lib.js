/* ============================================================
   GÊNESE — Biblioteca de Assets: app + seções
   ============================================================ */
(function(){
  const IC = window.GENESE_ICONS, icon = window.iconSVG;
  const CR = window.GeneseCreature, MT = window.GeneseMeta;
  const BIO = window.GeneseBiomes, BLD = window.GeneseBuildings;

  const TOKENS = {
    base:[
      {nm:'Fundo / vazio', hex:'#1F2933', use:'Calma; espaço para o mundo respirar'},
      {nm:'Vida', hex:'#2F6F62', use:'Vegetação, vitalidade, biológico'},
      {nm:'Influência', hex:'#8C5BAA', use:'Presença do jogador, a mão invisível'},
      {nm:'Tensão', hex:'#C0563A', use:'Conflito, perigo, alerta'},
      {nm:'UI claro', hex:'#EAF2EF', use:'Painéis e texto'}
    ],
    ramps:[
      {label:'Agressividade', lo:'calmo', hi:'violento', stops:['#7E8CA0','#A8806F','#C0563A','#8E2D17']},
      {label:'Vitalidade / saúde', lo:'doente', hi:'saudável', stops:['#C7CDC8','#7FB29E','#2F6F62','#16463C']},
      {label:'Recursos', lo:'seco', hi:'abundante', stops:['#8A7C5E','#B8862F','#E0C46A','#F2E2A6']},
      {label:'Fé / religiosidade', lo:'laico', hi:'sagrado', stops:['#9A93A8','#A07CC4','#7C4DBE','#5E2E9E']},
      {label:'Densidade populacional', lo:'vazio', hi:'denso', stops:['#CFE0D8','#7FB29E','#2F6F62','#16463C']},
      {label:'Divergência linguística', lo:'mesma língua', hi:'incompreensão', stops:['#3E6B8C','#6E8AA0','#A77FA0','#C0563A']}
    ]
  };

  function head(o){ return `<header class="sec-head"><div class="eyebrow">${o.eyebrow||''}</div><h2>${o.title}</h2>${o.desc?`<p>${o.desc}</p>`:''}${o.refs?`<div class="refs">${o.refs}</div>`:''}</header>`; }
  function block(l,d,b){ return `<section class="block"><h3 class="bl">${l}</h3>${d?`<p class="bd">${d}</p>`:''}${b}</section>`; }

  /* ---------- 0. Visão geral ---------- */
  function overview(){
    return head({eyebrow:'BIBLIOTECA DE ASSETS · v0.1', title:'Assets de Gênese',
      desc:'Produção de arte vetorial real, pronta para o projeto Unity/C#. Tudo aqui é exportável como SVG (tingível em runtime) ou como tokens de cor. Espelha o catálogo AA e a direção do DV.',
      refs:'<b>Fonte:</b> AA · DV · GDD §8/§9'})
    + block('O que esta biblioteca entrega',
      'Cada seção é uma família de assets construída de verdade — não placeholder.',
      `<div class="grid c3">
        ${['Cores & tokens (paleta-base + 6 rampas semânticas, .json p/ Unity)','Conjunto de ícones (recurso, nudge, toolbar, estatística, evento, cursor) — monocromáticos','Kit de criatura paramétrica (forma-mãe + variações + poses)','Escrita procedural (glifos por cultura)','Marca (logotipo) + molduras de HUD','Cartões de destino final (7 variantes)'].map(t=>`<div class="spec"><div class="st">✦ ${t.split(' (')[0]}</div><ul><li>${(t.split('(')[1]||'').replace(')','')||'pronto para integrar'}</li></ul></div>`).join('')}
      </div>`)
    + `<div class="note">Agora os <b>biomas e construções</b> também têm <b>assets vetoriais reais</b> (flora, props e estruturas modulares, recoloríveis) — ver "Mundo (vetor)". As <b>cenas pintadas</b> no estilo dos concepts continuam como <b>fichas de direção</b> (paleta + briefing), prontas para ilustrador / geração de imagem. O resto é asset final.</div>`;
  }

  /* ---------- Cores ---------- */
  function cores(){
    const sw = TOKENS.base.map(c=>`<div class="swatch"><div class="chip" style="background:${c.hex}"></div><div class="meta"><div class="nm">${c.nm}</div><div class="hex">${c.hex}</div><div class="use">${c.use}</div></div></div>`).join('');
    const rp = TOKENS.ramps.map(r=>`<div class="ramprow"><div class="rl"><span>${r.label}</span><span>${r.lo} → ${r.hi}</span></div><div class="ramp">${r.stops.map(s=>`<span style="background:${s}"></span>`).join('')}</div></div>`).join('');
    return head({eyebrow:'TOKENS · DV §3', title:'Cores & Tokens',
      desc:'Dois níveis: a paleta-base (identidade) e as rampas semânticas (cor com significado de dado). As mesmas rampas valem para criaturas, ambiente e overlays — o jogador aprende a "ler" o mundo.',
      refs:'<b>Arquivo:</b> tokens.json (importar como paleta/constantes C#)'})
    + block('Paleta-base', '', `<div class="grid c4">${sw}</div>`)
    + block('Rampas semânticas', 'Baixo → alto. Use em barras, ícones e heatmaps da mesma estatística — sempre idênticas.', rp);
  }

  /* ---------- Ícones ---------- */
  function icones(){
    let out = head({eyebrow:'ASSETS · P0/P1 · AA §5.2', title:'Ícones',
      desc:'Monocromáticos (stroke = currentColor), tingíveis no Unity. viewBox 24×24, traço arredondado. Exportáveis como .svg individuais — ver a seção "Exportar / Unity".',
      refs:'<b>Arquivo:</b> icons.js · svg/icons/*.svg'});
    Object.keys(IC).forEach(cat=>{
      const tiles = Object.keys(IC[cat]).map(k=>{
        const it = IC[cat][k];
        return `<div class="itile"><div style="color:#EFE9F4">${icon(it.svg,30,1.7)}</div><div class="in">${it.label}</div><div class="if">${k}.svg</div></div>`;
      }).join('');
      out += block(cat, '', `<div class="grid c6">${tiles}</div>`);
    });
    return out;
  }

  /* ---------- Criaturas (kit) ---------- */
  let cstate = Object.assign({}, CR.DEFAULTS, {seed:1});
  const BODY_COLORS = ['#7FB29E','#9FB4C4','#C9A86A','#9579B6','#5FA9B0','#B0644C','#8FB36A','#D292B6','#7C4DBE','#E0C46A'];
  const SIGNAL_COLORS = ['#8C5BAA','#C0563A','#2F6F62','#B8862F','#3E6B8C'];

  function segCtl(label,key,opts){
    return `<div class="ctl"><div class="cl">${label}</div><div class="seg" data-ctl="${key}">${opts.map(([v,l])=>`<button data-v="${v}">${l}</button>`).join('')}</div></div>`;
  }
  function swCtl(label,key,cols){
    return `<div class="ctl"><div class="cl">${label}</div><div class="sw-row" data-sw="${key}">${cols.map(c=>`<button class="sw" data-v="${c}" style="background:${c}"></button>`).join('')}</div></div>`;
  }
  function criaturas(){
    return head({eyebrow:'ASSETS · NÚCLEO · AA §2 · DV §4', title:'Criatura paramétrica',
      desc:'A forma-mãe e suas peças modulares. Genoma completo (corpo, cauda, boca, focinho, olhos+pupila, orelhas, pescoço, membros, textura, acabamento, transparência, emissão de luz, assimetrias) + cultura (ornamentos) + estado (cor de sinal, postura) viram pixels. Ajuste e veja a criatura mudar — o mesmo princípio roda em C# no jogo.',
      refs:'<b>Arquivo:</b> creature.js (build(opts) → SVG)'})
    + `<div class="kit">
        <div class="cstage" id="cstage"></div>
        <div class="controls" id="ccontrols">
          ${segCtl('Forma','shape',[['egg','Ovo'],['round','Redonda'],['tall','Alta'],['squat','Baixa'],['pear','Pera'],['bean','Feijão'],['blob','Gota']])}
          ${segCtl('Textura','pattern',[['none','—'],['belly','Ventre'],['spots','Pintas'],['stripes','Listras'],['scales','Escamas'],['speckle','Salpico'],['feathers','Penas'],['spines','Espinhos'],['fur','Pelagem'],['moss','Musgo'],['mosaic','Mosaico'],['bumps','Bolhas'],['needles','Agulhas'],['plates','Placas']])}
          ${segCtl('Olhos','eyes',[['two','2'],['big','Grandes'],['small','Pequenos'],['three','3'],['one','Ciclope'],['none','Sem'],['compound','Composto'],['side','Laterais'],['sleepy','Sonol.']])}
          ${segCtl('Pupila','pupil',[['round','Redonda'],['vertical','Vertical'],['horizontal','Horiz.'],['star','Estrela'],['none','—']])}
          ${segCtl('Boca','mouth',[['simple','Simples'],['beak','Bico'],['tusks','Presas'],['trunk','Tromba'],['suction','Ventosa'],['lips','Lábios'],['none','Nenhuma']])}
          ${segCtl('Focinho / nariz','snout',[['none','—'],['long','Alongado'],['flat','Achatado'],['horn','Chifre']])}
          ${segCtl('Bochechas / queixo','cheeks',[['none','—'],['pouch','Papo'],['barbels','Barbilhões'],['jowls','Papada']])}
          ${segCtl('Orelhas','ears',[['none','—'],['pointy','Pontudas'],['round','Arred.'],['drooping','Caídas'],['tuft','Penacho'],['fan','Leque'],['multiple','Múltiplas']])}
          ${segCtl('Pescoço','neck',[['none','—'],['short','Curto'],['long','Longo']])}
          ${segCtl('Cauda','tail',[['none','—'],['short','Curta'],['long','Longa'],['curl','Espiral'],['tuft','Tufo'],['fan','Leque'],['sting','Ferrão'],['fork','Bífida']])}
          ${segCtl('Pernas','legs',[['none','—'],['stubby','Curtas'],['tall','Altas'],['back','P/ trás'],['webbed','Palmadas']])}
          ${segCtl('Braços','arms',[['none','—'],['stubby','Curtos'],['long','Longos'],['claws','Garras'],['flippers','Nadadeiras'],['tentacles','Tentáculos'],['wings','Asas'],['double','2º par'],['fins','Barbatanas']])}
          ${segCtl('Cabeça / antenas','antennae',[['none','—'],['pair','Par'],['single','Única'],['horns','Chifres'],['crest_bone','Crista óssea'],['hood','Capuz'],['mane','Crina']])}
          ${segCtl('Acabamento','finish',[['matte','Fosco'],['satin','Acetinado'],['metallic','Metálico'],['iridescent','Iridescente'],['pearl','Perolado'],['mirror','Espelhado']])}
          ${segCtl('Transparência','opacity',[['solid','Sólido'],['translucent','Translúcido'],['crystal','Cristalino'],['ghost','Fantasmal']])}
          ${segCtl('Emissão de luz','emit',[['none','—'],['veins','Veias'],['body','Corpo'],['eyes','Olhos'],['trail','Rastro']])}
          ${segCtl('Assimetria / marcas','asym',[['none','—'],['eye','Olho'],['scar','Cicatriz'],['burn','Queimadura']])}
          ${segCtl('Temperamento','temper',[['neutro','Neutro'],['curioso','Curioso'],['timido','Tímido'],['agressivo','Agressivo'],['preguicoso','Preguiçoso'],['brincalhao','Brincalhão'],['paranoico','Paranoico']])}
          ${segCtl('Ornamento (cultura)','ornament',[['none','—'],['headband','Faixa'],['facepaint','Pintura'],['religious','Fé'],['crest','Crista'],['scar_ritual','Escarif.'],['piercing','Piercing']])}
          ${swCtl('Cor base','color',BODY_COLORS)}
          ${swCtl('Cor secundária','color2',BODY_COLORS)}
          ${segCtl('Mistura de cor','blend',[['none','—'],['gradient','Degradê'],['twotone','Bicolor'],['belly2','Ventre 2ª'],['dorsal','Dorsal'],['speckle2','Manchas']])}
          ${swCtl('Cor de sinal','signal',SIGNAL_COLORS)}
          ${segCtl('Aura de sinal','glow',[['false','Off'],['true','On']])}
          ${segCtl('Tamanho','size',[['0.82','P'],['1','M'],['1.18','G']])}
          <div class="kit-actions"><button class="btn prim" id="cRand">Aleatório</button><button class="btn" id="cReset">Reset</button></div>
        </div>
      </div>`
    + block('Estados de animação (mesma criatura)', 'Posturas procedurais (squash & stretch / inclinação) sobre a criatura atual. Animação P0 do AA §2.2.', `<div class="poserow" id="cposes"></div>`)
    + block('Variações de silhueta para especiação', 'Forma-mãe + 5 ramos divergentes — base do sistema de espécies (AA §2.1, GDD §3.2).', `<div class="varrow" id="cvars"></div>`);
  }
  function renderCreature(){
    const st = document.getElementById('cstage'); if(!st) return;
    const o = Object.assign({}, cstate); o.size=parseFloat(o.size)||1; o.glow=(o.glow===true||o.glow==='true');
    st.innerHTML = `<span class="seed">genoma #${cstate.seed||1}</span>`+CR.build(o);
    document.querySelectorAll('#ccontrols .seg').forEach(seg=>{ const k=seg.dataset.ctl; seg.querySelectorAll('button').forEach(b=>b.classList.toggle('on', String(cstate[k])===b.dataset.v)); });
    document.querySelectorAll('#ccontrols .sw-row').forEach(row=>{ const k=row.dataset.sw; row.querySelectorAll('.sw').forEach(b=>b.classList.toggle('on', cstate[k]===b.dataset.v)); });
    const poses=document.getElementById('cposes');
    if(poses) poses.innerHTML = CR.POSES.map(p=>`<div class="posecell">${CR.build(Object.assign({},o,{pose:p}))}<div class="pl">${CR.POSE_LABELS[p]}</div></div>`).join('');
    const vars=document.getElementById('cvars');
    if(vars) vars.innerHTML = CR.VARIATIONS.map(v=>`<div class="varcell">${CR.build(Object.assign({pose:'idle'},v.o))}<div class="vl">${v.name}</div></div>`).join('');
  }
  function wireCreature(){
    const c=document.getElementById('ccontrols'); if(!c) return;
    c.addEventListener('click',e=>{
      const b=e.target.closest('button'); if(!b) return;
      const seg=e.target.closest('.seg'), sw=e.target.closest('.sw-row');
      if(seg){ cstate[seg.dataset.ctl]= b.dataset.v==='true'?true : b.dataset.v==='false'?false : b.dataset.v; renderCreature(); }
      else if(sw){ cstate[sw.dataset.sw]=b.dataset.v; renderCreature(); }
    });
    document.getElementById('cRand').addEventListener('click',()=>{
      const pick=a=>a[Math.floor(Math.random()*a.length)];
      cstate={ seed:Math.floor(Math.random()*999)+1, shape:pick(['egg','round','tall','squat','pear','bean','blob']), pattern:pick(['none','belly','spots','stripes','scales','speckle','feathers','spines','fur','moss','mosaic','bumps','needles','plates']),
        eyes:pick(['two','two','big','small','three','one','none','compound','side','sleepy']), pupil:pick(['round','round','vertical','horizontal','star']),
        mouth:pick(['simple','simple','beak','tusks','trunk','suction','lips','none']), snout:pick(['none','none','long','flat','horn']), cheeks:pick(['none','none','none','pouch','barbels','jowls']), ears:pick(['none','none','pointy','round','drooping','tuft','fan','multiple']),
        neck:pick(['none','none','short','long']), tail:pick(['none','none','short','long','curl','tuft','fan','sting','fork']),
        legs:pick(['none','stubby','stubby','tall','back','webbed']), arms:pick(['none','none','stubby','long','claws','flippers','tentacles','wings','double','fins']),
        antennae:pick(['none','none','pair','single','horns','crest_bone','hood','mane']),
        finish:pick(['matte','matte','satin','metallic','iridescent','pearl','mirror']), opacity:pick(['solid','solid','solid','translucent','crystal','ghost']), emit:pick(['none','none','none','veins','body','eyes','trail']),
        asym:pick(['none','none','none','eye','scar','burn']), temper:pick(['neutro','neutro','curioso','timido','agressivo','preguicoso','brincalhao','paranoico']),
        ornament:pick(['none','none','headband','facepaint','religious','crest','scar_ritual','piercing']), color:pick(BODY_COLORS), color2:pick(BODY_COLORS),
        blend:pick(['none','none','gradient','twotone','belly2','dorsal','speckle2']), signal:pick(SIGNAL_COLORS),
        glow:Math.random()>0.6, size:pick(['0.82','1','1.18']), pose:'idle' };
      renderCreature();
    });
    document.getElementById('cReset').addEventListener('click',()=>{ cstate=Object.assign({},CR.DEFAULTS,{seed:1}); renderCreature(); });
    renderCreature();
  }

  /* ---------- Escrita ---------- */
  function escrita(){
    const cultures=[
      {style:'angular', name:'Cultura A — Angular', desc:'Traços retos entre nós; sensação rígida, gravada.'},
      {style:'organic', name:'Cultura B — Orgânica', desc:'Curvas contínuas; sensação fluida, pintada.'},
      {style:'radial', name:'Cultura C — Radial', desc:'Glifos irradiam de um centro; sensação ritual.'},
      {style:'runic', name:'Cultura D — Rúnica', desc:'Haste vertical com ramos; sensação entalhada em madeira/osso.'},
      {style:'cuneiform', name:'Cultura E — Cuneiforme', desc:'Cunhas impressas; sensação de barro, contabilidade.'},
      {style:'spiral', name:'Cultura F — Espiral', desc:'Volutas que se abrem; sensação astronômica, cíclica.'}
    ];
    let cards = cultures.map(c=>{
      const glyphs = Array.from({length:8},(_,i)=>`<div class="glyphcell">${MT.glyphSVG(c.style, i*13+5, 34)}</div>`).join('');
      return `<div class="culture-card"><div class="ct">${c.name}</div><div class="cd">${c.desc}</div>
        <div class="glyphgrid">${glyphs}</div>
        <div style="margin-top:12px">${MT.word(c.style, 7, 5)}</div></div>`;
    }).join('');
    return head({eyebrow:'ASSETS · P2 · AA §6 · DV §6.3', title:'Escrita procedural',
      desc:'Cada civilização inventa um sistema de escrita próprio: glifos gerados de uma grade de nós, compostos por regras que diferem por cultura. Ilegível no início, "traduzível" depois. Aparece em construções, artefatos e na Crônica.',
      refs:'<b>Arquivo:</b> meta-assets.js (glyphSVG(style, seed))'})
    + `<div class="grid c3">${cards}</div>`
    + `<div class="note">Mesmo gerador, três culturas: o estado da civilização escolhe o estilo, e o <i>seed</i> deriva do nome/idioma — nada de aleatório vazio (GDD §1.5).</div>`;
  }

  /* ---------- Marca & molduras ---------- */
  function marca(){
    return head({eyebrow:'ASSETS · P1 · AA §8', title:'Marca & Molduras de HUD',
      desc:'A marca "a semente que ramifica": uma espiral nasce de um único ponto (a criação) e termina numa bifurcação que brota (a evolução / divergência das espécies). Acompanha as molduras de interface (9-slice) que vestem o HUD: painel, barra de recurso e botão redondo.',
      refs:'<b>Arquivo:</b> meta-assets.js'})
    + block('Logotipo', 'Lockup horizontal + marca isolada (ícone do app). Funciona em fundo claro e escuro.',
      `<div class="grid c2">
        <div class="logo-demo dark">${MT.wordmark()}</div>
        <div class="logo-demo light">${MT.wordmark()}</div>
      </div>
      <div class="grid c6" style="margin-top:14px">
        ${[64,48,32].map(s=>`<div class="framecell" style="text-align:center"><div class="fl">marca ${s}px</div><div style="color:var(--influencia)">${MT.mark(s)}</div></div>`).join('')}
      </div>`)
    + block('Molduras de HUD (9-slice)', 'Vetores prontos para esticar nas bordas no Unity (Image type: Sliced).',
      `<div class="grid c3">
        <div class="framecell"><div class="fl">painel / card</div>${MT.panelFrame(220,96)}</div>
        <div class="framecell"><div class="fl">barra de recurso (Atenção)</div><div style="padding-top:24px">${MT.barFrame()}</div></div>
        <div class="framecell" style="text-align:center"><div class="fl">botão redondo</div>${MT.roundBtn(true)}</div>
      </div>`);
  }

  /* ---------- Cartões de destino ---------- */
  function destino(){
    const D=[
      {t:'Transcendência', d:'Supera a forma biológica.', tone:'sublime, ambíguo', c:'#7C4DBE', g:IC['Nudges (intervenção)'].inspiracao.svg},
      {t:'Estagnação eterna', d:'Equilíbrio estável sem evolução.', tone:'melancólico, pacífico', c:'#8A8377', g:IC['Macro-estatísticas'].estabilidade.svg},
      {t:'Colapso ecológico', d:'O ambiente se esgota; a população desaba.', tone:'trágico, evitável', c:'#B8862F', g:IC['Eventos & emoções'].colapso.svg},
      {t:'Autodestruição', d:'Guerra leva à aniquilação mútua.', tone:'trágico, sombrio', c:'#C0563A', g:IC['Eventos & emoções'].conflito.svg},
      {t:'Absorção', d:'Outra civilização a integra.', tone:'agridoce', c:'#3E6B8C', g:IC['Macro-estatísticas'].coesao.svg},
      {t:'Diáspora', d:'Fragmenta-se e se espalha.', tone:'melancólico', c:'#8C5BAA', g:IC['Macro-estatísticas'].expansionismo.svg},
      {t:'Continuidade', d:'Segue indefinidamente, sempre mudando.', tone:'aberto', c:'#2F6F62', g:IC['Barra de ferramentas'].contextual.svg}
    ];
    const cards=D.map(d=>`<div class="destiny"><div class="dh" style="background:${d.c}1f;color:${d.c}">${icon(d.g,30,1.7)}</div>
      <div class="db"><div class="dt">${d.t}</div><div class="dd">${d.d}</div><div class="dtone">${d.tone}</div></div></div>`).join('');
    return head({eyebrow:'ASSETS · P2 · AA §7', title:'Cartões de Destino Final',
      desc:'O jogo não tem vitória — tem destinos. Um layout, sete sabores. Tela-resumo compartilhável ao fim (ou a qualquer momento) da partida.',
      refs:'<b>GDD §6.4</b>'})
    + `<div class="grid c4">${cards}</div>`;
  }

  /* ---------- Biomas & construções (direção) ---------- */
  function direcao(){
    const biomes=Object.keys(BIO.BIOMES).map(key=>{
      const b=BIO.BIOMES[key];
      const cols=[b.pal.foliage,b.pal.foliage2,b.pal.rock,b.pal.accent,b.ground].filter(Boolean);
      return `<div class="spec"><div class="st"><span style="width:14px;height:14px;border-radius:50%;background:${b.pal.foliage};border:1.5px solid var(--line);display:inline-block"></span>${b.nm}</div>
        <div class="sswatch">${cols.map(c=>`<i style="background:${c}"></i>`).join('')}</div>
        <ul><li>Props: ${b.props.map(p=>p.replace(/_/g,' ')).join(', ')}${b.fx&&b.fx.length?` · efeitos: ${b.fx.join(', ')}`:''}</li></ul></div>`;
    }).join('');

    const bcards=BLD.STAGES.map(s=>`<div class="spec"><div class="st">▣ ${s.nm}</div>
      <ul><li>${s.items.map(k=>k.replace(/_/g,' ')).join(' · ')}</li></ul></div>`).join('');

    return head({eyebrow:'DIREÇÃO · AA §3, §4', title:'Biomas & Construções — Mapa de direção',
      desc:'Visão consolidada de tudo que existe no mundo: cada bioma com sua paleta + props + efeitos, e cada estágio com suas estruturas. As cenas e estruturas já existem como vetor real ("Biomas — Flora & Props" e "Construções modulares"); a arte isométrica final é pintada por cima desta direção. As cores saem dos tokens.',
      refs:'<b>Origem:</b> AA §3 (biomas), §4 (construções) · DV §5'})
    + block(`Biomas (${Object.keys(BIO.BIOMES).length})`, 'Naturais + fantásticos. Variações sazonais = recolorir a mesma cena.', `<div class="grid c3">${biomes}</div>`)
    + block(`Construções por estágio (${BLD.STAGES.length} estágios)`, 'Modular por estágio sociopolítico + estilo cultural (10 culturas) + variante Ruínas.', `<div class="grid c2">${bcards}</div>`);
  }

  /* ---------- Unity / export ---------- */
  function unity(){
    const code = `<span class="k">using</span> UnityEngine;

<span class="k">public static class</span> GenesePalette {
  <span class="k">public static readonly</span> Color Influencia = <span class="k">new</span> Color32(0x8C,0x5B,0xAA,0xFF);
  <span class="k">public static readonly</span> Color Vida       = <span class="k">new</span> Color32(0x2F,0x6F,0x62,0xFF);
  <span class="k">public static readonly</span> Color Tensao     = <span class="k">new</span> Color32(0xC0,0x56,0x3A,0xFF);
}

<span class="s">// Ícones: import .svg (com.unity.vectorgraphics) → tinta via SpriteRenderer.color</span>
<span class="s">// Criatura: porte build(opts) p/ C# montando SpriteRenderers por camada (corpo, olhos, ornamento)</span>`;
    return head({eyebrow:'INTEGRAÇÃO', title:'Exportar / Unity (C#)',
      desc:'Como levar estes assets para o projeto. SVGs são tingíveis (uma cópia, várias cores). O kit de criatura é o mesmo princípio do render paramétrico do E10 §3.1.',
      refs:'<b>Arquivos:</b> svg/icons/*.svg · svg/creature/*.svg · tokens.json'})
    + block('Pacote de SVGs', 'Gere os .svg individuais pelo botão abaixo (ícones + criaturas-exemplo) e arraste para Assets/ no Unity. Use o pacote Vector Graphics para importar.',
      `<div class="kit-actions"><span class="pill ok">svg/icons/ — ${Object.values(IC).reduce((a,c)=>a+Object.keys(c).length,0)} ícones</span><span class="pill ok">svg/creature/ — 6 variações + 4 poses</span><span class="pill mod">tokens.json</span></div>`)
    + block('Paleta em C#', '', `<pre class="code">${code}</pre>`)
    + block('Notas de pipeline', '',
      `<ul class="chk"><li class="done">Ícones monocromáticos (currentColor) → tingir em runtime sem duplicar asset</li>
        <li class="done">Criatura em camadas (corpo · padrão · olhos · ornamento) → montar por genoma em C#</li>
        <li>Sprite sheets de animação: derivar das 4 poses (idle/walk/forrage/alert)</li>
        <li>9-slice das molduras: Image type Sliced, bordas nos studs</li></ul>`);
  }

  /* ---------- Flora & props (vetor real) ---------- */
  function flora(){
    const neutral={trunk:'#6E5A3A',foliage:'#2F6F62',foliage2:'#56A06A',rock:'#8A8377',accent:'#C9A86A'};
    const keys=Object.keys(BIO.PROPS);
    const sheet=keys.map(k=>`<div class="itile" style="min-height:140px;justify-content:flex-end">${BIO.propSVG(k,neutral,0.92)}<div class="in" style="color:#EFE9F4;margin-top:8px">${k.replace(/_/g,' ')}</div></div>`).join('');
    const scenes=Object.keys(BIO.BIOMES).map(key=>{
      const b=BIO.BIOMES[key];
      return `<div class="scene-card"><div class="scene-art">${BIO.scene(key)}</div>
        <div class="scene-meta"><div class="sn">${b.nm}</div>
          <div class="sswatch">${[b.pal.foliage,b.pal.foliage2,b.pal.rock,b.pal.accent,b.ground].map(c=>`<i style="background:${c}"></i>`).join('')}</div>
          <div class="sk">${b.props.map(p=>p.replace(/_/g,' ')).join(' · ')}</div></div></div>`;
    }).join('');
    return head({eyebrow:'ASSETS · VETOR · AA §3', title:'Biomas — Flora & Props',
      desc:'Sprites vetoriais isolados (árvores, rochas, cristais, plantas) no mesmo estilo de contorno das criaturas — tintáveis e exportáveis. Cada bioma é uma seleção de props sobre uma paleta. As cenas-vinheta abaixo são o preview de produção (a arte isométrica final é pintada por cima desta direção).',
      refs:'<b>Arquivo:</b> biomes.js (propSVG · scene)'})
    + block('Catálogo de props', `${keys.length} sprites, recoloríveis por paleta de bioma.`, `<div class="grid c6">${sheet}</div>`)
    + block('Cenas por bioma (vetor)', 'Mesmos props, paletas diferentes — a "leitura" de cada bioma.', `<div class="grid c2">${scenes}</div>`);
  }

  /* ---------- Construções modulares ---------- */
  function construcoes(){
    const pal=BLD.CULTURES.floresta;
    const stages=BLD.STAGES.map(s=>`<div class="spec" style="background:var(--show)">
      <div class="st" style="color:#EFE9F4">▣ ${s.nm} <span style="font-family:var(--mono);font-size:11px;color:var(--ink-soft);margin-left:auto">${s.items.length} peças</span></div>
      <div class="bld-row">${s.items.map(k=>`<div class="bld-cell">${BLD.buildingSVG(k,pal,0.82)}<div class="bl2">${k.replace(/_/g,' ')}</div></div>`).join('')}</div></div>`).join('');
    const variants=Object.keys(BLD.CULTURES).map(ck=>`<div class="scene-card"><div class="scene-art">${BLD.village(ck)}</div>
      <div class="scene-meta"><div class="sn">${BLD.CULTURES[ck].nm}</div><div class="sk">mesma aldeia, paleta + acentos culturais</div></div></div>`).join('');
    const ruins = BLD.ruinSVG ? `<div class="bld-row">${(BLD.RUIN_SHOWCASE||[]).map(k=>`<div class="bld-cell">${BLD.ruinSVG(k,BLD.CULTURES.medieval,0.82)}<div class="bl2">${k.replace(/_/g,' ')} (ruína)</div></div>`).join('')}</div>` : '';
    const total = BLD.STAGES.reduce((a,s)=>a+s.items.length,0);
    return head({eyebrow:'ASSETS · VETOR · AA §4', title:'Construções modulares',
      desc:`Estruturas por estágio sociopolítico — da caverna pré-histórica ao galpão industrial. ${total} peças vetoriais tintáveis em 11 estágios; a mesma peça muda de cultura só pela paleta (10 culturas). Mais a variante Ruínas, que destrói qualquer estrutura proceduralmente.`,
      refs:'<b>Arquivo:</b> buildings.js + buildings-extra2–5.js (buildingSVG · village · ruinSVG)'})
    + block('Estruturas por estágio', `${total} peças em 11 estágios cronológicos.`, `<div class="grid c2">${stages}</div>`)
    + block('Variantes culturais (mesma aldeia)', 'A fogueira-coração no centro; o estilo da civilização recolore tudo. 10 culturas disponíveis.', `<div class="grid c2">${variants}</div>`)
    + block('Ruínas — variante destruída', 'Gerador procedural: come o topo, racha as paredes e adiciona entulho + vegetação. Vale para qualquer estrutura — ótimo para guerra/abandono.', ruins);
  }

  /* ---------- Efeitos & partículas ---------- */
  function efeitos(){
    const fx=[
      {svg:MT.heartstone('#5FE0C2'), nm:'Pedra-coração', f:'heartstone()', d:'O núcleo de vida da aldeia (fogueira/cristal central).'},
      {svg:MT.nudgeRipple('#8C5BAA'), nm:'Ondulação de nudge', f:'nudgeRipple()', d:'Feedback do toque do jogador no mundo.'},
      {svg:MT.signalPulse('#C0563A'), nm:'Pulso de sinal', f:'signalPulse()', d:'Emoção/estado irradiando de uma criatura.'},
      {svg:MT.lightBeam('#EAD37A'), nm:'Raio do observador', f:'lightBeam()', d:'A influência divina descendo do céu.'},
      {svg:MT.weather('rain'), nm:'Clima — chuva', f:"weather('rain')", d:'Partícula de chuva (módulo recolorível).'},
      {svg:MT.weather('snow'), nm:'Clima — neve', f:"weather('snow')", d:'Partícula de neve para tundra.'},
      {svg:MT.weather('ember'), nm:'Clima — brasas', f:"weather('ember')", d:'Faíscas para bioma vulcânico.'}
    ];
    const cells=fx.map(x=>`<div class="itile" style="min-height:150px;justify-content:flex-end;gap:6px">${x.svg}<div class="in" style="color:#EFE9F4;margin-top:6px">${x.nm}</div><div class="if">${x.f}</div></div>`).join('');
    return head({eyebrow:'ASSETS · P1 · DV §7', title:'Efeitos & partículas',
      desc:'Os elementos vivos da tela: o núcleo luminoso da aldeia, o feedback de nudge, o pulso de emoção, o raio do observador e os módulos de clima. Tudo recolorível pelos tokens.',
      refs:'<b>Arquivo:</b> meta-assets.js (fx)'})
    + block('Conjunto de efeitos', 'Vetores + gradientes radiais; animar por escala/opacidade em runtime.', `<div class="grid c4">${cells}</div>`);
  }

  /* ---------- Registro ---------- */
  const SECTIONS=[
    {id:'overview', group:'Início', title:'Visão geral', dot:'#8C5BAA', build:overview},
    {id:'cores', group:'Fundamentos', title:'Cores & Tokens', dot:'#C0563A', build:cores},
    {id:'icones', group:'Fundamentos', title:'Ícones', dot:'#3E6B8C', build:icones},
    {id:'criaturas', group:'Núcleo visual', title:'Criatura paramétrica', dot:'#2F6F62', build:criaturas, after:wireCreature},
    {id:'escrita', group:'Núcleo visual', title:'Escrita procedural', dot:'#7C4DBE', build:escrita},
    {id:'efeitos', group:'Núcleo visual', title:'Efeitos & partículas', dot:'#5FE0C2', build:efeitos},
    {id:'flora', group:'Mundo (vetor)', title:'Biomas — Flora & Props', dot:'#56A06A', build:flora},
    {id:'construcoes', group:'Mundo (vetor)', title:'Construções modulares', dot:'#C9A86A', build:construcoes},
    {id:'marca', group:'Identidade', title:'Marca & Molduras', dot:'#8C5BAA', build:marca},
    {id:'destino', group:'Identidade', title:'Cartões de Destino', dot:'#B8862F', build:destino},
    {id:'direcao', group:'Direção (pintado)', title:'Biomas & Construções', dot:'#2F6F62', build:direcao},
    {id:'unity', group:'Integração', title:'Exportar / Unity', dot:'#3E6B8C', build:unity}
  ];
  const byId=Object.fromEntries(SECTIONS.map(s=>[s.id,s]));

  function buildNav(){
    const groups={}; SECTIONS.forEach(s=>{(groups[s.group]=groups[s.group]||[]).push(s);});
    const rail=document.getElementById('rail');
    let h=`<h1>Gênese</h1><div class="sub">Biblioteca de Assets</div>`;
    Object.keys(groups).forEach(g=>{ h+=`<div style="margin-top:16px"><span style="font-family:var(--mono);font-size:10px;letter-spacing:2px;text-transform:uppercase;color:var(--ink-soft);margin:0 6px 7px;display:block">${g}</span>`;
      groups[g].forEach(s=>{ h+=`<button class="nav-item" data-id="${s.id}"><span class="dot" style="background:${s.dot}"></span>${s.title}<span class="num">${String(SECTIONS.indexOf(s)+1).padStart(2,'0')}</span></button>`; }); h+=`</div>`; });
    rail.innerHTML=h;
    rail.querySelectorAll('.nav-item').forEach(b=>b.addEventListener('click',()=>{location.hash=b.dataset.id;}));
    const sel=document.getElementById('mobileSel');
    sel.innerHTML=Object.keys(groups).map(g=>`<optgroup label="${g}">`+groups[g].map(s=>`<option value="${s.id}">${s.title}</option>`).join('')+`</optgroup>`).join('');
    sel.addEventListener('change',()=>location.hash=sel.value);
  }
  function render(){
    const id=location.hash.replace('#','')||SECTIONS[0].id; const s=byId[id]||SECTIONS[0];
    const stage=document.getElementById('stage');
    stage.innerHTML=`<div class="sec">${s.build()}</div>`+`<div class="foot">GÊNESE · assets de produção (vetor) — ícones, criatura paramétrica completa, glifos, marca, molduras, biomas (flora + cenas) e construções modulares (11 estágios · 10 culturas · ruínas), tudo recolorível. Fonte de verdade: AA · DV · GDD.</div>`;
    window.scrollTo(0,0);
    document.querySelectorAll('.nav-item').forEach(b=>b.classList.toggle('active',b.dataset.id===s.id));
    const sel=document.getElementById('mobileSel'); if(sel)sel.value=s.id;
    if(s.after) s.after();
  }
  buildNav();
  window.addEventListener('hashchange',render);
  render();
})();

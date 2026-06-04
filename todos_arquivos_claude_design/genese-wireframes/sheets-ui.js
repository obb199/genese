/* ============================================================
   GÊNESE Wireframes — UI / screen sheets
   ============================================================ */
(function(){
const W = window.WF;

/* ---------- 1. MUNDO VIVO (HUD) ---------- */
function mundoVivo(){
  const toolbar = `
    <div class="toolbar tb-bottom" id="hudToolbar">
      <div class="tbtn" title="Observar">👁</div>
      <div class="tbtn" title="Ambiente / Pressão">🌿</div>
      <div class="tbtn" title="Gráficos / Estatísticas">📊</div>
      <div class="tbtn" title="Povo / Figuras">👥</div>
      <div class="tbtn" title="Códice cultural">📖</div>
      <div class="tbtn ctx" title="Ação contextual do bioma">✦</div>
    </div>`;
  const world = `
  <div class="world" id="hudWorld">
    <div class="world-tag"><span><b>MUNDO VIVO</b>simulação em tempo real — criaturas, ambiente,<br>construções e território, vistos de cima em ângulo suave</span></div>

    <div class="hud hud-box hud-tl">
      <span class="lbl">ATENÇÃO</span>
      <div class="att"><span class="runeic">✦</span><span class="track"><i></i></span></div>
    </div>

    <div class="hud hud-box hud-tc">CICLO: <b>233</b><br>TEMPO: <b>16:42:19</b></div>

    <div class="hud hud-box hud-tr">
      <div class="pop"><span class="ggic">☻</span> POPULAÇÃO: 215</div>
      <div class="track"><i></i></div>
      <div class="subs"><span>☖ 72</span><span>🔥 96</span><span>♥ 32</span></div>
    </div>

    <div class="hud-cards">
      <div class="hud-box hud-card"><div class="ct">MELHORIA</div><div class="cv">Filtros de toxinas</div></div>
      <div class="hud-box hud-card"><div class="ct">PLANO DE MIGRAÇÃO</div><div class="cv">Zona limpa</div></div>
      <div class="hud-box hud-card"><div class="ct">PESQUISA</div><div class="cv">Resistência à contaminação</div></div>
    </div>

    ${toolbar}
  </div>`;

  return W.head({
    eyebrow:'TELAS · P0 · GDD §9.3',
    title:'Mundo Vivo — HUD',
    desc:'A tela principal e sempre presente: a simulação ao vivo com uma moldura de leitura instantânea por cima. Tudo que o jogador lê de relance mora aqui; os painéis profundos abrem sob demanda. Reconstruído a partir dos seus concepts.',
    refs:'<b>Origem:</b> AA §5.1 (HUD do mundo vivo) · M12 · M13 · GDD §4.3, §5.1'
  })
  + W.block('Layout do HUD sobre o mundo',
    'Use o seletor de <b>posição da barra de ferramentas</b> nos Tweaks (canto superior direito) para comparar barra inferior, central, à esquerda ou à direita.',
    world)
  + W.callouts([
    {t:'Canto sup. esquerdo — <b>ATENÇÃO</b>: o recurso central do jogador. Regenera devagar e cresce conforme a civilização "crê" na sua presença (laço com a religiosidade, GDD §5.1).'},
    {t:'Centro — <b>CICLO + TEMPO</b>: relógio da partida; controla velocidade (pausar / acelerar), GDD §2.4.', k:'note'},
    {t:'Canto sup. direito — <b>POPULAÇÃO</b> + 3 sub-indicadores (indivíduos · sustento · moral coletiva). Leitura rápida do "humor" da civilização.'},
    {t:'Coluna direita — <b>MELHORIA / MIGRAÇÃO / PESQUISA</b>: o que a civilização decidiu por conta própria. São <b>leitura, não comando</b> — você influencia, eles escolhem.', k:'note'},
    {t:'Barra — verbos de observação: Observar · Ambiente · Gráficos · Povo · Códice. O botão ✦ é a <b>ação contextual do bioma</b> (muda de ícone: biohazard, floco de neve, vulcão…).'}
  ])
  + W.block('Estados da moldura (mesma tela, leituras diferentes)','',
    W.grid([
      W.slot({label:'Modo Observar',cap:'foco livre — densidade de eventos sobe onde se olha',tint:'player',h:104}),
      W.slot({label:'Modo Pressão Ambiental',cap:'pincéis de clima / recurso / geografia',tint:'life',h:104}),
      W.slot({label:'Modo Nudge',cap:'paleta de intervenções pontuais',tint:'faith',h:104}),
      W.slot({label:'Overlay ativo',cap:'mundo repintado por estatística',tint:'info',h:104})
    ],'cols-4'));
}

/* ---------- 2. TELA DE INFLUÊNCIA ---------- */
function influencia(){
  const nudges = [
    {label:'Sinal',cap:'luz / som / aparição',tint:'player'},
    {label:'Proteção',cap:'reduz risco local',tint:'life'},
    {label:'Faísca de mutação',cap:'↑ taxa de mutação',tint:'tension'},
    {label:'Semente de ideia',cap:'inclinação cultural',tint:'faith'},
    {label:'Inspiração',cap:'↑ chance de Figura',tint:'resource'}
  ].map(n=>W.slot(Object.assign({h:120,p:'P0'},n)));

  const pressao = [
    {label:'Clima',cap:'esfriar · aquecer · chuva · seca',tint:'info'},
    {label:'Recursos',cap:'secar rio · brotar bosque',tint:'life'},
    {label:'Perigos',cap:'predador · doença · abrigo',tint:'tension'},
    {label:'Geografia',cap:'erguer serra · abrir mar (custo altíssimo)',tint:'neutral'}
  ].map(n=>W.slot(Object.assign({h:120},n)));

  return W.head({
    eyebrow:'TELAS · P0 · GDD §5',
    title:'Tela de Influência',
    desc:'O coração mecânico — materializa "influência, não controle". Três alavancas, todas indiretas e pagas em Atenção. Nenhuma tem efeito fixo: tudo passa pelo filtro cultural antes de virar resultado.',
    refs:'<b>Origem:</b> AA §5.1 (tela de influência) · M12 · GDD §5.2, §5.3'
  })
  + W.block('Alavanca 1 — Pressão ambiental','Você altera o mundo, não as criaturas. Elas reagem por conta própria.',W.grid(pressao,'cols-4'))
  + W.block('Alavanca 2 — Nudges (intervenção pontual)','Paleta de estímulos diretos mas interpretáveis. Cada um mostra o custo em Atenção e fica esmaecido se faltar recurso.',W.grid(nudges,'cols-5'))
  + W.block('Alavanca 3 — Direcionamento de atenção','',
    W.grid([
      W.slot({label:'Cursor de foco',cap:'onde você olha floresce mais',tint:'player',h:108,p:'P0'}),
      W.slot({label:'Medidor de Atenção',cap:'gasto / regeneração',tint:'player',h:108,p:'P0'}),
      W.slot({label:'Histórico de intervenções',cap:'o que você fez e como foi lido',tint:'neutral',h:108})
    ],'cols-3'))
  + W.callouts([
    {t:'<b>Regra de ouro:</b> o mesmo Sinal pode ser chamado divino numa cultura, mau presságio em outra, fenômeno curioso numa terceira. A UI deve sugerir incerteza — nunca prometer um efeito.',k:'note'},
    {t:'Confirmar um nudge custa Atenção e abre um "recibo" sóbrio: <i>você plantou um estímulo; a resposta é deles</i>.'}
  ]);
}

/* ---------- 3. PAINEL DA CIVILIZAÇÃO ---------- */
function civilizacao(){
  const macro = W.stats([
    {nm:'Agressividade',val:64,tint:'tension'},
    {nm:'Expansionismo',val:48,tint:'tension'},
    {nm:'Taxa reprodutiva',val:71,tint:'life'},
    {nm:'Coesão social',val:55,tint:'life'},
    {nm:'Abertura / Xenofobia',val:33,tint:'info'},
    {nm:'Inovação',val:60,tint:'info'},
    {nm:'Religiosidade',val:78,tint:'faith'},
    {nm:'Igualitarismo',val:42,tint:'neutral'},
    {nm:'Sustentabilidade',val:29,tint:'resource'},
    {nm:'Moral coletiva',val:51,tint:'life'},
    {nm:'Estabilidade',val:46,tint:'neutral'},
    {nm:'Curiosidade coletiva',val:67,tint:'info'}
  ]);
  return W.head({
    eyebrow:'TELAS · P1 · GDD §4.3',
    title:'Painel da Civilização',
    desc:'O painel macro. Centenas de números, mas o princípio é um: as macro-estatísticas emergem das micro. O jogador observa e influencia condições — nunca define um valor direto.',
    refs:'<b>Origem:</b> AA §5.1 · M13 · GDD §4.3'
  })
  + W.grid([
    W.panel('Macro-estatísticas', macro + W.legend([
      {c:'var(--tension)',t:'tensão'},{c:'var(--life)',t:'vitalidade'},
      {c:'var(--faith)',t:'fé'},{c:'var(--info)',t:'cognição'},{c:'var(--resource)',t:'recurso'}
    ]), {dots:true}),
    W.panel('Estágio & tendências',
      W.slot({label:'Estágio sociopolítico atual',cap:'Bando → Tribo → Aldeia → Estado → Império',tint:'player',h:74})
      + `<div style="height:12px"></div>`
      + W.slot({label:'Sparklines de tendência',cap:'cada estatística ao longo do tempo',tint:'info',h:110})
      + `<div style="height:12px"></div>`
      + W.slot({label:'Eventos recentes',cap:'marcos, crises, surgimento de Figuras',tint:'neutral',h:96}),
      {dots:true})
  ],'cols-2')
  + W.callouts([
    {t:'Cada barra abre um <b>histograma de dispersão</b>: não só a média da população, mas o quanto os indivíduos divergem dela (semente de cismas e especiação).',k:'note'},
    {t:'Estatísticas avançadas (belicismo naval, pietas funerária, complexidade artística…) só aparecem quando a civilização cruza o marco que as torna reais.'}
  ]);
}

/* ---------- 4. FICHA DE INDIVÍDUO / FIGURA ---------- */
function ficha(){
  return W.head({
    eyebrow:'TELAS · P1 · GDD §4.1–4.2',
    title:'Ficha de Indivíduo / Figura',
    desc:'O zoom máximo: uma única criatura. Três blocos — genética herdada, traços adquiridos na vida, papel social. Algumas criaturas viram Figuras: profetas, inventores, tiranos cujo destino muda toda a civilização.',
    refs:'<b>Origem:</b> AA §5.1 · M05 · GDD §4.1, §4.2'
  })
  + W.grid([
    W.panel('Identidade',
      W.slot({label:'Retrato paramétrico',cap:'render por genoma + cultura (DV §4)',tint:'player',h:170})
      + `<div style="height:10px"></div>`
      + W.slot({label:'Nome no idioma local',cap:'gerado pela escrita procedural (M08)',tint:'faith',h:56})
      + `<div style="height:10px"></div>`
      + W.slot({label:'⬥ Badge "Figura notável"',cap:'aparece só para indivíduos excepcionais',tint:'tension',h:56})),
    W.panel('Genética (herdada)',
      W.stats([
        {nm:'Força',val:58,tint:'tension'},{nm:'Velocidade',val:71,tint:'info'},
        {nm:'Percepção',val:64,tint:'info'},{nm:'Fertilidade',val:40,tint:'life'},
        {nm:'Longevidade',val:52,tint:'life'},{nm:'Resist. doença',val:33,tint:'tension'}
      ])
      + W.slot({label:'Genoma comportamental',cap:'24 predisposições (M02) — medo↔coragem, curiosidade, altruísmo…',tint:'neutral',h:90}),{dots:true}),
    W.panel('Adquirido & Social',
      W.slot({label:'Habilidades aprendidas',cap:'caça · construção · cura',tint:'info',h:64})
      + `<div style="height:9px"></div>`
      + W.slot({label:'Memórias & traumas marcantes',tint:'tension',h:64})
      + `<div style="height:9px"></div>`
      + W.slot({label:'Papel & prestígio',cap:'líder · curandeiro · artista',tint:'player',h:64})
      + `<div style="height:9px"></div>`
      + W.slot({label:'Relações',cap:'aliados · rivais · parentes',tint:'life',h:64}))
  ],'cols-3')
  + W.callouts([
    {t:'Da ficha de uma Figura saem dois botões de influência: <b>Proteção</b> (reduzir seu risco) e ignorar. A morte prematura de uma Figura é um dos momentos mais consequentes do jogo.',k:'note'}
  ]);
}

/* ---------- 5. CÓDICE CULTURAL / RELIGIOSO ---------- */
function codice(){
  return W.head({
    eyebrow:'TELAS · P1 · GDD §3.7–3.8',
    title:'Códice Cultural / Religioso',
    desc:'A enciclopédia viva: o filtro que reinterpreta todas as suas intervenções. Crenças, tabus, mitos, arte, idioma e — central neste jogo — a imagem que a civilização faz de você.',
    refs:'<b>Origem:</b> AA §5.1 · M09 · M10 · GDD §3.7, §3.8, §5.3'
  })
  + W.grid([
    W.panel('Cultura',
      W.slot({label:'Valores & tabus',cap:'o sagrado e o proibido',tint:'player',h:88})
      + `<div style="height:9px"></div>`
      + W.slot({label:'Mitos & ritos',cap:'nascimento · morte · colheita',tint:'faith',h:88})
      + `<div style="height:9px"></div>`
      + W.slot({label:'Arte & ornamento',cap:'padrões visíveis em corpos e construções',tint:'resource',h:88})),
    W.panel('Religião',
      W.slot({label:'Estágio',cap:'Animismo → Panteão → Culto → Doutrina → Teologia',tint:'faith',h:62})
      + `<div style="height:10px"></div>`
      + W.stats([
        {nm:'Fervor',val:78,tint:'faith'},{nm:'Dogmatismo',val:61,tint:'tension'},
        {nm:'Organização',val:44,tint:'neutral'}
      ])
      + W.slot({label:'Imagem do jogador',cap:'deus · trapaceiro · força da natureza · demônio',tint:'player',h:74}),{dots:true}),
    W.panel('Idioma emergente',
      W.slot({label:'Sistema de escrita',cap:'glifos procedurais (M08) — ilegível no início',tint:'info',h:118})
      + `<div style="height:9px"></div>`
      + W.slot({label:'Régua de tradução',cap:'gradualmente "traduzível" pelo jogador',tint:'neutral',h:62}))
  ],'cols-3')
  + W.callouts([
    {t:'A escrita aparece como <b>elemento gráfico</b> por toda parte (construções, artefatos, Crônica). Cada civilização tem um sistema de símbolos com identidade própria — ver a folha "Escrita procedural".',k:'note'}
  ]);
}

/* ---------- 6. MAPA + OVERLAYS ---------- */
function mapa(){
  const ramps = W.ramp({label:'Agressividade',stops:['#7E8CA0','#A8806F','#C0563A','#8E2D17'],lo:'calmo',hi:'violento'})
    + W.ramp({label:'Recursos',stops:['#8A7C5E','#B8862F','#E0C46A','#F2E2A6'],lo:'seco',hi:'abundante'})
    + W.ramp({label:'Fé / religiosidade',stops:['#9A93A8','#A07CC4','#7C4DBE','#5E2E9E'],lo:'laico',hi:'sagrado'})
    + W.ramp({label:'Densidade populacional',stops:['#CFE0D8','#7FB29E','#2F6F62','#16463C'],lo:'vazio',hi:'denso'})
    + W.ramp({label:'Divergência linguística',stops:['#3E6B8C','#6E8AA0','#A77FA0','#C0563A'],lo:'mesma língua',hi:'incompreensão'});
  return W.head({
    eyebrow:'TELAS · P1 · GDD §8.5',
    title:'Mapa + Overlays',
    desc:'A visão ampla. Sobreposições alternáveis repintam o mundo por estatística — localizar tensões e padrões espaciais num instante. As cores são consistentes com as paletas semânticas, então o jogador "aprende a ler" o mundo.',
    refs:'<b>Origem:</b> AA §5.1, §5.2 · M13 · DV §3.2, §6.1'
  })
  + W.grid([
    `<div>`+ W.slot({label:'MAPA-MÚNDI',cap:'territórios, fronteiras, múltiplas civilizações, rotas de migração',tint:'info',h:300}) +`</div>`,
    W.panel('Seletor de overlays', ramps + W.legend([{c:'var(--player)',t:'cor = dado, não só estética'}]),{dots:true})
  ],'cols-2')
  + W.block('Botões de overlay (alternáveis)','',
    W.grid([
      W.slot({label:'Agressividade',tint:'tension',h:70}),
      W.slot({label:'Recursos',tint:'resource',h:70}),
      W.slot({label:'Fé',tint:'faith',h:70}),
      W.slot({label:'Densidade',tint:'life',h:70}),
      W.slot({label:'Divergência linguística',tint:'info',h:70}),
      W.slot({label:'Sustentabilidade',tint:'neutral',h:70})
    ],'cols-6'))
  + W.callouts([{t:'Cada civilização constrói visivelmente diferente e marca território (totens, campos, estradas). Cicatrizes — desmatamento, poluição, campos de batalha — ficam no mapa e contam a história.',k:'note'}]);
}

/* ---------- 7. LINHA DO TEMPO / CRÔNICA ---------- */
function cronica(){
  return W.head({
    eyebrow:'TELAS · P1 · GDD §9.4',
    title:'Linha do Tempo / Crônica',
    desc:'A história narrada da partida — com nomes próprios no idioma emergente, eras, Figuras, guerras e divergências. O artefato que o jogador relembra e compartilha. Cada partida vira uma lenda diferente.',
    refs:'<b>Origem:</b> AA §5.1, §7 · M13 · GDD §9.4, §9.5'
  })
  + W.grid([
    W.panel('Crônica narrativa',
      W.slot({label:'Cabeçalho da civilização',cap:'nome · espécie · destino (se houver)',tint:'player',h:64})
      + `<div style="height:10px"></div>`
      + W.slot({label:'Eras & marcos',cap:'linguagem · agricultura · escrita · especiação',tint:'faith',h:120})
      + `<div style="height:10px"></div>`
      + W.slot({label:'Texto narrado com nomes próprios',cap:'gerado do mesmo estado simulado',tint:'neutral',h:100}),{dots:true}),
    W.panel('Lapso de evolução (GIF)',
      W.slot({label:'Quadro do "time-lapse"',cap:'bioma mudando de cor, fronteiras avançando, populações migrando e divergindo — em segundos',tint:'info',h:190})
      + `<div style="height:10px"></div>`
      + W.slot({label:'Controles + exportar',cap:'gerar · reproduzir · compartilhar (com moldura "de Gênese")',tint:'player',h:64}),{dots:true})
  ],'cols-2')
  + W.callouts([
    {t:'<b>Leitura dupla:</b> mundo vivo + overlays = intuição imediata; gráficos, fichas e códice = precisão sob demanda. O jogador nunca é obrigado a abrir planilhas, mas pode descer ao detalhe sempre.',k:'note'}
  ]);
}

window.SHEETS_UI = [
  {id:'hud', group:'Telas (UI)', title:'Mundo Vivo (HUD)', dot:'var(--player)', build:mundoVivo},
  {id:'influencia', group:'Telas (UI)', title:'Tela de Influência', dot:'var(--player)', build:influencia},
  {id:'civilizacao', group:'Telas (UI)', title:'Painel da Civilização', dot:'var(--info)', build:civilizacao},
  {id:'ficha', group:'Telas (UI)', title:'Ficha de Figura', dot:'var(--info)', build:ficha},
  {id:'codice', group:'Telas (UI)', title:'Códice Cultural', dot:'var(--faith)', build:codice},
  {id:'mapa', group:'Telas (UI)', title:'Mapa + Overlays', dot:'var(--info)', build:mapa},
  {id:'cronica', group:'Telas (UI)', title:'Linha do Tempo / Crônica', dot:'var(--faith)', build:cronica}
];
})();

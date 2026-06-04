/* ============================================================
   GÊNESE Wireframes — asset layout sheets
   ============================================================ */
(function(){
const W = window.WF;

/* ---------- CRIATURAS ---------- */
function criaturas(){
  const sil = ['Forma-mãe','Var. 1','Var. 2','Var. 3','Var. 4','Var. 5+']
    .map((l,i)=>W.slot({label:l,cap:i?'silhueta divergente':'corpo-base de tudo',tint:i?'neutral':'player',h:120,p:'P0',mod:'modular'}));

  const parts = [
    {label:'Camadas de textura',cap:'pelo · escama · pele · casca',tint:'neutral',p:'P1'},
    {label:'Apêndices modulares',cap:'membros · antenas · caudas · asas',tint:'neutral',p:'P1'},
    {label:'Olhos / sentidos',cap:'ligados ao gene de percepção',tint:'info',p:'P1'},
    {label:'Rampas de cor do corpo',cap:'genética + ambiente',tint:'life',p:'P0'}
  ].map(o=>W.slot(Object.assign({h:118,mod:'modular'},o)));

  const anim = [
    ['Ocioso','idle / respiração','P0'],['Locomoção','andar · correr','P0'],
    ['Forragear','coletar recurso','P0'],['Alerta / Fugir','detecção de ameaça','P0'],
    ['Cor de sinal / postura','comunicação não-verbal','P0'],['Cortejo','seleção de parceiro','P1'],
    ['Conflito / Ataque','disputa · combate','P1'],['Cuidado parental','investimento na prole','P1'],
    ['Trabalho / Construção','erguer estruturas','P1'],['Morte','fim do indivíduo','P1']
  ].map(([l,c,p])=>W.slot({label:l,cap:c,p,tint:'neutral',h:96,mod:'modular'}));

  const orn = [
    {label:'Pinturas corporais',cap:'identidade tribal / papel',tint:'resource',p:'P1'},
    {label:'Adornos de papel social',cap:'líder · curandeiro · artesão',tint:'player',p:'P1'},
    {label:'Marcas religiosas',cap:'símbolos de fé no corpo',tint:'faith',p:'P2'}
  ].map(o=>W.slot(Object.assign({h:110,mod:'modular'},o)));

  return W.head({
    eyebrow:'ASSETS · NÚCLEO VISUAL · AA §2',
    title:'Criaturas',
    desc:'O sistema mais importante e mais modular. Tudo se compõe de uma forma-mãe parametrizada por genoma (cor, textura, forma, tamanho) e cultura (ornamentos). Nada de sprites fechados — genética e cultura viram pixels.',
    refs:'<b>Origem:</b> AA §2 · DV §4 · GDD §8.3 · M01–M03'
  })
  + W.block('2.1 · Base & partes modulares','A forma-mãe deve ser reconhecível mesmo minúscula na tela. Cada parâmetro varia isoladamente.',
    W.grid(sil,'cols-6') + `<div style="height:14px"></div>` + W.grid(parts,'cols-4'))
  + W.block('Evolução & especiação visíveis','Mostrar como uma linhagem se transforma ao longo de eras e como duas populações divergem até virarem espécies visivelmente distintas.',
    W.grid([
      W.slot({label:'Era I',cap:'linhagem-base',tint:'life',h:108}),
      W.slot({label:'Era II',cap:'deriva acumulada',tint:'life',h:108}),
      W.slot({label:'Ramo A — clima frio',cap:'maior · peludo',tint:'info',h:108}),
      W.slot({label:'Ramo B — árido',cap:'menor · claro',tint:'resource',h:108}),
      W.slot({label:'Espécies distintas',cap:'além do limiar de incompatibilidade',tint:'tension',h:108})
    ],'cols-5'))
  + W.block('2.2 · Estados de animação','Por criatura modular. Animação procedural simples (squash & stretch, balanço, respiração).',W.grid(anim,'cols-5'))
  + W.block('2.3 · Ornamentos culturais','Camada de cultura sobreposta ao corpo.',W.grid(orn,'cols-3'))
  + W.block('Checklist — Criaturas','',
    W.checklist([
      'Forma-mãe aprovada e funcional no sistema paramétrico',
      'Pelo menos 5 variações de silhueta para especiação',
      'Camadas de textura (pelo/escama/pele/casca) montáveis',
      'Apêndices, olhos e rampas de cor modulares prontos',
      'Animações P0 entregues (idle, locomoção, forragear, alerta, sinal/postura)',
      'Animações P1 entregues (cortejo, conflito, parental, trabalho, morte)',
      'Ornamentos culturais (pinturas, adornos, marcas religiosas) montáveis'
    ]));
}

/* ---------- AMBIENTE & BIOMAS ---------- */
function ambiente(){
  const biomes = [
    {label:'Pradaria / Savana',cap:'terreno-base inicial',tint:'life',p:'P0'},
    {label:'Água (rio/lago/mar)',cap:'corpos d’água',tint:'info',p:'P0'},
    {label:'Floresta',cap:'mata densa',tint:'life',p:'P1'},
    {label:'Deserto',cap:'árido',tint:'resource',p:'P1'},
    {label:'Tundra / Gelo',cap:'frio',tint:'info',p:'P1'},
    {label:'Montanha / Rocha',cap:'relevo e barreiras',tint:'neutral',p:'P1'},
    {label:'Pântano / Úmido',cap:'transição',tint:'life',p:'P2'},
    {label:'Vulcânico',cap:'lava · basalto',tint:'tension',p:'P2'}
  ].map(o=>W.slot(Object.assign({h:120},o)));

  const seasons = ['Primavera','Verão','Outono','Inverno']
    .map(s=>W.slot({label:s,cap:'mesma região, estação diferente',tint:'neutral',h:96,p:'P1'}));

  const nat = [
    {label:'Vegetação colhível',cap:'plantas · frutos · árvores',tint:'life',p:'P0'},
    {label:'Fontes de água / material',cap:'nascentes · jazidas · pedras',tint:'info',p:'P0'},
    {label:'Predadores / perigos',cap:'ameaças não-jogáveis',tint:'tension',p:'P1'},
    {label:'Efeitos: seca · cheia · neve',cap:'estados visuais do clima',tint:'info',p:'P1'},
    {label:'Catástrofes: vulcão · terremoto',cap:'geológicas',tint:'tension',p:'P2'},
    {label:'Cicatrizes ambientais',cap:'desmatamento · poluição · batalha',tint:'neutral',p:'P2'}
  ].map(o=>W.slot(Object.assign({h:110},o)));

  return W.head({
    eyebrow:'ASSETS · AA §3',
    title:'Ambiente & Biomas',
    desc:'O mundo é uma tela viva: muda com clima, estação e a marca da civilização sobre ele. Biomas com identidade de cor/textura própria; cicatrizes permanecem visíveis e contam a história.',
    refs:'<b>Origem:</b> AA §3 · DV §5 · GDD §8.4 · M06 · M07'
  })
  + W.block('3.1 · Tilesets de bioma','Seus concepts já cobrem todos estes — pradaria, floresta, deserto, tundra, montanha, pântano, costa, vulcânico.',W.grid(biomes,'cols-4'))
  + W.block('Variações sazonais','Cada bioma em 4 estações — a mesma região reconhecível, recolorida.',W.grid(seasons,'cols-4'))
  + W.block('3.2 · Elementos naturais & fenômenos','',W.grid(nat,'cols-3'))
  + W.block('Checklist — Ambiente','',
    W.checklist([
      'Tilesets P0 prontos (pradaria, água)',
      'Tilesets P1 prontos (floresta, deserto, tundra, montanha)',
      'Variações sazonais de cada bioma',
      'Vegetação e fontes de recurso colhíveis',
      'Predadores / perigos ambientais',
      'Efeitos climáticos (seca, cheia, neve) e catástrofes (vulcão, terremoto)',
      'Cicatrizes ambientais persistentes'
    ]));
}

/* ---------- CONSTRUÇÕES & TERRITÓRIO ---------- */
function construcoes(){
  const prog = [
    {label:'Abrigo de bando',cap:'estrutura primitiva inicial',tint:'neutral',p:'P0'},
    {label:'Assentamento tribal',cap:'tendas · fogueira · totens',tint:'resource',p:'P1'},
    {label:'Aldeia / Cidade',cap:'casas · armazéns · praça',tint:'life',p:'P1'},
    {label:'Estruturas de estado',cap:'muralhas · monumentos',tint:'info',p:'P2'}
  ].map(o=>W.slot(Object.assign({h:132,mod:'modular'},o)));

  const civic = [
    {label:'Templos / estruturas religiosas',cap:'por estágio de religião',tint:'faith',p:'P2'},
    {label:'Mercado',cap:'onde a economia se materializa',tint:'resource',p:'P1'},
    {label:'Marcadores de território',cap:'fronteiras · totens · estradas · campos',tint:'player',p:'P1'},
    {label:'Variações por estilo cultural',cap:'mesma função, estética distinta',tint:'neutral',p:'P2',mod:'modular'}
  ].map(o=>W.slot(Object.assign({h:120},o)));

  return W.head({
    eyebrow:'ASSETS · AA §4',
    title:'Construções & Território',
    desc:'Refletem o estágio sociopolítico e o estilo artístico da cultura — duas civilizações constroem visivelmente diferente. Modular por estágio + estilo cultural.',
    refs:'<b>Origem:</b> AA §4 · DV §5 · GDD §3.9 · M05 · M09'
  })
  + W.block('Progressão por estágio','Bando → Tribo → Aldeia/Cidade → Estado. Cada salto reescreve a silhueta do assentamento.',W.grid(prog,'cols-4'))
  + W.block('Estruturas cívicas & marcação de território','',W.grid(civic,'cols-4'))
  + W.callouts([{t:'O mesmo "mercado" ou "templo" deve ter <b>variantes por cultura</b>: a Aliança da Floresta e a Legião da Ordem constroem o mesmo edifício de modos irreconhecíveis entre si.',k:'note'}])
  + W.block('Checklist — Construções','',
    W.checklist([
      'Abrigo de bando (P0) pronto',
      'Progressão tribal → aldeia/cidade → estado',
      'Templos por estágio religioso',
      'Mercado e marcadores de território',
      'Variações de estilo construtivo por cultura'
    ]));
}

/* ---------- ÍCONES & OVERLAYS ---------- */
function icones(){
  const stat = ['Agressividade','Expansionismo','Reprodução','Coesão','Abertura','Inovação','Religiosidade','Igualitarismo','Sustentabilidade','Moral','Estabilidade','Curiosidade']
    .map(n=>W.chip({glyph:'▱',name:n}));
  const res = [{g:'❖',n:'Comida'},{g:'≋',n:'Água'},{g:'▰',n:'Material'},{g:'✧',n:'Luxo'}].map(o=>W.chip({glyph:o.g,name:o.n}));
  const nud = [{g:'✦',n:'Sinal'},{g:'⛨',n:'Proteção'},{g:'✺',n:'Faísca'},{g:'❋',n:'Semente'},{g:'☼',n:'Inspiração'}].map(o=>W.chip({glyph:o.g,name:o.n}));
  const evt = [{g:'!',n:'Alerta'},{g:'★',n:'Marco'},{g:'⚔',n:'Conflito'},{g:'☂',n:'Crise'},{g:'♥',n:'Moral'},{g:'☉',n:'Presságio'}].map(o=>W.chip({glyph:o.g,name:o.n}));

  return W.head({
    eyebrow:'ASSETS · P0/P1 · AA §5.2',
    title:'Ícones & Overlays',
    desc:'Traduz centenas de estatísticas em sensação, com leitura dupla. Ícones e rampas consistentes para que a cor seja dado — não só estética. As rampas reusam as paletas semânticas.',
    refs:'<b>Origem:</b> AA §5.2 · DV §3.2, §6.2 · M12 · M13'
  })
  + W.block('Ícones de recurso','P0 — base do loop econômico.',W.grid(res,'cols-4'))
  + W.block('Ícones de nudge','P0 — as cinco intervenções pontuais.',W.grid(nud,'cols-5'))
  + W.block('Ícones de macro-estatística','P1 — um por estatística (placeholders; substituir por glifos finais).',W.grid(stat,'cols-6'))
  + W.block('Ícones de evento / emoção coletiva','',W.grid(evt,'cols-6'))
  + W.block('Cursores & indicadores de foco','',
    W.grid([
      W.slot({label:'Cursor de observação',cap:'marca onde o olhar pousa',tint:'player',h:84,p:'P0'}),
      W.slot({label:'Indicador de ação',cap:'alvo de pressão / nudge',tint:'tension',h:84,p:'P0'}),
      W.slot({label:'Pino de Figura',cap:'localiza indivíduo notável',tint:'info',h:84,p:'P1'})
    ],'cols-3'))
  + W.callouts([{t:'As <b>rampas de cor de overlay</b> vivem na folha "Mapa + Overlays" e devem bater 1:1 com qualquer barra ou ícone da mesma estatística.',k:'note'}]);
}

window.SHEETS_ASSETS = [
  {id:'criaturas', group:'Assets de arte', title:'Criaturas', dot:'var(--player)', build:criaturas},
  {id:'ambiente', group:'Assets de arte', title:'Ambiente & Biomas', dot:'var(--life)', build:ambiente},
  {id:'construcoes', group:'Assets de arte', title:'Construções & Território', dot:'var(--resource)', build:construcoes},
  {id:'icones', group:'Assets de arte', title:'Ícones & Overlays', dot:'var(--info)', build:icones}
];
})();

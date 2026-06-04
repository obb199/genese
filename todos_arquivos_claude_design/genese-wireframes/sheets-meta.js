/* ============================================================
   GÊNESE Wireframes — meta sheets (escrita, artefatos, loja/onboarding)
   ============================================================ */
(function(){
const W = window.WF;

/* ---------- ESCRITA PROCEDURAL ---------- */
function escrita(){
  const glyphs = Array.from({length:12},(_,i)=>
    W.slot({label:'Glifo '+(i+1),tint:i%3?'neutral':'faith',h:74}));
  return W.head({
    eyebrow:'ASSETS · P2 · AA §6',
    title:'Escrita Procedural & Símbolos',
    desc:'Cada civilização inventa um sistema de escrita visual próprio, que aparece em construções, artefatos e na Crônica — ilegível no início, "traduzível" depois. Trate cada idioma como arte com identidade própria.',
    refs:'<b>Origem:</b> AA §6 · DV §6.3 · M08'
  })
  + W.block('Conjunto de glifos-base','Inventário de formas a partir do qual a geração procedural compõe escritas (placeholders — substituir por sistema final).',W.grid(glyphs,'cols-6'))
  + W.block('Regras visuais de composição','Como os glifos se combinam, diferindo por cultura.',
    W.grid([
      W.slot({label:'Cultura A — angular',cap:'traços retos, vertical',tint:'info',h:108}),
      W.slot({label:'Cultura B — orgânica',cap:'curvas, contínua',tint:'life',h:108}),
      W.slot({label:'Cultura C — radial',cap:'em torno de um centro',tint:'faith',h:108})
    ],'cols-3'))
  + W.block('Artefatos com escrita','',
    W.grid([
      W.slot({label:'Tábuas / inscrições',tint:'neutral',h:104,p:'P2'}),
      W.slot({label:'Pinturas rupestres',tint:'resource',h:104,p:'P2'}),
      W.slot({label:'Estandartes',tint:'tension',h:104,p:'P2'})
    ],'cols-3'))
  + W.block('Checklist — Escrita','',
    W.checklist([
      'Conjunto de glifos-base para geração procedural',
      'Regras visuais de composição por civilização',
      'Artefatos que exibem a escrita no mundo'
    ]));
}

/* ---------- ARTEFATOS COMPARTILHÁVEIS ---------- */
function artefatos(){
  const destinos = [
    {label:'Transcendência',cap:'supera a forma biológica',tint:'faith'},
    {label:'Estagnação eterna',cap:'equilíbrio sem evolução',tint:'neutral'},
    {label:'Colapso ecológico',cap:'ambiente esgotado',tint:'resource'},
    {label:'Autodestruição',cap:'aniquilação mútua',tint:'tension'},
    {label:'Absorção',cap:'integrada por outra',tint:'info'},
    {label:'Diáspora',cap:'fragmenta e se espalha',tint:'player'},
    {label:'Continuidade',cap:'segue sem fim definido',tint:'life'}
  ].map(o=>W.slot(Object.assign({h:104},o)));

  return W.head({
    eyebrow:'ASSETS · P2 · AA §7',
    title:'Artefatos Compartilháveis',
    desc:'Peças com identidade visual própria, pensadas para viver fora do jogo. Devem ser reconhecíveis como "de Gênese" quando compartilhadas.',
    refs:'<b>Origem:</b> AA §7 · DV §7 · M13 · M14'
  })
  + W.grid([
    W.panel('Moldura do GIF de evolução',
      W.slot({label:'Enquadramento + moldura + tipografia',cap:'o lapso temporal exportado',tint:'info',h:170}),{dots:true}),
    W.panel('Layout da Crônica',
      W.slot({label:'Tipografia + moldura da história narrada',cap:'nomes próprios no idioma emergente',tint:'faith',h:170}),{dots:true})
  ],'cols-2')
  + W.block('Cartão de "destino final"','Tela-resumo do final alcançado. O jogo não tem vitória — tem destinos. Um layout, sete sabores.',W.grid(destinos,'cols-4'))
  + W.block('Checklist — Artefatos','',
    W.checklist(['Moldura / identidade do GIF de evolução','Layout da Crônica','Cartão de destino final (7 variantes)']));
}

/* ---------- MARCA, LOJA & ONBOARDING ---------- */
function loja(){
  const onboarding = W.flow([
    {n:'01', img:'LOGO + brilho · "começar"', name:'Splash / Menu', desc:'Tom contemplativo. Logo, continuar, novo mundo, opções.'},
    {n:'02', img:'mãe-criatura + texto curto', name:'Prólogo', desc:'A premissa: você observa e influencia — nunca comanda.'},
    {n:'03', img:'grade de 8+ biomas iniciais', name:'Escolher o mundo', desc:'Bioma de origem (seus 9 concepts de início de partida).'},
    {n:'04', img:'cartões de ideologia', name:'Semente cultural', desc:'Inclinação inicial sutil — pode divergir totalmente depois.'},
    {n:'05', img:'HUD com 1 dica por vez', name:'Primeiros ciclos', desc:'O jogo ensina pela evolução, não por tutorial separado.'}
  ]);

  const ideos = [
    {n:'Aliança da Floresta', c:'#2F6F62', d:'Harmonia com a natureza. Conhecimento ancestral e espiritualidade.'},
    {n:'Clãs do Deserto', c:'#B8862F', d:'Povo resiliente e orgulhoso. Honra, tradição e sobrevivência.'},
    {n:'Sindicato Tóxico', c:'#7E8C2F', d:'Tecnologia e ambição acima de tudo. Controle, poluição e opressão.'},
    {n:'Conselho Místico', c:'#8C5BAA', d:'Buscam compreender os segredos do mundo. Neutralidade e sabedoria.'},
    {n:'Congregado Científico', c:'#3E6B8C', d:'Conhecimento é poder. Busca por progresso e transcendência.'},
    {n:'Nômades do Ermo', c:'#8A8377', d:'Vagam pelas ruínas. Liberdade, adaptação e desconfiança.'},
    {n:'Legião da Ordem', c:'#C0563A', d:'Disciplina e fé acima de tudo. Expansão e controle territorial.'}
  ].map(i=>`<div class="ideo"><div class="in"><span class="ib" style="background:${i.c}"></span>${i.n}</div><div class="id">${i.d}</div></div>`).join('');

  const store = [
    {label:'Logotipo + variações',tint:'player',p:'P1'},
    {label:'Ícone do app',cap:'adaptável Android + Steam',tint:'neutral',p:'P2'},
    {label:'Capsules da Steam',cap:'vários tamanhos',tint:'info',p:'P2'},
    {label:'Capturas de tela',cap:'por plataforma',tint:'life',p:'P2'},
    {label:'Feature graphic (Play)',tint:'resource',p:'P2'},
    {label:'Trailer / vídeo',tint:'tension',p:'P2'},
    {label:'Splash / menu principal',tint:'player',p:'P1'},
    {label:'Tela de carregamento',tint:'neutral',p:'P1'}
  ].map(o=>W.slot(Object.assign({h:108},o)));

  return W.head({
    eyebrow:'TELAS + ASSETS · AA §8 · E11/E12',
    title:'Marca, Loja & Onboarding',
    desc:'O que leva a Gênese ao jogador: abertura, primeira partida e material de loja. O onboarding ensina pela própria evolução da civilização — sem tutoriais separados.',
    refs:'<b>Origem:</b> AA §8 · GDD §9.2 · E10/E11/E12'
  })
  + W.block('Fluxo de onboarding (primeira partida)','Da abertura aos primeiros ciclos. Cada quadro é uma tela; o conteúdo de arte sai dos seus concepts.',onboarding)
  + W.block('Semente cultural — ideologias','As 7 inclinações do seu mapa-múndi. São pontos de partida sutis: a cultura emergente pode reforçá-las, distorcê-las ou abandoná-las por completo.',
    `<div class="grid cols-3" style="align-items:start">${ideos}</div>`)
  + W.callouts([{t:'Coerente com "influência, não controle": a ideologia inicial <b>não trava</b> a partida. É só a primeira lente cultural — pode virar o oposto com o tempo.',k:'note'}])
  + W.block('Assets de marca & loja','',W.grid(store,'cols-4'))
  + W.block('Checklist — Marca & loja','',
    W.checklist([
      'Logotipo e variações',
      'Ícone do app (adaptável Android + Steam)',
      'Capsules da Steam em todos os tamanhos exigidos',
      'Capturas de tela por plataforma',
      'Banner / feature graphic da Play',
      'Trailer / vídeo',
      'Splash e menu principal',
      'Tamanhos/formatos conferidos na documentação atual das lojas'
    ]));
}

window.SHEETS_META = [
  {id:'escrita', group:'Meta & Loja', title:'Escrita Procedural', dot:'var(--faith)', build:escrita},
  {id:'artefatos', group:'Meta & Loja', title:'Artefatos Compartilháveis', dot:'var(--info)', build:artefatos},
  {id:'loja', group:'Meta & Loja', title:'Marca, Loja & Onboarding', dot:'var(--player)', build:loja}
];
})();

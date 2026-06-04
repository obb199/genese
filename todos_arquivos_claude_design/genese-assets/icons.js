/* ============================================================
   GÊNESE — Icon library (SVG, monochrome, tintável)
   viewBox 24×24 · stroke=currentColor · fill por forma
   ============================================================ */
(function(){
  const F = 'fill="currentColor" stroke="none"';   // filled shape shorthand

  // category -> { name: {label, svg} }
  const ICONS = {
    'Barra de ferramentas': {
      observar:  {label:'Observar',  svg:`<path d="M2 12s3.6-6.5 10-6.5S22 12 22 12s-3.6 6.5-10 6.5S2 12 2 12Z"/><circle cx="12" cy="12" r="2.7"/>`},
      ambiente:  {label:'Ambiente',  svg:`<path d="M5 20c-1-8 5-15 14-15 1 8-5 15-14 15Z"/><path d="M5 20C8 14 12 10.5 17 8.5"/>`},
      graficos:  {label:'Gráficos',  svg:`<line x1="5" y1="21" x2="5" y2="14" stroke-width="2.6"/><line x1="12" y1="21" x2="12" y2="6" stroke-width="2.6"/><line x1="19" y1="21" x2="19" y2="10" stroke-width="2.6"/>`},
      povo:      {label:'Povo',      svg:`<circle cx="9" cy="8.5" r="3"/><path d="M3.5 19.5c0-3.3 2.5-5.5 5.5-5.5s5.5 2.2 5.5 5.5"/><circle cx="17" cy="9.5" r="2.2"/><path d="M16 14c2.4 0 4.5 1.8 4.5 5"/>`},
      codice:    {label:'Códice',    svg:`<path d="M12 5.5C12 5.5 9.5 4 5.5 4S4 4.7 4 5.5V18c0 .8 2 .8 5.5.8S12 19.5 12 19.5"/><path d="M12 5.5C12 5.5 14.5 4 18.5 4S20 4.7 20 5.5V18c0 .8-2 .8-5.5.8S12 19.5 12 19.5"/><line x1="12" y1="5.5" x2="12" y2="19.5"/>`},
      contextual:{label:'Ação contextual', svg:`<circle cx="12" cy="12" r="3.2"/><path d="M12 2.5v3M12 18.5v3M2.5 12h3M18.5 12h3M5 5l2.2 2.2M16.8 16.8 19 19M19 5l-2.2 2.2M7.2 16.8 5 19"/>`}
    },

    'Bioma (contextual)': {
      contaminacao:{label:'Contaminação', svg:`<circle cx="12" cy="13.5" r="2.3"/><path d="M12 11.2V4.5M12 4.5c-1.6 0-3 1-3.6 2.4M12 4.5c1.6 0 3 1 3.6 2.4"/><path d="M10 15.2 4.2 18.6M4.2 18.6c-.8-1.4-.8-3.1 0-4.5M4.2 18.6c.8 1.4 2.3 2.2 3.9 2.2"/><path d="M14 15.2l5.8 3.4M19.8 18.6c.8-1.4.8-3.1 0-4.5M19.8 18.6c-.8 1.4-2.3 2.2-3.9 2.2"/>`},
      gelo:        {label:'Gelo / Frio', svg:`<path d="M12 2v20M3.4 7l17.2 10M20.6 7 3.4 17"/><path d="M12 6.5 9.5 5M12 6.5 14.5 5M12 17.5 9.5 19M12 17.5 14.5 19M6.7 9.2 6.4 6.4M6.7 9.2 4 9M17.3 14.8l.3 2.8M17.3 14.8 20 15M6.7 14.8 4 15M6.7 14.8l-.3 2.8M17.3 9.2 20 9M17.3 9.2l.3-2.8"/>`},
      vulcao:      {label:'Vulcânico', svg:`<path d="M3 21 9 11h6l6 10Z"/><path d="M9.5 11 11 5l1 2.2 1-3.2 1.5 7"/><path d="M8.5 21l1.8-3.5 1.7 1.8 1.7-2.6 1.8 4.3"/>`},
      arido:       {label:'Árido / Sol', svg:`<circle cx="12" cy="12" r="4.2"/><path d="M12 2.5v2.4M12 19.1v2.4M2.5 12h2.4M19.1 12h2.4M5.2 5.2l1.7 1.7M17.1 17.1l1.7 1.7M18.8 5.2l-1.7 1.7M6.9 17.1l-1.7 1.7"/>`},
      ciencia:     {label:'Científico', svg:`<circle cx="12" cy="12" r="2.6"/><circle cx="12" cy="12" r="6.5"/><path d="M12 5.5v-3M12 21.5v-3M5.5 12h-3M21.5 12h-3"/>`}
    },

    'Recursos': {
      comida:  {label:'Comida',   svg:`<path d="M12 21c-4 0-7-3.5-7-7.5 0-3 2-5 4.5-5 .6 0 1.2.1 1.7.4M12 21c4 0 7-3.5 7-7.5 0-3-2-5-4.5-5-.6 0-1.2.1-1.7.4" ${F}/><path d="M12 9.3C12 6 13.5 3.5 17 3c.3 3-1.4 5.6-5 6.3Z" ${F}/>`},
      agua:    {label:'Água',     svg:`<path d="M12 3c4 5 6.2 8 6.2 11.2A6.2 6.2 0 0 1 5.8 14.2C5.8 11 8 8 12 3Z" ${F}/>`},
      material:{label:'Material', svg:`<path d="M12 3 21 8v8l-9 5-9-5V8Z"/><path d="M12 3v18M3 8l9 5 9-5"/>`},
      luxo:    {label:'Luxo',     svg:`<path d="M6 4h12l3 5-9 11L3 9Z"/><path d="M3 9h18M9 4 6 9l6 11M15 4l3 5-6 11"/>`}
    },

    'Nudges (intervenção)': {
      sinal:      {label:'Sinal',       svg:`<path d="M12 2.5 13.6 9 20.5 9 14.7 13 16.6 19.8 12 15.7 7.4 19.8 9.3 13 3.5 9 10.4 9Z" ${F}/>`},
      protecao:   {label:'Proteção',    svg:`<path d="M12 3 5 6v5c0 4.4 3 7.6 7 9.5 4-1.9 7-5.1 7-9.5V6Z"/><path d="M9 11.5 11.2 14 15.5 9.3"/>`},
      faisca:     {label:'Faísca de mutação', svg:`<circle cx="12" cy="12" r="2" ${F}/><path d="M12 2.5v4M12 17.5v4M2.5 12h4M17.5 12h4M5.2 5.2 8 8M16 16l2.8 2.8M18.8 5.2 16 8M8 16l-2.8 2.8"/>`},
      semente:    {label:'Semente de ideia', svg:`<path d="M12 21v-7.5"/><path d="M12 14.5c-3.3 0-5.5-2.2-5.5-5.5 3.3 0 5.5 2.2 5.5 5.5Z" ${F}/><path d="M12 12.5c2.7 0 4.5-1.8 4.5-4.5-2.7 0-4.5 1.8-4.5 4.5Z" ${F}/>`},
      inspiracao: {label:'Inspiração',  svg:`<circle cx="12" cy="10" r="4.5"/><path d="M9.5 14.5V18h5v-3.5M10 19.5h4"/><path d="M12 2.2v1.8M3.6 6 5.1 7M20.4 6 18.9 7M2.8 12h1.6M19.6 12h1.6"/>`}
    },

    'Macro-estatísticas': {
      agressividade: {label:'Agressividade', svg:`<path d="M12 3 8.5 12.5h7Z" ${F}/><line x1="12" y1="12.5" x2="12" y2="21"/>`},
      expansionismo: {label:'Expansionismo', svg:`<path d="M12 12 5 5M5 5h4M5 5v4M12 12l7-7M19 5h-4M19 5v4M12 12l-7 7M5 19h4M5 19v-4M12 12l7 7M19 19h-4M19 19v-4"/>`},
      reproducao:    {label:'Taxa reprodutiva', svg:`<ellipse cx="12" cy="13.5" rx="6" ry="7.5"/><path d="M9.5 9.5c0-2 1-3.5 2.5-4M14.5 12c0 1.5-1 2.8-2.5 3"/>`},
      coesao:        {label:'Coesão social', svg:`<circle cx="9" cy="12" r="5"/><circle cx="15" cy="12" r="5"/>`},
      abertura:      {label:'Abertura', svg:`<path d="M14 4 6 6v14h8M14 4l4 1.5V20h-4M14 4v16"/><circle cx="12" cy="12" r="1" ${F}/>`},
      inovacao:      {label:'Inovação', svg:`<path d="M12 3a6 6 0 0 0-3.5 10.8c.6.5 1 1.2 1 2V17h5v-1.2c0-.8.4-1.5 1-2A6 6 0 0 0 12 3Z"/><path d="M10 20h4M10.5 22h3"/>`},
      religiosidade: {label:'Religiosidade', svg:`<path d="M2.5 13s3.4-5.5 9.5-5.5S21.5 13 21.5 13s-3.4 5.5-9.5 5.5S2.5 13 2.5 13Z"/><circle cx="12" cy="13" r="2.4" ${F}/><path d="M12 7.5V3.5"/>`},
      igualitarismo: {label:'Igualitarismo', svg:`<path d="M12 4v16M7 20h10"/><line x1="5" y1="8" x2="19" y2="8"/><path d="M5 8 2.8 13a3 3 0 0 0 4.4 0ZM19 8l-2.2 5a3 3 0 0 0 4.4 0Z"/>`},
      sustentabilidade:{label:'Sustentabilidade', svg:`<path d="M12 21c5-1 8-5 8-10 0-1.5-.4-3-1-4-1.5 3.5-4.5 4-7 5.5-2 1.2-3 3-3 5.5"/><path d="M8 6a8 8 0 0 0 0 11"/><path d="M8 17.5 6.5 16M8 17.5 9.6 16"/>`},
      moral:         {label:'Moral coletiva', svg:`<path d="M12 20.5 4.5 13a4.6 4.6 0 0 1 6.5-6.5l1 1 1-1A4.6 4.6 0 0 1 19.5 13Z" ${F}/>`},
      estabilidade:  {label:'Estabilidade', svg:`<path d="M12 3 21 20H3Z"/><line x1="12" y1="9" x2="12" y2="20"/>`},
      curiosidade:   {label:'Curiosidade', svg:`<circle cx="10.5" cy="10.5" r="6"/><line x1="15" y1="15" x2="20.5" y2="20.5" stroke-width="2.2"/><path d="M8.6 9.2c0-1.3 1-2.2 2.2-2.2s2.1.8 2.1 2c0 1.6-2 1.6-2 3M10.9 14h0"/>`}
    },

    'Eventos & emoções': {
      colapso:   {label:'Colapso ecológico', svg:`<path d="M4 20.5h7"/><path d="M9 20.5C9 16 8.6 13 6 11.8"/><path d="M6 11.8C3.7 11 2.3 12.4 2.8 14.8 5.2 15.4 6.6 14.2 6 11.8Z" ${F}/><path d="M8.4 15.6C10.4 14.7 12 15.8 11.8 18 9.7 18.4 8.4 17.6 8.4 15.6Z" ${F}/><path d="M18.5 7v8"/><path d="M15.8 12.3 18.5 15l2.7-2.7"/>`},
      alerta:    {label:'Alerta',     svg:`<path d="M12 4 21 19H3Z"/><line x1="12" y1="10" x2="12" y2="14"/><circle cx="12" cy="16.5" r=".6" ${F}/>`},
      marco:     {label:'Marco',      svg:`<path d="M12 3 14 9 20.3 9 15.2 13 17 19.3 12 15.5 7 19.3 8.8 13 3.7 9 10 9Z"/>`},
      conflito:  {label:'Conflito',   svg:`<path d="M5 5l9 9M14 14l4 4M16 14l2-1 1 1-1 2ZM19 5l-9 9M10 14 6 18M8 14l-2-1-1 1 1 2Z"/>`},
      crise:     {label:'Crise',      svg:`<path d="M7 16a4 4 0 0 1-.5-8 5.5 5.5 0 0 1 10.6-1.4A3.6 3.6 0 0 1 17 16Z"/><path d="M12 14l-2 4M12 14l1 0-1.5 3"/>`},
      presagio:  {label:'Presságio',  svg:`<circle cx="12" cy="12" r="8"/><path d="M14.5 5a7 7 0 0 0 0 14 8 8 0 0 1 0-14Z" ${F}/>`},
      cisma:     {label:'Cisma',      svg:`<path d="M12 3v4M12 11v2M12 17v4"/><path d="M12 7 8 9M12 13l4 2"/>`}
    },

    'Cursores & foco': {
      observacao:{label:'Cursor de observação', svg:`<circle cx="12" cy="12" r="6.5"/><path d="M12 2v3M12 19v3M2 12h3M19 12h3"/><circle cx="12" cy="12" r="1.2" ${F}/>`},
      acao:      {label:'Alvo de ação', svg:`<circle cx="12" cy="12" r="8"/><circle cx="12" cy="12" r="4"/><circle cx="12" cy="12" r="1.2" ${F}/>`},
      figura:    {label:'Pino de Figura', svg:`<path d="M12 21c4-5 6-8 6-11a6 6 0 0 0-12 0c0 3 2 6 6 11Z"/><path d="M12 6.5 13 9l2.4.1-1.9 1.5.7 2.3L12 11.6 9.8 13l.7-2.3L8.6 9.1 11 9Z" ${F}/>`}
    }
  };

  // build a full <svg> string for an icon inner
  function svg(inner, size, sw){
    return `<svg viewBox="0 0 24 24" width="${size||24}" height="${size||24}" fill="none" stroke="currentColor" stroke-width="${sw||1.7}" stroke-linecap="round" stroke-linejoin="round">${inner}</svg>`;
  }

  window.GENESE_ICONS = ICONS;
  window.iconSVG = svg;
})();

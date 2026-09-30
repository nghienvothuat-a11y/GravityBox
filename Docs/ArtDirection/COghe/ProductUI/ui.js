/* Review-only UI. No Unity gameplay, save data or asset definitions are modified. */
(()=>{
 const root=document.getElementById('app'), I=window.COGHE_ICONS;
 const params=new URLSearchParams(location.search), initial=params.get('view')||'menu';
 let victoryTimer=null;
 const state={view:initial,screen:'gameplay',modal:null,music:true,sound:true,fragment:1,level:12,homeUnlocked:true,confirm:null};
 const icon=n=>I[n]||'';
 const btn=(name,action,label,cls='')=>`<button class="icon-button ${cls}" data-action="${action}" aria-label="${label}" title="${label}">${icon(name)}</button>`;
 const wordmark=()=>'<div class="wordmark" aria-label="COghe">C<span class="logo-o">O</span>ghe</div>';
 const scene=(split=false)=>`<div class="stage ${split?'fragment-stage':''}" aria-hidden="true"><img src="assets/${split?'fragments':'gameplay'}-source.png" alt=""></div>`;
 const creature=()=>'<div class="creature" aria-hidden="true"><img src="assets/creature-source.png" alt=""></div>';
 const tag=()=>`<div class="level-tag" aria-label="Level ${state.level}"><span class="number">${String(state.level).padStart(2,'0')}</span><span class="line"></span><span class="small">DAY<br>LAB</span></div>`;
 const top=()=>`<header class="topbar">${tag()}${btn('Pause','pause','Pause')}</header>`;
 const tile=(n,a,l,extra='')=>`<button class="pause-tile" data-action="${a}" aria-label="${l}" ${extra}><span class="tile-face">${icon(n)}</span><span>${l}</span></button>`;
 function menu(){
  return `<section class="screen menu-view" aria-label="Main menu"><div class="menu-brand">${wordmark()}<div class="brand-rule"></div></div><div class="menu-hero"><div class="hero-ring"></div>${creature()}<span class="specimen-dot"></span></div><div class="menu-actions"><button class="play-button primary" data-action="play">${icon('Play')}<span>Play</span></button><div class="menu-level">Level ${state.level}</div><div class="menu-secondary"><button class="menu-item" data-action="home" aria-label="${state.homeUnlocked?'Home':'Home, unlocks after level 10'}"><span class="icon-button">${icon('House')}${state.homeUnlocked?'':`<span class="badge-lock">${icon('LockKeyhole')}</span>`}</span><span>Home</span></button><button class="menu-item" data-action="story"><span class="icon-button">${icon('Film')}</span><span>Intro</span></button></div></div><div class="footer-line"></div></section>`;
 }
 function gameplay(){
  const split=state.screen==='fragments';
  return `<section class="screen gameplay-view" aria-label="Gameplay"><header class="topbar">${tag()}${btn('Pause','pause','Pause')}</header>${scene(split)}<div class="scene-touch" aria-label="Scene preview; tap for destination feedback" role="img"></div>${split?`<div class="fragment-tray" role="group" aria-label="Select a body fragment">${[0,1].map(i=>`<button class="fragment" data-action="fragment-${i}" aria-pressed="${state.fragment===i}" aria-label="Fragment ${i+1}, 50 percent"><span class="blob-dot"></span><span>50%</span></button>`).join('')}</div>`:''}${state.screen==='holding'?`<div class="release-control">${btn('Hand','release','Release object').replace('</button>',`${icon('X').replace('<svg','<svg class="release-mark"')}</button>`)}</div>`:''}<div class="camera-control">${btn('Maximize','overview','Reset camera')}</div></section>`;
 }
 function pause(){
  return `<div class="scrim"><section class="dialog" role="dialog" tabindex="-1" aria-modal="true" aria-labelledby="pause-title">${btn('X','resume','Close pause menu','ghost close-dialog')}<h1 id="pause-title">Paused</h1><div class="subline">${state.view==='home'?'Home':'Level '+state.level}</div><div class="pause-grid">${state.view==='home'?'':tile('RotateCcw','restart','Restart')+tile(state.homeUnlocked?'House':'LockKeyhole','home','Home')}${tile('Grid2X2','main-menu','Menu')}${state.view==='home'?'':tile('CircleHelp','help','How to play')}<button class="pause-tile" data-action="music" aria-label="Music ${state.music?'on':'off'}" aria-pressed="${state.music}"><span class="tile-face ${state.music?'':'music-off'}">${icon('Music2')}<span class="toggle-dot">${icon(state.music?'Check':'X')}</span></span><span>Music</span></button><button class="pause-tile" data-action="sound" aria-label="Sound ${state.sound?'on':'off'}" aria-pressed="${state.sound}"><span class="tile-face">${icon(state.sound?'Volume2':'VolumeX')}<span class="toggle-dot">${icon(state.sound?'Check':'X')}</span></span><span>Sound</span></button></div><hr><button class="resume primary" data-action="resume" aria-label="Resume game">${icon('Play')}<span>Resume</span></button></section></div>`;
 }
 function help(){
  return `<div class="scrim"><section class="dialog help-dialog" role="dialog" tabindex="-1" aria-modal="true" aria-labelledby="help-title">${btn('X','help-back','Close help','ghost close-dialog')}<h1 id="help-title">How to play</h1><div class="help-list"><div class="help-item"><div class="gesture">${icon('Pointer')}</div><div class="help-copy"><strong>Tap to guide</strong><span>Choose a surface or handle.</span></div></div><div class="help-item"><div class="gesture">${icon('Hand')}${icon('MoveHorizontal').replace('<svg','<svg class="tiny"')}</div><div class="help-copy"><strong>Drag to look</strong><span>See another side of the box.</span></div></div><div class="help-item"><div class="gesture">${icon('MoveDiagonal2')}</div><div class="help-copy"><strong>Pinch to zoom</strong><span>Bring the details closer.</span></div></div></div><p class="help-bottom">Guide COghe to the glowing exit.</p><button class="resume primary" data-action="help-back">${icon('Check')}<span>Got it</span></button></section></div>`;
 }
 function success(){
  return `<section class="screen success-view" aria-label="Level complete"><div class="success-brand"><div class="success-mark">${icon('Check')}</div><h1>Well done!</h1></div><div class="success-hero">${creature()}</div><div class="success-footer">${icon('ArrowRight')}<span>Level ${state.level+1}</span></div><div class="countdown" aria-label="Next level loads after the celebration"></div></section>`;
 }
 function home(){
  return `<section class="screen home-view" aria-label="COghe’s home"><header class="topbar">${btn('ArrowLeft','home-back','Back','quiet')}<span class="home-title">Home</span>${btn('Pause','pause-home','Pause','quiet')}</header><div class="home-hero"><div class="home-ground"></div>${creature()}</div><div class="home-actions"><button data-action="feed"><span class="icon-button">${icon('Apple')}</span><span>Feed</span></button><button data-action="pet"><span class="icon-button">${icon('Heart')}</span><span>Play</span></button><button data-action="collection"><span class="icon-button">${icon('Grid2X2')}</span><span>Collection</span></button></div><div class="footer-line"></div></section>`;
 }
 function intro(){
  return `<section class="screen intro-view" aria-label="Watch the introduction"><video playsinline poster="assets/intro-poster.jpg" preload="metadata" src="../../../Intro/COghe/preview.mp4" aria-label="Existing COghe introduction preview"></video><div class="intro-controls">${btn('ArrowLeft','menu','Back to main menu','quiet')}${btn('ChevronsRight','menu','Skip introduction','quiet')}</div><div class="intro-play"><button class="icon-button primary" data-action="start-video" aria-label="Play introduction">${icon('Play')}</button></div></section>`;
 }
 function confirm(){
  const restart=state.confirm==='restart';
  return `<div class="scrim"><section class="dialog confirm" role="dialog" tabindex="-1" aria-modal="true" aria-labelledby="confirm-title"><div class="confirmation-icon">${icon(restart?'RotateCcw':'ArrowLeft')}</div><h1 id="confirm-title">${restart?'Restart this puzzle?':'Leave this puzzle?'}</h1><p class="message">${restart?'Start this level again.':'Your level is saved.<br>This attempt will restart.'}</p><div class="confirm-actions"><button class="stay" data-action="cancel-confirm" aria-label="Keep playing">${icon('X')}</button><button class="leave" data-action="confirm" aria-label="${restart?'Restart level':'Leave level'}">${icon('Check')}</button></div></section></div>`;
 }
 function render(){
  let view=state.view;
  root.innerHTML=(view==='menu'?menu():view==='home'?home():view==='intro'?intro():view==='success'?success():gameplay())+(state.modal==='pause'?pause():state.modal==='help'?help():state.modal==='confirm'?confirm():'');
  root.dataset.view=state.view;root.dataset.modal=state.modal||'';
  const video=root.querySelector('video');if(video)video.onended=()=>go('menu');
  const focus=root.querySelector('[role=dialog]');if(focus)focus.focus({preventScroll:true});
 }
 function go(view){clearTimeout(victoryTimer);state.view=view;state.modal=null;if(view==='fragments'||view==='holding'){state.screen=view;state.view='gameplay';state.level=view==='fragments'?42:12;}else if(view==='gameplay'){state.screen='gameplay';}render();}
 function toast(message){root.querySelector('.toast')?.remove();const t=document.createElement('div');t.className='toast';t.setAttribute('role','status');t.textContent=message;root.appendChild(t);setTimeout(()=>t.remove(),2200);}
 function action(a){
  if(a==='play'){go('gameplay');return;}
  if(a==='pause'||a==='pause-home'){state.modal='pause';render();return;}
  if(a==='resume'){state.modal=null;render();return;}
  if(a==='menu'){go('menu');return;}
  if(a==='main-menu'&&state.view==='home'){go('menu');return;}
  if(a==='main-menu'||a==='restart'){state.confirm=a;state.modal='confirm';render();return;}
  if(a==='cancel-confirm'){state.modal='pause';render();return;}
  if(a==='confirm'){go(state.confirm==='restart'?'gameplay':state.confirm==='home'?'home':'menu');return;}
  if(a==='home'){
   if(!state.homeUnlocked){toast('Complete level 10 to unlock Home.');return;}
   state.returnFromHome=state.view==='gameplay'?'gameplay':'menu';
   if(state.view==='gameplay'){state.confirm='home';state.modal='confirm';render();}else go('home');return;
  }
  if(a==='home-back'){go(state.returnFromHome||'menu');return;}
  if(a==='story'){go('intro');return;}
  if(a==='start-video'){root.querySelector('.intro-play').remove();root.querySelector('video').play().catch(()=>toast('Tap to play the intro.'));return;}
  if(a==='help'){state.modal='help';render();return;}
  if(a==='help-back'){state.modal='pause';render();return;}
  if(a==='music'||a==='sound'){state[a]=!state[a];render();return;}
  if(a.startsWith('fragment-')){state.fragment=Number(a.slice(-1));render();return;}
  if(a==='release'){root.querySelector('.release-control')?.remove();toast('Released');return;}
  if(a==='overview'){toast('Overview');return;}
  if(a==='feed'||a==='pet'){const t=document.createElement('div');t.className='heart-float';t.innerHTML=icon(a==='feed'?'Apple':'Heart');root.appendChild(t);setTimeout(()=>t.remove(),1600);return;}
  if(a==='collection'){toast('Collection layout comes next.');return;}
 }
 root.addEventListener('click',e=>{const b=e.target.closest('[data-action]');if(b){action(b.dataset.action);return;}if(e.target.closest('.scene-touch')){const r=root.getBoundingClientRect(),t=document.createElement('div');t.className='touch-feedback';t.style.left=(e.clientX-r.left)+'px';t.style.top=(e.clientY-r.top)+'px';root.appendChild(t);setTimeout(()=>t.remove(),650);}});
 window.addEventListener('keydown',e=>{if(e.key==='Tab' && state.modal){const d=root.querySelector('[role=dialog]'),bs=[...d.querySelectorAll('button')],first=bs[0],last=bs[bs.length-1];if(e.shiftKey&&(document.activeElement===first||document.activeElement===d)){e.preventDefault();last.focus();}else if(!e.shiftKey&&(document.activeElement===last||document.activeElement===d)){e.preventDefault();first.focus();}}if(e.key==='Escape'){if(state.modal==='help')state.modal='pause';else if(state.modal)state.modal=null;else if(state.view==='gameplay'||state.view==='home')state.modal='pause';else return;render();}});
 // Review states are selected outside the product. There is no level picker in this UI.
 if(initial==='pause'||initial==='help'){state.view='gameplay';state.modal=initial;}
 if(initial==='fragments'){state.view='gameplay';state.screen='fragments';state.level=42;}
 if(initial==='holding'){state.view='gameplay';state.screen='holding';}
 if(initial==='menu-locked'){state.view='menu';state.homeUnlocked=false;state.level=1;}
 render();
 window.COGHE_PREVIEW={state,go,action,celebrate(){go('success');root.classList.add('celebrating');victoryTimer=setTimeout(()=>{state.level++;state.homeUnlocked=state.level>10;root.classList.remove('celebrating');go('gameplay');toast('Mockup: next puzzle loads here.');},3500);}};
})();

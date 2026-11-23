(() => {
  "use strict";
  const canvas = document.querySelector("canvas[data-game]");
  const bar = document.querySelector(".bar");
  if (!canvas || !bar) return;

  const pads = {
    Island: {
      main: [["KeyE","調べる・採集"],["Tab","家具袋"],["KeyR","回す・回収"],["Enter","決定"]],
      more: [["KeyJ","島の依頼"],["KeyU","道具・交易"],["KeyT","渡る"],["ShiftLeft","走る"],["Escape","手帳・戻る"]]
    },
    Ember: {
      main: [["KeyJ","斬撃・地区"],["Space","回避"],["KeyE","調べる"],["Enter","決定"]],
      more: [["KeyL","衝撃波"],["KeyQ","回復"],["KeyK","鍛冶"],["KeyB","図鑑"],["KeyT","帰還"],["Backspace","戻る"],["Escape","休憩"]]
    },
    Astral: {
      main: [["KeyE","調べる"],["KeyJ","職技"],["Space","待機"],["Enter","決定"]],
      more: [["KeyF","投光"],["KeyQ","薬"],["Tab","戦術図"],["KeyB","航行録"],["KeyD","今日の星図"],["Backspace","戻る"],["Escape","休憩"]]
    },
    Harvest: {
      main: [["KeyE","畑の世話"],["KeyK","料理"],["KeyO","配膳"],["Enter","決定"]],
      more: [["KeyP","種袋"],["KeyM","売店"],["KeyT","畑を移動"],["KeyN","就寝"],["KeyB","帳簿"],["KeyG","品評会"],["Digit1","切る"],["Digit2","煮る"],["Digit3","焼く"],["Digit4","和える"],["Backspace","戻る"],["Escape","休憩"]]
    },
    Voxel: {
      main: [["Space","設置"],["KeyN","次の部位"],["Tab","素材帳"],["Enter","出品・決定"]],
      more: [["KeyB","設計帳"],["KeyG","ガイド"],["KeyR","素材を回す"],["KeyV","設計を回す"],["Delete","回収"],["KeyF","採集"],["KeyZ","取り消し"],["KeyY","やり直し"],["KeyT","島を移動"],["KeyH","手帳"],["KeyQ","視点を左へ"],["KeyE","視点を右へ"],["Equal","近づく"],["Minus","離れる"],["KeyI","格子の奥"],["KeyJ","格子の左"],["KeyK","格子の手前"],["KeyL","格子の右"],["KeyU","格子を上へ"],["KeyO","格子を下へ"],["F8","絵はがき"],["F9","アルバム"],["Escape","戻る"]]
    },
    Tides: {
      main: [["Space","投げる・巻く"],["KeyE","調べる"],["KeyH","魚の手帖"],["Enter","決定"]],
      more: [["KeyT","渡航"],["KeyB","釣具店"],["KeyJ","依頼"],["KeyR","時刻"],["KeyP","ゆっくり釣り"],["KeyF","お気に入り"],["KeyX","仕掛け回収"],["F8","釣果カード"],["F9","アルバム"],["Escape","戻る"]]
    }
  };
  const pad = pads[canvas.dataset.game];
  if (!pad) return;
  const touchDevice = matchMedia("(any-pointer: coarse)").matches || navigator.maxTouchPoints > 0;
  const section = document.createElement("section");
  section.id = "touch-pad";
  section.setAttribute("aria-label", "ゲームのタッチ操作");
  const cluster = className => {
    const group = document.createElement("div");
    group.className = className;
    return group;
  };
  function button(text) {
    const el = document.createElement("button");
    el.type = "button";
    el.textContent = text;
    return el;
  }
  const move = cluster("touch-dpad");
  move.setAttribute("role","group");
  move.setAttribute("aria-label","方向");
  const actions = cluster("touch-actions");
  const extra = cluster("touch-more");
  extra.id = "touch-more-keys";
  extra.hidden = true;
  const more = button("その他の操作");
  more.className = "touch-expand";
  more.setAttribute("aria-controls",extra.id);
  more.setAttribute("aria-expanded","false");
  const toggle = button("タッチ操作");
  toggle.setAttribute("aria-controls",section.id);
  toggle.className = "touch-toggle";
  (bar.querySelector(".buttons") || bar).append(toggle);
  section.append(move,actions,more,extra);
  bar.after(section);
  function show(visible) {
    section.hidden = !visible;
    toggle.setAttribute("aria-expanded",String(visible));
    if (!visible) clearAll();
  }
  toggle.addEventListener("click",() => show(section.hidden));
  more.addEventListener("click",() => {
    clearAll();
    extra.hidden = !extra.hidden;
    more.setAttribute("aria-expanded",String(!extra.hidden));
    more.textContent = extra.hidden ? "その他の操作" : "その他を閉じる";
  });

  const nonLetters = {
    Enter: ["Enter",13], Escape:["Escape",27], Tab:["Tab",9], Space:[" ",32],
    ArrowLeft:["ArrowLeft",37], ArrowUp:["ArrowUp",38], ArrowRight:["ArrowRight",39],
    ArrowDown:["ArrowDown",40], Backspace:["Backspace",8], Delete:["Delete",46], ShiftLeft:["Shift",16],
    Minus:["-",189], Equal:["=",187], F5:["F5",116], F8:["F8",119], F9:["F9",120], F12:["F12",123]
  };
  const down = new Map();
  const pointers = new Map();
  function send(type,code) {
    let [key,number] = nonLetters[code] || [code.slice(-1).toLowerCase(),code.slice(-1).charCodeAt(0)];
    const event = new KeyboardEvent(type, {
      key, code, keyCode:number, which:number, bubbles:true, cancelable:true,
      location:code==="ShiftLeft"?1:0, shiftKey: code==="ShiftLeft" ? type==="keydown" : down.has("ShiftLeft")
    });
    // Some mobile browsers leave legacy keyboard fields at zero on constructed events.
    if (!event.keyCode) Object.defineProperty(event,"keyCode",{get:()=>number});
    if (!event.which) Object.defineProperty(event,"which",{get:()=>number});
    canvas.dispatchEvent(event);
    // Voxel uses arrow keys in books, and WASD to pan while building.
    if(canvas.dataset.game==="Voxel" && code.startsWith("Arrow"))
      send(type,{ArrowUp:"KeyW",ArrowDown:"KeyS",ArrowLeft:"KeyA",ArrowRight:"KeyD"}[code]);
  }
  function releaseKey(code) {
    const state=down.get(code);
    if (!state) return;
    clearTimeout(state.timer);
    down.delete(code);
    send("keyup",code);
    for(const el of state.buttons) el.classList.remove("is-down");
  }
  function stop(id,immediately) {
    const held=pointers.get(id);
    if (!held) return;
    pointers.delete(id);
    const state=down.get(held.code);
    if (!state) return;
    state.ids.delete(id);
    if (state.ids.size) return;
    const remaining = Math.max(0,110 - (performance.now()-state.since));
    if (immediately || remaining===0) releaseKey(held.code);
    else state.timer=setTimeout(()=>releaseKey(held.code),remaining);
  }
  function clearAll() {
    pointers.clear();
    for (const code of [...down.keys()]) releaseKey(code);
  }
  function press(id,el) {
    const code=el.dataset.code;
    const old=pointers.get(id);
    if(old?.el===el)return;
    stop(id,true);
    const previous=down.get(code);
    if(previous&&!previous.ids.size)releaseKey(code);
    let state=down.get(code);
    if(!state){
      state={ids:new Set(),buttons:new Set(),since:performance.now(),timer:0};
      down.set(code,state);
      canvas.focus({preventScroll:true});
      send("keydown",code);
    }
    state.ids.add(id);
    state.buttons.add(el);
    pointers.set(id,{code,el});
    el.classList.add("is-down");
  }
  function keycap(group,code,label,spoken) {
    const el=button(label);
    el.dataset.code=code;
    if(spoken)el.setAttribute("aria-label",spoken);
    group.append(el);
    el.addEventListener("pointerdown",e=>{
      if(e.button!==0 || !document.body.classList.contains("playing"))return;
      e.preventDefault();
      el.setPointerCapture(e.pointerId);
      press(e.pointerId,el);
    });
    el.addEventListener("pointermove",e=>{
      if(group!==move || !el.hasPointerCapture(e.pointerId))return;
      const next=document.elementFromPoint(e.clientX,e.clientY)?.closest(".touch-dpad [data-code]");
      if(next)press(e.pointerId,next);
      else stop(e.pointerId,true);
    });
    el.addEventListener("pointerup",e=>{e.preventDefault();stop(e.pointerId,false);});
    el.addEventListener("pointercancel",e=>stop(e.pointerId,true));
    el.addEventListener("lostpointercapture",e=>stop(e.pointerId,true));
    el.addEventListener("click",e=>{
      if(e.detail!==0 || !document.body.classList.contains("playing"))return;
      const id=Symbol(code);press(id,el);stop(id,false);
    });
  }
  for(const [code,glyph,name] of [["ArrowUp","↑","上"],["ArrowLeft","←","左"],["ArrowRight","→","右"],["ArrowDown","↓","下"]])
    keycap(move,code,glyph,name);
  for(const choice of pad.main)keycap(actions,...choice);
  for(const choice of pad.more)keycap(extra,...choice);
  if(canvas.dataset.game!=="Island")keycap(extra,"F5","記録を保存");
  keycap(extra,"F12","画面を撮影");
  section.addEventListener("contextmenu",e=>e.preventDefault());
  window.addEventListener("blur",clearAll);
  window.addEventListener("pagehide",clearAll);
  document.addEventListener("visibilitychange",()=>{if(document.hidden)clearAll();});
  document.addEventListener("fullscreenchange",clearAll);
  const full=document.getElementById("full");
  function touchFull(enabled) {
    document.body.classList.toggle("touch-full",enabled);
    if(full)full.textContent=enabled?"全画面を閉じる":"全画面";
  }
  if(full){
    full.addEventListener("click",event=>{
      if((section.hidden && !document.body.classList.contains("touch-full")) || !document.body.classList.contains("playing"))return;
      event.stopImmediatePropagation();
      clearAll();
      if(document.body.classList.contains("touch-full")){
        touchFull(false);
        if(document.fullscreenElement)document.exitFullscreen().catch(()=>{});
      }else{
        touchFull(true);
        // Fullscreen the page so that touch controls stay beside the canvas.
        document.documentElement.requestFullscreen?.().catch(()=>{});
      }
    },true);
    document.addEventListener("fullscreenchange",()=>{
      if(!document.fullscreenElement && document.body.classList.contains("touch-full"))touchFull(false);
    });
  }
  // Dialog controls regain keyboard navigation while the game is paused behind them.
  for(const dialog of document.querySelectorAll("dialog"))
    new MutationObserver(()=>{if(dialog.open)clearAll();}).observe(dialog,{attributes:true,attributeFilter:["open"]});
  show(touchDevice);
})();

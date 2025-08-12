// ゲームの状態管理
const gameState = {
    currentDay: 1,
    currentPeriod: 'morning', // morning, afternoon, evening
    currentScene: 'title',
    currentDialogue: 0,
    currentScenario: null,
    isTyping: false,
    autoPlay: false,
    settings: {
        textSpeed: 5,
        autoSpeed: 5,
        bgmVolume: 50,
        seVolume: 50,
        fullscreen: false
    },
    flags: {},
    affection: {
        misaki: 0,
        yukino: 0,
        ayame: 0
    },
    unlockedCGs: [],
    unlockedEndings: []
};

// 初期化
window.onload = function() {
    loadSettings();
    setupEventListeners();
    initializeAffectionDisplay();
};

// イベントリスナー設定
function setupEventListeners() {
    document.addEventListener('keydown', handleKeyPress);
    document.addEventListener('click', handleClick);
    
    // 設定スライダー
    document.getElementById('text-speed').addEventListener('input', updateTextSpeed);
    document.getElementById('auto-speed').addEventListener('input', updateAutoSpeed);
    document.getElementById('bgm-volume').addEventListener('input', updateBGMVolume);
    document.getElementById('se-volume').addEventListener('input', updateSEVolume);
}

// キー入力処理
function handleKeyPress(e) {
    if (gameState.currentScene === 'game') {
        switch(e.key) {
            case 'Enter':
            case ' ':
                advanceDialogue();
                break;
            case 'Escape':
                openMenu();
                break;
            case 's':
                if (e.ctrlKey) {
                    e.preventDefault();
                    quickSave();
                }
                break;
            case 'l':
                if (e.ctrlKey) {
                    e.preventDefault();
                    quickLoad();
                }
                break;
        }
    }
}

// クリック処理
function handleClick(e) {
    if (e.target.classList.contains('text-box') && gameState.currentScene === 'game') {
        advanceDialogue();
    }
}

// 新規ゲーム開始
function startNewGame() {
    resetGameState();
    gameState.currentScene = 'game';
    document.getElementById('title-screen').style.display = 'none';
    document.getElementById('game-screen').style.display = 'block';
    
    // プロローグ開始
    loadScenario('prologue');
}

// ゲーム状態リセット
function resetGameState() {
    gameState.currentDay = 1;
    gameState.currentPeriod = 'morning';
    gameState.currentDialogue = 0;
    gameState.flags = {};
    gameState.affection = {
        misaki: 0,
        yukino: 0,
        ayame: 0
    };
    updateStatusBar();
    updateAffectionDisplay();
}

// シナリオ読み込み
function loadScenario(scenarioId) {
    if (scenarios[scenarioId]) {
        gameState.currentScenario = scenarios[scenarioId];
        gameState.currentDialogue = 0;
        displayDialogue();
    }
}

// 会話表示
function displayDialogue() {
    const scenario = gameState.currentScenario;
    const dialogue = scenario.dialogues[gameState.currentDialogue];
    
    if (!dialogue) {
        // シナリオ終了
        endScenario();
        return;
    }
    
    // 背景変更
    if (dialogue.background) {
        changeBackground(dialogue.background);
    }
    
    // キャラクター表示
    if (dialogue.character) {
        showCharacter(dialogue.character, dialogue.expression);
    } else {
        hideCharacter();
    }
    
    // 話者名表示
    document.getElementById('speaker-name').textContent = dialogue.speaker || '';
    
    // テキスト表示
    if (dialogue.choices) {
        // 選択肢表示
        displayChoices(dialogue.choices);
        typeText(dialogue.text, false);
    } else {
        // 通常テキスト
        typeText(dialogue.text, true);
    }
    
    // エフェクト処理
    if (dialogue.effect) {
        applyEffect(dialogue.effect);
    }
}

// テキストアニメーション
function typeText(text, showIndicator) {
    const textElement = document.getElementById('dialogue-text');
    const indicator = document.getElementById('next-indicator');
    
    gameState.isTyping = true;
    indicator.style.display = 'none';
    
    let index = 0;
    const speed = 50 / gameState.settings.textSpeed;
    
    textElement.textContent = '';
    
    function typeChar() {
        if (index < text.length) {
            textElement.textContent += text[index];
            index++;
            setTimeout(typeChar, speed);
        } else {
            gameState.isTyping = false;
            if (showIndicator) {
                indicator.style.display = 'block';
            }
        }
    }
    
    typeChar();
}

// 会話を進める
function advanceDialogue() {
    if (gameState.isTyping) {
        // タイピング中はスキップ
        gameState.isTyping = false;
        const textElement = document.getElementById('dialogue-text');
        const dialogue = gameState.currentScenario.dialogues[gameState.currentDialogue];
        textElement.textContent = dialogue.text;
        document.getElementById('next-indicator').style.display = 'block';
        return;
    }
    
    const choicesContainer = document.getElementById('choices-container');
    if (choicesContainer.style.display !== 'none') {
        // 選択肢表示中は進めない
        return;
    }
    
    gameState.currentDialogue++;
    displayDialogue();
}

// 選択肢表示
function displayChoices(choices) {
    const container = document.getElementById('choices-container');
    container.innerHTML = '';
    container.style.display = 'block';
    
    choices.forEach((choice, index) => {
        const button = document.createElement('button');
        button.className = 'choice-btn';
        button.textContent = choice.text;
        button.onclick = () => selectChoice(index);
        container.appendChild(button);
    });
}

// 選択肢選択
function selectChoice(index) {
    const dialogue = gameState.currentScenario.dialogues[gameState.currentDialogue];
    const choice = dialogue.choices[index];
    
    // 好感度変更
    if (choice.affection) {
        for (let [character, value] of Object.entries(choice.affection)) {
            changeAffection(character, value);
        }
    }
    
    // フラグ設定
    if (choice.flag) {
        gameState.flags[choice.flag] = true;
    }
    
    // 次の会話へ
    if (choice.next !== undefined) {
        gameState.currentDialogue = choice.next;
    } else {
        gameState.currentDialogue++;
    }
    
    document.getElementById('choices-container').style.display = 'none';
    displayDialogue();
}

// 好感度変更
function changeAffection(character, amount) {
    gameState.affection[character] = Math.max(0, Math.min(100, 
        gameState.affection[character] + amount));
    updateAffectionDisplay();
    
    // ポップアップ表示
    showAffectionPopup(character, amount);
}

// 好感度表示更新
function updateAffectionDisplay() {
    const container = document.getElementById('affection-bars');
    container.innerHTML = '';
    
    for (let [key, value] of Object.entries(gameState.affection)) {
        const character = characters[key];
        if (!character) continue;
        
        const bar = document.createElement('div');
        bar.className = 'affection-bar';
        bar.innerHTML = `
            <span class="char-name">${character.name}</span>
            <div class="bar-container">
                <div class="bar-fill" style="width: ${value}%"></div>
            </div>
            <span class="bar-value">${value}</span>
        `;
        container.appendChild(bar);
    }
}

// 好感度ポップアップ
function showAffectionPopup(character, amount) {
    const popup = document.createElement('div');
    popup.className = 'affection-popup';
    popup.style.color = amount > 0 ? '#ff6b9d' : '#666';
    popup.textContent = `${characters[character].name} ${amount > 0 ? '+' : ''}${amount}`;
    
    document.querySelector('.main-area').appendChild(popup);
    
    setTimeout(() => {
        popup.remove();
    }, 2000);
}

// 背景変更
function changeBackground(bg) {
    const element = document.getElementById('background');
    element.className = `background bg-${bg}`;
}

// キャラクター表示
function showCharacter(characterId, expression = 'normal') {
    const element = document.getElementById('character-image');
    element.className = `character-sprite char-${characterId} expr-${expression}`;
    element.style.display = 'block';
}

// キャラクター非表示
function hideCharacter() {
    document.getElementById('character-image').style.display = 'none';
}

// エフェクト適用
function applyEffect(effect) {
    switch(effect) {
        case 'shake':
            document.querySelector('.main-area').classList.add('shake');
            setTimeout(() => {
                document.querySelector('.main-area').classList.remove('shake');
            }, 500);
            break;
        case 'flash':
            document.querySelector('.main-area').classList.add('flash');
            setTimeout(() => {
                document.querySelector('.main-area').classList.remove('flash');
            }, 300);
            break;
    }
}

// シナリオ終了
function endScenario() {
    // 時間進行
    advanceTime();
    
    // 次のシナリオ決定
    decideNextScenario();
}

// 時間進行
function advanceTime() {
    const periods = ['morning', 'afternoon', 'evening'];
    const currentIndex = periods.indexOf(gameState.currentPeriod);
    
    if (currentIndex < periods.length - 1) {
        gameState.currentPeriod = periods[currentIndex + 1];
    } else {
        gameState.currentPeriod = 'morning';
        gameState.currentDay++;
    }
    
    updateStatusBar();
}

// ステータスバー更新
function updateStatusBar() {
    const periodNames = {
        morning: '朝',
        afternoon: '昼',
        evening: '夕'
    };
    
    document.getElementById('current-day').textContent = `Day ${gameState.currentDay}`;
    document.getElementById('current-period').textContent = periodNames[gameState.currentPeriod];
}

// 次のシナリオ決定
function decideNextScenario() {
    // 好感度に基づいてイベント選択
    const maxAffection = Math.max(...Object.values(gameState.affection));
    
    if (gameState.currentDay === 1 && gameState.currentPeriod === 'morning') {
        loadScenario('day1_morning');
    } else if (maxAffection >= 30) {
        // 好感度イベント
        for (let [character, value] of Object.entries(gameState.affection)) {
            if (value === maxAffection && value >= 30) {
                loadScenario(`${character}_event1`);
                return;
            }
        }
    } else {
        // 通常イベント
        loadScenario('daily_event');
    }
}

// クイックセーブ
function quickSave() {
    const saveData = {
        gameState: gameState,
        timestamp: new Date().toISOString()
    };
    localStorage.setItem('quicksave', JSON.stringify(saveData));
    showPopup('クイックセーブしました');
}

// クイックロード
function quickLoad() {
    const saveData = localStorage.getItem('quicksave');
    if (saveData) {
        const data = JSON.parse(saveData);
        Object.assign(gameState, data.gameState);
        
        document.getElementById('title-screen').style.display = 'none';
        document.getElementById('game-screen').style.display = 'block';
        
        updateStatusBar();
        updateAffectionDisplay();
        displayDialogue();
        
        showPopup('クイックロードしました');
    } else {
        showPopup('セーブデータがありません');
    }
}

// セーブ画面表示
function saveGame() {
    showSaveLoadScreen('save');
}

// ロード画面表示
function loadGame() {
    showSaveLoadScreen('load');
}

// セーブ/ロード画面表示
function showSaveLoadScreen(mode) {
    document.getElementById('save-load-screen').style.display = 'block';
    document.getElementById('save-load-title').textContent = mode === 'save' ? 'セーブ' : 'ロード';
    
    const slotsContainer = document.getElementById('save-slots');
    slotsContainer.innerHTML = '';
    
    for (let i = 1; i <= 10; i++) {
        const slot = document.createElement('div');
        slot.className = 'save-slot';
        
        const saveData = localStorage.getItem(`save_${i}`);
        if (saveData) {
            const data = JSON.parse(saveData);
            slot.innerHTML = `
                <span>Slot ${i}</span>
                <span>Day ${data.gameState.currentDay}</span>
                <span>${new Date(data.timestamp).toLocaleString()}</span>
            `;
        } else {
            slot.innerHTML = `<span>Slot ${i}</span><span>-Empty-</span>`;
        }
        
        slot.onclick = () => {
            if (mode === 'save') {
                performSave(i);
            } else {
                performLoad(i);
            }
        };
        
        slotsContainer.appendChild(slot);
    }
}

// セーブ実行
function performSave(slot) {
    const saveData = {
        gameState: gameState,
        timestamp: new Date().toISOString()
    };
    localStorage.setItem(`save_${slot}`, JSON.stringify(saveData));
    showPopup(`スロット${slot}にセーブしました`);
    closeSaveLoad();
}

// ロード実行
function performLoad(slot) {
    const saveData = localStorage.getItem(`save_${slot}`);
    if (saveData) {
        const data = JSON.parse(saveData);
        Object.assign(gameState, data.gameState);
        
        document.getElementById('title-screen').style.display = 'none';
        document.getElementById('save-load-screen').style.display = 'none';
        document.getElementById('game-screen').style.display = 'block';
        
        updateStatusBar();
        updateAffectionDisplay();
        displayDialogue();
        
        showPopup(`スロット${slot}からロードしました`);
    }
}

// セーブ/ロード画面を閉じる
function closeSaveLoad() {
    document.getElementById('save-load-screen').style.display = 'none';
}

// メニューを開く
function openMenu() {
    saveGame();
}

// ギャラリー表示
function showGallery() {
    document.getElementById('gallery-screen').style.display = 'block';
    showGalleryTab('cg');
}

// ギャラリータブ切り替え
function showGalleryTab(tab) {
    const tabs = document.querySelectorAll('.tab-btn');
    tabs.forEach(t => t.classList.remove('active'));
    event.target.classList.add('active');
    
    const content = document.getElementById('gallery-content');
    content.innerHTML = '';
    
    switch(tab) {
        case 'cg':
            content.innerHTML = '<p>CGギャラリー（未実装）</p>';
            break;
        case 'endings':
            content.innerHTML = '<p>エンディング一覧（未実装）</p>';
            break;
        case 'music':
            content.innerHTML = '<p>BGMプレイヤー（未実装）</p>';
            break;
    }
}

// ギャラリーを閉じる
function closeGallery() {
    document.getElementById('gallery-screen').style.display = 'none';
}

// 設定画面表示
function showSettings() {
    document.getElementById('settings-screen').style.display = 'block';
}

// 設定画面を閉じる
function closeSettings() {
    document.getElementById('settings-screen').style.display = 'none';
    saveSettings();
}

// 設定更新
function updateTextSpeed() {
    const value = document.getElementById('text-speed').value;
    document.getElementById('text-speed-value').textContent = value;
    gameState.settings.textSpeed = parseInt(value);
}

function updateAutoSpeed() {
    const value = document.getElementById('auto-speed').value;
    document.getElementById('auto-speed-value').textContent = value;
    gameState.settings.autoSpeed = parseInt(value);
}

function updateBGMVolume() {
    const value = document.getElementById('bgm-volume').value;
    document.getElementById('bgm-volume-value').textContent = value;
    gameState.settings.bgmVolume = parseInt(value);
}

function updateSEVolume() {
    const value = document.getElementById('se-volume').value;
    document.getElementById('se-volume-value').textContent = value;
    gameState.settings.seVolume = parseInt(value);
}

// 設定保存
function saveSettings() {
    localStorage.setItem('settings', JSON.stringify(gameState.settings));
}

// 設定読み込み
function loadSettings() {
    const settings = localStorage.getItem('settings');
    if (settings) {
        gameState.settings = JSON.parse(settings);
        
        document.getElementById('text-speed').value = gameState.settings.textSpeed;
        document.getElementById('auto-speed').value = gameState.settings.autoSpeed;
        document.getElementById('bgm-volume').value = gameState.settings.bgmVolume;
        document.getElementById('se-volume').value = gameState.settings.seVolume;
        
        updateTextSpeed();
        updateAutoSpeed();
        updateBGMVolume();
        updateSEVolume();
    }
}

// ポップアップ表示
function showPopup(message) {
    const popup = document.getElementById('popup');
    document.getElementById('popup-message').textContent = message;
    popup.style.display = 'flex';
    
    setTimeout(() => {
        popup.style.display = 'none';
    }, 2000);
}

// ポップアップを閉じる
function closePopup() {
    document.getElementById('popup').style.display = 'none';
}

// 好感度表示初期化
function initializeAffectionDisplay() {
    updateAffectionDisplay();
}
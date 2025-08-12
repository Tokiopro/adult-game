#!/bin/bash

echo "==========================================="
echo "Unity 3D恋愛シミュレーターセットアップ"
echo "==========================================="
echo ""

# プロジェクトディレクトリ
PROJECT_DIR="$(pwd)/UnityProject"

# Unity Hubの確認
if [ ! -f "unity-hub.AppImage" ]; then
    echo "❌ Unity Hubが見つかりません"
    echo "先にsetup_unity.shを実行してください"
    exit 1
fi

echo "📁 プロジェクトフォルダ構造を作成中..."

# Unity プロジェクトフォルダ構造作成
mkdir -p "$PROJECT_DIR/Assets/Scripts/Core"
mkdir -p "$PROJECT_DIR/Assets/Scripts/UI"
mkdir -p "$PROJECT_DIR/Assets/Scripts/Characters"
mkdir -p "$PROJECT_DIR/Assets/Scripts/Dialogue"
mkdir -p "$PROJECT_DIR/Assets/Scripts/Save"
mkdir -p "$PROJECT_DIR/Assets/Scripts/Camera"
mkdir -p "$PROJECT_DIR/Assets/Scripts/Audio"
mkdir -p "$PROJECT_DIR/Assets/Scripts/Effects"

mkdir -p "$PROJECT_DIR/Assets/Prefabs/Characters"
mkdir -p "$PROJECT_DIR/Assets/Prefabs/UI"
mkdir -p "$PROJECT_DIR/Assets/Prefabs/Environment"
mkdir -p "$PROJECT_DIR/Assets/Prefabs/Effects"

mkdir -p "$PROJECT_DIR/Assets/Models/Characters"
mkdir -p "$PROJECT_DIR/Assets/Models/Environment"
mkdir -p "$PROJECT_DIR/Assets/Models/Props"

mkdir -p "$PROJECT_DIR/Assets/Textures/Characters"
mkdir -p "$PROJECT_DIR/Assets/Textures/Environment"
mkdir -p "$PROJECT_DIR/Assets/Textures/UI"

mkdir -p "$PROJECT_DIR/Assets/Materials/Characters"
mkdir -p "$PROJECT_DIR/Assets/Materials/Environment"
mkdir -p "$PROJECT_DIR/Assets/Materials/Effects"

mkdir -p "$PROJECT_DIR/Assets/Animations/Characters"
mkdir -p "$PROJECT_DIR/Assets/Animations/UI"
mkdir -p "$PROJECT_DIR/Assets/Animations/Camera"

mkdir -p "$PROJECT_DIR/Assets/Audio/BGM"
mkdir -p "$PROJECT_DIR/Assets/Audio/SE"
mkdir -p "$PROJECT_DIR/Assets/Audio/Voice"

mkdir -p "$PROJECT_DIR/Assets/Resources/DialogueData"
mkdir -p "$PROJECT_DIR/Assets/Resources/CharacterData"
mkdir -p "$PROJECT_DIR/Assets/Resources/SaveData"

mkdir -p "$PROJECT_DIR/Assets/Scenes"
mkdir -p "$PROJECT_DIR/Assets/Shaders"

echo "✅ フォルダ構造作成完了"

# 既存のC#スクリプトを適切な場所に移動
echo "📝 既存スクリプトを配置中..."

if [ -f "$PROJECT_DIR/DialogueSystem.cs" ]; then
    mv "$PROJECT_DIR/DialogueSystem.cs" "$PROJECT_DIR/Assets/Scripts/Dialogue/"
fi

if [ -f "$PROJECT_DIR/CharacterManager.cs" ]; then
    mv "$PROJECT_DIR/CharacterManager.cs" "$PROJECT_DIR/Assets/Scripts/Characters/"
fi

if [ -f "$PROJECT_DIR/SaveLoadSystem.cs" ]; then
    mv "$PROJECT_DIR/SaveLoadSystem.cs" "$PROJECT_DIR/Assets/Scripts/Save/"
fi

if [ -f "$PROJECT_DIR/GameEventSystem.cs" ]; then
    mv "$PROJECT_DIR/GameEventSystem.cs" "$PROJECT_DIR/Assets/Scripts/Core/"
fi

if [ -f "$PROJECT_DIR/AffectionPopup.cs" ]; then
    mv "$PROJECT_DIR/AffectionPopup.cs" "$PROJECT_DIR/Assets/Scripts/UI/"
fi

echo "✅ スクリプト配置完了"

# Unity Hubを起動
echo ""
echo "🚀 Unity Hubを起動します..."
echo ""
./unity-hub.AppImage --no-sandbox &

echo "==========================================="
echo "Unity Hubでの手順："
echo "==========================================="
echo ""
echo "1. Unity Hubにサインイン"
echo ""
echo "2. [Installs]タブで Unity 2022.3 LTS をインストール"
echo "   - Modules追加："
echo "     ✓ Windows Build Support (IL2CPP)"
echo "     ✓ Visual Studio (推奨)"
echo ""
echo "3. [Projects]タブで [New project] をクリック"
echo "   - Template: 3D (URP)"
echo "   - Project name: SchoolLoveSimulator3D"
echo "   - Location: $PROJECT_DIR"
echo ""
echo "4. プロジェクトが開いたら："
echo "   a) Window > Package Manager を開く"
echo "   b) UnityPackages.txt のパッケージをインストール"
echo "   c) setup_game_components.sh を実行"
echo ""
echo "==========================================="
echo ""
echo "準備ができたら次のコマンドを実行："
echo "  ./setup_game_components.sh"
echo ""
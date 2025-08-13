#!/bin/bash

echo "================================================"
echo "Unity Windows EXE ビルド完全ガイド"
echo "================================================"
echo ""

# カラー定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# プロジェクトパス
PROJECT_PATH="$(pwd)/UnityProject"
BUILD_PATH="$(pwd)/Builds/Windows"

echo -e "${BLUE}■ Unity EXEビルド手順${NC}"
echo ""

echo -e "${YELLOW}【準備】Unity Editorを起動${NC}"
echo "1. Unity Hubを開く"
echo "2. SchoolLoveSimulator3D プロジェクトを開く"
echo ""

echo -e "${YELLOW}【ステップ1】ビルド設定を開く${NC}"
echo "File → Build Settings..."
echo ""

echo -e "${YELLOW}【ステップ2】プラットフォーム設定${NC}"
echo "1. Platform リストから 'PC, Mac & Linux Standalone' を選択"
echo "2. Target Platform: Windows"
echo "3. Architecture: x86_64"
echo "4. 'Switch Platform' ボタンをクリック（未選択の場合）"
echo ""

echo -e "${YELLOW}【ステップ3】Player Settings設定${NC}"
echo "'Player Settings...' ボタンをクリックして以下を設定:"
echo ""
echo -e "${GREEN}▼ Company Settings${NC}"
echo "  Company Name: YourStudio"
echo "  Product Name: SchoolLoveSimulator"
echo "  Version: 1.0.0"
echo ""
echo -e "${GREEN}▼ Icon設定${NC}"
echo "  Default Icon: ゲームアイコン画像を設定"
echo "  （推奨: 1024x1024px PNG）"
echo ""
echo -e "${GREEN}▼ Resolution and Presentation${NC}"
echo "  Fullscreen Mode: Fullscreen Window"
echo "  Default Screen Width: 1920"
echo "  Default Screen Height: 1080"
echo "  Supported Aspect Ratios: 16:9にチェック"
echo "  Allow Fullscreen Switch: ON"
echo ""
echo -e "${GREEN}▼ Splash Image${NC}"
echo "  Show Splash Screen: ON/OFF（お好みで）"
echo "  Splash Style: Dark on Light"
echo "  Animation: Static"
echo ""
echo -e "${GREEN}▼ Other Settings${NC}"
echo "  Rendering:"
echo "    Color Space: Linear"
echo "    Auto Graphics API: ON"
echo "  Configuration:"
echo "    Scripting Backend: IL2CPP"
echo "    Api Compatibility Level: .NET Standard 2.1"
echo "    C++ Compiler Configuration: Master"
echo ""

echo -e "${YELLOW}【ステップ4】Quality Settings${NC}"
echo "Edit → Project Settings → Quality"
echo "  Ultra設定を選択（最高品質）"
echo "  Anti Aliasing: 4x Multi Sampling"
echo "  Shadows: Soft Shadows"
echo "  Shadow Resolution: Very High Resolution"
echo "  Texture Quality: Full Res"
echo ""

echo -e "${YELLOW}【ステップ5】ビルド実行${NC}"
echo "1. Build Settings に戻る"
echo "2. Scenes In Build にシーンを追加:"
echo "   - MainMenu"
echo "   - GameScene"
echo "   - LoadingScene"
echo "3. 'Build' ボタンをクリック"
echo "4. 保存先: $BUILD_PATH"
echo "5. ファイル名: SchoolLoveSimulator.exe"
echo ""

echo "================================================"
echo -e "${BLUE}■ コマンドラインビルド（上級者向け）${NC}"
echo "================================================"
echo ""

# Unity CLIビルドスクリプト作成
cat > build_unity_cli.sh << 'EOF'
#!/bin/bash

# Unity実行ファイルのパス（環境に応じて変更）
UNITY_PATH="/path/to/Unity/Editor/Unity"
PROJECT_PATH="$(pwd)/UnityProject"
BUILD_PATH="$(pwd)/Builds/Windows/SchoolLoveSimulator.exe"

echo "Unity CLIビルドを開始..."

$UNITY_PATH \
  -batchmode \
  -nographics \
  -silent-crashes \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -buildWindowsPlayer "$BUILD_PATH" \
  -buildTarget Win64 \
  -logFile build.log

if [ $? -eq 0 ]; then
    echo "ビルド成功！"
    echo "出力先: $BUILD_PATH"
else
    echo "ビルド失敗。build.log を確認してください。"
    cat build.log
fi
EOF

chmod +x build_unity_cli.sh

echo "CLIビルドスクリプトを作成しました: build_unity_cli.sh"
echo ""

echo "================================================"
echo -e "${BLUE}■ ビルド後の設定${NC}"
echo "================================================"
echo ""

echo -e "${YELLOW}【配布用フォルダ構成】${NC}"
echo "SchoolLoveSimulator/"
echo "├── SchoolLoveSimulator.exe    # メイン実行ファイル"
echo "├── SchoolLoveSimulator_Data/   # ゲームデータ"
echo "├── UnityPlayer.dll             # Unity実行時ライブラリ"
echo "├── UnityCrashHandler64.exe    # クラッシュハンドラ"
echo "└── MonoBleedingEdge/           # Monoランタイム"
echo ""

echo -e "${YELLOW}【インストーラー作成（オプション）】${NC}"
echo "1. Inno Setup をダウンロード"
echo "   https://jrsoftware.org/isdl.php"
echo ""
echo "2. インストーラースクリプト作成"
echo ""

# Inno Setupスクリプト作成
cat > installer_setup.iss << 'EOF'
[Setup]
AppName=School Love Simulator
AppVersion=1.0.0
AppPublisher=YourStudio
AppPublisherURL=https://yourstudio.com
DefaultDirName={pf}\SchoolLoveSimulator
DefaultGroupName=School Love Simulator
OutputDir=Installer
OutputBaseFilename=SchoolLoveSimulator_Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "Builds\Windows\SchoolLoveSimulator.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Builds\Windows\SchoolLoveSimulator_Data\*"; DestDir: "{app}\SchoolLoveSimulator_Data"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Builds\Windows\UnityPlayer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Builds\Windows\UnityCrashHandler64.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Builds\Windows\MonoBleedingEdge\*"; DestDir: "{app}\MonoBleedingEdge"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\School Love Simulator"; Filename: "{app}\SchoolLoveSimulator.exe"
Name: "{group}\{cm:UninstallProgram,School Love Simulator}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\School Love Simulator"; Filename: "{app}\SchoolLoveSimulator.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\SchoolLoveSimulator.exe"; Description: "{cm:LaunchProgram,School Love Simulator}"; Flags: nowait postinstall skipifsilent
EOF

echo "インストーラースクリプトを作成しました: installer_setup.iss"
echo ""

echo "================================================"
echo -e "${BLUE}■ 最適化のヒント${NC}"
echo "================================================"
echo ""
echo "1. テクスチャ圧縮:"
echo "   - DXT5/BC7 for Windows"
echo "   - Texture Max Size を適切に設定"
echo ""
echo "2. オーディオ圧縮:"
echo "   - BGM: Vorbis"
echo "   - SE: ADPCM"
echo ""
echo "3. ビルドサイズ削減:"
echo "   - Strip Engine Code: ON"
echo "   - Managed Stripping Level: High"
echo ""
echo "4. パフォーマンス:"
echo "   - Static Batching: ON"
echo "   - Dynamic Batching: ON"
echo "   - GPU Instancing: ON"
echo ""

echo "================================================"
echo -e "${GREEN}■ トラブルシューティング${NC}"
echo "================================================"
echo ""
echo "Q: ビルドが失敗する"
echo "A: Console でエラーを確認。主な原因:"
echo "   - Missing scripts"
echo "   - Shader compilation errors"
echo "   - Platform module未インストール"
echo ""
echo "Q: 実行時にクラッシュする"
echo "A: 以下を確認:"
echo "   - DirectX 11以上がインストール済み"
echo "   - Visual C++ Redistributable"
echo "   - グラフィックドライバ最新"
echo ""
echo "Q: ファイルサイズが大きい"
echo "A: 以下で最適化:"
echo "   - Compression Method: LZ4HC"
echo "   - Texture Compression"
echo "   - Audio Compression"
echo ""

echo "================================================"
echo -e "${GREEN}✓ ガイド作成完了！${NC}"
echo "================================================"
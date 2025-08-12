#!/bin/bash

echo "================================================"
echo "Unity Editor 完全インストールガイド"
echo "================================================"
echo ""

# カラー定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Unity Hubの確認
echo -e "${YELLOW}ステップ 1: Unity Hub確認${NC}"
if [ -f "unity-hub.AppImage" ]; then
    echo -e "${GREEN}✓ Unity Hub が見つかりました${NC}"
else
    echo -e "${RED}✗ Unity Hub が見つかりません${NC}"
    echo "setup_unity.sh を先に実行してください"
    exit 1
fi

echo ""
echo -e "${YELLOW}ステップ 2: Unity Hub起動${NC}"
echo "Unity Hubを起動します..."
./unity-hub.AppImage --no-sandbox &
HUB_PID=$!

sleep 3

echo ""
echo "================================================"
echo -e "${GREEN}Unity Editor インストール手順${NC}"
echo "================================================"
echo ""
echo "Unity Hubが起動しました。以下の手順に従ってください："
echo ""
echo -e "${YELLOW}1. サインイン${NC}"
echo "   - Unity IDでサインイン"
echo "   - Personal ライセンスを選択（無料）"
echo ""
echo -e "${YELLOW}2. Unity Editor インストール${NC}"
echo "   a) [Installs] タブをクリック"
echo "   b) [Install Editor] ボタンをクリック"
echo "   c) Unity 2022.3.XX LTS を選択（最新のLTS版）"
echo ""
echo -e "${YELLOW}3. モジュール選択${NC}"
echo "   必須モジュール："
echo "   ☑ Windows Build Support (IL2CPP)"
echo "   ☑ Linux Build Support (Mono)"
echo "   ☑ Documentation"
echo ""
echo "   推奨モジュール："
echo "   ☑ Visual Studio または Visual Studio Code"
echo "   ☑ Android Build Support（モバイル版を作る場合）"
echo ""
echo -e "${YELLOW}4. インストール実行${NC}"
echo "   - [Install] ボタンをクリック"
echo "   - ダウンロード完了まで待機（約30分-1時間）"
echo ""
echo "================================================"
echo -e "${GREEN}インストール完了後の設定${NC}"
echo "================================================"
echo ""
echo -e "${YELLOW}5. 新規プロジェクト作成${NC}"
echo "   a) [Projects] タブをクリック"
echo "   b) [New project] ボタンをクリック"
echo "   c) テンプレート選択："
echo "      - 3D (URP) を選択"
echo "   d) プロジェクト設定："
echo "      - Project name: SchoolLoveSimulator3D"
echo "      - Location: $(pwd)/UnityProject"
echo "   e) [Create project] をクリック"
echo ""
echo -e "${YELLOW}6. プロジェクトが開いたら${NC}"
echo "   以下のコマンドを実行："
echo -e "${GREEN}   ./setup_unity_packages.sh${NC}"
echo ""
echo "================================================"
echo ""

# Unity Hub のバージョン確認コマンド作成
cat > check_unity_version.sh << 'EOF'
#!/bin/bash
echo "Unity インストール状況確認"
echo "========================"

# Unity Editorのパスを検索
UNITY_PATH=$(find ~ -name "Unity" -type f -executable 2>/dev/null | grep -E "2022\.[0-9]+\.[0-9]+")

if [ -n "$UNITY_PATH" ]; then
    echo "✓ Unity Editor が見つかりました:"
    echo "  パス: $UNITY_PATH"
    $UNITY_PATH -version 2>/dev/null || echo "  バージョン確認には Unity Hub から起動が必要です"
else
    echo "✗ Unity Editor が見つかりません"
    echo "  Unity Hub から 2022.3 LTS をインストールしてください"
fi
EOF

chmod +x check_unity_version.sh

echo -e "${GREEN}ヘルパースクリプトを作成しました:${NC}"
echo "  ./check_unity_version.sh - Unity インストール確認"
echo ""
echo -e "${YELLOW}Unity Hub PID: $HUB_PID${NC}"
echo "Unity Hub を閉じる場合: kill $HUB_PID"
echo ""
echo "================================================"
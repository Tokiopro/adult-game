#!/bin/bash

echo "Unity プロジェクトのセットアップ"
echo "================================"

# Unity Hubの起動
echo "Unity Hubを起動します..."
./unity-hub.AppImage --no-sandbox &

echo ""
echo "Unity Hubが起動しました。以下の手順で進めてください:"
echo ""
echo "1. Unity Hubにサインイン"
echo "2. 'Installs'タブで Unity 2022.3 LTS をインストール"
echo "3. Windows Build Support を追加"
echo "4. 'Projects'タブで 'New Project' をクリック"
echo "5. プロジェクト名: 'SchoolLoveSimulator'"
echo "6. テンプレート: '3D' を選択"
echo "7. プロジェクトの保存先: $(pwd)/UnityProject"
echo ""
echo "インストール完了後、setup_game_scripts.sh を実行してください"
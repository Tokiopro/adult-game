#!/bin/bash

# ================================================
# FANZA/DMM販売用自動ビルドシステム
# 最新コードを即座にEXEファイルに反映
# ================================================

# カラー定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# 設定
PROJECT_PATH="$(pwd)/UnityProject"
BUILD_PATH="$(pwd)/Builds/Release"
VERSION_FILE="$(pwd)/version.txt"
CONFIG_PATH="$(pwd)/GameConfig"

# Unity パス（環境に応じて変更）
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    UNITY_PATH="/opt/Unity/Editor/Unity"
elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
    UNITY_PATH="C:/Program Files/Unity/Hub/Editor/2022.3.10f1/Editor/Unity.exe"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.10f1/Unity.app/Contents/MacOS/Unity"
fi

echo -e "${BLUE}================================================${NC}"
echo -e "${BLUE}  FANZA/DMM 販売用ビルドシステム${NC}"
echo -e "${BLUE}================================================${NC}"
echo ""

# バージョン管理
get_version() {
    if [ -f "$VERSION_FILE" ]; then
        cat "$VERSION_FILE"
    else
        echo "1.0.0"
    fi
}

increment_version() {
    current_version=$(get_version)
    IFS='.' read -ra VERSION_PARTS <<< "$current_version"
    
    # パッチバージョンをインクリメント
    VERSION_PARTS[2]=$((VERSION_PARTS[2] + 1))
    
    new_version="${VERSION_PARTS[0]}.${VERSION_PARTS[1]}.${VERSION_PARTS[2]}"
    echo "$new_version" > "$VERSION_FILE"
    echo "$new_version"
}

# ステップ1: プロジェクト準備
prepare_project() {
    echo -e "${YELLOW}[1/7] プロジェクト準備中...${NC}"
    
    # ビルドディレクトリ作成
    mkdir -p "$BUILD_PATH"
    mkdir -p "$CONFIG_PATH"
    
    # 設定ファイルを外部化
    create_config_files
    
    echo -e "${GREEN}✓ プロジェクト準備完了${NC}"
}

# ステップ2: 設定ファイル作成
create_config_files() {
    echo -e "${YELLOW}[2/7] 設定ファイル生成中...${NC}"
    
    # ゲーム設定JSON
    cat > "$CONFIG_PATH/GameConfig.json" << EOF
{
    "version": "$(get_version)",
    "buildDate": "$(date +%Y-%m-%d)",
    "gameSettings": {
        "resolutions": ["1920x1080", "1280x720", "2560x1440"],
        "defaultResolution": "1920x1080",
        "fullscreen": false,
        "vsync": true,
        "targetFrameRate": 60
    },
    "contentSettings": {
        "enableAdultContent": true,
        "mosaicEnabled": true,
        "mosaicSize": 16,
        "ageVerificationRequired": true
    },
    "audioSettings": {
        "masterVolume": 0.8,
        "bgmVolume": 0.7,
        "seVolume": 0.8,
        "voiceVolume": 0.9
    },
    "debugSettings": {
        "enableDebugMode": false,
        "showFPS": false,
        "enableCheats": false
    }
}
EOF

    # コンテンツ設定
    cat > "$CONFIG_PATH/ContentConfig.json" << EOF
{
    "heroines": [
        {
            "id": "ayame",
            "name": "綾女",
            "age": 18,
            "enabled": true
        },
        {
            "id": "misaki",
            "name": "美咲",
            "age": 19,
            "enabled": true
        },
        {
            "id": "yukino",
            "name": "雪乃",
            "age": 20,
            "enabled": true
        }
    ],
    "stages": {
        "maxIntimacyLevel": 500,
        "level1Threshold": 100,
        "level2Threshold": 250,
        "level3Threshold": 500
    },
    "unlockedContent": {
        "positions": ["missionary"],
        "actions": ["hold_hands", "hug"],
        "locations": ["school", "cafe", "park"]
    }
}
EOF

    echo -e "${GREEN}✓ 設定ファイル生成完了${NC}"
}

# ステップ3: Unity ビルド
build_unity() {
    echo -e "${YELLOW}[3/7] Unity ビルド実行中...${NC}"
    
    # ビルドスクリプト作成
    cat > "$PROJECT_PATH/Assets/Editor/AutoBuilder.cs" << 'EOF'
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public class AutoBuilder
{
    [MenuItem("Build/Build for FANZA-DMM")]
    public static void BuildForRelease()
    {
        string buildPath = "../Builds/Release/SchoolLoveSimulator.exe";
        
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.CompressWithLz4HC
        };
        
        // Player Settings
        PlayerSettings.companyName = "YourStudio";
        PlayerSettings.productName = "SchoolLoveSimulator";
        PlayerSettings.bundleVersion = GetVersion();
        PlayerSettings.defaultIsNativeResolution = false;
        PlayerSettings.runInBackground = true;
        
        // 最適化設定
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_Standard_2_0);
        PlayerSettings.stripEngineCode = true;
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {report.summary.totalSize} bytes");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError("Build failed");
            EditorApplication.Exit(1);
        }
    }
    
    static string[] GetScenes()
    {
        return new string[]
        {
            "Assets/Scenes/Startup.unity",
            "Assets/Scenes/MainGame.unity",
            "Assets/Scenes/DateScene.unity",
            "Assets/Scenes/IntimateScene.unity"
        };
    }
    
    static string GetVersion()
    {
        string versionFile = "../version.txt";
        if (File.Exists(versionFile))
            return File.ReadAllText(versionFile).Trim();
        return "1.0.0";
    }
}
EOF

    # Unity でビルド実行
    echo "Unity ビルド開始..."
    "$UNITY_PATH" \
        -batchmode \
        -nographics \
        -quit \
        -projectPath "$PROJECT_PATH" \
        -executeMethod AutoBuilder.BuildForRelease \
        -logFile build.log \
        2>&1 | while read line; do
            echo -e "  ${line}"
        done
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Unity ビルド成功${NC}"
    else
        echo -e "${RED}✗ Unity ビルド失敗${NC}"
        cat build.log
        exit 1
    fi
}

# ステップ4: モザイク処理確認
verify_mosaic() {
    echo -e "${YELLOW}[4/7] モザイク処理確認中...${NC}"
    
    # ビルドされたファイルにモザイク設定が含まれているか確認
    if grep -q "mosaicEnabled" "$BUILD_PATH/SchoolLoveSimulator_Data/StreamingAssets/GameConfig.json" 2>/dev/null; then
        echo -e "${GREEN}✓ モザイク処理設定確認${NC}"
    else
        echo -e "${YELLOW}⚠ モザイク設定をStreamingAssetsにコピー${NC}"
        mkdir -p "$BUILD_PATH/SchoolLoveSimulator_Data/StreamingAssets"
        cp -r "$CONFIG_PATH"/* "$BUILD_PATH/SchoolLoveSimulator_Data/StreamingAssets/"
    fi
}

# ステップ5: DLLパッキング
pack_dlls() {
    echo -e "${YELLOW}[5/7] DLL最適化中...${NC}"
    
    # 不要なDLLを削除
    cd "$BUILD_PATH"
    
    # デバッグシンボル削除
    find . -name "*.pdb" -delete
    
    # ログファイル削除
    find . -name "*.log" -delete
    
    echo -e "${GREEN}✓ DLL最適化完了${NC}"
}

# ステップ6: パッケージング
create_package() {
    echo -e "${YELLOW}[6/7] 配布パッケージ作成中...${NC}"
    
    VERSION=$(get_version)
    PACKAGE_NAME="SchoolLoveSimulator_v${VERSION}_FANZA"
    PACKAGE_PATH="$(pwd)/Packages/$PACKAGE_NAME"
    
    mkdir -p "$PACKAGE_PATH/Game"
    
    # ゲームファイルをコピー
    cp -r "$BUILD_PATH"/* "$PACKAGE_PATH/Game/"
    
    # マニュアル作成
    create_manual "$PACKAGE_PATH"
    
    # システム要件作成
    create_requirements "$PACKAGE_PATH"
    
    # お読みください作成
    create_readme "$PACKAGE_PATH"
    
    # ZIP圧縮
    cd "$(pwd)/Packages"
    if command -v 7z &> /dev/null; then
        7z a -tzip "${PACKAGE_NAME}.zip" "$PACKAGE_NAME/*" -mx9
    else
        zip -r9 "${PACKAGE_NAME}.zip" "$PACKAGE_NAME"
    fi
    
    echo -e "${GREEN}✓ パッケージ作成完了: ${PACKAGE_NAME}.zip${NC}"
}

# マニュアル作成
create_manual() {
    local path=$1
    cat > "$path/manual.txt" << EOF
========================================
  恋愛シミュレーター v$(get_version)
  操作マニュアル
========================================

【基本操作】
・移動: WASD キー
・カメラ: マウス
・決定: Space / 左クリック
・メニュー: Tab / ESC
・インタラクション: E キー

【ゲームの流れ】
1. ヒロインを選択
2. 会話やデートで親密度を上げる
3. 親密度レベルに応じてイベント解放
   - レベル1: デート可能
   - レベル2: 告白可能
   - レベル3: 特別な関係

【セーブ/ロード】
・メニュー → セーブ/ロード
・オートセーブ対応

【設定変更】
・GameConfig.json を編集することで
  各種設定を変更できます

【サポート】
メール: support@yourstudio.com
EOF
}

# システム要件作成
create_requirements() {
    local path=$1
    cat > "$path/system_requirements.txt" << EOF
========================================
  動作環境
========================================

【必須環境】
OS: Windows 10/11 (64bit)
CPU: Intel Core i5 以上
メモリ: 8GB以上
GPU: DirectX 11 対応 (VRAM 2GB以上)
ストレージ: 2GB以上の空き容量

【推奨環境】
OS: Windows 11 (64bit)
CPU: Intel Core i7 以上
メモリ: 16GB以上
GPU: GeForce GTX 1060 以上
ストレージ: 4GB以上の空き容量

【必須ランタイム】
・DirectX 11
・Visual C++ 2019 再頒布可能パッケージ
・.NET Framework 4.8

【注意事項】
・18歳未満の方はプレイできません
・インターネット接続は不要です
EOF
}

# お読みください作成
create_readme() {
    local path=$1
    cat > "$path/お読みください.txt" << EOF
========================================
  重要なお知らせ
========================================

本作品は成人向けコンテンツを含みます。
18歳未満の方のプレイは固くお断りします。

【初回起動時】
年齢確認画面が表示されます。
18歳以上であることを確認してください。

【インストール】
1. ZIPファイルを解凍
2. 任意の場所にフォルダを配置
3. Game/SchoolLoveSimulator.exe を実行

【アンインストール】
フォルダごと削除してください。
レジストリは使用していません。

【著作権】
本作品の無断転載・配布は禁止です。
購入者本人のみがプレイ可能です。

制作: YourStudio
販売: FANZA/DMM
EOF
}

# ステップ7: インストーラー作成
create_installer() {
    echo -e "${YELLOW}[7/7] インストーラー作成中...${NC}"
    
    # Inno Setup スクリプト生成
    create_installer_script
    
    # Inno Setup 実行（Windows環境の場合）
    if [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
        if command -v iscc &> /dev/null; then
            iscc installer_config.iss
            echo -e "${GREEN}✓ インストーラー作成完了${NC}"
        else
            echo -e "${YELLOW}⚠ Inno Setup がインストールされていません${NC}"
        fi
    else
        echo -e "${YELLOW}⚠ インストーラーは Windows 環境で作成してください${NC}"
    fi
}

# インストーラースクリプト生成
create_installer_script() {
    cat > installer_config.iss << EOF
[Setup]
AppName=恋愛シミュレーター
AppVersion=$(get_version)
AppPublisher=YourStudio
AppPublisherURL=https://yourstudio.com
DefaultDirName={pf}\\SchoolLoveSimulator
DefaultGroupName=恋愛シミュレーター
OutputDir=Installers
OutputBaseFilename=SchoolLoveSimulator_Setup_v$(get_version)
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\\Japanese.isl"

[Files]
Source: "Builds\\Release\\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\\恋愛シミュレーター"; Filename: "{app}\\SchoolLoveSimulator.exe"
Name: "{group}\\アンインストール"; Filename: "{uninstallexe}"
Name: "{commondesktop}\\恋愛シミュレーター"; Filename: "{app}\\SchoolLoveSimulator.exe"

[Run]
Filename: "{app}\\SchoolLoveSimulator.exe"; Description: "ゲームを起動"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  // 年齢確認
  if MsgBox('このゲームは18歳以上を対象としています。' + #13#10 + 
            'あなたは18歳以上ですか？', mbConfirmation, MB_YESNO) = IDYES then
  begin
    Result := True;
  end
  else
  begin
    MsgBox('18歳未満の方はインストールできません。', mbError, MB_OK);
    Result := False;
  end;
end;
EOF
}

# バージョンアップ
update_version() {
    new_version=$(increment_version)
    echo -e "${BLUE}バージョンを ${new_version} に更新しました${NC}"
}

# メイン処理
main() {
    echo -e "${BLUE}ビルドを開始します...${NC}"
    echo ""
    
    # バージョン更新
    update_version
    
    # 各ステップ実行
    prepare_project
    build_unity
    verify_mosaic
    pack_dlls
    create_package
    create_installer
    
    echo ""
    echo -e "${GREEN}================================================${NC}"
    echo -e "${GREEN}  ✓ ビルド完了！${NC}"
    echo -e "${GREEN}================================================${NC}"
    echo ""
    echo -e "出力先:"
    echo -e "  パッケージ: $(pwd)/Packages/"
    echo -e "  インストーラー: $(pwd)/Installers/"
    echo ""
    echo -e "${BLUE}FANZA/DMM への提出準備が整いました！${NC}"
}

# 実行
main
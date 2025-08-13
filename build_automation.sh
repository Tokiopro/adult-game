#!/bin/bash

echo "================================================"
echo "Unity ビルド自動化 & デプロイシステム"
echo "================================================"
echo ""

# カラー定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
NC='\033[0m'

# プロジェクト設定
PROJECT_NAME="SchoolLoveSimulator"
PROJECT_PATH="$(pwd)/UnityProject"
BUILD_PATH="$(pwd)/Builds"
RELEASE_PATH="$(pwd)/Releases"
VERSION="1.0.0"
BUILD_NUMBER=$(date +%Y%m%d%H%M)

# Unity設定（環境に応じて変更）
UNITY_PATH="/path/to/Unity/Editor/Unity"

echo -e "${BLUE}■ ビルド自動化スクリプト${NC}"
echo ""

# ディレクトリ作成
mkdir -p "$BUILD_PATH/Windows"
mkdir -p "$BUILD_PATH/Mac"
mkdir -p "$BUILD_PATH/Linux"
mkdir -p "$RELEASE_PATH"

# Unity Editor Buildスクリプト作成
cat > UnityProject/Assets/Editor/BuildScript.cs << 'EOF'
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System;

public class BuildScript
{
    // ビルド設定
    private static string[] scenes = new string[]
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/GameScene.unity",
        "Assets/Scenes/LoadingScene.unity"
    };
    
    // Windows ビルド
    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Windows/SchoolLoveSimulator.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        LogBuildResult(report);
    }
    
    // Mac ビルド
    [MenuItem("Build/Build Mac")]
    public static void BuildMac()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Mac/SchoolLoveSimulator.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        LogBuildResult(report);
    }
    
    // Linux ビルド
    [MenuItem("Build/Build Linux")]
    public static void BuildLinux()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Linux/SchoolLoveSimulator",
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.None
        };
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        LogBuildResult(report);
    }
    
    // 全プラットフォームビルド
    [MenuItem("Build/Build All Platforms")]
    public static void BuildAll()
    {
        BuildWindows();
        BuildMac();
        BuildLinux();
    }
    
    // コマンドライン用ビルド
    public static void PerformBuild()
    {
        string platform = GetArg("-platform");
        
        switch (platform)
        {
            case "windows":
                BuildWindows();
                break;
            case "mac":
                BuildMac();
                break;
            case "linux":
                BuildLinux();
                break;
            case "all":
                BuildAll();
                break;
            default:
                Debug.LogError($"Unknown platform: {platform}");
                EditorApplication.Exit(1);
                break;
        }
    }
    
    // ビルド結果ログ
    private static void LogBuildResult(BuildReport report)
    {
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {report.summary.outputPath}");
            Debug.Log($"Total time: {report.summary.totalTime}");
            Debug.Log($"Total size: {report.summary.totalSize} bytes");
        }
        else
        {
            Debug.LogError($"Build failed with {report.summary.totalErrors} errors");
            EditorApplication.Exit(1);
        }
    }
    
    // コマンドライン引数取得
    private static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
EOF

echo -e "${GREEN}✓ Unityビルドスクリプト作成完了${NC}"
echo ""

# CI/CD設定（GitHub Actions）
cat > .github/workflows/build.yml << 'EOF'
name: Unity Build

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:

jobs:
  build:
    runs-on: ubuntu-latest
    
    strategy:
      matrix:
        platform: [windows, mac, linux]
    
    steps:
    - uses: actions/checkout@v3
      with:
        lfs: true
    
    - uses: game-ci/unity-builder@v2
      env:
        UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
      with:
        targetPlatform: ${{ matrix.platform }}
        projectPath: UnityProject
        buildName: SchoolLoveSimulator
        buildsPath: Builds
    
    - uses: actions/upload-artifact@v3
      with:
        name: Build-${{ matrix.platform }}
        path: Builds/${{ matrix.platform }}
        retention-days: 14

  release:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v3
    
    - uses: actions/download-artifact@v3
      with:
        path: Builds
    
    - name: Create Release
      id: create_release
      uses: actions/create-release@v1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        tag_name: v${{ github.run_number }}
        release_name: Release ${{ github.run_number }}
        draft: false
        prerelease: false
    
    - name: Upload Release Assets
      uses: actions/upload-release-asset@v1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        upload_url: ${{ steps.create_release.outputs.upload_url }}
        asset_path: ./Builds/Build-windows.zip
        asset_name: SchoolLoveSimulator-Windows.zip
        asset_content_type: application/zip
EOF

echo -e "${GREEN}✓ GitHub Actions CI/CD設定作成完了${NC}"
echo ""

# バッチビルドスクリプト
cat > batch_build.sh << 'EOF'
#!/bin/bash

echo "Starting batch build..."

# Unity Editorパス（環境に応じて変更）
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.10f1/Unity.app/Contents/MacOS/Unity"

# Windowsビルド
echo "Building for Windows..."
$UNITY_PATH \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$(pwd)/UnityProject" \
    -executeMethod BuildScript.PerformBuild \
    -platform windows \
    -logFile "build_windows.log"

# Macビルド
echo "Building for Mac..."
$UNITY_PATH \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$(pwd)/UnityProject" \
    -executeMethod BuildScript.PerformBuild \
    -platform mac \
    -logFile "build_mac.log"

# Linuxビルド
echo "Building for Linux..."
$UNITY_PATH \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$(pwd)/UnityProject" \
    -executeMethod BuildScript.PerformBuild \
    -platform linux \
    -logFile "build_linux.log"

echo "All builds complete!"
EOF

chmod +x batch_build.sh

echo -e "${GREEN}✓ バッチビルドスクリプト作成完了${NC}"
echo ""

# 配布パッケージ作成スクリプト
cat > create_release.sh << 'EOF'
#!/bin/bash

VERSION="1.0.0"
BUILD_DATE=$(date +%Y%m%d)
RELEASE_NAME="SchoolLoveSimulator_v${VERSION}_${BUILD_DATE}"

echo "Creating release packages..."

# Windows版パッケージ
echo "Packaging Windows version..."
cd Builds/Windows
zip -r "../../Releases/${RELEASE_NAME}_Windows.zip" .
cd ../..

# Mac版パッケージ
echo "Packaging Mac version..."
cd Builds/Mac
zip -r "../../Releases/${RELEASE_NAME}_Mac.zip" .
cd ../..

# Linux版パッケージ
echo "Packaging Linux version..."
cd Builds/Linux
tar -czf "../../Releases/${RELEASE_NAME}_Linux.tar.gz" .
cd ../..

echo "Release packages created in Releases/"
ls -la Releases/
EOF

chmod +x create_release.sh

echo -e "${GREEN}✓ リリースパッケージ作成スクリプト作成完了${NC}"
echo ""

# Steam配布用設定
cat > steam_build.vdf << 'EOF'
"AppBuild"
{
    "AppID" "YOUR_APP_ID"
    "Desc" "School Love Simulator"
    "BuildOutput" "./steam_output"
    "ContentRoot" "./Builds/Windows"
    "SetLive" "beta"
    
    "Depots"
    {
        "YOUR_DEPOT_ID"
        {
            "FileMapping"
            {
                "LocalPath" "*"
                "DepotPath" "."
                "recursive" "1"
            }
        }
    }
}
EOF

echo -e "${GREEN}✓ Steam配布設定作成完了${NC}"
echo ""

echo "================================================"
echo -e "${BLUE}■ デプロイ自動化設定${NC}"
echo "================================================"
echo ""

# デプロイスクリプト
cat > deploy.sh << 'EOF'
#!/bin/bash

DEPLOY_TYPE=$1
VERSION=$2

case $DEPLOY_TYPE in
    "steam")
        echo "Deploying to Steam..."
        steamcmd +login YOUR_USERNAME +run_app_build ../steam_build.vdf +quit
        ;;
        
    "itch")
        echo "Deploying to itch.io..."
        butler push Builds/Windows YOUR_USERNAME/school-love-simulator:windows --version $VERSION
        butler push Builds/Mac YOUR_USERNAME/school-love-simulator:mac --version $VERSION
        butler push Builds/Linux YOUR_USERNAME/school-love-simulator:linux --version $VERSION
        ;;
        
    "ftp")
        echo "Deploying to FTP server..."
        lftp -c "
            open ftp://YOUR_FTP_SERVER
            user YOUR_USERNAME YOUR_PASSWORD
            mirror -R Builds/Windows /public_html/downloads/windows
            bye
        "
        ;;
        
    *)
        echo "Usage: ./deploy.sh [steam|itch|ftp] [version]"
        exit 1
        ;;
esac

echo "Deployment complete!"
EOF

chmod +x deploy.sh

echo -e "${GREEN}✓ デプロイスクリプト作成完了${NC}"
echo ""

echo "================================================"
echo -e "${MAGENTA}■ 使用方法${NC}"
echo "================================================"
echo ""
echo "1. ローカルビルド:"
echo "   ./batch_build.sh"
echo ""
echo "2. リリースパッケージ作成:"
echo "   ./create_release.sh"
echo ""
echo "3. デプロイ:"
echo "   ./deploy.sh steam 1.0.0"
echo "   ./deploy.sh itch 1.0.0"
echo ""
echo "4. CI/CDパイプライン:"
echo "   GitHubにpushすると自動ビルド"
echo ""
echo "================================================"
echo -e "${YELLOW}■ 必要な設定${NC}"
echo "================================================"
echo ""
echo "1. Unity License設定:"
echo "   GitHub Secretsに UNITY_LICENSE を設定"
echo ""
echo "2. Steam設定:"
echo "   - App ID取得"
echo "   - Steamworksアカウント設定"
echo ""
echo "3. itch.io設定:"
echo "   - butler CLIインストール"
echo "   - アカウント認証"
echo ""
echo "================================================"
echo -e "${GREEN}✓ ビルド自動化システム構築完了！${NC}"
echo "================================================"
# 学園恋愛シミュレーター

## プロジェクト概要
Unity と Blender を使用した恋愛シミュレーションゲームの開発環境

## 環境構築状況

### 完了した項目
- ✅ Unity Hub インストール済み
- ✅ Blender 4.2 ダウンロード済み
- ✅ プロジェクト構造作成済み
- ✅ 基本ゲームシステム実装済み
  - 会話システム (DialogueSystem.cs)
  - キャラクター管理 (CharacterManager.cs)
  - セーブ/ロード機能 (SaveLoadSystem.cs)
  - イベントシステム (GameEventSystem.cs)

### セットアップ手順

#### 1. Unity セットアップ
```bash
# Unity Hub を起動
./unity-hub.AppImage --no-sandbox

# Unity Editor のインストール
# Unity Hub から Unity 2022.3 LTS をインストール
# Windows Build Support を追加
```

#### 2. Blender セットアップ
```bash
# Blender の展開（まだの場合）
tar -xf blender.tar.xz

# Blender の起動
./blender-4.2.3-linux-x64/blender
```

#### 3. Unity プロジェクト作成
```bash
# セットアップスクリプトを実行
./setup_unity.sh
```

#### 4. Windows ビルド
```bash
# ビルド設定の確認
./build_windows.sh
```

## プロジェクト構造
```
adult-game/
├── UnityProject/          # Unity プロジェクトファイル
│   ├── DialogueSystem.cs  # 会話システム
│   ├── CharacterManager.cs # キャラクター管理
│   ├── SaveLoadSystem.cs  # セーブ/ロード
│   └── GameEventSystem.cs # イベントシステム
├── BlenderAssets/         # 3Dモデル用
├── Documentation/         # ゲーム設計書
│   └── game_design.md
├── Builds/               # ビルド出力
└── unity-hub.AppImage    # Unity Hub

```

## 実装済み機能

### 会話システム
- テキスト表示アニメーション
- 選択肢システム
- キャラクター表情切り替え
- ボイス再生対応

### キャラクター管理
- 好感度システム
- 性格タイプ設定
- イベント解放機能
- 日別交流制限

### セーブ/ロード
- 20スロット対応
- オートセーブ機能
- クイックセーブ/ロード
- スクリーンショット付き

## 次のステップ

1. Unity Editor をインストール（Unity Hub から）
2. 新規プロジェクトを作成
3. スクリプトファイルをインポート
4. UI要素を作成
5. テストプレイ
6. Windows向けにビルド

## 注意事項
- 全年齢対象コンテンツとして開発
- Windows向けEXEファイルとして出力
- 解像度: 1920x1080推奨

## ライセンス
個人使用のみ
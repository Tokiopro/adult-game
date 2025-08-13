using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;
using UnityEngine.Rendering;

namespace SchoolLoveSimulator.Editor
{
    /// <summary>
    /// ビルド最適化ツール
    /// テクスチャ圧縮、アセット最適化、ビルドサイズ削減
    /// </summary>
    public class BuildOptimizer : EditorWindow
    {
        private Vector2 scrollPosition;
        private OptimizationSettings settings;
        private BuildReport lastBuildReport;
        private Dictionary<string, AssetInfo> assetAnalysis;
        
        [System.Serializable]
        public class OptimizationSettings
        {
            public bool optimizeTextures = true;
            public bool optimizeModels = true;
            public bool optimizeAudio = true;
            public bool optimizeShaders = true;
            public bool stripUnusedAssets = true;
            public bool compressAssets = true;
            public bool generateAtlas = true;
            
            public TextureOptimizationSettings textureSettings = new TextureOptimizationSettings();
            public ModelOptimizationSettings modelSettings = new ModelOptimizationSettings();
            public AudioOptimizationSettings audioSettings = new AudioOptimizationSettings();
            public BuildOptimizationSettings buildSettings = new BuildOptimizationSettings();
        }
        
        [System.Serializable]
        public class TextureOptimizationSettings
        {
            public int maxTextureSize = 2048;
            public TextureImporterFormat androidFormat = TextureImporterFormat.ETC2_RGBA8;
            public TextureImporterFormat iosFormat = TextureImporterFormat.PVRTC_RGBA4;
            public TextureImporterFormat windowsFormat = TextureImporterFormat.DXT5;
            public bool generateMipmaps = true;
            public bool crunchCompression = true;
            public int crunchQuality = 50;
        }
        
        [System.Serializable]
        public class ModelOptimizationSettings
        {
            public ModelImporterMeshCompression meshCompression = ModelImporterMeshCompression.High;
            public bool optimizeMesh = true;
            public bool generateLightmapUV = false;
            public float animationCompressionError = 0.5f;
            public bool importBlendShapes = true;
            public bool weldVertices = true;
        }
        
        [System.Serializable]
        public class AudioOptimizationSettings
        {
            public AudioCompressionFormat compressionFormat = AudioCompressionFormat.Vorbis;
            public float quality = 0.7f;
            public AudioClipLoadType loadType = AudioClipLoadType.CompressedInMemory;
            public bool forceToMono = false;
        }
        
        [System.Serializable]
        public class BuildOptimizationSettings
        {
            public StrippingLevel strippingLevel = StrippingLevel.StripAssemblies;
            public bool stripEngineCode = true;
            public Il2CppCompilerConfiguration compilerConfiguration = Il2CppCompilerConfiguration.Master;
            public bool enableCodeOptimization = true;
        }
        
        public class AssetInfo
        {
            public string path;
            public string type;
            public long size;
            public bool isOptimized;
            public List<string> issues = new List<string>();
        }
        
        [MenuItem("Tools/Build/Build Optimizer")]
        public static void ShowWindow()
        {
            GetWindow<BuildOptimizer>("Build Optimizer");
        }
        
        void OnEnable()
        {
            LoadSettings();
            AnalyzeProject();
        }
        
        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawHeader();
            EditorGUILayout.Space();
            
            DrawOptimizationSettings();
            EditorGUILayout.Space();
            
            DrawAnalysisResults();
            EditorGUILayout.Space();
            
            DrawActionButtons();
            
            EditorGUILayout.EndScrollView();
        }
        
        void DrawHeader()
        {
            EditorGUILayout.LabelField("Build Optimization Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool helps optimize your build size and performance by compressing assets and applying optimization settings.",
                MessageType.Info
            );
        }
        
        void DrawOptimizationSettings()
        {
            EditorGUILayout.LabelField("Optimization Settings", EditorStyles.boldLabel);
            
            settings.optimizeTextures = EditorGUILayout.Toggle("Optimize Textures", settings.optimizeTextures);
            if (settings.optimizeTextures)
            {
                EditorGUI.indentLevel++;
                DrawTextureSettings();
                EditorGUI.indentLevel--;
            }
            
            settings.optimizeModels = EditorGUILayout.Toggle("Optimize Models", settings.optimizeModels);
            if (settings.optimizeModels)
            {
                EditorGUI.indentLevel++;
                DrawModelSettings();
                EditorGUI.indentLevel--;
            }
            
            settings.optimizeAudio = EditorGUILayout.Toggle("Optimize Audio", settings.optimizeAudio);
            if (settings.optimizeAudio)
            {
                EditorGUI.indentLevel++;
                DrawAudioSettings();
                EditorGUI.indentLevel--;
            }
            
            settings.optimizeShaders = EditorGUILayout.Toggle("Optimize Shaders", settings.optimizeShaders);
            settings.stripUnusedAssets = EditorGUILayout.Toggle("Strip Unused Assets", settings.stripUnusedAssets);
            settings.compressAssets = EditorGUILayout.Toggle("Compress Assets", settings.compressAssets);
            settings.generateAtlas = EditorGUILayout.Toggle("Generate Texture Atlas", settings.generateAtlas);
        }
        
        void DrawTextureSettings()
        {
            var texSettings = settings.textureSettings;
            
            texSettings.maxTextureSize = EditorGUILayout.IntField("Max Texture Size", texSettings.maxTextureSize);
            texSettings.windowsFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Windows Format", texSettings.windowsFormat);
            texSettings.generateMipmaps = EditorGUILayout.Toggle("Generate Mipmaps", texSettings.generateMipmaps);
            texSettings.crunchCompression = EditorGUILayout.Toggle("Crunch Compression", texSettings.crunchCompression);
            
            if (texSettings.crunchCompression)
            {
                texSettings.crunchQuality = EditorGUILayout.IntSlider("Crunch Quality", texSettings.crunchQuality, 0, 100);
            }
        }
        
        void DrawModelSettings()
        {
            var modelSettings = settings.modelSettings;
            
            modelSettings.meshCompression = (ModelImporterMeshCompression)EditorGUILayout.EnumPopup(
                "Mesh Compression", modelSettings.meshCompression
            );
            modelSettings.optimizeMesh = EditorGUILayout.Toggle("Optimize Mesh", modelSettings.optimizeMesh);
            modelSettings.weldVertices = EditorGUILayout.Toggle("Weld Vertices", modelSettings.weldVertices);
            modelSettings.importBlendShapes = EditorGUILayout.Toggle("Import Blend Shapes", modelSettings.importBlendShapes);
            modelSettings.animationCompressionError = EditorGUILayout.FloatField(
                "Animation Compression Error", modelSettings.animationCompressionError
            );
        }
        
        void DrawAudioSettings()
        {
            var audioSettings = settings.audioSettings;
            
            audioSettings.compressionFormat = (AudioCompressionFormat)EditorGUILayout.EnumPopup(
                "Compression Format", audioSettings.compressionFormat
            );
            audioSettings.quality = EditorGUILayout.Slider("Quality", audioSettings.quality, 0, 1);
            audioSettings.loadType = (AudioClipLoadType)EditorGUILayout.EnumPopup("Load Type", audioSettings.loadType);
            audioSettings.forceToMono = EditorGUILayout.Toggle("Force To Mono", audioSettings.forceToMono);
        }
        
        void DrawAnalysisResults()
        {
            EditorGUILayout.LabelField("Project Analysis", EditorStyles.boldLabel);
            
            if (assetAnalysis != null && assetAnalysis.Count > 0)
            {
                // サイズ統計
                long totalSize = assetAnalysis.Values.Sum(a => a.size);
                int unoptimizedCount = assetAnalysis.Values.Count(a => !a.isOptimized);
                
                EditorGUILayout.LabelField($"Total Assets: {assetAnalysis.Count}");
                EditorGUILayout.LabelField($"Total Size: {FormatSize(totalSize)}");
                EditorGUILayout.LabelField($"Unoptimized Assets: {unoptimizedCount}");
                
                EditorGUILayout.Space();
                
                // 大きいアセットのトップ10
                EditorGUILayout.LabelField("Largest Assets:", EditorStyles.boldLabel);
                var largestAssets = assetAnalysis.Values
                    .OrderByDescending(a => a.size)
                    .Take(10);
                    
                foreach (var asset in largestAssets)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.LabelField(
                        Path.GetFileName(asset.path),
                        GUILayout.Width(200)
                    );
                    EditorGUILayout.LabelField(
                        asset.type,
                        GUILayout.Width(100)
                    );
                    EditorGUILayout.LabelField(
                        FormatSize(asset.size),
                        GUILayout.Width(100)
                    );
                    
                    if (!asset.isOptimized)
                    {
                        EditorGUILayout.LabelField("⚠", GUILayout.Width(20));
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Click 'Analyze Project' to scan assets", MessageType.Info);
            }
        }
        
        void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Analyze Project", GUILayout.Height(30)))
            {
                AnalyzeProject();
            }
            
            if (GUILayout.Button("Optimize All", GUILayout.Height(30)))
            {
                OptimizeAllAssets();
            }
            
            if (GUILayout.Button("Build with Optimization", GUILayout.Height(30)))
            {
                BuildWithOptimization();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Save Settings"))
            {
                SaveSettings();
            }
            
            if (GUILayout.Button("Load Settings"))
            {
                LoadSettings();
            }
            
            if (GUILayout.Button("Reset to Default"))
            {
                settings = new OptimizationSettings();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        void AnalyzeProject()
        {
            assetAnalysis = new Dictionary<string, AssetInfo>();
            
            // テクスチャ分析
            var textures = AssetDatabase.FindAssets("t:Texture2D");
            foreach (var guid in textures)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                
                if (importer != null)
                {
                    var info = new AssetInfo
                    {
                        path = path,
                        type = "Texture",
                        size = GetFileSize(path),
                        isOptimized = IsTextureOptimized(importer)
                    };
                    
                    if (!info.isOptimized)
                    {
                        info.issues.Add($"Size: {importer.maxTextureSize}");
                        if (!importer.crunchedCompression)
                            info.issues.Add("No crunch compression");
                    }
                    
                    assetAnalysis[path] = info;
                }
            }
            
            // モデル分析
            var models = AssetDatabase.FindAssets("t:Model");
            foreach (var guid in models)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                
                if (importer != null)
                {
                    var info = new AssetInfo
                    {
                        path = path,
                        type = "Model",
                        size = GetFileSize(path),
                        isOptimized = IsModelOptimized(importer)
                    };
                    
                    if (!info.isOptimized)
                    {
                        if (importer.meshCompression == ModelImporterMeshCompression.Off)
                            info.issues.Add("No mesh compression");
                        if (!importer.optimizeMesh)
                            info.issues.Add("Mesh not optimized");
                    }
                    
                    assetAnalysis[path] = info;
                }
            }
            
            // オーディオ分析
            var audioClips = AssetDatabase.FindAssets("t:AudioClip");
            foreach (var guid in audioClips)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                
                if (importer != null)
                {
                    var info = new AssetInfo
                    {
                        path = path,
                        type = "Audio",
                        size = GetFileSize(path),
                        isOptimized = IsAudioOptimized(importer)
                    };
                    
                    assetAnalysis[path] = info;
                }
            }
            
            Debug.Log($"Analysis complete. Found {assetAnalysis.Count} assets.");
        }
        
        long GetFileSize(string path)
        {
            var fullPath = Path.Combine(Application.dataPath.Replace("/Assets", ""), path);
            if (File.Exists(fullPath))
            {
                var fileInfo = new FileInfo(fullPath);
                return fileInfo.Length;
            }
            return 0;
        }
        
        bool IsTextureOptimized(TextureImporter importer)
        {
            return importer.maxTextureSize <= settings.textureSettings.maxTextureSize &&
                   importer.crunchedCompression == settings.textureSettings.crunchCompression;
        }
        
        bool IsModelOptimized(ModelImporter importer)
        {
            return importer.meshCompression != ModelImporterMeshCompression.Off &&
                   importer.optimizeMesh;
        }
        
        bool IsAudioOptimized(AudioImporter importer)
        {
            var sampleSettings = importer.defaultSampleSettings;
            return sampleSettings.compressionFormat != AudioCompressionFormat.PCM;
        }
        
        void OptimizeAllAssets()
        {
            int optimizedCount = 0;
            
            try
            {
                AssetDatabase.StartAssetEditing();
                
                if (settings.optimizeTextures)
                {
                    optimizedCount += OptimizeTextures();
                }
                
                if (settings.optimizeModels)
                {
                    optimizedCount += OptimizeModels();
                }
                
                if (settings.optimizeAudio)
                {
                    optimizedCount += OptimizeAudio();
                }
                
                if (settings.optimizeShaders)
                {
                    OptimizeShaders();
                }
                
                if (settings.generateAtlas)
                {
                    GenerateTextureAtlases();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
            
            Debug.Log($"Optimization complete. Optimized {optimizedCount} assets.");
            
            // 再分析
            AnalyzeProject();
        }
        
        int OptimizeTextures()
        {
            int count = 0;
            var textures = AssetDatabase.FindAssets("t:Texture2D");
            
            foreach (var guid in textures)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                
                if (importer != null && !IsTextureOptimized(importer))
                {
                    var platformSettings = importer.GetPlatformTextureSettings("Standalone");
                    platformSettings.overridden = true;
                    platformSettings.maxTextureSize = settings.textureSettings.maxTextureSize;
                    platformSettings.format = settings.textureSettings.windowsFormat;
                    platformSettings.crunchedCompression = settings.textureSettings.crunchCompression;
                    platformSettings.compressionQuality = settings.textureSettings.crunchQuality;
                    
                    importer.SetPlatformTextureSettings(platformSettings);
                    importer.mipmapEnabled = settings.textureSettings.generateMipmaps;
                    
                    importer.SaveAndReimport();
                    count++;
                }
            }
            
            return count;
        }
        
        int OptimizeModels()
        {
            int count = 0;
            var models = AssetDatabase.FindAssets("t:Model");
            
            foreach (var guid in models)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                
                if (importer != null && !IsModelOptimized(importer))
                {
                    importer.meshCompression = settings.modelSettings.meshCompression;
                    importer.optimizeMesh = settings.modelSettings.optimizeMesh;
                    importer.weldVertices = settings.modelSettings.weldVertices;
                    importer.importBlendShapes = settings.modelSettings.importBlendShapes;
                    importer.animationCompression = ModelImporterAnimationCompression.Optimal;
                    
                    importer.SaveAndReimport();
                    count++;
                }
            }
            
            return count;
        }
        
        int OptimizeAudio()
        {
            int count = 0;
            var audioClips = AssetDatabase.FindAssets("t:AudioClip");
            
            foreach (var guid in audioClips)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                
                if (importer != null && !IsAudioOptimized(importer))
                {
                    var sampleSettings = importer.defaultSampleSettings;
                    sampleSettings.compressionFormat = settings.audioSettings.compressionFormat;
                    sampleSettings.quality = settings.audioSettings.quality;
                    sampleSettings.loadType = settings.audioSettings.loadType;
                    
                    importer.defaultSampleSettings = sampleSettings;
                    importer.forceToMono = settings.audioSettings.forceToMono;
                    
                    importer.SaveAndReimport();
                    count++;
                }
            }
            
            return count;
        }
        
        void OptimizeShaders()
        {
            // シェーダーバリアント削減
            var collection = new ShaderVariantCollection();
            var shaders = AssetDatabase.FindAssets("t:Shader");
            
            foreach (var guid in shaders)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                
                if (shader != null)
                {
                    // 使用されているバリアントのみ含める
                    // これは実際のプロジェクトで使用されているマテリアルに基づいて行う必要があります
                }
            }
            
            AssetDatabase.CreateAsset(collection, "Assets/OptimizedShaderVariants.shadervariants");
        }
        
        void GenerateTextureAtlases()
        {
            // UI用テクスチャアトラス生成
            var uiTextures = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Textures/UI" });
            
            if (uiTextures.Length > 0)
            {
                // スプライトパッカーの設定
                EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
                
                Debug.Log($"Texture atlas generation initiated for {uiTextures.Length} UI textures");
            }
        }
        
        void BuildWithOptimization()
        {
            // 最適化を適用してからビルド
            OptimizeAllAssets();
            
            // ビルド設定
            PlayerSettings.SetManagedStrippingLevel(
                BuildTargetGroup.Standalone,
                settings.buildSettings.strippingLevel
            );
            
            PlayerSettings.stripEngineCode = settings.buildSettings.stripEngineCode;
            
            PlayerSettings.SetIl2CppCompilerConfiguration(
                BuildTargetGroup.Standalone,
                settings.buildSettings.compilerConfiguration
            );
            
            // ビルドオプション
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray(),
                locationPathName = "Builds/Optimized/SchoolLoveSimulator.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            
            if (settings.compressAssets)
            {
                buildOptions.options |= BuildOptions.CompressWithLz4HC;
            }
            
            // ビルド実行
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            lastBuildReport = report;
            
            // ビルドレポート表示
            ShowBuildReport(report);
        }
        
        void ShowBuildReport(BuildReport report)
        {
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded!");
                Debug.Log($"Total size: {FormatSize((long)report.summary.totalSize)}");
                Debug.Log($"Build time: {report.summary.totalTime}");
                
                // サイズ詳細
                foreach (var step in report.steps)
                {
                    Debug.Log($"{step.name}: {step.duration}");
                }
            }
            else
            {
                Debug.LogError($"Build failed: {report.summary.result}");
            }
        }
        
        string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }
        
        void SaveSettings()
        {
            var json = JsonUtility.ToJson(settings, true);
            EditorPrefs.SetString("BuildOptimizerSettings", json);
            Debug.Log("Settings saved");
        }
        
        void LoadSettings()
        {
            var json = EditorPrefs.GetString("BuildOptimizerSettings", "");
            if (!string.IsNullOrEmpty(json))
            {
                settings = JsonUtility.FromJson<OptimizationSettings>(json);
                Debug.Log("Settings loaded");
            }
            else
            {
                settings = new OptimizationSettings();
            }
        }
    }
}
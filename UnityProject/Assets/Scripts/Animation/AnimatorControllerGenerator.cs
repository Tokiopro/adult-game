using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;

namespace SchoolLoveSimulator.Editor
{
    /// <summary>
    /// AnimatorController自動生成ツール
    /// Blenderからインポートしたアニメーションを自動的に組み込む
    /// </summary>
    public class AnimatorControllerGenerator : EditorWindow
    {
        private string controllerName = "CharacterAnimator";
        private string savePath = "Assets/Animations/Controllers";
        private AnimationClip[] animationClips;
        private List<AnimationClipData> clipDataList = new List<AnimationClipData>();
        
        [System.Serializable]
        public class AnimationClipData
        {
            public AnimationClip clip;
            public string stateName;
            public AnimationType type;
            public bool isLooping;
            public float speed = 1f;
            
            public AnimationClipData(AnimationClip c)
            {
                clip = c;
                stateName = c.name;
                type = DetectAnimationType(c.name);
                isLooping = ShouldLoop(type);
                speed = 1f;
            }
            
            private static AnimationType DetectAnimationType(string name)
            {
                string lower = name.ToLower();
                
                if (lower.Contains("idle")) return AnimationType.Idle;
                if (lower.Contains("walk")) return AnimationType.Walk;
                if (lower.Contains("run")) return AnimationType.Run;
                if (lower.Contains("jump")) return AnimationType.Jump;
                if (lower.Contains("wave")) return AnimationType.Interaction;
                if (lower.Contains("bow")) return AnimationType.Interaction;
                if (lower.Contains("nod")) return AnimationType.Interaction;
                if (lower.Contains("sit")) return AnimationType.Interaction;
                if (lower.Contains("happy")) return AnimationType.Expression;
                if (lower.Contains("sad")) return AnimationType.Expression;
                if (lower.Contains("angry")) return AnimationType.Expression;
                if (lower.Contains("surprised")) return AnimationType.Expression;
                
                return AnimationType.Other;
            }
            
            private static bool ShouldLoop(AnimationType type)
            {
                return type == AnimationType.Idle || 
                       type == AnimationType.Walk || 
                       type == AnimationType.Run;
            }
        }
        
        public enum AnimationType
        {
            Idle,
            Walk,
            Run,
            Jump,
            Interaction,
            Expression,
            Other
        }
        
        [MenuItem("Tools/Animation/Animator Controller Generator")]
        public static void ShowWindow()
        {
            GetWindow<AnimatorControllerGenerator>("Animator Generator");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Animator Controller Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 基本設定
            controllerName = EditorGUILayout.TextField("Controller Name", controllerName);
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Clips", EditorStyles.boldLabel);
            
            // アニメーションクリップの読み込み
            if (GUILayout.Button("Load Animation Clips from Selection"))
            {
                LoadAnimationClips();
            }
            
            // クリップリスト表示
            if (clipDataList.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Found {clipDataList.Count} clips:");
                
                foreach (var clipData in clipDataList)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(clipData.clip.name, GUILayout.Width(200));
                    clipData.type = (AnimationType)EditorGUILayout.EnumPopup(clipData.type, GUILayout.Width(100));
                    clipData.isLooping = EditorGUILayout.Toggle("Loop", clipData.isLooping, GUILayout.Width(60));
                    clipData.speed = EditorGUILayout.FloatField("Speed", clipData.speed, GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.Space();
            
            // 生成ボタン
            GUI.enabled = clipDataList.Count > 0;
            if (GUILayout.Button("Generate Animator Controller", GUILayout.Height(30)))
            {
                GenerateAnimatorController();
            }
            GUI.enabled = true;
            
            EditorGUILayout.Space();
            
            // プリセットボタン
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Basic Character"))
            {
                GenerateBasicCharacterController();
            }
            if (GUILayout.Button("Advanced Character"))
            {
                GenerateAdvancedCharacterController();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        void LoadAnimationClips()
        {
            clipDataList.Clear();
            List<AnimationClip> clips = new List<AnimationClip>();
            
            // 選択されたオブジェクトからアニメーションクリップを検索
            foreach (var obj in Selection.objects)
            {
                if (obj is AnimationClip)
                {
                    clips.Add(obj as AnimationClip);
                }
                else if (obj is GameObject)
                {
                    // FBXなどからクリップを抽出
                    var path = AssetDatabase.GetAssetPath(obj);
                    var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                    foreach (var asset in assets)
                    {
                        if (asset is AnimationClip && !asset.name.StartsWith("__preview"))
                        {
                            clips.Add(asset as AnimationClip);
                        }
                    }
                }
            }
            
            // AnimationClipDataに変換
            foreach (var clip in clips)
            {
                clipDataList.Add(new AnimationClipData(clip));
            }
            
            Debug.Log($"Loaded {clipDataList.Count} animation clips");
        }
        
        void GenerateAnimatorController()
        {
            // 保存パスの確認
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            
            // AnimatorController作成
            var controller = AnimatorController.CreateAnimatorControllerAtPath(
                $"{savePath}/{controllerName}.controller"
            );
            
            // パラメータ追加
            AddParameters(controller);
            
            // レイヤー作成
            var baseLayer = controller.layers[0];
            baseLayer.name = "Base Layer";
            
            // ステート作成
            CreateStates(controller, baseLayer);
            
            // トランジション作成
            CreateTransitions(controller, baseLayer);
            
            // サブステートマシン作成
            CreateSubStateMachines(controller, baseLayer);
            
            // 保存
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Animator Controller created: {savePath}/{controllerName}.controller");
            
            // 作成したコントローラーを選択
            Selection.activeObject = controller;
        }
        
        void AddParameters(AnimatorController controller)
        {
            // Float parameters
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("DirectionX", AnimatorControllerParameterType.Float);
            controller.AddParameter("DirectionY", AnimatorControllerParameterType.Float);
            controller.AddParameter("VelocityX", AnimatorControllerParameterType.Float);
            controller.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
            
            // Bool parameters
            controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsSitting", AnimatorControllerParameterType.Bool);
            
            // Trigger parameters
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Wave", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Bow", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Nod", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("ShakeHead", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Sit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Stand", AnimatorControllerParameterType.Trigger);
            
            // Expression triggers
            controller.AddParameter("Happy", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Sad", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Angry", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Surprised", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Love", AnimatorControllerParameterType.Trigger);
            
            // Int parameters
            controller.AddParameter("ExpressionID", AnimatorControllerParameterType.Int);
            controller.AddParameter("ActionID", AnimatorControllerParameterType.Int);
        }
        
        void CreateStates(AnimatorController controller, AnimatorControllerLayer layer)
        {
            var stateMachine = layer.stateMachine;
            
            // Entry位置
            Vector3 statePosition = new Vector3(250, 50, 0);
            float verticalSpacing = 80;
            float horizontalSpacing = 200;
            
            // カテゴリごとにステート作成
            Dictionary<AnimationType, List<AnimatorState>> statesByType = 
                new Dictionary<AnimationType, List<AnimatorState>>();
            
            foreach (AnimationType type in System.Enum.GetValues(typeof(AnimationType)))
            {
                statesByType[type] = new List<AnimatorState>();
            }
            
            // クリップからステート作成
            foreach (var clipData in clipDataList)
            {
                var state = stateMachine.AddState(clipData.stateName, 
                    statePosition + new Vector3(
                        (int)clipData.type * horizontalSpacing,
                        statesByType[clipData.type].Count * verticalSpacing,
                        0
                    ));
                
                state.motion = clipData.clip;
                state.speed = clipData.speed;
                
                // ループ設定
                if (clipData.isLooping)
                {
                    state.motion.wrapMode = WrapMode.Loop;
                }
                
                statesByType[clipData.type].Add(state);
                
                // Idleをデフォルトステートに設定
                if (clipData.type == AnimationType.Idle && 
                    statesByType[AnimationType.Idle].Count == 1)
                {
                    stateMachine.defaultState = state;
                }
            }
        }
        
        void CreateTransitions(AnimatorController controller, AnimatorControllerLayer layer)
        {
            var stateMachine = layer.stateMachine;
            var states = stateMachine.states.Select(s => s.state).ToList();
            
            // Idle state を取得
            var idleState = states.FirstOrDefault(s => s.name.ToLower().Contains("idle"));
            if (idleState == null) return;
            
            // Walk states
            var walkStates = states.Where(s => s.name.ToLower().Contains("walk")).ToList();
            foreach (var walkState in walkStates)
            {
                // Idle -> Walk
                var toWalk = idleState.AddTransition(walkState);
                toWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
                toWalk.hasExitTime = false;
                toWalk.duration = 0.2f;
                
                // Walk -> Idle
                var toIdle = walkState.AddTransition(idleState);
                toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
                toIdle.hasExitTime = false;
                toIdle.duration = 0.2f;
            }
            
            // Run states
            var runStates = states.Where(s => s.name.ToLower().Contains("run")).ToList();
            foreach (var runState in runStates)
            {
                foreach (var walkState in walkStates)
                {
                    // Walk -> Run
                    var toRun = walkState.AddTransition(runState);
                    toRun.AddCondition(AnimatorConditionMode.Greater, 3f, "Speed");
                    toRun.hasExitTime = false;
                    toRun.duration = 0.15f;
                    
                    // Run -> Walk
                    var toWalk = runState.AddTransition(walkState);
                    toWalk.AddCondition(AnimatorConditionMode.Less, 3f, "Speed");
                    toWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
                    toWalk.hasExitTime = false;
                    toWalk.duration = 0.15f;
                }
            }
            
            // Interaction states (Trigger based)
            var interactionStates = states.Where(s => 
                s.name.ToLower().Contains("wave") || 
                s.name.ToLower().Contains("bow") ||
                s.name.ToLower().Contains("nod")
            ).ToList();
            
            foreach (var interactionState in interactionStates)
            {
                // Any State -> Interaction
                var fromAny = stateMachine.AddAnyStateTransition(interactionState);
                
                // トリガー名を取得
                string triggerName = GetTriggerName(interactionState.name);
                if (!string.IsNullOrEmpty(triggerName))
                {
                    fromAny.AddCondition(AnimatorConditionMode.If, 0, triggerName);
                    fromAny.hasExitTime = false;
                    fromAny.duration = 0.1f;
                }
                
                // Interaction -> Idle
                var toIdle = interactionState.AddTransition(idleState);
                toIdle.hasExitTime = true;
                toIdle.exitTime = 0.9f;
                toIdle.duration = 0.2f;
            }
        }
        
        string GetTriggerName(string stateName)
        {
            string lower = stateName.ToLower();
            
            if (lower.Contains("wave")) return "Wave";
            if (lower.Contains("bow")) return "Bow";
            if (lower.Contains("nod")) return "Nod";
            if (lower.Contains("jump")) return "Jump";
            if (lower.Contains("sit")) return "Sit";
            if (lower.Contains("happy")) return "Happy";
            if (lower.Contains("sad")) return "Sad";
            if (lower.Contains("angry")) return "Angry";
            if (lower.Contains("surprised")) return "Surprised";
            
            return "";
        }
        
        void CreateSubStateMachines(AnimatorController controller, AnimatorControllerLayer layer)
        {
            var rootStateMachine = layer.stateMachine;
            
            // Expression Sub State Machine
            var expressionSSM = rootStateMachine.AddStateMachine(
                "Expressions", 
                new Vector3(500, 200, 0)
            );
            
            // 表情ステートを移動
            var expressionStates = rootStateMachine.states
                .Where(s => s.state.name.ToLower().Contains("happy") ||
                           s.state.name.ToLower().Contains("sad") ||
                           s.state.name.ToLower().Contains("angry") ||
                           s.state.name.ToLower().Contains("surprised"))
                .Select(s => s.state)
                .ToList();
            
            foreach (var state in expressionStates)
            {
                // Note: Unity doesn't support moving states between state machines directly
                // This would need custom implementation
            }
        }
        
        void GenerateBasicCharacterController()
        {
            // 基本的なコントローラーのプリセット
            var controller = AnimatorController.CreateAnimatorControllerAtPath(
                $"{savePath}/BasicCharacter.controller"
            );
            
            // 基本パラメータ
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            
            var layer = controller.layers[0];
            var stateMachine = layer.stateMachine;
            
            // 基本ステート
            var idle = stateMachine.AddState("Idle", new Vector3(250, 50, 0));
            var walk = stateMachine.AddState("Walk", new Vector3(250, 150, 0));
            var run = stateMachine.AddState("Run", new Vector3(250, 250, 0));
            var jump = stateMachine.AddState("Jump", new Vector3(450, 150, 0));
            
            stateMachine.defaultState = idle;
            
            // トランジション
            var idleToWalk = idle.AddTransition(walk);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToWalk.hasExitTime = false;
            
            var walkToIdle = walk.AddTransition(idle);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            walkToIdle.hasExitTime = false;
            
            var walkToRun = walk.AddTransition(run);
            walkToRun.AddCondition(AnimatorConditionMode.Greater, 3f, "Speed");
            walkToRun.hasExitTime = false;
            
            var runToWalk = run.AddTransition(walk);
            runToWalk.AddCondition(AnimatorConditionMode.Less, 3f, "Speed");
            runToWalk.hasExitTime = false;
            
            var anyToJump = stateMachine.AddAnyStateTransition(jump);
            anyToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
            anyToJump.hasExitTime = false;
            
            var jumpToIdle = jump.AddTransition(idle);
            jumpToIdle.hasExitTime = true;
            jumpToIdle.exitTime = 0.9f;
            
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            
            Debug.Log("Basic Character Controller created");
        }
        
        void GenerateAdvancedCharacterController()
        {
            // 高度なコントローラーのプリセット
            var controller = AnimatorController.CreateAnimatorControllerAtPath(
                $"{savePath}/AdvancedCharacter.controller"
            );
            
            // 全パラメータ追加
            AddParameters(controller);
            
            // ブレンドツリー作成
            CreateBlendTrees(controller);
            
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            
            Debug.Log("Advanced Character Controller created");
        }
        
        void CreateBlendTrees(AnimatorController controller)
        {
            var layer = controller.layers[0];
            var stateMachine = layer.stateMachine;
            
            // Locomotion Blend Tree
            var locomotionState = stateMachine.AddState("Locomotion", new Vector3(250, 50, 0));
            
            var blendTree = new BlendTree();
            blendTree.name = "Locomotion";
            blendTree.blendType = BlendTreeType.FreeformDirectional2D;
            blendTree.blendParameter = "VelocityX";
            blendTree.blendParameterY = "VelocityY";
            
            // Note: Add motion fields would need actual clips
            // This is a template structure
            
            locomotionState.motion = blendTree;
            stateMachine.defaultState = locomotionState;
            
            AssetDatabase.AddObjectToAsset(blendTree, controller);
        }
    }
}
import bpy
import math
from mathutils import Vector

"""
キャラクター表情アニメーション作成スクリプト
シェイプキーを使用した多彩な表情変化を生成
"""

class ExpressionAnimationCreator:
    def __init__(self):
        self.fps = 30
        self.expression_duration = 2.0  # 各表情2秒
        
        # 表情定義（シェイプキー名と強度）
        self.expressions = {
            "happy": {
                "smile": 1.0,
                "eyes_happy": 0.8,
                "cheek_raise": 0.6,
                "brow_happy": 0.4
            },
            "shy": {
                "smile": 0.3,
                "eyes_down": 0.7,
                "blush": 1.0,
                "brow_worried": 0.3
            },
            "surprised": {
                "mouth_open": 0.8,
                "eyes_wide": 1.0,
                "brow_up": 0.9,
                "pupils_small": 0.5
            },
            "sad": {
                "mouth_sad": 0.8,
                "eyes_sad": 0.7,
                "brow_sad": 0.9,
                "tears": 0.3
            },
            "angry": {
                "mouth_frown": 0.7,
                "eyes_angry": 0.8,
                "brow_angry": 1.0,
                "nose_wrinkle": 0.4
            },
            "love": {
                "smile": 0.9,
                "eyes_heart": 1.0,
                "blush": 0.8,
                "sparkle": 1.0
            }
        }
        
    def get_face_mesh(self):
        """顔のメッシュオブジェクトを取得"""
        for obj in bpy.data.objects:
            if obj.type == 'MESH' and ("face" in obj.name.lower() or "head" in obj.name.lower()):
                return obj
        return None
        
    def create_shape_keys(self, mesh_obj):
        """必要なシェイプキーを作成"""
        if not mesh_obj.data.shape_keys:
            # ベースシェイプキー作成
            mesh_obj.shape_key_add(name="Basis")
            
        shape_keys = mesh_obj.data.shape_keys
        
        # 必要なシェイプキーのリスト
        required_keys = [
            # 口
            "smile", "mouth_open", "mouth_sad", "mouth_frown",
            "mouth_o", "mouth_a", "mouth_i", "mouth_u", "mouth_e",
            
            # 目
            "eyes_happy", "eyes_down", "eyes_wide", "eyes_sad",
            "eyes_angry", "eyes_heart", "eyes_closed", "blink",
            "pupils_small", "pupils_large",
            
            # 眉
            "brow_happy", "brow_worried", "brow_up", "brow_sad",
            "brow_angry", "brow_neutral",
            
            # その他
            "cheek_raise", "blush", "tears", "nose_wrinkle",
            "sparkle", "sweat"
        ]
        
        # シェイプキーを作成
        for key_name in required_keys:
            if key_name not in shape_keys.key_blocks:
                shape_key = mesh_obj.shape_key_add(name=key_name)
                # デフォルトで0に設定
                shape_key.value = 0
                
        return shape_keys
        
    def create_expression_animation(self, mesh_obj, expression_name, expression_data):
        """特定の表情アニメーションを作成"""
        shape_keys = mesh_obj.data.shape_keys
        if not shape_keys:
            return
            
        # アクション作成
        action_name = f"Expression_{expression_name}"
        action = bpy.data.actions.new(name=action_name)
        
        # 一時的にアクションを設定
        if not mesh_obj.data.shape_keys.animation_data:
            mesh_obj.data.shape_keys.animation_data_create()
        mesh_obj.data.shape_keys.animation_data.action = action
        
        total_frames = int(self.fps * self.expression_duration)
        
        # 各シェイプキーをアニメート
        for shape_name, target_value in expression_data.items():
            if shape_name in shape_keys.key_blocks:
                shape_key = shape_keys.key_blocks[shape_name]
                
                # キーフレーム設定
                # 開始（ニュートラル）
                bpy.context.scene.frame_set(1)
                shape_key.value = 0
                shape_key.keyframe_insert(data_path="value", frame=1)
                
                # 表情への変化（イーズイン）
                bpy.context.scene.frame_set(15)
                shape_key.value = target_value * 0.7
                shape_key.keyframe_insert(data_path="value", frame=15)
                
                # 最大表情
                bpy.context.scene.frame_set(30)
                shape_key.value = target_value
                shape_key.keyframe_insert(data_path="value", frame=30)
                
                # 表情維持
                bpy.context.scene.frame_set(total_frames - 15)
                shape_key.value = target_value
                shape_key.keyframe_insert(data_path="value", frame=total_frames - 15)
                
                # ニュートラルへ戻る
                bpy.context.scene.frame_set(total_frames)
                shape_key.value = 0
                shape_key.keyframe_insert(data_path="value", frame=total_frames)
                
    def create_blink_animation(self, mesh_obj):
        """瞬きアニメーション"""
        shape_keys = mesh_obj.data.shape_keys
        if not shape_keys or "blink" not in shape_keys.key_blocks:
            return
            
        action = bpy.data.actions.new(name="Blink")
        
        if not mesh_obj.data.shape_keys.animation_data:
            mesh_obj.data.shape_keys.animation_data_create()
        mesh_obj.data.shape_keys.animation_data.action = action
        
        blink_key = shape_keys.key_blocks["blink"]
        
        # 速い瞬き（6フレーム）
        blink_frames = [
            (1, 0),      # 開いている
            (3, 1),      # 閉じる
            (6, 0),      # 開く
        ]
        
        for frame, value in blink_frames:
            bpy.context.scene.frame_set(frame)
            blink_key.value = value
            blink_key.keyframe_insert(data_path="value", frame=frame)
            
    def create_lip_sync_animation(self, mesh_obj):
        """リップシンク用の母音アニメーション"""
        shape_keys = mesh_obj.data.shape_keys
        if not shape_keys:
            return
            
        vowels = ["a", "i", "u", "e", "o"]
        
        for vowel in vowels:
            shape_name = f"mouth_{vowel}"
            if shape_name not in shape_keys.key_blocks:
                continue
                
            action = bpy.data.actions.new(name=f"LipSync_{vowel.upper()}")
            
            if not mesh_obj.data.shape_keys.animation_data:
                mesh_obj.data.shape_keys.animation_data_create()
            mesh_obj.data.shape_keys.animation_data.action = action
            
            shape_key = shape_keys.key_blocks[shape_name]
            
            # 口の形への変化
            lip_frames = [
                (1, 0),      # 閉じている
                (5, 0.8),    # 開く
                (10, 1.0),   # 最大
                (15, 0.8),   # 少し閉じる
                (20, 0),     # 閉じる
            ]
            
            for frame, value in lip_frames:
                bpy.context.scene.frame_set(frame)
                shape_key.value = value
                shape_key.keyframe_insert(data_path="value", frame=frame)
                
    def create_emotion_transition(self, mesh_obj):
        """感情間のスムーズな遷移アニメーション"""
        shape_keys = mesh_obj.data.shape_keys
        if not shape_keys:
            return
            
        action = bpy.data.actions.new(name="EmotionTransition")
        
        if not mesh_obj.data.shape_keys.animation_data:
            mesh_obj.data.shape_keys.animation_data_create()
        mesh_obj.data.shape_keys.animation_data.action = action
        
        # ニュートラル → ハッピー → 恥ずかしい → ニュートラル
        transitions = [
            (1, 30, {}),  # ニュートラル
            (30, 60, self.expressions["happy"]),  # ハッピー
            (60, 90, self.expressions["shy"]),    # 恥ずかしい
            (90, 120, {})  # ニュートラルへ戻る
        ]
        
        for start_frame, end_frame, expression in transitions:
            for shape_name, target_value in expression.items():
                if shape_name in shape_keys.key_blocks:
                    shape_key = shape_keys.key_blocks[shape_name]
                    
                    bpy.context.scene.frame_set(start_frame)
                    current_value = shape_key.value
                    shape_key.keyframe_insert(data_path="value", frame=start_frame)
                    
                    bpy.context.scene.frame_set(end_frame)
                    shape_key.value = target_value
                    shape_key.keyframe_insert(data_path="value", frame=end_frame)
                    
    def set_interpolation_mode(self, mesh_obj):
        """補間モードを設定"""
        if not mesh_obj.data.shape_keys.animation_data:
            return
            
        action = mesh_obj.data.shape_keys.animation_data.action
        if not action:
            return
            
        for fcurve in action.fcurves:
            for keyframe in fcurve.keyframe_points:
                keyframe.interpolation = 'BEZIER'
                keyframe.handle_left_type = 'AUTO'
                keyframe.handle_right_type = 'AUTO'
                
    def create_all_expressions(self):
        """全表情アニメーション作成"""
        print("表情アニメーション作成開始...")
        
        # 顔メッシュ取得
        face_mesh = self.get_face_mesh()
        if not face_mesh:
            # テスト用の顔メッシュを作成
            bpy.ops.mesh.primitive_uv_sphere_add(location=(0, 0, 2))
            face_mesh = bpy.context.active_object
            face_mesh.name = "FaceMesh"
            
        # シェイプキー作成
        self.create_shape_keys(face_mesh)
        
        # 各表情アニメーション作成
        for expression_name, expression_data in self.expressions.items():
            print(f"  - {expression_name}表情作成中...")
            self.create_expression_animation(face_mesh, expression_name, expression_data)
            
        # 追加アニメーション
        print("  - 瞬きアニメーション作成中...")
        self.create_blink_animation(face_mesh)
        
        print("  - リップシンクアニメーション作成中...")
        self.create_lip_sync_animation(face_mesh)
        
        print("  - 感情遷移アニメーション作成中...")
        self.create_emotion_transition(face_mesh)
        
        # 補間設定
        self.set_interpolation_mode(face_mesh)
        
        print("\n表情アニメーション作成完了！")
        print("作成された表情:")
        for expression_name in self.expressions.keys():
            print(f"  - Expression_{expression_name}")
        print("  - Blink（瞬き）")
        print("  - LipSync_A, I, U, E, O（リップシンク）")
        print("  - EmotionTransition（感情遷移）")
        
        return face_mesh

# 表情プリセット作成
class ExpressionPresets:
    """よく使う表情の組み合わせ"""
    
    @staticmethod
    def create_conversation_set():
        """会話シーン用の表情セット"""
        presets = {
            "listening": {
                "smile": 0.2,
                "eyes_happy": 0.1,
                "brow_neutral": 1.0
            },
            "talking": {
                "smile": 0.3,
                "mouth_a": 0.5,
                "eyes_happy": 0.2
            },
            "thinking": {
                "eyes_down": 0.4,
                "brow_worried": 0.3,
                "mouth_i": 0.1
            },
            "agreeing": {
                "smile": 0.6,
                "eyes_happy": 0.5,
                "cheek_raise": 0.3
            }
        }
        return presets
        
    @staticmethod
    def create_reaction_set():
        """リアクション用の表情セット"""
        presets = {
            "embarrassed": {
                "eyes_down": 0.6,
                "blush": 1.0,
                "smile": 0.2,
                "sweat": 0.3
            },
            "excited": {
                "eyes_wide": 0.7,
                "smile": 0.9,
                "sparkle": 0.8,
                "brow_up": 0.4
            },
            "disappointed": {
                "eyes_sad": 0.5,
                "mouth_sad": 0.4,
                "brow_sad": 0.6
            },
            "confused": {
                "eyes_wide": 0.3,
                "brow_worried": 0.7,
                "mouth_o": 0.2
            }
        }
        return presets

# 実行
if __name__ == "__main__":
    creator = ExpressionAnimationCreator()
    face_mesh = creator.create_all_expressions()
    
    print("\n使用方法:")
    print("1. 各表情アニメーションはActionとして保存されています")
    print("2. UnityのAnimatorControllerで表情を切り替えできます")
    print("3. Blend Shapesとして各表情がエクスポートされます")
    print("\n表情プリセット:")
    print("ExpressionPresets.create_conversation_set() - 会話用")
    print("ExpressionPresets.create_reaction_set() - リアクション用")
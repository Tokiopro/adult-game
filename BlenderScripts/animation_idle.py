import bpy
import math
from mathutils import Vector, Quaternion

"""
キャラクター待機モーション作成スクリプト
呼吸や重心移動などの自然な待機アニメーションを生成
"""

class IdleAnimationCreator:
    def __init__(self):
        self.fps = 30
        self.duration = 4.0  # 4秒のループアニメーション
        self.total_frames = int(self.fps * self.duration)
        
    def setup_scene(self):
        """シーン設定"""
        scene = bpy.context.scene
        scene.frame_start = 1
        scene.frame_end = self.total_frames
        scene.render.fps = self.fps
        
    def get_armature(self):
        """アーマチュアを取得または作成"""
        # 既存のアーマチュアを探す
        for obj in bpy.data.objects:
            if obj.type == 'ARMATURE':
                return obj
                
        # なければ簡易的なアーマチュアを作成
        return self.create_simple_armature()
        
    def create_simple_armature(self):
        """簡易的な人型アーマチュアを作成"""
        bpy.ops.object.armature_add(location=(0, 0, 0))
        armature = bpy.context.active_object
        armature.name = "CharacterArmature"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bones = armature.data.edit_bones
        
        # ルートボーン
        root = bones.new("Root")
        root.head = (0, 0, 0)
        root.tail = (0, 0, 0.1)
        
        # 脊椎
        spine1 = bones.new("Spine1")
        spine1.head = (0, 0, 0.9)
        spine1.tail = (0, 0, 1.2)
        spine1.parent = root
        
        spine2 = bones.new("Spine2")
        spine2.head = (0, 0, 1.2)
        spine2.tail = (0, 0, 1.5)
        spine2.parent = spine1
        
        # 頭
        head = bones.new("Head")
        head.head = (0, 0, 1.5)
        head.tail = (0, 0, 1.8)
        head.parent = spine2
        
        # 左腕
        shoulder_l = bones.new("Shoulder_L")
        shoulder_l.head = (0.2, 0, 1.4)
        shoulder_l.tail = (0.5, 0, 1.4)
        shoulder_l.parent = spine2
        
        arm_l = bones.new("Arm_L")
        arm_l.head = (0.5, 0, 1.4)
        arm_l.tail = (0.8, 0, 1.1)
        arm_l.parent = shoulder_l
        
        # 右腕
        shoulder_r = bones.new("Shoulder_R")
        shoulder_r.head = (-0.2, 0, 1.4)
        shoulder_r.tail = (-0.5, 0, 1.4)
        shoulder_r.parent = spine2
        
        arm_r = bones.new("Arm_R")
        arm_r.head = (-0.5, 0, 1.4)
        arm_r.tail = (-0.8, 0, 1.1)
        arm_r.parent = shoulder_r
        
        # 左脚
        hip_l = bones.new("Hip_L")
        hip_l.head = (0.15, 0, 0.9)
        hip_l.tail = (0.15, 0, 0.45)
        hip_l.parent = root
        
        leg_l = bones.new("Leg_L")
        leg_l.head = (0.15, 0, 0.45)
        leg_l.tail = (0.15, 0, 0)
        leg_l.parent = hip_l
        
        # 右脚
        hip_r = bones.new("Hip_R")
        hip_r.head = (-0.15, 0, 0.9)
        hip_r.tail = (-0.15, 0, 0.45)
        hip_r.parent = root
        
        leg_r = bones.new("Leg_R")
        leg_r.head = (-0.15, 0, 0.45)
        leg_r.tail = (-0.15, 0, 0)
        leg_r.parent = hip_r
        
        bpy.ops.object.mode_set(mode='OBJECT')
        return armature
        
    def animate_breathing(self, armature):
        """呼吸アニメーション"""
        bpy.ops.object.mode_set(mode='POSE')
        
        # Spine2ボーン（胸部）で呼吸を表現
        spine2 = armature.pose.bones.get("Spine2")
        if not spine2:
            return
            
        # 呼吸のキーフレーム
        breathing_frames = [
            (1, 1.0),      # 通常
            (30, 1.02),    # 吸う
            (60, 1.0),     # 通常
            (90, 0.98),    # 吐く
            (120, 1.0)     # 通常（ループ）
        ]
        
        for frame, scale in breathing_frames:
            bpy.context.scene.frame_set(frame)
            spine2.scale = (scale, scale, scale)
            spine2.keyframe_insert(data_path="scale", frame=frame)
            
    def animate_weight_shift(self, armature):
        """重心移動アニメーション"""
        root = armature.pose.bones.get("Root")
        if not root:
            return
            
        # 重心移動のキーフレーム
        weight_frames = [
            (1, (0, 0, 0)),
            (40, (0.02, 0, 0)),
            (80, (-0.02, 0, 0)),
            (120, (0, 0, 0))
        ]
        
        for frame, loc in weight_frames:
            bpy.context.scene.frame_set(frame)
            root.location = loc
            root.keyframe_insert(data_path="location", frame=frame)
            
    def animate_head_movement(self, armature):
        """頭の微細な動き"""
        head = armature.pose.bones.get("Head")
        if not head:
            return
            
        # 頭の動きのキーフレーム
        head_frames = [
            (1, (0, 0, 0)),
            (20, (0, 0, math.radians(2))),
            (50, (0, math.radians(-3), 0)),
            (80, (0, 0, math.radians(-2))),
            (110, (0, math.radians(3), 0)),
            (120, (0, 0, 0))
        ]
        
        for frame, rot in head_frames:
            bpy.context.scene.frame_set(frame)
            head.rotation_euler = rot
            head.keyframe_insert(data_path="rotation_euler", frame=frame)
            
    def animate_arm_sway(self, armature):
        """腕の自然な揺れ"""
        arms = ["Arm_L", "Arm_R"]
        
        for i, arm_name in enumerate(arms):
            arm = armature.pose.bones.get(arm_name)
            if not arm:
                continue
                
            # 左右で位相をずらす
            phase_offset = math.pi if i == 1 else 0
            
            for frame in range(1, self.total_frames + 1):
                bpy.context.scene.frame_set(frame)
                
                # サインカーブで自然な揺れ
                t = (frame - 1) / self.total_frames * 2 * math.pi
                sway = math.sin(t + phase_offset) * 0.05
                
                arm.rotation_euler = (sway, 0, 0)
                arm.keyframe_insert(data_path="rotation_euler", frame=frame)
                
    def animate_blink(self):
        """瞬きアニメーション（シェイプキー用）"""
        # メッシュオブジェクトを探す
        mesh_obj = None
        for obj in bpy.data.objects:
            if obj.type == 'MESH' and "head" in obj.name.lower():
                mesh_obj = obj
                break
                
        if not mesh_obj or not mesh_obj.data.shape_keys:
            print("瞬き用のシェイプキーが見つかりません")
            return
            
        # Blinkシェイプキーを探す
        blink_key = mesh_obj.data.shape_keys.key_blocks.get("Blink")
        if not blink_key:
            return
            
        # 瞬きのタイミング（ランダム感を出す）
        blink_frames = [
            (35, 36, 37),   # 1回目の瞬き
            (85, 86, 87),   # 2回目の瞬き
        ]
        
        for blink_set in blink_frames:
            start, peak, end = blink_set
            
            # 瞬き前
            bpy.context.scene.frame_set(start - 1)
            blink_key.value = 0
            blink_key.keyframe_insert(data_path="value", frame=start - 1)
            
            # 瞬き最大
            bpy.context.scene.frame_set(peak)
            blink_key.value = 1
            blink_key.keyframe_insert(data_path="value", frame=peak)
            
            # 瞬き後
            bpy.context.scene.frame_set(end + 1)
            blink_key.value = 0
            blink_key.keyframe_insert(data_path="value", frame=end + 1)
            
    def set_interpolation_mode(self, armature):
        """補間モードを設定"""
        if not armature.animation_data or not armature.animation_data.action:
            return
            
        for fcurve in armature.animation_data.action.fcurves:
            for keyframe in fcurve.keyframe_points:
                keyframe.interpolation = 'BEZIER'
                keyframe.handle_left_type = 'AUTO'
                keyframe.handle_right_type = 'AUTO'
                
    def create_idle_animation(self):
        """待機アニメーション作成のメイン関数"""
        print("待機アニメーション作成開始...")
        
        # シーン設定
        self.setup_scene()
        
        # アーマチュア取得
        armature = self.get_armature()
        if not armature:
            print("アーマチュアが見つかりません")
            return
            
        # アクション作成
        action = bpy.data.actions.new(name="IdleAnimation")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        # 各種アニメーション適用
        self.animate_breathing(armature)
        self.animate_weight_shift(armature)
        self.animate_head_movement(armature)
        self.animate_arm_sway(armature)
        self.animate_blink()
        
        # 補間設定
        self.set_interpolation_mode(armature)
        
        # NLAトラックに追加（Unity用）
        if armature.animation_data:
            track = armature.animation_data.nla_tracks.new()
            track.name = "IdleTrack"
            strip = track.strips.new(
                name="IdleStrip",
                start=1,
                action=action
            )
            strip.repeat = 10  # ループ設定
            
        print("待機アニメーション作成完了！")
        print(f"アニメーション名: IdleAnimation")
        print(f"フレーム数: {self.total_frames}")
        print(f"長さ: {self.duration}秒")
        
        # プレビュー再生
        bpy.context.scene.frame_current = 1
        bpy.ops.screen.animation_play()

# 実行
if __name__ == "__main__":
    creator = IdleAnimationCreator()
    creator.create_idle_animation()
    
    print("\n使用方法:")
    print("1. このアニメーションはループ再生されます")
    print("2. export_to_unity.py でFBXエクスポート時に含まれます")
    print("3. Unity側でAnimatorControllerに設定してください")
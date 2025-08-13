import bpy
import math
from mathutils import Vector, Quaternion

"""
キャラクターインタラクションアニメーション作成スクリプト
プレイヤーとの対話用のジェスチャーやリアクション動作を生成
"""

class InteractionAnimationCreator:
    def __init__(self):
        self.fps = 30
        self.default_duration = 2.0
        
        # インタラクションタイプ定義
        self.interactions = {
            "wave": self.create_wave_animation,
            "bow": self.create_bow_animation,
            "nod": self.create_nod_animation,
            "shake_head": self.create_shake_head_animation,
            "point": self.create_point_animation,
            "clap": self.create_clap_animation,
            "jump": self.create_jump_animation,
            "turn_around": self.create_turn_around_animation,
            "sit_down": self.create_sit_down_animation,
            "stand_up": self.create_stand_up_animation,
            "hug": self.create_hug_animation,
            "hand_kiss": self.create_hand_kiss_animation
        }
        
    def get_armature(self):
        """アーマチュアを取得"""
        for obj in bpy.data.objects:
            if obj.type == 'ARMATURE':
                return obj
        return None
        
    def create_wave_animation(self, armature):
        """手を振るアニメーション"""
        action = bpy.data.actions.new(name="Wave")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        # 右腕を使用
        shoulder_r = armature.pose.bones.get("Shoulder_R")
        arm_r = armature.pose.bones.get("Arm_R")
        
        if not shoulder_r or not arm_r:
            return
            
        wave_frames = [
            (1, (0, 0, 0), (0, 0, 0)),  # 開始位置
            (10, (math.radians(-70), math.radians(30), math.radians(45)), 
                 (0, math.radians(-20), 0)),  # 腕を上げる
            (20, (math.radians(-70), math.radians(30), math.radians(25)), 
                 (0, math.radians(-30), 0)),  # 左に振る
            (30, (math.radians(-70), math.radians(30), math.radians(65)), 
                 (0, math.radians(-10), 0)),  # 右に振る
            (40, (math.radians(-70), math.radians(30), math.radians(25)), 
                 (0, math.radians(-30), 0)),  # 左に振る
            (50, (math.radians(-70), math.radians(30), math.radians(65)), 
                 (0, math.radians(-10), 0)),  # 右に振る
            (60, (0, 0, 0), (0, 0, 0))  # 元に戻る
        ]
        
        for frame, shoulder_rot, arm_rot in wave_frames:
            bpy.context.scene.frame_set(frame)
            shoulder_r.rotation_euler = shoulder_rot
            shoulder_r.keyframe_insert(data_path="rotation_euler", frame=frame)
            arm_r.rotation_euler = arm_rot
            arm_r.keyframe_insert(data_path="rotation_euler", frame=frame)
            
    def create_bow_animation(self, armature):
        """お辞儀アニメーション"""
        action = bpy.data.actions.new(name="Bow")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        root = armature.pose.bones.get("Root")
        spine1 = armature.pose.bones.get("Spine1")
        spine2 = armature.pose.bones.get("Spine2")
        head = armature.pose.bones.get("Head")
        
        bow_frames = [
            (1, (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0)),  # 直立
            (20, (0, 0, -0.1), 
                 (math.radians(30), 0, 0), 
                 (math.radians(15), 0, 0), 
                 (math.radians(10), 0, 0)),  # お辞儀
            (40, (0, 0, -0.1), 
                 (math.radians(30), 0, 0), 
                 (math.radians(15), 0, 0), 
                 (math.radians(10), 0, 0)),  # 維持
            (60, (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0))  # 戻る
        ]
        
        for frame, root_loc, spine1_rot, spine2_rot, head_rot in bow_frames:
            bpy.context.scene.frame_set(frame)
            
            if root:
                root.location = root_loc
                root.keyframe_insert(data_path="location", frame=frame)
            if spine1:
                spine1.rotation_euler = spine1_rot
                spine1.keyframe_insert(data_path="rotation_euler", frame=frame)
            if spine2:
                spine2.rotation_euler = spine2_rot
                spine2.keyframe_insert(data_path="rotation_euler", frame=frame)
            if head:
                head.rotation_euler = head_rot
                head.keyframe_insert(data_path="rotation_euler", frame=frame)
                
    def create_nod_animation(self, armature):
        """頷きアニメーション"""
        action = bpy.data.actions.new(name="Nod")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        head = armature.pose.bones.get("Head")
        if not head:
            return
            
        nod_frames = [
            (1, (0, 0, 0)),
            (8, (math.radians(15), 0, 0)),   # 下を向く
            (15, (math.radians(-5), 0, 0)),  # 少し上を向く
            (22, (math.radians(15), 0, 0)),  # 下を向く
            (30, (0, 0, 0))                  # 元に戻る
        ]
        
        for frame, rot in nod_frames:
            bpy.context.scene.frame_set(frame)
            head.rotation_euler = rot
            head.keyframe_insert(data_path="rotation_euler", frame=frame)
            
    def create_shake_head_animation(self, armature):
        """首を横に振るアニメーション"""
        action = bpy.data.actions.new(name="ShakeHead")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        head = armature.pose.bones.get("Head")
        if not head:
            return
            
        shake_frames = [
            (1, (0, 0, 0)),
            (8, (0, math.radians(-25), 0)),   # 左を向く
            (16, (0, math.radians(25), 0)),   # 右を向く
            (24, (0, math.radians(-25), 0)),  # 左を向く
            (32, (0, math.radians(25), 0)),   # 右を向く
            (40, (0, 0, 0))                   # 元に戻る
        ]
        
        for frame, rot in shake_frames:
            bpy.context.scene.frame_set(frame)
            head.rotation_euler = rot
            head.keyframe_insert(data_path="rotation_euler", frame=frame)
            
    def create_point_animation(self, armature):
        """指差しアニメーション"""
        action = bpy.data.actions.new(name="Point")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        shoulder_r = armature.pose.bones.get("Shoulder_R")
        arm_r = armature.pose.bones.get("Arm_R")
        
        if not shoulder_r or not arm_r:
            return
            
        point_frames = [
            (1, (0, 0, 0), (0, 0, 0)),
            (15, (math.radians(-90), 0, 0), (0, 0, 0)),  # 腕を前に出す
            (30, (math.radians(-90), 0, 0), (0, 0, 0)),  # 維持
            (45, (0, 0, 0), (0, 0, 0))                    # 戻る
        ]
        
        for frame, shoulder_rot, arm_rot in point_frames:
            bpy.context.scene.frame_set(frame)
            shoulder_r.rotation_euler = shoulder_rot
            shoulder_r.keyframe_insert(data_path="rotation_euler", frame=frame)
            arm_r.rotation_euler = arm_rot
            arm_r.keyframe_insert(data_path="rotation_euler", frame=frame)
            
    def create_clap_animation(self, armature):
        """拍手アニメーション"""
        action = bpy.data.actions.new(name="Clap")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        shoulder_l = armature.pose.bones.get("Shoulder_L")
        shoulder_r = armature.pose.bones.get("Shoulder_R")
        arm_l = armature.pose.bones.get("Arm_L")
        arm_r = armature.pose.bones.get("Arm_R")
        
        clap_frames = [
            (1, (0, 0, 0), (0, 0, 0)),
            (5, (math.radians(-60), math.radians(45), 0), 
                (math.radians(-60), math.radians(-45), 0)),  # 手を合わせる
            (10, (math.radians(-60), math.radians(30), 0), 
                 (math.radians(-60), math.radians(-30), 0)),  # 少し開く
            (15, (math.radians(-60), math.radians(45), 0), 
                 (math.radians(-60), math.radians(-45), 0)),  # 手を合わせる
            (20, (math.radians(-60), math.radians(30), 0), 
                 (math.radians(-60), math.radians(-30), 0)),  # 少し開く
            (25, (math.radians(-60), math.radians(45), 0), 
                 (math.radians(-60), math.radians(-45), 0)),  # 手を合わせる
            (35, (0, 0, 0), (0, 0, 0))                        # 戻る
        ]
        
        for frame, l_rot, r_rot in clap_frames:
            bpy.context.scene.frame_set(frame)
            if shoulder_l:
                shoulder_l.rotation_euler = l_rot
                shoulder_l.keyframe_insert(data_path="rotation_euler", frame=frame)
            if shoulder_r:
                shoulder_r.rotation_euler = r_rot
                shoulder_r.keyframe_insert(data_path="rotation_euler", frame=frame)
                
    def create_jump_animation(self, armature):
        """ジャンプアニメーション"""
        action = bpy.data.actions.new(name="Jump")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        root = armature.pose.bones.get("Root")
        if not root:
            return
            
        jump_frames = [
            (1, (0, 0, 0)),
            (8, (0, 0, -0.2)),     # しゃがむ
            (12, (0, 0, 0.8)),     # ジャンプ最高点
            (20, (0, 0, -0.1)),    # 着地
            (25, (0, 0, 0))        # 元に戻る
        ]
        
        for frame, loc in jump_frames:
            bpy.context.scene.frame_set(frame)
            root.location = loc
            root.keyframe_insert(data_path="location", frame=frame)
            
    def create_turn_around_animation(self, armature):
        """振り返りアニメーション"""
        action = bpy.data.actions.new(name="TurnAround")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        root = armature.pose.bones.get("Root")
        if not root:
            return
            
        turn_frames = [
            (1, (0, 0, 0)),
            (30, (0, 0, math.radians(180))),   # 180度回転
            (60, (0, 0, math.radians(360)))    # 元に戻る（360度）
        ]
        
        for frame, rot in turn_frames:
            bpy.context.scene.frame_set(frame)
            root.rotation_euler = rot
            root.keyframe_insert(data_path="rotation_euler", frame=frame)
            
    def create_sit_down_animation(self, armature):
        """座るアニメーション"""
        action = bpy.data.actions.new(name="SitDown")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        root = armature.pose.bones.get("Root")
        hip_l = armature.pose.bones.get("Hip_L")
        hip_r = armature.pose.bones.get("Hip_R")
        leg_l = armature.pose.bones.get("Leg_L")
        leg_r = armature.pose.bones.get("Leg_R")
        
        sit_frames = [
            (1, (0, 0, 0), (0, 0, 0), (0, 0, 0)),
            (30, (0, 0, -0.5), 
                 (math.radians(-90), 0, 0), 
                 (math.radians(90), 0, 0)),  # 座った状態
            (60, (0, 0, -0.5), 
                 (math.radians(-90), 0, 0), 
                 (math.radians(90), 0, 0))   # 維持
        ]
        
        for frame, root_loc, hip_rot, leg_rot in sit_frames:
            bpy.context.scene.frame_set(frame)
            if root:
                root.location = root_loc
                root.keyframe_insert(data_path="location", frame=frame)
            if hip_l:
                hip_l.rotation_euler = hip_rot
                hip_l.keyframe_insert(data_path="rotation_euler", frame=frame)
            if hip_r:
                hip_r.rotation_euler = hip_rot
                hip_r.keyframe_insert(data_path="rotation_euler", frame=frame)
            if leg_l:
                leg_l.rotation_euler = leg_rot
                leg_l.keyframe_insert(data_path="rotation_euler", frame=frame)
            if leg_r:
                leg_r.rotation_euler = leg_rot
                leg_r.keyframe_insert(data_path="rotation_euler", frame=frame)
                
    def create_stand_up_animation(self, armature):
        """立ち上がるアニメーション"""
        action = bpy.data.actions.new(name="StandUp")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        # SitDownの逆再生
        root = armature.pose.bones.get("Root")
        hip_l = armature.pose.bones.get("Hip_L")
        hip_r = armature.pose.bones.get("Hip_R")
        leg_l = armature.pose.bones.get("Leg_L")
        leg_r = armature.pose.bones.get("Leg_R")
        
        stand_frames = [
            (1, (0, 0, -0.5), 
                (math.radians(-90), 0, 0), 
                (math.radians(90), 0, 0)),   # 座った状態
            (30, (0, 0, 0), (0, 0, 0), (0, 0, 0))  # 立った状態
        ]
        
        for frame, root_loc, hip_rot, leg_rot in stand_frames:
            bpy.context.scene.frame_set(frame)
            if root:
                root.location = root_loc
                root.keyframe_insert(data_path="location", frame=frame)
            if hip_l:
                hip_l.rotation_euler = hip_rot
                hip_l.keyframe_insert(data_path="rotation_euler", frame=frame)
            if hip_r:
                hip_r.rotation_euler = hip_rot
                hip_r.keyframe_insert(data_path="rotation_euler", frame=frame)
            if leg_l:
                leg_l.rotation_euler = leg_rot
                leg_l.keyframe_insert(data_path="rotation_euler", frame=frame)
            if leg_r:
                leg_r.rotation_euler = leg_rot
                leg_r.keyframe_insert(data_path="rotation_euler", frame=frame)
                
    def create_hug_animation(self, armature):
        """ハグアニメーション"""
        action = bpy.data.actions.new(name="Hug")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        shoulder_l = armature.pose.bones.get("Shoulder_L")
        shoulder_r = armature.pose.bones.get("Shoulder_R")
        arm_l = armature.pose.bones.get("Arm_L")
        arm_r = armature.pose.bones.get("Arm_R")
        
        hug_frames = [
            (1, (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0)),
            (20, (math.radians(-80), math.radians(60), 0), 
                 (math.radians(-80), math.radians(-60), 0),
                 (0, math.radians(-45), 0),
                 (0, math.radians(-45), 0)),  # 腕を前に出してハグ
            (50, (math.radians(-80), math.radians(60), 0), 
                 (math.radians(-80), math.radians(-60), 0),
                 (0, math.radians(-45), 0),
                 (0, math.radians(-45), 0)),  # 維持
            (70, (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0))  # 戻る
        ]
        
        for frame, sl_rot, sr_rot, al_rot, ar_rot in hug_frames:
            bpy.context.scene.frame_set(frame)
            if shoulder_l:
                shoulder_l.rotation_euler = sl_rot
                shoulder_l.keyframe_insert(data_path="rotation_euler", frame=frame)
            if shoulder_r:
                shoulder_r.rotation_euler = sr_rot
                shoulder_r.keyframe_insert(data_path="rotation_euler", frame=frame)
            if arm_l:
                arm_l.rotation_euler = al_rot
                arm_l.keyframe_insert(data_path="rotation_euler", frame=frame)
            if arm_r:
                arm_r.rotation_euler = ar_rot
                arm_r.keyframe_insert(data_path="rotation_euler", frame=frame)
                
    def create_hand_kiss_animation(self, armature):
        """手にキスするアニメーション"""
        action = bpy.data.actions.new(name="HandKiss")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        bpy.ops.object.mode_set(mode='POSE')
        
        shoulder_r = armature.pose.bones.get("Shoulder_R")
        arm_r = armature.pose.bones.get("Arm_R")
        head = armature.pose.bones.get("Head")
        spine2 = armature.pose.bones.get("Spine2")
        
        kiss_frames = [
            (1, (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0)),
            (20, (math.radians(-120), math.radians(30), 0), 
                 (0, math.radians(-90), 0),
                 (math.radians(20), math.radians(10), 0),
                 (math.radians(10), 0, 0)),  # 手を顔に近づける
            (35, (math.radians(-120), math.radians(30), 0), 
                 (0, math.radians(-90), 0),
                 (math.radians(25), math.radians(10), 0),
                 (math.radians(15), 0, 0)),  # キス
            (50, (math.radians(-120), math.radians(30), 0), 
                 (0, math.radians(-90), 0),
                 (math.radians(20), math.radians(10), 0),
                 (math.radians(10), 0, 0)),  # 少し離す
            (70, (0, 0, 0), (0, 0, 0), (0, 0, 0), (0, 0, 0))  # 戻る
        ]
        
        for frame, s_rot, a_rot, h_rot, sp_rot in kiss_frames:
            bpy.context.scene.frame_set(frame)
            if shoulder_r:
                shoulder_r.rotation_euler = s_rot
                shoulder_r.keyframe_insert(data_path="rotation_euler", frame=frame)
            if arm_r:
                arm_r.rotation_euler = a_rot
                arm_r.keyframe_insert(data_path="rotation_euler", frame=frame)
            if head:
                head.rotation_euler = h_rot
                head.keyframe_insert(data_path="rotation_euler", frame=frame)
            if spine2:
                spine2.rotation_euler = sp_rot
                spine2.keyframe_insert(data_path="rotation_euler", frame=frame)
                
    def set_interpolation_mode(self, armature):
        """補間モードを設定"""
        if not armature.animation_data or not armature.animation_data.action:
            return
            
        for fcurve in armature.animation_data.action.fcurves:
            for keyframe in fcurve.keyframe_points:
                keyframe.interpolation = 'BEZIER'
                keyframe.handle_left_type = 'AUTO'
                keyframe.handle_right_type = 'AUTO'
                
    def create_all_interactions(self):
        """全インタラクションアニメーション作成"""
        print("インタラクションアニメーション作成開始...")
        
        # アーマチュア取得
        armature = self.get_armature()
        if not armature:
            print("アーマチュアが見つかりません")
            # アーマチュアを作成
            from animation_idle import IdleAnimationCreator
            idle_creator = IdleAnimationCreator()
            armature = idle_creator.create_simple_armature()
            
        # 各インタラクション作成
        for interaction_name, create_func in self.interactions.items():
            print(f"  - {interaction_name}作成中...")
            create_func(armature)
            self.set_interpolation_mode(armature)
            
        print("\nインタラクションアニメーション作成完了！")
        print("作成されたアニメーション:")
        for name in self.interactions.keys():
            print(f"  - {name}")
            
        return armature

# 実行
if __name__ == "__main__":
    creator = InteractionAnimationCreator()
    armature = creator.create_all_interactions()
    
    print("\n使用方法:")
    print("1. 各アニメーションはActionとして保存されています")
    print("2. UnityのAnimatorControllerでトリガー設定できます")
    print("3. DialogueSystemと連携して会話中に再生できます")
    print("\nUnityでの実装例:")
    print("- Wave: 挨拶シーンで使用")
    print("- Bow: 感謝や謝罪で使用")
    print("- Nod/ShakeHead: 会話の返答で使用")
    print("- Hug: 親密度が高い時のイベントで使用")
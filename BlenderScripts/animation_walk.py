import bpy
import math
from mathutils import Vector, Quaternion

"""
キャラクター歩行モーション作成スクリプト
自然な歩行サイクルアニメーションを生成
"""

class WalkAnimationCreator:
    def __init__(self):
        self.fps = 30
        self.duration = 1.0  # 1秒の歩行サイクル
        self.total_frames = int(self.fps * self.duration)
        self.stride_length = 0.6  # 歩幅
        
    def setup_scene(self):
        """シーン設定"""
        scene = bpy.context.scene
        scene.frame_start = 1
        scene.frame_end = self.total_frames
        scene.render.fps = self.fps
        
    def get_armature(self):
        """アーマチュアを取得"""
        for obj in bpy.data.objects:
            if obj.type == 'ARMATURE':
                return obj
        return None
        
    def create_walk_cycle(self, armature):
        """歩行サイクルの作成"""
        bpy.ops.object.mode_set(mode='POSE')
        
        # アクション作成
        action = bpy.data.actions.new(name="WalkCycle")
        armature.animation_data_create()
        armature.animation_data.action = action
        
        # 各ボーンのアニメーション
        self.animate_root_movement(armature)
        self.animate_hip_movement(armature)
        self.animate_legs(armature)
        self.animate_arms(armature)
        self.animate_spine(armature)
        self.animate_head(armature)
        
    def animate_root_movement(self, armature):
        """ルートの上下動と前進"""
        root = armature.pose.bones.get("Root")
        if not root:
            return
            
        # 歩行時の上下動
        bounce_frames = [
            (1, (0, 0, 0)),
            (8, (0, 0, 0.02)),    # 左足着地
            (15, (0, 0, -0.01)),  # 中間
            (23, (0, 0, 0.02)),   # 右足着地
            (30, (0, 0, 0))       # サイクル終了
        ]
        
        for frame, loc in bounce_frames:
            bpy.context.scene.frame_set(frame)
            root.location = loc
            root.keyframe_insert(data_path="location", frame=frame)
            
    def animate_hip_movement(self, armature):
        """腰の回転と傾き"""
        root = armature.pose.bones.get("Root")
        if not root:
            return
            
        # 腰の回転
        hip_rotation_frames = [
            (1, (0, 0, 0)),
            (8, (0, 0, math.radians(5))),     # 左足前
            (15, (0, 0, 0)),
            (23, (0, 0, math.radians(-5))),   # 右足前
            (30, (0, 0, 0))
        ]
        
        for frame, rot in hip_rotation_frames:
            bpy.context.scene.frame_set(frame)
            root.rotation_euler = rot
            root.keyframe_insert(data_path="rotation_euler", frame=frame)
            
    def animate_legs(self, armature):
        """脚の歩行動作"""
        # 左脚と右脚で位相を反転
        leg_data = [
            ("Hip_L", "Leg_L", 0),      # 左脚
            ("Hip_R", "Leg_R", 0.5)     # 右脚（半周期ずれ）
        ]
        
        for hip_name, leg_name, phase in leg_data:
            hip = armature.pose.bones.get(hip_name)
            leg = armature.pose.bones.get(leg_name)
            
            if not hip or not leg:
                continue
                
            # 歩行サイクルのキーフレーム
            for frame in range(1, self.total_frames + 1):
                bpy.context.scene.frame_set(frame)
                
                # 正規化された時間（0-1）
                t = ((frame - 1) / self.total_frames + phase) % 1.0
                
                # 太ももの動き
                if t < 0.25:  # 前方への振り出し
                    hip_angle = math.radians(30) * (t * 4)
                elif t < 0.5:  # 着地から中立
                    hip_angle = math.radians(30) * (2 - t * 4)
                elif t < 0.75:  # 後方への移動
                    hip_angle = math.radians(-20) * ((t - 0.5) * 4)
                else:  # 中立へ戻る
                    hip_angle = math.radians(-20) * (4 - t * 4)
                    
                hip.rotation_euler = (hip_angle, 0, 0)
                hip.keyframe_insert(data_path="rotation_euler", frame=frame)
                
                # 膝の動き
                if t < 0.25:  # 膝を曲げて持ち上げ
                    knee_angle = math.radians(60) * (t * 4)
                elif t < 0.5:  # 着地で伸ばす
                    knee_angle = math.radians(60) * (2 - t * 4)
                elif t < 0.75:  # 支持脚として伸びた状態
                    knee_angle = math.radians(5)
                else:  # 次の振り出しの準備
                    knee_angle = math.radians(30) * ((t - 0.75) * 4)
                    
                leg.rotation_euler = (knee_angle, 0, 0)
                leg.keyframe_insert(data_path="rotation_euler", frame=frame)
                
    def animate_arms(self, armature):
        """腕の振り"""
        arm_data = [
            ("Shoulder_L", "Arm_L", 0.5),   # 左腕（脚と逆位相）
            ("Shoulder_R", "Arm_R", 0)      # 右腕
        ]
        
        for shoulder_name, arm_name, phase in arm_data:
            shoulder = armature.pose.bones.get(shoulder_name)
            arm = armature.pose.bones.get(arm_name)
            
            if not shoulder or not arm:
                continue
                
            for frame in range(1, self.total_frames + 1):
                bpy.context.scene.frame_set(frame)
                
                t = ((frame - 1) / self.total_frames + phase) % 1.0
                
                # 肩の振り
                shoulder_swing = math.sin(t * 2 * math.pi) * math.radians(20)
                shoulder.rotation_euler = (shoulder_swing, 0, 0)
                shoulder.keyframe_insert(data_path="rotation_euler", frame=frame)
                
                # 肘の自然な曲げ
                elbow_bend = math.radians(15) + abs(math.sin(t * 2 * math.pi)) * math.radians(10)
                arm.rotation_euler = (0, elbow_bend, 0)
                arm.keyframe_insert(data_path="rotation_euler", frame=frame)
                
    def animate_spine(self, armature):
        """背骨の自然な動き"""
        spine_bones = ["Spine1", "Spine2"]
        
        for spine_name in spine_bones:
            spine = armature.pose.bones.get(spine_name)
            if not spine:
                continue
                
            # 歩行に合わせた背骨の回転
            spine_frames = [
                (1, (0, 0, 0)),
                (8, (math.radians(2), 0, math.radians(-3))),
                (15, (0, 0, 0)),
                (23, (math.radians(2), 0, math.radians(3))),
                (30, (0, 0, 0))
            ]
            
            for frame, rot in spine_frames:
                bpy.context.scene.frame_set(frame)
                spine.rotation_euler = rot
                spine.keyframe_insert(data_path="rotation_euler", frame=frame)
                
    def animate_head(self, armature):
        """頭部の安定化"""
        head = armature.pose.bones.get("Head")
        if not head:
            return
            
        # 頭は比較的安定、わずかな動きのみ
        for frame in range(1, self.total_frames + 1):
            bpy.context.scene.frame_set(frame)
            
            t = (frame - 1) / self.total_frames
            
            # 軽い上下動の補正
            head_bob = math.sin(t * 4 * math.pi) * math.radians(2)
            head.rotation_euler = (head_bob, 0, 0)
            head.keyframe_insert(data_path="rotation_euler", frame=frame)
            
    def set_interpolation_mode(self, armature):
        """補間モードを設定"""
        if not armature.animation_data or not armature.animation_data.action:
            return
            
        for fcurve in armature.animation_data.action.fcurves:
            for keyframe in fcurve.keyframe_points:
                keyframe.interpolation = 'BEZIER'
                keyframe.handle_left_type = 'AUTO'
                keyframe.handle_right_type = 'AUTO'
                
    def create_walk_animation(self):
        """歩行アニメーション作成のメイン関数"""
        print("歩行アニメーション作成開始...")
        
        # シーン設定
        self.setup_scene()
        
        # アーマチュア取得
        armature = self.get_armature()
        if not armature:
            print("アーマチュアが見つかりません")
            # アーマチュアを作成（IdleAnimationCreatorから流用）
            from animation_idle import IdleAnimationCreator
            idle_creator = IdleAnimationCreator()
            armature = idle_creator.create_simple_armature()
            
        # 歩行サイクル作成
        self.create_walk_cycle(armature)
        
        # 補間設定
        self.set_interpolation_mode(armature)
        
        # NLAトラックに追加
        if armature.animation_data:
            track = armature.animation_data.nla_tracks.new()
            track.name = "WalkTrack"
            strip = track.strips.new(
                name="WalkStrip",
                start=1,
                action=armature.animation_data.action
            )
            strip.repeat = 10  # ループ設定
            
        print("歩行アニメーション作成完了！")
        print(f"アニメーション名: WalkCycle")
        print(f"フレーム数: {self.total_frames}")
        print(f"長さ: {self.duration}秒")
        
        # プレビュー再生
        bpy.context.scene.frame_current = 1
        bpy.ops.screen.animation_play()

# バリエーション作成
class WalkVariations:
    """歩行のバリエーション作成"""
    
    @staticmethod
    def create_run_cycle():
        """走りモーション"""
        creator = WalkAnimationCreator()
        creator.duration = 0.6  # より速いサイクル
        creator.stride_length = 1.2  # より大きな歩幅
        creator.create_walk_animation()
        
        # アクション名を変更
        if bpy.context.active_object.animation_data:
            bpy.context.active_object.animation_data.action.name = "RunCycle"
            
    @staticmethod
    def create_sneak_walk():
        """忍び歩き"""
        creator = WalkAnimationCreator()
        creator.duration = 2.0  # ゆっくり
        creator.stride_length = 0.3  # 小さな歩幅
        creator.create_walk_animation()
        
        if bpy.context.active_object.animation_data:
            bpy.context.active_object.animation_data.action.name = "SneakWalk"
            
    @staticmethod
    def create_feminine_walk():
        """女性的な歩き方"""
        creator = WalkAnimationCreator()
        creator.duration = 1.2
        creator.stride_length = 0.4
        creator.create_walk_animation()
        
        # 腰の動きを強調
        if bpy.context.active_object.animation_data:
            bpy.context.active_object.animation_data.action.name = "FeminineWalk"

# 実行
if __name__ == "__main__":
    # 基本の歩行
    creator = WalkAnimationCreator()
    creator.create_walk_animation()
    
    print("\n追加バリエーション作成:")
    print("WalkVariations.create_run_cycle() - 走り")
    print("WalkVariations.create_sneak_walk() - 忍び歩き")
    print("WalkVariations.create_feminine_walk() - 女性的な歩き")
    print("\nUnityでの使用:")
    print("1. FBXエクスポートで全アニメーションが含まれます")
    print("2. AnimatorControllerでBlend Treeを使用して切り替え")
import bpy
import bmesh
from mathutils import Vector, Matrix
import math

"""
アニメキャラクター用自動リグ生成スクリプト
Rigifyを使用した高品質なリグを自動生成
"""

class AutoRigGenerator:
    def __init__(self):
        self.armature = None
        self.mesh = None
        
    def prepare_scene(self):
        """シーン準備とRigifyアドオン有効化"""
        # Rigifyアドオンを有効化
        if 'rigify' not in bpy.context.preferences.addons:
            bpy.ops.preferences.addon_enable(module='rigify')
            
        print("Rigify addon enabled")
        
    def get_selected_mesh(self):
        """選択されたメッシュを取得"""
        selected = bpy.context.selected_objects
        for obj in selected:
            if obj.type == 'MESH':
                self.mesh = obj
                return obj
        
        print("警告: メッシュを選択してください")
        return None
        
    def create_metarig(self):
        """Rigifyメタリグを作成"""
        # 既存の選択を解除
        bpy.ops.object.select_all(action='DESELECT')
        
        # Human Meta-Rigを追加
        bpy.ops.object.armature_human_metarig_add()
        self.armature = bpy.context.active_object
        self.armature.name = "CharacterMetaRig"
        
        return self.armature
        
    def adjust_metarig_to_mesh(self):
        """メタリグをメッシュに合わせて調整"""
        if not self.mesh or not self.armature:
            return
            
        # メッシュのバウンディングボックスを取得
        mesh_height = self.mesh.dimensions.z
        mesh_width = self.mesh.dimensions.x
        
        # アーマチュアをメッシュの位置に移動
        self.armature.location = self.mesh.location
        
        # 編集モードで骨を調整
        bpy.context.view_layer.objects.active = self.armature
        bpy.ops.object.mode_set(mode='EDIT')
        
        edit_bones = self.armature.data.edit_bones
        
        # 重要な骨の位置を調整
        self.adjust_spine_bones(edit_bones, mesh_height)
        self.adjust_arm_bones(edit_bones, mesh_width)
        self.adjust_leg_bones(edit_bones, mesh_height)
        self.adjust_head_bones(edit_bones, mesh_height)
        
        # 指の骨を追加/調整
        self.setup_finger_bones(edit_bones)
        
        bpy.ops.object.mode_set(mode='OBJECT')
        
    def adjust_spine_bones(self, edit_bones, mesh_height):
        """脊椎ボーンの調整"""
        spine_bones = ['spine', 'spine.001', 'spine.002', 'spine.003', 'spine.004', 'spine.005', 'spine.006']
        
        # 脊椎の高さを段階的に設定
        spine_heights = [
            0.0,      # pelvis
            0.15,     # spine
            0.35,     # spine.001
            0.55,     # spine.002
            0.75,     # spine.003 (chest)
            0.85,     # spine.004 (chest)
            0.95,     # spine.005 (neck)
            1.05,     # spine.006 (neck)
        ]
        
        for i, bone_name in enumerate(spine_bones):
            if bone_name in edit_bones:
                bone = edit_bones[bone_name]
                if i < len(spine_heights):
                    bone.head.z = spine_heights[i] * mesh_height
                    if i + 1 < len(spine_heights):
                        bone.tail.z = spine_heights[i + 1] * mesh_height
                        
    def adjust_arm_bones(self, edit_bones, mesh_width):
        """腕ボーンの調整"""
        # 左腕
        if 'shoulder.L' in edit_bones:
            shoulder_l = edit_bones['shoulder.L']
            shoulder_l.head.x = mesh_width * 0.15
            shoulder_l.tail.x = mesh_width * 0.35
            
        if 'upper_arm.L' in edit_bones:
            upper_arm_l = edit_bones['upper_arm.L']
            upper_arm_l.head.x = mesh_width * 0.35
            upper_arm_l.tail.x = mesh_width * 0.5
            
        if 'forearm.L' in edit_bones:
            forearm_l = edit_bones['forearm.L']
            forearm_l.head.x = mesh_width * 0.5
            forearm_l.tail.x = mesh_width * 0.65
            
        if 'hand.L' in edit_bones:
            hand_l = edit_bones['hand.L']
            hand_l.head.x = mesh_width * 0.65
            hand_l.tail.x = mesh_width * 0.75
            
        # 右腕（左腕のミラー）
        for bone_name in ['shoulder.R', 'upper_arm.R', 'forearm.R', 'hand.R']:
            if bone_name in edit_bones:
                bone = edit_bones[bone_name]
                bone.head.x = -bone.head.x
                bone.tail.x = -bone.tail.x
                
    def adjust_leg_bones(self, edit_bones, mesh_height):
        """脚ボーンの調整"""
        leg_bones = {
            'thigh.L': (0.45, 0.25),
            'shin.L': (0.25, 0.05),
            'foot.L': (0.05, -0.05),
            'toe.L': (-0.05, -0.1)
        }
        
        for bone_name, (head_height, tail_height) in leg_bones.items():
            if bone_name in edit_bones:
                bone = edit_bones[bone_name]
                bone.head.z = head_height * mesh_height
                bone.tail.z = tail_height * mesh_height
                
            # 右脚のミラー
            bone_name_r = bone_name.replace('.L', '.R')
            if bone_name_r in edit_bones:
                bone_r = edit_bones[bone_name_r]
                bone_r.head.z = head_height * mesh_height
                bone_r.tail.z = tail_height * mesh_height
                
    def adjust_head_bones(self, edit_bones, mesh_height):
        """頭部ボーンの調整"""
        if 'spine.006' in edit_bones:
            neck = edit_bones['spine.006']
            neck.tail.z = mesh_height * 1.1
            
        if 'head' in edit_bones:
            head = edit_bones['head']
            head.head.z = mesh_height * 1.1
            head.tail.z = mesh_height * 1.3
            
    def setup_finger_bones(self, edit_bones):
        """指ボーンのセットアップ"""
        # 指の基本設定
        fingers = ['thumb', 'f_index', 'f_middle', 'f_ring', 'f_pinky']
        
        for side in ['L', 'R']:
            for finger in fingers:
                # 各指に3つの関節を作成
                for i in range(3):
                    bone_name = f"{finger}.0{i+1}.{side}"
                    if bone_name not in edit_bones:
                        # 既存の手のボーンから新しい指ボーンを作成
                        if f'hand.{side}' in edit_bones:
                            hand = edit_bones[f'hand.{side}']
                            
                            # 新しい指ボーンを作成
                            new_bone = edit_bones.new(bone_name)
                            
                            # 位置を設定
                            finger_offset = fingers.index(finger) * 0.02
                            new_bone.head = hand.tail + Vector((finger_offset, 0, 0))
                            new_bone.tail = new_bone.head + Vector((0.02, 0, 0))
                            
                            # 親を設定
                            if i == 0:
                                new_bone.parent = hand
                            else:
                                parent_name = f"{finger}.0{i}.{side}"
                                if parent_name in edit_bones:
                                    new_bone.parent = edit_bones[parent_name]
                                    
    def generate_rigify_rig(self):
        """Rigifyリグを生成"""
        if not self.armature:
            print("エラー: メタリグが見つかりません")
            return None
            
        # アーマチュアを選択
        bpy.context.view_layer.objects.active = self.armature
        self.armature.select_set(True)
        
        # Rigifyリグを生成
        try:
            bpy.ops.pose.rigify_generate()
            
            # 生成されたリグを取得
            rig = None
            for obj in bpy.data.objects:
                if obj.type == 'ARMATURE' and 'rig' in obj.name.lower() and obj != self.armature:
                    rig = obj
                    break
                    
            if rig:
                rig.name = "CharacterRig"
                print(f"リグ生成成功: {rig.name}")
                
                # メタリグを非表示
                self.armature.hide_viewport = True
                
                return rig
            else:
                print("警告: リグが生成されませんでした")
                return None
                
        except Exception as e:
            print(f"エラー: Rigify生成失敗 - {e}")
            return None
            
    def setup_bone_layers(self, rig):
        """ボーンレイヤーの整理"""
        if not rig:
            return
            
        bpy.context.view_layer.objects.active = rig
        bpy.ops.object.mode_set(mode='POSE')
        
        # レイヤー設定
        layer_settings = {
            0: ['DEF-'],           # 変形ボーン
            1: ['MCH-'],           # メカニズムボーン
            2: ['ORG-'],           # オリジナルボーン
            8: ['torso', 'chest'], # 胴体コントロール
            9: ['upper_arm', 'forearm', 'hand'],  # 腕コントロール
            10: ['thigh', 'shin', 'foot'],        # 脚コントロール
            11: ['head', 'neck'],                 # 頭部コントロール
            16: ['finger'],                       # 指コントロール
        }
        
        pose_bones = rig.pose.bones
        
        for bone in pose_bones:
            # デフォルトで全レイヤーをオフ
            bone.bone.layers = [False] * 32
            
            # 適切なレイヤーに配置
            for layer_idx, keywords in layer_settings.items():
                for keyword in keywords:
                    if keyword in bone.name.lower():
                        bone.bone.layers[layer_idx] = True
                        break
                        
        bpy.ops.object.mode_set(mode='OBJECT')
        
    def setup_ik_constraints(self, rig):
        """IK制約の設定"""
        if not rig:
            return
            
        bpy.context.view_layer.objects.active = rig
        bpy.ops.object.mode_set(mode='POSE')
        
        pose_bones = rig.pose.bones
        
        # IKターゲットの作成
        ik_targets = {
            'hand_ik.L': 'forearm.L',
            'hand_ik.R': 'forearm.R',
            'foot_ik.L': 'shin.L',
            'foot_ik.R': 'shin.R'
        }
        
        for target_name, bone_name in ik_targets.items():
            if bone_name in pose_bones:
                bone = pose_bones[bone_name]
                
                # IK制約を追加
                ik = bone.constraints.new('IK')
                ik.chain_count = 2
                
                # ポールターゲットの設定
                pole_name = target_name.replace('_ik', '_pole')
                if pole_name in pose_bones:
                    ik.pole_target = rig
                    ik.pole_subtarget = pole_name
                    ik.pole_angle = math.radians(-90)
                    
        bpy.ops.object.mode_set(mode='OBJECT')
        
    def bind_mesh_to_rig(self, rig):
        """メッシュをリグにバインド"""
        if not self.mesh or not rig:
            print("エラー: メッシュまたはリグが見つかりません")
            return
            
        # メッシュとリグを選択
        bpy.ops.object.select_all(action='DESELECT')
        self.mesh.select_set(True)
        rig.select_set(True)
        bpy.context.view_layer.objects.active = rig
        
        # 自動ウェイトでペアレント
        try:
            bpy.ops.object.parent_set(type='ARMATURE_AUTO')
            print("メッシュをリグにバインドしました")
        except Exception as e:
            print(f"バインドエラー: {e}")
            # 手動バインドを試行
            bpy.ops.object.parent_set(type='ARMATURE_ENVELOPE')
            
    def create_custom_bone_shapes(self, rig):
        """カスタムボーンシェイプの作成"""
        if not rig:
            return
            
        # ボーンシェイプ用コレクション作成
        if "BoneShapes" not in bpy.data.collections:
            shape_collection = bpy.data.collections.new("BoneShapes")
            bpy.context.scene.collection.children.link(shape_collection)
        else:
            shape_collection = bpy.data.collections["BoneShapes"]
            
        # シェイプを作成
        shapes = {
            'Circle': self.create_circle_shape,
            'Square': self.create_square_shape,
            'Arrow': self.create_arrow_shape,
            'Sphere': self.create_sphere_shape
        }
        
        for shape_name, create_func in shapes.items():
            if shape_name not in bpy.data.objects:
                shape_obj = create_func(shape_name)
                shape_collection.objects.link(shape_obj)
                shape_obj.hide_render = True
                shape_obj.hide_viewport = True
                
    def create_circle_shape(self, name):
        """円形シェイプ作成"""
        mesh = bpy.data.meshes.new(name)
        obj = bpy.data.objects.new(name, mesh)
        
        # 円を作成
        verts = []
        edges = []
        for i in range(16):
            angle = (i / 16) * 2 * math.pi
            verts.append((math.cos(angle) * 0.1, math.sin(angle) * 0.1, 0))
            edges.append((i, (i + 1) % 16))
            
        mesh.from_pydata(verts, edges, [])
        return obj
        
    def create_square_shape(self, name):
        """四角形シェイプ作成"""
        mesh = bpy.data.meshes.new(name)
        obj = bpy.data.objects.new(name, mesh)
        
        verts = [
            (-0.1, -0.1, 0), (0.1, -0.1, 0),
            (0.1, 0.1, 0), (-0.1, 0.1, 0)
        ]
        edges = [(0, 1), (1, 2), (2, 3), (3, 0)]
        
        mesh.from_pydata(verts, edges, [])
        return obj
        
    def create_arrow_shape(self, name):
        """矢印シェイプ作成"""
        mesh = bpy.data.meshes.new(name)
        obj = bpy.data.objects.new(name, mesh)
        
        verts = [
            (0, 0, 0), (0.1, 0, 0),
            (0.08, 0.02, 0), (0.08, -0.02, 0)
        ]
        edges = [(0, 1), (1, 2), (1, 3)]
        
        mesh.from_pydata(verts, edges, [])
        return obj
        
    def create_sphere_shape(self, name):
        """球形シェイプ作成"""
        mesh = bpy.data.meshes.new(name)
        obj = bpy.data.objects.new(name, mesh)
        
        # 簡易的な球形ワイヤーフレーム
        verts = []
        edges = []
        
        # 水平円
        for i in range(8):
            angle = (i / 8) * 2 * math.pi
            verts.append((math.cos(angle) * 0.1, math.sin(angle) * 0.1, 0))
            edges.append((i, (i + 1) % 8))
            
        # 垂直円
        for i in range(8):
            angle = (i / 8) * 2 * math.pi
            verts.append((0, math.cos(angle) * 0.1, math.sin(angle) * 0.1))
            edges.append((8 + i, 8 + ((i + 1) % 8)))
            
        mesh.from_pydata(verts, edges, [])
        return obj
        
    def setup_bone_groups(self, rig):
        """ボーングループの設定"""
        if not rig:
            return
            
        bpy.context.view_layer.objects.active = rig
        bpy.ops.object.mode_set(mode='POSE')
        
        # カラーテーマ
        colors = {
            'FK_Controls': (1.0, 0.5, 0.0),    # オレンジ
            'IK_Controls': (0.0, 0.5, 1.0),    # 青
            'Root_Controls': (1.0, 0.0, 0.0),  # 赤
            'Tweak_Controls': (0.0, 1.0, 0.0), # 緑
            'Face_Controls': (1.0, 1.0, 0.0)   # 黄
        }
        
        # ボーングループを作成
        for group_name, color in colors.items():
            if group_name not in rig.pose.bone_groups:
                group = rig.pose.bone_groups.new(name=group_name)
                group.color_set = 'CUSTOM'
                group.colors.normal = color
                group.colors.select = tuple(c * 1.3 for c in color)
                group.colors.active = tuple(c * 1.5 for c in color)
                
        # ボーンをグループに割り当て
        for bone in rig.pose.bones:
            if 'ik' in bone.name.lower():
                bone.bone_group = rig.pose.bone_groups['IK_Controls']
            elif 'fk' in bone.name.lower():
                bone.bone_group = rig.pose.bone_groups['FK_Controls']
            elif 'root' in bone.name.lower() or 'torso' in bone.name.lower():
                bone.bone_group = rig.pose.bone_groups['Root_Controls']
            elif 'tweak' in bone.name.lower():
                bone.bone_group = rig.pose.bone_groups['Tweak_Controls']
            elif any(face_part in bone.name.lower() for face_part in ['eye', 'mouth', 'brow', 'nose', 'jaw']):
                bone.bone_group = rig.pose.bone_groups['Face_Controls']
                
        bpy.ops.object.mode_set(mode='OBJECT')
        
    def auto_generate_rig(self):
        """自動リグ生成のメイン処理"""
        print("="*50)
        print("自動リグ生成開始")
        print("="*50)
        
        # 準備
        self.prepare_scene()
        
        # メッシュ取得
        mesh = self.get_selected_mesh()
        if not mesh:
            print("エラー: メッシュを選択してください")
            return None
            
        # メタリグ作成
        print("1. メタリグを作成中...")
        self.create_metarig()
        
        # メッシュに合わせて調整
        print("2. メッシュに合わせて調整中...")
        self.adjust_metarig_to_mesh()
        
        # Rigifyリグ生成
        print("3. Rigifyリグを生成中...")
        rig = self.generate_rigify_rig()
        
        if rig:
            # ボーンレイヤー設定
            print("4. ボーンレイヤーを設定中...")
            self.setup_bone_layers(rig)
            
            # IK制約設定
            print("5. IK制約を設定中...")
            self.setup_ik_constraints(rig)
            
            # カスタムシェイプ作成
            print("6. カスタムボーンシェイプを作成中...")
            self.create_custom_bone_shapes(rig)
            
            # ボーングループ設定
            print("7. ボーングループを設定中...")
            self.setup_bone_groups(rig)
            
            # メッシュをバインド
            print("8. メッシュをリグにバインド中...")
            self.bind_mesh_to_rig(rig)
            
            print("="*50)
            print("✓ リグ生成完了！")
            print("="*50)
            print("\n使用方法:")
            print("1. Poseモードでキャラクターをポージング")
            print("2. IKハンドルで手足を制御")
            print("3. FKコントロールで細かい調整")
            print("4. export_to_unity.pyでUnityにエクスポート")
            
            return rig
        else:
            print("リグ生成に失敗しました")
            return None

# 実行
if __name__ == "__main__":
    generator = AutoRigGenerator()
    generator.auto_generate_rig()
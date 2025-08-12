import bpy
import bmesh
from mathutils import Vector
import math

"""
Blender用美少女キャラクターベースモデル生成スクリプト
使用方法: Blenderのスクリプトエディタで実行
"""

class AnimeCharacterCreator:
    def __init__(self):
        self.clean_scene()
        
    def clean_scene(self):
        """シーンをクリーンアップ"""
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete(use_global=False)
        
    def create_head(self):
        """アニメ風の頭部を作成"""
        # UV球を作成して変形
        bpy.ops.mesh.primitive_uv_sphere_add(
            segments=16, 
            ring_count=8,
            radius=1.0,
            location=(0, 0, 1.6)
        )
        head = bpy.context.active_object
        head.name = "Head"
        
        # 頭部を変形（アニメ風に）
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.transform.resize(value=(0.9, 0.8, 1.0))
        
        # 顔の下部を平らに
        bm = bmesh.from_edit_mesh(head.data)
        for vert in bm.verts:
            if vert.co.z < -0.3:
                vert.co.z *= 0.5
        bmesh.update_edit_mesh(head.data)
        
        bpy.ops.object.mode_set(mode='OBJECT')
        
        # スムーズシェーディング
        bpy.ops.object.shade_smooth()
        
        return head
        
    def create_body(self):
        """シンプルな体を作成"""
        # 胴体
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=8,
            radius=0.4,
            depth=1.2,
            location=(0, 0, 0.6)
        )
        body = bpy.context.active_object
        body.name = "Body"
        
        # 胴体を変形
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.transform.resize(value=(0.8, 0.6, 1.0))
        bpy.ops.object.mode_set(mode='OBJECT')
        
        bpy.ops.object.shade_smooth()
        
        return body
        
    def create_arms(self):
        """腕を作成"""
        arms = []
        
        for side, x_pos in [("L", -0.5), ("R", 0.5)]:
            # 上腕
            bpy.ops.mesh.primitive_cylinder_add(
                vertices=8,
                radius=0.12,
                depth=0.5,
                location=(x_pos, 0, 0.9),
                rotation=(0, math.radians(90), 0)
            )
            upper_arm = bpy.context.active_object
            upper_arm.name = f"UpperArm_{side}"
            
            # 前腕
            bpy.ops.mesh.primitive_cylinder_add(
                vertices=8,
                radius=0.1,
                depth=0.5,
                location=(x_pos * 1.5, 0, 0.9),
                rotation=(0, math.radians(90), 0)
            )
            lower_arm = bpy.context.active_object
            lower_arm.name = f"LowerArm_{side}"
            
            # 手
            bpy.ops.mesh.primitive_uv_sphere_add(
                segments=8,
                ring_count=4,
                radius=0.15,
                location=(x_pos * 2, 0, 0.9)
            )
            hand = bpy.context.active_object
            hand.name = f"Hand_{side}"
            
            arms.extend([upper_arm, lower_arm, hand])
            
        # スムーズシェーディング
        for arm in arms:
            arm.select_set(True)
            bpy.context.view_layer.objects.active = arm
            bpy.ops.object.shade_smooth()
            arm.select_set(False)
            
        return arms
        
    def create_legs(self):
        """脚を作成"""
        legs = []
        
        for side, x_pos in [("L", -0.2), ("R", 0.2)]:
            # 太もも
            bpy.ops.mesh.primitive_cylinder_add(
                vertices=8,
                radius=0.15,
                depth=0.6,
                location=(x_pos, 0, -0.3)
            )
            thigh = bpy.context.active_object
            thigh.name = f"Thigh_{side}"
            
            # すね
            bpy.ops.mesh.primitive_cylinder_add(
                vertices=8,
                radius=0.12,
                depth=0.6,
                location=(x_pos, 0, -0.9)
            )
            shin = bpy.context.active_object
            shin.name = f"Shin_{side}"
            
            # 足
            bpy.ops.mesh.primitive_cube_add(
                size=0.3,
                location=(x_pos, 0.1, -1.3)
            )
            foot = bpy.context.active_object
            foot.name = f"Foot_{side}"
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.transform.resize(value=(0.8, 1.5, 0.3))
            bpy.ops.object.mode_set(mode='OBJECT')
            
            legs.extend([thigh, shin, foot])
            
        # スムーズシェーディング
        for leg in legs:
            leg.select_set(True)
            bpy.context.view_layer.objects.active = leg
            bpy.ops.object.shade_smooth()
            leg.select_set(False)
            
        return legs
        
    def create_hair(self):
        """髪の毛を作成（シンプルなメッシュ）"""
        # ベースとなる球を作成
        bpy.ops.mesh.primitive_uv_sphere_add(
            segments=16,
            ring_count=8,
            radius=1.1,
            location=(0, 0, 1.7)
        )
        hair = bpy.context.active_object
        hair.name = "Hair"
        
        # 髪の形に変形
        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(hair.data)
        
        for vert in bm.verts:
            # 前髪
            if vert.co.y > 0 and vert.co.z > 0:
                vert.co.y *= 1.2
            # 後ろ髪を長く
            if vert.co.y < 0:
                vert.co.z -= 0.3
                vert.co.y *= 1.3
            # 顔の部分を削除
            if vert.co.z < 0.5 and vert.co.y > -0.3:
                vert.co = (0, 0, 0)
                
        bmesh.update_edit_mesh(hair.data)
        bpy.ops.mesh.remove_doubles()
        bpy.ops.object.mode_set(mode='OBJECT')
        
        bpy.ops.object.shade_smooth()
        
        return hair
        
    def create_eyes(self):
        """アニメ風の大きな目を作成"""
        eyes = []
        
        for side, x_pos in [("L", -0.25), ("R", 0.25)]:
            # 目の球体
            bpy.ops.mesh.primitive_uv_sphere_add(
                segments=16,
                ring_count=8,
                radius=0.15,
                location=(x_pos, 0.7, 1.6)
            )
            eye = bpy.context.active_object
            eye.name = f"Eye_{side}"
            
            # 目を平らに変形
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.transform.resize(value=(1.0, 0.3, 1.2))
            bpy.ops.object.mode_set(mode='OBJECT')
            
            eyes.append(eye)
            
        return eyes
        
    def join_all_parts(self):
        """全パーツを結合"""
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.join()
        
        character = bpy.context.active_object
        character.name = "AnimeCharacter"
        
        # 原点を足元に設定
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.origin_set(type='ORIGIN_CENTER_OF_MASS', center='BOUNDS')
        character.location = (0, 0, 1.5)
        
        return character
        
    def add_materials(self):
        """基本マテリアルを追加"""
        character = bpy.context.active_object
        
        # 肌マテリアル
        skin_mat = bpy.data.materials.new(name="Skin")
        skin_mat.use_nodes = True
        skin_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (1.0, 0.8, 0.7, 1.0)
        
        # 髪マテリアル
        hair_mat = bpy.data.materials.new(name="Hair")
        hair_mat.use_nodes = True
        hair_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.8, 0.5, 0.3, 1.0)
        
        # 服マテリアル
        cloth_mat = bpy.data.materials.new(name="Cloth")
        cloth_mat.use_nodes = True
        cloth_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.2, 0.3, 0.8, 1.0)
        
        # マテリアルを割り当て
        character.data.materials.append(skin_mat)
        character.data.materials.append(hair_mat)
        character.data.materials.append(cloth_mat)
        
    def create_character(self):
        """キャラクターを作成"""
        print("Creating anime character base...")
        
        # 各パーツを作成
        self.create_head()
        self.create_body()
        self.create_arms()
        self.create_legs()
        self.create_hair()
        self.create_eyes()
        
        # パーツを結合
        self.join_all_parts()
        
        # マテリアルを追加
        self.add_materials()
        
        print("Character creation complete!")
        print("次のステップ:")
        print("1. Sculpt Modeで詳細を追加")
        print("2. Weight Paintでリグ設定")
        print("3. テクスチャペイント")
        
# スクリプト実行
if __name__ == "__main__":
    creator = AnimeCharacterCreator()
    creator.create_character()
    
    # ビューを調整
    for area in bpy.context.screen.areas:
        if area.type == 'VIEW_3D':
            for space in area.spaces:
                if space.type == 'VIEW_3D':
                    space.shading.type = 'SOLID'
                    space.shading.color_type = 'MATERIAL'
    
    # フレームを調整
    bpy.ops.view3d.view_all()
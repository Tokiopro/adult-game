import bpy
import bmesh
from mathutils import Vector
import math

"""
美咲（元気系チアリーダー）の3Dモデル作成スクリプト
"""

class MisakiCharacterCreator:
    def __init__(self):
        self.clean_scene()
        self.character_name = "Misaki"
        self.hair_color = (1.0, 0.6, 0.4, 1.0)  # 明るい茶髪
        self.eye_color = (0.8, 0.4, 0.2, 1.0)   # 茶色の瞳
        self.skin_color = (1.0, 0.85, 0.75, 1.0)  # 健康的な肌色
        
    def clean_scene(self):
        """シーンをクリーンアップ"""
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete(use_global=False)
        
    def create_head(self):
        """元気な表情の頭部を作成"""
        bpy.ops.mesh.primitive_uv_sphere_add(
            segments=32, 
            ring_count=16,
            radius=0.8,
            location=(0, 0, 1.6)
        )
        head = bpy.context.active_object
        head.name = f"{self.character_name}_Head"
        
        # 顔の形を調整（丸顔）
        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(head.data)
        
        for vert in bm.verts:
            # 頬をふっくら
            if abs(vert.co.x) > 0.3 and vert.co.z < 0.2 and vert.co.z > -0.2:
                vert.co.x *= 1.1
            # 顎を小さく
            if vert.co.z < -0.4:
                vert.co.x *= 0.7
                vert.co.y *= 0.8
        
        bmesh.update_edit_mesh(head.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.shade_smooth()
        
        return head
        
    def create_hair(self):
        """ツインテールの髪型を作成"""
        hair_parts = []
        
        # メインの髪
        bpy.ops.mesh.primitive_uv_sphere_add(
            segments=24,
            ring_count=12,
            radius=0.9,
            location=(0, -0.1, 1.7)
        )
        main_hair = bpy.context.active_object
        main_hair.name = f"{self.character_name}_Hair_Main"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(1.0, 1.2, 1.1))
        bpy.ops.object.mode_set(mode='OBJECT')
        hair_parts.append(main_hair)
        
        # ツインテール（左右）
        for side, x_pos in [("L", -0.6), ("R", 0.6)]:
            # ベース部分
            bpy.ops.mesh.primitive_cylinder_add(
                vertices=16,
                radius=0.25,
                depth=0.8,
                location=(x_pos, -0.2, 1.5),
                rotation=(math.radians(30), 0, math.radians(30) if side == "L" else -math.radians(30))
            )
            tail_base = bpy.context.active_object
            tail_base.name = f"{self.character_name}_TwinTail_Base_{side}"
            hair_parts.append(tail_base)
            
            # 先端部分（カール）
            bpy.ops.mesh.primitive_torus_add(
                major_radius=0.3,
                minor_radius=0.15,
                location=(x_pos * 1.5, -0.3, 1.2),
                rotation=(math.radians(45), 0, 0)
            )
            tail_end = bpy.context.active_object
            tail_end.name = f"{self.character_name}_TwinTail_End_{side}"
            hair_parts.append(tail_end)
        
        # 前髪
        bpy.ops.mesh.primitive_cube_add(
            size=0.5,
            location=(0, 0.6, 1.8)
        )
        bangs = bpy.context.active_object
        bangs.name = f"{self.character_name}_Bangs"
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(1.5, 0.3, 0.5))
        bpy.ops.object.mode_set(mode='OBJECT')
        hair_parts.append(bangs)
        
        # すべての髪パーツを結合
        for part in hair_parts:
            part.select_set(True)
        bpy.context.view_layer.objects.active = hair_parts[0]
        bpy.ops.object.join()
        
        hair = bpy.context.active_object
        hair.name = f"{self.character_name}_Hair"
        bpy.ops.object.shade_smooth()
        
        return hair
        
    def create_body(self):
        """スポーティーな体型"""
        # チアリーダーらしい体型
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=16,
            radius=0.35,
            depth=1.0,
            location=(0, 0, 0.7)
        )
        body = bpy.context.active_object
        body.name = f"{self.character_name}_Body"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(body.data)
        
        for vert in bm.verts:
            # ウエストを細く
            if abs(vert.co.z) < 0.1:
                vert.co.x *= 0.85
                vert.co.y *= 0.85
            # 胸部を調整
            if vert.co.z > 0.2:
                vert.co.y *= 1.1
        
        bmesh.update_edit_mesh(body.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.shade_smooth()
        
        return body
        
    def create_cheerleader_uniform(self):
        """チアリーダーユニフォームのメッシュ"""
        # スカート
        bpy.ops.mesh.primitive_cone_add(
            vertices=24,
            radius1=0.4,
            radius2=0.5,
            depth=0.4,
            location=(0, 0, 0.3)
        )
        skirt = bpy.context.active_object
        skirt.name = f"{self.character_name}_Skirt"
        
        # トップス（簡略化）
        bpy.ops.mesh.primitive_cube_add(
            size=0.5,
            location=(0, 0, 0.9)
        )
        top = bpy.context.active_object
        top.name = f"{self.character_name}_Top"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(0.8, 0.6, 0.8))
        bpy.ops.object.mode_set(mode='OBJECT')
        
        # 結合
        skirt.select_set(True)
        top.select_set(True)
        bpy.context.view_layer.objects.active = top
        bpy.ops.object.join()
        
        uniform = bpy.context.active_object
        uniform.name = f"{self.character_name}_Uniform"
        bpy.ops.object.shade_smooth()
        
        return uniform
        
    def create_eyes(self):
        """大きく元気な目"""
        eyes = []
        
        for side, x_pos in [("L", -0.2), ("R", 0.2)]:
            # 目の球体
            bpy.ops.mesh.primitive_uv_sphere_add(
                segments=16,
                ring_count=8,
                radius=0.12,
                location=(x_pos, 0.65, 1.6)
            )
            eye = bpy.context.active_object
            eye.name = f"{self.character_name}_Eye_{side}"
            
            # 大きめの目に変形
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.transform.resize(value=(1.2, 0.3, 1.3))
            bpy.ops.object.mode_set(mode='OBJECT')
            
            eyes.append(eye)
            
        return eyes
        
    def create_accessories(self):
        """ポンポンなどのアクセサリー"""
        accessories = []
        
        # ポンポン（簡略化）
        for side, x_pos in [("L", -0.8), ("R", 0.8)]:
            bpy.ops.mesh.primitive_ico_sphere_add(
                subdivisions=2,
                radius=0.15,
                location=(x_pos, 0.3, -0.5)
            )
            pompom = bpy.context.active_object
            pompom.name = f"{self.character_name}_Pompom_{side}"
            accessories.append(pompom)
        
        return accessories
        
    def add_materials(self):
        """美咲用のマテリアル設定"""
        obj = bpy.context.active_object
        
        # 既存のマテリアルをクリア
        obj.data.materials.clear()
        
        # 肌マテリアル
        skin_mat = bpy.data.materials.new(name=f"{self.character_name}_Skin")
        skin_mat.use_nodes = True
        skin_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = self.skin_color
        skin_mat.node_tree.nodes["Principled BSDF"].inputs[7].default_value = 0.5  # Roughness
        
        # 髪マテリアル
        hair_mat = bpy.data.materials.new(name=f"{self.character_name}_Hair")
        hair_mat.use_nodes = True
        hair_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = self.hair_color
        hair_mat.node_tree.nodes["Principled BSDF"].inputs[7].default_value = 0.3
        
        # ユニフォームマテリアル（赤と白）
        uniform_mat = bpy.data.materials.new(name=f"{self.character_name}_Uniform")
        uniform_mat.use_nodes = True
        uniform_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.9, 0.1, 0.2, 1.0)
        
        # 目のマテリアル
        eye_mat = bpy.data.materials.new(name=f"{self.character_name}_Eyes")
        eye_mat.use_nodes = True
        eye_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = self.eye_color
        eye_mat.node_tree.nodes["Principled BSDF"].inputs[7].default_value = 0.1
        
        obj.data.materials.append(skin_mat)
        obj.data.materials.append(hair_mat)
        obj.data.materials.append(uniform_mat)
        obj.data.materials.append(eye_mat)
        
    def create_character(self):
        """美咲のキャラクターを作成"""
        print(f"Creating {self.character_name} (Cheerleader)...")
        
        # パーツ作成
        parts = []
        parts.append(self.create_head())
        parts.append(self.create_hair())
        parts.append(self.create_body())
        parts.append(self.create_cheerleader_uniform())
        parts.extend(self.create_eyes())
        parts.extend(self.create_accessories())
        
        # すべてのパーツを選択
        for part in parts:
            if part:
                part.select_set(True)
        
        # アクティブオブジェクトを設定
        if parts:
            bpy.context.view_layer.objects.active = parts[0]
            
        # 結合
        bpy.ops.object.join()
        
        character = bpy.context.active_object
        character.name = f"{self.character_name}_Model"
        
        # マテリアル追加
        self.add_materials()
        
        # 位置調整
        character.location = (0, 0, 0)
        
        print(f"{self.character_name} model created successfully!")
        return character

# 実行
if __name__ == "__main__":
    creator = MisakiCharacterCreator()
    character = creator.create_character()
    
    # ビューを調整
    for area in bpy.context.screen.areas:
        if area.type == 'VIEW_3D':
            for space in area.spaces:
                if space.type == 'VIEW_3D':
                    space.shading.type = 'SOLID'
                    space.shading.color_type = 'MATERIAL'
    
    bpy.ops.view3d.view_all()
    
    print(f"\n{character.name} is ready for export to Unity!")
    print("Use export_to_unity.py to export as FBX")
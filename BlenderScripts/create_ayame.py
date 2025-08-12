import bpy
import bmesh
from mathutils import Vector
import math

"""
あやめ（内気な美術部員）の3Dモデル作成スクリプト
"""

class AyameCharacterCreator:
    def __init__(self):
        self.clean_scene()
        self.character_name = "Ayame"
        self.hair_color = (0.5, 0.3, 0.5, 1.0)  # 紫がかった髪色
        self.eye_color = (0.6, 0.4, 0.7, 1.0)   # 紫の瞳
        self.skin_color = (1.0, 0.9, 0.85, 1.0)  # やや薄い肌色
        
    def clean_scene(self):
        """シーンをクリーンアップ"""
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete(use_global=False)
        
    def create_head(self):
        """優しく内気な表情の頭部"""
        bpy.ops.mesh.primitive_uv_sphere_add(
            segments=32, 
            ring_count=16,
            radius=0.72,
            location=(0, 0, 1.55)
        )
        head = bpy.context.active_object
        head.name = f"{self.character_name}_Head"
        
        # 顔の形を調整（小顔）
        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(head.data)
        
        for vert in bm.verts:
            # 全体的に小さめ
            vert.co *= 0.95
            # 頬を少しふっくら
            if abs(vert.co.x) > 0.25 and vert.co.z < 0 and vert.co.z > -0.3:
                vert.co.x *= 1.05
            # 顎を小さく丸く
            if vert.co.z < -0.35:
                vert.co.x *= 0.8
                vert.co.y *= 0.85
        
        bmesh.update_edit_mesh(head.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.shade_smooth()
        
        return head
        
    def create_hair(self):
        """セミロングのふわふわヘア"""
        hair_parts = []
        
        # メインの髪（セミロング）
        bpy.ops.mesh.primitive_uv_sphere_add(
            segments=24,
            ring_count=16,
            radius=0.85,
            location=(0, -0.15, 1.6)
        )
        main_hair = bpy.context.active_object
        main_hair.name = f"{self.character_name}_Hair_Main"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(main_hair.data)
        
        for vert in bm.verts:
            # 後ろに流れる髪
            if vert.co.y < 0:
                vert.co.y *= 1.3
                vert.co.z -= abs(vert.co.y) * 0.2
            # ふわふわ感を出す
            if vert.co.z > 0:
                vert.co.x *= 1.1
                vert.co.y *= 1.1
        
        bmesh.update_edit_mesh(main_hair.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        hair_parts.append(main_hair)
        
        # 前髪（目が少し隠れる）
        bpy.ops.mesh.primitive_cube_add(
            size=0.5,
            location=(0, 0.62, 1.75)
        )
        bangs = bpy.context.active_object
        bangs.name = f"{self.character_name}_Bangs"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(1.4, 0.3, 0.7))
        bm = bmesh.from_edit_mesh(bangs.data)
        
        # 前髪を自然に
        for vert in bm.verts:
            if vert.co.z < 0:
                vert.co.z *= 0.7
                vert.co.y += 0.05
        
        bmesh.update_edit_mesh(bangs.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        hair_parts.append(bangs)
        
        # サイドの髪（顔を隠すように）
        for side, x_pos in [("L", -0.45), ("R", 0.45)]:
            bpy.ops.mesh.primitive_cube_add(
                size=0.4,
                location=(x_pos, 0.2, 1.4)
            )
            side_hair = bpy.context.active_object
            side_hair.name = f"{self.character_name}_SideHair_{side}"
            
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.transform.resize(value=(0.3, 0.5, 1.5))
            bpy.ops.transform.rotate(value=math.radians(10 if side == "L" else -10), orient_axis='Z')
            bpy.ops.object.mode_set(mode='OBJECT')
            hair_parts.append(side_hair)
        
        # ヘアピン（簡略化）
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=8,
            radius=0.02,
            depth=0.1,
            location=(-0.3, 0.6, 1.8),
            rotation=(math.radians(90), 0, math.radians(45))
        )
        hairpin = bpy.context.active_object
        hairpin.name = f"{self.character_name}_Hairpin"
        hair_parts.append(hairpin)
        
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
        """小柄な体型"""
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=16,
            radius=0.28,
            depth=0.95,
            location=(0, 0, 0.65)
        )
        body = bpy.context.active_object
        body.name = f"{self.character_name}_Body"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(body.data)
        
        for vert in bm.verts:
            # 小柄で華奢な体型
            vert.co *= 0.9
            # なで肩
            if vert.co.z > 0.3:
                vert.co.x *= 0.85
        
        bmesh.update_edit_mesh(body.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.shade_smooth()
        
        return body
        
    def create_uniform(self):
        """美術部のエプロン付き制服"""
        uniform_parts = []
        
        # セーラー服（上）
        bpy.ops.mesh.primitive_cube_add(
            size=0.5,
            location=(0, 0, 0.85)
        )
        top = bpy.context.active_object
        top.name = f"{self.character_name}_Top"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(0.75, 0.6, 0.9))
        bpy.ops.object.mode_set(mode='OBJECT')
        uniform_parts.append(top)
        
        # スカート（長め）
        bpy.ops.mesh.primitive_cone_add(
            vertices=20,
            radius1=0.3,
            radius2=0.4,
            depth=0.7,
            location=(0, 0, 0.25)
        )
        skirt = bpy.context.active_object
        skirt.name = f"{self.character_name}_Skirt"
        uniform_parts.append(skirt)
        
        # エプロン（美術部）
        bpy.ops.mesh.primitive_cube_add(
            size=0.4,
            location=(0, 0.15, 0.7)
        )
        apron = bpy.context.active_object
        apron.name = f"{self.character_name}_Apron"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(0.8, 0.1, 1.2))
        bpy.ops.object.mode_set(mode='OBJECT')
        uniform_parts.append(apron)
        
        # 結合
        for part in uniform_parts:
            part.select_set(True)
        bpy.context.view_layer.objects.active = uniform_parts[0]
        bpy.ops.object.join()
        
        uniform = bpy.context.active_object
        uniform.name = f"{self.character_name}_Uniform"
        bpy.ops.object.shade_smooth()
        
        return uniform
        
    def create_eyes(self):
        """大きくて優しい目"""
        eyes = []
        
        for side, x_pos in [("L", -0.17), ("R", 0.17)]:
            # 目の球体
            bpy.ops.mesh.primitive_uv_sphere_add(
                segments=16,
                ring_count=8,
                radius=0.11,
                location=(x_pos, 0.63, 1.55)
            )
            eye = bpy.context.active_object
            eye.name = f"{self.character_name}_Eye_{side}"
            
            # 大きくて丸い目
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.transform.resize(value=(1.1, 0.3, 1.2))
            # 少し下向き（内気な表情）
            bpy.ops.transform.rotate(value=math.radians(-5), orient_axis='X')
            bpy.ops.object.mode_set(mode='OBJECT')
            
            eyes.append(eye)
            
        return eyes
        
    def create_accessories(self):
        """スケッチブックと鉛筆"""
        accessories = []
        
        # スケッチブック（簡略化）
        bpy.ops.mesh.primitive_cube_add(
            size=0.3,
            location=(-0.5, 0.3, 0.5)
        )
        sketchbook = bpy.context.active_object
        sketchbook.name = f"{self.character_name}_Sketchbook"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(1.0, 0.05, 1.3))
        bpy.ops.transform.rotate(value=math.radians(15), orient_axis='Y')
        bpy.ops.object.mode_set(mode='OBJECT')
        accessories.append(sketchbook)
        
        # 鉛筆（簡略化）
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=6,
            radius=0.015,
            depth=0.3,
            location=(-0.4, 0.35, 0.7),
            rotation=(math.radians(80), 0, math.radians(30))
        )
        pencil = bpy.context.active_object
        pencil.name = f"{self.character_name}_Pencil"
        accessories.append(pencil)
        
        # ベレー帽（美術部らしさ）
        bpy.ops.mesh.primitive_uv_sphere_add(
            segments=16,
            ring_count=8,
            radius=0.35,
            location=(0.1, 0, 2.0)
        )
        beret = bpy.context.active_object
        beret.name = f"{self.character_name}_Beret"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(1.0, 1.0, 0.3))
        bpy.ops.transform.rotate(value=math.radians(20), orient_axis='X')
        bpy.ops.object.mode_set(mode='OBJECT')
        accessories.append(beret)
        
        return accessories
        
    def add_materials(self):
        """あやめ用のマテリアル設定"""
        obj = bpy.context.active_object
        
        # 既存のマテリアルをクリア
        obj.data.materials.clear()
        
        # 肌マテリアル
        skin_mat = bpy.data.materials.new(name=f"{self.character_name}_Skin")
        skin_mat.use_nodes = True
        skin_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = self.skin_color
        skin_mat.node_tree.nodes["Principled BSDF"].inputs[7].default_value = 0.45
        
        # 髪マテリアル（紫がかった色）
        hair_mat = bpy.data.materials.new(name=f"{self.character_name}_Hair")
        hair_mat.use_nodes = True
        hair_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = self.hair_color
        hair_mat.node_tree.nodes["Principled BSDF"].inputs[7].default_value = 0.35
        
        # 制服マテリアル（セーラー服）
        uniform_mat = bpy.data.materials.new(name=f"{self.character_name}_Uniform")
        uniform_mat.use_nodes = True
        uniform_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.2, 0.2, 0.4, 1.0)
        
        # エプロンマテリアル（ベージュ）
        apron_mat = bpy.data.materials.new(name=f"{self.character_name}_Apron")
        apron_mat.use_nodes = True
        apron_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.9, 0.85, 0.7, 1.0)
        
        # 目のマテリアル（紫の瞳）
        eye_mat = bpy.data.materials.new(name=f"{self.character_name}_Eyes")
        eye_mat.use_nodes = True
        eye_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = self.eye_color
        eye_mat.node_tree.nodes["Principled BSDF"].inputs[7].default_value = 0.15
        
        # ベレー帽マテリアル
        beret_mat = bpy.data.materials.new(name=f"{self.character_name}_Beret")
        beret_mat.use_nodes = True
        beret_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.3, 0.2, 0.3, 1.0)
        
        obj.data.materials.append(skin_mat)
        obj.data.materials.append(hair_mat)
        obj.data.materials.append(uniform_mat)
        obj.data.materials.append(apron_mat)
        obj.data.materials.append(eye_mat)
        obj.data.materials.append(beret_mat)
        
    def create_character(self):
        """あやめのキャラクターを作成"""
        print(f"Creating {self.character_name} (Art Club Member)...")
        
        # パーツ作成
        parts = []
        parts.append(self.create_head())
        parts.append(self.create_hair())
        parts.append(self.create_body())
        parts.append(self.create_uniform())
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
        
        # 位置調整（少し小さめ）
        character.location = (0, 0, -0.05)
        character.scale = (0.95, 0.95, 0.95)
        
        print(f"{self.character_name} model created successfully!")
        return character

# 実行
if __name__ == "__main__":
    creator = AyameCharacterCreator()
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
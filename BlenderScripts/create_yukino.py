import bpy
import bmesh
from mathutils import Vector
import math

"""
雪乃（クールな生徒会長）の3Dモデル作成スクリプト
"""

class YukinoCharacterCreator:
    def __init__(self):
        self.clean_scene()
        self.character_name = "Yukino"
        self.hair_color = (0.1, 0.1, 0.2, 1.0)  # 黒髪に近い紺色
        self.eye_color = (0.3, 0.6, 0.8, 1.0)   # 青い瞳
        self.skin_color = (1.0, 0.95, 0.9, 1.0)  # 色白
        
    def clean_scene(self):
        """シーンをクリーンアップ"""
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete(use_global=False)
        
    def create_head(self):
        """クールで整った顔立ち"""
        bpy.ops.mesh.primitive_uv_sphere_add(
            segments=32, 
            ring_count=16,
            radius=0.75,
            location=(0, 0, 1.65)
        )
        head = bpy.context.active_object
        head.name = f"{self.character_name}_Head"
        
        # 顔の形を調整（シャープな輪郭）
        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(head.data)
        
        for vert in bm.verts:
            # 頬をシャープに
            if abs(vert.co.x) > 0.3 and vert.co.z < 0.1 and vert.co.z > -0.3:
                vert.co.x *= 0.9
            # 顎をシャープに
            if vert.co.z < -0.4:
                vert.co.x *= 0.6
                vert.co.y *= 0.7
                vert.co.z *= 1.1
        
        bmesh.update_edit_mesh(head.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.shade_smooth()
        
        return head
        
    def create_hair(self):
        """ロングストレートヘア"""
        hair_parts = []
        
        # メインの髪（ロング）
        bpy.ops.mesh.primitive_cube_add(
            size=1,
            location=(0, -0.2, 1.5)
        )
        main_hair = bpy.context.active_object
        main_hair.name = f"{self.character_name}_Hair_Main"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(0.9, 0.8, 1.5))
        
        # 髪を滑らかに変形
        bm = bmesh.from_edit_mesh(main_hair.data)
        for vert in bm.verts:
            # 後ろに流れる髪
            if vert.co.z < 0:
                vert.co.y -= vert.co.z * 0.3
            # 先端を細く
            if vert.co.z < -0.5:
                vert.co.x *= 0.7
                vert.co.y *= 0.8
        
        bmesh.update_edit_mesh(main_hair.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        
        # Subdivision Surface モディファイア追加
        subdiv = main_hair.modifiers.new(name="Subdivision", type='SUBSURF')
        subdiv.levels = 2
        
        hair_parts.append(main_hair)
        
        # 前髪（きっちり分け）
        bpy.ops.mesh.primitive_cube_add(
            size=0.4,
            location=(0, 0.65, 1.85)
        )
        bangs = bpy.context.active_object
        bangs.name = f"{self.character_name}_Bangs"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(1.8, 0.3, 0.6))
        bpy.ops.object.mode_set(mode='OBJECT')
        hair_parts.append(bangs)
        
        # サイドの髪
        for side, x_pos in [("L", -0.5), ("R", 0.5)]:
            bpy.ops.mesh.primitive_cube_add(
                size=0.5,
                location=(x_pos, 0, 1.3)
            )
            side_hair = bpy.context.active_object
            side_hair.name = f"{self.character_name}_SideHair_{side}"
            
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.transform.resize(value=(0.3, 0.4, 1.8))
            bpy.ops.object.mode_set(mode='OBJECT')
            hair_parts.append(side_hair)
        
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
        """スレンダーな体型"""
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=16,
            radius=0.32,
            depth=1.1,
            location=(0, 0, 0.7)
        )
        body = bpy.context.active_object
        body.name = f"{self.character_name}_Body"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(body.data)
        
        for vert in bm.verts:
            # スレンダーな体型
            if abs(vert.co.z) < 0.2:
                vert.co.x *= 0.8
                vert.co.y *= 0.8
            # 肩幅を狭く
            if vert.co.z > 0.4:
                vert.co.x *= 0.9
        
        bmesh.update_edit_mesh(body.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.shade_smooth()
        
        return body
        
    def create_uniform(self):
        """生徒会長の制服（ブレザー）"""
        uniform_parts = []
        
        # ジャケット
        bpy.ops.mesh.primitive_cube_add(
            size=0.6,
            location=(0, 0, 0.9)
        )
        jacket = bpy.context.active_object
        jacket.name = f"{self.character_name}_Jacket"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(0.9, 0.7, 1.0))
        bpy.ops.object.mode_set(mode='OBJECT')
        uniform_parts.append(jacket)
        
        # スカート（膝丈）
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=20,
            radius=0.35,
            depth=0.6,
            location=(0, 0, 0.2)
        )
        skirt = bpy.context.active_object
        skirt.name = f"{self.character_name}_Skirt"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(skirt.data)
        
        for vert in bm.verts:
            # スカートの形状
            if vert.co.z < 0:
                vert.co.x *= 1.2
                vert.co.y *= 1.2
        
        bmesh.update_edit_mesh(skirt.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        uniform_parts.append(skirt)
        
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
        """切れ長の目"""
        eyes = []
        
        for side, x_pos in [("L", -0.18), ("R", 0.18)]:
            # 目の球体
            bpy.ops.mesh.primitive_uv_sphere_add(
                segments=16,
                ring_count=8,
                radius=0.1,
                location=(x_pos, 0.65, 1.65)
            )
            eye = bpy.context.active_object
            eye.name = f"{self.character_name}_Eye_{side}"
            
            # 切れ長の目に変形
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.transform.resize(value=(1.3, 0.25, 0.9))
            bpy.ops.object.mode_set(mode='OBJECT')
            
            eyes.append(eye)
            
        return eyes
        
    def create_accessories(self):
        """生徒会長の腕章"""
        accessories = []
        
        # 腕章（簡略化）
        bpy.ops.mesh.primitive_cube_add(
            size=0.1,
            location=(-0.4, 0.1, 0.9)
        )
        armband = bpy.context.active_object
        armband.name = f"{self.character_name}_Armband"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(0.8, 0.3, 2.0))
        bpy.ops.object.mode_set(mode='OBJECT')
        
        accessories.append(armband)
        
        # メガネ（オプション）
        bpy.ops.mesh.primitive_torus_add(
            major_radius=0.2,
            minor_radius=0.01,
            location=(0, 0.7, 1.65)
        )
        glasses = bpy.context.active_object
        glasses.name = f"{self.character_name}_Glasses"
        
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.transform.resize(value=(1.0, 0.1, 0.8))
        bpy.ops.object.mode_set(mode='OBJECT')
        
        accessories.append(glasses)
        
        return accessories
        
    def add_materials(self):
        """雪乃用のマテリアル設定"""
        obj = bpy.context.active_object
        
        # 既存のマテリアルをクリア
        obj.data.materials.clear()
        
        # 肌マテリアル（色白）
        skin_mat = bpy.data.materials.new(name=f"{self.character_name}_Skin")
        skin_mat.use_nodes = True
        skin_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = self.skin_color
        skin_mat.node_tree.nodes["Principled BSDF"].inputs[7].default_value = 0.4
        
        # 髪マテリアル（黒髪）
        hair_mat = bpy.data.materials.new(name=f"{self.character_name}_Hair")
        hair_mat.use_nodes = True
        hair_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = self.hair_color
        hair_mat.node_tree.nodes["Principled BSDF"].inputs[7].default_value = 0.2
        hair_mat.node_tree.nodes["Principled BSDF"].inputs[4].default_value = 0.8  # Metallic
        
        # 制服マテリアル（紺色）
        uniform_mat = bpy.data.materials.new(name=f"{self.character_name}_Uniform")
        uniform_mat.use_nodes = True
        uniform_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.1, 0.1, 0.3, 1.0)
        
        # 目のマテリアル（青い瞳）
        eye_mat = bpy.data.materials.new(name=f"{self.character_name}_Eyes")
        eye_mat.use_nodes = True
        eye_mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = self.eye_color
        eye_mat.node_tree.nodes["Principled BSDF"].inputs[7].default_value = 0.1
        eye_mat.node_tree.nodes["Principled BSDF"].inputs[15].default_value = 1.45  # IOR
        
        obj.data.materials.append(skin_mat)
        obj.data.materials.append(hair_mat)
        obj.data.materials.append(uniform_mat)
        obj.data.materials.append(eye_mat)
        
    def create_character(self):
        """雪乃のキャラクターを作成"""
        print(f"Creating {self.character_name} (Student Council President)...")
        
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
        
        # 位置調整（少し背を高く）
        character.location = (0, 0, 0.05)
        
        print(f"{self.character_name} model created successfully!")
        return character

# 実行
if __name__ == "__main__":
    creator = YukinoCharacterCreator()
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
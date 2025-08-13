import bpy
import bmesh
from mathutils import Vector, Matrix
import math

"""
キャラクター用物理シミュレーション設定スクリプト
髪の毛、服、アクセサリーに物理演算を適用
"""

class PhysicsSimulationSetup:
    def __init__(self):
        self.character_mesh = None
        self.hair_objects = []
        self.cloth_objects = []
        
    def setup_hair_physics(self, hair_object):
        """髪の毛の物理シミュレーション設定"""
        if not hair_object or hair_object.type != 'MESH':
            print("エラー: 有効な髪メッシュを選択してください")
            return
            
        print(f"髪の物理設定: {hair_object.name}")
        
        # Cloth物理を追加
        bpy.context.view_layer.objects.active = hair_object
        bpy.ops.object.modifier_add(type='CLOTH')
        
        cloth_modifier = hair_object.modifiers["Cloth"]
        cloth_settings = cloth_modifier.settings
        
        # 髪の毛用の設定
        cloth_settings.quality = 10  # 高品質
        
        # マテリアル設定
        cloth_settings.mass = 0.05  # 軽い髪
        cloth_settings.tension_stiffness = 15  # 張力
        cloth_settings.compression_stiffness = 15  # 圧縮
        cloth_settings.shear_stiffness = 5  # せん断
        cloth_settings.bending_stiffness = 0.5  # 曲げ（髪の柔らかさ）
        
        # ダンピング（減衰）
        cloth_settings.tension_damping = 5
        cloth_settings.compression_damping = 5
        cloth_settings.shear_damping = 5
        cloth_settings.bending_damping = 0.5
        
        # 空気抵抗
        cloth_settings.air_damping = 1.0
        
        # 重力の影響
        cloth_settings.effector_weights.gravity = 0.5  # 髪は軽いので重力を弱める
        
        # ピン設定（髪の根元を固定）
        self.setup_hair_pinning(hair_object)
        
        # コリジョン設定
        cloth_modifier.collision_settings.collision_quality = 5
        cloth_modifier.collision_settings.distance_min = 0.001
        cloth_modifier.collision_settings.self_distance_min = 0.002
        
        # 髪用の頂点グループ作成
        self.create_hair_vertex_groups(hair_object)
        
    def create_hair_vertex_groups(self, hair_object):
        """髪の頂点グループを作成"""
        bpy.context.view_layer.objects.active = hair_object
        bpy.ops.object.mode_set(mode='EDIT')
        
        mesh = bmesh.from_edit_mesh(hair_object.data)
        mesh.verts.ensure_lookup_table()
        
        # 根元グループ（固定）
        root_group = hair_object.vertex_groups.new(name="Hair_Root")
        
        # 中間グループ（半固定）
        middle_group = hair_object.vertex_groups.new(name="Hair_Middle")
        
        # 先端グループ（自由）
        tip_group = hair_object.vertex_groups.new(name="Hair_Tip")
        
        # 高さに基づいて頂点をグループ分け
        min_z = min(v.co.z for v in mesh.verts)
        max_z = max(v.co.z for v in mesh.verts)
        height_range = max_z - min_z
        
        for vert in mesh.verts:
            relative_height = (vert.co.z - min_z) / height_range
            
            if relative_height > 0.8:  # 上部20%は根元
                root_group.add([vert.index], 1.0, 'ADD')
            elif relative_height > 0.4:  # 中間40%
                weight = (relative_height - 0.4) / 0.4
                middle_group.add([vert.index], weight, 'ADD')
            else:  # 下部40%は先端
                tip_group.add([vert.index], 1.0, 'ADD')
                
        bmesh.update_edit_mesh(hair_object.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        
    def setup_hair_pinning(self, hair_object):
        """髪のピン設定"""
        if "Hair_Root" in hair_object.vertex_groups:
            cloth_modifier = hair_object.modifiers.get("Cloth")
            if cloth_modifier:
                cloth_modifier.settings.vertex_group_mass = "Hair_Root"
                cloth_modifier.settings.pin_stiffness = 1.0
                
    def setup_cloth_physics(self, cloth_object):
        """服の物理シミュレーション設定"""
        if not cloth_object or cloth_object.type != 'MESH':
            print("エラー: 有効な服メッシュを選択してください")
            return
            
        print(f"服の物理設定: {cloth_object.name}")
        
        # Cloth物理を追加
        bpy.context.view_layer.objects.active = cloth_object
        bpy.ops.object.modifier_add(type='CLOTH')
        
        cloth_modifier = cloth_object.modifiers["Cloth"]
        cloth_settings = cloth_modifier.settings
        
        # 服用の設定
        cloth_settings.quality = 8
        
        # マテリアル設定（服の種類に応じて調整）
        cloth_type = self.detect_cloth_type(cloth_object.name.lower())
        
        if cloth_type == "silk":
            # シルク設定
            cloth_settings.mass = 0.1
            cloth_settings.tension_stiffness = 20
            cloth_settings.compression_stiffness = 20
            cloth_settings.shear_stiffness = 10
            cloth_settings.bending_stiffness = 0.05
            
        elif cloth_type == "cotton":
            # コットン設定
            cloth_settings.mass = 0.3
            cloth_settings.tension_stiffness = 40
            cloth_settings.compression_stiffness = 40
            cloth_settings.shear_stiffness = 20
            cloth_settings.bending_stiffness = 0.5
            
        elif cloth_type == "leather":
            # レザー設定
            cloth_settings.mass = 0.5
            cloth_settings.tension_stiffness = 80
            cloth_settings.compression_stiffness = 80
            cloth_settings.shear_stiffness = 40
            cloth_settings.bending_stiffness = 10
            
        else:
            # デフォルト設定
            cloth_settings.mass = 0.2
            cloth_settings.tension_stiffness = 30
            cloth_settings.compression_stiffness = 30
            cloth_settings.shear_stiffness = 15
            cloth_settings.bending_stiffness = 0.2
            
        # ダンピング
        cloth_settings.tension_damping = 10
        cloth_settings.compression_damping = 10
        cloth_settings.shear_damping = 10
        cloth_settings.bending_damping = 0.5
        
        # 空気抵抗
        cloth_settings.air_damping = 2.0
        
        # セルフコリジョン
        cloth_modifier.collision_settings.use_self_collision = True
        cloth_modifier.collision_settings.self_distance_min = 0.003
        
        # 服用の頂点グループ作成
        self.create_cloth_vertex_groups(cloth_object)
        
    def detect_cloth_type(self, name):
        """服の種類を名前から推測"""
        if any(keyword in name for keyword in ['silk', 'dress', 'skirt']):
            return "silk"
        elif any(keyword in name for keyword in ['shirt', 'cotton', 'tshirt']):
            return "cotton"
        elif any(keyword in name for keyword in ['leather', 'jacket', 'coat']):
            return "leather"
        else:
            return "default"
            
    def create_cloth_vertex_groups(self, cloth_object):
        """服の頂点グループを作成"""
        bpy.context.view_layer.objects.active = cloth_object
        bpy.ops.object.mode_set(mode='EDIT')
        
        mesh = bmesh.from_edit_mesh(cloth_object.data)
        mesh.verts.ensure_lookup_table()
        
        # 固定グループ（襟、ウエストなど）
        pinned_group = cloth_object.vertex_groups.new(name="Cloth_Pinned")
        
        # 自由グループ（裾など）
        free_group = cloth_object.vertex_groups.new(name="Cloth_Free")
        
        # 高さと位置に基づいてグループ分け
        for vert in mesh.verts:
            # 上部の頂点は固定
            if vert.co.z > 1.0:  # 肩や襟の高さ
                pinned_group.add([vert.index], 1.0, 'ADD')
            # 下部の頂点は自由
            elif vert.co.z < 0.5:  # 裾の高さ
                free_group.add([vert.index], 1.0, 'ADD')
                
        bmesh.update_edit_mesh(cloth_object.data)
        bpy.ops.object.mode_set(mode='OBJECT')
        
        # ピン設定
        cloth_modifier = cloth_object.modifiers.get("Cloth")
        if cloth_modifier and "Cloth_Pinned" in cloth_object.vertex_groups:
            cloth_modifier.settings.vertex_group_mass = "Cloth_Pinned"
            
    def setup_collision_object(self, body_object):
        """ボディをコリジョンオブジェクトとして設定"""
        if not body_object or body_object.type != 'MESH':
            print("エラー: 有効なボディメッシュを選択してください")
            return
            
        print(f"コリジョン設定: {body_object.name}")
        
        # Collision物理を追加
        bpy.context.view_layer.objects.active = body_object
        bpy.ops.object.modifier_add(type='COLLISION')
        
        collision_modifier = body_object.modifiers["Collision"]
        collision_settings = collision_modifier.settings
        
        # コリジョン設定
        collision_settings.thickness_outer = 0.02  # 外側の厚み
        collision_settings.thickness_inner = 0.001  # 内側の厚み
        collision_settings.damping = 0.5  # 減衰
        collision_settings.friction = 0.5  # 摩擦
        
    def setup_soft_body_physics(self, accessory_object):
        """アクセサリー用ソフトボディ設定"""
        if not accessory_object or accessory_object.type != 'MESH':
            print("エラー: 有効なアクセサリーメッシュを選択してください")
            return
            
        print(f"ソフトボディ設定: {accessory_object.name}")
        
        # Soft Body物理を追加
        bpy.context.view_layer.objects.active = accessory_object
        bpy.ops.object.modifier_add(type='SOFT_BODY')
        
        soft_body = accessory_object.modifiers["Softbody"]
        soft_settings = soft_body.settings
        
        # ソフトボディ設定
        soft_settings.mass = 0.1
        soft_settings.friction = 0.5
        soft_settings.speed = 0.5
        
        # ゴール設定（形状維持）
        soft_settings.use_goal = True
        soft_settings.goal_default = 0.7
        soft_settings.goal_spring = 0.5
        soft_settings.goal_friction = 0.5
        
        # エッジ設定
        soft_settings.use_edges = True
        soft_settings.pull = 0.9
        soft_settings.push = 0.9
        soft_settings.bend = 0.1
        soft_settings.aerodynamics_type = 'SIMPLE'
        soft_settings.aero = 0.5
        
    def setup_wind_force(self):
        """風力の設定"""
        # 風力フィールドを作成
        bpy.ops.object.effector_add(type='WIND')
        wind = bpy.context.active_object
        wind.name = "Wind_Force"
        
        # 風の設定
        wind.field.strength = 50  # 風力
        wind.field.flow = 0.5  # 流れ
        wind.field.noise = 0.3  # ノイズ
        wind.field.seed = 1  # ランダムシード
        
        # 風の方向と位置
        wind.location = (5, 0, 2)
        wind.rotation_euler = (0, math.radians(90), 0)
        
        return wind
        
    def setup_turbulence_force(self):
        """乱流の設定"""
        # 乱流フィールドを作成
        bpy.ops.object.effector_add(type='TURBULENCE')
        turbulence = bpy.context.active_object
        turbulence.name = "Turbulence_Force"
        
        # 乱流の設定
        turbulence.field.strength = 5
        turbulence.field.size = 2
        turbulence.field.flow = 1
        
        # 位置
        turbulence.location = (0, 0, 1)
        
        return turbulence
        
    def bake_physics_simulation(self, start_frame=1, end_frame=250):
        """物理シミュレーションのベイク"""
        print(f"物理シミュレーションをベイク中... ({start_frame}-{end_frame}フレーム)")
        
        # シーンのフレーム範囲設定
        scene = bpy.context.scene
        scene.frame_start = start_frame
        scene.frame_end = end_frame
        
        # すべてのClothモディファイアをベイク
        for obj in bpy.data.objects:
            if obj.type == 'MESH':
                for modifier in obj.modifiers:
                    if modifier.type == 'CLOTH':
                        # キャッシュをクリア
                        bpy.context.view_layer.objects.active = obj
                        bpy.ops.ptcache.free_bake(modifier=modifier.name)
                        
                        # ベイク実行
                        modifier.point_cache.frame_start = start_frame
                        modifier.point_cache.frame_end = end_frame
                        bpy.ops.ptcache.bake(modifier=modifier.name, bake=True)
                        
                    elif modifier.type == 'SOFT_BODY':
                        # ソフトボディのベイク
                        bpy.context.view_layer.objects.active = obj
                        bpy.ops.ptcache.free_bake(modifier=modifier.name)
                        bpy.ops.ptcache.bake(modifier=modifier.name, bake=True)
                        
        print("ベイク完了！")
        
    def optimize_simulation_settings(self):
        """シミュレーション設定の最適化"""
        print("シミュレーション設定を最適化中...")
        
        # ビューポート表示の最適化
        for obj in bpy.data.objects:
            if obj.type == 'MESH':
                for modifier in obj.modifiers:
                    if modifier.type in ['CLOTH', 'SOFT_BODY', 'COLLISION']:
                        # ビューポートでの表示をシンプル化
                        modifier.show_viewport = True
                        modifier.show_render = True
                        
        # シーン設定の最適化
        scene = bpy.context.scene
        scene.rigidbody_world.substeps_per_frame = 10
        scene.rigidbody_world.solver_iterations = 10
        
        print("最適化完了！")
        
    def create_physics_control_panel(self):
        """物理制御用のカスタムプロパティを作成"""
        # ダミーオブジェクトを作成
        bpy.ops.object.empty_add(type='PLAIN_AXES')
        control = bpy.context.active_object
        control.name = "Physics_Controller"
        
        # カスタムプロパティを追加
        control["wind_strength"] = 50.0
        control["wind_noise"] = 0.3
        control["turbulence_strength"] = 5.0
        control["gravity_influence"] = 1.0
        control["cloth_stiffness"] = 1.0
        control["hair_stiffness"] = 1.0
        
        # プロパティの範囲設定
        control.id_properties_ui("wind_strength").update(min=0, max=200)
        control.id_properties_ui("wind_noise").update(min=0, max=1)
        control.id_properties_ui("turbulence_strength").update(min=0, max=20)
        control.id_properties_ui("gravity_influence").update(min=0, max=2)
        control.id_properties_ui("cloth_stiffness").update(min=0, max=2)
        control.id_properties_ui("hair_stiffness").update(min=0, max=2)
        
        return control
        
    def setup_complete_physics(self, character_name="Character"):
        """キャラクター全体の物理設定"""
        print("="*50)
        print("キャラクター物理シミュレーション設定")
        print("="*50)
        
        # キャラクターのパーツを検索
        for obj in bpy.data.objects:
            if obj.type == 'MESH':
                name_lower = obj.name.lower()
                
                # ボディをコリジョンに設定
                if 'body' in name_lower or 'torso' in name_lower:
                    self.setup_collision_object(obj)
                    
                # 髪の物理設定
                elif 'hair' in name_lower:
                    self.setup_hair_physics(obj)
                    self.hair_objects.append(obj)
                    
                # 服の物理設定
                elif any(cloth in name_lower for cloth in ['cloth', 'shirt', 'skirt', 'dress', 'jacket']):
                    self.setup_cloth_physics(obj)
                    self.cloth_objects.append(obj)
                    
                # アクセサリーの物理設定
                elif any(acc in name_lower for acc in ['accessory', 'ribbon', 'tie', 'scarf']):
                    self.setup_soft_body_physics(obj)
                    
        # 風力と乱流を追加
        print("\n環境フォースを追加中...")
        self.setup_wind_force()
        self.setup_turbulence_force()
        
        # コントロールパネル作成
        print("\n物理コントロールパネルを作成中...")
        self.create_physics_control_panel()
        
        # 最適化
        print("\n設定を最適化中...")
        self.optimize_simulation_settings()
        
        print("\n="*50)
        print("✓ 物理シミュレーション設定完了！")
        print("="*50)
        print("\n次のステップ:")
        print("1. スペースキーで再生してシミュレーションを確認")
        print("2. Physics_Controllerで風力などを調整")
        print("3. bake_physics_simulation()でベイク")
        print("4. export_to_unity.pyでUnityにエクスポート")

# 実行
if __name__ == "__main__":
    physics_setup = PhysicsSimulationSetup()
    physics_setup.setup_complete_physics()
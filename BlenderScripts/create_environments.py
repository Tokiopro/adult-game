import bpy
import bmesh
from mathutils import Vector
import math

"""
ゲーム環境3Dアセット生成スクリプト
教室、カフェ、公園、ホテルルームなどを自動生成
"""

class EnvironmentCreator:
    def __init__(self):
        self.clean_scene()
        
    def clean_scene(self):
        """シーンをクリーンアップ"""
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete(use_global=False)
        
    def create_classroom(self):
        """教室環境を生成"""
        print("Creating classroom environment...")
        
        # 床
        bpy.ops.mesh.primitive_plane_add(size=10, location=(0, 0, 0))
        floor = bpy.context.active_object
        floor.name = "Classroom_Floor"
        
        # 壁
        self.create_walls(10, 10, 3)
        
        # 机と椅子
        for x in range(-3, 4, 2):
            for y in range(-3, 2, 2):
                self.create_desk_and_chair(x, y, 0.7)
                
        # 黒板
        self.create_blackboard()
        
        # 窓
        self.create_windows()
        
        # ドア
        self.create_door()
        
        print("Classroom created!")
        
    def create_cafe(self):
        """カフェ環境を生成"""
        print("Creating cafe environment...")
        
        # 床
        bpy.ops.mesh.primitive_plane_add(size=8, location=(0, 0, 0))
        floor = bpy.context.active_object
        floor.name = "Cafe_Floor"
        
        # 壁（おしゃれな感じ）
        self.create_walls(8, 8, 3, wall_type="cafe")
        
        # テーブルと椅子
        positions = [(-2, -2), (2, -2), (-2, 2), (2, 2), (0, 0)]
        for x, y in positions:
            self.create_cafe_table(x, y)
            
        # カウンター
        self.create_counter()
        
        # 装飾
        self.create_cafe_decorations()
        
        print("Cafe created!")
        
    def create_park(self):
        """公園環境を生成"""
        print("Creating park environment...")
        
        # 地面（草地）
        bpy.ops.mesh.primitive_plane_add(size=20, location=(0, 0, 0))
        ground = bpy.context.active_object
        ground.name = "Park_Ground"
        
        # ベンチ
        for i in range(3):
            x = (i - 1) * 5
            self.create_bench(x, 0, 0)
            
        # 木
        for i in range(5):
            x = (i - 2) * 4
            y = 5 if i % 2 == 0 else -5
            self.create_tree(x, y, 0)
            
        # 街灯
        self.create_street_lights()
        
        # 小道
        self.create_path()
        
        print("Park created!")
        
    def create_hotel_room(self):
        """ホテルルーム環境を生成"""
        print("Creating hotel room environment...")
        
        # 床
        bpy.ops.mesh.primitive_plane_add(size=6, location=(0, 0, 0))
        floor = bpy.context.active_object
        floor.name = "Hotel_Floor"
        
        # 壁
        self.create_walls(6, 6, 3, wall_type="hotel")
        
        # ベッド（重要）
        self.create_bed(0, 0, 0)
        
        # サイドテーブル
        self.create_side_table(-2, 0, 0)
        self.create_side_table(2, 0, 0)
        
        # ソファ
        self.create_sofa(-1, -2, 0)
        
        # 照明（ムード照明）
        self.create_mood_lighting()
        
        # バスルームドア
        self.create_bathroom_door()
        
        print("Hotel room created!")
        
    def create_walls(self, width, depth, height, wall_type="default"):
        """壁を生成"""
        # 前壁
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0, -depth/2, height/2))
        wall = bpy.context.active_object
        wall.scale = (width, 0.1, height)
        wall.name = f"{wall_type}_Wall_Front"
        
        # 後壁
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0, depth/2, height/2))
        wall = bpy.context.active_object
        wall.scale = (width, 0.1, height)
        wall.name = f"{wall_type}_Wall_Back"
        
        # 左壁
        bpy.ops.mesh.primitive_cube_add(size=1, location=(-width/2, 0, height/2))
        wall = bpy.context.active_object
        wall.scale = (0.1, depth, height)
        wall.name = f"{wall_type}_Wall_Left"
        
        # 右壁
        bpy.ops.mesh.primitive_cube_add(size=1, location=(width/2, 0, height/2))
        wall = bpy.context.active_object
        wall.scale = (0.1, depth, height)
        wall.name = f"{wall_type}_Wall_Right"
        
    def create_desk_and_chair(self, x, y, height):
        """机と椅子を生成"""
        # 机
        bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y, height))
        desk = bpy.context.active_object
        desk.scale = (0.6, 0.4, 0.05)
        desk.name = f"Desk_{x}_{y}"
        
        # 机の脚
        for dx, dy in [(-0.25, -0.15), (0.25, -0.15), (-0.25, 0.15), (0.25, 0.15)]:
            bpy.ops.mesh.primitive_cylinder_add(
                radius=0.02, depth=height,
                location=(x + dx, y + dy, height/2)
            )
            leg = bpy.context.active_object
            leg.name = f"DeskLeg_{x}_{y}"
            
        # 椅子
        bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y - 0.5, height/2))
        chair = bpy.context.active_object
        chair.scale = (0.4, 0.4, 0.05)
        chair.name = f"Chair_{x}_{y}"
        
    def create_blackboard(self):
        """黒板を生成"""
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 4.9, 2))
        board = bpy.context.active_object
        board.scale = (3, 0.05, 1.5)
        board.name = "Blackboard"
        
        # 黒板マテリアル
        mat = bpy.data.materials.new(name="Blackboard_Material")
        mat.use_nodes = True
        mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.1, 0.1, 0.1, 1)
        board.data.materials.append(mat)
        
    def create_windows(self):
        """窓を生成"""
        for x in [-3, 0, 3]:
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 4.95, 2))
            window = bpy.context.active_object
            window.scale = (0.8, 0.05, 1)
            window.name = f"Window_{x}"
            
            # ガラスマテリアル
            mat = bpy.data.materials.new(name=f"Glass_{x}")
            mat.use_nodes = True
            bsdf = mat.node_tree.nodes["Principled BSDF"]
            bsdf.inputs[15].default_value = 0  # Transmission
            bsdf.inputs[14].default_value = 1.45  # IOR
            window.data.materials.append(mat)
            
    def create_door(self):
        """ドアを生成"""
        bpy.ops.mesh.primitive_cube_add(size=1, location=(4.95, -2, 1))
        door = bpy.context.active_object
        door.scale = (0.05, 1, 2)
        door.name = "Door"
        
    def create_cafe_table(self, x, y):
        """カフェテーブルを生成"""
        # テーブル（丸型）
        bpy.ops.mesh.primitive_cylinder_add(
            radius=0.4, depth=0.05,
            location=(x, y, 0.7)
        )
        table = bpy.context.active_object
        table.name = f"CafeTable_{x}_{y}"
        
        # テーブル脚（1本）
        bpy.ops.mesh.primitive_cylinder_add(
            radius=0.05, depth=0.7,
            location=(x, y, 0.35)
        )
        leg = bpy.context.active_object
        leg.name = f"TableLeg_{x}_{y}"
        
        # 椅子（2脚）
        for dy in [-0.6, 0.6]:
            bpy.ops.mesh.primitive_cylinder_add(
                radius=0.2, depth=0.05,
                location=(x, y + dy, 0.4)
            )
            seat = bpy.context.active_object
            seat.name = f"CafeSeat_{x}_{y}"
            
    def create_counter(self):
        """カウンターを生成"""
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 3.5, 0.5))
        counter = bpy.context.active_object
        counter.scale = (4, 0.5, 1)
        counter.name = "Counter"
        
        # コーヒーマシン
        bpy.ops.mesh.primitive_cube_add(size=1, location=(1, 3.5, 1.2))
        machine = bpy.context.active_object
        machine.scale = (0.3, 0.3, 0.4)
        machine.name = "CoffeeMachine"
        
    def create_bench(self, x, y, z):
        """ベンチを生成"""
        # 座面
        bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y, z + 0.4))
        seat = bpy.context.active_object
        seat.scale = (2, 0.5, 0.05)
        seat.name = f"BenchSeat_{x}"
        
        # 背もたれ
        bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y - 0.2, z + 0.7))
        back = bpy.context.active_object
        back.scale = (2, 0.05, 0.5)
        back.name = f"BenchBack_{x}"
        
    def create_tree(self, x, y, z):
        """木を生成"""
        # 幹
        bpy.ops.mesh.primitive_cylinder_add(
            radius=0.2, depth=3,
            location=(x, y, z + 1.5)
        )
        trunk = bpy.context.active_object
        trunk.name = f"TreeTrunk_{x}_{y}"
        
        # 葉（球体）
        bpy.ops.mesh.primitive_uv_sphere_add(
            radius=1.5,
            location=(x, y, z + 3.5)
        )
        leaves = bpy.context.active_object
        leaves.name = f"TreeLeaves_{x}_{y}"
        
        # 葉のマテリアル
        mat = bpy.data.materials.new(name=f"Leaves_{x}_{y}")
        mat.use_nodes = True
        mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.2, 0.6, 0.2, 1)
        leaves.data.materials.append(mat)
        
    def create_bed(self, x, y, z):
        """ベッドを生成（重要な家具）"""
        # マットレス
        bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y, z + 0.3))
        mattress = bpy.context.active_object
        mattress.scale = (1, 2, 0.3)
        mattress.name = "Bed_Mattress"
        
        # 枕
        for dy in [0.8, -0.8]:
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y + 0.7, z + 0.5))
            pillow = bpy.context.active_object
            pillow.scale = (0.4, 0.3, 0.1)
            pillow.name = f"Pillow_{dy}"
            
        # ベッドフレーム
        bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y, z + 0.15))
        frame = bpy.context.active_object
        frame.scale = (1.1, 2.1, 0.15)
        frame.name = "Bed_Frame"
        
        # シーツマテリアル
        mat = bpy.data.materials.new(name="Bed_Sheets")
        mat.use_nodes = True
        mat.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.9, 0.9, 1.0, 1)
        mattress.data.materials.append(mat)
        
    def create_mood_lighting(self):
        """ムード照明を生成"""
        # 天井ライト（暖色）
        bpy.ops.object.light_add(type='POINT', location=(0, 0, 2.8))
        light = bpy.context.active_object
        light.data.energy = 50
        light.data.color = (1.0, 0.8, 0.6)
        light.name = "MoodLight_Ceiling"
        
        # ベッドサイドランプ
        for x in [-2, 2]:
            bpy.ops.object.light_add(type='POINT', location=(x, 0, 1))
            lamp = bpy.context.active_object
            lamp.data.energy = 20
            lamp.data.color = (1.0, 0.7, 0.5)
            lamp.name = f"BedsideLamp_{x}"
            
    def create_all_environments(self):
        """全環境を個別ファイルとして生成"""
        environments = [
            ("classroom", self.create_classroom),
            ("cafe", self.create_cafe),
            ("park", self.create_park),
            ("hotel_room", self.create_hotel_room)
        ]
        
        for name, create_func in environments:
            # シーンクリア
            self.clean_scene()
            
            # 環境生成
            create_func()
            
            # 保存
            filepath = f"BlenderAssets/Environments/{name}.blend"
            bpy.ops.wm.save_as_mainfile(filepath=filepath)
            print(f"Saved: {filepath}")
            
    def export_to_fbx(self, environment_name):
        """FBX形式でエクスポート"""
        export_path = f"UnityProject/Assets/Models/Environments/{environment_name}.fbx"
        
        bpy.ops.export_scene.fbx(
            filepath=export_path,
            use_selection=False,
            global_scale=1.0,
            apply_unit_scale=True,
            apply_scale_options='FBX_SCALE_ALL',
            axis_forward='-Z',
            axis_up='Y'
        )
        
        print(f"Exported to: {export_path}")

# 実行
if __name__ == "__main__":
    creator = EnvironmentCreator()
    
    # 個別に環境を作成する場合
    # creator.create_classroom()
    # creator.create_hotel_room()
    
    # 全環境を作成
    creator.create_all_environments()
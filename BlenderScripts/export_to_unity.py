import bpy
import os
from pathlib import Path

"""
BlenderモデルをUnity用にエクスポートするスクリプト
使用方法: Blenderでモデルを開いた状態で実行
"""

class UnityExporter:
    def __init__(self):
        self.export_path = Path(__file__).parent.parent / "UnityProject" / "Assets" / "Models" / "Characters"
        self.ensure_export_directory()
        
    def ensure_export_directory(self):
        """エクスポートディレクトリを確認/作成"""
        os.makedirs(self.export_path, exist_ok=True)
        
    def prepare_for_export(self):
        """エクスポート前の準備"""
        print("Preparing model for Unity export...")
        
        # すべてのオブジェクトを選択
        bpy.ops.object.select_all(action='SELECT')
        
        # トランスフォームを適用
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        
        # モディファイアを適用
        for obj in bpy.context.selected_objects:
            if obj.type == 'MESH':
                bpy.context.view_layer.objects.active = obj
                for modifier in obj.modifiers:
                    try:
                        bpy.ops.object.modifier_apply(modifier=modifier.name)
                    except:
                        print(f"Could not apply modifier: {modifier.name}")
                        
    def optimize_mesh(self):
        """メッシュを最適化"""
        print("Optimizing mesh...")
        
        for obj in bpy.context.selected_objects:
            if obj.type == 'MESH':
                bpy.context.view_layer.objects.active = obj
                bpy.ops.object.mode_set(mode='EDIT')
                
                # 重複頂点を削除
                bpy.ops.mesh.select_all(action='SELECT')
                bpy.ops.mesh.remove_doubles(threshold=0.001)
                
                # 法線を再計算
                bpy.ops.mesh.normals_make_consistent(inside=False)
                
                bpy.ops.object.mode_set(mode='OBJECT')
                
    def setup_unity_scale(self):
        """Unity用のスケール設定"""
        print("Setting Unity scale...")
        
        # Unityは1unit = 1mなので、適切にスケーリング
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.transform.resize(value=(100, 100, 100))
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        
    def export_fbx(self, filename="character"):
        """FBX形式でエクスポート"""
        export_file = self.export_path / f"{filename}.fbx"
        
        print(f"Exporting to: {export_file}")
        
        # FBXエクスポート設定
        bpy.ops.export_scene.fbx(
            filepath=str(export_file),
            check_existing=False,
            filter_glob="*.fbx",
            use_selection=True,
            use_active_collection=False,
            global_scale=1.0,
            apply_unit_scale=True,
            apply_scale_options='FBX_SCALE_ALL',
            bake_space_transform=False,
            object_types={'MESH', 'ARMATURE'},
            use_mesh_modifiers=True,
            use_mesh_modifiers_render=True,
            mesh_smooth_type='FACE',
            use_subsurf=False,
            use_mesh_edges=False,
            use_tspace=True,
            use_custom_props=False,
            add_leaf_bones=False,
            primary_bone_axis='Y',
            secondary_bone_axis='X',
            use_armature_deform_only=True,
            armature_nodetype='NULL',
            bake_anim=True,
            bake_anim_use_all_bones=True,
            bake_anim_use_nla_strips=True,
            bake_anim_use_all_actions=True,
            bake_anim_force_startend_keying=True,
            bake_anim_step=1.0,
            bake_anim_simplify_factor=1.0,
            path_mode='AUTO',
            embed_textures=True,
            batch_mode='OFF',
            use_batch_own_dir=True,
            axis_forward='-Z',
            axis_up='Y'
        )
        
        print(f"Export complete: {export_file}")
        
    def create_unity_meta_file(self, filename="character"):
        """Unity用のメタデータファイルを作成"""
        meta_content = """fileFormatVersion: 2
guid: AUTO_GENERATED
ModelImporter:
  serializedVersion: 21300
  internalIDToNameTable: []
  externalObjects: {}
  materials:
    materialImportMode: 2
    materialName: 0
    materialSearch: 1
    materialLocation: 1
  animations:
    legacyGenerateAnimations: 4
    bakeSimulation: 0
    resampleCurves: 1
    optimizeGameObjects: 0
    removeConstantScaleCurves: 0
    motionNodeName: 
    rigImportErrors: 
    rigImportWarnings: 
    animationImportErrors: 
    animationImportWarnings: 
    animationRetargetingWarnings: 
    animationDoRetargetingWarnings: 0
    importAnimatedCustomProperties: 0
    importConstraints: 0
    animationCompression: 1
    animationRotationError: 0.5
    animationPositionError: 0.5
    animationScaleError: 0.5
    animationWrapMode: 0
    extraExposedTransformPaths: []
    extraUserProperties: []
    clipAnimations: []
    isReadable: 0
  meshes:
    lODScreenPercentages: []
    globalScale: 1
    meshCompression: 0
    addColliders: 0
    useSRGBMaterialColor: 1
    sortHierarchyByName: 1
    importVisibility: 1
    importBlendShapes: 1
    importCameras: 1
    importLights: 1
    nodeNameCollisionStrategy: 1
    fileIdsGeneration: 2
    swapUVChannels: 0
    generateSecondaryUV: 0
    useFileUnits: 1
    keepQuads: 0
    weldVertices: 1
    bakeAxisConversion: 0
    preserveHierarchy: 0
    skinWeightsMode: 0
    maxBonesPerVertex: 4
    minBoneWeight: 0.001
    optimizeBones: 1
    meshOptimizationFlags: -1
    indexFormat: 0
    secondaryUVAngleDistortion: 8
    secondaryUVAreaDistortion: 15.000001
    secondaryUVHardAngle: 88
    secondaryUVMarginMethod: 1
    secondaryUVMinLightmapResolution: 40
    secondaryUVMinObjectScale: 1
    secondaryUVPackMargin: 4
    useFileScale: 1
  tangentSpace:
    normalSmoothAngle: 60
    normalImportMode: 0
    tangentImportMode: 3
    normalCalculationMode: 4
    legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes: 0
    blendShapeNormalImportMode: 1
    normalSmoothingSource: 0
  importAnimation: 1
  humanDescription:
    serializedVersion: 3
    human: []
    skeleton: []
    armTwist: 0.5
    foreArmTwist: 0.5
    upperLegTwist: 0.5
    legTwist: 0.5
    armStretch: 0.05
    legStretch: 0.05
    feetSpacing: 0
    globalScale: 1
    rootMotionBoneName: 
    hasTranslationDoF: 0
    hasExtraRoot: 0
    skeletonHasParents: 1
  lastHumanDescriptionAvatarSource: {instanceID: 0}
  autoGenerateAvatarMappingIfUnspecified: 1
  animationType: 2
  humanoidOversampling: 1
  avatarSetup: 0
  addHumanoidExtraRootOnlyWhenUsingAvatar: 1
  remapMaterialsIfMaterialImportModeIsNone: 0
  additionalBone: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
        
        meta_file = self.export_path / f"{filename}.fbx.meta"
        with open(meta_file, 'w') as f:
            f.write(meta_content)
            
        print(f"Meta file created: {meta_file}")
        
    def export_to_unity(self, filename="character"):
        """完全なUnityエクスポートプロセス"""
        print("="*50)
        print("Starting Unity Export Process")
        print("="*50)
        
        # 準備
        self.prepare_for_export()
        
        # 最適化
        self.optimize_mesh()
        
        # スケール設定
        self.setup_unity_scale()
        
        # FBXエクスポート
        self.export_fbx(filename)
        
        # メタファイル作成
        self.create_unity_meta_file(filename)
        
        print("="*50)
        print("Export Complete!")
        print(f"File exported to: {self.export_path}/{filename}.fbx")
        print("Import this file into Unity project")
        print("="*50)

# 実行
if __name__ == "__main__":
    exporter = UnityExporter()
    
    # ファイル名を取得（現在のBlendファイル名から）
    blend_filename = bpy.path.basename(bpy.context.blend_data.filepath)
    if blend_filename:
        export_name = blend_filename.replace(".blend", "")
    else:
        export_name = "character_model"
        
    exporter.export_to_unity(export_name)
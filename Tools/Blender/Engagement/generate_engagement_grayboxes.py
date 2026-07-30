import argparse
import math
import os
import sys

import bpy


LUMI_ACTIONS = [
    "IdleCalm", "IdleLook", "Enter", "Point", "Listen", "Wave",
    "Encourage", "CelebrateCalm", "Exit", "ReducedMotionIdle"
]

ARARA_ACTIONS = ["Idle", "Enter", "WingFlap", "ShortFlight", "Land", "ReducedMotionResponse"]


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials, bpy.data.actions):
        for block in list(datablocks):
            datablocks.remove(block)


def material(name, color):
    value = bpy.data.materials.new(name)
    value.diffuse_color = (*color, 1.0)
    value.use_nodes = False
    return value


def add_uv(name, location, scale, material_value, segments=12, rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material_value)
    return obj


def add_cube(name, location, scale, material_value, rotation=(0.0, 0.0, 0.0)):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material_value)
    return obj


def parent_all(root, objects):
    for obj in objects:
        obj.parent = root


def make_actions(root, names, flight=False):
    root.animation_data_create()
    for index, name in enumerate(names):
        action = bpy.data.actions.new(name=name)
        action.use_fake_user = True
        curve = action.fcurves.new(data_path="location", index=2)
        curve.keyframe_points.add(3)
        amplitude = 0.04 + (index % 3) * 0.02
        if flight and name == "ShortFlight":
            amplitude = 0.35
        curve.keyframe_points[0].co = (1.0, 0.0)
        curve.keyframe_points[1].co = (24.0, amplitude)
        curve.keyframe_points[2].co = (48.0, 0.0)
        for key in curve.keyframe_points:
            key.interpolation = "SINE"
    root.animation_data.action = bpy.data.actions.get(names[0])


def save_and_export(root, blend_path, fbx_path):
    os.makedirs(os.path.dirname(blend_path), exist_ok=True)
    os.makedirs(os.path.dirname(fbx_path), exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    pending = list(root.children)
    while pending:
        child = pending.pop()
        child.select_set(True)
        pending.extend(list(child.children))
    bpy.context.view_layer.objects.active = root
    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=True,
        apply_unit_scale=True,
        add_leaf_bones=False,
        bake_anim=True,
        bake_anim_use_all_actions=True,
        bake_anim_simplify_factor=1.0,
        object_types={"EMPTY", "MESH"},
        axis_forward="-Z",
        axis_up="Y",
    )


def create_lumi(blend_path, fbx_path):
    clear_scene()
    gold = material("Lumi_Gold", (0.95, 0.56, 0.12))
    dark = material("Lumi_Dark", (0.12, 0.08, 0.05))
    root = bpy.data.objects.new("PLACEHOLDER_LUMI_ROOT", None)
    bpy.context.collection.objects.link(root)
    parts = [
        add_uv("Lumi_Body", (0, 0, 0.75), (0.42, 0.30, 0.55), gold),
        add_uv("Lumi_Head", (0, 0, 1.48), (0.48, 0.42, 0.42), gold),
        add_uv("Lumi_Muzzle", (0, -0.36, 1.38), (0.26, 0.16, 0.16), gold),
        add_uv("Lumi_Ear_L", (-0.31, 0, 1.79), (0.16, 0.10, 0.17), gold, 10, 6),
        add_uv("Lumi_Ear_R", (0.31, 0, 1.79), (0.16, 0.10, 0.17), gold, 10, 6),
        add_uv("Lumi_Eye_L", (-0.16, -0.38, 1.54), (0.055, 0.035, 0.075), dark, 8, 6),
        add_uv("Lumi_Eye_R", (0.16, -0.38, 1.54), (0.055, 0.035, 0.075), dark, 8, 6),
        add_uv("Lumi_Nose", (0, -0.51, 1.39), (0.07, 0.04, 0.05), dark, 8, 6),
        add_cube("Lumi_Arm_L", (-0.46, 0, 0.83), (0.13, 0.15, 0.38), gold, (0, 0.2, -0.12)),
        add_cube("Lumi_Arm_R", (0.46, 0, 0.83), (0.13, 0.15, 0.38), gold, (0, -0.2, 0.12)),
        add_cube("Lumi_Leg_L", (-0.22, 0, 0.20), (0.16, 0.18, 0.30), gold),
        add_cube("Lumi_Leg_R", (0.22, 0, 0.20), (0.16, 0.18, 0.30), gold),
        add_cube("Lumi_Tail", (0.43, 0.15, 0.48), (0.09, 0.09, 0.55), gold, (0.35, 0.0, -0.45)),
    ]
    spot_positions = [(-0.24, -0.29, 0.9), (0.22, -0.29, 0.68), (0.0, -0.32, 1.0), (-0.20, -0.37, 1.63), (0.22, -0.37, 1.68)]
    for idx, position in enumerate(spot_positions):
        parts.append(add_uv("Lumi_Spot_%02d" % idx, position, (0.065, 0.025, 0.065), dark, 8, 6))
    parent_all(root, parts)
    make_actions(root, LUMI_ACTIONS)
    save_and_export(root, blend_path, fbx_path)


def create_arara(blend_path, fbx_path):
    clear_scene()
    blue = material("Arara_Blue", (0.05, 0.35, 0.82))
    warm = material("Arara_Warm", (0.92, 0.18, 0.12))
    root = bpy.data.objects.new("PLACEHOLDER_ARARA_ROOT", None)
    bpy.context.collection.objects.link(root)
    parts = [
        add_uv("Arara_Body", (0, 0, 0.65), (0.28, 0.25, 0.48), blue),
        add_uv("Arara_Head", (0, -0.03, 1.15), (0.30, 0.28, 0.30), warm),
        add_cube("Arara_Beak", (0, -0.35, 1.10), (0.11, 0.20, 0.10), warm, (0.25, 0, 0)),
        add_uv("Arara_Eye_L", (-0.13, -0.27, 1.22), (0.04, 0.025, 0.05), blue, 8, 6),
        add_uv("Arara_Eye_R", (0.13, -0.27, 1.22), (0.04, 0.025, 0.05), blue, 8, 6),
        add_cube("Arara_Wing_L", (-0.38, 0, 0.72), (0.32, 0.07, 0.43), blue, (0.0, -0.18, -0.18)),
        add_cube("Arara_Wing_R", (0.38, 0, 0.72), (0.32, 0.07, 0.43), blue, (0.0, 0.18, 0.18)),
        add_cube("Arara_Tail_L", (-0.10, 0.10, 0.05), (0.08, 0.08, 0.52), blue, (0.08, 0, -0.05)),
        add_cube("Arara_Tail_R", (0.10, 0.10, 0.05), (0.08, 0.08, 0.52), warm, (-0.08, 0, 0.05)),
        add_cube("Arara_Perch", (0, 0, -0.30), (0.48, 0.08, 0.07), warm),
    ]
    parent_all(root, parts)
    make_actions(root, ARARA_ACTIONS, flight=True)
    save_and_export(root, blend_path, fbx_path)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", required=True)
    script_args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    args = parser.parse_args(script_args)
    root = os.path.abspath(args.project_root)
    create_lumi(
        os.path.join(root, "SourceAssets", "Blender", "Characters", "Lumi", "PLACEHOLDER_Lumi.blend"),
        os.path.join(root, "Assets", "Models", "Characters", "Lumi", "PLACEHOLDER_Lumi.fbx"),
    )
    create_arara(
        os.path.join(root, "SourceAssets", "Blender", "Animals", "Arara", "PLACEHOLDER_Arara.blend"),
        os.path.join(root, "Assets", "Models", "Animals", "Arara", "PLACEHOLDER_Arara.fbx"),
    )
    print("ENGAGEMENT_GRAYBOXES_GENERATED")


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""Exporta letras Blender independentes para FBX binário compatível com Unity 2019.4."""

import argparse
import json
import os
import string
import sys
import time
import traceback

import bpy

EXPECTED_BLENDER = (2, 93)


def script_args():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input-dir", default="SourceAssets/Blender/Alphabet/blends")
    parser.add_argument("--output-dir", default="Assets/Models/Alphabet/FBX")
    parser.add_argument("--letters", default="A-Z")
    parser.add_argument("--overwrite", action="store_true")
    parser.add_argument("--report", default="Builds/Art/Alphabet/export-report.json")
    values = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    return parser.parse_args(values)


def resolve(value):
    return os.path.normpath(value if os.path.isabs(value) else os.path.join(os.getcwd(), value))


def parse_letters(raw):
    value = raw.strip().upper()
    if value == "A-Z":
        return list(string.ascii_uppercase)
    result = []
    for token in value.replace(";", ",").split(","):
        token = token.strip()
        if not token:
            continue
        if len(token) == 3 and token[1] == "-":
            result.extend(chr(code) for code in range(ord(token[0]), ord(token[2]) + 1))
        else:
            result.extend(token)
    if not result or any(letter not in string.ascii_uppercase for letter in result):
        raise ValueError("Seleção de letras inválida")
    if len(result) != len(set(result)):
        raise ValueError("Letras duplicadas")
    return result


def export_one(letter, source, destination, overwrite):
    started = time.perf_counter()
    expected_name = "RA_Letter_" + letter
    if not os.path.isfile(source):
        raise FileNotFoundError(source)
    if os.path.exists(destination) and not overwrite:
        raise FileExistsError("FBX já existe; use --overwrite: " + destination)

    bpy.ops.wm.open_mainfile(filepath=source, load_ui=False)
    objects = list(bpy.context.scene.objects)
    meshes = [obj for obj in objects if obj.type == "MESH"]
    if len(objects) != 1 or len(meshes) != 1:
        raise RuntimeError("Fonte deve conter exatamente um objeto mesh")
    obj = meshes[0]
    if obj.name != expected_name:
        raise RuntimeError("Nome incorreto: " + obj.name)
    if obj.modifiers:
        raise RuntimeError("Modificadores pendentes")
    if len(obj.data.materials) != 1 or obj.data.materials[0].name != "RA_Mat_" + letter:
        raise RuntimeError("Material incorreto")
    if any(abs(value - 1.0) > 0.00001 for value in obj.scale):
        raise RuntimeError("Escala não aplicada")
    if any(abs(value) > 0.00001 for value in obj.rotation_euler):
        raise RuntimeError("Rotação não aplicada")

    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    os.makedirs(os.path.dirname(destination), exist_ok=True)

    bpy.ops.export_scene.fbx(
        filepath=destination,
        check_existing=False,
        use_selection=True,
        use_active_collection=False,
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_NONE",
        bake_space_transform=False,
        object_types={"MESH"},
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        use_subsurf=False,
        use_mesh_edges=False,
        use_tspace=False,
        use_custom_props=False,
        add_leaf_bones=False,
        primary_bone_axis="Y",
        secondary_bone_axis="X",
        use_armature_deform_only=True,
        armature_nodetype="NULL",
        bake_anim=False,
        path_mode="AUTO",
        embed_textures=False,
        batch_mode="OFF",
        axis_forward="-Z",
        axis_up="Y",
    )
    size = os.path.getsize(destination) if os.path.isfile(destination) else 0
    if size <= 0:
        raise RuntimeError("FBX vazio ou ausente")
    return {
        "letter": letter,
        "source": source.replace("\\", "/"),
        "file": destination.replace("\\", "/"),
        "bytes": size,
        "objects": 1,
        "meshes": 1,
        "vertices": len(obj.data.vertices),
        "faces": len(obj.data.polygons),
        "material": obj.data.materials[0].name,
        "durationSeconds": round(time.perf_counter() - started, 4),
        "success": True,
        "error": "",
    }


def write_report(path, letters, results):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    payload = {
        "generatedAt": time.strftime("%Y-%m-%dT%H:%M:%S%z"),
        "blenderVersion": bpy.app.version_string,
        "requestedLetters": letters,
        "success": len(results) == len(letters) and all(item["success"] for item in results),
        "results": results,
    }
    temporary = path + ".tmp"
    with open(temporary, "w", encoding="utf-8") as stream:
        json.dump(payload, stream, ensure_ascii=False, indent=2)
        stream.write("\n")
    os.replace(temporary, path)


def main():
    if bpy.app.version[:2] != EXPECTED_BLENDER:
        raise RuntimeError("Blender 2.93 obrigatório; encontrado " + bpy.app.version_string)
    options = script_args()
    letters = parse_letters(options.letters)
    input_dir = resolve(options.input_dir)
    output_dir = resolve(options.output_dir)
    report = resolve(options.report)
    results = []

    for letter in letters:
        source = os.path.join(input_dir, "RA_Letter_{}.blend".format(letter))
        destination = os.path.join(output_dir, "RA_Letter_{}.fbx".format(letter))
        try:
            result = export_one(letter, source, destination, options.overwrite)
            results.append(result)
            print("[OK] {}: {} bytes".format(letter, result["bytes"]))
        except Exception as exc:
            error = "{}: {}".format(type(exc).__name__, exc)
            print("[ERRO] {}: {}".format(letter, error), file=sys.stderr)
            traceback.print_exc()
            results.append({
                "letter": letter,
                "source": source.replace("\\", "/"),
                "file": destination.replace("\\", "/"),
                "bytes": 0,
                "success": False,
                "error": error,
            })
            break

    write_report(report, letters, results)
    if len(results) != len(letters) or not all(item["success"] for item in results):
        raise SystemExit(1)


if __name__ == "__main__":
    main()


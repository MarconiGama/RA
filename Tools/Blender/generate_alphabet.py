#!/usr/bin/env python3
"""Gera letras 3D A-Z reproduzíveis no Blender 2.93 em modo background."""

import argparse
import json
import math
import os
import string
import sys
import time
import traceback

import bpy
import bmesh
from mathutils import Matrix, Vector

EXPECTED_BLENDER = (2, 93)
DEFAULT_OUTPUT = "SourceAssets/Blender/Alphabet/blends"
DEFAULT_REPORT = "Builds/Art/Alphabet/generation-report.json"
MAX_DIMENSION = 1.8
TARGET_DEPTH = 0.28
EXTRUDE = 0.16
BEVEL_DEPTH = 0.035
CURVE_RESOLUTION = 8
BEVEL_RESOLUTION = 4
PALETTE = [
    (0.12, 0.48, 0.92, 1.0),
    (0.16, 0.72, 0.38, 1.0),
    (1.00, 0.78, 0.10, 1.0),
    (1.00, 0.42, 0.10, 1.0),
    (0.92, 0.18, 0.20, 1.0),
    (0.55, 0.25, 0.82, 1.0),
    (0.95, 0.28, 0.58, 1.0),
    (0.08, 0.72, 0.78, 1.0),
]


def script_args():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", default=DEFAULT_OUTPUT)
    parser.add_argument("--letters", default="A-Z")
    parser.add_argument("--overwrite", action="store_true")
    parser.add_argument("--report", default=DEFAULT_REPORT)
    args = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    return parser.parse_args(args)


def resolve_path(value):
    if os.path.isabs(value):
        return os.path.normpath(value)
    return os.path.normpath(os.path.join(os.getcwd(), value))


def parse_letters(value):
    raw = value.strip().upper()
    if raw == "A-Z":
        return list(string.ascii_uppercase)
    values = []
    for token in raw.replace(";", ",").split(","):
        token = token.strip()
        if not token:
            continue
        if len(token) == 3 and token[1] == "-":
            start, end = ord(token[0]), ord(token[2])
            if start > end:
                raise ValueError("Intervalo de letras invertido: " + token)
            values.extend(chr(code) for code in range(start, end + 1))
        else:
            values.extend(list(token))
    invalid = [letter for letter in values if letter not in string.ascii_uppercase]
    if invalid:
        raise ValueError("Letras inválidas: " + ", ".join(invalid))
    if len(values) != len(set(values)):
        raise ValueError("Letras duplicadas na seleção")
    if not values:
        raise ValueError("Nenhuma letra selecionada")
    return values


def clean_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in (bpy.data.curves, bpy.data.meshes, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        for datablock in list(collection):
            collection.remove(datablock)


def load_font():
    candidates = [
        os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts", "ARLRDBD.TTF"),
        os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts", "arialbd.ttf"),
    ]
    for path in candidates:
        if os.path.isfile(path):
            return bpy.data.fonts.load(path, check_existing=True), path
    return bpy.data.fonts.get("Bfont"), "Bfont (Blender)"


def clean_mesh(mesh):
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.00001)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    loose = [vert for vert in bm.verts if not vert.link_edges and not vert.link_faces]
    if loose:
        bmesh.ops.delete(bm, geom=loose, context="VERTS")
    bm.to_mesh(mesh)
    bm.free()
    mesh.update()


def center_geometry_on_base(obj):
    vertices = obj.data.vertices
    if not vertices:
        raise RuntimeError("A malha não possui vértices")
    xs = [vertex.co.x for vertex in vertices]
    ys = [vertex.co.y for vertex in vertices]
    zs = [vertex.co.z for vertex in vertices]
    offset = Vector((-(min(xs) + max(xs)) / 2.0, -(min(ys) + max(ys)) / 2.0, -min(zs)))
    obj.data.transform(Matrix.Translation(offset))
    obj.data.update()
    obj.location = (0.0, 0.0, 0.0)


def create_material(letter, color):
    material = bpy.data.materials.new("RA_Mat_" + letter)
    material.diffuse_color = color
    material.use_nodes = True
    principled = material.node_tree.nodes.get("Principled BSDF")
    if principled:
        principled.inputs["Base Color"].default_value = color
        principled.inputs["Roughness"].default_value = 0.55
        principled.inputs["Metallic"].default_value = 0.0
    return material


def generate_letter(letter, output_file, overwrite):
    if os.path.exists(output_file) and not overwrite:
        raise FileExistsError("Arquivo já existe; use --overwrite: " + output_file)

    started = time.perf_counter()
    clean_scene()
    font, font_label = load_font()

    curve = bpy.data.curves.new("RA_Letter_{}_Curve".format(letter), "FONT")
    curve.body = letter
    curve.align_x = "CENTER"
    curve.align_y = "CENTER"
    curve.size = 1.0
    curve.extrude = EXTRUDE
    curve.bevel_depth = BEVEL_DEPTH
    curve.bevel_resolution = BEVEL_RESOLUTION
    curve.resolution_u = CURVE_RESOLUTION
    curve.fill_mode = "BOTH"
    if font:
        curve.font = font

    obj = bpy.data.objects.new("RA_Letter_" + letter, curve)
    bpy.context.scene.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.convert(target="MESH")
    obj = bpy.context.active_object
    obj.name = "RA_Letter_" + letter
    obj.data.name = "RA_Letter_{}_Mesh".format(letter)

    # Texto nasce no plano XY. A rotação o coloca em pé no plano XZ, com profundidade em Y.
    obj.rotation_euler[0] = math.radians(90.0)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    clean_mesh(obj.data)

    largest = max(obj.dimensions)
    if largest <= 0.0:
        raise RuntimeError("Dimensões inválidas")
    scale = MAX_DIMENSION / largest
    obj.scale = (scale, scale, scale)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if obj.dimensions.y <= 0.0:
        raise RuntimeError("Profundidade inválida")
    obj.scale.y = TARGET_DEPTH / obj.dimensions.y
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    center_geometry_on_base(obj)
    clean_mesh(obj.data)

    material = create_material(letter, PALETTE[(ord(letter) - ord("A")) % len(PALETTE)])
    obj.data.materials.clear()
    obj.data.materials.append(material)

    for polygon in obj.data.polygons:
        polygon.use_smooth = True

    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    if len(bpy.context.scene.objects) != 1:
        raise RuntimeError("A cena final deve conter exatamente um objeto")
    if len(obj.data.materials) != 1:
        raise RuntimeError("A letra deve conter exatamente um material")
    if any(abs(value - 1.0) > 0.00001 for value in obj.scale):
        raise RuntimeError("Escala não aplicada")
    if any(abs(value) > 0.00001 for value in obj.rotation_euler):
        raise RuntimeError("Rotação não aplicada")

    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=output_file, check_existing=False, compress=True)

    return {
        "letter": letter,
        "file": output_file.replace("\\", "/"),
        "font": font_label.replace("\\", "/"),
        "objects": 1,
        "meshes": 1,
        "vertices": len(obj.data.vertices),
        "faces": len(obj.data.polygons),
        "material": material.name,
        "dimensions": [round(value, 6) for value in obj.dimensions],
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
        "success": all(item["success"] for item in results) and len(results) == len(letters),
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
    args = script_args()
    letters = parse_letters(args.letters)
    output_dir = resolve_path(args.output_dir)
    report_path = resolve_path(args.report)
    results = []

    for letter in letters:
        output_file = os.path.join(output_dir, "RA_Letter_{}.blend".format(letter))
        try:
            result = generate_letter(letter, output_file, args.overwrite)
            print("[OK] {}: {} vértices, {} faces".format(letter, result["vertices"], result["faces"]))
            results.append(result)
        except Exception as exc:
            error = "{}: {}".format(type(exc).__name__, exc)
            print("[ERRO] {}: {}".format(letter, error), file=sys.stderr)
            traceback.print_exc()
            results.append({
                "letter": letter,
                "file": output_file.replace("\\", "/"),
                "objects": 0,
                "meshes": 0,
                "vertices": 0,
                "faces": 0,
                "material": "",
                "dimensions": [],
                "durationSeconds": 0.0,
                "success": False,
                "error": error,
            })
            break

    write_report(report_path, letters, results)
    if len(results) != len(letters) or not all(item["success"] for item in results):
        raise SystemExit(1)


if __name__ == "__main__":
    main()

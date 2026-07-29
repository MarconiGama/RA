#!/usr/bin/env python3
"""Inspeciona um .blend no Blender 2.93 e emite JSON sem alterá-lo."""

import argparse
import json
import os
import sys

import bpy
import bmesh


def args():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--file", required=True)
    parser.add_argument("--expected-name")
    parser.add_argument("--report")
    values = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    return parser.parse_args(values)


def inspect(path, expected_name):
    bpy.ops.wm.open_mainfile(filepath=path, load_ui=False)
    objects = list(bpy.context.scene.objects)
    meshes = [obj for obj in objects if obj.type == "MESH"]
    result = {
        "file": path.replace("\\", "/"),
        "objects": len(objects),
        "meshes": len(meshes),
        "cameras": sum(obj.type == "CAMERA" for obj in objects),
        "lights": sum(obj.type == "LIGHT" for obj in objects),
        "armatures": sum(obj.type == "ARMATURE" for obj in objects),
        "success": False,
        "errors": [],
    }
    if len(objects) != 1:
        result["errors"].append("Esperado exatamente um objeto")
    if len(meshes) != 1:
        result["errors"].append("Esperada exatamente uma mesh")
    if meshes:
        obj = meshes[0]
        bm = bmesh.new()
        bm.from_mesh(obj.data)
        loose = sum(not vert.link_edges and not vert.link_faces for vert in bm.verts)
        degenerate = sum(face.calc_area() <= 1e-10 for face in bm.faces)
        bm.free()
        result.update({
            "name": obj.name,
            "vertices": len(obj.data.vertices),
            "faces": len(obj.data.polygons),
            "materials": [slot.name for slot in obj.data.materials if slot],
            "dimensions": [round(value, 6) for value in obj.dimensions],
            "location": [round(value, 6) for value in obj.location],
            "rotation": [round(value, 6) for value in obj.rotation_euler],
            "scale": [round(value, 6) for value in obj.scale],
            "modifiers": len(obj.modifiers),
            "looseVertices": loose,
            "degenerateFaces": degenerate,
        })
        if expected_name and obj.name != expected_name:
            result["errors"].append("Nome incorreto: " + obj.name)
    if result["cameras"] or result["lights"] or result["armatures"]:
        result["errors"].append("Objetos proibidos encontrados")
    result["success"] = not result["errors"]
    return result


def main():
    options = args()
    path = os.path.abspath(options.file)
    if not os.path.isfile(path):
        raise FileNotFoundError(path)
    result = inspect(path, options.expected_name)
    serialized = json.dumps(result, ensure_ascii=False, indent=2)
    print(serialized)
    if options.report:
        report = os.path.abspath(options.report)
        os.makedirs(os.path.dirname(report), exist_ok=True)
        with open(report, "w", encoding="utf-8") as stream:
            stream.write(serialized + "\n")
    if not result["success"]:
        raise SystemExit(1)


if __name__ == "__main__":
    main()


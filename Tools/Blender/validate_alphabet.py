#!/usr/bin/env python3
"""Validação fail-closed das fontes Blender e dos FBX do alfabeto RA."""

import argparse
import json
import os
import string
import sys
import time

import bpy
import bmesh

MIN_VERTICES = 50
MAX_VERTICES = 20000
MAX_DIMENSION = 2.0
TOLERANCE = 0.0001


def script_args():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--blend-dir", default="SourceAssets/Blender/Alphabet/blends")
    parser.add_argument("--fbx-dir", default="Assets/Models/Alphabet/FBX")
    parser.add_argument("--letters", default="A-Z")
    parser.add_argument("--stage", choices=("blend", "fbx", "all"), default="all")
    parser.add_argument("--report-json", default="Builds/Art/Alphabet/validation-report.json")
    parser.add_argument("--report-md", default="docs/art-pipeline/VALIDATION_REPORT.md")
    values = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    return parser.parse_args(values)


def resolve(path):
    return os.path.normpath(path if os.path.isabs(path) else os.path.join(os.getcwd(), path))


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
        raise ValueError("Seleção inválida")
    if len(result) != len(set(result)):
        raise ValueError("Letras duplicadas")
    return result


def near(values, expected):
    return all(abs(value - expected) <= TOLERANCE for value in values)


def mesh_metrics(obj):
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bm.normal_update()
    loose = sum(not vertex.link_faces for vertex in bm.verts)
    degenerate = sum(face.calc_area() <= 1e-10 for face in bm.faces)
    non_manifold = sum(not edge.is_manifold for edge in bm.edges)
    volume = bm.calc_volume(signed=True)
    coordinates = [
        (round(vertex.co.x, 6), round(vertex.co.y, 6), round(vertex.co.z, 6))
        for vertex in bm.verts
    ]
    duplicates = len(coordinates) - len(set(coordinates))
    bm.free()
    return {
        "vertices": len(obj.data.vertices),
        "faces": len(obj.data.polygons),
        "looseVertices": loose,
        "degenerateFaces": degenerate,
        "nonManifoldEdges": non_manifold,
        "duplicateVertices": duplicates,
        "signedVolume": volume,
    }


def validate_scene(letter, source_kind):
    expected_name = "RA_Letter_" + letter
    expected_material = "RA_Mat_" + letter
    objects = list(bpy.context.scene.objects)
    meshes = [obj for obj in objects if obj.type == "MESH"]
    errors = []
    if len(objects) != 1:
        errors.append("objetos: esperado 1, encontrado {}".format(len(objects)))
    if len(meshes) != 1:
        errors.append("meshes: esperado 1, encontrado {}".format(len(meshes)))
    if any(obj.type == "CAMERA" for obj in objects):
        errors.append("câmera presente")
    if any(obj.type == "LIGHT" for obj in objects):
        errors.append("luz presente")
    if any(obj.type == "ARMATURE" for obj in objects):
        errors.append("armature presente")

    result = {
        "objects": len(objects),
        "meshes": len(meshes),
        "vertices": 0,
        "faces": 0,
        "material": "",
        "dimensions": [],
        "errors": errors,
    }
    if len(meshes) != 1:
        return result

    obj = meshes[0]
    metrics = mesh_metrics(obj)
    result.update(metrics)
    materials = [material.name for material in obj.data.materials if material]
    result["material"] = materials[0] if len(materials) == 1 else ", ".join(materials)
    result["dimensions"] = [round(value, 6) for value in obj.dimensions]

    if obj.name != expected_name:
        errors.append("nome incorreto: " + obj.name)
    if len(materials) != 1 or materials[0] != expected_material:
        errors.append("material incorreto")
    if obj.modifiers:
        errors.append("modificadores pendentes")
    if metrics["vertices"] < MIN_VERTICES or metrics["vertices"] > MAX_VERTICES:
        errors.append("vértices fora do intervalo")
    if metrics["faces"] <= 0:
        errors.append("malha sem faces")
    if max(obj.dimensions) > MAX_DIMENSION + TOLERANCE or min(obj.dimensions) <= 0:
        errors.append("dimensões fora do limite")
    if metrics["looseVertices"]:
        errors.append("vértices soltos")
    if metrics["degenerateFaces"]:
        errors.append("faces degeneradas")
    if metrics["duplicateVertices"]:
        errors.append("vértices duplicados")
    if metrics["nonManifoldEdges"]:
        errors.append("arestas não manifold")
    if metrics["signedVolume"] <= 0:
        errors.append("normais invertidas ou volume inválido")

    # O FBX reimportado pode receber conversão de eixos pelo importador Blender.
    if source_kind == "blend":
        if not near(obj.scale, 1.0):
            errors.append("escala não aplicada")
        if not near(obj.rotation_euler, 0.0):
            errors.append("rotação não aplicada")
        if not near(obj.location, 0.0):
            errors.append("localização não zerada")
        coordinates = [vertex.co for vertex in obj.data.vertices]
        min_z = min(vector.z for vector in coordinates)
        center_x = (min(vector.x for vector in coordinates) + max(vector.x for vector in coordinates)) / 2.0
        center_y = (min(vector.y for vector in coordinates) + max(vector.y for vector in coordinates)) / 2.0
        if abs(min_z) > TOLERANCE or abs(center_x) > TOLERANCE or abs(center_y) > TOLERANCE:
            errors.append("origem/base inválida")
    return result


def validate_blend(letter, path):
    if not os.path.isfile(path) or os.path.getsize(path) <= 0:
        return {"exists": os.path.isfile(path), "errors": ["arquivo .blend ausente ou vazio"]}
    bpy.ops.wm.open_mainfile(filepath=path, load_ui=False)
    result = validate_scene(letter, "blend")
    result["exists"] = True
    result["bytes"] = os.path.getsize(path)
    return result


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    # open_mainfile deixa datablocks do arquivo fonte carregados; removê-los evita
    # que o importador acrescente ".001" a nomes de mesh/material corretos no FBX.
    for collection in (bpy.data.meshes, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        for datablock in list(collection):
            collection.remove(datablock)


def validate_fbx(letter, path):
    if not os.path.isfile(path) or os.path.getsize(path) <= 0:
        return {"exists": os.path.isfile(path), "errors": ["arquivo FBX ausente ou vazio"]}
    clear_scene()
    bpy.ops.import_scene.fbx(filepath=path, use_custom_normals=True, use_image_search=False)
    result = validate_scene(letter, "fbx")
    result["exists"] = True
    result["bytes"] = os.path.getsize(path)
    return result


def write_json(path, payload):
    if not path:
        return
    os.makedirs(os.path.dirname(path), exist_ok=True)
    temporary = path + ".tmp"
    with open(temporary, "w", encoding="utf-8") as stream:
        json.dump(payload, stream, ensure_ascii=False, indent=2)
        stream.write("\n")
    os.replace(temporary, path)


def dimensions_text(values):
    return " × ".join("{:.3f}".format(value) for value in values) if values else "—"


def write_markdown(path, payload):
    if not path:
        return
    os.makedirs(os.path.dirname(path), exist_ok=True)
    lines = [
        "# Relatório de validação — alfabeto 3D",
        "",
        "- Gerado em: {}".format(payload["generatedAt"]),
        "- Blender: {}".format(payload["blenderVersion"]),
        "- Resultado: **{}**".format("APROVADO" if payload["success"] else "REPROVADO"),
        "- Aprovados: {}/{}".format(payload["passCount"], len(payload["results"])),
        "",
        "| Letra | Blend | FBX | Objetos | Vértices | Faces | Material | Dimensões | Status |",
        "|---|---:|---:|---:|---:|---:|---|---|---|",
    ]
    for item in payload["results"]:
        blend = item.get("blend", {})
        fbx = item.get("fbx", {})
        source = blend if blend.get("vertices") else fbx
        lines.append(
            "| {letter} | {blend_ok} | {fbx_ok} | {objects} | {vertices} | {faces} | {material} | {dimensions} | {status} |".format(
                letter=item["letter"],
                blend_ok="OK" if blend.get("exists") and not blend.get("errors") else "FALHA",
                fbx_ok="OK" if fbx.get("exists") and not fbx.get("errors") else "FALHA",
                objects=source.get("objects", 0),
                vertices=source.get("vertices", 0),
                faces=source.get("faces", 0),
                material=source.get("material", "—"),
                dimensions=dimensions_text(source.get("dimensions", [])),
                status="APROVADO" if item["success"] else "REPROVADO",
            )
        )
    failures = [item for item in payload["results"] if not item["success"]]
    if failures:
        lines.extend(["", "## Falhas", ""])
        for item in failures:
            messages = item.get("errors", [])
            lines.append("- {}: {}".format(item["letter"], "; ".join(messages)))
    lines.extend([
        "",
        "O sucesso integral exige o conjunto A–Z, uma única mesh e material por letra e zero falhas.",
        "",
    ])
    with open(path, "w", encoding="utf-8", newline="\n") as stream:
        stream.write("\n".join(lines))


def main():
    if bpy.app.version[:2] != (2, 93):
        raise RuntimeError("Blender 2.93 obrigatório")
    options = script_args()
    letters = parse_letters(options.letters)
    blend_dir = resolve(options.blend_dir)
    fbx_dir = resolve(options.fbx_dir)
    results = []

    for letter in letters:
        item = {"letter": letter, "errors": []}
        if options.stage in ("blend", "all"):
            blend = validate_blend(letter, os.path.join(blend_dir, "RA_Letter_{}.blend".format(letter)))
            item["blend"] = blend
            item["errors"].extend("Blend: " + error for error in blend["errors"])
        if options.stage in ("fbx", "all"):
            fbx = validate_fbx(letter, os.path.join(fbx_dir, "RA_Letter_{}.fbx".format(letter)))
            item["fbx"] = fbx
            item["errors"].extend("FBX: " + error for error in fbx["errors"])
        item["success"] = not item["errors"]
        results.append(item)
        print("[{}] {}".format("OK" if item["success"] else "FALHA", letter))

    full_alphabet = letters == list(string.ascii_uppercase)
    pass_count = sum(item["success"] for item in results)
    payload = {
        "generatedAt": time.strftime("%Y-%m-%dT%H:%M:%S%z"),
        "blenderVersion": bpy.app.version_string,
        "stage": options.stage,
        "fullAlphabet": full_alphabet,
        "success": pass_count == len(results) and (full_alphabet if options.stage == "all" else True),
        "passCount": pass_count,
        "failCount": len(results) - pass_count,
        "results": results,
    }
    write_json(resolve(options.report_json), payload)
    if options.report_md and options.stage == "all":
        write_markdown(resolve(options.report_md), payload)
    if not payload["success"]:
        raise SystemExit(1)


if __name__ == "__main__":
    main()

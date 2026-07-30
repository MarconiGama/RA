#!/usr/bin/env python3
"""Build printable A-Z target catalogs from the preserved Vuforia textures."""

from __future__ import annotations

import argparse
import hashlib
import json
import string
import sys
from dataclasses import asdict, dataclass
from pathlib import Path
from xml.etree import ElementTree as ET

from PIL import Image, ImageDraw, ImageFilter, ImageFont, ImageStat
import reportlab
from reportlab.lib.colors import HexColor
from reportlab.lib.pagesizes import A4
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.pdfgen import canvas


LETTERS = tuple(string.ascii_uppercase)
ASPECT_TOLERANCE_PERCENT = 0.15
TWO_UP_WIDTH_MM = 165.0
MIN_MARGIN_MM = 12.0
TWO_UP_GAP_MM = 18.0
WARNING = "Imprimir em tamanho real / escala 100%. Não usar Ajustar à página."
REPORTLAB_FONTS = Path(reportlab.__file__).resolve().parent / "fonts"
NORMAL_FONT = "TargetCatalogSans"
BOLD_FONT = "TargetCatalogSansBold"
pdfmetrics.registerFont(TTFont(NORMAL_FONT, str(REPORTLAB_FONTS / "Vera.ttf")))
pdfmetrics.registerFont(TTFont(BOLD_FONT, str(REPORTLAB_FONTS / "VeraBd.ttf")))


@dataclass(frozen=True)
class Target:
    letter: str
    path: Path
    width: int
    height: int
    pixel_aspect: float
    xml_width: float
    xml_height: float
    xml_aspect: float
    aspect_delta_percent: float
    byte_count: int
    sha256: str
    luma_mean: float
    luma_stddev: float
    luma_min: int
    luma_max: int
    entropy: float
    edge_mean: float
    low_information: bool


def repo_root() -> Path:
    return Path(__file__).resolve().parents[2]


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def load_inventory(root: Path) -> dict:
    path = root / "docs/target-catalog/target-inventory.json"
    if not path.is_file():
        raise RuntimeError(f"Inventário ausente: {path}")
    return json.loads(path.read_text(encoding="utf-8"))


def load_xml_targets(xml_path: Path) -> dict[str, tuple[float, float]]:
    if not xml_path.is_file():
        raise RuntimeError(f"XML canônico ausente: {xml_path}")
    parsed = ET.parse(xml_path)
    items: list[tuple[str, tuple[float, float]]] = []
    for element in parsed.getroot().iter():
        if element.tag.split("}")[-1] != "ImageTarget":
            continue
        name = element.attrib.get("name", "")
        raw_size = element.attrib.get("size", "").split()
        if len(raw_size) != 2:
            raise RuntimeError(f"Dimensão XML inválida para {name!r}")
        items.append((name, (float(raw_size[0]), float(raw_size[1]))))
    names = [name for name, _ in items]
    if len(items) != 26 or len(set(names)) != 26 or set(names) != set(LETTERS):
        raise RuntimeError(
            "O XML deve conter exatamente 26 targets únicos formando A-Z; "
            f"recebido: {names}"
        )
    if any(width <= 0 or height <= 0 for _, (width, height) in items):
        raise RuntimeError("O XML contém dimensão não positiva")
    return dict(items)


def validate_sources(root: Path) -> list[Target]:
    inventory = load_inventory(root)
    xml_path = root / "Assets/StreamingAssets/Vuforia/Alfabeto.xml"
    dat_path = root / "Assets/StreamingAssets/Vuforia/Alfabeto.dat"
    if not dat_path.is_file():
        raise RuntimeError(f"DAT canônico ausente: {dat_path}")

    source_db = inventory["sourceDatabase"]
    if sha256(xml_path) != source_db["xmlSha256"]:
        raise RuntimeError("SHA-256 do Alfabeto.xml divergiu do inventário")
    if sha256(dat_path) != source_db["datSha256"]:
        raise RuntimeError("SHA-256 do Alfabeto.dat divergiu do inventário")

    xml_targets = load_xml_targets(xml_path)
    expected = {item["letter"]: item for item in inventory["targets"]}
    source_dir = root / "Assets/Editor/Vuforia/ImageTargetTextures/Alfabeto"
    actual_jpegs = sorted(source_dir.glob("*_scaled.jpg"))
    expected_paths = [source_dir / f"{letter}_scaled.jpg" for letter in LETTERS]
    if actual_jpegs != expected_paths:
        actual_names = [path.name for path in actual_jpegs]
        raise RuntimeError(
            "O diretório deve conter exatamente A_scaled.jpg até Z_scaled.jpg; "
            f"recebido: {actual_names}"
        )

    targets: list[Target] = []
    seen_hashes: dict[str, str] = {}
    for letter, path in zip(LETTERS, expected_paths):
        record = expected.get(letter)
        if record is None or record.get("targetName") != letter:
            raise RuntimeError(f"Registro de inventário inválido para {letter}")
        current_hash = sha256(path)
        if current_hash != record["sha256"]:
            raise RuntimeError(f"SHA-256 divergente: {path}")
        if current_hash in seen_hashes:
            raise RuntimeError(
                f"Conteúdo duplicado: {seen_hashes[current_hash]} e {letter}"
            )
        seen_hashes[current_hash] = letter

        try:
            with Image.open(path) as image:
                if image.format != "JPEG":
                    raise RuntimeError(f"Formato não JPEG: {path}")
                image.verify()
            with Image.open(path) as image:
                image.load()
                width, height = image.size
                if width <= 0 or height <= 0:
                    raise RuntimeError(f"Dimensão não positiva: {path}")
                if (width, height) != (record["width"], record["height"]):
                    raise RuntimeError(f"Dimensão divergente: {path}")
                gray = image.convert("L")
                stats = ImageStat.Stat(gray)
                luma_min, luma_max = gray.getextrema()
                entropy = float(gray.entropy())
                edge_mean = float(ImageStat.Stat(gray.filter(ImageFilter.FIND_EDGES)).mean[0])
        except (OSError, SyntaxError) as error:
            raise RuntimeError(f"JPEG inválido: {path}: {error}") from error

        luma_mean = float(stats.mean[0])
        luma_stddev = float(stats.stddev[0])
        if luma_min == luma_max == 255:
            raise RuntimeError(f"Imagem completamente branca: {path}")
        if luma_min == luma_max == 0:
            raise RuntimeError(f"Imagem completamente preta: {path}")

        xml_width, xml_height = xml_targets[letter]
        pixel_aspect = width / height
        xml_aspect = xml_width / xml_height
        delta = abs(pixel_aspect - xml_aspect) / xml_aspect * 100.0
        if delta > ASPECT_TOLERANCE_PERCENT:
            raise RuntimeError(
                f"Proporção {letter} diverge {delta:.6f}% do XML "
                f"(limite {ASPECT_TOLERANCE_PERCENT:.2f}%)"
            )
        low_information = luma_stddev < 20.0 or entropy < 4.0 or edge_mean < 3.0
        targets.append(
            Target(
                letter=letter,
                path=path,
                width=width,
                height=height,
                pixel_aspect=pixel_aspect,
                xml_width=xml_width,
                xml_height=xml_height,
                xml_aspect=xml_aspect,
                aspect_delta_percent=delta,
                byte_count=path.stat().st_size,
                sha256=current_hash,
                luma_mean=luma_mean,
                luma_stddev=luma_stddev,
                luma_min=int(luma_min),
                luma_max=int(luma_max),
                entropy=entropy,
                edge_mean=edge_mean,
                low_information=low_information,
            )
        )
    return targets


def new_canvas(path: Path) -> canvas.Canvas:
    result = canvas.Canvas(
        str(path),
        pagesize=A4,
        pageCompression=1,
        invariant=1,
    )
    result.setTitle(path.stem)
    result.setAuthor("Projeto RealidadeA")
    result.setSubject("Catálogo recuperado de targets Vuforia A-Z")
    return result


def draw_crop_marks(
    pdf: canvas.Canvas, x: float, y: float, width: float, height: float
) -> None:
    offset = 1.0 * mm
    length = 4.0 * mm
    pdf.saveState()
    pdf.setStrokeColor(HexColor("#666666"))
    pdf.setLineWidth(0.25)
    for corner_x, direction in ((x, -1), (x + width, 1)):
        pdf.line(
            corner_x + direction * offset,
            y,
            corner_x + direction * length,
            y,
        )
        pdf.line(
            corner_x + direction * offset,
            y + height,
            corner_x + direction * length,
            y + height,
        )
    for corner_y, direction in ((y, -1), (y + height, 1)):
        pdf.line(
            x,
            corner_y + direction * offset,
            x,
            corner_y + direction * length,
        )
        pdf.line(
            x + width,
            corner_y + direction * offset,
            x + width,
            corner_y + direction * length,
        )
    pdf.restoreState()


def draw_identification(
    pdf: canvas.Canvas, target: Target, x: float, baseline: float, image_width: float
) -> None:
    pdf.saveState()
    pdf.setFillColor(HexColor("#111111"))
    pdf.setFont(BOLD_FONT, 11)
    pdf.drawString(x, baseline, f"Target {target.letter}")
    printed_height_mm = image_width / target.pixel_aspect / mm
    detail = (
        f"Imagem: {image_width / mm:.1f} x {printed_height_mm:.1f} mm  |  "
        f"XML: {target.xml_width:.6f} x {target.xml_height:.6f}"
    )
    pdf.setFillColor(HexColor("#444444"))
    pdf.setFont(NORMAL_FONT, 6.5)
    pdf.drawRightString(x + image_width, baseline, detail)
    pdf.restoreState()


def draw_footer(pdf: canvas.Canvas, page_number: int, page_total: int) -> None:
    page_width, _ = A4
    pdf.saveState()
    pdf.setFillColor(HexColor("#222222"))
    pdf.setFont(BOLD_FONT, 7.2)
    pdf.drawString(MIN_MARGIN_MM * mm, 7.5 * mm, WARNING)
    pdf.setFont(NORMAL_FONT, 7.2)
    pdf.drawRightString(
        page_width - MIN_MARGIN_MM * mm,
        7.5 * mm,
        f"Página {page_number}/{page_total}",
    )
    pdf.restoreState()


def build_two_up(targets: list[Target], output: Path) -> None:
    pdf = new_canvas(output)
    page_width, page_height = A4
    image_width = TWO_UP_WIDTH_MM * mm
    x = (page_width - image_width) / 2
    for page_index in range(13):
        pair = targets[page_index * 2 : page_index * 2 + 2]
        heights = [image_width / target.pixel_aspect for target in pair]
        group_height = sum(heights) + TWO_UP_GAP_MM * mm
        bottom = (page_height - group_height) / 2
        positions = [
            bottom + heights[1] + TWO_UP_GAP_MM * mm,
            bottom,
        ]
        for target, image_height, y in zip(pair, heights, positions):
            pdf.drawImage(
                str(target.path),
                x,
                y,
                width=image_width,
                height=image_height,
                preserveAspectRatio=True,
                anchor="c",
            )
            draw_crop_marks(pdf, x, y, image_width, image_height)
            draw_identification(pdf, target, x, y - 5.0 * mm, image_width)
        draw_footer(pdf, page_index + 1, 13)
        pdf.showPage()
    pdf.save()


def build_one_up(targets: list[Target], output: Path) -> None:
    pdf = new_canvas(output)
    page_width, page_height = A4
    image_width = (210.0 - 2 * MIN_MARGIN_MM) * mm
    for page_index, target in enumerate(targets, start=1):
        image_height = image_width / target.pixel_aspect
        x = (page_width - image_width) / 2
        y = (page_height - image_height) / 2
        pdf.drawImage(
            str(target.path),
            x,
            y,
            width=image_width,
            height=image_height,
            preserveAspectRatio=True,
            anchor="c",
        )
        draw_crop_marks(pdf, x, y, image_width, image_height)
        draw_identification(pdf, target, x, y - 7.0 * mm, image_width)
        draw_footer(pdf, page_index, 26)
        pdf.showPage()
    pdf.save()


def build_contact_pdf(targets: list[Target], output: Path) -> None:
    pdf = new_canvas(output)
    page_width, page_height = A4
    margin = 12.0 * mm
    columns, rows = 4, 7
    header_height = 22.0 * mm
    footer_height = 13.0 * mm
    grid_width = page_width - 2 * margin
    grid_height = page_height - 2 * margin - header_height - footer_height
    cell_width = grid_width / columns
    cell_height = grid_height / rows

    pdf.setFillColor(HexColor("#111111"))
    pdf.setFont(BOLD_FONT, 16)
    pdf.drawString(margin, page_height - margin - 5.0 * mm, "Contato A-Z - conferência visual")
    pdf.setFillColor(HexColor("#555555"))
    pdf.setFont(NORMAL_FONT, 8)
    pdf.drawString(
        margin,
        page_height - margin - 11.0 * mm,
        "Miniaturas para QA. Esta versão não é adequada para reconhecimento/tracking.",
    )

    grid_top = page_height - margin - header_height
    for index, target in enumerate(targets):
        row, column = divmod(index, columns)
        cell_x = margin + column * cell_width
        cell_top = grid_top - row * cell_height
        image_max_width = cell_width - 4.0 * mm
        image_max_height = cell_height - 8.0 * mm
        scale = min(
            image_max_width / target.width,
            image_max_height / target.height,
        )
        image_width = target.width * scale
        image_height = target.height * scale
        x = cell_x + (cell_width - image_width) / 2
        y = cell_top - 5.0 * mm - image_height
        pdf.setFillColor(HexColor("#111111"))
        pdf.setFont(BOLD_FONT, 8)
        pdf.drawCentredString(cell_x + cell_width / 2, cell_top - 3.0 * mm, target.letter)
        pdf.drawImage(
            str(target.path),
            x,
            y,
            width=image_width,
            height=image_height,
            preserveAspectRatio=True,
            anchor="c",
        )

    pdf.setFillColor(HexColor("#333333"))
    pdf.setFont(BOLD_FONT, 7.5)
    pdf.drawString(margin, 8.0 * mm, "NÃO USAR ESTA PÁGINA PARA TRACKING.")
    pdf.setFont(NORMAL_FONT, 7.5)
    pdf.drawRightString(page_width - margin, 8.0 * mm, "Página 1/1")
    pdf.showPage()
    pdf.save()


def build_contact_sheet_png(targets: list[Target], output: Path) -> None:
    columns, rows = 4, 7
    cell_width, cell_height = 300, 220
    header_height = 80
    sheet = Image.new(
        "RGB",
        (columns * cell_width, header_height + rows * cell_height),
        "#F2F3F5",
    )
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.truetype(str(REPORTLAB_FONTS / "VeraBd.ttf"), 20)
    small_font = ImageFont.truetype(str(REPORTLAB_FONTS / "Vera.ttf"), 14)
    draw.text((24, 16), "Targets A-Z - contact sheet de validação", fill="#111111", font=font)
    draw.text(
        (24, 46),
        "Somente QA visual; não usar para tracking.",
        fill="#555555",
        font=small_font,
    )
    for index, target in enumerate(targets):
        row, column = divmod(index, columns)
        left = column * cell_width
        top = header_height + row * cell_height
        draw.text(
            (left + 14, top + 10),
            target.letter,
            fill="#111111",
            font=font,
        )
        with Image.open(target.path) as image:
            preview = image.convert("RGB")
            preview.thumbnail((cell_width - 28, cell_height - 52), Image.Resampling.LANCZOS)
            x = left + (cell_width - preview.width) // 2
            y = top + 44 + (cell_height - 52 - preview.height) // 2
            sheet.paste(preview, (x, y))
    sheet.save(output, format="PNG", optimize=True)


def write_validation_results(targets: list[Target], output: Path) -> None:
    payload = {
        "schemaVersion": 1,
        "status": "PASS",
        "targetCount": len(targets),
        "aspectTolerancePercent": ASPECT_TOLERANCE_PERCENT,
        "lowInformationTargets": [
            target.letter for target in targets if target.low_information
        ],
        "targets": [
            {
                **{
                    key: value
                    for key, value in asdict(target).items()
                    if key != "path"
                },
                "path": target.path.as_posix(),
            }
            for target in targets
        ],
    }
    output.write_text(
        json.dumps(payload, indent=2, ensure_ascii=False) + "\n",
        encoding="utf-8",
    )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Valida os targets preservados e gera o catálogo físico A-Z."
    )
    parser.add_argument(
        "--output-dir",
        type=Path,
        help="Diretório de saída (padrão: Builds/TargetCatalog no repositório).",
    )
    parser.add_argument(
        "--validate-only",
        action="store_true",
        help="Valida fontes e hashes sem gerar arquivos.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    root = repo_root()
    targets = validate_sources(root)
    print(f"VALIDATION_STATUS=PASS TARGETS={len(targets)}")
    print(f"TOTAL_IMAGE_BYTES={sum(target.byte_count for target in targets)}")
    low_information = [target.letter for target in targets if target.low_information]
    print(
        "LOW_INFORMATION_TARGETS="
        + (",".join(low_information) if low_information else "NONE")
    )
    if args.validate_only:
        return 0

    output_dir = (args.output_dir or root / "Builds/TargetCatalog").resolve()
    output_dir.mkdir(parents=True, exist_ok=True)
    two_up = output_dir / "Catalogo_Alfabeto_RA_A4_2_por_pagina.pdf"
    one_up = output_dir / "Catalogo_Alfabeto_RA_A4_1_por_pagina.pdf"
    contact_pdf = output_dir / "Catalogo_Alfabeto_RA_Contato.pdf"
    contact_png = output_dir / "target-contact-sheet.png"
    validation_json = output_dir / "validation-results.json"

    build_two_up(targets, two_up)
    build_one_up(targets, one_up)
    build_contact_pdf(targets, contact_pdf)
    build_contact_sheet_png(targets, contact_png)
    write_validation_results(targets, validation_json)
    for artifact in (two_up, one_up, contact_pdf, contact_png, validation_json):
        print(f"GENERATED={artifact}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as error:
        print(f"ERROR={error}", file=sys.stderr)
        raise SystemExit(1)

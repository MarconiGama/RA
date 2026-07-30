#!/usr/bin/env python3
"""Regression tests for the recovered A-Z target catalog."""

from __future__ import annotations

import base64
import hashlib
import importlib.util
import string
import sys
import tempfile
import unittest
from pathlib import Path

from PIL import Image
from pypdf import PdfReader
from reportlab.lib.pagesizes import A4
from reportlab.lib.units import mm


SCRIPT = Path(__file__).with_name("build-target-catalog.py")
SPEC = importlib.util.spec_from_file_location("target_catalog_builder", SCRIPT)
if SPEC is None or SPEC.loader is None:
    raise RuntimeError(f"Não foi possível importar {SCRIPT}")
builder = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = builder
SPEC.loader.exec_module(builder)


def file_hash(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


class TargetCatalogTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.root = builder.repo_root()
        cls.inventory = builder.load_inventory(cls.root)
        cls.targets = builder.validate_sources(cls.root)

    def test_exact_complete_alphabet_and_unique_hashes(self) -> None:
        self.assertEqual(26, len(self.targets))
        self.assertEqual(string.ascii_uppercase, "".join(t.letter for t in self.targets))
        self.assertEqual(26, len({t.sha256 for t in self.targets}))
        self.assertEqual(2_286_348, sum(t.byte_count for t in self.targets))

    def test_all_sources_are_valid_nonuniform_jpegs(self) -> None:
        for target in self.targets:
            with self.subTest(letter=target.letter):
                with Image.open(target.path) as image:
                    self.assertEqual("JPEG", image.format)
                    image.verify()
                self.assertGreater(target.width, 0)
                self.assertGreater(target.height, 0)
                self.assertFalse(target.luma_min == target.luma_max == 255)
                self.assertFalse(target.luma_min == target.luma_max == 0)
                self.assertGreater(target.luma_stddev, 20.0)
                self.assertLessEqual(
                    target.aspect_delta_percent,
                    builder.ASPECT_TOLERANCE_PERCENT,
                )

    def test_manifest_matches_database_and_all_images(self) -> None:
        manifest = self.root / "docs/target-catalog/checksums.sha256"
        entries = [
            line.split("  ", 1)
            for line in manifest.read_text(encoding="utf-8").splitlines()
            if line.strip()
        ]
        self.assertEqual(28, len(entries))
        for expected_hash, relative_path in entries:
            with self.subTest(path=relative_path):
                self.assertEqual(
                    expected_hash,
                    file_hash(self.root / relative_path),
                )

    def test_two_up_layout_has_required_clearance(self) -> None:
        image_width = builder.TWO_UP_WIDTH_MM * mm
        page_width, page_height = A4
        self.assertGreaterEqual(
            (page_width - image_width) / 2,
            builder.MIN_MARGIN_MM * mm,
        )
        self.assertGreaterEqual(
            builder.TWO_UP_GAP_MM,
            builder.MIN_MARGIN_MM,
        )
        for first, second in zip(self.targets[::2], self.targets[1::2]):
            group_height = (
                image_width / first.pixel_aspect
                + image_width / second.pixel_aspect
                + builder.TWO_UP_GAP_MM * mm
            )
            self.assertGreaterEqual(
                (page_height - group_height) / 2,
                builder.MIN_MARGIN_MM * mm,
            )

    def test_generated_artifacts_page_counts_a4_and_source_immutability(self) -> None:
        before = {target.path: file_hash(target.path) for target in self.targets}
        with tempfile.TemporaryDirectory(prefix="target-catalog-test-") as temporary:
            output = Path(temporary)
            two_up = output / "two-up.pdf"
            one_up = output / "one-up.pdf"
            contact = output / "contact.pdf"
            contact_png = output / "contact.png"
            validation_json = output / "validation.json"

            builder.build_two_up(self.targets, two_up)
            builder.build_one_up(self.targets, one_up)
            builder.build_contact_pdf(self.targets, contact)
            builder.build_contact_sheet_png(self.targets, contact_png)
            builder.write_validation_results(self.targets, validation_json)

            expected_pages = ((two_up, 13), (one_up, 26), (contact, 1))
            expected_width, expected_height = A4
            for pdf_path, count in expected_pages:
                with self.subTest(pdf=pdf_path.name):
                    reader = PdfReader(str(pdf_path))
                    self.assertEqual(count, len(reader.pages))
                    for page in reader.pages:
                        self.assertAlmostEqual(
                            expected_width,
                            float(page.mediabox.width),
                            places=2,
                        )
                        self.assertAlmostEqual(
                            expected_height,
                            float(page.mediabox.height),
                            places=2,
                        )

            for path in (two_up, one_up):
                reader = PdfReader(str(path))
                for page in reader.pages:
                    self.assertIn(
                        "Imprimir em tamanho real / escala 100%.",
                        page.extract_text(),
                    )

            one_up_reader = PdfReader(str(one_up))
            for page, target in zip(one_up_reader.pages, self.targets):
                xobjects = page["/Resources"]["/XObject"]
                embedded = [
                    ref.get_object()
                    for ref in xobjects.values()
                    if ref.get_object().get("/Subtype") == "/Image"
                ]
                self.assertEqual(1, len(embedded))
                self.assertEqual(
                    ["/ASCII85Decode", "/DCTDecode"],
                    [str(item) for item in embedded[0]["/Filter"]],
                )
                jpeg_bytes = base64.a85decode(embedded[0]._data, adobe=True)
                self.assertEqual(target.path.read_bytes(), jpeg_bytes)

            with Image.open(contact_png) as image:
                self.assertEqual((1200, 1620), image.size)
                extrema = image.convert("L").getextrema()
                self.assertNotEqual(extrema[0], extrema[1])

        after = {target.path: file_hash(target.path) for target in self.targets}
        self.assertEqual(before, after)


if __name__ == "__main__":
    unittest.main(verbosity=2)

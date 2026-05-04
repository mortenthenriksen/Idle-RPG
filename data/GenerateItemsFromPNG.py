#!/usr/bin/env python3
"""
generate_item_csv.py
--------------------
Scans res://assets/items/equipment/equipable/ for PNG files,
parses their names, and appends new entries to items.csv.

Filename convention expected (any of):
  {material}_{slot}_{extra}.png          e.g. gold_armour_chest.png
  {slot}_{type}{N}_tier{T}.png           e.g. wooden_gloves_type6_tier1.png
  {material}_{slot}.png                  e.g. obsidian_boots.png

Run from the root of your Godot project:
  python generate_item_csv.py [--dry-run] [--csv path/to/items.csv] [--assets path/to/equipable/]
"""

import os, re, csv, math, argparse, sys

# ─────────────────────────────────────────────────────────────────────────────
# CONFIG  —  tweak these to your liking
# ─────────────────────────────────────────────────────────────────────────────

DEFAULT_ASSETS_DIR = "assets/items/equipment/equipable"
DEFAULT_CSV_PATH   = "data/items.csv"

# Which stat columns the CSV has (must match Item.cs field names)
CSV_COLUMNS = [
    "id", "name", "slot", "icon_path",
    "damage_min", "damage_max",
    "Health_min",   "Health_max",
    "defense_min","defense_max",
    "movement_speed_min", "movement_speed_max",
]

# Slots inferred from keyword in filename -> canonical slot name
SLOT_KEYWORDS = {
    "helmet":  "Helmet",
    "helm":    "Helmet",
    "chest":   "Chest",
    "armour":  "Chest",
    "armor":   "Chest",
    "pants":   "Pants",
    "legs":    "Pants",
    "boots":   "Boots",
    "boot":    "Boots",
    "gloves":  "Gloves",
    "glove":   "Gloves",
    "shield":  "Shield",
    "weapon":  "Weapon",
    "sword":   "Weapon",
    "axe":     "Weapon",
    "bow":     "Weapon",
    "staff":   "Weapon",
    "amulet":  "Amulet",
    "necklace":"Amulet",
    "ring":    "Ring",
}

# Per-slot: which stats the slot primarily gives (others get 0)
SLOT_STAT_WEIGHTS = {
    # (damage_w, Health_w, defense_w, move_w)
    "Weapon":  (1.0,  0.0,  0.0,  0.0),
    "Shield":  (0.0,  0.3,  1.0,  0.0),
    "Helmet":  (0.0,  0.5,  0.8,  0.0),
    "Chest":   (0.0,  0.8,  1.0,  0.0),
    "Pants":   (0.0,  0.5,  0.6,  0.3),
    "Boots":   (0.0,  0.2,  0.3,  1.0),
    "Gloves":  (0.5,  0.0,  0.3,  0.0),
    "Amulet":  (0.3,  0.5,  0.2,  0.2),
    "Ring":    (0.2,  0.3,  0.2,  0.2),
}

# Base stat values at type=1 (before type scaling)
BASE_STATS = {
    "damage":   (2.0,  5.0),   # (min, max)
    "Health":     (5.0, 12.0),
    "defense":  (2.0,  6.0),
    "move":     (1.0,  3.0),   # % bonus
}

# Each type level multiplies stats by this factor
TYPE_SCALE_PER_LEVEL = 1.35   # type6 = 1.35^5 ≈ 4.65× type1

# Materials that don't carry type numbers — assign a pseudo-type
MATERIAL_TIER = {
    "wooden":    1,
    "leather":   2,
    "iron":      3,
    "steel":     4,
    "gold":      5,
    "obsidian":  6,
    "shadow":    7,
    "void":      8,
    "divine":    9,
}

# ─────────────────────────────────────────────────────────────────────────────
# HELPERS
# ─────────────────────────────────────────────────────────────────────────────

def infer_slot(stem: str) -> str:
    """Return slot name from filename stem, or None if unrecognised."""
    parts = stem.lower().replace("-", "_").split("_")
    for part in parts:
        if part in SLOT_KEYWORDS:
            return SLOT_KEYWORDS[part]
    return None


def infer_type_level(stem: str) -> int:
    """Return the type number (1-9+) encoded in the filename."""
    # explicit "typeN"
    m = re.search(r"type(\d+)", stem, re.IGNORECASE)
    if m:
        return int(m.group(1))
    # material keyword
    for mat, lvl in MATERIAL_TIER.items():
        if mat in stem.lower():
            return lvl
    return 1  # fallback


def compute_stat_range(base_min, base_max, type_level, weight):
    """Scale a stat range by type level and weight."""
    if weight == 0:
        return 0.0, 0.0
    scale = TYPE_SCALE_PER_LEVEL ** (type_level - 1)
    lo = round(base_min * scale * weight, 2)
    hi = round(base_max * scale * weight, 2)
    return lo, hi


def make_display_name(stem: str) -> str:
    """Turn 'wooden_gloves_type6_tier1' into 'Wooden Gloves Type6 Tier1'."""
    return stem.replace("_", " ").title()


def godot_path(assets_dir: str, filename: str) -> str:
    """Convert a local path to a res:// path."""
    # normalise slashes
    rel = os.path.join(assets_dir, filename).replace("\\", "/")
    # strip leading project root if present
    rel = re.sub(r"^.*?assets/", "assets/", rel)
    return f"res://{rel}"


# ─────────────────────────────────────────────────────────────────────────────
# MAIN
# ─────────────────────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(description="Generate item CSV from PNG assets.")
    parser.add_argument("--assets", default=DEFAULT_ASSETS_DIR,
                        help="Path to the equipable folder (default: %(default)s)")
    parser.add_argument("--csv",    default=DEFAULT_CSV_PATH,
                        help="Path to the CSV file to create/update (default: %(default)s)")
    parser.add_argument("--dry-run", action="store_true",
                        help="Print rows without writing the file")
    args = parser.parse_args()

    assets_dir = args.assets
    csv_path   = args.csv

    if not os.path.isdir(assets_dir):
        print(f"ERROR: assets directory not found: {assets_dir}")
        print("Run this script from your Godot project root, or pass --assets <path>")
        sys.exit(1)

    # ── read existing CSV so we don't duplicate ──────────────────────────────
    existing_icons = set()
    existing_rows  = []
    next_id        = 1

    if os.path.isfile(csv_path):
        with open(csv_path, newline="", encoding="utf-8") as f:
            reader = csv.DictReader(f)
            for row in reader:
                existing_rows.append(row)
                existing_icons.add(row.get("icon_path", ""))
                try:
                    next_id = max(next_id, int(row["id"]) + 1)
                except (KeyError, ValueError):
                    pass

    # ── scan PNG files ────────────────────────────────────────────────────────
    new_rows = []
    skipped  = []

    png_files = sorted(f for f in os.listdir(assets_dir) if f.lower().endswith(".png"))

    for filename in png_files:
        stem      = os.path.splitext(filename)[0]
        icon_path = godot_path(assets_dir, filename)

        if icon_path in existing_icons:
            skipped.append(filename)
            continue

        slot = infer_slot(stem)
        if slot is None:
            print(f"  [SKIP] Could not infer slot: {filename}")
            continue

        type_level = infer_type_level(stem)
        weights    = SLOT_STAT_WEIGHTS.get(slot, (0, 0, 0, 0))
        dmg_w, Health_w, def_w, move_w = weights

        dmg_min,  dmg_max  = compute_stat_range(*BASE_STATS["damage"],  type_level, dmg_w)
        Health_min, Health_max = compute_stat_range(*BASE_STATS["Health"],    type_level, Health_w)
        def_min,  def_max  = compute_stat_range(*BASE_STATS["defense"], type_level, def_w)
        mov_min,  mov_max  = compute_stat_range(*BASE_STATS["move"],    type_level, move_w)

        row = {
            "id":                 next_id,
            "name":               stem,
            "slot":               slot,
            "icon_path":          icon_path,
            "damage_min":         dmg_min,
            "damage_max":         dmg_max,
            "Health_min":           Health_min,
            "Health_max":           Health_max,
            "defense_min":        def_min,
            "defense_max":        def_max,
            "movement_speed_min": mov_min,
            "movement_speed_max": mov_max,
        }
        new_rows.append(row)
        next_id += 1

    # ── report ────────────────────────────────────────────────────────────────
    print(f"\nFound {len(png_files)} PNG(s) in '{assets_dir}'")
    print(f"  Already in CSV : {len(skipped)}")
    print(f"  New entries    : {len(new_rows)}")

    if not new_rows:
        print("Nothing to add.")
        return

    # preview
    print("\nNew rows preview:")
    print(f"  {'id':>4}  {'name':<40}  {'slot':<8}  type  dmg        Health       def        move")
    print(f"  {'-'*4}  {'-'*40}  {'-'*8}  {'-'*4}  {'-'*10} {'-'*10} {'-'*10} {'-'*10}")
    for r in new_rows:
        type_lvl = infer_type_level(r["name"].lower().replace(" ","_"))
        print(f"  {r['id']:>4}  {r['name']:<40}  {r['slot']:<8}  {type_lvl:>4}  "
              f"{r['damage_min']:>4}-{r['damage_max']:<4}  "
              f"{r['Health_min']:>4}-{r['Health_max']:<4}  "
              f"{r['defense_min']:>4}-{r['defense_max']:<4}  "
              f"{r['movement_speed_min']:>4}-{r['movement_speed_max']:<4}")

    if args.dry_run:
        print("\n[DRY RUN] No file written.")
        return

    # ── write CSV ─────────────────────────────────────────────────────────────
    os.makedirs(os.path.dirname(csv_path) or ".", exist_ok=True)
    all_rows = existing_rows + new_rows

    with open(csv_path, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=CSV_COLUMNS, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(all_rows)

    print(f"\n✓ Written {len(all_rows)} total rows to '{csv_path}'")
    print(f"  ({len(new_rows)} new, {len(existing_rows)} existing)")


if __name__ == "__main__":
    main()
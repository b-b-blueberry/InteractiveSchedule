"""
blueberry's quick StardewXNBHack -> xnbcli-packable spritefont converter

Why this script?
    StardewXNBHack exports fonts into an easily-editable MonoGame format,
    while xnbcli expects fonts to be in a hybrid XNA/custom format with header data to repack them.

    If you're adding a custom spritefont to Stardew Valley you'll likely be adding it through the Content API, eg.
        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals(MySpriteFontAssetKey)
                (T)(object) Helper.Content.Load<SpriteFont>(MySpriteFontLocalAssetPath + ".xnb");
            return (T)(object) null
        }

Instructions on usage:
    1. Set 'target_dir' to some directory on your PC containing 1 font file.
       If many fonts are in the same directory, only the first alphabetically will be used.
    2. Set 'header_filename' to the name of your provided xnbcli header file, instructions below.
    3. Set 'output_filename' to whatever you like to match your project.
       The output file will expect a PNG spritefont image with the same name as 'output_filename'.
       If you like, you can change the PNG path expected after the file is converted by opening and searching 'export'.
    4. Run this script!
    5. Copy the output file to your 'xnbcli/unpacked' folder along with the matching PNG spritefont image,
       and run 'pack.bat' to pack it into an XNB file.
       If it failed, the 'xnbcli/packed' directory will be empty, or there will be an XNB of ~10 bytes in size.
    6. Copy the packed XNB file to your mod folder and include it in your project.

Instructions for providing a header file:
    For this script, we automatically pack the xnbcli header data into the file to save on time doing it manually
    each time you edit and pack the font. It's much easier.
    1. Unpack any spritefont through xnbcli
       (place the SpriteFont.xnb in 'xnbcli/packed' and run 'unpack.bat')
    2. Remove the entire 'content' entry from the file and close braces.
    3. You should now be left with a valid JSON file containing some 'header' and 'readers' entries.
       Save as some filename to match 'header_filename: str' below and place in your 'target_dir'.
"""

import os
import json
import glob
import pathlib

encoding = "utf-8"  # Spritefont should use "utf-8" for special characters. "utf-8-sig" apparently causes SyntaxError.
texture_format = 0  # Only tested values 0 and 5, seems to be unimportant.

target_dir: str = os.path.normpath("E:/Dev/Projects/SDV/SpritefontXnbHackToXnbcli")  # Substitute with your working dir.
out_filename: str = "output.json"     # Name of font will always be this value. No batch operations; one font at a time.
header_filename: str = "header.json"  # Files given to xnbcli to be packed require some standard headers.
out_fp: str = os.path.join(target_dir, out_filename)
header_fp: str= os.path.join(target_dir, header_filename)


# Form dictionaries from Glyphs field dict values formatted as strings, converting values to integers
# StardewXNBHack exports them in MonoGame format, which serialises X,Y,Width,Height into a space-separated
# string containing colon-separated key-value pairs.
# xnbcli expects them as discrete dictionaries, so we convert them here.
def field_to_str_int_dict(field: str):
    pairs = [pair.split(":") for pair in field[1:-1].split(" ")]
    return dict(zip([pair[0] for pair in pairs], [int(pair[1]) for pair in pairs]))


def main():
    os.chdir(target_dir)
    files = glob.glob(pathname="*.json", recursive=False)  # Opens only the first file found in target_dir
    in_fp = [f for f in files if f != out_filename][0]

    # Read file containing header data from any game spritefont unpacked through xnbcli previously
    with open(file=header_fp, mode="r", encoding=encoding) as header_file:
        header_js = json.loads(header_file.read())

        # Read StardewXNBHack unpacked font file
        with open(file=in_fp, mode="r", encoding=encoding) as in_file:
            in_js = json.loads(in_file.read())

            # Parse MonoGame-formatted fields into XNA/XNB format
            kerning = []
            cropping = []
            glyphs = []
            for index, character in enumerate(in_js["Characters"]):
                try:
                    glyph = in_js["Glyphs"][character]
                except KeyError:
                    # Skip characters with missing values: character will remain in characterMap/Characters.
                    continue
                bounds_field = field_to_str_int_dict(glyph["BoundsInTexture"])
                glyphs.append({
                    "x": bounds_field["X"],
                    "y": bounds_field["Y"],
                    "width": bounds_field["Width"],
                    "height": bounds_field["Height"],
                })
                crop_field = field_to_str_int_dict(glyph["Cropping"])
                cropping.append({
                    "x": crop_field["X"],
                    "y": crop_field["Y"],
                    "width": crop_field["Width"],
                    "height": crop_field["Height"]
                })
                kerning.append({
                    "x": int(glyph["LeftSideBearing"]),
                    "y": int(glyph["RightSideBearing"]),
                    "z": int(glyph["Width"]),
                })

            out_stem = pathlib.Path(out_fp).stem  # Stem is used for font image file, assuming filenames are identical.
            out_js = header_js
            out_js["content"] = {}

            # Order of fields seemingly matters here to xnbcli: 'texture' must be the first entry in 'content'.
            out_js["content"]["texture"] = {"format": texture_format, "export": out_stem + ".png"}

            # Populate readily-available fields
            out_js["content"]["verticalLineSpacing"] = in_js["LineSpacing"]
            out_js["content"]["horizontalSpacing"] = in_js["Spacing"]
            out_js["content"]["defaultCharacter"] = in_js["DefaultCharacter"]
            out_js["content"]["characterMap"] = in_js["Characters"]

            # Populate nested fields
            out_js["content"]["glyphs"] = glyphs
            out_js["content"]["kerning"] = kerning
            out_js["content"]["cropping"] = cropping

            # Write output to file
            with open(file=out_fp, mode="w", encoding=encoding) as out_file:
                json.dump(obj=out_js, fp=out_file, indent=4)


if __name__ == '__main__':
    main()

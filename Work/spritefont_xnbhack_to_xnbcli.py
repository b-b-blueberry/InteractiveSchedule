"""
blueberry's quick StardewXNBHack -> xnbcli-packable spritefont converter

Instructions on usage:
    1.  Set 'target_dir' to some directory on your PC containing 1 font file.
        If many fonts are in the same directory, only the first alphabetically will be used.
    2.  Set 'header_filename' to the name of your provided xnbcli header file, instructions below.
    3.  Set 'output_filename' to whatever you like to match your project.
        The output file will expect a PNG spritefont image with the same name as 'output_filename'.
        If you like, you can change the PNG path expected after the file is converted by opening and searching 'export'.
    4.  Run this script!

    (Optional:)

    5a. Set 'xnbcli_dir' to some directory containing your install of xnbcli.
        If set to a valid path, the 'out_fp' file will be created as normal,
        then xnbcli will pack it and deploy the XNB file into the given 'output_dir'.

    (Otherwise:)

    5b. Copy the output file to your 'xnbcli/unpacked' folder along with the matching PNG spritefont image,
        and run 'pack.bat' to pack it into an XNB file.
        If it failed, the 'xnbcli/packed' directory will be empty, or there will be an XNB of ~10 bytes in size.
    5c. Copy the packed XNB file to your mod folder and include it in your project.



Instructions for providing a header file:
    For this script, we automatically pack the xnbcli header data into the file to save on time doing it manually
    each time you edit and pack the font. It's much easier.
    1.  Unpack any spritefont through xnbcli
        (place the SpriteFont.xnb in 'xnbcli/packed' and run 'unpack.bat')
    2.  Remove the entire 'content' entry from the file and close braces.
    3.  You should now be left with a valid JSON file containing some 'header' and 'readers' entries.
        Save as some filename to match 'header_filename: str' below and place in your 'target_dir'.


Why this script?
    If you're adding a custom spritefont to Stardew Valley you'll likely be adding it through the Content API, eg.

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals(MySpriteFontAssetKey)
                (T)(object) Helper.Content.Load<SpriteFont>(MySpriteFontLocalAssetPath + ".xnb");
            return (T)(object) null
        }

    Since complete fonts are comprised of a definition (JSON) file and an image (PNG) file,
    we aren't able to load a single font through a single call to Content.Load<SpriteFont>:
    instead, we compile the font files into an XNB which is then automatically unpacked and loaded for us
    by the content manager.

    StardewXNBHack exports fonts into a sensibly-organised and easily-editable MonoGame format,
    while xnbcli expects fonts to be in a hybrid XNA/custom format with header data to repack them.
    This script converts font definition files from a well-defined format into a working mess.


Notes on input file format:
    To save on brain problems, fonts shouldn't ever be made in the XNA/custom format.
    It expects to be passed separate lists for 'bounds'/'cropping'/'kerning', with entries aligned with
    the index of the character in 'characters'. This is both clinically insane and impossible to reasonably manage.

    Instead, this program expects files to be formatted in the MonoGame/StardewXNBHack format:

        {
            "LineSpacing": 26,
            "Spacing": 0,
            "DefaultCharacter": "~",
            "Glyphs":
            {
                // Numerical
                "1":
                {
                  "BoundsInTexture": "{X:24 Y:0 Width:12 Height:20}",
                  "Cropping": "{X:0 Y:0 Width:12 Height:26}",
                  "Character": "1",
                  "LeftSideBearing": 0.0,
                  "Width": 12.0,
                  "RightSideBearing": 0.0,
                  "WidthIncludingBearings": 12.0
                },
                // Alphabetical
                "A":
                {
                  "BoundsInTexture": "{X:24 Y:0 Width:12 Height:20}",
                  "Cropping": "{X:0 Y:0 Width:12 Height:26}",
                  "Character": "A",
                  "LeftSideBearing": 0.0,
                  "Width": 12.0,
                  "RightSideBearing": 0.0,
                  "WidthIncludingBearings": 12.0
                }
            }
        }

    Formatting notes:
        -   The 'Characters' field is NOT required in the source file.
            The output file 'characterMap' list is populated with keys from your 'Glyphs' dictionary.
            You can happily omit making any separate list similar to 'Characters' altogether.
        -   The 'Character' field in each 'Glyphs' entry is NOT required in the source file.
            This can be omitted also for the same reason.
        -   Being a dictionary, each character in 'Glyphs' should only be defined once.
        -   Following MonoGame, values in serialised dictionary values should not be comma-separated, nor have precision.
        -   Other numerical values may be given as precise decimals.
        -   No trailing commas should be used throughout.
        -   C-style comments are accepted, but not preserved.
        -   The order of entries in the source file, or the order of each character in 'Glyphs', does not matter.
            Your entries will be ordered in the output file as expected by xnbcli/XNA.
"""

import os
import commentjson
import glob
import pathlib

encoding = "utf-8"  # Spritefont should use "utf-8" for special characters. "utf-8-sig" apparently causes SyntaxError.
texture_format = 0  # Only tested values 0 and 5, seems to be unimportant.

xnbcli_dir: str = os.path.normpath("E:/Dev/Projects/SDV/XNBCLI/xnbcli.exe")  # Optional; to automatically deploy font.
target_dir: str = os.path.normpath("E:/Dev/Projects/SDV/SpritefontXnbHackToXnbcli")  # Working source file directory.
output_dir: str = os.path.normpath("E:/Dev/Projects/SDV/Projects/InteractiveSchedule/InteractiveSchedule/assets")
out_filename: str = "output.json"  # Name of font will always be this value. No batch operations; one font at a time.
header_filename: str = "header.json"  # Files given to xnbcli to be packed require some standard headers.
out_fp: str = os.path.join(target_dir, out_filename)
header_fp: str = os.path.join(target_dir, header_filename)


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
        header_js = commentjson.loads(header_file.read())

        # Read StardewXNBHack unpacked font file
        with open(file=in_fp, mode="r", encoding=encoding) as in_file:
            in_js = commentjson.loads(in_file.read())

            # Parse MonoGame-formatted fields into XNA/XNB format
            kerning = []
            cropping = []
            glyphs = []

            # Characters must be sorted by ascending unicode decimal in the output file
            source_glyph_keys_sorted = sorted(in_js["Glyphs"].keys(), key=str)

            for character in source_glyph_keys_sorted:
                glyph = in_js["Glyphs"][character]
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
                    "x": float(glyph["LeftSideBearing"]),
                    "y": float(glyph["RightSideBearing"]),
                    "z": float(glyph["Width"]),
                })

            out_stem = pathlib.Path(out_fp).stem  # Stem is used for font image file, assuming filenames are identical.
            out_js = header_js
            out_js["content"] = {}

            # Order of fields seemingly matters here to xnbcli: 'texture' must be the first entry in 'content'.
            out_js["content"]["texture"] = {"format": texture_format, "export": out_stem + ".png"}

            out_js["content"]["glyphs"] = glyphs
            out_js["content"]["cropping"] = cropping
            out_js["content"]["characterMap"] = source_glyph_keys_sorted
            out_js["content"]["verticalLineSpacing"] = in_js["LineSpacing"]
            out_js["content"]["horizontalSpacing"] = in_js["Spacing"]
            out_js["content"]["kerning"] = kerning
            out_js["content"]["defaultCharacter"] = in_js["DefaultCharacter"]

            # Write output to file
            with open(file=out_fp, mode="w", encoding=encoding) as out_file:
                commentjson.dump(obj=out_js, fp=out_file, indent=4)

            # Run xnbcli to pack font file and deploy XNB to output directory
            if os.path.exists(xnbcli_dir):
                os.system(str.format("{} pack \"{}\" \"{}\"",
                                     xnbcli_dir,
                                     target_dir,
                                     output_dir))


if __name__ == '__main__':
    main()

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;
using xTile.Tiles;

namespace InteractiveSchedule
{
	public static class ISUtilities
	{
		public static Interface.Desktop GetDesktopFromOnscreenMenus()
		{
			return Game1.onScreenMenus.OfType<Interface.Desktop>().FirstOrDefault();
		}

		public static bool IsPointWithinViewportBounds(int x, int y)
		{
			return x > 0 && y > 0 && x < Game1.viewport.Width && y < Game1.viewport.Height;
		}

		public static string GetSingleLineEllipsisString(string text, SpriteFont font, int width)
		{
			string[] displayText = Game1.parseText(text: text ?? "null", whichFont: font, width: width).Split('\n');
			return $"{displayText.First().Trim()}{(displayText.Length == 1 ? "" : "...")}";
		}

		public static NPC GetCharacterUnderCursor()
		{
			Vector2 cursorTile = new Vector2(Game1.currentCursorTile.X, Game1.currentCursorTile.Y);
			NPC npc = Game1.currentLocation.isCharacterAtTile(cursorTile)
				?? Game1.currentLocation.isCharacterAtTile(new Vector2(cursorTile.X, cursorTile.Y + 1));
			return npc;
		}

		public static void SetNpcToSchedulePoint(ref NPC npc, int time)
		{
			
		}

		public static Vector2 GetOffsetToCentreText(SpriteFont font, string text, Point bounds, bool wrap)
		{
			if (wrap)
				text = Game1.parseText(text: text, whichFont: font, width: bounds.X);
			Vector2 textSize = font.MeasureString(text);
			textSize = GetOffsetToCentre(dimensions: textSize, bounds: bounds);
			if (font == Game1.smallFont)
				textSize.Y += 1.5f * Interface.CustomMenu.MenuScale;
			return textSize;
		}

		public static Vector2 GetOffsetToCentre(Vector2 dimensions, Point bounds)
		{
			return GetOffsetToCentre(dimensions: Utility.Vector2ToPoint(dimensions), bounds: bounds);
		}

		public static Vector2 GetOffsetToCentre(Point dimensions, Point bounds)
		{
			return new Vector2(
				(bounds.X - dimensions.X) / 2,
				(bounds.Y - dimensions.Y) / 2);
		}

		public static Tile GetTileOnHighestLayer(GameLocation location, int x, int y)
		{
			IEnumerable<Layer> layers = location.Map.Layers.Reverse();
			foreach (Layer layer in layers)
			{
				if (layer.Tiles[x, y] != null)
				{
					return layer.Tiles[x, y];
				}
			}
			return null;
		}

		public static List<Tile> GetTilesAtPosition(Vector2 tilePosition)
		{
			List<Tile> tiles = new List<Tile>();
			foreach (Layer layer in Game1.currentLocation.Map.Layers.Reverse())
			{
				Tile tile = layer.Tiles[(int)tilePosition.X, (int)tilePosition.Y];
				if (tile != null)
					tiles.Add(tile);
			}
			return tiles;
		}
	}
}

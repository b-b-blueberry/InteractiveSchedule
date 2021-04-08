using Microsoft.Xna.Framework;
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
		public static NPC GetCharacterUnderCursor()
		{
			Vector2 cursorTile = new Vector2(Game1.currentCursorTile.X, Game1.currentCursorTile.Y - 1);
			NPC npc = Game1.currentLocation.isCharacterAtTile(cursorTile)
				?? Game1.currentLocation.isCharacterAtTile(new Vector2(cursorTile.X, cursorTile.Y - 1));
			return npc;
		}

		public static void SetNpcToSchedulePoint(ref NPC npc, int time)
		{
			
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
	}
}

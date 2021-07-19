using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.ObjectModel;
using xTile.Tiles;

namespace InteractiveSchedule.Interface.Views
{
	public class TileTabView : Components.TabViewComponent
	{
		public readonly List<Texture2D> PreservedTileSheetImages = new List<Texture2D>();
		public const string TilePropertiesTabId = "tileproperties";
		public const string TileIndexesTabId = "tileindexes";

		public TileTabView(IClickableMenu parentMenu, Point relativePosition)
			: base (parentMenu: parentMenu, relativePosition: relativePosition)
		{
			this.AddTabs(whichTabs: new [] {TilePropertiesTabId, TileIndexesTabId });
			if (!((Menus.TileInfoMenu)_parentMenu).PreservedCursorTiles.Any(tile => tile.Properties.Any()))
			{
				// Default to tile indexes for first-time open if no properties were shown on the cursor
				ActiveTab = TileIndexesTabId;
			}
		}

		public void SetTileSheetImages(List<Tile> tiles)
		{
			PreservedTileSheetImages.Clear();
			foreach (Tile tile in tiles)
			{
				PreservedTileSheetImages.Add(Game1.content.Load<Texture2D>(tile.TileSheet.ImageSource));
			}
		}

		public override void DrawContent(SpriteBatch b)
		{
			Vector2 position = ContentOrigin;
			if (ActiveTab == TilePropertiesTabId)
			{
				if (((Menus.TileInfoMenu)_parentMenu).PreservedCursorTiles.Any(tile => tile.Properties.Any()))
				{
					// Tile properties
					foreach (Tile tile in ((Menus.TileInfoMenu)_parentMenu).PreservedCursorTiles)
					{
						foreach (KeyValuePair<string, PropertyValue> property in tile.Properties)
						{
							position.Y += this.DrawText(b,
								position: position,
								text: property.Key + ": " + property.Value.ToString(),
								font: BodyTextFont, colour: BodyTextColour);
						}
					}
				}
				else
				{
					// Filler text
					this.DrawText(b,
						position: position,
						text: ModEntry.Instance.i18n.Get("ui.tileinfo.tileproperties.text.none"),
						font: BodyTextFont,
						colour: BodyTextColour);
				}
			}
			else if (ActiveTab == TileIndexesTabId)
			{
				// Tile indexes in sheets
				for (int i = 0; i < ((Menus.TileInfoMenu)_parentMenu).PreservedCursorTiles.Count; ++i)
				{
					Tile tile = ((Menus.TileInfoMenu)_parentMenu).PreservedCursorTiles[i];
					float xOffset = 0;

					// Tile layer
					this.DrawText(b,
						position: position,
						text: tile.Layer.Id.Take(4).Aggregate("", (str, cur) => str + cur) + ".",
						font: BodyTextFont,
						colour: BodyTextColour);
					xOffset += BodyTextFont.MeasureString("BBBB.").X;

					// Tile preview
					b.Draw(texture: PreservedTileSheetImages[i],
						destinationRectangle: new Rectangle(
							(int)(position.X + xOffset),
							(int)position.Y,
							tile.TileSheet.TileWidth * 2,
							tile.TileSheet.TileHeight * 2),
						sourceRectangle: tile.TileSheet.GetTileImageBounds(tile.TileIndex).ToXna(),
						color: Color.White);
					xOffset += (tile.TileSheet.TileWidth * 2) + Padding.X;

					// Tile index and sheet ID
					position.Y += this.DrawText(b,
						position: new Vector2(position.X + xOffset, position.Y),
						text: tile.TileIndex.ToString().PadRight(6, ' ') + tile.TileSheet.Id,
						font: BodyTextFont, colour: BodyTextColour);
				}
			}
		}
	}
}

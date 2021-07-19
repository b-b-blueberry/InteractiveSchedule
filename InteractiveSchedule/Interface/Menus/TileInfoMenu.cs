using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace InteractiveSchedule.Interface.Menus
{
	public class TileInfoMenu : WindowPage
	{
		public ClickableTextureComponent GridViewButton, TileHighlightButton, PathsLayerButton;
		public bool ShowGrid;
		public bool ShowTileHighlight;
		public bool ShowPathsLayer;
		public List<Tile> CursorTiles = new List<Tile>();
		public List<Tile> PreservedCursorTiles = new List<Tile>();
		public Vector2 PreservedTilePosition;
		public string PreservedTileLocationName;
		public bool IsHoveringTile => CursorTiles.Count > 0;

		private float _tileHighlightOpacity;
		private Color _tileHighlightColour;
		private Color _tileActionColour;
		private Color _gridColour;
		private int _gridWidth;
		private static Texture2D _pathsTexture;

		private Views.TileTabView _tilePageTabView;

		public override bool IsOnHomePage => PreservedCursorTiles.Count == 0;
		public override bool IsUpButtonVisible => !IsOnHomePage;
		public override bool IsActionButtonSidebarVisible => true;

		public TileInfoMenu(Point position)
			: base(position: position)
		{
			ModEntry.Instance.Helper.Events.Display.RenderedWorld += this.Display_RenderedWorld;
		}

		protected override void cleanupBeforeExit()
		{
			ModEntry.Instance.Helper.Events.Display.RenderedWorld -= this.Display_RenderedWorld;
			GridViewButton = null;
			TileHighlightButton = null;

			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();

			_tileHighlightOpacity = 0.35f;
			_tileHighlightColour = Color.CornflowerBlue;
			_tileActionColour = Color.Orchid;
			_gridColour = Color.LavenderBlush * 0.6f;
			_gridWidth = 1;
			_pathsTexture = Game1.content.Load<Texture2D>("Maps\\paths");
			ShowGrid = false;
			ShowTileHighlight = false;
		}

		public override void RealignElements()
		{
			base.RealignElements();

			if (WindowBar != null)
			{
				WindowBar.width = width = 600;
				height = WindowBar.IsFullscreen
					? WindowBar.FullscreenHeight
					: 420;
			}

			if (_tilePageTabView == null)
			{
				Point relativePosition = new Point(
					(int)BorderSafeOffset.X,
					(int)BorderSafeOffset.Y
						+ (int)HeadingTextFont.MeasureString(ModEntry.Instance.i18n.Get("ui.tileinfo.tile.heading")).Y
						+ (Padding.Y * 2)
						+ (int)BodyTextFont.MeasureString("Eggbody").Y
						+ (Padding.Y * 3));
				_tilePageTabView = new Views.TileTabView(parentMenu: this, relativePosition: relativePosition);
			}
			else
			{
				_tilePageTabView.RealignElements();
			}
		}

		public override void RealignFloatingButtons()
		{
			base.RealignFloatingButtons();
		}

		protected override void AddActionButtons()
		{
			base.AddActionButtons();

			GridViewButton = this.CreateActionButton(which: nameof(GridViewButton));
			TileHighlightButton = this.CreateActionButton(which: nameof(TileHighlightButton));
			PathsLayerButton = this.CreateActionButton(which: nameof(PathsLayerButton));

			SidebarActionButtons.AddRange(new [] { GridViewButton, TileHighlightButton, PathsLayerButton });
		}

		protected override void ClickUpButton()
		{
			PreservedCursorTiles.Clear();
		}

		public bool SelectTile(GameLocation location, Vector2 tilePosition)
		{
			PreservedTileLocationName = location.Name;
			PreservedTilePosition = tilePosition;
			PreservedCursorTiles = ISUtilities.GetTilesAtPosition(tilePosition);
			_tilePageTabView.SetTileSheetImages(PreservedCursorTiles);
			return PreservedCursorTiles.Count > 0;
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			_tilePageTabView.receiveKeyPress(key);
		}

		protected override void Hover(int x, int y)
		{
			_tilePageTabView.performHoverAction(x, y);
		}

		protected override void LeftClick(int x, int y, bool playSound)
		{
			// Action buttons
			if (SidebarActionButtons.Any())
			{
				if (GridViewButton.containsPoint(x, y))
				{
					ShowGrid = !ShowGrid;
					return;
				}
				if (TileHighlightButton.containsPoint(x, y))
				{
					ShowTileHighlight = !ShowTileHighlight;
					return;
				}
				if (PathsLayerButton.containsPoint(x, y))
				{
					ShowPathsLayer = !ShowPathsLayer;
					return;
				}
			}

			_tilePageTabView.receiveLeftClick(x, y, playSound);
		}

		private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
		{
			if (Game1.currentLocation == null)
			{
				return;
			}

			if (ShowPathsLayer)
			{
				TileInfoMenu.DrawPathsLayer(e.SpriteBatch);
			}
			if (ShowGrid)
			{
				TileInfoMenu.DrawGrid(e.SpriteBatch, colour: _gridColour, width: _gridWidth, opacity: 1f);
			}
			Point mouse = Game1.getMousePosition();
			bool isWindowHovered = Desktop.Taskbar.IsExpanded && Desktop.Children.Any(child => child.isWithinBounds(mouse.X, mouse.Y)
				|| ((child.GetParentMenu() is WindowBar windowBar) && windowBar.isWithinBounds(mouse.X, mouse.Y)));
			if (ShowTileHighlight && !isWindowHovered)
			{
				this.DrawTileHighlight(e.SpriteBatch, showInfo: true);
			}
		}

		private void DrawCoordinatesPage(SpriteBatch b)
		{
			Vector2 targetPosition;
			//Tile tile = ModEntry.GetTileOnHighestLayer(location: Game1.currentLocation, x: (int)targetPosition.X, y: (int)targetPosition.Y);

			Vector2 position = ContentOrigin;
			string text;

			text = ModEntry.Instance.i18n.Get("ui.tileinfo.home.heading");
			position.Y += this.DrawHeading(b, position: position, text: text, drawBackground: true).Y;

			targetPosition = new Vector2(Game1.viewport.X, Game1.viewport.Y);
			text = "View: " + targetPosition.ToString();
			position.Y += this.DrawText(b, position: position, text: text);

			targetPosition = Utility.PointToVector2(Game1.getMousePosition());
			text = "Screen: " + targetPosition.ToString();
			position.Y += this.DrawText(b, position: position, text: text);

			targetPosition = new Vector2(Game1.viewport.X + Game1.getMouseX(), Game1.viewport.Y + Game1.getMouseY());
			text = "World: " + targetPosition.ToString();
			position.Y += this.DrawText(b, position: position, text: text);

			targetPosition = Game1.currentCursorTile;
			text = "Tile: " + targetPosition.ToString();
			position.Y += this.DrawText(b, position: position, text: text);

			targetPosition = new Vector2(Game1.currentLocation.Map.DisplayWidth, Game1.currentLocation.Map.DisplayHeight) / Game1.tileSize;
			text = "Map: " + targetPosition.ToString();
			position.Y += this.DrawText(b, position: position, text: text);

			targetPosition = Game1.player.getTileLocation();
			text = "Player: " + "(X:" + (int)targetPosition.X + " Y:" + (int)targetPosition.Y + ")";
			position.Y += this.DrawText(b, position: position, text: text);
		}

		private void DrawTilePage(SpriteBatch b)
		{
			// Heading
			Vector2 position = ContentOrigin;
			position.Y += this.DrawHeading(b,
				position: position,
				text: $"{PreservedTileLocationName} {PreservedTilePosition}",
				drawBackground: true).Y;

			// Tile tab view contents
			_tilePageTabView.draw(b);
		}

		private static void DrawGrid(SpriteBatch b, Color colour, int width = 1, float opacity = 1f)
		{
			// DON'T touch ANYTHING

			Point start = new Point();
			Point end = new Point();

			start.X = Game1.viewport.X > 0 ? 0 - (Game1.viewport.X % Game1.tileSize) : 0 - Game1.viewport.X;
			start.Y = Game1.viewport.Y > 0 ? 0 - ((Game1.viewport.Y % Game1.tileSize) / 2) : 0 - (Game1.viewport.Y / 2);
			end.X = Math.Min(Game1.currentLocation.Map.DisplayWidth - Game1.viewport.X, start.X + Math.Min(Game1.viewport.Width, Game1.currentLocation.Map.DisplayWidth));
			end.Y = Math.Min(Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Y, start.Y + Math.Min(Game1.viewport.Height, Game1.currentLocation.Map.DisplayHeight));

			for (int i = start.Y; i < end.Y + Game1.tileSize; i += Game1.tileSize)
			{
				Point point = new Point(start.X, start.Y + i);
				Desktop.DrawLine(b: b, colour: colour * opacity, startPosition: point, length: end.X - start.X, width: width, isHorizontal: true);
			}

			start.X = Game1.viewport.X > 0 ? 0 - ((Game1.viewport.X % Game1.tileSize) / 2) : 0 - (Game1.viewport.X / 2);
			start.Y = Game1.viewport.Y > 0 ? 0 - (Game1.viewport.Y % Game1.tileSize) : 0 - Game1.viewport.Y;
			end.X = Math.Min(Game1.currentLocation.Map.DisplayWidth - Game1.viewport.X, start.X + Math.Min(Game1.viewport.Width, Game1.currentLocation.Map.DisplayWidth));
			end.Y = Math.Min(Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Y, start.Y + Math.Min(Game1.viewport.Height, Game1.currentLocation.Map.DisplayHeight));

			for (int i = start.X; i < end.X + Game1.tileSize; i += Game1.tileSize)
			{
				Point point = new Point(start.X + i, start.Y);
				Desktop.DrawLine(b: b, colour: colour * opacity, startPosition: point, length: end.Y - start.Y, width: width, isHorizontal: false);
			}
		}

		private void DrawTileHighlight(SpriteBatch b, bool showInfo)
		{
			// Identify bounding tiles
			float xForPosViewport = Math.Min(Game1.currentLocation.Map.DisplayWidth - Game1.tileSize, (Game1.currentCursorTile.X * Game1.tileSize)) - Game1.viewport.X;
			float yForPosViewport = Math.Min(Game1.currentLocation.Map.DisplayHeight - Game1.tileSize, (Game1.currentCursorTile.Y * Game1.tileSize)) - Game1.viewport.Y;
			float xForNegViewport = 0 - Game1.viewport.X;
			float yForNegViewport = 0 - Game1.viewport.Y;
			int x = (int)((Game1.getMouseX() + Game1.viewport.X > 0) ? xForPosViewport : xForNegViewport);
			int y = (int)((Game1.getMouseY() + Game1.viewport.Y > 0) ? yForPosViewport : yForNegViewport);

			// Tile highlight
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: new Rectangle(x, y, Game1.tileSize, Game1.tileSize),
				color: _tileHighlightColour * _tileHighlightOpacity);

			if (showInfo)
			{
				// Mark tiles under cursor
				CursorTiles = ISUtilities.GetTilesAtPosition(Game1.currentCursorTile);

				// Tile coordinates
				float scale = 2f;
				int padding = (int)(2 * scale);
				int yOffset = (int)(Padding.Y * scale);
				b.Draw(
					texture: Game1.fadeToBlackRect,
					destinationRectangle: new Rectangle(x, y - yOffset - padding, Game1.tileSize, yOffset + padding),
					color: _tileHighlightColour);
				Utility.drawTinyDigits(
					toDraw: (int)Game1.currentCursorTile.X,
					b: b,
					position: new Vector2(x + padding, y - yOffset),
					scale: scale,
					layerDepth: 1f,
					c: Color.White);
				Utility.drawTinyDigits(
					toDraw: (int)Game1.currentCursorTile.Y,
					b: b,
					position: new Vector2(x + padding + (14 * scale), y - yOffset),
					scale: scale,
					layerDepth: 1f,
					c: Color.White);

				// Draw tile actions
				if (CursorTiles.Any(tile => tile.Properties.Any()))
				{
					int addedY = 0;
					int textWidth = 0;
					const float textScale = 0.5f;

					// Draw container
					foreach (Tile tile in CursorTiles)
					{
						foreach (string property in tile.Properties.Keys)
						{
							addedY += yOffset;
							textWidth = (int)Math.Max(textWidth, padding + (BodyTextFont.MeasureString(text: property).X * textScale));
						}
					}
					addedY = Math.Max(Game1.tileSize, addedY);
					textWidth = Math.Max(Game1.tileSize, textWidth);
					b.Draw(
						texture: Game1.fadeToBlackRect,
						destinationRectangle: new Rectangle(x + Game1.tileSize, y - yOffset - padding, textWidth, yOffset + padding),
						color: _tileActionColour);
					b.Draw(
						texture: Game1.fadeToBlackRect,
						destinationRectangle: new Rectangle(x + Game1.tileSize, y, textWidth, addedY),
						color: _tileActionColour * (_tileHighlightOpacity * 2f));

					// Draw tile action properties
					addedY = 0;
					foreach (Tile tile in CursorTiles)
					{
						foreach (string property in tile.Properties.Keys)
						{
							b.DrawString(
								spriteFont: BodyTextFont,
								text: property,
								position: new Vector2(x + Game1.tileSize + padding, y + addedY),
								color: Color.White,
								rotation: 0f,
								origin: Vector2.Zero,
								scale: textScale,
								effects: SpriteEffects.None,
								layerDepth: 1f);
							addedY += yOffset;
						}
					}
				}
			}
		}

		public static void DrawPathsLayer(SpriteBatch b)
		{
			TileSheet tileSheet = Game1.currentLocation.Map.TileSheets.FirstOrDefault(ts => ts.ImageSource == _pathsTexture.Name);
			if (tileSheet == null)
				return;
			Layer paths = Game1.currentLocation.Map.Layers.FirstOrDefault(layer => layer.DependsOnTileSheet(tileSheet));
			if (paths == null || _pathsTexture == null)
				return;

			for (int x = 0; x < paths.LayerWidth; ++x)
			{
				for (int y = 0; y < paths.LayerHeight; ++y)
				{
					if (paths.Tiles[x, y] == null)
						continue;
					b.Draw(
						texture: _pathsTexture,
						position: new Vector2((x * Game1.tileSize) - Game1.viewport.X, (y * Game1.tileSize) - Game1.viewport.Y),
						sourceRectangle: Game1.getSourceRectForStandardTileSheet(tileSheet: _pathsTexture, tilePosition: paths.Tiles[x, y].TileIndex, width: 16, height: 16),
						color: Color.White,
						rotation: 0f,
						origin: Vector2.Zero,
						scale: Game1.pixelZoom,
						effects: SpriteEffects.None,
						layerDepth: 1f);
				}
			}
		}

		protected override void DrawContent(SpriteBatch b)
		{
			if (IsOnHomePage)
			{
				this.DrawCoordinatesPage(b);
			}
			else
			{
				this.DrawTilePage(b);
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}

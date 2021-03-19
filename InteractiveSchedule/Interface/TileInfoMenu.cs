using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;

namespace InteractiveSchedule.Interface
{
	public class TileInfoMenu : WindowPage
	{
		public ClickableTextureComponent ToggleGridButton;
		public ClickableTextureComponent ToggleTileHighlightButton;

		private Color _tileHighlightColour;
		private Color _gridColour;
		private int _gridWidth;
		private bool _showGrid;
		private bool _showTileHighlight;

		public override bool IsOnHomePage => true;
		public override bool IsActionButtonSidebarVisible => true;

		public TileInfoMenu(Point position) : base(position: position)
		{
			ModEntry.Instance.Helper.Events.Display.RenderedWorld += this.Display_RenderedWorld;

			this.RealignElements();
		}

		protected override void cleanupBeforeExit()
		{
			ModEntry.Instance.Helper.Events.Display.RenderedWorld -= this.Display_RenderedWorld;
			ToggleGridButton = null;
			ToggleTileHighlightButton = null;

			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();

			_tileHighlightColour = Color.CornflowerBlue * 0.35f;
			_gridColour = Color.LavenderBlush * 0.6f;
			_gridWidth = 1;
			_showGrid = false;
			_showTileHighlight = false;
		}

		public override void RealignElements()
		{
			base.RealignElements();

			if (_parentMenu != null)
			{
				Vector2 bodyCharSize = BodyTextFont.MeasureString("(");
				Vector2 headingCharSize = HeadingTextFont.MeasureString(")");
				width = (int)(Math.Max(_parentMenu.width, (bodyCharSize.X * 28) + Padding.X));
				height = (int)((bodyCharSize.Y * 6) + (headingCharSize.Y * 1) + (Padding.Y * 3));
			}
		}

		public override void RealignFloatingButtons()
		{
			base.RealignFloatingButtons();
		}

		protected override void AddActionButtons()
		{
			base.AddActionButtons();

			Rectangle sourceRect = new Rectangle(0, ActionButtonIconOffsetY + (1 * ActionButtonIconSize.Y), ActionButtonIconSize.X, ActionButtonIconSize.Y);
			ToggleGridButton = this.CreateActionButton(which: nameof(ToggleGridButton), sourceRect: sourceRect);
			SidebarActionButtons.Add(ToggleGridButton);

			sourceRect.X += sourceRect.Width;
			ToggleTileHighlightButton = this.CreateActionButton(which: nameof(ToggleTileHighlightButton), sourceRect: sourceRect);
			SidebarActionButtons.Add(ToggleTileHighlightButton);
		}

		protected override void ClickUpButton()
		{
			throw new NotImplementedException();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);

			if (!IsSelected || !ShouldDraw)
				return;

			if (ToggleGridButton.containsPoint(x, y))
			{
				_showGrid = !_showGrid;
			}
			if (ToggleTileHighlightButton.containsPoint(x, y))
			{
				_showTileHighlight = !_showTileHighlight;
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);

			if (!ShouldDraw)
				return;
		}

		private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
		{
			if (Game1.currentLocation == null)
			{
				return;
			}

			if (_showGrid)
			{
				TileInfoMenu.DrawGrid(e.SpriteBatch, colour: _gridColour, width: _gridWidth, opacity: 1f);
			}

			Point mouse = Game1.getMousePosition();
			bool isChildHovered = Desktop.Taskbar.IsExpanded && Desktop.Children.Any(child => child.isWithinBounds(mouse.X, mouse.Y)
				|| ((child.GetParentMenu() is WindowBar windowBar) && windowBar.isWithinBounds(mouse.X, mouse.Y)));
			if (_showTileHighlight && !isChildHovered)
			{
				TileInfoMenu.DrawTileHighlight(e.SpriteBatch, colour: _tileHighlightColour);
			}
		}

		private void DrawTileInfo(SpriteBatch b)
		{
			Vector2 targetPosition;
			//Tile tile = ModEntry.GetTileOnHighestLayer(location: Game1.currentLocation, x: (int)targetPosition.X, y: (int)targetPosition.Y);

			Vector2 position = Utility.PointToVector2(ContentSafeArea.Location);
			string text;

			text = ModEntry.Instance.i18n.Get("ui.tileinfo.heading");
			this.DrawHeading(b, position: position, text: text, drawBackground: true);

			position.Y += ContentSafeArea.Height;
			targetPosition = new Vector2(Game1.viewport.X, Game1.viewport.Y);
			text = "View: " + targetPosition.ToString();
			position.Y -= BodyTextFont.MeasureString(text: text).Y;
			this.DrawText(b, position: position, text: text);

			targetPosition = Utility.PointToVector2(Game1.getMousePosition());
			text = "Screen: " + targetPosition.ToString();
			position.Y -= BodyTextFont.MeasureString(text: text).Y;
			this.DrawText(b, position: position, text: text);

			targetPosition = new Vector2(Game1.viewport.X + Game1.getMouseX(), Game1.viewport.Y + Game1.getMouseY());
			text = "World: " + targetPosition.ToString();
			position.Y -= BodyTextFont.MeasureString(text: text).Y;
			this.DrawText(b, position: position, text: text);

			targetPosition = Game1.currentCursorTile;
			text = "Tile: " + targetPosition.ToString();
			position.Y -= BodyTextFont.MeasureString(text: text).Y;
			this.DrawText(b, position: position, text: text);

			targetPosition = new Vector2(Game1.currentLocation.Map.DisplayWidth, Game1.currentLocation.Map.DisplayHeight) / Game1.tileSize;
			text = "Map: " + targetPosition.ToString();
			position.Y -= BodyTextFont.MeasureString(text: text).Y;
			this.DrawText(b, position: position, text: text);

			targetPosition = Game1.player.Position;
			text = "Player: " + "(X:" + (int)targetPosition.X + " Y:" + (int)targetPosition.Y + ")";
			position.Y -= BodyTextFont.MeasureString(text: text).Y;
			this.DrawText(b, position: position, text: text);
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

		private static void DrawTileHighlight(SpriteBatch b, Color colour)
		{
			// it works, don't touch it

			float xForPosViewport = Math.Min(Game1.currentLocation.Map.DisplayWidth - Game1.tileSize, (Game1.currentCursorTile.X * Game1.tileSize)) - Game1.viewport.X;
			float yForPosViewport = Math.Min(Game1.currentLocation.Map.DisplayHeight - Game1.tileSize, (Game1.currentCursorTile.Y * Game1.tileSize)) - Game1.viewport.Y;
			float xForNegViewport = 0 - Game1.viewport.X;
			float yForNegViewport = 0 - Game1.viewport.Y;
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: new Rectangle(
					(int)((Game1.getMouseX() + Game1.viewport.X > 0) ? xForPosViewport : xForNegViewport),
					(int)((Game1.getMouseY() + Game1.viewport.Y > 0) ? yForPosViewport : yForNegViewport),
					Game1.tileSize,
					Game1.tileSize),
				color: colour);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);

			if (!ShouldDraw)
				return;

			this.DrawTileInfo(b);

			this.DrawFloatingActionButtons(b);
			this.DrawHoverText(b);
		}
	}
}

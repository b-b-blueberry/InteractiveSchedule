using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface
{
	public abstract class CustomMenu : IClickableMenu
	{
		/// <summary>
		/// Base scale multiplier for the menu.
		/// Affects sprite size and menu element bounds.
		/// </summary>
		public const int MenuScale = 3;

		public int BorderWidth { get; protected set; }
		/// <summary>
		/// The area of the page, after considering <see cref="BorderWidth"/>, to place content/multimedia.
		/// Designed for media spanning the whole width of the page, ignoring padding and sidebars.
		/// </summary>
		public Rectangle BorderSafeArea { get; protected set; }
		/// <summary>
		/// The area of the page, after considering <see cref="BorderWidth"/>, <see cref="Padding"/>, and <see cref="ActionButtonSidebarArea"/>, to place content/multimedia.
		/// Designed for common spaced page elements such as text and images.
		/// </summary>
		public Rectangle ContentSafeArea { get; protected set; }
		protected static float ShadowOpacity;
		protected static Color ShadowColour;
		protected static Color BodyTextColour;
		protected static Color HeadingTextColour;
		protected static SpriteFont HeadingTextFont => Game1.dialogueFont;
		protected static SpriteFont BodyTextFont => Game1.smallFont;

		protected static float HoverDelayScale;
		protected static float LeftClickHeldDelayScale;
		protected static float RightClickHeldDelayScale;
		protected static float LeftClickHoldDelay;
		protected static float RightClickHoldDelay;

		protected float _leftClickHeldTimer;
		protected float _hoverTimer;
		protected string _hoverText;

		/// <summary>
		/// Dimensions of shared window icons found in <see cref="WindowComponent"/> and <see cref="Taskbar.Icons"/>.
		/// </summary>
		protected static readonly Point IconSize = new Point(12, 12);
		/// <summary>
		/// Common offset for custom menu elements; used for margins, padding, and spacing.
		/// </summary>
		protected static readonly Point Padding = new Point(6 * MenuScale, 3 * MenuScale);
		/// <summary>
		/// Index of the window icons for each <see cref="WindowPage"/> type in the source <see cref="ModEntry.Sprites"/> spritesheet.
		/// </summary>
		protected static readonly List<string> IconSourceIndex = new List<string>
		{
			nameof(Menus.ModInfoMenu), nameof(Menus.CharacterListMenu), nameof(Menus.SchedulePreviewMenu),
			"GiftsMenu", "DialogueMenu", "FileManagerMenu", "BuildMenu",
			nameof(Menus.TileInfoMenu), nameof(Menus.MapMenu), "OptionsMenu", "HelpMenu"
		};

		public virtual void SetDefaults()
		{
			ShadowOpacity = 0.5f;
			ShadowColour = Color.MidnightBlue;
			BodyTextColour = Game1.textColor;
			HeadingTextColour = Color.White;

			LeftClickHeldDelayScale = 1f;
			RightClickHeldDelayScale = 1f;
			HoverDelayScale = 1f;
			LeftClickHoldDelay = 10f;
			RightClickHoldDelay = 2f;
		}

		public abstract void RealignElements();

		protected CustomMenu()
		{
			this.SetDefaults();
		}

		/// <summary>
		/// Returns the source area for the window icon of some <see cref="WindowPage"/> item, indexed by type name.
		/// </summary>
		/// <param name="menuTypeName">Type name for this <see cref="WindowPage"/> object.</param>
		protected static Rectangle GetIconSourceRect(string menuTypeName)
		{
			return new Rectangle(IconSourceIndex.IndexOf(menuTypeName) * IconSize.X, 48, IconSize.X, IconSize.Y);
		}

		/// <summary>
		/// Draws a simple light transparent square over the bounds of a <see cref="ClickableComponent"/>.
		/// </summary>
		/// <param name="clickable">Clickable item to draw over.</param>
		protected static void DrawHighlight(SpriteBatch b, ClickableComponent clickable)
		{
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: clickable.bounds,
				color: Color.White * 0.3f);
		}

		public virtual int DrawText(SpriteBatch b, Vector2 position, string text, SpriteFont font, Color colour)
		{
			b.DrawString(
				spriteFont: font,
				text: text,
				position: position,
				color: colour);
			return (int)font.MeasureString(text).Y + Padding.Y;
		}

		/// <summary>
		/// Common method for calling existing <see cref="IClickableMenu.drawHoverText"/> method.
		/// </summary>
		protected void DrawHoverText(SpriteBatch b)
		{
			if (!string.IsNullOrEmpty(_hoverText))
				IClickableMenu.drawHoverText(b, text: _hoverText, font: Game1.smallFont);
		}

		public override void draw(SpriteBatch b)
		{
			if (!string.IsNullOrEmpty(_hoverText))
				IClickableMenu.drawHoverText(b, text: _hoverText, font: Game1.smallFont);

			base.draw(b);
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (_parentMenu is WindowBar)
				_parentMenu?.update(time);
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (_parentMenu is WindowBar)
				_parentMenu?.receiveKeyPress(key);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (_parentMenu is WindowBar)
				_parentMenu?.receiveLeftClick(x, y, playSound);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			base.receiveRightClick(x, y, playSound);
			if (_parentMenu is WindowBar)
				_parentMenu?.receiveRightClick(x, y, playSound);
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (_parentMenu is WindowBar)
				_parentMenu?.leftClickHeld(x, y);
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			if (_parentMenu is WindowBar)
				_parentMenu?.releaseLeftClick(x, y);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (_parentMenu is WindowBar)
				_parentMenu?.receiveScrollWheelAction(direction);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (_parentMenu is WindowBar)
				_parentMenu?.performHoverAction(x, y);
		}

		// NO snapping
		public override void automaticSnapBehavior(int direction, int oldRegion, int oldID) {}
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID) {}
		public override void setCurrentlySnappedComponentTo(int id) {}
		public override void snapToDefaultClickableComponent() {}
		public override void snapCursorToCurrentSnappedComponent() {}
	}
}

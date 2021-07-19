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
		public const int MenuScale = 2;

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
		/// <summary>
		/// Offset used for relative positioning of elements aligned to border-safe area.
		/// </summary>
		public Vector2 BorderSafeOffset => new Vector2(
					BorderSafeArea.X - xPositionOnScreen,
					BorderSafeArea.Y - yPositionOnScreen);
		/// <summary>
		/// Offset used for relative positioning of elements aligned to content-safe area.
		/// </summary>
		public Vector2 ContentSafeOffset => new Vector2(
			ContentSafeArea.X - xPositionOnScreen,
			ContentSafeArea.Y - yPositionOnScreen);
		/// <summary>
		/// Position for <see cref="DrawContent"/> for the first element to be drawn to.
		/// </summary>
		protected Vector2 ContentOrigin => Utility.PointToVector2(ContentSafeArea.Location);
		internal static float ShadowOpacity;
		internal static Color ShadowColour;
		internal static Color ShadowColourAlternate;
		internal static Color BodyTextColour;
		internal static Color HeadingTextColour;
		internal static SpriteFont HeadingTextFont => Game1.dialogueFont;
		internal static SpriteFont BodyTextFont => Game1.smallFont;
		internal static SpriteFont TextBoxFont => ModEntry.MonoThinFont;

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
			nameof(Menus.ModInfoMenu), nameof(Menus.CharacterListMenu),
			"AnimationsMenu", "GiftTastesMenu", "DialogueMenu",
			nameof(Menus.SchedulePreviewMenu), nameof(Menus.TileInfoMenu), nameof(Menus.MapMenu),
			"AssetViewMenu",
			nameof(Menus.ProjectViewMenu),
			"FileManagerMenu", "BuildMenu", "OptionsMenu", "HelpMenu"
		};
		internal static readonly Point WidgetIconSize = new Point(16, 16);
		internal static readonly Point WidgetIconSourceOrigin = new Point(16, 148);
		internal enum WidgetSourceIndex
		{
			ViewingLocationWidget
		};

		public virtual void SetDefaults()
		{
			ShadowOpacity = 0.5f;
			ShadowColour = Color.MidnightBlue;
			ShadowColourAlternate = Color.Black;
			BodyTextColour = Game1.textColor;
			HeadingTextColour = Color.White;
		}

		public abstract void RealignElements();

		protected CustomMenu()
		{
			this.SetDefaults();
		}

		public void SnapWithinViewportBounds()
		{
			Point difference = new Point(
				xPositionOnScreen + width - Game1.viewport.Width,
				yPositionOnScreen + height - Game1.viewport.Height);
			if (difference.X > 0)
				xPositionOnScreen -= Math.Max(0, difference.X);
			if (difference.Y > 0)
				yPositionOnScreen -= Math.Max(0, difference.Y);
			this.RealignElements();
		}

		/// <summary>
		/// Returns the source area for the window icon of some <see cref="WindowPage"/> item, indexed by type name.
		/// </summary>
		/// <param name="menuTypeName">Type name for this <see cref="WindowPage"/> object.</param>
		internal static Rectangle GetIconSourceRect(string menuTypeName)
		{
			return new Rectangle(IconSourceIndex.IndexOf(menuTypeName) * IconSize.X, 48, IconSize.X, IconSize.Y);
		}

		protected static Rectangle GetWidgetIconSourceRect(string widgetTypeName)
		{
			if (!Enum.IsDefined(typeof(WidgetSourceIndex), widgetTypeName))
				return Rectangle.Empty;
			int index = (int)Enum.Parse(typeof(WidgetSourceIndex), widgetTypeName);
			return new Rectangle(
				WidgetIconSourceOrigin.X + (index * WidgetIconSize.X),
				WidgetIconSourceOrigin.Y,
				WidgetIconSize.X,
				WidgetIconSize.Y);
		}

		/// <summary>
		/// Draws a simple light transparent square over the bounds of a <see cref="ClickableComponent"/>.
		/// </summary>
		/// <param name="clickable">Clickable item to draw over.</param>
		protected static void DrawHighlight(SpriteBatch b, Rectangle bounds)
		{
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: bounds,
				color: Color.White * 0.3f);
		}

		public virtual int DrawText(SpriteBatch b, Vector2 position, string text, SpriteFont font, Color colour, bool drawShadow = false)
		{
			if (drawShadow)
			{
				Utility.drawTextWithColoredShadow(b: b,
					text: text,
					font: font,
					position: position,
					color: colour,
					shadowColor: ShadowColour);
			}
			else
			{
				b.DrawString(
					spriteFont: font,
					text: text,
					position: position,
					color: colour);
			}
			return (int)font.MeasureString(text).Y + Padding.Y;
		}

		public override void draw(SpriteBatch b)
		{
			ModEntry.Instance.Desktop.DrawHoverText(b, _hoverText);
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
		// NO gamepad support
		// NO mobile phones
		// NO fun for children
		// NO new friends
		public override void automaticSnapBehavior(int direction, int oldRegion, int oldID) {}
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID) {}
		public override void setCurrentlySnappedComponentTo(int id) {}
		public override void snapToDefaultClickableComponent() {}
		public override void snapCursorToCurrentSnappedComponent() {}
	}
}

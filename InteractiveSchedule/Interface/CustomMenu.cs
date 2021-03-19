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

		protected const float ShadowOpacity = 0.5f;
		protected static readonly Color ShadowColour = Color.MidnightBlue;
		protected static float HoverDelayScale = 1f;
		protected static float LeftClickHeldDelayScale = 1f;
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
			nameof(ModInfoMenu), nameof(CharacterListMenu), "ScheduleMenu", "GiftsMenu", "DialogueMenu",
			"FileManagerMenu", "BuildMenu", nameof(TileInfoMenu), nameof(MapViewMenu), "OptionsMenu", "HelpMenu"
		};

		public abstract void SetDefaults();
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
			_parentMenu?.update(time);
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			_parentMenu?.receiveKeyPress(key);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			_parentMenu?.receiveLeftClick(x, y, playSound);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			base.receiveRightClick(x, y, playSound);
			_parentMenu?.receiveRightClick(x, y, playSound);
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			_parentMenu?.leftClickHeld(x, y);
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			_parentMenu?.releaseLeftClick(x, y);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			_parentMenu?.receiveScrollWheelAction(direction);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			_parentMenu?.performHoverAction(x, y);
		}
	}
}

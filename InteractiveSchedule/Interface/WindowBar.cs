﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface
{
	/// <summary>
	/// The header bar of a <see cref="Components.WindowComponent"/>.
	/// Generated by <see cref="WindowPage"/> and paired when one is instantiated.
	/// </summary>
	public class WindowBar : Components.WindowComponent
	{
		/// <summary> Window title to be drawn in the header bar. </summary>
		public string DisplayName;
		public override bool IsSelected =>_childMenu == null || (_childMenu is Components.WindowComponent window && window.IsSelected);
		/// <summary>
		/// Whether the window is collapsed, hiding the paired <see cref="WindowPage"/>.
		/// Window is realigned when set.
		/// See also: <seealso cref="_isMinimised"/>
		/// </summary>
		public bool IsMinimised
		{
			get
			{
				return _isMinimised;
			}
			set
			{
				this.RememberWindowSize();
				int i = (int)MenuButton.Minimise;
				_isMinimised = value;
				if (_isMinimised)
					IsFullscreen = false;
				MenuButtons[i].sourceRect.X = (BarButtonSourceOffset[i] + (_isMinimised ? 1 : 0)) * BarButtonDimensions.X;
				this.RealignElements();
			}
		}
		/// <summary>
		/// Whether the window is temporarily expanded to fill the viewport.
		/// Window is realigned when set.
		/// See also: <seealso cref="_isFullscreen"/>
		/// </summary>
		public bool IsFullscreen
		{
			get
			{
				return _isFullscreen;
			}
			set
			{
				this.RememberWindowSize();
				int i = (int)MenuButton.Window;
				_isFullscreen = value;
				if (_isFullscreen)
					IsMinimised = false;
				MenuButtons[i].sourceRect.X = (BarButtonSourceOffset[i] + (_isFullscreen ? 1 : 0)) * BarButtonDimensions.X;
				this.RealignElements();
			}
		}
		/// <summary> Whether the content is temporarily hidden exclusive of <see cref="IsMinimised"/>. </summary>
		public bool IsContentHidden;
		/// <summary> Whether the child <see cref="WindowPage"/> should be visible and interactible. </summary>
		public bool ShouldDrawChild => !IsMinimised && !IsContentHidden;
		/// <summary> Whether the window bar should have the usual interactible <see cref="MenuButtons"/>.</summary>
		public readonly bool ShouldDrawMenuButtons;
		/// <summary>
		/// Preserved on-screen position and dimensions for this window exclusive of its size when <see cref="IsFullscreen"/>.
		/// <see cref="WindowPage"/> dimensions are reflected here.
		/// </summary>
		public Rectangle AreaBeforeFullscreen;
		/// <summary>The expected height of a <see cref="WindowPage"/> when <see cref="WindowBar.IsFullscreen"/>.</summary>
		public int FullscreenHeight => Game1.viewport.Height - this.height;
		/// <summary> Source area for the header bar window icon also used in <see cref="Taskbar.Icons"/>. </summary>
		public Rectangle IconSource;
		/// <summary>
		/// Window outside border colour.
		/// Matches header bar colour seen in <see cref="ModEntry.Sprites"/> spritesheet and used in <see cref="DrawWindowBarContainer"/>.
		/// </summary>
		public static Color BorderColour;
		/// <summary>
		/// Common window menu buttons appearing in the top-right of this header bar.
		/// Indexed in <see cref="MenuButton"/>.
		/// </summary>
		public readonly List<ClickableTextureComponent> MenuButtons = new List<ClickableTextureComponent>();
		public enum MenuButton
		{
			Minimise, Window, Close
		}

		private bool _isMinimised;
		private bool _isFullscreen;

		private const int ButtonScale = 3;
		private const int ButtonSpacing = 3 * MenuScale;
		private static readonly Point BarButtonDimensions = new Point(9, 9);
		private static readonly Point Dimensions = new Point(200, BarButtonDimensions.Y * ButtonScale);
		private static readonly Point BarButtonAreaSize = new Point(((ButtonSpacing + BarButtonDimensions.X) * ButtonScale), BarButtonDimensions.Y * ButtonScale);
		private static readonly int[] BarButtonSourceOffset = new int[] { 0, 3, 2 };

		public WindowBar(Components.WindowComponent childMenu, Point position, bool shouldDrawMenuButtons = true, bool warningStyle = false) : base()
		{
			_childMenu = childMenu;
			DisplayName = ModEntry.Instance.i18n.Get("ui." + childMenu.GetType().Name.ToLower() + ".title");
			xPositionOnScreen = position.X;
			yPositionOnScreen = position.Y;
			ShouldDrawMenuButtons = shouldDrawMenuButtons;
			this.RealignElements();
		}

		protected override void cleanupBeforeExit()
		{
			MenuButtons.Clear();
			this.ExitChildMenu();
			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();

			// Get default border colour for window pages
			Color[] pixels = new Color[1];
			ModEntry.Sprites.GetData(level: 0, rect: new Rectangle(0, 10, 1, 1), data: pixels, startIndex: 0, elementCount: 1);
			BorderColour = pixels[0];
		}

		public override void RealignElements()
		{
			base.RealignElements();
			this.RememberWindowSize();
			this.SetForChildMenu(childMenu: _childMenu);
			if (_childMenu != null && _childMenu is WindowPage child)
			{
				child.RealignElements();
				child.RealignFloatingButtons();
			}
			this.SetupMenuButtons();
		}

		public Point GetDefaultDimensions()
		{
			if (string.IsNullOrEmpty(DisplayName))
				return Dimensions;

			Vector2 displayNameSize = Game1.smallFont.MeasureString(DisplayName);
			return new Point(
				(int)(Math.Max(Dimensions.X, displayNameSize.X) + (IconSource.Width * MenuScale) + (BarButtonAreaSize.X * 3) + (Padding.X * 3) + 32),
				(int)(Math.Max(Dimensions.Y, displayNameSize.Y) + (Padding.Y * 2)));
		}

		/// <summary>
		/// Preserve position and dimensions for this window.
		/// See also: <seealso cref="AreaBeforeFullscreen"/>
		/// </summary>
		private void RememberWindowSize()
		{
			Point dimensions = this.GetDefaultDimensions();
			AreaBeforeFullscreen = new Rectangle(xPositionOnScreen, yPositionOnScreen, dimensions.X, dimensions.Y);
		}

		public void SetForChildMenu(IClickableMenu childMenu)
		{
			this.SetChildMenu(menu: childMenu);

			string name = _childMenu.GetType().Name;
			IconSource = CustomMenu.GetIconSourceRect(name);
			DisplayName = ModEntry.Instance.i18n.Get("ui." + name.ToLower() + ".title");
			if (!IsContentHidden)
			{
				// Don't undo position changes made when dragging windows
				xPositionOnScreen = _isFullscreen ? Desktop.Taskbar.width : AreaBeforeFullscreen.X;
				yPositionOnScreen = _isFullscreen ? 0 : AreaBeforeFullscreen.Y;
			}
			if (!_isFullscreen && !_isMinimised)
			{
				// Preserve dimensions in case user enters fullscreen mode
				this.RememberWindowSize();
			}
			// Set width to best fit content and context
			width = _isFullscreen
				? Game1.viewport.Width - Desktop.Taskbar.width
				//: _isMinimised
					//? this.GetDefaultDimensions().X
					: AreaBeforeFullscreen.Width;
			height = AreaBeforeFullscreen.Height;
		}

		public void SetupMenuButtons()
		{
			ClickableTextureComponent[] menuButtons = new ClickableTextureComponent[BarButtonSourceOffset.Length];
			for (int i = 0; i < menuButtons.Length; ++i)
			{
				menuButtons[i] = new ClickableTextureComponent(
					bounds: new Rectangle(
						xPositionOnScreen + width - (BarButtonAreaSize.X * 2) + (i * BarButtonDimensions.X * ButtonScale) + (i * ButtonSpacing) - Padding.X,
						yPositionOnScreen + ((height - (BarButtonDimensions.Y * ButtonScale)) / 2),
						BarButtonDimensions.X * ButtonScale,
						BarButtonDimensions.Y * ButtonScale),
					texture: ModEntry.Sprites,
					sourceRect: new Rectangle(
						BarButtonSourceOffset[i] * BarButtonDimensions.X,
						0,
						BarButtonDimensions.X,
						BarButtonDimensions.Y),
					scale: ButtonScale);
				if ((i == (int)MenuButton.Minimise && IsMinimised) || (i == (int)MenuButton.Window && IsFullscreen))
				{
					menuButtons[i].sourceRect.X += BarButtonDimensions.X;
				}
			}
			MenuButtons.Clear();
			MenuButtons.AddRange(menuButtons);
		}

		public void ExitChildMenu()
		{
			if (_childMenu != null)
			{
				Desktop.Children.Remove(_childMenu);
				_childMenu.exitThisMenu();
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);

			if (ShouldDrawMenuButtons)
			{
				if (MenuButtons[(int)MenuButton.Minimise].containsPoint(x, y))
				{
					IsMinimised = !IsMinimised;
				}
				else if (MenuButtons[(int)MenuButton.Window].containsPoint(x, y))
				{
					IsFullscreen = !IsFullscreen;
				}
				else if (MenuButtons[(int)MenuButton.Close].containsPoint(x, y))
				{
					this.exitThisMenu();
				}
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			if (!IsSelected)
				return;

			++_leftClickHeldTimer;
			if (_leftClickHeldTimer > LeftClickHoldDelay * LeftClickHeldDelayScale
				&& (IsContentHidden || this.isWithinBounds(x, y)))
			{
				if (x != Game1.getOldMouseX() || y != Game1.getOldMouseY())
				{
					// Start dragging window when mouse is moved after holding on window bar
					IsContentHidden = true;
				}
				if (IsContentHidden)
				{
					// Drag window around screenspace, but keep window within viewport bounds
					Point minimumPoint = new Point(Desktop.Taskbar.width - (1 * MenuScale), 0);
					Point maximumPoint = new Point(Game1.viewport.Width - 64, Game1.viewport.Height - height);
					Point oldMouse = new Point(Game1.getOldMouseX(), Game1.getOldMouseY());
					Point delta = new Point(x - oldMouse.X, y - oldMouse.Y);
					xPositionOnScreen = Math.Min(maximumPoint.X, Math.Max(minimumPoint.X, xPositionOnScreen + delta.X));
					yPositionOnScreen = Math.Min(maximumPoint.Y, Math.Max(minimumPoint.Y, yPositionOnScreen + delta.Y));
					if (IsFullscreen)
						IsFullscreen = false;
				}
			}

			base.leftClickHeld(x, y);
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (!IsSelected)
				return;

			_leftClickHeldTimer = 0f;
			if (IsSelected && IsContentHidden)
			{
				this.RealignElements();
				IsContentHidden = false;
			}

			base.releaseLeftClick(x, y);
		}

		public static void DrawWindowBarContainer(SpriteBatch b, Rectangle area, Color colour, bool greyed, bool simpleStyle = false, bool drawShadow = false)
		{
			WindowBar.DrawWindowBarContainer(b, x: area.X, y: area.Y, w: area.Width, h: area.Height, colour: colour, greyed: greyed, simpleStyle: simpleStyle, drawShadow: drawShadow);
		}

		public static void DrawWindowBarContainer(SpriteBatch b, int x, int y, int w, int h, Color colour, bool greyed, bool simpleStyle = false, bool drawShadow = false)
		{
			// Draw window drop shadow
			if (drawShadow)
			{
				b.Draw(
					texture: Game1.fadeToBlackRect,
					destinationRectangle: new Rectangle(x - (1 * MenuScale), y + (1 * MenuScale), w, h),
					color: ShadowColour * ShadowOpacity);
			}
			// Draw window bar container
			draw(c: colour);
			// Draw another transparent overlay to grey-out windows
			if (greyed)
			{
				//draw(c: Color.White * 0.25f);
				b.Draw(
					texture: Game1.fadeToBlackRect,
					destinationRectangle: new Rectangle(x, y, w, h),
					color: Color.Black * ShadowOpacity);
			}

			void draw(Color c)
			{
				Point origin = new Point(simpleStyle ? 9 : 0, 9);
				Point sideWidths = new Point(1, 8);
				Point area = new Point(4, 4);
				Point areaScaled = new Point(4 * MenuScale, 4 * MenuScale);

				// top-left
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: new Rectangle(x, y, areaScaled.X, areaScaled.Y),
					sourceRectangle: new Rectangle(origin.X, origin.Y, area.X, area.Y),
					color: c);
				// top-right
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: new Rectangle(x + w - areaScaled.X, y, areaScaled.X, areaScaled.Y),
					sourceRectangle: new Rectangle(origin.X + area.X + sideWidths.X, origin.Y, area.X, area.Y),
					color: c);
				// bottom-left
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: new Rectangle(x, y + h - areaScaled.Y, areaScaled.X, areaScaled.Y),
					sourceRectangle: new Rectangle(origin.X, origin.Y + area.Y + sideWidths.Y, area.X, area.Y),
					color: c);
				// bottom-right
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: new Rectangle(x + w - areaScaled.X, y + h - areaScaled.Y, areaScaled.X, areaScaled.Y),
					sourceRectangle: new Rectangle(origin.X + area.X + 1, origin.Y + area.Y + sideWidths.Y, area.X, area.Y),
					color: c);

				// top
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: new Rectangle(x + areaScaled.X, y, w - (areaScaled.X * 2), areaScaled.Y),
					sourceRectangle: new Rectangle(origin.X + area.X, origin.Y, sideWidths.X, area.Y),
					color: c);
				// bottom
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: new Rectangle(x + areaScaled.X, y + h - areaScaled.Y, w - (areaScaled.X * 2), areaScaled.Y),
					sourceRectangle: new Rectangle(origin.X + area.X, origin.Y + area.Y + sideWidths.Y, sideWidths.X, area.Y),
					color: c);

				// left
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: new Rectangle(x, y + areaScaled.Y, areaScaled.X, h - (areaScaled.Y * 2)),
					sourceRectangle: new Rectangle(origin.X, origin.Y + area.Y, area.X, sideWidths.Y),
					color: c);
				// right
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: new Rectangle(x + w - areaScaled.X, y + areaScaled.Y, areaScaled.X, h - (areaScaled.Y * 2)),
					sourceRectangle: new Rectangle(origin.X + area.X + sideWidths.X, origin.Y + area.Y, area.X, sideWidths.Y),
					color: c);

				// middle
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: new Rectangle(x + areaScaled.X, y + areaScaled.Y, w - (areaScaled.X * 2), h - (areaScaled.Y * 2)),
					sourceRectangle: new Rectangle(origin.X + area.X, origin.Y + area.Y, sideWidths.X, sideWidths.Y),
					color: c);
			}
		}

		private void DrawWindowBar(SpriteBatch b)
		{
			Color colour = IsSelected ? Color.White : Color.LightSlateGray;
			Vector2 position;

			// Draw window bar container
			WindowBar.DrawWindowBarContainer(b, x: xPositionOnScreen, y: yPositionOnScreen, w: width, h: height, colour: Desktop.Taskbar.InterfaceColour, greyed: !IsSelected, simpleStyle: !IsSelected);
			if (ShouldDrawChild)
			{
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: new Rectangle(xPositionOnScreen + (1 * MenuScale), yPositionOnScreen + height - (1 * MenuScale), width: width - (2 * MenuScale), height: 1 * MenuScale),
					sourceRectangle: new Rectangle(1 + (IsSelected ? 0 : 9), 9 + 16 - 2, 1, 1),
					color: colour);
			}

			// Draw window icon
			position = new Vector2(
				xPositionOnScreen + Padding.X,
				yPositionOnScreen + ((height - IconSource.Height * MenuScale) / 2));
			b.Draw(texture: ModEntry.Sprites,
				destinationRectangle: new Rectangle((int)position.X, (int)position.Y, IconSource.Width * MenuScale, IconSource.Height * MenuScale),
				sourceRectangle: IconSource,
				color: colour);

			// Draw window title
			position = new Vector2(
				xPositionOnScreen + (Padding.X * 2) + (IconSource.Width * MenuScale),
				yPositionOnScreen + (Game1.smallFont.MeasureString(DisplayName).Y / MenuScale));
			if (IsSelected)
			{
				Utility.drawTextWithColoredShadow(b: b, text: DisplayName, font: Game1.smallFont, position: position, color: colour, shadowColor: CustomMenu.ShadowColour);
			}
			else
			{
				b.DrawString(spriteFont: Game1.smallFont, text: DisplayName, position: position, color: colour);
			}

			if (!IsContentHidden && ShouldDrawMenuButtons)
			{
				Point mouse = Game1.getMousePosition();
				foreach (ClickableTextureComponent button in MenuButtons)
				{
					// Draw menu buttons
					if (IsSelected)
						button.draw(b);
					else
						button.draw(b, c: colour, layerDepth: 1f);

					// Draw highlight over hovered buttons
					if (button.containsPoint(mouse.X, mouse.Y))
						CustomMenu.DrawHighlight(b, button);
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			this.DrawWindowBar(b);
			base.draw(b);
		}
	}
}

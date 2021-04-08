﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface
{
	/// <summary>
	/// The body contents of a <see cref="WindowComponent"/>.
	/// Generates a paired <see cref="WindowBar"/> when instantiated if not a modal.
	/// </summary>
	public abstract class WindowPage : Components.WindowComponent
	{
		/// <summary>
		/// Contextual popup window elements.
		/// </summary>
		public WindowPage ModalWindow;
		public readonly bool IsModal;
		/// <summary>
		/// Traverses up a level in the page, if the page has some kind of sub-page in addition to its landing page.
		/// No behaviour if <see cref="IsOnHomePage"/>.
		/// </summary>
		public ClickableTextureComponent UpButton;
		/// <summary>
		/// Reference to this window's paired header bar.
		/// </summary>
		public WindowBar WindowBar => _parentMenu is WindowBar ? _parentMenu as WindowBar : null;
		/// <summary> Whether this whole <see cref="WindowPage"/> should be interactible or rendered in any way.</summary>
		public bool ShouldDraw => WindowBar == null || WindowBar.ShouldDrawChild;
		public override bool IsSelected => !Desktop.Children.Any() || this.GetType().Name == Desktop.Children.First().GetType().Name || IsModal;
		/// <summary>
		/// Whether the window is on some sub-page other than its landing page.
		/// </summary>
		public abstract bool IsOnHomePage { get; }
		/// <summary>
		/// Whether to show the <see cref="UpButton"/> used for navigating towards the landing page.
		/// </summary>
		public abstract bool IsUpButtonVisible { get; }
		/// <summary> Whether to enable draw/action behaviours for the sidebar containing items from <see cref="SidebarActionButtons"/>.</summary>
		public abstract bool IsActionButtonSidebarVisible { get; }
		/// <summary>
		/// Sidebar action buttons are a list of any extra feature buttons in the menu,
		/// and are positioned in a sidebar to the right of the window page content.
		/// Draw, hover, and realign behaviours are handled in <see cref="WindowPage"/>, interaction handling should be done per-case.
		/// </summary>
		public List<ClickableTextureComponent> SidebarActionButtons = new List<ClickableTextureComponent>();
		/// <summary>
		/// Floating action buttons are saved as a dictionary of the button itself and its relative position.
		/// Unlike <see cref="SidebarActionButtons"/>, they may be positioned arbitrarily, but the position is still relative to the menu origin.
		/// Draw, hover, and realign behaviours are handled in <see cref="WindowPage"/>, interaction handling should be done per-case.
		/// </summary>
		public Dictionary<ClickableTextureComponent, Point> FloatingActionButtons = new Dictionary<ClickableTextureComponent, Point>();

		public Color PageColour;
		public Color SidebarColour;
		public Color InnerBorderColourSelected;
		public Color InnerBorderColourDeselected;

		/// <summary>
		/// The area of the page outside of the <see cref="ContentSafeArea"/> to position items from <see cref="SidebarActionButtons"/>.
		/// If this and <see cref="UpButton"/> are visible, it will also position itself here.
		/// </summary>
		public Rectangle ActionButtonSidebarArea { get; private set; }
		/// <summary>
		/// Position for <see cref="SidebarActionButtons"/> to be centred within <see cref="ActionButtonSidebarArea"/> if origin is <see cref="Vector2.Zero"/>.
		/// </summary>
		public Point ActionButtonOrigin { get; private set; }
		protected bool ShouldUpdateHoverText => IsSelected && ShouldDraw && string.IsNullOrEmpty(_hoverText);

		protected const float ActionButtonHoverScale = 0.4f;
		protected const int ActionButtonIconSourceY = 74;
		protected static readonly Point UpButtonSize = new Point(16, 16);
		protected static readonly Point ActionButtonSize = new Point(18, 18);
		protected static readonly Point ActionButtonIconSize = new Point(14, 14);
		protected static readonly Point ActionButtonIconOffset = new Point(ActionButtonIconSize.X - ActionButtonSize.X, ActionButtonIconSize.Y - ActionButtonSize.Y);
		private static readonly Rectangle UpButtonSource = new Rectangle(46, 0, UpButtonSize.X, UpButtonSize.Y);
		private static readonly Rectangle UpButtonIconSource = new Rectangle(62, 0, UpButtonSize.X, UpButtonSize.Y);
		private static readonly Rectangle ActionButtonSource = new Rectangle(44, 26, ActionButtonSize.X, ActionButtonSize.Y);
		private const int ActionButtonIconRowCount = 9;
		protected enum ActionButtonIcon
		{
			Return = 0,
			View,
			Schedule,
			ViewSpawn,
			_4,
			Flag,
			FollowPath,
			Grid = ActionButtonIconRowCount,
			TileHighlight,
			PathsLayer,
			Pathing,
			Play,
			Stop,
			Pause,
			TimeStop = ActionButtonIconRowCount * 2,
			_1,
			_2,
			_3,
			Save,
			Load,
			OK,
			Cancel,
			Blank
		}
		protected static readonly Dictionary<string, ActionButtonIcon> ActionButtonIcons = new Dictionary<string, ActionButtonIcon>
		{
			// Menus
			{ nameof(Menus.TileInfoMenu.PathsLayerButton), ActionButtonIcon.PathsLayer },
			{ nameof(Menus.TileInfoMenu.TileHighlightButton), ActionButtonIcon.TileHighlight },
			{ nameof(Menus.TileInfoMenu.GridViewButton), ActionButtonIcon.Grid },
			{ nameof(Menus.CharacterListMenu.GoToButton), ActionButtonIcon.View },
			{ nameof(Menus.CharacterListMenu.ViewSpawnButton), ActionButtonIcon.ViewSpawn },
			{ nameof(Menus.CharacterListMenu.OpenScheduleButton), ActionButtonIcon.Schedule },
			{ nameof(Menus.CharacterListMenu.ReturnFromButton), ActionButtonIcon.Return },
			{ nameof(Menus.MapMenu.WarpHereButton), ActionButtonIcon.Blank },
			{ nameof(Menus.MapMenu.ViewLocationButton), ActionButtonIcon.View },
			{ nameof(Menus.MapMenu.ReturnViewLocationButton), ActionButtonIcon.Return },
			// Modals
			{ nameof(Modals.LocationSelectModal.GoButton), ActionButtonIcon.OK },
		};


		protected WindowPage(Point position, WindowPage modalParent = null) : base()
		{
			IsModal = modalParent != null;
			if (IsModal)
			{
				_parentMenu = modalParent;
			}
			else
			{
				this.GenerateWindowBar(position);
			}
			this.AddActionButtons();
			this.RealignElements();
		}

		protected override void cleanupBeforeExit()
		{
			UpButton = null;
			SidebarActionButtons.Clear();
			FloatingActionButtons.Clear();
			ModalWindow?.exitThisMenu();
			ModalWindow = null;

			base.cleanupBeforeExit();
		}

		protected abstract void ClickUpButton();
		protected virtual void AddActionButtons()
		{
			SidebarActionButtons.Clear();
			FloatingActionButtons.Clear();
		}

		public virtual void RealignFloatingButtons()
		{
			foreach (KeyValuePair<ClickableTextureComponent, Point> button in FloatingActionButtons)
			{
				button.Key.bounds = new Rectangle(
					BorderSafeArea.X + button.Value.X,
					BorderSafeArea.Y + button.Value.Y,
					button.Key.sourceRect.Width * MenuScale,
					button.Key.sourceRect.Height * MenuScale);
			}
		}

		public override void RealignElements()
		{
			// Position
			base.RealignElements();

			if (_parentMenu == null)
				return;

			// Page decoration area
			BorderSafeArea = new Rectangle(
				xPositionOnScreen + (BorderWidth * MenuScale),
				yPositionOnScreen,
				width - (BorderWidth * MenuScale * 2),
				height);

			// Action button sidebar
			int sidebarWidth = SidebarActionButtons.Count == 0 ? 0 : (ActionButtonSize.X * MenuScale) + (Padding.X * BorderWidth);
			ActionButtonSidebarArea = new Rectangle(
				BorderSafeArea.X + BorderSafeArea.Width - sidebarWidth,
				BorderSafeArea.Y,
				sidebarWidth,
				BorderSafeArea.Height);

			// Page content area
			// Don't remove padding from bottom/right (ie. width - Padding.X * 2), this is fine for a little leniency with content width.
			// Just keep in mind that right-aligned content needs to be padded manually.
			ContentSafeArea = new Rectangle(
				BorderSafeArea.X + Padding.X,
				BorderSafeArea.Y + Padding.Y,
				BorderSafeArea.Width - ActionButtonSidebarArea.Width - Padding.X,
				BorderSafeArea.Height - Padding.Y);

			// Up button
			if (UpButton == null)
			{
				UpButton = new ClickableTextureComponent(
					bounds: Rectangle.Empty,
					texture: ModEntry.Sprites,
					sourceRect: UpButtonSource,
					scale: MenuScale);
			}
			UpButton.bounds = new Rectangle(sidebarWidth > 0
					? ActionButtonSidebarArea.X + ((sidebarWidth - (UpButtonSource.Width * MenuScale)) / 2) + (BorderWidth / 2 * MenuScale)
					: BorderSafeArea.X + BorderSafeArea.Width - Padding.X - (UpButtonSource.Width * MenuScale),
				BorderSafeArea.Y + Padding.Y,
				UpButtonSource.Width * MenuScale,
				UpButtonSource.Height * MenuScale);

			// Action buttons
			ActionButtonOrigin = new Point(
				ActionButtonSidebarArea.X + ((ActionButtonSidebarArea.Width - (ActionButtonSize.X * MenuScale) - ActionButtonIconOffset.X * MenuScale) / 2),
				ActionButtonSidebarArea.Y + height - (ActionButtonSize.Y * MenuScale) - (Padding.Y * 2));

			// Sidebar action buttons
			for (int i = 0; i < SidebarActionButtons.Count; ++i)
			{
				SidebarActionButtons[i].bounds = new Rectangle(
					ActionButtonOrigin.X,
					ActionButtonOrigin.Y - (ActionButtonSize.Y * MenuScale * i) - (Padding.Y * i),
					ActionButtonSize.X * MenuScale,
					ActionButtonSize.Y * MenuScale);
			}

			ModalWindow?.RealignElements();

			this.RealignFloatingButtons();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();

			BorderWidth = 2;
			PageColour = Color.PeachPuff;
			SidebarColour = Color.LightSkyBlue;
			InnerBorderColourSelected = Color.White;
			InnerBorderColourDeselected = Color.LightSlateGray;
		}

		/// <summary>
		/// Creates a new <see cref="Interface.WindowBar"/> instance, which in turn pairs itself to this <see cref="WindowPage"/>.
		/// </summary>
		/// <param name="position">Screen coordinates to position the window at once created. Will update the position of this window.</param>
		private void GenerateWindowBar(Point position)
		{
			_ = new WindowBar(childMenu: this, position: position);
		}

		protected static Rectangle GetSourceAreaForActionButtonIcon(ActionButtonIcon whichIcon)
		{
			if (!Enum.IsDefined(enumType: typeof(ActionButtonIcon), value: whichIcon))
			{
				throw new IndexOutOfRangeException();
			}

			Rectangle sourceRect = new Rectangle(
				((int)whichIcon % ActionButtonIconRowCount) * ActionButtonIconSize.X,
				ActionButtonIconSourceY + (((int)whichIcon / ActionButtonIconRowCount) * ActionButtonIconSize.Y),
				ActionButtonIconSize.X,
				ActionButtonIconSize.Y);

			return sourceRect;
		}

		protected ClickableTextureComponent CreateActionButton(string which)
		{
			string key = "action." + this.GetType().Name.ToLower() + "." + which.ToLower() + ".label";
			Translation hoverText = ModEntry.Instance.i18n.Get(key);
			Rectangle sourceRect = GetSourceAreaForActionButtonIcon(whichIcon: ActionButtonIcons[which]);
			return new ClickableTextureComponent(
				bounds: Rectangle.Empty,
				texture: ModEntry.Sprites,
				sourceRect: sourceRect,
				scale: MenuScale)
			{
				hoverText = hoverText.HasValue() ? hoverText : null
			};
		}

		public void CentreInParent()
		{
			if (_parentMenu != null)
			{
				xPositionOnScreen = _parentMenu.xPositionOnScreen + ((_parentMenu.width - width) / 2);
				yPositionOnScreen = _parentMenu.yPositionOnScreen + ((_parentMenu.height - height) / 2);
			}
		}

		public override bool isWithinBounds(int x, int y)
		{
			return ShouldDraw && base.isWithinBounds(x, y);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			ModalWindow?.performHoverAction(x, y);

			if (ShouldDraw && ModalWindow == null)
			{
				if (IsActionButtonSidebarVisible)
				{
					foreach (ClickableTextureComponent button in SidebarActionButtons)
					{
						button.tryHover(x, y, maxScaleIncrease: ActionButtonHoverScale);
						if (button.containsPoint(x, y))
						{
							_hoverText = button.hoverText;
						}
					}
				}

				foreach (ClickableTextureComponent button in FloatingActionButtons.Keys)
				{
					button.tryHover(x, y, maxScaleIncrease: ActionButtonHoverScale);
					if (button.containsPoint(x, y))
					{
						_hoverText = button.hoverText;
					}
				}

				UpButton.tryHover(x, y, maxScaleIncrease: ActionButtonHoverScale);
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (!IsSelected || !ShouldDraw)
				return;

			base.receiveKeyPress(key);
			ModalWindow?.receiveKeyPress(key);

			// TODO: ModalWindow checks in WindowPage.receiveKeyPress()
			if (Game1.keyboardDispatcher.Subscriber != null)
			{
				return;
			}
			if (!IsOnHomePage)
			{
				this.ClickUpButton();
			}
			/*else if (WindowBar != null)
			{
				if (WindowBar.IsFullscreen)
					WindowBar.IsFullscreen = false;
				else
					WindowBar.IsMinimised = true;
			}*/
		}

		/// <summary>
		/// Children should call base method before custom behaviour.
		/// </summary>
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			// TODO: FIX: Resolve leftclick occurring for UpButton and then for any elements appearing beneath it

			base.receiveLeftClick(x, y, playSound);
			ModalWindow?.receiveLeftClick(x, y, playSound);

			if (IsSelected && ShouldDraw && ModalWindow == null && !IsOnHomePage && UpButton != null && UpButton.containsPoint(x, y))
			{
				this.ClickUpButton();
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			ModalWindow?.update(time);
		}

		public int DrawHeading(SpriteBatch b, Vector2 position, string text, bool drawBackground)
		{
			if (drawBackground)
			{
				b.Draw(
					texture: Game1.fadeToBlackRect,
					destinationRectangle: new Rectangle(BorderSafeArea.X, BorderSafeArea.Y, BorderSafeArea.Width - ActionButtonSidebarArea.Width, (int)(position.Y - BorderSafeArea.Y + HeadingTextFont.MeasureString(text).Y + Padding.Y)),
					color: SidebarColour * ShadowOpacity);
			}
			text = Game1.parseText(text, whichFont: BodyTextFont, width: (int)(ContentSafeArea.Width - (position.X - ContentSafeArea.X)));
			Utility.drawTextWithColoredShadow(b,
				text: text,
				font: HeadingTextFont,
				position: position,
				color: HeadingTextColour,
				shadowColor: ShadowColour * ShadowOpacity);
			return (int)HeadingTextFont.MeasureString(text).Y + (Padding.Y * 2);
		}

		public int DrawSubheading(SpriteBatch b, Vector2 position, string text)
		{
			text = Game1.parseText(text, whichFont: BodyTextFont, width: (int)(ContentSafeArea.Width - (position.X - ContentSafeArea.X)));
			Utility.drawTextWithColoredShadow(b,
				text: text,
				font: BodyTextFont,
				position: position,
				color: HeadingTextColour,
				shadowColor: ShadowColour * ShadowOpacity);
			return (int)BodyTextFont.MeasureString(text).Y + Padding.Y;
		}

		public int DrawText(SpriteBatch b, Vector2 position, string text)
		{
			text = Game1.parseText(text, whichFont: BodyTextFont, width: (int)(ContentSafeArea.Width - (position.X - ContentSafeArea.X)));
			return base.DrawText(b, position: position, text: text, font: BodyTextFont, colour: BodyTextColour);
		}

		public void DrawActionButton(SpriteBatch b, ClickableTextureComponent button)
		{
			// Action button
			b.Draw(
				texture: ModEntry.Sprites,
				position: new Vector2(
					button.bounds.X + (ActionButtonIconOffset.X * MenuScale / 2),
					button.bounds.Y + (ActionButtonIconOffset.Y * MenuScale / 2)),
				sourceRectangle: ActionButtonSource,
				color: Desktop.Taskbar.InterfaceColour,
				rotation: 0f, origin: Vector2.Zero, scale: MenuScale, effects: SpriteEffects.None, layerDepth: 1f);
			// Action button icon
			button.draw(b);
		}

		public void DrawActionButtonSidebar(SpriteBatch b)
		{
			b.Draw(texture: Game1.fadeToBlackRect,
				destinationRectangle: ActionButtonSidebarArea,
				color: SidebarColour);
			Desktop.DrawLine(b,
				colour: Color.White * 0.75f,
				startPosition: new Point(ActionButtonSidebarArea.X, ActionButtonSidebarArea.Y),
				length: ActionButtonSidebarArea.Height,
				width: BorderWidth / 2 * MenuScale,
				isHorizontal: false);

			foreach (ClickableTextureComponent button in SidebarActionButtons)
			{
				this.DrawActionButton(b, button: button);
			}
		}

		/// <summary>
		/// Should be called after child draw behaviour so as to draw above custom elements.
		/// </summary>
		public void DrawFloatingActionButtons(SpriteBatch b)
		{
			foreach (ClickableTextureComponent button in FloatingActionButtons.Keys)
			{
				this.DrawActionButton(b, button: button);
			}
		}

		protected void DarkenForModal(SpriteBatch b)
		{
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: BorderSafeArea,
				color: Color.Black * 0.5f);
		}

		public void DrawWindow(SpriteBatch b)
		{
			int lineWidth = BorderWidth / 2 * MenuScale;

			// Window border
			b.Draw( // outer:
				texture: Game1.fadeToBlackRect,
				destinationRectangle: new Rectangle(xPositionOnScreen, yPositionOnScreen - (lineWidth * 2), width, height + (lineWidth * 4)),
				color: WindowBar.BorderColour);
			b.Draw( // inner:
				texture: Game1.fadeToBlackRect,
				destinationRectangle: new Rectangle(xPositionOnScreen + lineWidth, yPositionOnScreen - lineWidth, width - (lineWidth * 2), height + (lineWidth * 2)),
				color: IsSelected ? InnerBorderColourSelected : InnerBorderColourDeselected);

			// Background fill colour
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: BorderSafeArea,
				color: PageColour);
		}

		public abstract void DrawContent(SpriteBatch b);

		public override void draw(SpriteBatch b)
		{
			// Draw window
			if (ShouldDraw)
			{
				this.DrawWindow(b);
				if (IsActionButtonSidebarVisible && SidebarActionButtons.Any())
				{
					this.DrawActionButtonSidebar(b);
				}
			}

			// Draw window bar
			if (WindowBar != null)
			{
				WindowBar.draw(b);
			}

			if (!ShouldDraw)
				return;

			// Draw custom window content
			this.DrawContent(b);

			// Draw up button
			if (ShouldDraw && IsUpButtonVisible && ModalWindow == null)
			{
				// container
				UpButton.draw(b, c: Desktop.Taskbar.InterfaceColour, layerDepth: 1f);
				// icon
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: UpButton.bounds,
					sourceRectangle: UpButtonIconSource,
					color: Color.White);
			}

			if (ModalWindow != null)
			{
				this.DarkenForModal(b);
				ModalWindow.draw(b);
			}

			this.DrawFloatingActionButtons(b);

			base.draw(b); // Hover text
		}
	}
}

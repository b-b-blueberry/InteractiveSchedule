using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface
{
	public class Taskbar : CustomMenu
	{
		public Desktop Desktop => ModEntry.Instance.Desktop;
		public ClickableTextureComponent ExpandButton;
		public ClickableTextureComponent AvatarButton;
		public ClickableTextureComponent MuteButton;
		public readonly List<ClickableTextureComponent> Icons = new List<ClickableTextureComponent>();
		public bool IsExpanded;

		public Color BackgroundColour;
		public Color InterfaceColour;

		private float _xTranslateScale;

		private const float XTranslateRate = 0.08f;
		private static readonly Rectangle ExpandButtonSource = new Rectangle(36, 9, 9, 16);
		private static readonly Rectangle ExpandArrowSource = new Rectangle(46, 17, 5, 8);
		private static readonly Rectangle AvatarButtonSource = new Rectangle(0, 25, 22, 22);
		private static readonly Rectangle MuteButtonSource = new Rectangle(78, 0, 12, 12);
		private static readonly List<string> IconsToAdd = new List<string>
		{
			nameof(Menus.CharacterListMenu),
			"AnimationsMenu", "GiftTastesMenu", "DialogueMenu",
			nameof(Menus.SchedulePreviewMenu), nameof(Menus.TileInfoMenu), nameof(Menus.MapMenu),
			"AssetViewMenu",
			nameof(Menus.ProjectViewMenu),
			"FileManagerMenu", "BuildMenu", "OptionsMenu", "HelpMenu"
		};

		public Taskbar()
		{
			ModEntry.Instance.Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
			this.SetupTaskbarButtons();
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();

			ModEntry.Instance.Helper.Events.Display.RenderedWorld -= Display_RenderedWorld;
			ModEntry.Instance.SetActiveState(active: false);
		}

		protected override void cleanupBeforeExit()
		{
			ModEntry.Instance.Helper.Events.Display.RenderedWorld -= Display_RenderedWorld;

			this.SetActiveState(active: false);
			ExpandButton = null;
			AvatarButton = null;
			MuteButton = null;
			Icons.Clear();

			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			BackgroundColour = Color.MediumSlateBlue * 0.15f;
			//InterfaceColour = new Color(136, 211, 255);
			InterfaceColour = Color.White;
		}

		public void SetupTaskbarButtons()
		{
			Icons.Clear();

			// Show/hide tab button
			ExpandButton = new ClickableTextureComponent(
				name: nameof(ExpandButton),
				bounds: Rectangle.Empty,
				label: null,
				hoverText: null,
				texture: ModEntry.Sprites,
				sourceRect: ExpandButtonSource,
				scale: MenuScale);

			// Main icon
			AvatarButton = new ClickableTextureComponent(
				name: nameof(AvatarButton),
				bounds: Rectangle.Empty,
				label: null,
				hoverText: null,
				texture: ModEntry.Sprites,
				sourceRect: AvatarButtonSource,
				scale: MenuScale);
			Icons.Add(AvatarButton);
			
			// Taskbar icons
			foreach (string name in IconsToAdd)
			{
				Icons.Add(new ClickableTextureComponent(
					name: name,
					bounds: Rectangle.Empty,
					label: null,
					hoverText: ModEntry.Instance.i18n.Get("ui." + name.ToLower() + ".title"),
					texture: ModEntry.Sprites,
					sourceRect: CustomMenu.GetIconSourceRect(name),
					scale: MenuScale));
			}

			// Mute icon
			MuteButton = new ClickableTextureComponent(
				name: nameof(MuteButton),
				bounds: Rectangle.Empty,
				label: null,
				hoverText: null,
				texture: ModEntry.Sprites,
				sourceRect: MuteButtonSource,
				scale: MenuScale);
			Icons.Add(MuteButton);

			this.RealignElements();
		}

		public override void RealignElements()
		{
			// Menu dimensions
			width = (IconSize.Y * MenuScale) + (Padding.X * 2);
			height = Game1.viewport.Height;
			xPositionOnScreen = (int)(0 - width + (width * _xTranslateScale));
			yPositionOnScreen = 0;

			// Main icon
			AvatarButton.bounds = new Rectangle(
				xPositionOnScreen + (1 * MenuScale),
				yPositionOnScreen + (1 * MenuScale) + Padding.Y,
				AvatarButton.sourceRect.Width * MenuScale,
				AvatarButton.sourceRect.Height * MenuScale);

			// Show/hide button
			ExpandButton.bounds = new Rectangle(
				xPositionOnScreen + width,
				AvatarButton.bounds.Y + ((AvatarButtonSource.Height - ExpandButtonSource.Height) / 2 * MenuScale),
				ExpandButtonSource.Width * MenuScale,
				ExpandButtonSource.Height * MenuScale);

			// Taskbar icons, except for first (avatar) and last (mute)
			for (int i = 1; i < Icons.Count - 1; ++i)
			{
				Icons[i].bounds = new Rectangle(
					xPositionOnScreen + Padding.X,
					AvatarButton.bounds.Y + (Padding.Y * 3) + ((AvatarButton.bounds.Height + (Padding.Y / 3)) * i),
					IconSize.X * MenuScale,
					IconSize.Y * MenuScale);
			}

			// Mute icon
			MuteButton.bounds = new Rectangle(
				xPositionOnScreen + Padding.X,
				yPositionOnScreen + height - (MuteButtonSource.Height * MenuScale) - (Padding.Y * 2),
				MuteButtonSource.Width * MenuScale,
				MuteButtonSource.Height * MenuScale);
		}

		/// <summary>
		/// Enables or disables taskbar features, hiding it offscreen if disabled.
		/// </summary>
		/// <param name="active">Whether to enable or disable the taskbar.</param>
		public void SetActiveState(bool active)
		{
			IsExpanded = active;
			ModEntry.Instance.SetActiveState(active: active);
			_xTranslateScale += (active ? 1 : -1) * XTranslateRate;
		}

		/// <summary>
		/// Check whether any taskbar icons are under the cursor coordinates provided, and if so, perform on-click behaviours.
		/// </summary>
		internal IClickableMenu TryClickTaskbarIcon(int x, int y)
		{
			// Clicking taskbar icons
			if (Icons.FirstOrDefault(i => i.containsPoint(x, y)) is ClickableTextureComponent icon && icon != null)
			{
				return this.ClickTaskbarIcon(typeName: icon.name);
			}
			return null;
		}

		internal IClickableMenu ClickTaskbarIcon(string typeName, bool? forceSelected = null)
		{
			IClickableMenu menu = Desktop.Children.FirstOrDefault(child => typeName == child.GetType().Name);
			if (menu == null)
			{
				menu = this.CreateNewMenu(typeName);
			}
			else if (menu.GetParentMenu() is WindowBar windowBar)
			{
				// Redirect to existing menu windows if they exist, or force select menus
				if (windowBar.IsSelected)
				{
					windowBar.IsMinimised = !windowBar.IsMinimised;
				}
				else if (forceSelected.HasValue && forceSelected.Value)
				{
					windowBar.IsMinimised = false;
				}
				if (!forceSelected.HasValue || forceSelected.Value)
				{
					Desktop.SelectedChildIndex = Desktop.Children.IndexOf(menu);
				}
			}
			return menu;
		}

		/// <summary>
		/// Creates and redirects window selection to a new <see cref="IClickableMenu"/> on the <see cref="Desktop"/>.
		/// </summary>
		/// <param name="typeName">Name of the menu type to create, as stored in elements of <see cref="Icons"/>, indexed by <see cref="IconsToAdd"/>.</param>
		public IClickableMenu CreateNewMenu(string typeName)
		{
			// Add a new menu window to the desktop if no instance currently exists
			IClickableMenu firstChild = Desktop.Children.FirstOrDefault();
			Point position = firstChild != null
				? new Point(firstChild.xPositionOnScreen + (25 * MenuScale), firstChild.yPositionOnScreen)
				: new Point(width + (50 * MenuScale), 10 * MenuScale);

			Type thisType = this.GetType();
			Type type = Type.GetType(thisType.AssemblyQualifiedName.Replace(thisType.Name, "Menus." + typeName));
			IClickableMenu menu = (IClickableMenu)type
				.GetConstructor(new Type[] { typeof(Point) })
				.Invoke(new object[] { position });

			if (menu != null)
			{
				// Register our new menu to the desktop
				Desktop.Children.Add(menu);
				Desktop.SelectedChildIndex = Desktop.Children.IndexOf(menu);
			}

			return menu;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (_xTranslateScale > 0f && _xTranslateScale < 1f)
			{
				return;
			}

			// Clicking show/hide button
			if (ExpandButton.containsPoint(x, y))
			{
				this.SetActiveState(!IsExpanded);
			}

			// Clicking main icon
			if (AvatarButton.containsPoint(x, y))
			{
				IClickableMenu menu = Desktop.Children.FirstOrDefault(child => nameof(Menus.ModInfoMenu) == child.GetType().Name);
				if (menu == null)
				{
					Point position = new Point(width + (75 * MenuScale), 15 * MenuScale);
					menu = new Menus.ModInfoMenu(position);
					((Menus.ModInfoMenu)menu).WindowBar.RealignElements();
					Desktop.Children.Insert(0, menu);
				}
				else
				{
					// Redirect to existing menu windows if they exist
					if (menu.GetParentMenu() is WindowBar windowBar)
					{
						windowBar.IsMinimised = false;
					}
				}
				return;
			}

			// Clicking mute icon
			if (MuteButton.containsPoint(x, y))
			{
				Desktop.IsMuted = !Desktop.IsMuted;
				MuteButton.sourceRect.X = MuteButtonSource.X + (Desktop.IsMuted ? MuteButtonSource.Width : 0);
				return;
			}

			// Taskbar icons
			this.TryClickTaskbarIcon(x, y);

			base.receiveLeftClick(x, y, playSound);
		}

		public override void performHoverAction(int x, int y)
		{
			for (int i = 1; i < Icons.Count; ++i)
			{
				Icons[i].tryHover(x, y, maxScaleIncrease: 0.5f);
			}

			base.performHoverAction(x, y);
		}

		public override void update(GameTime time)
		{
			base.update(time);

			if ((!IsExpanded && _xTranslateScale > 0f) || (IsExpanded && _xTranslateScale < 1f))
			{
				_xTranslateScale = Math.Max(0f, Math.Min(1f, _xTranslateScale + ((!IsExpanded ? -1 : 1) * XTranslateRate)));
				this.RealignElements();
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.RealignElements();
		}

		private static void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
		{
			if (Game1.currentLocation == null)
			{
				return;
			}

			/*
			switch (ModEntry.Instance._state)
			{
				case ModEntry.States.Path:
				{
					if (ModEntry.Instance._schedulePath.Path.Count > 0)
					{
						this.DrawSchedulePath(e.SpriteBatch);
					}
				}
				break;
			}
			*/
			// Fade out background
			if (ModEntry.Instance.Desktop.Taskbar?._xTranslateScale > 0f)
			{
				e.SpriteBatch.Draw(
					texture: Game1.fadeToBlackRect,
					destinationRectangle: Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea(),
					color: ModEntry.Instance.Desktop.Taskbar.BackgroundColour * ModEntry.Instance.Desktop.Taskbar._xTranslateScale);
			}
		}

		private void DrawExpandButton(SpriteBatch b)
		{
			ExpandButton.draw(b, c: InterfaceColour, layerDepth: 1f);
			b.Draw(texture: ModEntry.Sprites,
				destinationRectangle: new Rectangle(
					ExpandButton.bounds.X + ((ExpandButtonSource.Width - ExpandArrowSource.Width) / 2 * MenuScale),
					ExpandButton.bounds.Y + ((ExpandButtonSource.Height - ExpandArrowSource.Height) / 2 * MenuScale),
					ExpandArrowSource.Width * MenuScale,
					ExpandArrowSource.Height * MenuScale),
				sourceRectangle: ExpandArrowSource,
				color: Color.White,
				rotation: 0f,
				origin: Vector2.Zero,
				effects: IsExpanded ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				layerDepth: 1f);
		}

		internal void DrawAvatarButton(SpriteBatch b, Rectangle destinationRectangle)
		{
			Rectangle sourceRectangle = AvatarButton.sourceRect;

			// Shadow
			sourceRectangle.X += sourceRectangle.Width;
			b.Draw(
				texture: AvatarButton.texture,
				destinationRectangle: destinationRectangle,
				sourceRectangle: sourceRectangle,
				color: ShadowColour * ShadowOpacity);

			// Icon
			sourceRectangle.X -= sourceRectangle.Width;
			b.Draw(
				texture: AvatarButton.texture,
				destinationRectangle: destinationRectangle,
				sourceRectangle: sourceRectangle,
				color: Color.White);
		}

		public override void draw(SpriteBatch b)
		{
			try
			{
				// Show/hide tab button
				this.DrawExpandButton(b);

				if (_xTranslateScale > 0f)
				{
					WindowBar.DrawWindowBarContainer(b, x: xPositionOnScreen, y: yPositionOnScreen, w: width, h: height, colour: InterfaceColour, greyed: false, simpleStyle: false);

					// Main icon
					this.DrawAvatarButton(b, AvatarButton.bounds);

					// Taskbar icons, except for first (avatar) and last (mute)
					for (int i = 1; i < Icons.Count - 1; ++i)
					{
						Rectangle area = new Rectangle(Icons[i].bounds.X - (Padding.X / 2), Icons[i].bounds.Y - Padding.Y, Icons[i].bounds.Width + Padding.X, Icons[i].bounds.Height + (Padding.Y * 2));
						WindowBar.DrawWindowBarContainer(b, area: area, colour: InterfaceColour, greyed: false, simpleStyle: true, drawShadow: true);
						Icons[i].draw(b);
					}

					// Mute icon
					MuteButton.draw(b);
				}

				base.draw(b);
			}
			catch (Exception e)
			{
				Log.E(e.ToString());
				this.emergencyShutDown();
				this.exitThisMenuNoSound();
			}
		}
	}
}

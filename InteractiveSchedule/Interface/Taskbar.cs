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

		private static readonly float _xTranslateRate = 0.08f;
		private static readonly Rectangle ExpandButtonSource = new Rectangle(36, 9, 9, 16);
		private static readonly Rectangle ExpandArrowSource = new Rectangle(46, 17, 5, 8);
		private static readonly Rectangle AvatarButtonSource = new Rectangle(0, 25, 22, 22);
		private static readonly Rectangle MuteButtonSource = new Rectangle(78, 0, 12, 12);
		private static readonly List<string> IconsToAdd = new List<string>
		{
			nameof(CharacterListMenu), "ScheduleMenu", "DialogueMenu", "FileManagerMenu",
			nameof(TileInfoMenu), nameof(MapViewMenu), "OptionsMenu", "HelpMenu"
		};

		public Taskbar()
		{
			ModEntry.Instance.Helper.Events.Display.RenderedWorld += this.Display_RenderedWorld;
			this.SetupTaskbarButtons();
		}

		protected override void cleanupBeforeExit()
		{
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

		public void SetActiveState(bool active)
		{
			IsExpanded = active;
			Game1.activeClickableMenu = active ? Desktop : null;
			Game1.displayHUD = !active;
			Game1.isTimePaused = active;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			// Clicking show/hide button
			if (ExpandButton.containsPoint(x, y))
			{
				this.SetActiveState(!IsExpanded);
				return;
			}

			// Clicking main icon
			if (AvatarButton.containsPoint(x, y))
			{
				IClickableMenu menu = Desktop.Children.FirstOrDefault(child => nameof(ModInfoMenu) == child.GetType().Name);
				if (menu == null)
				{
					Point position = new Point(width + (75 * MenuScale), 15 * MenuScale);
					menu = new ModInfoMenu(position);
					((ModInfoMenu)menu).WindowBar.RealignElements();
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

			// Clicking taskbar icons
			if (Icons.FirstOrDefault(i => i.containsPoint(x, y)) is ClickableTextureComponent icon && icon != null)
			{
				IClickableMenu menu = Desktop.Children.FirstOrDefault(child => icon.name == child.GetType().Name);
				if (menu == null)
				{
					// Add a new menu window to the desktop if no instance currently exists
					IClickableMenu lastChild = Desktop.Children.LastOrDefault();
					Point position = lastChild != null
						? new Point(lastChild.xPositionOnScreen + (25 * MenuScale), lastChild.yPositionOnScreen + (25 * MenuScale))
						: new Point(width + (50 * MenuScale), 50 * MenuScale);

					if (icon.name == nameof(CharacterListMenu))
					{
						menu = new CharacterListMenu(position: position);
					}
					else if (icon.name == nameof(TileInfoMenu))
					{
						menu = new TileInfoMenu(position: position);
					}
					else if (icon.name == nameof(MapViewMenu))
					{
						menu = new MapViewMenu(position: position);
					}

					if (menu != null)
					{
						// Resize the window bar once content has been loaded
						menu.xPositionOnScreen -= Math.Max(0, Game1.viewport.Width - (menu.xPositionOnScreen + menu.width));
						menu.yPositionOnScreen -= Math.Max(0, Game1.viewport.Height - (menu.yPositionOnScreen + menu.height));
						((WindowPage)menu).WindowBar.RealignElements();

						// Register our new menu to the desktop
						Desktop.Children.Add(menu);
					}
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
				_xTranslateScale = Math.Max(0f, Math.Min(1f, _xTranslateScale + ((!IsExpanded ? -1 : 1) * _xTranslateRate)));
				this.RealignElements();
			}
		}

		private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
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
			if (_xTranslateScale > 0f)
			{
				e.SpriteBatch.Draw(
					texture: Game1.fadeToBlackRect,
					destinationRectangle: Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea(),
					color: BackgroundColour * _xTranslateScale);
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
	}
}

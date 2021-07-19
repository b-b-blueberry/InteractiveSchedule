using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace InteractiveSchedule.Interface
{
	/// <summary>
	/// Desktop window manager handling all <see cref="WindowPage"/> objects, <see cref="Interface.Taskbar"/> instance
	/// </summary>
	public class Desktop : CustomMenu
	{
		/// <summary>
		/// <see cref="Interface.Taskbar"/> object handling creation of new instances of <see cref="WindowPage"/> objects from <see cref="Taskbar.Icons"/>.
		/// </summary>
		public Taskbar Taskbar { get; private set; }
		/// <summary>
		/// List of all menu elements to be updated
		/// </summary>
		public readonly List<IClickableMenu> Children = new List<IClickableMenu>();
		public int SelectedChildIndex;
		public readonly List<CustomMenu> Widgets = new List<CustomMenu>();
		/// <summary>
		/// Collection of notifications to show to the user for some duration before being removed.
		/// </summary>
		public readonly Queue<KeyValuePair<string, float>> Notifications = new Queue<KeyValuePair<string, float>>();
		public enum NotificationDuration
		{
			Short = 300,
			Long = 800,
		}
		/// <summary>
		/// Whether to play menu interaction sounds.
		/// </summary>
		public bool IsMuted;
		public bool IsDesktopActive => _isEnabled && Taskbar != null && Taskbar.IsExpanded && Game1.game1.IsActive;

		internal static readonly Rectangle TileHighlightGreenSource = new Rectangle(194, 388, 16, 16);
		internal static readonly Rectangle TileHighlightRedSource = new Rectangle(210, 388, 16, 16);
		internal static readonly Rectangle TileHighlightDoorGreenSource = new Rectangle(226, 388, 16, 16);
		internal static readonly Rectangle TileHighlightDoorRedSource = new Rectangle(242, 388, 16, 16);

		internal enum Cursor
		{
			Pointer,
			Crosshair,
			Caret,
			Finger,
			Flag,
		}
		internal Cursor _mouseCursor;
		internal Vector2 _cameraDragOrigin;
		private IModHelper Helper => ModEntry.Instance.Helper;
		private static readonly Rectangle CursorSource = new Rectangle(0, 60, 13, 13);
		private static readonly Rectangle CameraDragPinSource = new Rectangle(52, 17, 8, 9);
		private string _oldHoverText;

		private static float HoverDelayScale;
		private static float LeftClickHeldDelayScale;
		private static float RightClickHeldDelayScale;
		private static float LeftClickHoldDelay;
		private static float RightClickHoldDelay;

		private static float LeftClickHeldTimer;
		private static float RightClickHeldTimer;
		public bool IsLeftClickHeld => LeftClickHeldTimer > LeftClickHoldDelay * LeftClickHeldDelayScale;
		public bool IsRightClickHeld => RightClickHeldTimer > RightClickHoldDelay * RightClickHeldDelayScale;

		/// <summary>
		/// Whether all features of the desktop are enabled.
		/// </summary>
		private bool _isEnabled = true;


		public Desktop()
		{
			this.Reset();
			Game1.onScreenMenus.Add(this);
			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			Helper.Events.Input.ButtonReleased += Input_ButtonReleased;
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();

			Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;
			Helper.Events.Input.ButtonReleased -= Input_ButtonReleased;
			ModEntry.Instance.SetActiveState(active: false);
			Game1.onScreenMenus.Remove(this);
			Game1.activeClickableMenu = null;
		}

		protected override void cleanupBeforeExit()
		{
			this.SetFarmerVisibility(visible: true);
			this.ReturnFromViewLocation();

			Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;
			Helper.Events.Input.ButtonReleased -= Input_ButtonReleased;
			Taskbar = null;
			Children.Clear();
			Widgets.Clear();
			Game1.onScreenMenus.Remove(this);
			Game1.activeClickableMenu = null;

			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			LeftClickHeldDelayScale = 1f;
			RightClickHeldDelayScale = 1f;
			HoverDelayScale = 1f;
			LeftClickHoldDelay = 1f;
			RightClickHoldDelay = 1f;
		}

		public override void RealignElements() {}

		/// <summary>
		/// Clears all <see cref="Children"/> and replaces <see cref="Taskbar"/>.
		/// </summary>
		public void Reset()
		{
			Children.Clear();
			Widgets.Clear();
			this.AddTaskbar();
		}

		/// <summary>
		/// Replaces <see cref="Taskbar"/> with some new <see cref="Interface.Taskbar"/>
		/// </summary>
		private void AddTaskbar(Taskbar taskbar = null)
		{
			Taskbar = taskbar ?? new Taskbar();
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			Taskbar.gameWindowSizeChanged(oldBounds, newBounds);
			foreach (IClickableMenu child in Children)
			{
				if (child is CustomMenu)
					((CustomMenu)child).RealignElements();
			}
			foreach (CustomMenu widget in Widgets)
			{
				widget.RealignElements();
			}
		}

		public override void update(GameTime time)
		{
			if (!Game1.game1.IsActive || !_isEnabled)
				return;

			if (Taskbar.IsExpanded)
			{
				// Reorder the list of children to bring recently-selected children to the front
				if (SelectedChildIndex > 0 && Children.Count > 1)
				{
					IClickableMenu child = Children[SelectedChildIndex];
					Children.RemoveAt(SelectedChildIndex);
					Children.Insert(0, child);
				}
				SelectedChildIndex = 0;

				if (RightClickHeldTimer > RightClickHoldDelay * RightClickHeldDelayScale)
				{
					// Drag camera with RightMouse held

					const int dragUnit = Game1.tileSize;
					const float dragMaxRange = 4f;
					const float dragScale = dragMaxRange * dragUnit;
					const float dragVelocity = 16f;

					int mouseX = Game1.getOldMouseX(ui_scale: false);
					int mouseY = Game1.getOldMouseY(ui_scale: false);

					Vector2 cameraVector = new Vector2(
						mouseX - _cameraDragOrigin.X,
						mouseY - _cameraDragOrigin.Y);
					Vector2 cameraMagnitude = cameraVector * (1f / dragScale);
					Vector2 cameraMotion = cameraMagnitude * dragVelocity;

					Game1.panScreen((int)cameraMotion.X, (int)cameraMotion.Y);
				}
			}

			base.update(time);
			Taskbar.update(time);

			if (Taskbar.IsExpanded)
			{
				for (int i = Children.Count - 1; i >= 0; --i)
					Children[i].update(time);
				for (int i = Widgets.Count - 1; i >= 0; --i)
					Widgets[i].update(time);
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (Taskbar == null)
				return;

			try
			{
				_mouseCursor = (int)Cursor.Pointer;

				// Draw all children
				if (Taskbar.IsExpanded)
				{
					// Draw widgets under children
					for (int i = Widgets.Count - 1; i >= 0; --i)
						Widgets[i].draw(b);
					for (int i = Children.Count - 1; i >= 0; --i)
						Children[i].draw(b);
				}

				// Unique draw behaviours
				switch (ModEntry.Instance._state)
				{
					case ModEntry.States.Path:
					{

					}
					break;
				}

				// Draw taskbar
				Taskbar.draw(b);

				// Draw extra info popups
				this.DrawInfoBubbles(b);

				base.draw(b);

				// Mouse cursor
				if (Taskbar.IsExpanded)
				{
					this.DrawCustomCursor(b);
				}
				else
				{
					Game1.mouseCursorTransparency = 1f;
					this.drawMouse(b);
				}
			}
			catch (Exception e)
			{
				Log.E(e.ToString());
				this.emergencyShutDown();
				this.exitThisMenuNoSound();
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (!IsDesktopActive)
				return;

			bool isExitKey = key == Keys.Escape
				|| Game1.options.doesInputListContain(Game1.options.journalButton, key)
				|| Game1.options.doesInputListContain(Game1.options.menuButton, key);
			if (isExitKey)
			{
				bool selectedWindowIsInactive = Children.First() is WindowPage windowPage && (windowPage.IsOnHomePage || windowPage.WindowBar.IsMinimised);
				if ((Children.Count == 0 || selectedWindowIsInactive) && Game1.keyboardDispatcher.Subscriber == null)
				{
					Taskbar.SetActiveState(active: false);
				}
			}

			base.receiveKeyPress(key);
			Taskbar.receiveKeyPress(key);
			foreach (IClickableMenu child in Children.ToList())
				child.receiveKeyPress(key);
			for (int i = Widgets.Count - 1; i >= 0; --i)
				Widgets[i].receiveKeyPress(key);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			// Attempt to expand taskbar
			if (_isEnabled && Taskbar != null && !Taskbar.IsExpanded && Game1.game1.IsActive)
			{
				if (Taskbar.ExpandButton.containsPoint(x, y))
				{
					Taskbar.SetActiveState(active: true);
				}
			}

			if (!IsDesktopActive || !ISUtilities.IsPointWithinViewportBounds(x, y))
				return;

			base.receiveLeftClick(x, y, playSound);
			Taskbar.receiveLeftClick(x, y, playSound);

			// Start LeftMouse held behaviours
			++LeftClickHeldTimer;

			foreach (IClickableMenu child in Children.ToList())
			{
				// Handle clicks for window bars
				child.receiveLeftClick(x, y, playSound);

				bool clickedParent = child.GetParentMenu() is WindowBar parent && parent.isWithinBounds(x, y);
				if (clickedParent)
				{
					if (SelectedChildIndex == 0)
						SelectedChildIndex = Children.IndexOf(child);
					return;
				}
			}

			foreach (IClickableMenu child in Children.ToList())
			{
				// Handle clicks for window pages
				WindowBar parent = child.GetParentMenu() as WindowBar;
				bool clickedChild = child.isWithinBounds(x, y) && (parent == null || parent.ShouldDrawChild);
				bool clickedTaskbarIcon = Taskbar.Icons.Any(icon => icon.name == child.GetType().Name && icon.containsPoint(x, y));
				if (clickedChild || clickedTaskbarIcon)
				{
					if (SelectedChildIndex == 0)
						SelectedChildIndex = Children.IndexOf(child);
					return;
				}
			}

			SelectedChildIndex = 0;

			// Clicking characters in the world will open their page in the character menu
			if (ISUtilities.GetCharacterUnderCursor() is NPC npc && npc != null)
			{
				IClickableMenu menu = Taskbar.ClickTaskbarIcon(typeName: nameof(Menus.CharacterListMenu));
				if (menu is Menus.CharacterListMenu characterMenu && characterMenu.WindowBar != null)
				{
					characterMenu.WindowBar.IsMinimised = false;
					characterMenu.SetCharacter(npc);
					if (SelectedChildIndex == 0)
						SelectedChildIndex = Children.IndexOf(characterMenu);
					return;
				}
			}

			// Clicking tiles in the world with ShowTileHighlight enabled will open its page in the tile menu
			if (Children.FirstOrDefault(menu => menu is Menus.TileInfoMenu) is Menus.TileInfoMenu tileMenu && tileMenu != null && tileMenu.WindowBar != null)
			{
				if (tileMenu.IsHoveringTile)
				{
					if (tileMenu.SelectTile(location: Game1.currentLocation, tilePosition: Game1.currentCursorTile))
					{
						tileMenu.WindowBar.IsMinimised = false;
						SelectedChildIndex = Children.IndexOf(tileMenu);
						return;
					}
				}
			}

			for (int i = Widgets.Count - 1; i >= 0; --i)
				Widgets[i].receiveLeftClick(x, y, playSound);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (!IsDesktopActive || !ISUtilities.IsPointWithinViewportBounds(x, y))
				return;

			base.receiveRightClick(x, y, playSound);
			Taskbar.receiveRightClick(x, y, playSound);
			foreach (IClickableMenu child in Children.ToList())
				child.receiveRightClick(x, y, playSound);

			// Start RightMouse held behaviours
			++RightClickHeldTimer;
			if (_cameraDragOrigin == Vector2.Zero)
			{
				_cameraDragOrigin = new Vector2(x, y);
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			if (!IsDesktopActive)
				return;

			base.leftClickHeld(x, y);
			Taskbar.leftClickHeld(x, y);
			foreach (IClickableMenu child in Children.ToList())
				child.leftClickHeld(x, y);
			foreach (CustomMenu widget in Widgets.ToList())
				widget.leftClickHeld(x, y);
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (!IsDesktopActive)
				return;

			base.releaseLeftClick(x, y);
			Taskbar.releaseLeftClick(x, y);
			foreach (IClickableMenu child in Children.ToList())
				child.releaseLeftClick(x, y);
			foreach (CustomMenu widget in Widgets.ToList())
				widget.releaseLeftClick(x, y);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (!IsDesktopActive)
				return;

			base.receiveScrollWheelAction(direction);
			Taskbar.receiveScrollWheelAction(direction);
			foreach (IClickableMenu child in Children.ToList())
				child.receiveScrollWheelAction(direction);
			foreach (CustomMenu widget in Widgets.ToList())
				widget.receiveScrollWheelAction(direction);
		}

		public override void performHoverAction(int x, int y)
		{
			if (!IsDesktopActive)
				return;

			base.performHoverAction(x, y);
			Taskbar.performHoverAction(x, y);
			foreach (IClickableMenu child in Children.ToList())
				child.performHoverAction(x, y);
			foreach (CustomMenu widget in Widgets.ToList())
				widget.performHoverAction(x, y);
		}

		private static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (ModEntry.Instance.Desktop == null)
			{
				ModEntry.Instance.Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;
				return;
			}

			if (!Context.IsWorldReady)
				return;

			if (ModEntry.Config.DebugMode && Game1.keyboardDispatcher.Subscriber == null)
			{
				switch (e.Button)
				{
					case SButton.J:
					{
						Log.D("INTERFACE: Set enabled: " + !ModEntry.Instance.Desktop._isEnabled);
						ModEntry.Instance.Desktop._isEnabled = !ModEntry.Instance.Desktop._isEnabled;
						break;
					}
					case SButton.K:
					{
						Log.D("INTERFACE: Set new interface");
						ModEntry.Instance.Desktop.exitThisMenu();
						Desktop newInterface = new Desktop();
						ModEntry.Instance.Desktop = newInterface;
						break;
					}
					case SButton.L:
					{
						if (!ModEntry.Instance.Desktop._isEnabled)
							break;
						Log.D("INTERFACE: Clear children");
						ModEntry.Instance.Desktop.Reset();
						break;
					}
					/*case SButton.OemSemicolon:
					{
						if (!_isEnabled)
							break;
						Log.D("INTERFACE: Add new CharacterListMenu");
						Menus.CharacterListMenu childMenu = new Menus.CharacterListMenu(new Point(75, 25));
						Children.Add(childMenu);
						break;
					}*/
				}
			}

			if (!ModEntry.Instance.Desktop.IsDesktopActive)
				return;

			e.Button.TryGetKeyboard(out Keys key);

			// Menu
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				ModEntry.Instance.Helper.Input.Suppress(e.Button);
				return;
			}

			// Journal
			if (Game1.options.doesInputListContain(Game1.options.journalButton, key))
			{
				ModEntry.Instance.Helper.Input.Suppress(e.Button);
				return;
			}
		}

		private static void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
		{
			if (ModEntry.Instance.Desktop == null)
			{
				ModEntry.Instance.Helper.Events.Input.ButtonReleased -= Input_ButtonReleased;
				return;
			}

			if (ModEntry.Instance.Desktop.Taskbar.IsExpanded)
			{
				// Releasing RightMouse will end camera dragging
				if (e.Button == SButton.MouseRight)
				{
					RightClickHeldTimer = 0f;
					ModEntry.Instance.Desktop._cameraDragOrigin = Vector2.Zero;
				}
			}
		}

		public void PushNotification(string text, NotificationDuration duration)
		{
			Notifications.Enqueue(new KeyValuePair<string, float>(text, (float)duration));
		}

		/// <summary>
		/// Cast the viewport to some new location
		/// </summary>
		/// <param name="locationName">Name of location to view.</param>
		/// <param name="tilePosition">Tile to centre view over.</param>
		public void ViewLocation(string locationName, Point tilePosition, bool notify)
		{
			GameLocation location = Game1.getLocationFromName(locationName);
			if (location == null)
			{
				Log.E("Bad view location: " + locationName + " - location doesn't exist!");
				return;
			}

			if (string.IsNullOrEmpty(ModEntry.Instance._originalLocation))
			{
				ModEntry.Instance._originalLocation = Game1.player.currentLocation.Name;
				ModEntry.Instance._originalPosition = Game1.player.Position;
			}

			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.currentLocation = location;
			Game1.player.viewingLocation.Value = locationName;
			Game1.currentLocation.resetForPlayerEntry();
			if (!ModEntry.Config.SnappyMenus)
			{
				Game1.globalFadeToClear(afterFade: delegate {
					Taskbar.yPositionOnScreen = 0;
					Taskbar.RealignElements(); // Mystery invisible taskbar solution
				});
			}
			this.SetFarmerVisibility(visible: false);
			Game1.panScreen(
				x: (tilePosition.X * Game1.tileSize) - (Game1.viewport.Width / 2),
				y: (tilePosition.Y * Game1.tileSize) - (Game1.viewport.Height / 2));
			string text = ModEntry.Instance.i18n.Get("notif.warping.text", 
				new { LocationName = locationName });
			this.PushNotification(text: text, duration: NotificationDuration.Short);

			if (Widgets.FirstOrDefault(widget => widget is ViewingLocationWidget) is ViewingLocationWidget widget && widget != null)
			{
				widget.RealignElements();
			}
			else
			{
				Widgets.Add(new ViewingLocationWidget());
			}
		}

		public void ReturnFromViewLocation()
		{
			if (string.IsNullOrEmpty(ModEntry.Instance._originalLocation))
				return;

			LocationRequest locationRequest = Game1.getLocationRequest(ModEntry.Instance._originalLocation);
			locationRequest.OnWarp += delegate
			{
				Widgets.RemoveAll(widget => widget is ViewingLocationWidget);

				this.SetFarmerVisibility(visible: true);

				Taskbar.yPositionOnScreen = 0;
				Taskbar.RealignElements(); // Mystery invisible taskbar solution
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);

			ModEntry.Instance._originalLocation = null;
		}

		public void SetFarmerVisibility(bool visible)
		{
			if (visible)
			{
				Game1.player.viewingLocation.Value = null;
			}
			Game1.viewport.Location = new Location(0, 0);
			Game1.viewportFreeze = !visible;
			Game1.displayFarmer = visible;
		}

		public void WarpFarmerTo(string location)
		{
			this.SetFarmerVisibility(visible: true);

			int warpX = 0, warpY = 0;
			Utility.getDefaultWarpLocation(location_name: location, x: ref warpX, y: ref warpY);
			Game1.player.warpFarmer(new Warp(
				x: 0, y: 0,
				targetName: location, targetX: warpX, targetY: warpY,
				flipFarmer: false));
		}

		/// <summary>
		/// Common method for calling existing <see cref="IClickableMenu.drawHoverText"/> method.
		/// </summary>
		internal void DrawHoverText(SpriteBatch b, string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				_oldHoverText = text;
				SpriteFont font = Game1.smallFont;
				int cursorSize = CursorSource.Height * MenuScale;
				Vector2 textSize = font.MeasureString(text);
				Rectangle rectangle = new Rectangle(
					Game1.getOldMouseX(),
					Game1.getOldMouseY() + cursorSize,
					(int)textSize.X + (Padding.X * 2),
					(int)textSize.Y + (Padding.Y * 2));
				rectangle.X = Math.Max(0, rectangle.X - Math.Max(0, rectangle.X + rectangle.Width - Game1.viewport.Width));
				rectangle.Y = Math.Max(0, rectangle.Y - Math.Max(0, rectangle.Y + rectangle.Height + (cursorSize * 2) + Padding.Y - Game1.viewport.Height));
				WindowBar.DrawWindowBarContainer(b,
					area: rectangle,
					colour: Color.White,
					greyed: false,
					simpleStyle: true,
					drawShadow: true,
					alternateColour: true);
				this.DrawText(b,
					position: new Vector2(rectangle.X + Padding.X, rectangle.Y + Padding.Y),
					text: text,
					font: font,
					colour: CustomMenu.BodyTextColour);
			}
		}

		private void DrawInfoBubbles(SpriteBatch b)
		{
			/*foreach (KeyValuePair<string, float> pair in Notifications)
			{
				SpriteFont font = Game1.smallFont;
				Color colour = Color.White * ;
				int yOffset = 0;
				int textWidth = ;
				Vector2 position = new Vector2(Game1.viewport.Width - , Game1.viewport.Height - );
				Rectangle sourceRect = new Rectangle();
				Rectangle destinationRect = new Rectangle();
				string text = Game1.parseText(pair.Key, whichFont: font, width: textWidth);

				b.Draw(texture: ModEntry.Sprites,
					destinationRectangle: ,
					sourceRectangle: ,
					color: colour);

				// Notification message text
				b.DrawString(
					spriteFont: Game1.smallFont,
					text: text,
					position: position,
					color: colour)
				position.Y -= ;
			}*/
		}

		private void DrawCustomCursor(SpriteBatch b)
		{
			Vector2 position = Utility.PointToVector2(Game1.getMousePosition());
			Color colour = Color.White;
			float scale = MenuScale;
			if (RightClickHeldTimer > RightClickHoldDelay * RightClickHeldDelayScale)
			{
				// Crosshair cursor when dragging camera
				_mouseCursor = Cursor.Crosshair;

				// Draw camera drag pin to mark point from where amplitude of camera drag movement increases
				b.Draw(
					texture: ModEntry.Sprites,
					position: _cameraDragOrigin,
					sourceRectangle: CameraDragPinSource,
					color: Color.White,
					rotation: 0f,
					origin: Vector2.Zero,
					scale: MenuScale,
					effects: SpriteEffects.None,
					layerDepth: 1f);
			}
			else if (Game1.keyboardDispatcher.Subscriber != null)
			{
				// Caret cursor when interacting with an active text box
				_mouseCursor = Cursor.Caret;
				//position.X -= 4;
				position.Y -= 6;
				colour = Game1.textColor;
				scale = 2f;
			}
			else if (!string.IsNullOrEmpty(_oldHoverText))
			{
				// Finger cursor when over clickables
				_mouseCursor = Cursor.Finger;
			}
			else if (Children.All(child => !child.isWithinBounds((int)position.X, (int)position.Y)
				&& Widgets.All(widget => !widget.isWithinBounds((int)position.X, (int)position.Y))
				&& ISUtilities.GetCharacterUnderCursor() != null))
			{
				// Finger cursor when over NPCs
				_mouseCursor = Cursor.Finger;
			}

			// Draw mouse cursor
			b.Draw(texture: ModEntry.Sprites,
				position: position,
				sourceRectangle: new Rectangle(CursorSource.X + ((int)_mouseCursor * CursorSource.Width), CursorSource.Y, CursorSource.Width, CursorSource.Height),
				color: colour,
				rotation: 0f,
				origin: Vector2.Zero,
				scale: scale,
				effects: SpriteEffects.None,
				layerDepth: 1f);

			// Reset hover text
			_oldHoverText = null;
		}

		private void DrawSchedulePath(SpriteBatch b)
		{
			foreach (Location location in ModEntry.Instance._schedulePath.Path)
			{
				b.Draw(
					texture: Game1.mouseCursors,
					position: new Vector2(location.X, location.Y),
					sourceRectangle: Game1.currentLocation.isTilePassable(location, Game1.viewport)
						? TileHighlightGreenSource
						: TileHighlightRedSource,
					color: Color.White,
					rotation: 0f, origin: Vector2.Zero, scale: Game1.pixelZoom, effects: SpriteEffects.None, layerDepth: 1f);
			}
		}

		public static void DrawGradient(SpriteBatch b, Rectangle area, Color colour, SpriteEffects effects)
		{
			b.Draw(
				texture: ModEntry.Sprites,
				destinationRectangle: area,
				sourceRectangle: ModEntry.GradientRect,
				color: colour,
				rotation: 0f,
				origin: Vector2.Zero,
				effects: effects,
				layerDepth: 1f);
		}

		/// <summary>
		/// Draw a simple straight line to the screen along X or Y axis.
		/// </summary>
		public static void DrawLine(SpriteBatch b, Color colour, Point startPosition, int length, int width, bool isHorizontal)
		{
			Rectangle destinationRectangle = new Rectangle(startPosition.X, startPosition.Y, isHorizontal ? length : width, isHorizontal ? width : length);
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: destinationRectangle,
				sourceRectangle: null,
				color: colour,
				rotation: 0f, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: 1f);
		}

		public void PlaySound(string which)
		{
			if (IsMuted)
				return;
			Game1.playSound(which);
		}
	}
}

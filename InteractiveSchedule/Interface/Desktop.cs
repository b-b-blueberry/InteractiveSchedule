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
	public class Desktop : IClickableMenu
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
		/// <summary>
		/// Whether to play menu interaction sounds.
		/// </summary>
		public bool IsMuted;

		internal static readonly Rectangle TileHighlightGreenSource = new Rectangle(194, 388, 16, 16);
		internal static readonly Rectangle TileHighlightRedSource = new Rectangle(210, 388, 16, 16);
		internal static readonly Rectangle TileHighlightDoorGreenSource = new Rectangle(226, 388, 16, 16);
		internal static readonly Rectangle TileHighlightDoorRedSource = new Rectangle(242, 388, 16, 16);

		/// <summary>
		/// Name of player's current location before casting view using <see cref="ViewLocation"/>.
		/// Null after using <see cref="ReturnFromViewLocation"/>.
		/// </summary>
		internal string _originalLocation;
		/// <summary>
		/// Absolute world position of the player before casting view using <see cref="ViewLocation"/>.
		/// </summary>
		internal Vector2 _originalPosition;
		/// <summary>
		/// Whether all features of the desktop are enabled.
		/// </summary>
		private bool _isEnabled = true;
		private IModHelper Helper => ModEntry.Instance.Helper;

		public Desktop()
		{
			this.Reset();
			Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
		}

		/// <summary>
		/// Clears all <see cref="Children"/> and replaces <see cref="Taskbar"/>.
		/// </summary>
		public void Reset()
		{
			Children.Clear();
			this.AddTaskbar();
		}

		/// <summary>
		/// Replaces <see cref="Taskbar"/> with some new <see cref="Interface.Taskbar"/>
		/// </summary>
		private void AddTaskbar(Taskbar taskbar = null)
		{
			Taskbar = taskbar ?? new Taskbar();
		}

		protected override void cleanupBeforeExit()
		{
			Helper.Events.Input.ButtonPressed -= this.Input_ButtonPressed;
			Taskbar = null;
			Children.Clear();

			base.cleanupBeforeExit();
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
		}

		public override void update(GameTime time)
		{
			if (!Game1.game1.IsActive)
			{
				return;
			}

			if (SelectedChildIndex > 0 && Children.Count > 1)
			{
				IClickableMenu child = Children[SelectedChildIndex];
				Children.RemoveAt(SelectedChildIndex);
				Children.Insert(0, child);
			}
			SelectedChildIndex = 0;

			base.update(time);
			Taskbar.update(time);
			foreach (IClickableMenu child in Children)
				child.update(time);
		}

		public override void draw(SpriteBatch b)
		{
			if (Taskbar.IsExpanded)
			{
				for (int i = Children.Count - 1; i >= 0; --i)
				{
					Children[i].draw(b);
				}
			}
			switch (ModEntry.Instance._state)
			{
				case ModEntry.States.Path:
				{

				}
				break;
			}

			Taskbar.draw(b);
			base.draw(b);

			// Cursor
			Game1.mouseCursorTransparency = 1f;
			this.drawMouse(b);
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			Taskbar.receiveKeyPress(key);
			foreach (IClickableMenu child in Children.ToList())
				child.receiveKeyPress(key);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (x < 0 || y < 0 || x > Game1.viewport.Width || y > Game1.viewport.Height)
				return;

			base.receiveLeftClick(x, y, playSound);
			Taskbar.receiveLeftClick(x, y, playSound);

			foreach (IClickableMenu child in Children.ToList())
			{
				// Handle clicks for window bars
				child.receiveLeftClick(x, y, playSound);

				bool clickedParent = child.GetParentMenu() is WindowBar parent && parent.isWithinBounds(x, y);
				if (clickedParent)
				{
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
					SelectedChildIndex = Children.IndexOf(child);
					return;
				}
			}

			SelectedChildIndex = 0;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			base.receiveRightClick(x, y, playSound);
			Taskbar.receiveRightClick(x, y, playSound);
			foreach (IClickableMenu child in Children.ToList())
				child.receiveRightClick(x, y, playSound);
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			Taskbar.leftClickHeld(x, y);
			foreach (IClickableMenu child in Children.ToList())
				child.leftClickHeld(x, y);
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			Taskbar.releaseLeftClick(x, y);
			foreach (IClickableMenu child in Children.ToList())
				child.releaseLeftClick(x, y);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			Taskbar.receiveScrollWheelAction(direction);
			foreach (IClickableMenu child in Children)
				child.receiveScrollWheelAction(direction);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			Taskbar.performHoverAction(x, y);
			foreach (IClickableMenu child in Children)
				child.performHoverAction(x, y);
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (ModEntry.Config.DebugMode)
			{
				switch (e.Button)
				{
					case SButton.J:
					{
						Log.D("INTERFACE: Set enabled: " + !_isEnabled);
						_isEnabled = !_isEnabled;
						break;
					}
					case SButton.K:
					{
						Log.D("INTERFACE: Set new interface");
						this.exitThisMenu();
						Desktop newInterface = new Desktop();
						ModEntry.Instance.Desktop = newInterface;
						break;
					}
					case SButton.L:
					{
						if (!_isEnabled)
							break;
						Log.D("INTERFACE: Clear children");
						this.Reset();
						break;
					}
					case SButton.OemSemicolon:
					{
						if (!_isEnabled)
							break;
						Log.D("INTERFACE: Add new CharacterListMenu");
						CharacterListMenu childMenu = new CharacterListMenu(new Point(75, 25));
						Children.Add(childMenu);
						break;
					}
				}
			}

			if (!Game1.game1.IsActive || !_isEnabled)
			{
				return;
			}
			e.Button.TryGetKeyboard(out Keys key);

			if (Taskbar.IsExpanded)
			{
				// Menu
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
				{
					Helper.Input.Suppress(e.Button);
					return;
				}

				// Journal
				if (Game1.options.doesInputListContain(Game1.options.journalButton, key))
				{
					Helper.Input.Suppress(e.Button);
					return;
				}
			}
		}

		/// <summary>
		/// Cast the viewport to some new location
		/// </summary>
		/// <param name="locationName">Name of location to view.</param>
		/// <param name="tileLocation">Tile to centre view over.</param>
		public void ViewLocation(string locationName, Point tileLocation)
		{
			GameLocation location = Game1.getLocationFromName(locationName);
			if (location == null)
			{
				Log.E("Bad view location: " + locationName + " - location doesn't exist!");
				return;
			}

			if (string.IsNullOrEmpty(_originalLocation))
			{
				_originalLocation = Game1.player.currentLocation.Name;
				_originalPosition = Game1.player.Position;
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
			Game1.viewportFreeze = true;
			Game1.viewport.Location = new Location(0, 0);
			Game1.panScreen(
				x: (tileLocation.X * Game1.tileSize) - (Game1.viewport.Width / 2),
				y: (tileLocation.Y * Game1.tileSize) - (Game1.viewport.Height / 2));
			Game1.displayFarmer = false;
		}

		public void ReturnFromViewLocation()
		{
			LocationRequest locationRequest = Game1.getLocationRequest(_originalLocation);
			locationRequest.OnWarp += delegate
			{
				Game1.player.viewingLocation.Value = null;
				Game1.viewportFreeze = false;
				Game1.viewport.Location = new Location(0, 0);
				Game1.displayFarmer = true;
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);

			_originalLocation = null;
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
	}
}

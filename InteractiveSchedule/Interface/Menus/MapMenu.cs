using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Locations;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace InteractiveSchedule.Interface.Menus
{
	public class MapMenu : WindowPage
	{
		public ClickableTextureComponent ReturnViewLocationButton, ViewLocationButton, WarpHereButton;
		public Dictionary<string, string> LocationStrings = new Dictionary<string, string>();
		public Dictionary<string, Rectangle> LocationRegions = new Dictionary<string, Rectangle>();
		public readonly List<ClickableComponent> MapLocations = new List<ClickableComponent>();
		public override bool IsOnHomePage => true;
		public override bool IsUpButtonVisible => !IsOnHomePage;
		public override bool IsActionButtonSidebarVisible => false;

		private GameLocation _lastHoveredLocation;
		private Vector2 _mapOrigin;
		private readonly Texture2D _map;
		private static readonly Rectangle MapSource = new Rectangle(0, 0, 300, 180);

		public MapMenu(Point position)
			: base(position: position)
		{
			_map = Game1.content.Load<Texture2D>("LooseSprites\\map");
		}

		protected override void cleanupBeforeExit()
		{
			_lastHoveredLocation = null;
			ViewLocationButton = null;
			MapLocations.Clear();
			LocationStrings.Clear();
			LocationRegions.Clear();

			base.cleanupBeforeExit();
		}

		public override void RealignElements()
		{
			base.RealignElements();

			if (WindowBar != null && _map != null)
			{
				WindowBar.width = width = Math.Max(WindowBar.width, (MapSource.Width + 4) * MenuScale);
				height = WindowBar.IsFullscreen
					? WindowBar.FullscreenHeight
					: MapSource.Height * MenuScale;
				_mapOrigin = new Vector2(
					Math.Max(BorderSafeArea.X, (width - (MapSource.Width * MenuScale)) / 2),
					Math.Max(BorderSafeArea.Y, (height - (MapSource.Height * MenuScale)) / 2));
			}

			this.AddMapPoints();
		}

		protected override void AddActionButtons()
		{
			base.AddActionButtons();

			WarpHereButton = this.CreateActionButton(which: nameof(WarpHereButton));
			ViewLocationButton = this.CreateActionButton(which: nameof(ViewLocationButton));
			ReturnViewLocationButton = this.CreateActionButton(which: nameof(ReturnViewLocationButton));
		}

		public override void RealignFloatingButtons()
		{
			if (ViewLocationButton != null)
			{
				int yOffset = (ActionButtonSize.Y * MenuScale) + (Padding.Y * 2);
				FloatingActionButtons[ReturnViewLocationButton] = new Point(
					Padding.X,
					ActionButtonOrigin.Y - yOffset - Padding.Y);
				FloatingActionButtons[ViewLocationButton] = new Point(
					FloatingActionButtons[ReturnViewLocationButton].X,
					FloatingActionButtons[ReturnViewLocationButton].Y - yOffset);
				FloatingActionButtons[WarpHereButton] = new Point(
					FloatingActionButtons[ViewLocationButton].X,
					FloatingActionButtons[ViewLocationButton].Y - yOffset);
			}

			base.RealignFloatingButtons();
		}

		protected override void ClickUpButton()
		{
			throw new NotImplementedException();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();

			LocationStrings = new Dictionary<string, string>
			{
				{ "Farm", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11064", Game1.MasterPlayer.farmName.Value) },
				{ "Beach", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11174") },
				{ "Forest", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11186") },
				{ "Mountain", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11176") },
				{ "Desert", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11062") },
				{ "Backwoods", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11065") },
				{ "BusStop", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11066") },
				{ "WizardHouse", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11067") },
				{ "AnimalShop", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11068") },
				{ "LeahHouse", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11070") },
				{ "SamHouse", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11072") },
				{ "HaleyHouse", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11074") },
				{ "Town", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190") },
				{ "Hospital", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11076") },
				{ "HarveyRoom", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11076") },
				{ "SeedShop", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11078") },
				{ "Blacksmith", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11081") },
				{ "Saloon", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11083") },
				{ "ManorHouse", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11085") },
				{ "ArchaeologyHouse", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11086") },
				{ "ElliottHouse", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11088") },
				{ "JoshHouse", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11093") },
				{ "ScienceHouse", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11095") },
				{ "Tent", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11097") },
				{ "Mine", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11098") },
				{ "AdventureGuild", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11099") },
				{ "FishShop", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11107") },
				{ "BathHouseEntry", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11110") },
				{ "Woods", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11114") },
				{ "Sewer", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11089") },
				{ "Railroad", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11119") },
				{ "Trailer", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.PamHouseHomeOf") },
				{ "JojaMart", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11105") },
				{ "AbandonedJojaMart", Game1.content.LoadString("Strings\\StringsFromCSFiles:AbandonedJojaMart") },
				{ "MovieTheater", Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheater_Map") },
				{ "CommunityCenter", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11117") },
				{ "LonelyStone", Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11122") },
				{ "Island", Game1.content.LoadString("Strings\\StringsFromCSFiles:IslandName") },
			};

			// 1x scale relative coordinates, edited from optimised decompiled values in StardewValley.Menus.MapPage.cs:MapPage
			// Points of interest are placed above wide locations to allow them to be hovered and selected above their location
			LocationRegions = new Dictionary<string, Rectangle>
			{
				{ "WizardHouse", new Rectangle(49, 88, 9, 19) },
				{ "AnimalShop", new Rectangle(105, 98, 19, 10) },
				{ "LeahHouse", new Rectangle(113, 109, 8, 6) },
				{ "SamHouse", new Rectangle(153, 99, 9, 13) },
				{ "HaleyHouse", new Rectangle(163, 102, 10, 8) },
				{ "HarveyRoom", new Rectangle(170, 73, 4, 3) },
				{ "Hospital", new Rectangle(170, 76, 4, 8) },
				{ "SeedShop", new Rectangle(174, 74, 7, 10) },
				{ "Blacksmith", new Rectangle(213, 97, 20, 9) },
				{ "Saloon", new Rectangle(179, 88, 7, 10) },
				{ "ManorHouse", new Rectangle(192, 97, 11, 14) },
				{ "ArchaeologyHouse", new Rectangle(223, 104, 8, 7) },
				{ "ElliottHouse", new Rectangle(206, 141, 7, 5) },
				{ "JoshHouse", new Rectangle(187, 79, 9, 9) },
				{ "ScienceHouse", new Rectangle(183, 37, 12, 8) },
				{ "Tent", new Rectangle(194, 30, 8, 8) },
				{ "AdventureGuild", new Rectangle(225, 27, 8, 9) },
				{ "Mine", new Rectangle(214, 19, 12, 12) },
				{ "FishShop", new Rectangle(211, 152, 9, 10) },
				{ "BathHouseEntry", new Rectangle(144, 15, 12, 9) },
				{ "Sewer", new Rectangle(95, 149, 6, 8) },
				{ "Trailer", new Rectangle(195, 90, 7, 5) },
				{ "JojaMart", new Rectangle(218, 70, 13, 13) },
				{ "CommunityCenter", new Rectangle(173, 51, 11, 9) },
				{ "Railroad", new Rectangle(136, 6, 56, 20) },
				{ "Woods", new Rectangle(0, 68, 49, 44) },
				{ "Forest", new Rectangle(58, 94, 48, 54) },
				{ "Beach", new Rectangle(192, 136, 48, 14) },
				{ "Mountain", new Rectangle(194, 36, 38, 18) },
				{ "Desert", new Rectangle(0, 0, 73, 38) },
				{ "Backwoods", new Rectangle(90, 24, 47, 33) },
				{ "BusStop", new Rectangle(128, 50, 22, 36) },
				{ "Town", new Rectangle(168, 85, 11, 15) },
				{ "Farm", new Rectangle(81, 63, 47, 33) },
			};

			// Add extra locations
			// Keep this locked for Stardew Valley 1.5 spoilers
			if (Game1.MasterPlayer.hasOrWillReceiveMail("Visited_Island"))
			{
				LocationRegions.Add("Island", new Rectangle(260, 150, 40, 30));
				LocationRegions.Add("LonelyStone", new Rectangle(182, 163, 7, 7));
			}

			// Scale components to fit menu
			foreach (string key in LocationRegions.Keys.ToList())
			{
				LocationRegions[key] = new Rectangle(
					LocationRegions[key].X * MenuScale,
					LocationRegions[key].Y * MenuScale,
					LocationRegions[key].Width * MenuScale,
					LocationRegions[key].Height * MenuScale);
			}
		}

		/// <summary>
		/// Populates the <see cref="MapLocations"/> list with ClickableComponents defining hoverable/clickable areas in the map designating locations and points of interest.
		/// Points are scaled to <see cref="CustomMenu.MenuScale"/>.
		/// </summary>
		private void AddMapPoints()
		{
			MapLocations.Clear();

			foreach (string key in LocationRegions.Keys)
			{
				MapLocations.Add(new ClickableComponent(
					bounds: new Rectangle(
						(int)_mapOrigin.X + LocationRegions[key].X,
						(int)_mapOrigin.Y + LocationRegions[key].Y,
						LocationRegions[key].Width,
						LocationRegions[key].Height),
					name: key));
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);

			if (!IsSelected || !ShouldDraw || ModalWindow != null)
				return;

			if (FloatingActionButtons.Any())
			{
				if (WarpHereButton.containsPoint(x, y))
				{
					if (string.IsNullOrEmpty(Game1.player.viewingLocation.Value)
						|| (Game1.currentLocation != null && Game1.player.viewingLocation.Value == Game1.player.currentLocation.Name))
					{
						Desktop.PlaySound("cancel");
					}
					else
					{
						Desktop.WarpFarmerTo(Game1.player.viewingLocation.Value);
					}
				}
				if (ViewLocationButton.containsPoint(x, y))
				{
					ModalWindow = new Modals.LocationSelectModal(position: new Point(25 * MenuScale, 5 * MenuScale), parent: this);
				}
				if (ReturnViewLocationButton.containsPoint(x, y))
				{
					Desktop.ReturnFromViewLocation();
				}
			}

			if (MapLocations.Any(point => point.containsPoint(x, y)) && _lastHoveredLocation != null)
			{
				// Clicking on map regions will cast the view to that location
				int warpX = 0, warpY = 0;
				Utility.getDefaultWarpLocation(location_name: _lastHoveredLocation.Name, x: ref warpX, y: ref warpY);
				Desktop.ViewLocation(locationName: _lastHoveredLocation.Name, tilePosition: new Point(warpX, warpY), notify: true);
			}
		}

		public override void performHoverAction(int x, int y)
		{
			_hoverText = "";

			base.performHoverAction(x, y);

			if (!ShouldUpdateHoverText || ModalWindow != null)
				return;

			foreach (ClickableComponent point in MapLocations)
			{
				if (point.containsPoint(x, y))
				{
					if (_lastHoveredLocation == null || _lastHoveredLocation.Name != point.name)
					{
						_lastHoveredLocation = Game1.getLocationFromName(point.name);
					}
					_hoverText = LocationStrings[point.name] + "\n\n>  " + (_lastHoveredLocation == null ? "???" : _lastHoveredLocation.mapPath.Value);
					return;
				}
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		/// <summary>
		/// Draws the game map to the page. Ignores <see cref="WindowPage.ContentSafeArea"/>
		/// </summary>
		private void DrawMap(SpriteBatch b)
		{
			b.Draw(
				texture: _map,
				position: _mapOrigin,
				sourceRectangle: MapSource,
				color: Color.White,
				rotation: 0f,
				origin: Vector2.Zero,
				scale: MenuScale,
				effects: SpriteEffects.None,
				layerDepth: 0.86f);
		}

		public override void DrawContent(SpriteBatch b)
		{
			this.DrawMap(b);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}

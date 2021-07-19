using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface.Menus
{
	public class CharacterListMenu : WindowPage
	{
		public ClickableTextureComponent ReturnFromButton, GoToButton, ViewScheduleButton, GoToSpawnButton;
		private Rectangle _genderArea, _birthdayArea, _propertiesArea, _currentLocationArea, _defaultLocationArea;
		private enum Properties
		{
			Datable,
			Visible,
			Sociable,
			Scheduled
		}
		private static readonly Rectangle GenderSourceArea = new Rectangle(82, 18, 10, 10);
		private static readonly Rectangle SeasonSourceArea = new Rectangle(102, 18, 10, 10);
		private static readonly Rectangle PropertiesSourceArea = new Rectangle(82, 28, 10, 10);
		private static readonly Rectangle CurrentLocationSourceArea = new Rectangle(142, 18, 10, 10);
		private static readonly Rectangle DefaultLocationSourceArea = new Rectangle(152, 18, 10, 10);

		/// <summary> List of names for each <see cref="StardewValley.NPC"/> loaded in the NPCDispositions data file, paired with their display names.</summary>
		public Dictionary<string, string> Names = new Dictionary<string, string>();
		/// <summary> Clickable mugshot icons for each <see cref="StardewValley.NPC"/> appearing in <see cref="Names"/>.</summary>
		public List<ClickableTextureComponent> Heads = new List<ClickableTextureComponent>();

		public override bool IsOnHomePage => SelectedChara == null;
		public override bool IsUpButtonVisible => !IsOnHomePage;
		public override bool IsActionButtonSidebarVisible => !IsOnHomePage;

		/// <summary> Currently selected <see cref="StardewValley.NPC"/>, determines the sub-page contents.</summary>
		public NPC SelectedChara { get; private set; }

		/// <summary> Scale for each clickable icon in <see cref="Heads"/>. </summary>
		private int _headScale;
		/// <summary> Number of <see cref="Heads"/> icons in each row to draw to the window.</summary>
		private int _headsWide;
		private bool[] _properties;
		private string[] _hoverTextCached;
		private string _defaultLocationName;

		public CharacterListMenu(Point position)
			: base(position: position)
		{
		}

		protected override void cleanupBeforeExit()
		{
			Names.Clear();
			Heads.Clear();
			ReturnFromButton = null;
			GoToButton = null;
			ViewScheduleButton = null;
			SelectedChara = null;

			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();

			_headScale = MenuScale;
			_headsWide = 8;
		}

		public override void RealignElements()
		{
			base.RealignElements();

			if (!Heads.Any())
			{
				this.AddCharaButtons();
			}

			if (WindowBar != null)
			{
				// Character catalogue
				Rectangle headSize = Heads.FirstOrDefault()?.sourceRect ?? new Rectangle(0, 0, 16, 24);

				WindowBar.width = width = Math.Max(WindowBar.width, (8 * headSize.Width) + (Padding.X * 2) + ActionButtonSidebarArea.Width);
				height = WindowBar.IsFullscreen
					? WindowBar.FullscreenHeight
					: Math.Max(400, (((Heads.Count / _headsWide) + 1) * headSize.Height * _headScale) + (Padding.Y * 2));

				int spacing = 2;
				Point actualHeadSize = new Point(headSize.Width + (spacing * 2), headSize.Height + (spacing * 2));

				_headsWide = Math.Max(8, ContentSafeArea.Width / actualHeadSize.X / _headScale);

				int xToCentre = (ContentSafeArea.Width - (_headsWide * actualHeadSize.X * _headScale)) / 2;
				int yOffset = Padding.Y;

				for (int i = 0; i < Heads.Count; ++i)
				{
					int x = (i % _headsWide) * actualHeadSize.X * _headScale;
					int y = i / _headsWide * actualHeadSize.Y * _headScale;
					Heads[i].bounds = new Rectangle(
						ContentSafeArea.X + xToCentre + x,
						ContentSafeArea.Y + yOffset + y,
						actualHeadSize.X * MenuScale,
						actualHeadSize.Y * MenuScale);
				}

				// Character info sheet
				_genderArea = new Rectangle(
					ContentSafeArea.X,
					ContentSafeArea.Y + HeadingHeight + (Padding.Y * 2),
					GenderSourceArea.Width * MenuScale,
					GenderSourceArea.Height * MenuScale);
				_birthdayArea = new Rectangle(
					_genderArea.X + _genderArea.Width + (Padding.X),
					_genderArea.Y,
					SeasonSourceArea.Width * MenuScale,
					SeasonSourceArea.Height * MenuScale);
				_propertiesArea = new Rectangle(
					_birthdayArea.X + _birthdayArea.Width + (Padding.X * 2),
					_birthdayArea.Y,
					PropertiesSourceArea.Width * (_properties?.Length ?? 0) * MenuScale,
					PropertiesSourceArea.Height * MenuScale);
				_defaultLocationArea = new Rectangle(
					_genderArea.X,
					_genderArea.Y + _genderArea.Height + (Padding.Y * 2),
					DefaultLocationSourceArea.Width * MenuScale,
					DefaultLocationSourceArea.Height * MenuScale);
				_currentLocationArea = new Rectangle(
					_defaultLocationArea.X,
					_defaultLocationArea.Y + (ModEntry.MonoThinFont.LineSpacing * 2),
					CurrentLocationSourceArea.Width * MenuScale,
					CurrentLocationSourceArea.Height * MenuScale);
			}
		}

		public override void RealignFloatingButtons()
		{
			base.RealignFloatingButtons();
		}

		protected override void AddActionButtons()
		{
			base.AddActionButtons();

			ViewScheduleButton = this.CreateActionButton(which: nameof(ViewScheduleButton));
			GoToSpawnButton = this.CreateActionButton(which: nameof(GoToSpawnButton));
			GoToButton = this.CreateActionButton(which: nameof(GoToButton));
			ReturnFromButton = this.CreateActionButton(which: nameof(ReturnFromButton));

			SidebarActionButtons.AddRange(new [] { ReturnFromButton, GoToButton, GoToSpawnButton, ViewScheduleButton });
		}

		protected override void ClickUpButton()
		{
			this.SetCharacter(character: null);
		}

		public void SetCharacter(NPC character)
		{
			SelectedChara = character;
			if (SelectedChara == null)
			{
				_defaultLocationName = null;
				_properties = null;
				_hoverTextCached = null;
			}
			else
			{
				_defaultLocationName = Game1.getLocationFromName(SelectedChara.DefaultMap).Name;
				_properties = new bool[]
				{
					SelectedChara.datable.Value,
					SelectedChara.CanSocialize,
					!SelectedChara.IsInvisible,
					SelectedChara.getMasterScheduleRawData() != null
				};
				_hoverTextCached = new string[]
				{
					ModEntry.Instance.i18n.Get("ui.characterlist.properties.birthday",
						tokens: new
						{
							SeasonName = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(
								Game1.content.LoadString("Strings\\StringsFromCSFiles:" + SelectedChara.Birthday_Season)),
							DayNumber = SelectedChara.Birthday_Day,
							DayName = Game1.shortDayDisplayNameFromDayOfSeason(SelectedChara.Birthday_Day)
						}),
					ModEntry.Instance.i18n.Get("ui.characterlist.properties.gender",
						tokens: new
						{
							MaleFemale = ModEntry.Instance.i18n.Get("ui.characterlist.properties." + (SelectedChara.Gender == 0 ? "male" : "female"))
						}),
					ModEntry.Instance.i18n.Get("ui.characterlist.properties.defaultlocation"),
					ModEntry.Instance.i18n.Get("ui.characterlist.properties.currentlocation"),
				};
			}
			this.RealignElements();
		}

		private void AddCharaButtons()
		{
			// Populate character catalogue
			Dictionary<string, string> NPCDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			foreach (string name in NPCDispositions.Keys)
			{
				string displayName = NPCDispositions[name].Split('/').Length > 11
					? NPCDispositions[name].Split('/')[11]
					: name;
				Names[name] = displayName;
			}

			foreach (KeyValuePair<string, string> namePair in Names)
			{
				if (NPCDispositions.ContainsKey(namePair.Key))
				{
					string[] location = NPCDispositions[namePair.Key].Split('/')[10].Split(' ');
					string textureName = NPC.getTextureNameForCharacter(namePair.Key);
					if (location.Length > 2)
					{
						NPC npc = new NPC(
							sprite: new AnimatedSprite("Characters\\" + textureName, 0, 16, 32),
							position: new Vector2(int.Parse(location[1]) * Game1.tileSize, int.Parse(location[2]) * Game1.tileSize),
							defaultMap: location[0],
							facingDir: 0,
							name: namePair.Key,
							schedule: null,
							portrait: Game1.content.Load<Texture2D>("Portraits\\" + textureName),
							eventActor: false);

						Heads.Add(new ClickableTextureComponent(
							name: namePair.Key,
							bounds: Rectangle.Empty,
							label: null,
							hoverText: namePair.Value,
							texture: npc.Sprite.Texture,
							sourceRect: npc.getMugShotSourceRect(),
							scale: _headScale));
					}
				}
			}
		}

		protected override void Hover(int x, int y)
		{
			if (IsOnHomePage)
			{
				if (Heads.FirstOrDefault(head => head.containsPoint(x, y)) is ClickableTextureComponent clickable && clickable != null)
					_hoverText = clickable.hoverText;
			}
			else
			{
				GoToButton.tryHover(x, y, maxScaleIncrease: ActionButtonHoverScale);
				ViewScheduleButton.tryHover(x, y, maxScaleIncrease: ActionButtonHoverScale);
				if (_birthdayArea.Contains(x, y))
				{
					_hoverText = _hoverTextCached[0];
				}
				else if (_genderArea.Contains(x, y))
				{
					_hoverText = _hoverTextCached[1];
				}
				else if (_propertiesArea.Contains(x, y))
				{
					string[] hoverText = new string[] { "datable", "sociable", "visible", "scheduled" };
					float localX = x - _propertiesArea.X;
					float ratio = localX / _propertiesArea.Width;
					int which = (int)(ratio * hoverText.Length);
					_hoverText = ModEntry.Instance.i18n.Get("ui.characterlist.properties." + hoverText[which],
						tokens: new
						{
							TrueFalse = _properties[which]
						});
				}
				else if (_defaultLocationArea.Contains(x, y))
				{
					_hoverText = _hoverTextCached[2];
				}
				else if (_currentLocationArea.Contains(x, y))
				{
					_hoverText = _hoverTextCached[3];
				}
			}
		}

		protected override void LeftClick(int x, int y, bool playSound)
		{
			if (SidebarActionButtons.Any())
			{
				if (IsOnHomePage)
				{
					ClickableTextureComponent button = Heads.FirstOrDefault(head => head.containsPoint(x, y));
					if (button != null)
					{
						this.SetCharacter(Game1.getCharacterFromName(name: button.name));
					}
				}
				else
				{
					if (ReturnFromButton.containsPoint(x, y))
					{
						if (!string.IsNullOrEmpty(ModEntry.Instance._originalLocation) && SelectedChara.currentLocation.Name != ModEntry.Instance._originalLocation)
						{
							// Return view to farmer if in another location
							Desktop.ReturnFromViewLocation();
						}
						else
						{
							// Pan view to farmer if in this location
							Game1.panScreen(
								x: (Game1.player.getTileX() * Game1.tileSize) - (Game1.viewport.Width / 2),
								y: (Game1.player.getTileY() * Game1.tileSize) - (Game1.viewport.Height / 2));
						}
					}
					else if (GoToButton.containsPoint(x, y))
					{
						string locationName = SelectedChara.currentLocation.Name;
						Point tilePosition = SelectedChara.getTileLocationPoint();
						if (locationName != Game1.currentLocation.Name)
						{
							// Cast view to characters in other locations
							Desktop.ViewLocation(locationName: locationName, tilePosition: tilePosition, notify: true);
						}
						else if (SelectedChara.getLocalPosition(Game1.viewport) is Vector2 v
							&& v.X > 0 && v.Y > 0 && v.X < Game1.viewport.Width && v.Y < Game1.viewport.Height)
						{
							// Pan view to characters in this location
							Game1.panScreen(
								x: (SelectedChara.getTileX() * Game1.tileSize) - (Game1.viewport.Width / 2),
								y: (SelectedChara.getTileY() * Game1.tileSize) - (Game1.viewport.Height / 2));
						}
					}
					else if (ViewScheduleButton.containsPoint(x, y))
					{
						IClickableMenu menu = Desktop.Taskbar.ClickTaskbarIcon(typeName: nameof(SchedulePreviewMenu), forceSelected: true);
						if (menu is SchedulePreviewMenu scheduleMenu && scheduleMenu.WindowBar != null)
						{
							scheduleMenu.SetCharacter(SelectedChara);
							return;
						}
					}
					else if (GoToSpawnButton.containsPoint(x, y))
					{
						string locationName = SelectedChara.DefaultMap;
						Point tilePosition = Utility.Vector2ToPoint(SelectedChara.DefaultPosition / Game1.tileSize);
						if (locationName != Game1.currentLocation.Name)
						{
							// Cast view to spawn positions in other locations
							Desktop.ViewLocation(locationName: locationName, tilePosition: tilePosition, notify: true);
						}
						else if (new Vector2(SelectedChara.DefaultPosition.X - Game1.viewport.X, SelectedChara.DefaultPosition.Y - Game1.viewport.Y) is Vector2 v
							&& v.X > 0 && v.Y > 0 && v.X < Game1.viewport.Width && v.Y < Game1.viewport.Height)
						{
							// Pan view to characters in this location
							Game1.panScreen(
								x: (int)(SelectedChara.DefaultPosition.X * Game1.tileSize) - (Game1.viewport.Width / 2),
								y: (int)(SelectedChara.DefaultPosition.Y * Game1.tileSize) - (Game1.viewport.Height / 2));
						}
					}
				}
			}
		}

		protected override void DrawContent(SpriteBatch b)
		{
			if (IsOnHomePage)
			{
				// Character heads
				foreach (ClickableTextureComponent head in Heads)
				{
					head.draw(b);
				}
			}
			else if (SelectedChara != null)
			{
				Vector2 position = ContentOrigin;

				string text;
				Vector2 textSize;
				int textWidth;

				// Character name
				textWidth = (int)(GoToButton.bounds.X - position.X);
				text = Game1.parseText(text: SelectedChara.Name, whichFont: HeadingTextFont, width: textWidth);
				this.DrawHeading(b, position: position, text: text, drawBackground: true,
					subheading: null, subheadingBelow: false,
					characterSprite: SelectedChara);

				// Character properties
				position.Y = _propertiesArea.Y;
				int xOffset, yOffset;
				// gender
				xOffset = SelectedChara.Gender == 0 ? 0 : GenderSourceArea.Width;
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: _genderArea,
					sourceRectangle: new Rectangle(GenderSourceArea.X + xOffset, GenderSourceArea.Y, GenderSourceArea.Width, GenderSourceArea.Height),
					color: Color.White);
				// birthday icon
				xOffset = Utility.getSeasonNumber(SelectedChara.Birthday_Season) * SeasonSourceArea.Width;
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: _birthdayArea,
					sourceRectangle: new Rectangle(SeasonSourceArea.X + xOffset, SeasonSourceArea.Y, SeasonSourceArea.Width, SeasonSourceArea.Height),
					color: Color.White);
				// birthday date
				yOffset = (int)ISUtilities.GetOffsetToCentre(
					dimensions: new Point(0, 7),
					bounds: new Point(9999, _birthdayArea.Height)).Y / 2;
				Utility.drawTinyDigits(toDraw: SelectedChara.Birthday_Day,
					b,
					position: new Vector2(_birthdayArea.X + _birthdayArea.Width + (Padding.X / 4), _birthdayArea.Y + yOffset),
					scale: MenuScale,
					layerDepth: 1f,
					c: Color.White);
				// properties
				for (int i = 0; i < _properties?.Length; ++i)
				{
					xOffset = i * PropertiesSourceArea.Width;
					yOffset = _properties[i] ? 0 : PropertiesSourceArea.Height;
					b.Draw(
						texture: ModEntry.Sprites,
						position: new Vector2(_propertiesArea.X + (xOffset * MenuScale), position.Y),
						sourceRectangle: new Rectangle(PropertiesSourceArea.X + xOffset, PropertiesSourceArea.Y + yOffset, PropertiesSourceArea.Width, PropertiesSourceArea.Height),
						color: Color.White,
						rotation: 0f,
						origin: Vector2.Zero,
						scale: MenuScale,
						effects: SpriteEffects.None,
						layerDepth: 1f);
				}

				// Character location
				SpriteFont font = ModEntry.MonoThinFont;

				position = Utility.PointToVector2(_defaultLocationArea.Location);
				b.Draw(
					texture: ModEntry.Sprites,
					position: position,
					sourceRectangle: DefaultLocationSourceArea,
					color: Color.White,
					rotation: 0f,
					origin: Vector2.Zero,
					scale: MenuScale,
					effects: SpriteEffects.None,
					layerDepth: 1f);

				text = ModEntry.Instance.i18n.Get("ui.tileinfo.locationformat",
					tokens: new
					{
						LocationName = _defaultLocationName,
						TilePosition = (SelectedChara.DefaultPosition / Game1.tileSize).ToString()
					});
				position.Y += this.DrawText(b,
					position: new Vector2(position.X + (DefaultLocationSourceArea.Width * MenuScale) + Padding.X, position.Y),
					text: text, font: font);

				position = Utility.PointToVector2(_currentLocationArea.Location);
				b.Draw(
					texture: ModEntry.Sprites,
					position: position,
					sourceRectangle: CurrentLocationSourceArea,
					color: Color.White,
					rotation: 0f,
					origin: Vector2.Zero,
					scale: MenuScale,
					effects: SpriteEffects.None,
					layerDepth: 1f);

				text = ModEntry.Instance.i18n.Get("ui.tileinfo.locationformat",
					tokens: new
					{
						LocationName = SelectedChara.currentLocation.Name,
						TilePosition = SelectedChara.getTileLocationPoint().ToString()
					});
				this.DrawText(b,
					position: new Vector2(position.X + (CurrentLocationSourceArea.Width * MenuScale) + Padding.X, position.Y),
					text: text, font: font);
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}

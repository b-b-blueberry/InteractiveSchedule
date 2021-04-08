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
		public ClickableTextureComponent ReturnFromButton, GoToButton, OpenScheduleButton, ViewSpawnButton;

		/// <summary> List of names for each <see cref="StardewValley.NPC"/> loaded in the NPCDispositions data file, paired with their display names.</summary>
		public Dictionary<string, string> Names = new Dictionary<string, string>();
		/// <summary> Clickable mugshot icons for each <see cref="StardewValley.NPC"/> appearing in <see cref="Names"/>.</summary>
		public List<ClickableTextureComponent> Heads = new List<ClickableTextureComponent>();

		public override bool IsOnHomePage => SelectedChara == null;
		public override bool IsUpButtonVisible => !IsOnHomePage;
		public override bool IsActionButtonSidebarVisible => !IsOnHomePage;

		/// <summary> Currently selected <see cref="StardewValley.NPC"/>, determines the sub-page contents.</summary>
		public NPC SelectedChara;

		/// <summary>  Scale for each clickable icon in <see cref="Heads"/>. </summary>
		private int _headScale;
		/// <summary> Number of <see cref="Heads"/> icons in each row to draw to the window.</summary>
		private int _headsWide;

		public CharacterListMenu(Point position) : base(position: position)
		{
			this.AddCharaButtons();
			this.RealignElements();
		}

		protected override void cleanupBeforeExit()
		{
			Names.Clear();
			Heads.Clear();
			ReturnFromButton = null;
			GoToButton = null;
			OpenScheduleButton = null;
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

			if (WindowBar != null)
			{
				// Character catalogue
				Rectangle headSize = Heads.FirstOrDefault()?.sourceRect ?? new Rectangle(0, 0, 16, 24);

				WindowBar.width = width = Math.Max(WindowBar.width, (_headsWide * headSize.Width) + (Padding.X * 2) + ActionButtonSidebarArea.Width);
				height = WindowBar.IsFullscreen
					? WindowBar.FullscreenHeight
					: (((Heads.Count / _headsWide) + 1) * headSize.Height * _headScale) + (Padding.Y * 2);

				int xToCentre = ((_headsWide * headSize.Width * _headScale) - ContentSafeArea.Width) / 2;

				for (int i = 0; i < Heads.Count; ++i)
				{
					int x = i % _headsWide * headSize.Width * _headScale;
					int y = i / _headsWide * headSize.Height * _headScale;
					Heads[i].bounds = new Rectangle(ContentSafeArea.X + xToCentre + x, ContentSafeArea.Y + y, headSize.Width * MenuScale, headSize.Height * MenuScale);
				}
			}
		}

		public override void RealignFloatingButtons()
		{
			base.RealignFloatingButtons();
		}

		protected override void AddActionButtons()
		{
			base.AddActionButtons();

			OpenScheduleButton = this.CreateActionButton(which: nameof(OpenScheduleButton));
			ViewSpawnButton = this.CreateActionButton(which: nameof(ViewSpawnButton));
			GoToButton = this.CreateActionButton(which: nameof(GoToButton));
			ReturnFromButton = this.CreateActionButton(which: nameof(ReturnFromButton));

			SidebarActionButtons.AddRange(new [] { ReturnFromButton, GoToButton, ViewSpawnButton, OpenScheduleButton });
		}

		protected override void ClickUpButton()
		{
			SelectedChara = null;
		}

		private void AddCharaButtons()
		{
			// Populate character catalogue
			Dictionary<string, string> NPCDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			foreach (string name in Game1.player.friendshipData.Keys)
			{
				string displayName = NPCDispositions.ContainsKey(name) && NPCDispositions[name].Split('/').Length > 11
					? NPCDispositions[name].Split('/')[11]
					: name;
				Names[name] = displayName;
			}

			foreach (KeyValuePair<string, string> namePair in Names)
			{
				if (NPCDispositions.ContainsKey(namePair.Key))
				{
					string[] location = NPCDispositions[namePair.Key].Split('/')[10].Split(' ');
					string texture_name = NPC.getTextureNameForCharacter(namePair.Key);
					if (location.Length > 2)
					{
						NPC npc = new NPC(
							sprite: new AnimatedSprite("Characters\\" + texture_name, 0, 16, 32),
							position: new Vector2(int.Parse(location[1]) * 64, int.Parse(location[2]) * 64),
							defaultMap: location[0],
							facingDir: 0,
							name: namePair.Key,
							schedule: null,
							portrait: Game1.content.Load<Texture2D>("Portraits\\" + texture_name),
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

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);

			if (!IsSelected || !ShouldDraw)
				return;

			if (SidebarActionButtons.Any())
			{
				if (IsOnHomePage)
				{
					ClickableTextureComponent button = Heads.FirstOrDefault(head => head.containsPoint(x, y));
					if (button != null)
					{
						SelectedChara = Game1.getCharacterFromName(name: button.name);
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
						if (SelectedChara.currentLocation.Name != Game1.currentLocation.Name)
						{
							// Cast view to characters in other locations
							Desktop.ViewLocation(locationName: SelectedChara.currentLocation.Name, tilePosition: SelectedChara.getTileLocationPoint(), notify: true);
						}
						else
						{
							// Pan view to characters in this location
							Game1.panScreen(
								x: (SelectedChara.getTileX() * Game1.tileSize) - (Game1.viewport.Width / 2),
								y: (SelectedChara.getTileY() * Game1.tileSize) - (Game1.viewport.Height / 2));
						}
					}
					else if (OpenScheduleButton.containsPoint(x, y))
					{
						ClickableTextureComponent icon = Desktop.Taskbar.Icons.First(i => i.name == nameof(SchedulePreviewMenu));
						IClickableMenu menu = Desktop.Taskbar.TryClickTaskbarIcon(icon.bounds.X, icon.bounds.Y);
						if (menu is SchedulePreviewMenu scheduleMenu && scheduleMenu.WindowBar != null)
						{
							scheduleMenu.WindowBar.IsMinimised = false;
							scheduleMenu.SetCharacter(SelectedChara);
							return;
						}
					}
					else if (ViewSpawnButton.containsPoint(x, y))
					{

					}
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			_hoverText = "";

			base.performHoverAction(x, y);

			if (!ShouldUpdateHoverText)
				return;

			if (IsOnHomePage)
			{
				if (Heads.FirstOrDefault(head => head.containsPoint(x, y)) is ClickableTextureComponent clickable && clickable != null)
					_hoverText = clickable.hoverText;
			}
			else
			{
				GoToButton.tryHover(x, y, maxScaleIncrease: ActionButtonHoverScale);
				OpenScheduleButton.tryHover(x, y, maxScaleIncrease: ActionButtonHoverScale);
			}
		}

		public override void DrawContent(SpriteBatch b)
		{
			if (IsOnHomePage)
			{
				// Character heads
				foreach (ClickableTextureComponent head in Heads)
				{
					head.draw(b);
				}
			}
			else
			{
				Vector2 position = Utility.PointToVector2(ContentSafeArea.Location);
				Vector2 positionChange = Vector2.Zero;

				string text;
				Vector2 textSize;
				int textWidth;

				Rectangle sourceRect = SelectedChara.getMugShotSourceRect();

				// Character name
				positionChange.X = (sourceRect.Width * MenuScale) + Padding.X;
				positionChange.Y = Padding.Y * 2;
				position.X += positionChange.X;
				textWidth = (int)(GoToButton.bounds.X - position.X);
				text = Game1.parseText(text: SelectedChara.displayName, whichFont: HeadingTextFont, width: textWidth);
				textSize = HeadingTextFont.MeasureString(text);
				this.DrawHeading(b, position: position, text: text, drawBackground: true);

				// Character sprite
				position.X -= positionChange.X;
				position.Y -= positionChange.Y;
				b.Draw(texture: SelectedChara.Sprite.Texture,
					position: position,
					sourceRectangle: sourceRect,
					color: Color.White,
					rotation: 0f, origin: Vector2.Zero, scale: MenuScale, effects: SpriteEffects.None, layerDepth: 1f);
				position.Y += positionChange.Y;

				// Character location
				positionChange.Y = textSize.Y + (Padding.Y * 2);
				position.Y += positionChange.Y;
				text = ModEntry.Instance.i18n.Get("ui.tileinfo.locationformat",
				new
				{
					LocationName = SelectedChara.currentLocation.Name,
					TilePosition = SelectedChara.getTileLocationPoint().ToString()
				});
				position.Y += this.DrawText(b, position: position, text: text);
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}

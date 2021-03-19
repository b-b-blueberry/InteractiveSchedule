using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface
{
	public class CharacterListMenu : WindowPage
	{
		public Dictionary<string, string> Names = new Dictionary<string, string>();
		public List<ClickableTextureComponent> Heads = new List<ClickableTextureComponent>();

		public override bool IsOnHomePage => SelectedChara == null;
		public override bool IsActionButtonSidebarVisible => !IsOnHomePage;

		public NPC SelectedChara;
		public ClickableTextureComponent GoToButton, OpenScheduleButton;

		private int _headScale;
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

			if (_parentMenu != null)
			{
				// Character catalogue
				Rectangle headSize = Heads.FirstOrDefault()?.sourceRect ?? new Rectangle(0, 0, 16, 24);

				_parentMenu.width = width = Math.Max(_parentMenu.width, (_headsWide * headSize.Width) + (Padding.X * 2) + ActionButtonSidebarArea.Width);
				height = ((WindowBar)_parentMenu).IsFullscreen
					? Game1.viewport.Height - _parentMenu.height
					: (((Heads.Count / _headsWide) + 1) * headSize.Height * _headScale) + (Padding.Y * 2);

				for (int i = 0; i < Heads.Count; ++i)
				{
					int x = i % _headsWide * headSize.Width * _headScale;
					int y = i / _headsWide * headSize.Height * _headScale;
					Heads[i].bounds = new Rectangle(ContentSafeArea.X + x, ContentSafeArea.Y + y, headSize.Width * MenuScale, headSize.Height * MenuScale);
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

			Rectangle sourceRect = new Rectangle(0, ActionButtonIconOffsetY, ActionButtonIconSize.X, ActionButtonIconSize.Y);
			GoToButton = this.CreateActionButton(which: nameof(GoToButton), sourceRect: sourceRect);
			SidebarActionButtons.Add(GoToButton);

			sourceRect.X += sourceRect.Width;
			OpenScheduleButton = this.CreateActionButton(which: nameof(OpenScheduleButton), sourceRect: sourceRect);
			SidebarActionButtons.Add(OpenScheduleButton);
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
				if (GoToButton.containsPoint(x, y))
				{
					if (SelectedChara.currentLocation.Name != Game1.currentLocation.Name)
					{
						// Cast view to characters in other locations
						Desktop.ViewLocation(locationName: SelectedChara.currentLocation.Name, tileLocation: SelectedChara.getTileLocationPoint());
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

				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			_hoverText = "";

			base.performHoverAction(x, y);

			if (!IsSelected || !ShouldDraw)
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

		public override void draw(SpriteBatch b)
		{
			base.draw(b);

			if (!ShouldDraw)
				return;

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
				text = ModEntry.Instance.i18n.Get("ui.character.locationformat",
				new {
					LocationName = SelectedChara.currentLocation.Name,
					TileLocation = SelectedChara.getTileLocationPoint().ToString()
				});
				position.Y += this.DrawText(b, position: position, text: text);
			}

			this.DrawFloatingActionButtons(b);
			this.DrawHoverText(b);
		}
	}
}

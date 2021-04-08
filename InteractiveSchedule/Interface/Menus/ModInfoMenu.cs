using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

namespace InteractiveSchedule.Interface.Menus
{
	public class ModInfoMenu : WindowPage
	{
		private string _headerText;
		private string _subheaderText;
		private string _bodyText;
		private string _creditText;

		public override bool IsOnHomePage => true;
		public override bool IsUpButtonVisible => false;
		public override bool IsActionButtonSidebarVisible => false;

		public ModInfoMenu(Point position) : base(position: position)
		{
			this.RealignElements();
		}

		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();

			IManifest manifest = ModEntry.Instance.Helper.ModRegistry.Get(ModEntry.Instance.Helper.ModRegistry.ModID).Manifest;
			_headerText = manifest.Name + " " + manifest.Version;
			_subheaderText = manifest.Description;
			_bodyText = ModEntry.Instance.i18n.Get("ui.modinfo.body");
			_creditText = ModEntry.Instance.i18n.Get("ui.modinfo.credit");
		}

		public override void RealignElements()
		{
			base.RealignElements();

			if (WindowBar != null)
			{
				width = WindowBar.width;
				height = WindowBar.IsFullscreen
					? WindowBar.FullscreenHeight
					: 80 * MenuScale;
			}
		}

		public override void RealignFloatingButtons()
		{
			base.RealignFloatingButtons();
		}

		protected override void ClickUpButton()
		{
			throw new NotImplementedException();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
		}

		public override void DrawContent(SpriteBatch b)
		{
			Rectangle avatarSource = Desktop.Taskbar.AvatarButton.sourceRect;
			Vector2 position = Utility.PointToVector2(ContentSafeArea.Location);
			string text;
			int textWidth;

			textWidth = ContentSafeArea.Width;
			text = Game1.parseText(text: _headerText, whichFont: HeadingTextFont, width: textWidth);
			position.Y += this.DrawHeading(b, position: position, text: text, drawBackground: true);
			Rectangle avatarDest = new Rectangle(
					(int)(position.X + HeadingTextFont.MeasureString(text).X - (avatarSource.Width * MenuScale) - Padding.X),
					(int)(position.Y),
					avatarSource.Width * MenuScale,
					avatarSource.Height * MenuScale);
			Desktop.Taskbar.DrawAvatarButton(b, avatarDest);

			text = Game1.parseText(text: _subheaderText, whichFont: BodyTextFont, width: textWidth - (avatarSource.X * MenuScale) - Padding.X);
			position.Y += this.DrawText(b, position: position, text: text);

			text = Game1.parseText(text: _bodyText, whichFont: BodyTextFont, width: textWidth);
			position.Y += this.DrawText(b, position: position, text: text);

			// Blueberry credit
			b.Draw(
				texture: Game1.objectSpriteSheet,
				position: position,
				sourceRectangle: Game1.getSourceRectForStandardTileSheet(tileSheet: Game1.objectSpriteSheet, tilePosition: 258, width: 16, height: 16),
				color: Color.White, rotation: 0f, origin: Vector2.Zero, scale: MenuScale, effects: SpriteEffects.None, layerDepth: 1f);
			position.X += (16 * MenuScale) + Padding.X;
			text = Game1.parseText(text: _creditText, whichFont: BodyTextFont, width: textWidth);
			position.Y += (16 * MenuScale) / 4;
			this.DrawText(b, position: position, text: text);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

namespace InteractiveSchedule.Interface
{
	public class ModInfoMenu : WindowPage
	{
		private string _headerText;
		private string _subheaderText;
		private string _bodyText;

		public override bool IsOnHomePage => true;
		public override bool IsActionButtonSidebarVisible => false;

		public ModInfoMenu(Point position) : base(position)
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
		}

		public override void RealignElements()
		{
			base.RealignElements();

			if (WindowBar != null)
			{
				width = WindowBar.width;
			}
			height = WindowBar != null && WindowBar.IsFullscreen ? Game1.viewport.Height - WindowBar.height : 64 * MenuScale;
		}

		public override void RealignFloatingButtons()
		{
			base.RealignFloatingButtons();
		}

		protected override void ClickUpButton()
		{
			throw new NotImplementedException();
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);

			if (!ShouldDraw)
				return;

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

			this.DrawFloatingActionButtons(b);
			this.DrawHoverText(b);
		}
	}
}

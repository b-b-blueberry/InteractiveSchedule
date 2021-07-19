using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface
{
	public class ViewingLocationWidget : CustomMenu
	{
		public ClickableTextureComponent ReturnViewButton;

		private string _text;
		private Point _dimensions;

		public ViewingLocationWidget()
		{
			ReturnViewButton = new ClickableTextureComponent(
				bounds: Rectangle.Empty,
				texture: ModEntry.Sprites,
				sourceRect: GetWidgetIconSourceRect(this.GetType().Name),
				scale: MenuScale);

			this.RealignElements();
		}

		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();

			_dimensions = new Point(200, (WidgetIconSize.Y * MenuScale) + (Padding.Y * 3));
		}

		public override void RealignElements()
		{
			_text = ModEntry.Instance.i18n.Get("ui.viewinglocationwidget.viewlocation.label", new { LocationName = Game1.player.viewingLocation })
					+ "\n" + ModEntry.Instance.i18n.Get("ui.viewinglocationwidget.farmerlocation.label", new { LocationName = ModEntry.Instance._originalLocation });
			_text = Game1.parseText(text: _text, whichFont: BodyTextFont, width: _dimensions.X);

			Vector2 textSize = BodyTextFont.MeasureString(_text);
			width = (int)textSize.X + (WidgetIconSize.X * MenuScale) + (Padding.X * 2);
			height = Math.Max(_dimensions.Y, (int)textSize.Y + Padding.Y);

			xPositionOnScreen = Game1.viewport.Width - width;
			yPositionOnScreen = 0;

			ReturnViewButton.bounds = new Rectangle(
				xPositionOnScreen + Padding.X,
				yPositionOnScreen + ((height - (WidgetIconSize.Y * MenuScale)) / 2),
				WidgetIconSize.X * MenuScale,
				WidgetIconSize.Y * MenuScale);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);

			if (this.isWithinBounds(x, y))
			{
				ModEntry.Instance.Desktop.ReturnFromViewLocation();
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);

			ReturnViewButton.tryHover(x, y, maxScaleIncrease: 0.4f);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);

			Vector2 position = new Vector2(xPositionOnScreen + Padding.X, yPositionOnScreen + Padding.Y);

			// Background
			Desktop.DrawGradient(b,
				area: new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height),
				colour: Color.CornflowerBlue,
				effects: SpriteEffects.FlipHorizontally);
			Desktop.DrawGradient(b,
				area: new Rectangle(xPositionOnScreen, yPositionOnScreen + height, width, 1 * MenuScale),
				colour: WindowBar.BorderColour,
				effects: SpriteEffects.FlipHorizontally);

			// Icon
			ReturnViewButton.draw(b);
			position.X += (WidgetIconSize.X * MenuScale) + Padding.X;

			// Text
			this.DrawText(b,
				position: position,
				text: _text,
				font: BodyTextFont,
				colour: Color.White,
				drawShadow: true);
		}
	}
}

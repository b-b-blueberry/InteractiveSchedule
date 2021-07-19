using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface.Components
{
	public abstract class ViewComponent : CustomMenu
	{
		public readonly Point RelativePosition;
		public readonly bool DrawBorder;
		public Color ViewFillColour;
		public Color ViewBorderColour;
		public Color PageColour;

		protected ViewComponent(IClickableMenu parentMenu, Point relativePosition, bool drawBorder)
			: base()
		{
			_parentMenu = parentMenu;
			RelativePosition = relativePosition;
			DrawBorder = drawBorder;
			this.RealignElements();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();

			ViewFillColour = Color.PapayaWhip * 0.5f;
			ViewBorderColour = Color.PaleVioletRed * 0.5f;
			IClickableMenu parentMenu = _parentMenu;
			while (!(parentMenu is WindowPage) && parentMenu != null)
				parentMenu = parentMenu.GetParentMenu();
			if (parentMenu is WindowPage windowPage)
				PageColour = windowPage.PageColour;
		}

		public override void RealignElements()
		{
			// View component area fills the remainder of the parent menu area starting from their initial offset
			xPositionOnScreen = _parentMenu.xPositionOnScreen + RelativePosition.X;
			yPositionOnScreen = _parentMenu.yPositionOnScreen + RelativePosition.Y;
			width = ((WindowPage)_parentMenu).ContentSafeArea.Width
				- RelativePosition.X;
			height = ((WindowPage)_parentMenu).ContentSafeArea.Height - RelativePosition.Y;
		}

		private void DrawView(SpriteBatch b)
		{
			// border as inner shadow:
			int lineWidth = BorderWidth * MenuScale;
			Rectangle destRect = new Rectangle(xPositionOnScreen, yPositionOnScreen + lineWidth, width, height);
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: destRect,
				color: ViewBorderColour);

			// fill:
			// canvas
			destRect.Width -= lineWidth;
			destRect.Height -= lineWidth;
			destRect.Y += lineWidth;
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: destRect,
				color: PageColour);
			// colour
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: destRect,
				color: ViewFillColour);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (DrawBorder)
			{
				this.DrawView(b);
			}
		}
	}
}

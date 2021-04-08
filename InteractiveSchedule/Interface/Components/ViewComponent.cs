using Microsoft.Xna.Framework;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface
{
	public abstract class ViewComponent : CustomMenu
	{
		public readonly Point RelativePosition;

		protected ViewComponent(IClickableMenu parentMenu, Point relativePosition) : base()
		{
			_parentMenu = parentMenu;
			RelativePosition = relativePosition;
			this.RealignElements();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
		}

		public override void RealignElements()
		{
			// View component area fills the remainder of the parent menu area starting from their initial offset
			xPositionOnScreen = _parentMenu.xPositionOnScreen + RelativePosition.X;
			yPositionOnScreen = _parentMenu.yPositionOnScreen + RelativePosition.Y;
			width = ((WindowPage)_parentMenu).width
				- (((WindowPage)_parentMenu).BorderWidth * 2)
				- ((WindowPage)_parentMenu).ActionButtonSidebarArea.Width
				- RelativePosition.X;
			height = ((WindowPage)_parentMenu).height - RelativePosition.Y;
		}
	}
}

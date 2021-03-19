using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface
{
	/// <summary>
	/// Root class for <see cref="WindowBar"/> and <see cref="WindowPage"/>.
	/// </summary>
	public abstract class WindowComponent : CustomMenu
	{
		public Desktop Desktop => ModEntry.Instance.Desktop;
		public abstract bool IsSelected { get; }

		protected WindowComponent() : base() {}

		public override void RealignElements()
		{
			if (_parentMenu == null)
				return;

			xPositionOnScreen = _parentMenu.xPositionOnScreen;
			yPositionOnScreen = _parentMenu.yPositionOnScreen + _parentMenu.height;
		}

		protected override void cleanupBeforeExit()
		{
			_parentMenu = null;
			_childMenu = null;
			base.cleanupBeforeExit();
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface.Elements
{
	public abstract class DependentCustomElement
	{
		protected readonly CustomMenu _parentMenu;
		public Point RelativePosition;

		protected DependentCustomElement(CustomMenu parentMenu, Point relativePosition)
		{
			this._parentMenu = parentMenu;
			this.RelativePosition = relativePosition;
			this.RealignElements();
		}

		public virtual void RealignElements() {}
	}

	public abstract class CustomElement : DependentCustomElement
	{
		public Point Dimensions;
		public Rectangle Bounds;

		protected CustomElement(CustomMenu parentMenu, Point relativePosition, Point dimensions)
			: base (parentMenu: parentMenu, relativePosition: relativePosition)
		{
			this.Dimensions = dimensions;
			this.RealignElements();
		}

		public override void RealignElements()
		{
			this.Bounds = new Rectangle(
				this._parentMenu.ContentSafeArea.X + this.RelativePosition.X,
				this._parentMenu.ContentSafeArea.Y + this.RelativePosition.Y,
				this.Dimensions.X,
				this.Dimensions.Y);
		}

		public abstract bool LeftClick();

		public virtual bool IsHovered(int x, int y)
		{
			return this.Bounds.Contains(x, y);
		}
	}
}

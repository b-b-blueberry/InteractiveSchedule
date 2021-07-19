using Microsoft.Xna.Framework;

namespace InteractiveSchedule.Interface
{
	public abstract class WindowModal : WindowPage
	{
		public override bool IsOnHomePage => true;
		public override bool IsUpButtonVisible => false;
		public override bool IsActionButtonSidebarVisible => false;


		protected WindowModal(WindowPage modalParent = null)
			: base(Point.Zero)
		{
			_parentMenu = modalParent;
			this.RealignElements();
		}

		protected override void ClickUpButton() {}
	}
}

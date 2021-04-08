using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface.Menus
{
	public class SchedulePreviewMenu : WindowPage
	{
		public NPC SelectedChara;

		public override bool IsOnHomePage => CurrentPage == Page.Home;
		public override bool IsUpButtonVisible => !IsOnHomePage && CurrentPage != Page.Overview;
		public override bool IsActionButtonSidebarVisible => true;
		public enum Page
		{
			Home,
			Overview,
			CurrentPoint,

		}
		public Page CurrentPage = Page.Home;
		public int CurrentSchedulePoint = 0;
		public readonly List<Point> CurrentSchedule = new List<Point>();

		private readonly ScrollableListViewComponent _schedulesAvailableList;


		public SchedulePreviewMenu(Point position, WindowPage modalParent = null)
			: base(position, modalParent)
		{
			Point relativePosition = new Point(
					BorderSafeArea.X - xPositionOnScreen,
					BorderSafeArea.Y - yPositionOnScreen
					+ (int)BodyTextFont.MeasureString("Hello").Y
					+ (Padding.Y * 2));
			Dictionary<string, string> masterSchedule = SelectedChara.getMasterScheduleRawData();
			_schedulesAvailableList = new ScrollableListViewComponent(
				parentMenu: this,
				relativePosition: relativePosition,
				isHorizontal: false,
				items: masterSchedule.Keys.ToList().Cast<object>().ToList());
		}

		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
		}

		public override void RealignElements()
		{
			base.RealignElements();

			if (WindowBar != null)
			{
				WindowBar.width = width = 700;
				height = 400;
			}
		}

		protected override void ClickUpButton()
		{
			throw new NotImplementedException();
		}

		protected override void AddActionButtons()
		{
			base.AddActionButtons();
		}

		public void SetCharacter(NPC npc)
		{
			SelectedChara = npc;
			CurrentPage = Page.Overview;
			CurrentSchedule.Clear();
			CurrentSchedulePoint = 0;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (CurrentPage == Page.Overview)
				_schedulesAvailableList.receiveLeftClick(x, y, playSound);
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (CurrentPage == Page.Overview)
				_schedulesAvailableList.receiveKeyPress(key);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (CurrentPage == Page.Overview)
				_schedulesAvailableList.performHoverAction(x, y);
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (CurrentPage == Page.Overview)
				_schedulesAvailableList.update(time);
		}

		public override void DrawContent(SpriteBatch b)
		{
			Vector2 position = Utility.PointToVector2(ContentSafeArea.Location);
			if (CurrentPage == Page.Home)
			{
				position.Y += this.DrawHeading(b,
					position: position,
					text: ModEntry.Instance.i18n.Get("ui.schedulepreview.home.heading"),
					drawBackground: true);
				position.Y += this.DrawText(b,
					position: position,
					text: ModEntry.Instance.i18n.Get("ui.schedulepreview.home.text"));
			}
			else if (CurrentPage == Page.Overview)
			{
				position.Y += this.DrawSubheading(b,
					position: position,
					text: SelectedChara?.Name);
				position.Y += this.DrawText(b,
					position: position,
					text: "OverviewPage\nnothing to see");

				_schedulesAvailableList.draw(b);
			}
			else if (CurrentPage == Page.CurrentPoint)
			{
				position.Y += this.DrawText(b,
					position: position,
					text: "CurrentPoint page");
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}

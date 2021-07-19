using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface.Menus
{
	public class SchedulePreviewMenu : WindowPage
	{
		public ClickableTextureComponent ViewCharacterButton, EditListButton;
		public NPC SelectedChara { get; private set; }

		public override bool IsOnHomePage => CurrentPage == Page.Home;
		public override bool IsUpButtonVisible => !IsOnHomePage && CurrentPage != Page.Overview;
		public override bool IsActionButtonSidebarVisible => true;
		public enum Page
		{
			Home,
			Overview,
			Entry,
			Point,
		}
		private Page _currentPage = Page.Home;
		public Page CurrentPage {
			get => _currentPage;
			set
			{
				_currentPage = value;
				EditListButton.visible = ViewCharacterButton.visible = _currentPage == Page.Overview;
			}
		}
		public string CurrentEntry { get; private set; } = "";
		public int CurrentPoint { get; private set; } = 0;

		private Components.ScrollableListViewComponent _masterScheduleView;
		private Components.ScrollableListViewComponent _scheduleEntryView;


		public SchedulePreviewMenu(Point position) : base(position: position)
		{
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

			_masterScheduleView?.RealignElements();
			_scheduleEntryView?.RealignElements();
		}

		public override void RealignFloatingButtons()
		{
			base.RealignFloatingButtons();
			/*
			if (_masterScheduleView != null)
			{
				Vector2 offset = ISUtilities.GetOffsetToCentre(
					dimensions: new Vector2(ActionButtonIconSize.X, ActionButtonIconSize.Y) * MenuScale,
					bounds: new Point(9999, HeadingHeight));
				offset.X = (ActionButtonSize.X * MenuScale) + Padding.X;
				Point relativePosition = new Point(
					BorderSafeArea.Width - (int)offset.X,
					(int)offset.Y);
				FloatingActionButtons[ViewCharacterButton] = relativePosition;
				relativePosition.X -= (int)offset.X;
				FloatingActionButtons[EditListButton] = relativePosition;
			}*/
		}

		protected override void AddActionButtons()
		{
			base.AddActionButtons();

			ViewCharacterButton = this.CreateActionButton(which: nameof(ViewCharacterButton));
			EditListButton = this.CreateActionButton(which: nameof(EditListButton));
			SidebarActionButtons.AddRange(new[] { ViewCharacterButton, EditListButton });
		}

		protected override void ClickUpButton()
		{
			switch (CurrentPage)
			{
				case Page.Point:
					CurrentPage = Page.Entry;
					break;
				case Page.Entry:
					CurrentPage = Page.Overview;
					break;
			}
		}

		public void SetCharacter(NPC npc)
		{
			SelectedChara = npc;
			CurrentPage = Page.Overview;
			CurrentEntry = "";
			CurrentPoint = 0;
			this.SetMasterSchedule(npc: npc);
		}

		public void SetMasterSchedule(NPC npc)
		{
			// Reinitialise master schedule list
			Point relativePosition = new Point(
					ContentSafeArea.X - xPositionOnScreen,
					ContentSafeArea.Y - yPositionOnScreen
					+ (int)(HeadingTextFont.MeasureString(npc?.Name ?? "Hello").Y)
					+ (Padding.Y * 2));

			Dictionary<string, string> masterSchedule = npc.getMasterScheduleRawData();
			List<string> scheduleEntries = masterSchedule?.Keys.ToList() ?? new List<string>();
			_masterScheduleView = new Components.ScrollableListViewComponent(
				parentMenu: this,
				relativePosition: relativePosition,
				isHorizontal: false,
				items: Elements.StringListElement.BuildList(
					parentMenu: this,
					relativePosition: relativePosition,
					source: scheduleEntries),
				drawBorder: true,
				onItemClicked: delegate (int which)
				{
					this.SetScheduleEntry(
						npc: npc,
						masterSchedule: masterSchedule,
						entry: ((Elements.StringListElement)_masterScheduleView.Items[which]).Text);
					this.RealignElements();
				});
			this.RealignElements();
		}

		public void SetScheduleEntry(NPC npc, Dictionary<string, string> masterSchedule, string entry)
		{
			CurrentPage = Page.Entry;
			CurrentEntry = entry;
			CurrentPoint = 0;
			List<Elements.ListElement> items = Elements.ScheduleListElement.BuildList(
				parentMenu: this,
				relativePosition: _masterScheduleView.RelativePosition,
				dailySchedule: masterSchedule[entry]);
			_scheduleEntryView = new Components.ScrollableListViewComponent(
				parentMenu: this,
				relativePosition: _masterScheduleView.RelativePosition,
				isHorizontal: false,
				items: items,
				drawBorder: true,
				onItemClicked: delegate (int which)
				{
					string point = ((Elements.ScheduleListElement)items[which]).Text;
					string[] split = point.Split(' ');
					
					if ((split.First() == "GOTO" || split.First() == "MAIL") && masterSchedule.ContainsKey(split.Last()))
					{
						this.SetScheduleEntry(npc: npc, masterSchedule: masterSchedule, entry: split.Last());
					}
					else if (split.First() != "NOT")
					{
						CurrentPoint = int.Parse(split.First());
						this.CurrentPage = Page.Point;
					}
					else
					{
						Desktop.PlaySound("cancel");
						return;
					}
					this.RealignElements();
				});
			this.RealignElements();
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (CurrentPage == Page.Overview)
				_masterScheduleView.receiveKeyPress(key);
			if (CurrentPage == Page.Entry)
				_scheduleEntryView.receiveKeyPress(key);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (CurrentPage == Page.Overview)
				_masterScheduleView.receiveScrollWheelAction(direction);
			if (CurrentPage == Page.Entry)
				_scheduleEntryView.receiveScrollWheelAction(direction);
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (CurrentPage == Page.Overview)
				_masterScheduleView.update(time);
			if (CurrentPage == Page.Entry)
				_scheduleEntryView.update(time);
		}

		protected override void Hover(int x, int y)
		{
			if (CurrentPage == Page.Overview)
				_masterScheduleView.performHoverAction(x, y);
			if (CurrentPage == Page.Entry)
				_scheduleEntryView.performHoverAction(x, y);
		}

		protected override void LeftClick(int x, int y, bool playSound)
		{
			if (CurrentPage == Page.Overview)
			{
				_masterScheduleView.receiveLeftClick(x, y, playSound);
				if (ViewCharacterButton.containsPoint(x, y))
				{
					IClickableMenu menu = Desktop.Taskbar.ClickTaskbarIcon(nameof(CharacterListMenu), forceSelected: true);
					if (menu is CharacterListMenu characterMenu && characterMenu.WindowBar != null)
					{
						characterMenu.SetCharacter(this.SelectedChara);
					}
					return;
				}
				if (EditListButton.containsPoint(x, y))
				{
					return;
				}
			}
			else if (CurrentPage == Page.Entry)
			{
				_scheduleEntryView.receiveLeftClick(x, y, playSound);
			}
		}

		protected override void DrawContent(SpriteBatch b)
		{
			Vector2 position = ContentOrigin;
			if (CurrentPage == Page.Home)
			{
				position.Y += this.DrawHeading(b,
					position: position,
					text: ModEntry.Instance.i18n.Get("ui.schedulepreview.home.heading"),
					drawBackground: true).Y;
				position.Y += this.DrawText(b,
					position: position,
					text: ModEntry.Instance.i18n.Get("ui.schedulepreview.home.text"));
			}
			else
			{
				Vector2 positionChange = Vector2.Zero;

				string heading;
				string subheading = "‣  ";
				int textWidth;
				Vector2 headingSize;

				switch (CurrentPage)
				{
					case Page.Overview:
						subheading += ModEntry.Instance.i18n.Get("ui.schedulepreview.overview.heading");
						break;
					case Page.Entry:
						subheading += CurrentEntry;
						break;
					case Page.Point:
						subheading += $"{CurrentEntry}  {subheading}{CurrentPoint}";
						break;
					default:
						subheading += "null";
						break;
				}

				if (SelectedChara != null)
				{
					// Character name
					textWidth = this.ContentSafeArea.Width - (int)positionChange.X;
					heading = Game1.parseText(text: SelectedChara.displayName, whichFont: HeadingTextFont, width: textWidth);
					headingSize = this.DrawHeading(b, position: position, text: heading,
						drawBackground: true,
						subheading: subheading, subheadingBelow: false,
						characterSprite: SelectedChara);
				}
				else
				{
					// Schedule name
					heading = "<CHARACTER NAME>";
					headingSize = this.DrawHeading(b,
						position: position,
						text: heading,
						drawBackground: true);
				}

				position.Y += headingSize.Y;

				if (CurrentPage == Page.Overview)
				{
					_masterScheduleView.draw(b);
				}
				else if (CurrentPage == Page.Entry)
				{
					_scheduleEntryView.draw(b);
				}
				else if (CurrentPage == Page.Point)
				{

				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}

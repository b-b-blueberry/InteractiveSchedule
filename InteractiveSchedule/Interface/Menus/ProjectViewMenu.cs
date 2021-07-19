using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Menus;

namespace InteractiveSchedule.Interface.Menus
{
	public class ProjectViewMenu : WindowPage
	{
		public ClickableTextureComponent NewProjectButton;

		public override bool IsOnHomePage => ActiveProject == null;
		public override bool IsUpButtonVisible => ActiveProject != null;
		public override bool IsActionButtonSidebarVisible => true;

		private Components.TabViewComponent _projectTabs;
		private Components.ScrollableListViewComponent _projectList;
		private Data.Project _activeProject;
		public Data.Project ActiveProject
		{
			get => _activeProject;
			set
			{
				_activeProject = value;
				this.RealignElements();
			}
		}


		public ProjectViewMenu(Point position)
			: base(position: position)
		{
			_projectTabs = new Views.ProjectTabView(parentMenu: this, relativePosition: Point.Zero);
			this.ReloadProjects();
		}

		public void ReloadProjects()
		{
			Point relativePosition = new Point(
				(int)ContentSafeOffset.X,
				(int)ContentSafeOffset.Y + HeadingHeight + Padding.Y);
			List<Data.Project> projects = ModEntry.Instance.Projects.Keys.ToList();
			_projectList = new Components.ScrollableListViewComponent(
				parentMenu: this,
				relativePosition: relativePosition,
				isHorizontal: false,
				items: Elements.ProjectListElement.BuildList(
					parentMenu: this,
					relativePosition: relativePosition,
					projects: projects),
				drawBorder: true,
				onItemClicked: delegate (int which)
				{
					ActiveProject = projects[which];
				});
			ActiveProject = null;
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
				WindowBar.width = width = 800;
				height = WindowBar.IsFullscreen
					? WindowBar.FullscreenHeight
					: 600;
			}

			_projectList?.RealignElements();
			_projectTabs?.RealignElements();
		}

		public override void RealignFloatingButtons()
		{
			base.RealignFloatingButtons();
		}

		protected override void AddActionButtons()
		{
			base.AddActionButtons();

			NewProjectButton = this.CreateActionButton(nameof(NewProjectButton));

			SidebarActionButtons.AddRange(new[] { NewProjectButton });
		}

		protected override void ClickUpButton()
		{
			ActiveProject = null;
		}

		protected override void Hover(int x, int y)
		{

		}

		protected override void LeftClick(int x, int y, bool playSound)
		{
			// Action buttons
			if (SidebarActionButtons.Any())
			{
				if (NewProjectButton.containsPoint(x, y))
				{
					Data.Project.Make();
					this.ReloadProjects();
					return;
				}
			}

			// Views
			if (IsOnHomePage)
			{
				_projectList.receiveLeftClick(x, y, playSound);
			}
			else
			{
				_projectTabs.receiveLeftClick(x, y, playSound);
			}
		}

		protected override void DrawContent(SpriteBatch b)
		{
			string text;
			SpriteFont font = BodyTextFont;
			Vector2 position = ContentOrigin;

			if (IsOnHomePage)
			{
				text = ModEntry.Instance.i18n.Get("ui.projectview.home.heading");
				position.Y += this.DrawHeading(b,
					position: position,
					text: text,
					drawBackground: true).Y;
				if (_projectList.Items.Any())
				{
					_projectList.draw(b);
				}
				else
				{
					text = ModEntry.Instance.i18n.Get("ui.projectview.text.none");
					this.DrawText(b,
						position: ContentOrigin + ISUtilities.GetOffsetToCentreText(
							font: font,
							text: text,
							bounds: new Point(ContentSafeArea.Width, ContentSafeArea.Height),
							wrap: true),
						text: text,
						font: font);
				}
			}
			else
			{
				_projectTabs.draw(b);
			}
		}
	}
}

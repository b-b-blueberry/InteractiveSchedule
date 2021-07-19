using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface.Views
{
	public class ProjectTabView : Components.TabViewComponent
	{
		public const string ProjectTabId = "project";
		public const string ManifestTabId = "manifest";
		public const string FilesTabId = "files";

		private Elements.Checkbox _projectIsActiveCheckbox;


		public ProjectTabView(CustomMenu parentMenu, Point relativePosition)
			: base(parentMenu: parentMenu, relativePosition: relativePosition)
		{
			this.AddTabs(new [] { ProjectTabId, ManifestTabId, FilesTabId });
		}

		public override void RealignElements()
		{
			base.RealignElements();

			if (_projectIsActiveCheckbox == null)
			{
				_projectIsActiveCheckbox = new Elements.Checkbox(
					parentMenu: this,
					relativePosition: new Point(
						(int)this.ContentSafeOffset.X,
						(int)this.ContentSafeOffset.Y),
					onCheckedBehaviour: delegate ()
					{
						Log.W("Clicked project checkbox");
					},
					text: ModEntry.Instance.i18n.Get("ui.projectview.project.isactive"),
					initialState: true,
					isEnabled: true);
			}
			_projectIsActiveCheckbox.RealignElements();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (_projectIsActiveCheckbox.IsHovered(x, y))
				_projectIsActiveCheckbox.LeftClick();
		}

		public override void DrawContent(SpriteBatch b)
		{
			Data.Project project = ((Menus.ProjectViewMenu)_parentMenu).ActiveProject;
			Vector2 position = ContentOrigin;
			if (ActiveTab == ProjectTabId && project != null)
			{
				_projectIsActiveCheckbox.Draw(b);
				position.Y = _projectIsActiveCheckbox.Bounds.Y + _projectIsActiveCheckbox.Bounds.Height + Padding.Y;

				position.Y += this.DrawText(b,
					position: position,
					text: ModEntry.Instance.i18n.Get("ui.projectview.project.guid", tokens: new
					{
						Value = project.Guid
					}),
					font: BodyTextFont,
					colour: BodyTextColour);

				position.Y += this.DrawText(b,
					position: position,
					text: ModEntry.Instance.i18n.Get("ui.projectview.project.name"),
					font: BodyTextFont,
					colour: BodyTextColour);
			}
			else if (ActiveTab == ManifestTabId)
			{

			}
			else if (ActiveTab == FilesTabId)
			{

			}
		}
	}
}

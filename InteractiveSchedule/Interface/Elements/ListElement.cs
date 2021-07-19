using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface.Elements
{
	public abstract class ListElement : DependentCustomElement
	{
		protected static readonly int DefaultItemHeight = 14 * CustomMenu.MenuScale;
		public int ItemHeight;
		private int _itemWidth;
		public int ItemWidth
		{
			get => _itemWidth;
			set
			{
				_itemWidth = value;
				this.RealignElements();
			}
		}
		public bool CanBeClicked = true;


		protected ListElement(CustomMenu parentMenu, Point relativePosition)
			: base(parentMenu: parentMenu, relativePosition: relativePosition)
		{}

		public abstract void Draw(SpriteBatch b, Rectangle bounds, Vector2 relativePosition);

		// how do i create a virtual static method BuildList??? nobody knows. separate declarations ho
	}

	public class StringListElement : ListElement
	{
		public string DisplayText;
		public readonly string Text;


		public StringListElement(CustomMenu parentMenu, Point relativePosition, string text)
			: base(parentMenu: parentMenu, relativePosition: relativePosition)
		{
			this.ItemHeight = CustomMenu.TextBoxFont.LineSpacing;
			this.DisplayText = this.Text = text;
			this.RealignElements();
		}

		public override void RealignElements()
		{
			this.DisplayText = ISUtilities.GetSingleLineEllipsisString(
				text: this.Text,
				font: CustomMenu.TextBoxFont,
				width: this.ItemWidth);
		}

		public override void Draw(SpriteBatch b, Rectangle bounds, Vector2 relativePosition)
		{
			Vector2 position = new Vector2(bounds.X + relativePosition.X, bounds.Y + relativePosition.Y);

			this._parentMenu.DrawText(b,
				position: position,
				text: this.DisplayText,
				font: CustomMenu.TextBoxFont,
				colour: CustomMenu.BodyTextColour);
		}

		public static List<ListElement> BuildList(CustomMenu parentMenu, Point relativePosition, List<string> source)
		{
			return source
				.ConvertAll(s => new StringListElement(parentMenu: parentMenu, relativePosition: relativePosition, text: s))
				.ConvertAll(e => e as ListElement);
		}
	}

	public class ScheduleListElement : ListElement
	{
		public string DisplayText;
		public readonly string Text;
		public readonly Icons Icon;
		public enum Icons
		{
			None,
			Animation,
			Dialogue
		}


		public ScheduleListElement(CustomMenu parentMenu, Point relativePosition, string raw)
			: base(parentMenu: parentMenu, relativePosition: relativePosition)
		{
			this.ItemHeight = CustomMenu.TextBoxFont.LineSpacing;
			string[] fields = raw.Split(' ');
			int fieldsBeforeExtraData = 4;
			this.DisplayText = this.Text = string.Join(" ", fields.Take(fieldsBeforeExtraData));

			if (fields.Length > fieldsBeforeExtraData && !int.TryParse(fields.Last(), out int facingDirection))
			{
				// Parse schedule entries with unique fields to add icons, ignoring optional 'facingDirection' field
				this.Icon = fields[fields.Length - 1].Contains('\\')
					? Icons.Dialogue
					: Icons.Animation;
			}
			if (fields.First().StartsWith("NOT"))
			{
				// 'NOT' entries have no real implementation, so they have no page to display
				this.CanBeClicked = false;
			}
		}

		public static List<ListElement> BuildList(CustomMenu parentMenu, Point relativePosition, string dailySchedule)
		{
			List<string> entries = dailySchedule.Split('/').ToList();
			if (entries.Count > 1 && entries[1].StartsWith("GOTO") && entries[0].StartsWith("MAIL"))
			{
				// Merge 'MAIL' entries with their corresponding 'GOTO', redirect to the page for that entry
				entries[0] += " " + entries[1];
				entries.RemoveAt(1);
			}

			return entries
				.ConvertAll(s => new ScheduleListElement(parentMenu: parentMenu, relativePosition: relativePosition, raw: s))
				.ConvertAll(e => e as ListElement);
		}

		public override void RealignElements()
		{
			this.DisplayText = ISUtilities.GetSingleLineEllipsisString(
				text: this.Text,
				font: CustomMenu.TextBoxFont,
				width: this.ItemWidth);
		}

		public override void Draw(SpriteBatch b, Rectangle bounds, Vector2 relativePosition)
		{
			Vector2 position = new Vector2(bounds.X + relativePosition.X, bounds.Y + relativePosition.Y);

			this._parentMenu.DrawText(b,
				position: position,
				text: this.DisplayText,
				font: CustomMenu.TextBoxFont,
				colour: CustomMenu.BodyTextColour);

			if (this.Icon != Icons.None)
			{
				Rectangle sourceRect = CustomMenu.GetIconSourceRect(this.Icon == Icons.Animation ? "AnimationsMenu" : "DialogueMenu");
				position.X = bounds.X - relativePosition.X + bounds.Width - (sourceRect.Width * CustomMenu.MenuScale);
				b.Draw(
					texture: ModEntry.Sprites,
					position: position,
					sourceRectangle: sourceRect,
					color: Color.White,
					rotation: 0f,
					origin: Vector2.Zero,
					scale: CustomMenu.MenuScale,
					effects: SpriteEffects.None,
					layerDepth: 1f);
			}
		}
	}

	public class ProjectListElement : ListElement
	{
		public string DisplayText;
		public Data.Project Project;


		public ProjectListElement(CustomMenu parentMenu, Point relativePosition, Data.Project project)
			: base(parentMenu: parentMenu, relativePosition: relativePosition)
		{
			this.ItemHeight = Game1.smallFont.LineSpacing + ModEntry.MonoThinFont.LineSpacing + (8 * CustomMenu.MenuScale);
			this.Project = project;
		}

		public static List<ListElement> BuildList(CustomMenu parentMenu, Point relativePosition, List<Data.Project> projects)
		{
			return projects
				.ConvertAll(p => new ProjectListElement(parentMenu: parentMenu, relativePosition: relativePosition, project: p))
				.ConvertAll(e => e as ListElement);
		}

		public override void RealignElements()
		{
			this.DisplayText = ISUtilities.GetSingleLineEllipsisString(
				text: this.Project?.Name ?? ModEntry.Instance.i18n.Get("ui.projectview.title.none"),
				font: CustomMenu.TextBoxFont,
				width: this.ItemWidth);
		}

		public override void Draw(SpriteBatch b, Rectangle bounds, Vector2 relativePosition)
		{
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: new Rectangle(bounds.X, bounds.Y, bounds.Width, (int)CustomMenu.BodyTextFont.MeasureString(this.DisplayText).Y),
				color: ((WindowPage)this._parentMenu).SidebarColour * CustomMenu.ShadowOpacity);

			Vector2 position = new Vector2(bounds.X + relativePosition.X, bounds.Y + relativePosition.Y);

			this._parentMenu.DrawText(b,
				position: position,
				text: DisplayText,
				font: CustomMenu.BodyTextFont,
				colour: CustomMenu.HeadingTextColour,
				drawShadow: true);
			
		}
	}
}

using Microsoft.Xna.Framework;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace InteractiveSchedule.Interface.Modals
{
	public class LocationSelectModal : WindowModal
	{
		public ClickableTextureComponent GoButton;
		public Components.TextBoxComponent LocationTextBox;


		public LocationSelectModal(WindowPage parent)
			: base(modalParent: parent) {}

		protected override void cleanupBeforeExit()
		{
			GoButton = null;
			LocationTextBox.exitThisMenu();
			LocationTextBox = null;

			base.cleanupBeforeExit();
		}

		public override void RealignElements()
		{
			const int charsWide = 24;
			const int charsTall = 4;

			Vector2 textTitleDimension = new Vector2(
				Game1.smallFont.MeasureString("This is a multi-line text box.").X,
				Game1.smallFont.LineSpacing);

			Vector2 textBoxPosition = new Vector2(
				BorderWidth + Padding.X,
				BorderWidth + Padding.Y + textTitleDimension.Y + (Padding.Y * 2));

			if (LocationTextBox == null)
			{
				LocationTextBox = new Components.TextBoxComponent(
					parentMenu: this,
					defaultText: "",
					scrollable: true,
					showLineNumbers: true,
					singleLineOnly: false,
					numbersOnly: false,
					relativePosition: Utility.Vector2ToPoint(textBoxPosition),
					columns: charsWide,
					rows: charsTall);
			}

			Vector2 dimensions = new Vector2(
				Math.Max(textTitleDimension.X, LocationTextBox.width + ActionButtonSize.X + (Padding.X * 4)) + (Padding.X * 2),
				textTitleDimension.Y + LocationTextBox.height + BorderWidth + (Padding.Y * 4));

			width = (int)dimensions.X;
			height = (int)dimensions.Y;

			base.RealignElements();

			LocationTextBox.RealignElements();
		}

		public override void RealignFloatingButtons()
		{
			Point relativePosition = new Point(
				ContentSafeArea.Width - (ActionButtonSize.X * MenuScale),
				ContentSafeArea.Height - (ActionButtonSize.Y * MenuScale / 2) - (Padding.Y * 4));
			FloatingActionButtons[GoButton] = relativePosition;

			base.RealignFloatingButtons();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
		}

		protected override void AddActionButtons()
		{
			base.AddActionButtons();

			GoButton = this.CreateActionButton(which: nameof(GoButton));
		}

		public override void receiveKeyPress(Keys key)
		{
			if (!IsSelected || !ShouldDraw)
				return;

			base.receiveKeyPress(key);

			LocationTextBox.receiveKeyPress(key);
		}

		protected override void Hover(int x, int y) { }

		protected override void LeftClick(int x, int y, bool playSound)
		{
			if (_parentMenu is WindowPage windowPage)
			{
				if (!windowPage.IsSelected || !windowPage.ShouldDraw)
					return;

				LocationTextBox.receiveLeftClick(x, y, playSound);

				if (GoButton != null)
				{
					// Clicked search for location to warp button
					if (GoButton.containsPoint(x, y))
					{
						GameLocation location = string.IsNullOrEmpty(LocationTextBox.TextBox.Text)
							? null
							: Utility.fuzzyLocationSearch(LocationTextBox.TextBox.Text);
						if (location == null)
						{
							Desktop.PlaySound("cancel");
						}
						else
						{
							windowPage.ModalWindow = null;
							Desktop.ViewLocation(locationName: location.Name, tilePosition: Point.Zero, notify: true);
						}
						return;
					}
				}
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			LocationTextBox.update(time);
		}

		protected override void DrawContent(SpriteBatch b)
		{
			Vector2 position = ContentOrigin;
			position.Y += Padding.Y;
			//this.DrawText(b, position: position, text: ModEntry.Instance.i18n.Get("ui.locationselect.label"));
			this.DrawText(b, position: position, text: "This is a multi-line text box!");

			LocationTextBox.draw(b);
		}
	}
}

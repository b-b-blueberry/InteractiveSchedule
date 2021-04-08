using Microsoft.Xna.Framework;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace InteractiveSchedule.Interface.Modals
{
	public class LocationSelectModal : WindowPage
	{
		public ClickableTextureComponent GoButton;
		public TextBoxComponent LocationTextBox;
		public override bool IsOnHomePage => true;
		public override bool IsUpButtonVisible => false;
		public override bool IsActionButtonSidebarVisible => false;


		public LocationSelectModal(Point position, WindowPage parent)
			: base(position: position, modalParent: parent) {}

		protected override void cleanupBeforeExit()
		{
			GoButton = null;
			LocationTextBox.exitThisMenu();
			LocationTextBox = null;

			base.cleanupBeforeExit();
		}

		public override void RealignElements()
		{
			width = 150 * MenuScale;
			height = 48 * MenuScale;

			base.RealignElements();

			if (LocationTextBox == null)
			{
				LocationTextBox = new TextBoxComponent(
					parentMenu: this,
					defaultText: "",
					scrollable: true,
					showLineNumbers: true,
					singleLineOnly: false,
					numbersOnly: false,
					bounds: new Rectangle(
						ContentSafeArea.X - xPositionOnScreen,
						GoButton.bounds.Y - yPositionOnScreen - Padding.Y,
						ContentSafeArea.Width - (ActionButtonSize.X * MenuScale) - (Padding.X * 3),
						//(int)(BodyTextFont.MeasureString("Everywhere").Y + (Padding.Y * 2))));
						(int)(BodyTextFont.MeasureString("Every\nThing\nHere\nNow").Y + (Padding.Y * 2))));
			}

			LocationTextBox.RealignElements();
			ModalWindow?.RealignFloatingButtons();
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

		protected override void ClickUpButton()
		{
			throw new NotImplementedException();
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

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);

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

		public override void DrawContent(SpriteBatch b)
		{
			Vector2 position = Utility.PointToVector2(ContentSafeArea.Location);
			position.Y += Padding.Y;
			//this.DrawText(b, position: position, text: ModEntry.Instance.i18n.Get("ui.locationselect.label"));
			this.DrawText(b, position: position, text: "This is a multi-line text box");

			LocationTextBox.draw(b);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace InteractiveSchedule.Interface.Modals
{
	public class ConfirmPromptModal : WindowModal
	{
		public ClickableTextureComponent ConfirmButton, CancelButton;
		public string DisplayText;
		private string _text;

		public delegate void ConfirmCallback();
		public delegate void CancelCallback();
		private readonly ConfirmCallback _confirmCallback;
		private readonly CancelCallback _cancelCallback;


		public ConfirmPromptModal(WindowPage modalParent,
			string text, ConfirmCallback onConfirm, CancelCallback onCancel)
			: base(modalParent)
		{
			_text = text;
			_confirmCallback = onConfirm;
			_cancelCallback = onCancel;
		}

		protected override void cleanupBeforeExit()
		{
			ConfirmButton = null;
			CancelButton = null;
			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
		}

		public override void RealignElements()
		{
			base.RealignElements();
		}

		public override void RealignFloatingButtons()
		{
			width = 150 * MenuScale;

			DisplayText = Game1.parseText(text: _text, whichFont: BodyTextFont, width: width);
			Vector2 textSize = BodyTextFont.MeasureString(DisplayText);
			int xOffsetFromCentre = ((FloatingActionButtons.Count * ActionButtonSize.X * MenuScale) + Padding.X) / 2;

			height = Math.Max(96,
				(int)textSize.Y
				+ (Padding.Y * 2)
				+ (ActionButtonSize.Y * MenuScale)
				+ (Padding.Y * 2));

			Point relativePosition = new Point(
				ContentSafeArea.Width  / 2,
				(int)(textSize.Y + (Padding.Y * 2)));

			FloatingActionButtons[ConfirmButton] = new Point(
				relativePosition.X - xOffsetFromCentre,
				relativePosition.Y);
			FloatingActionButtons[ConfirmButton] = new Point(
				relativePosition.X + xOffsetFromCentre,
				relativePosition.Y);

			base.RealignFloatingButtons();
		}

		protected override void AddActionButtons()
		{
			base.AddActionButtons();

			ConfirmButton = this.CreateActionButton(nameof(ConfirmButton));
			CancelButton = this.CreateActionButton(nameof(CancelButton));
		}

		protected override void Hover(int x, int y) { }

		protected override void LeftClick(int x, int y, bool playSound)
		{
			if (_parentMenu is WindowPage windowPage)
			{
				if (!windowPage.IsSelected || !windowPage.ShouldDraw)
					return;

				if (ConfirmButton.containsPoint(x, y))
				{
					_confirmCallback();
					return;
				}
				if (CancelButton.containsPoint(x, y))
				{
					_cancelCallback();
					return;
				}
			}
		}

		protected override void DrawContent(SpriteBatch b)
		{
			Vector2 position = ContentOrigin;
			this.DrawText(b, position: position, text: DisplayText);
		}
	}
}

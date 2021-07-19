using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface.Elements
{
	public class Checkbox : CustomElement
	{
		private static readonly Rectangle SourceArea = new Rectangle(126, 0, 18, 18);
		private bool _state;
		public bool State
		{
			get => this._state;
			set
			{
				this._state = value;
				this.OnStateChangedBehaviour();
			}
		}
		public readonly string Text;
		public string DisplayText;
		public bool IsEnabled;
		public delegate void OnStageChanged();
		public OnStageChanged OnStateChangedBehaviour;


		public Checkbox(CustomMenu parentMenu, Point relativePosition, OnStageChanged onCheckedBehaviour, string text,
			bool initialState = false, bool isEnabled = true)
			: base (parentMenu: parentMenu, relativePosition: relativePosition,
				dimensions: new Point(Checkbox.SourceArea.Width * CustomMenu.MenuScale, Checkbox.SourceArea.Height * CustomMenu.MenuScale))
		{
			this.OnStateChangedBehaviour = onCheckedBehaviour;
			this.State = initialState;
			this.IsEnabled = isEnabled;
			this.DisplayText = this.Text = text;
			this.RealignElements();
		}

		public bool Toggle()
		{
			this.State = !this.State;
			return this.State;
		}

		public void Draw(SpriteBatch b)
		{
			b.Draw(
				texture: ModEntry.Sprites,
				destinationRectangle: this.Bounds,
				sourceRectangle: new Rectangle(
					Checkbox.SourceArea.X + (State ? Checkbox.SourceArea.Width : 0),
					Checkbox.SourceArea.Y,
					Checkbox.SourceArea.Width,
					Checkbox.SourceArea.Height),
				color: this.IsEnabled ? Color.White : Color.DarkGray * 0.5f);

			if (!string.IsNullOrEmpty(Text))
			{
				this._parentMenu.DrawText(b,
				position: new Vector2(
					this._parentMenu.ContentSafeArea.X + this.RelativePosition.X,
					this.Bounds.Y),
				text: this.DisplayText,
				font: WindowPage.BodyTextFont,
				colour: WindowPage.BodyTextColour);
			}
		}

		public override void RealignElements()
		{
			base.RealignElements();
			if (!string.IsNullOrEmpty(Text))
			{
				this.Bounds.X = this._parentMenu.ContentSafeArea.X + this.RelativePosition.X
					+ this._parentMenu.ContentSafeArea.Width - this.Bounds.Width;
				this.DisplayText = ISUtilities.GetSingleLineEllipsisString(
					text: this.Text,
					font: WindowPage.BodyTextFont,
					width: this.Bounds.X - this.RelativePosition.X);
			}
		}

		public override bool LeftClick()
		{
			if (!this.IsEnabled)
				return this.State;
			return this.Toggle();
		}
	}
}

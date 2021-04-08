using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface
{
	public class ScrollableListViewComponent : ViewComponent
	{
		public ClickableTextureComponent ScrollUpButton, ScrollDownButton, ScrollbarTrain;

		public readonly bool IsHorizontal;
		public readonly List<object> Items = new List<object>();
		public int ItemsHigh { get; private set; }
		public int ScrollIndex;
		public bool CanScrollBackward => Items.Any() && ScrollIndex >= ItemsHigh;
		public bool CanScrollForward => Items.Any() && ScrollIndex < Items.Count - ItemsHigh;
		public bool CanScroll => Items.Any() && (CanScrollBackward || CanScrollForward);

		private Vector2 _scrollbarOrigin;
		private Vector2 _scrollbarEnd;
		private Rectangle _scrollbarTrackArea;

		private const int ItemHeight = 16 * MenuScale;
		private const int ScrollbarWidth = 9;
		private static readonly Point ScrollbarSourceOrigin = new Point(63, 17);
		private static readonly Rectangle ScrollbarUpArrowSource = new Rectangle(ScrollbarSourceOrigin.X, 17, ScrollbarWidth, 8);
		private static readonly Rectangle ScrollbarTrainSource = new Rectangle(ScrollbarSourceOrigin.X, 27, ScrollbarWidth, 11);
		private static readonly Rectangle ScrollbarDownArrowSource = new Rectangle(ScrollbarSourceOrigin.X, 40, ScrollbarWidth, 8);


		public ScrollableListViewComponent(IClickableMenu parentMenu, Point relativePosition,
			bool isHorizontal, List<object> items)
			: base(parentMenu, relativePosition)
		{
			IsHorizontal = isHorizontal;
			Items.AddRange(items);

			ScrollUpButton = new ClickableTextureComponent(bounds: Rectangle.Empty,
				texture: ModEntry.Sprites,
				sourceRect: ScrollbarUpArrowSource,
				scale: MenuScale);
			ScrollDownButton = new ClickableTextureComponent(bounds: Rectangle.Empty,
				texture: ModEntry.Sprites,
				sourceRect: ScrollbarDownArrowSource,
				scale: MenuScale);
			ScrollbarTrain = new ClickableTextureComponent(bounds: Rectangle.Empty,
				texture: ModEntry.Sprites,
				sourceRect: ScrollbarTrainSource,
				scale: MenuScale);
		}

		protected override void cleanupBeforeExit()
		{
			ScrollUpButton = null;
			ScrollDownButton = null;
			ScrollbarTrain = null;
			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			BorderWidth = 1;
		}

		public override void RealignElements()
		{
			base.RealignElements();

			if (_parentMenu == null)
				return;

			int borderScaled = BorderWidth * MenuScale;
			int scrollbarScaled = ScrollbarWidth * MenuScale;

			BorderSafeArea = new Rectangle(
				xPositionOnScreen + borderScaled,
				yPositionOnScreen + borderScaled,
				width - (borderScaled * 2) - scrollbarScaled,
				height - borderScaled);

			ContentSafeArea = new Rectangle(
				BorderSafeArea.X + Padding.X,
				BorderSafeArea.Y + Padding.Y,
				BorderSafeArea.Width - (Padding.X * 2),
				BorderSafeArea.Height - (Padding.Y * 2));

			ItemsHigh = ContentSafeArea.Height / ItemHeight;

			// Scrollbar sits inside of the border-safe area
			int scrollbarArrowHeight = ScrollbarDownArrowSource.Height * MenuScale;
			_scrollbarOrigin = new Vector2(
				xPositionOnScreen + width - scrollbarScaled,
				yPositionOnScreen + borderWidth);
			_scrollbarEnd = new Vector2(
				_scrollbarOrigin.X,
				yPositionOnScreen + height + borderWidth);
			_scrollbarTrackArea = new Rectangle(
				(int)_scrollbarOrigin.X,
				(int)_scrollbarOrigin.Y + scrollbarArrowHeight,
				ScrollbarWidth * MenuScale,
				(int)(_scrollbarEnd.Y - _scrollbarOrigin.Y - (2 * scrollbarArrowHeight)));

			ScrollUpButton.bounds = new Rectangle(
				(int)_scrollbarOrigin.X,
				(int)_scrollbarOrigin.Y,
				ScrollbarUpArrowSource.Width * MenuScale,
				ScrollbarUpArrowSource.Height * MenuScale);
			ScrollDownButton.bounds = new Rectangle(
				(int)_scrollbarEnd.X,
				(int)_scrollbarEnd.Y - scrollbarArrowHeight,
				ScrollbarDownArrowSource.Width * MenuScale,
				ScrollbarDownArrowSource.Height * MenuScale);
			ScrollbarTrain.bounds = new Rectangle( // todo: scrollbar train position
				0, 0,
				ScrollbarTrainSource.Width * MenuScale,
				ScrollbarTrainSource.Height * MenuScale);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		private void DrawItems(SpriteBatch b)
		{
			for (int i = ScrollIndex; i < Items.Count; ++i)
			{

			}
		}

		private void DrawScrollbar(SpriteBatch b)
		{
			// TODO: Scrollbar middle flexible height

			// Scrollbar track
			const int endHeight = 2;
			int height = endHeight;
			int yPosition = ScrollUpButton.bounds.Y + ScrollUpButton.bounds.Height;
			Rectangle destinationRect = new Rectangle(
				(int)(_scrollbarOrigin.X),
				yPosition,
				ScrollbarWidth * MenuScale,
				height * MenuScale);
			// top:
			b.Draw(
				texture: ModEntry.Sprites,
				destinationRectangle: destinationRect,
				sourceRectangle: new Rectangle(
					ScrollbarSourceOrigin.X,
					ScrollbarSourceOrigin.Y + ScrollbarUpArrowSource.Height,
					ScrollbarWidth,
					2),
				color: Color.White);
			// middle:
			destinationRect = _scrollbarTrackArea;
			destinationRect.Y += height * MenuScale;
			b.Draw(
				texture: ModEntry.Sprites,
				destinationRectangle: destinationRect,
				sourceRectangle: new Rectangle(
					ScrollbarSourceOrigin.X,
					ScrollbarSourceOrigin.Y + ScrollbarUpArrowSource.Height + (height / 2),
					ScrollbarWidth,
					1),
				color: Color.White);
			// bottom:
			height = 2;
			destinationRect.Y += _scrollbarTrackArea.Height - (1 * MenuScale);
			destinationRect.Height = (height * MenuScale);
			b.Draw(
				texture: ModEntry.Sprites,
				destinationRectangle: destinationRect,
				sourceRectangle: new Rectangle(
					ScrollbarSourceOrigin.X,
					ScrollbarSourceOrigin.Y + ScrollbarUpArrowSource.Height + ScrollbarTrainSource.Height,
					ScrollbarWidth,
					height),
				color: Color.White);

			// Scrollbar train
			// top:
			if (false)
			{
				int cumulativeHeight = 0;
				height = 4;
				cumulativeHeight += height;
				yPosition = ScrollbarTrain.bounds.Y;
				destinationRect = new Rectangle((int)(_scrollbarOrigin.X), yPosition, ScrollbarWidth * MenuScale, height * MenuScale);
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: destinationRect,
					sourceRectangle: new Rectangle(ScrollbarTrainSource.X, ScrollbarTrainSource.Y + cumulativeHeight, ScrollbarTrainSource.Width, height),
					color: Color.White);
				// middle:
				destinationRect.Y += destinationRect.Height;
				height = 2;
				cumulativeHeight += height;
				destinationRect.Height = height * MenuScale;
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: destinationRect,
					sourceRectangle: new Rectangle(ScrollbarTrainSource.X, ScrollbarTrainSource.Y + cumulativeHeight, ScrollbarTrainSource.Width, height),
					color: Color.White);
				// bottom:
				destinationRect.Y += destinationRect.Height;
				height = 5;
				cumulativeHeight += height;
				destinationRect.Height = height * MenuScale;
				b.Draw(
					texture: ModEntry.Sprites,
					destinationRectangle: destinationRect,
					sourceRectangle: new Rectangle(ScrollbarTrainSource.X, ScrollbarTrainSource.Y + cumulativeHeight, ScrollbarTrainSource.Width, height),
					color: Color.White);
			}

			// Scrollbar buttons
			ScrollUpButton.draw(b);
			ScrollDownButton.draw(b);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			this.DrawScrollbar(b);
			this.DrawItems(b);
		}
	}
}

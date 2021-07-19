using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface.Components
{
	public class ScrollableListViewComponent : ViewComponent
	{
		public ClickableTextureComponent ScrollBackwardButton, ScrollForwardButton, ScrollbarTrain;

		public readonly bool IsHorizontal;
		public readonly List<Elements.ListElement> Items = new List<Elements.ListElement>();
		public readonly List<ClickableComponent> ListItems = new List<ClickableComponent>();
		public int ItemsHigh { get; private set; }
		public int ItemsToShow { get; private set; }
		private int _scrollIndex;
		public int ScrollIndex
		{
			get => _scrollIndex;
			private set
			{
				int index = Math.Max(0, Math.Min(Items.Count - ItemsToShow, value));
				_scrollIndex = index;
				this.RealignElements();
			}
		}
		public bool CanScrollBackward => Items.Any() && ScrollIndex > 0;
		public bool CanScrollForward => Items.Any() && ScrollIndex < Items.Count - ItemsHigh;
		public bool CanScroll => Items.Any() && (CanScrollBackward || CanScrollForward);

		private Vector2 _listOrigin;
		private Vector2 _scrollbarOrigin;
		private Vector2 _scrollbarEnd;
		private Rectangle _scrollbarTrackArea;

		private const int ScrollbarWidth = 9;
		private static readonly Point ScrollbarSourceOrigin = new Point(63, 17);
		private static readonly Rectangle ScrollbarUpArrowSource = new Rectangle(ScrollbarSourceOrigin.X, 17, ScrollbarWidth, 8);
		private static readonly Rectangle ScrollbarTrainSource = new Rectangle(ScrollbarSourceOrigin.X, 27, ScrollbarWidth, 11);
		private static readonly Rectangle ScrollbarDownArrowSource = new Rectangle(ScrollbarSourceOrigin.X, 40, ScrollbarWidth, 8);

		public delegate void ItemClickedCallback(int index);
		private readonly ItemClickedCallback _itemClickedCallback;


		public ScrollableListViewComponent(IClickableMenu parentMenu, Point relativePosition,
			bool isHorizontal, List<Elements.ListElement> items, bool drawBorder, ItemClickedCallback onItemClicked)
			: base(parentMenu: parentMenu, relativePosition: relativePosition, drawBorder: drawBorder)
		{
			IsHorizontal = isHorizontal;
			Items.AddRange(items);
			_itemClickedCallback = onItemClicked;

			ScrollBackwardButton = new ClickableTextureComponent(bounds: Rectangle.Empty,
				texture: ModEntry.Sprites,
				sourceRect: ScrollbarUpArrowSource,
				scale: MenuScale);
			ScrollForwardButton = new ClickableTextureComponent(bounds: Rectangle.Empty,
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
			ScrollBackwardButton = null;
			ScrollForwardButton = null;
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
				xPositionOnScreen,
				yPositionOnScreen + (borderScaled * 2),
				width - scrollbarScaled,
				height - borderScaled);

			ContentSafeArea = new Rectangle(
				BorderSafeArea.X,
				BorderSafeArea.Y,
				BorderSafeArea.Width,
				BorderSafeArea.Height);

			_listOrigin = new Vector2(
				ContentSafeArea.X,
				ContentSafeArea.Y);

			ListItems.Clear();
			int itemsToShowHeight = 0;
			for (int i = ScrollIndex; i < Items.Count; ++i)
			{
				int localIndex = i - ScrollIndex;
				itemsToShowHeight += Items[i].ItemHeight;
				if (itemsToShowHeight > BorderSafeArea.Height)
				{
					ItemsHigh = localIndex;
					break;
				}

				Rectangle bounds = new Rectangle(
					(int)_listOrigin.X,
					(int)_listOrigin.Y + (localIndex * Items[i].ItemHeight),
					ContentSafeArea.Width,
					Items[i].ItemHeight);
				ListItems.Add(new ClickableComponent(bounds: bounds, name: "list" + localIndex));
			}
			ItemsToShow = Math.Min(ListItems.Count, Items.Count);
			
			// Scrollbar sits inside of the border-safe area
			int scrollbarArrowHeight = ScrollbarUpArrowSource.Height * MenuScale;
			_scrollbarOrigin = new Vector2(
				xPositionOnScreen + width - scrollbarScaled,
				yPositionOnScreen + borderScaled);
			_scrollbarEnd = new Vector2(
				_scrollbarOrigin.X,
				yPositionOnScreen + height + borderScaled);
			_scrollbarTrackArea = new Rectangle(
				(int)_scrollbarOrigin.X,
				(int)_scrollbarOrigin.Y + scrollbarArrowHeight,
				ScrollbarWidth * MenuScale,
				(int)(_scrollbarEnd.Y - _scrollbarOrigin.Y - (2 * scrollbarArrowHeight)));

			if (ScrollBackwardButton != null)
			{
				ScrollBackwardButton.bounds = new Rectangle(
					(int)_scrollbarOrigin.X,
					(int)_scrollbarOrigin.Y,
					ScrollbarUpArrowSource.Width * MenuScale,
					ScrollbarUpArrowSource.Height * MenuScale);
				ScrollForwardButton.bounds = new Rectangle(
					(int)_scrollbarEnd.X,
					(int)_scrollbarEnd.Y - scrollbarArrowHeight,
					ScrollbarDownArrowSource.Width * MenuScale,
					ScrollbarDownArrowSource.Height * MenuScale);

				float items = Math.Max(1, Items.Count);
				float itemsOther = Math.Max(1, Items.Count - ListItems.Count);
				float ratio = ListItems.Count / items;
				float ratioSafe = Math.Min(1, ratio);
				float trainHeight = ratioSafe * _scrollbarTrackArea.Height;
				float progressRatio = (float)ScrollIndex / itemsOther;
				float trainProgress = progressRatio * (_scrollbarTrackArea.Height - trainHeight);
				ScrollbarTrain.bounds = new Rectangle(
					_scrollbarTrackArea.X,
					(int)(_scrollbarTrackArea.Y + trainProgress),
					ScrollbarTrainSource.Width * MenuScale,
					(int)(trainHeight));
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);

			if (_parentMenu is WindowPage windowPage)
			{
				if (!windowPage.IsSelected || !windowPage.ShouldDraw)
					return;

				if (ScrollBackwardButton.containsPoint(x, y))
					this.Scroll(up: true);
				else if (ScrollForwardButton.containsPoint(x, y))
					this.Scroll(up: false);
				else if (CanScroll && ListItems.Count < Items.Count && _scrollbarTrackArea.Contains(x, y))
				{
					int scrollTo = (int)((float)(y - _scrollbarTrackArea.Y) / _scrollbarTrackArea.Height * Items.Count);
					this.ScrollTo(index: scrollTo);
					return;
				}

				int index = this.GetHoveredItemIndex(x, y);
				if (index != -1)
				{
					_itemClickedCallback(ScrollIndex + index);
					return;
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);

			if (_parentMenu is WindowPage windowPage)
			{
				if (!windowPage.IsSelected || !windowPage.ShouldDraw || !BorderSafeArea.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
					return;

				this.Scroll(up: direction > 0);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		public void Scroll(bool up)
		{
			if (up && CanScrollBackward)
			{
				this.ScrollTo(--ScrollIndex);
			}
			else if (!up && CanScrollForward)
			{
				this.ScrollTo(++ScrollIndex);
			}
		}

		public void ScrollTo(int index)
		{
			ScrollIndex = Math.Max(0, Math.Min(index, Items.Count - 1));
			this.RealignElements();
		}

		public int GetHoveredItemIndex(int x, int y)
		{
			int index = -1;
			for (int i = 0; i < ListItems.Count && ScrollIndex + i < Items.Count && index == -1; ++i)
			{
				if (ListItems[i] != null && ListItems[i].containsPoint(x, y) && Items[ScrollIndex + i].CanBeClicked)
				{
					index = i;
				}
			}
			return index;
		}

		private void DrawItems(SpriteBatch b)
		{
			for (int i = 0; i < ItemsToShow; ++i)
			{
				int index = ScrollIndex + i;
				Vector2 offset = ISUtilities.GetOffsetToCentre(
					dimensions: new Point(0, Items[index].ItemHeight),
					bounds: new Point(9999, ListItems[i].bounds.Height));
				offset.X = Padding.X;
				Items[index].Draw(b,
					bounds: ListItems[i].bounds,
					relativePosition: new Vector2(offset.X, offset.Y));
				if (i < ItemsToShow - 1)
				{
					Desktop.DrawLine(b,
						colour: BodyTextColour * 0.2f,
						startPosition: new Point(
							ListItems[i].bounds.X,
							ListItems[i].bounds.Y + ListItems[i].bounds.Height),
						length: ContentSafeArea.Width,
						width: 1,
						isHorizontal: true);
				}
			}
		}

		private void DrawScrollbar(SpriteBatch b)
		{
			// TODO: Scrollbar middle flexible height

			// Scrollbar track
			int endHeight = 2;
			int height = endHeight;
			int yPosition = ScrollBackwardButton.bounds.Y + ScrollBackwardButton.bounds.Height;
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
			endHeight = 3;
			yPosition = ScrollbarTrain.bounds.Y;
			destinationRect = new Rectangle((int)(_scrollbarOrigin.X), yPosition, ScrollbarWidth * MenuScale, endHeight * MenuScale);
			b.Draw(
				texture: ModEntry.Sprites,
				destinationRectangle: destinationRect,
				sourceRectangle: new Rectangle(ScrollbarTrainSource.X, ScrollbarTrainSource.Y, ScrollbarTrainSource.Width, endHeight),
				color: Color.White);
			// middle:
			const int middleHeight = 4;
			destinationRect.Y += destinationRect.Height;
			height = Math.Max(middleHeight * MenuScale, ScrollbarTrain.bounds.Height - (endHeight * 2 * MenuScale));
			destinationRect.Height = height;
			b.Draw(
				texture: ModEntry.Sprites,
				destinationRectangle: destinationRect,
				sourceRectangle: new Rectangle(ScrollbarTrainSource.X, ScrollbarTrainSource.Y + endHeight, ScrollbarTrainSource.Width, middleHeight),
				color: Color.White);
			// bottom:
			destinationRect.Y += destinationRect.Height;
			destinationRect.Height = endHeight * MenuScale;
			b.Draw(
				texture: ModEntry.Sprites,
				destinationRectangle: destinationRect,
				sourceRectangle: new Rectangle(ScrollbarTrainSource.X, ScrollbarTrainSource.Y + endHeight + middleHeight, ScrollbarTrainSource.Width, endHeight),
				color: Color.White);

			// Scrollbar buttons
			ScrollBackwardButton.draw(b);
			ScrollForwardButton.draw(b);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);

			this.DrawScrollbar(b);

			Point cursor = new Point(Game1.getOldMouseX(), Game1.getOldMouseY());
			int index = this.GetHoveredItemIndex(cursor.X, cursor.Y);
			Rectangle highlightArea = Rectangle.Empty;
			if (index != -1 && index < ItemsToShow)
			{
				highlightArea = ListItems[index].bounds;
			}
			if (CanScroll && ListItems.Count < Items.Count)
			{
				if (ScrollbarTrain.containsPoint(cursor.X, cursor.Y))
					highlightArea = ScrollbarTrain.bounds;
				else if (ScrollBackwardButton.containsPoint(cursor.X, cursor.Y))
					highlightArea = ScrollBackwardButton.bounds;
				else if (ScrollForwardButton.containsPoint(cursor.X, cursor.Y))
					highlightArea = ScrollForwardButton.bounds;
			}

			if (highlightArea != Rectangle.Empty)
				DrawHighlight(b, highlightArea);

			this.DrawItems(b);
		}
	}
}

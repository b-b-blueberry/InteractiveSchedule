using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InteractiveSchedule.Interface.Components
{
	public class TextBoxComponent : ViewComponent
	{
		public CustomTextBox TextBox;
		public Point Dimensions;
		public bool ShowLineNumbers
		{
			get => _showLineNumbers;
			set
			{
				_showLineNumbers = value;
				this.RealignElements();
			}
		}
		public bool IsSelected => TextBox.Selected;
		public int CaretIndex
		{
			get => _caretIndex;
			set
			{
				_caretIndex = value;
				CaretPosition = this.GetCaretCoordinatesFromIndex(_caretIndex);
			}
		}
		public Point CaretPosition { get; private set; }
		public Color LineNumberColumnColour;
		public Color AuxiliaryColour;

		public readonly bool Scrollable;
		public readonly bool NumbersOnly;
		public readonly bool SingleLineOnly;

		private int _caretIndex;
		private int _caretDelay;
		private int _caretTimer;
		private int _caretWidth;
		private bool _showLineNumbers;

		internal const int LineNumberColumnWidth = 9;

		public delegate void Validator(Regex pattern, string message);
		public Validator validator = null;

		private TextBoxComponent(IClickableMenu parentMenu, string defaultText,
			bool scrollable, bool showLineNumbers, bool singleLineOnly, bool numbersOnly,
			Point relativePosition,
			char[] forbiddenCharacters = null)
			: base(parentMenu: parentMenu, relativePosition: relativePosition, drawBorder: true)
		{
			Scrollable = scrollable && !singleLineOnly;
			ShowLineNumbers = showLineNumbers && !singleLineOnly;
			SingleLineOnly = singleLineOnly;
			NumbersOnly = numbersOnly;
			TextBox = new CustomTextBox(
				container: this,
				font: TextBoxFont,
				defaultText: defaultText,
				forbiddenCharacters: forbiddenCharacters);

			this.RealignElements();
		}

		public TextBoxComponent(IClickableMenu parentMenu, string defaultText,
			bool scrollable, bool showLineNumbers, bool singleLineOnly, bool numbersOnly,
			Rectangle dimensions,
			char[] forbiddenCharacters = null)
			: this(parentMenu: parentMenu, defaultText: defaultText,
				  scrollable: scrollable, showLineNumbers: showLineNumbers, singleLineOnly: singleLineOnly, numbersOnly: numbersOnly,
				  relativePosition: new Point(dimensions.X, dimensions.Y),
				  forbiddenCharacters: forbiddenCharacters)
		{
			Dimensions = new Point(dimensions.Width, dimensions.Height);
		}

		public TextBoxComponent(IClickableMenu parentMenu, string defaultText,
			bool scrollable, bool showLineNumbers, bool singleLineOnly, bool numbersOnly,
			Point relativePosition, int columns, int rows,
			char[] forbiddenCharacters = null)
			: this(parentMenu: parentMenu, defaultText: defaultText,
				  scrollable: scrollable, showLineNumbers: showLineNumbers, singleLineOnly: singleLineOnly, numbersOnly: numbersOnly,
				  relativePosition: relativePosition,
				  forbiddenCharacters: forbiddenCharacters)
		{
			Dimensions = new Point(
				(columns * ModEntry.MonoThinFontWidth)
					+ (Padding.X * 2) + (BorderWidth * 2)
					+ (showLineNumbers ? LineNumberColumnWidth * MenuScale : 0)
					+ (scrollable && !singleLineOnly ? 0 : 0), // todo: scrollable text box: scrollbar width in ctor
				rows * ModEntry.MonoThinFont.LineSpacing);
		}

		protected override void cleanupBeforeExit()
		{
			TextBox = null;
			base.cleanupBeforeExit();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();

			AuxiliaryColour = ViewBorderColour;
			LineNumberColumnColour = AuxiliaryColour;
			_caretDelay = 750;
			_caretWidth = 1;
		}

		public override void RealignElements()
		{
			if (_parentMenu == null || TextBox == null)
				return;
			
			base.RealignElements();

			width = Dimensions.X;
			height = Dimensions.Y;

			// Position container relative to the parent menu
			xPositionOnScreen = _parentMenu.xPositionOnScreen + RelativePosition.X;
			yPositionOnScreen = _parentMenu.yPositionOnScreen + RelativePosition.Y;

			// Border-safe area considers borders only
			int borderWidth = CustomTextBox.BorderWidth * MenuScale;
			BorderSafeArea = new Rectangle(
				xPositionOnScreen + borderWidth,
				yPositionOnScreen + (2 * borderWidth),
				width - (borderWidth * 2),
				height - borderWidth);

			// Content-safe area considers padding and borders as well as other exclusive content blocks
			int scrollbarWidth = //Scrollable ? ScrollbarWidth * MenuScale : 0;
				0;
			int lineNumberColumnWidth = ShowLineNumbers && !SingleLineOnly ? LineNumberColumnWidth * MenuScale : 0;
			ContentSafeArea = new Rectangle(
				BorderSafeArea.X + (Padding.X / 2) + lineNumberColumnWidth,
				BorderSafeArea.Y + Padding.Y,
				BorderSafeArea.Width - Padding.X - scrollbarWidth - lineNumberColumnWidth,
				BorderSafeArea.Height - Padding.Y);

			TextBox.RealignElements();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			bool isPreviouslySelected = TextBox.Selected;
			TextBox.Selected = ContentSafeArea.Contains(x, y);
			if (TextBox.Selected && isPreviouslySelected)
			{
				CaretIndex = this.GetCaretIndexFromCursor(x, y);
				this.ResetCaretTimer(showCaret: true);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			TextBox.Update();
			_caretTimer += time.ElapsedGameTime.Milliseconds;
			if (_caretTimer >= _caretDelay * 2)
				_caretTimer = 0;
		}

		public void ResetCaretTimer(bool showCaret = false)
		{
			_caretTimer = showCaret ? _caretDelay / 2 : 0;
		}

		public void EnterTextAtCaret(char character)
		{
			this.EnterTextAtCaret(character.ToString());
		}

		public void EnterTextAtCaret(string text)
		{
			TextBox.InsertText(CaretIndex, text);
			CaretIndex += text.Length;
		}

		public void RemoveCharacterAtCaret()
		{
			TextBox.RemoveText(--CaretIndex);
			CaretPosition = this.GetCaretCoordinatesFromIndex(_caretIndex);
		}

		// TODO: REMOVE PasteTextAtCaret for being useless
		public void PasteTextAtCaret()
		{
			string text = "";
			if (DesktopClipboard.IsAvailable && DesktopClipboard.GetText(output: ref text) && !string.IsNullOrEmpty(text))
			{
				TextBox.InsertText(CaretIndex, text);
			}
			else
			{
				ModEntry.Instance.Desktop.PlaySound("cancel");
			}
		}
		
		public int GetCaretIndexFromCursor(int x, int y)
		{
			int localY = y - ContentSafeArea.Y;
			int characterY = localY / TextBox.Font.LineSpacing;

			int localX = x - ContentSafeArea.X;
			int characterX = localX / (int)TextBox.CharacterSize.X;

			return this.GetCaretIndexFromCoordinates(characterX, characterY);
		}

		public int GetCaretIndexFromCoordinates(Point point)
		{
			return this.GetCaretIndexFromCoordinates(point.X, point.Y);
		}

		public int GetCaretIndexFromCoordinates(int x, int y)
		{
			string[] splitText = TextBox.DisplayText.Split('\n');
			int caretIndex = 0;
			int targetRow = //_scrolledToRow;
				0;

			// Seek to current textbox start line
			for (int i = 0; i < targetRow; ++i)
				caretIndex += splitText[i].Length;

			// Seek to current visible line
			int localRow = Math.Min(splitText.Length - 1, y);
			int whichRow = Math.Max(0, targetRow + localRow);
			for (int i = 0; i < localRow; ++i)
				// add 1 for split-out '\n' character
				caretIndex += splitText[i].Length + 1; 

			// Seek to current character in current visible line
			int whichColumn = Math.Min(splitText[whichRow].Length, x);
			caretIndex += whichColumn;

			// uuuhhhhh move back by tyhe number of display newlines
			caretIndex -= TextBox.DisplayTextBreakLines.Count;

			return caretIndex;
		}

		public Point GetCaretCoordinatesFromIndex(int index)
		{
			index = Math.Max(0, Math.Min(TextBox.Text.Length - 1, index));
			string textBeforeIndex = TextBox.Text.Substring(0, index);
			Point position = new Point();
			
			int whichRow = textBeforeIndex.Count(c => c == '\n');
			int whichColumn = textBeforeIndex.Split('\n')[whichRow].Length;
			position = new Point(whichColumn, whichRow);

			return position;
		}

		private void DrawCaret(SpriteBatch b)
		{
			bool isCaretVisible = _caretTimer > _caretDelay / 4 * 2 && _caretTimer < _caretDelay / 4 * 3 * 2 ? true : false;
			float opacity = Math.Abs(_caretDelay - _caretTimer) < (_caretDelay / 2.5f) ? 1f : 0.5f;
			int caretLine = CaretPosition.Y + TextBox.DisplayTextBreakLines.Count(i => i <= CaretPosition.Y);
			int lineLength = TextBox.DisplayText.Split('\n')[caretLine].Length;
			int x = (CaretPosition.X / (int)TextBox.CharactersWide.X) + (CaretPosition.X % (int)TextBox.CharactersWide.X) + 1;
			caretLine += CaretPosition.X / (int)TextBox.CharactersWide.X;
			if (isCaretVisible)
			{
				b.Draw(
					texture: Game1.staminaRect,
					destinationRectangle: new Rectangle(
						ContentSafeArea.X + (int)(x * TextBox.CharacterSize.X),
						ContentSafeArea.Y + (int)(caretLine * TextBox.CharacterSize.Y),
						_caretWidth,
						(int)TextBox.CharacterSize.Y),
					color: TextBox.TextColour * opacity);
			}
		}

		private void DrawLineNumbers(SpriteBatch b)
		{
			// Line numbers container
			Vector2 position = new Vector2(
				xPositionOnScreen,
				yPositionOnScreen);
			b.Draw(
				texture: Game1.fadeToBlackRect,
				destinationRectangle: new Rectangle(
					(int)position.X,
					(int)position.Y,
					LineNumberColumnWidth * MenuScale,
					height),
				sourceRectangle: null,
				color: LineNumberColumnColour);

			// Line numbers
			string[] textSplit = TextBox.DisplayText.Split('\n');
			int num = 0;
			position.X = BorderSafeArea.X;
			position.Y = ContentSafeArea.Y - ModEntry.MonoThinFont.LineSpacing;
			for (int i = 0; i < textSplit.Length - 1; ++i)
			{
				position.Y += ModEntry.MonoThinFont.LineSpacing;
				if (TextBox.DisplayTextBreakLines.Contains(i))
					continue;
				b.DrawString(
					spriteFont: ModEntry.MonoThinFont,
					text: num.ToString(),
					position: position,
					color: Color.White);
				++num;
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (_parentMenu is WindowPage windowPage && !windowPage.ShouldDraw)
				return;

			base.draw(b);

			TextBox.Draw(spriteBatch: b);
			if (TextBox.Selected)
				this.DrawCaret(b);
			if (ShowLineNumbers)
				this.DrawLineNumbers(b);
		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using System.Linq;
using System.Collections.Generic;
using System;

namespace InteractiveSchedule.Interface
{
	/// <summary>
	/// Edited version of StardewValley.TextBox.
	/// </summary>
	public class CustomTextBox : IKeyboardSubscriber
	{
		public Components.TextBoxComponent Container;

		public string Text {
			get => _text;
			private set
			{
				_text = value;
				LineCount = CustomTextBox.GetLineCount(_text);
			}
		}
		public string DisplayText { get; private set; } = "";
		public List<int> DisplayTextBreakLines = new List<int>();
		public Point SelectedTextRange = Point.Zero;
		public readonly string DefaultText;
		public Vector2 CharacterSize { get; private set; }
		public Vector2 CharactersWide { get; private set; }
		public SpriteFont Font
		{
			get => _font;
			set
			{
				_font = value;
				CharacterSize = _font.MeasureString("X");
				CharactersWide = new Vector2(
					Container.ContentSafeArea.Width / CharacterSize.X,
					Container.ContentSafeArea.Height / CharacterSize.Y);
			}
		}
		public Color TextColour;
		public int LineCount { get; private set; }
		public bool WordWrap
		{
			get => _wordWrap;
			set
			{
				_wordWrap = value;
				this.UpdateDisplayText();
			}
		}
		public readonly char[] ForbiddenCharacters;

		public const int BorderWidth = 1;

		private SpriteFont _font;
		private string _text = "";
		private bool _selected;
		private bool _wordWrap;

		private const char ChTab = '\t';
		private const char ChNew = '\r';
		private const char ChBsp = '\b';

		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				if (_selected == value)
					return;

				_selected = value;
				if (_selected)
				{
					Game1.keyboardDispatcher.Subscriber = this;
					return;
				}
				if (Game1.keyboardDispatcher.Subscriber == this)
				{
					Game1.keyboardDispatcher.Subscriber = null;
				}
			}
		}

		public delegate void TextBoxEvent(CustomTextBox sender);
		public event TextBoxEvent OnEnterPressed;
		public event TextBoxEvent OnTabPressed;
		public event TextBoxEvent OnBackspacePressed;

		public CustomTextBox(Components.TextBoxComponent container, SpriteFont font, string defaultText,
			char[] forbiddenCharacters = null)
		{
			Container = container;
			Font = font;
			DefaultText = defaultText ?? "";
			ForbiddenCharacters = forbiddenCharacters ?? new char[0];

			this.SetDefaults();
		}

		public void SetDefaults()
		{
			TextColour = Game1.textColor;
		}

		public void Update()
		{
			Game1.input.GetMouseState();
			Selected = Container.isWithinBounds(Game1.getMouseX(), Game1.getMouseY());
		}

		public void RealignElements()
		{
			Font = _font;
			Text = _text;
		}

		private string ParseTextToDraw(string text)
		{
			DisplayTextBreakLines.Clear();
			string[] lines = text.Split('\n');
			string newDisplayText = "";

			for (int i = 0; i < lines.Length; ++i)
			{
				int currentIndex = 0;
				string line = lines[i];
				List<string> lineChunks = new List<string>();
				
				for (int j = 0; j < line.Length; j += (int)CharactersWide.X)
				{
					if (j + (int)CharactersWide.X > line.Length)
					{
						lineChunks.Add(line.Substring(j));
					}
					else
					{
						lineChunks.Add(line.Substring(j, (int)CharactersWide.X));
						int lineBreakIndex = i + DisplayTextBreakLines.Count + j + 1;
						DisplayTextBreakLines.Add(lineBreakIndex);
					}
				}

				newDisplayText += string.Join("\n", lineChunks) + "\n";
			}
			return newDisplayText;
		}

		public virtual void Draw(SpriteBatch spriteBatch)
		{
			if (Container.ContentSafeArea.Width < 1 || Container.ContentSafeArea.Height < 1)
				return;

			bool isDefaultText = string.IsNullOrEmpty(Text);
			Vector2 position = Utility.PointToVector2(Container.ContentSafeArea.Location);
			Color colour = TextColour * (isDefaultText ? 0.5f : 1f);
			spriteBatch.DrawString(
				spriteFont: Font,
				text: DisplayText,
				position: position,
				color: colour,
				rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: SpriteEffects.None, layerDepth: 1f);
		}

		public void RecieveCommandInput(char command)
		{
			if (!Selected)
				return;

			switch (command)
			{
				case ChBsp:
					if (Text.Length <= 0)
						break;
					
					if (this.OnBackspacePressed != null)
					{
						this.OnBackspacePressed(this);
					}
					else
					{
						Container.RemoveCharacterAtCaret();
					}
					break;
				case ChNew:
					if (Container.SingleLineOnly)
						return;
					if (this.OnEnterPressed != null)
					{
						this.OnEnterPressed(this);
					}
					Container.EnterTextAtCaret('\n');
					Container.ResetCaretTimer(showCaret: true);
					break;
				case ChTab:
					if (this.OnTabPressed != null)
					{
						this.OnTabPressed(this);
					}
					break;
			}
			this.UpdateDisplayText();
		}

		public void RecieveSpecialInput(Keys key)
		{
			Point position = Container.GetCaretCoordinatesFromIndex(Container.CaretIndex);
			switch (key)
			{
				case Keys.Up:
					--position.Y;
					break;
				case Keys.Down:
					++position.Y;
					break;
				case Keys.Left:
					--position.X;
					break;
				case Keys.Right:
					++position.X;
					break;
				default:
					return;
			}

			Container.CaretIndex = Container.GetCaretIndexFromCoordinates(position);
			Container.ResetCaretTimer(showCaret: true);
			this.UpdateDisplayText();
		}

		public void RecieveTextInput(char c)
		{
			if (!Selected)
				return;

			KeyboardState keyboard = Game1.input.GetKeyboardState();
			bool isShift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
			bool isCtrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
			bool isAlt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
			
			char cLower = c.ToString().ToLower()[0];
			if (isCtrl)
			{
				switch (cLower)
				{
					case 'c':
						return;
				}
			}

			if (ForbiddenCharacters.Contains(c) || (Container.NumbersOnly && !char.IsDigit(c)))
				return;

			Container.EnterTextAtCaret(c);
			this.UpdateDisplayText();
			Container.ResetCaretTimer(showCaret: false);
		}

		public void RecieveTextInput(string text)
		{
			string parsedText = "";
			foreach (char c in text)
			{
				if (ForbiddenCharacters.Contains(c))
					continue;
				parsedText += c;
			}
			if (Selected && (!Container.NumbersOnly || int.TryParse(parsedText, out int _)))
			{
				Container.EnterTextAtCaret(parsedText);
			}
			this.UpdateDisplayText();
		}

		public void InsertText(int index, string text)
		{
			if (index < 0)
				Text = text + Text;
			else if (index >= Text.Length - 1)
				Text += text;
			else
				Text = Text.Substring(0, index) + text + Text.Substring(index);
		}

		public void RemoveText(int index)
		{
			this.RemoveText(index, index + 1);
		}

		public void RemoveText(int startIndex, int endIndex)
		{
			if (Text.Length == 0)
				return;
			startIndex = Math.Max(0, startIndex);
			endIndex = Math.Max(0, Math.Min(Text.Length, endIndex));
			if (startIndex == endIndex || startIndex > endIndex)
				return;
			int count = endIndex - startIndex;
			Text = Text.Remove(startIndex, count);
			//Text = Text.Substring(0, startIndex) + Text.Substring(endIndex);
		}

		public static int GetLineCount(string text)
		{
			return text.Count(c => c == '\n');
		}

		private void UpdateDisplayText()
		{
			bool isDefaultText = string.IsNullOrEmpty(Text);
			string baseDrawText = isDefaultText ? DefaultText : this.ParseTextToDraw(Text) ?? "";
			string validDrawText = baseDrawText.Where(c => Font.Characters.Contains(c) || c == '\n').Aggregate("", (str, c) => str + c);
			DisplayText = validDrawText;
		}
	}
}

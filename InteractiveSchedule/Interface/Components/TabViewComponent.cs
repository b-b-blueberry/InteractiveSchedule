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
	/// <summary>
	/// Single-row tab selection menu page component.
	/// Parent menu must be some WindowPage instance.
	/// </summary>
	public abstract class TabViewComponent : ViewComponent
	{
		public readonly List<ClickableComponent> Tabs = new List<ClickableComponent>();
		public string ActiveTab;
		public int TabBarHeight;
		public static SpriteFont TabFont;
		

		protected TabViewComponent(IClickableMenu parentMenu, Point relativePosition)
			: base(parentMenu: parentMenu, relativePosition: relativePosition)
		{
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			TabFont = Game1.smallFont;
			BorderWidth = 1;
		}

		public override void RealignElements()
		{
			base.RealignElements();

			int borderScaled = BorderWidth * MenuScale;

			if (Tabs.Count > 0)
			{
				// Tabs and tab bar dimensions calculated before safe areas
				for (int i = 0; i < Tabs.Count; ++i)
				{
					Vector2 textSize = TabFont.MeasureString(Tabs[i].name);
					Tabs[i].bounds = new Rectangle(
						xPositionOnScreen + Padding.X + Tabs.Take(i).Sum(tab => tab.bounds.Width) - (i * borderScaled),
						yPositionOnScreen,
						(int)textSize.X + (Padding.X * 2),
						(int)textSize.Y + (Padding.Y * 2));
				}

				TabBarHeight = Tabs.Max(tab => tab.bounds.Height) + (BorderWidth * 2);
			}

			// Safe areas exclude tab bar area
			BorderSafeArea = new Rectangle(
				xPositionOnScreen + borderScaled,
				yPositionOnScreen + borderScaled + TabBarHeight,
				width - (borderScaled * 2),
				height - (borderScaled * 2));

			ContentSafeArea = new Rectangle(
				BorderSafeArea.X + Padding.X,
				BorderSafeArea.Y + Padding.Y,
				BorderSafeArea.Width - (Padding.X * 2),
				BorderSafeArea.Height - (Padding.Y * 2));
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			foreach (ClickableComponent tab in Tabs)
			{
				if (tab.containsPoint(x, y))
				{
					ActiveTab = tab.name;
					return;
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		protected void AddTabs(IEnumerable<string> whichTabs)
		{
			Tabs.Clear();
			foreach (string which in whichTabs)
			{
				Tabs.Add(this.GenerateTab(which));
			}
			ActiveTab = Tabs.FirstOrDefault()?.name;
		}

		private ClickableComponent GenerateTab(string which)
		{
			string displayText = ModEntry.Instance.i18n.Get("ui.tab." + which + ".label");
			Vector2 textSize = TabFont.MeasureString(displayText);
			ClickableComponent tab = new ClickableComponent(
				bounds: new Rectangle(
					-1,
					-1,
					Padding.X + (int)textSize.X,
					(int)textSize.Y),
				name: which,
				label: displayText);
			return tab;
		}

		public abstract void DrawContent(SpriteBatch b);

		private void DrawTabs(SpriteBatch b)
		{
			int borderScaled = BorderWidth * MenuScale;

			// Tab bar border
			Desktop.DrawLine(b,
				colour: ((WindowPage)_parentMenu).InnerBorderColourSelected,
				startPosition: new Point(xPositionOnScreen, yPositionOnScreen + TabBarHeight - borderScaled),
				length: width,
				width: borderScaled,
				isHorizontal: true);

			for (int i = 0; i < Tabs.Count; ++i)
			{
				Color colour;
				bool isActiveTab = Tabs[i].name == ActiveTab;
				int tabHeight = isActiveTab ? Tabs[i].bounds.Height : Tabs[i].bounds.Height - Padding.Y;
				Rectangle destRect = new Rectangle(
					Tabs.First().bounds.X + Tabs.Take(i).Sum(tab => tab.bounds.Width),
					Tabs.First().bounds.Y + Tabs[i].bounds.Height - tabHeight,
					Tabs[i].bounds.Width,
					tabHeight);

				// Border
				colour = isActiveTab ? ((WindowPage)_parentMenu).InnerBorderColourSelected : ((WindowPage)_parentMenu).InnerBorderColourDeselected;
				b.Draw(
					texture: Game1.fadeToBlackRect,
					destinationRectangle: destRect,
					color: colour);

				// Fill
				destRect.X += borderScaled;
				destRect.Y += borderScaled;
				destRect.Width -= borderScaled * 2;
				if (!isActiveTab)
				{
					destRect.Height -= borderScaled;
				}
				colour = isActiveTab ? ((WindowPage)_parentMenu).PageColour : Color.PapayaWhip;
				b.Draw(
					texture: Game1.fadeToBlackRect,
					destinationRectangle: destRect,
					color: colour);

				// Tab label
				b.DrawString(
					spriteFont: TabFont,
					text: Tabs[i].label,
					position: new Vector2(destRect.X + Padding.X, destRect.Y + Padding.Y),
					color: BodyTextColour);
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			this.DrawTabs(b);
			this.DrawContent(b);
		}
	}
}

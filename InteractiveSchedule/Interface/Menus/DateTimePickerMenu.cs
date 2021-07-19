﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSchedule.Interface.Menus
{
	public class DateTimePickerMenu : WindowPage
	{
		public override bool IsOnHomePage => true;
		public override bool IsUpButtonVisible => !IsOnHomePage;
		public override bool IsActionButtonSidebarVisible => true;


		public DateTimePickerMenu(Point position)
			: base(position)
		{
		}

		protected override void cleanupBeforeExit()
		{
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

		protected override void ClickUpButton()
		{
			throw new NotImplementedException();
		}
		protected override void AddActionButtons()
		{
			base.AddActionButtons();
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		protected override void Hover(int x, int y)
		{
		}

		protected override void LeftClick(int x, int y, bool playSound)
		{
		}

		protected override void DrawContent(SpriteBatch b)
		{
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}

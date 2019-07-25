using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using AuxLib.Input;

namespace AuxLib.ScreenManagement
{
    public class MenuItem
    {
        public string Text;

        public object Value;

        public delegate void ItemSelected(MenuItem Item, int idx);
        public event ItemSelected OnSelected;

        public MenuItem(string Txt, object Value,ItemSelected handler)
        {
            this.Text = Txt;
            this.Value = Value;
            this.OnSelected += handler;
        }

        public void Select(int idx)
        {
            OnSelected?.Invoke(this, idx);
        }
    }

	public class MenuComponent
	{
        List<MenuItem> menuItems;
		int selectedIndex;


		Color normal = Color.White;
		Color hilite = Color.Yellow;
        readonly SpriteBatch spriteBatch;
        readonly SpriteFont spriteFont;
        Rectangle Bounds;

		Vector2 position;
        Vector2 Offset;

		float width = 0f;
		float height = 0f;


		public int SelectedIndex
		{
			get { return selectedIndex; }
			set
			{
				selectedIndex = value;
				if (selectedIndex < 0)
					selectedIndex = 0;
				if (selectedIndex >= menuItems.Count)
                    selectedIndex = menuItems.Count - 1;
			}
		}

		public MenuComponent(Rectangle Bounds, Vector2 Offset, SpriteBatch spriteBatch, SpriteFont spriteFont, List<MenuItem> menuItems)			
		{
            this.Offset = Offset;
			this.spriteBatch = spriteBatch;
			this.spriteFont = spriteFont;            
			this.menuItems = menuItems;
            this.Bounds = Bounds;
		}

        public void Initialize()
        {
            height = 0;
            width = 0;
            foreach (var item in menuItems)
            {
                var size = spriteFont.MeasureString(item.Text);
                if (size.X > width)
                    width = size.X;
                height += spriteFont.LineSpacing + 5;
            }

            position = Offset + new Vector2(
                (Bounds.Width - width) / 2,
                (Bounds.Height - height) / 2);
        }


        public void Update(IInputHandler input)
		{
            if (input.WasPressed(0,Buttons.LeftThumbstickDown, Keys.Down))
			{
				selectedIndex++;
				if (selectedIndex == menuItems.Count)
					selectedIndex = 0;
			}
            if (input.WasPressed(0, Buttons.LeftThumbstickUp, Keys.Up))
            {
				selectedIndex--;
				if (selectedIndex < 0)
					selectedIndex = menuItems.Count - 1;
			}

            if (input.WasPressed(0,Buttons.A,Keys.Enter))
            {
                menuItems[selectedIndex].Select(selectedIndex);              
            }			
		}

		public void Draw()
		{			
			var location = position;
			Color tint;

			for (var i = 0; i < menuItems.Count; i++)
			{
                tint = (i == selectedIndex) ? hilite : normal;
                var item = menuItems[i].Text + " : " + menuItems[i].Value;

                spriteBatch.DrawString( spriteFont, item, location, tint,0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
				location.Y += spriteFont.LineSpacing + 5;
			}
		}
	}
    
}


using System;
using Game1.CollisionDetection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Humper.Sample.Basic;
using AuxLib.Debug;
using AuxLib;

namespace Game666
{
	public abstract class WorldScene : IScene
	{
		public WorldScene()
		{
		}

		protected World World { get; set; }
		 
		public virtual string Message
		{
			get
			{
				return $"[Up,Right,Down,Left]: move\n[Space]: show grid";
			} 
		}

		private SpriteBatch spriteBatch;

		private SpriteFont font;

		public virtual void Draw(SpriteBatch sb,SpriteFont font)
		{
			var b = this.World.Bounds;
			this.spriteBatch = sb;            
			this.World.DrawDebug(sb, font,(int)b.X, (int)b.Y, (int)b.Width, (int)b.Height);
		}

		//private void DrawCell(int x, int y, int w, int h, float alpha)
		//{
		//	if (Keyboard.GetState().IsKeyDown(Keys.Space))
		//		spriteBatch.DrawStroke(new Rectangle(x, y, w, h), new Color(Color.White, alpha));
		//}

		private void DrawBox(IBox box)
		{
			Color color;

			if (box.HasTag(Tags.Player))
				color = Color.White;
			else if (box.HasTag(Tags.Trigger))
				color = Color.Red;
			else if (box.HasTag(Tags.StaticBlock))
				color = Color.Green;
			else if (box.HasTag(Tags.PolyLine))
				color = Color.Yellow;
            else if (box.HasTag(Tags.JumpThroughBlock))
                color = Color.Blue;
            else
				color = new Color(165, 155, 250);

			spriteBatch.DrawRectangle(box.Bounds, color, 0.3f);
		}

        private void DrawPolyline(IPolyLine box)
        {
            var line = (IPolyLine)box;
            Color color;

            if (box.HasTag(Tags.Player))
                color = Color.White;
            else if (box.HasTag(Tags.Trigger))
                color = Color.Red;
            else if (box.HasTag(Tags.StaticBlock))
                color = Color.Green;
            else if (box.HasTag(Tags.PolyLine))
                color = Color.Yellow;
            else if (box.HasTag(Tags.JumpThroughBlock))
                color = Color.Blue;
            else
                color = new Color(165, 155, 250);

            for(var idx=0;idx < line.Points.Length-1;idx++)
            spriteBatch.DrawLine(color,line.Points[idx].ToVector2(), line.Points[idx+1].ToVector2(),0.3f);
        }

        public void LoadContent(ContentManager content)
		{
			this.font = content.Load<SpriteFont>("DiagnosticsFont");
		}
       

        public abstract void Initialize();


		public abstract void Update(GameTime time);
	}
}


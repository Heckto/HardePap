using System;
//using Game1.CollisionDetection;
//using Game1.CollisionDetection.Sample.Basic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Humper.Sample.Basic;
using AuxLib;
using tainicom.Aether.Physics2D.Dynamics;

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
		}

		
        public virtual void LoadContent(ContentManager content)
		{
			this.font = content.Load<SpriteFont>("DiagnosticsFont");
		}
       

        public abstract void Initialize();


		public abstract void Update(GameTime time);
	}
}


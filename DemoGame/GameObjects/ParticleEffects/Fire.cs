using System;
using System.IO;
using AuxLib.Serialization;
using Game1.GameObjects.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury;

namespace Game1.GameObjects.ParticleEffects
{
    public class FireEffect : GameObject, IDrawableItem, IUpdateableItem
    {
        private SpriteBatch batch;
        ParticleEffect effect;

        public FireEffect()
        {
            Visible = true;
            Name = "FIRE";
            var fn = Path.Combine(DemoGame.ContentManager.RootDirectory, "ParticleEffects\\small_fire_xml.xml");
            if (File.Exists(fn))
            {
                effect = XMLFileManager<ParticleEffect>.OpenFromFile(fn);
                foreach (var emitter in effect)
                {
                    emitter.ParticleTexture = DemoGame.ContentManager.Load<Texture2D>("Particles\\" + emitter.ParticleTextureAssetName);
                }
                effect.Initialise();
            }
            batch = new SpriteBatch(DemoGame.graphics.GraphicsDevice);            
        }

        public override GameObject clone()
        {
            throw new NotImplementedException();
        }

        public override bool contains(Vector2 worldpos)
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch sb,Matrix mat)
        {            
            batch.Draw(this.effect,ref mat);
        }

        public void Draw(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }

        public override void drawInEditor(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }

        public override void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color)
        {
            throw new NotImplementedException();
        }

        public override Rectangle getBoundingBox()
        {
            throw new NotImplementedException();
        }

        public override void OnTransformed()
        {
            //throw new NotImplementedException();
        }

        public void Update(GameTime gameTime, Level lvl)
        {
            this.effect.Trigger(Transform.Position);            
            this.effect.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}

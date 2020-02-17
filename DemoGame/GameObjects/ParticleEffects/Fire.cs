using System;
using System.IO;
using AuxLib.Serialization;
using AuxLib;
using Game1.GameObjects.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury;
using Microsoft.Xna.Framework.Content;

namespace Game1.GameObjects.ParticleEffects
{
    [Editable("ParticleEffects")]
    
    public class FireEffect : ParticleEffectObject
    {       

        public FireEffect() : base()
        {
            Visible = true;
            Name = "FIRE";           
        }

        public override void LoadContent(ContentManager contentManager)
        {
            var fn = Path.Combine(contentManager.RootDirectory, "ParticleEffects\\small_fire_xml.xml");
            if (File.Exists(fn))
            {
                effect = XMLFileManager<ParticleEffect>.OpenFromFile(fn);
                foreach (var emitter in effect)
                {
                    emitter.ParticleTexture = contentManager.Load<Texture2D>("Particles\\" + emitter.ParticleTextureAssetName);
                }
                effect.Initialise();
            }
            
        }

        public override void Initialize()
        {
            if (DemoGame.graphics != null)
            {
                batch = new SpriteBatch(DemoGame.graphics.GraphicsDevice);
            }
        }

    }
}

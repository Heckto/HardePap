using System;
using System.IO;
using AuxLib.Serialization;
using AuxLib.Extensions;
using Game1.GameObjects.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury;

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

        public override void LoadContent()
        {
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

        
    }
}

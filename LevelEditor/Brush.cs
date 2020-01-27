using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game1.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LevelEditor
{

    public class Brush
    {
        public string spriteName;
        

        public Brush(string name)
        {
            this.spriteName = name;            
        }

        public virtual void Draw(SpriteBatch sb, Vector2 pos) { }
    }
    public class TextureBrush : Brush
    {
        public string spriteSheet;
        public Texture2D texture;
        public TextureBrush(Texture2D tex,SpriteSheet ss, string name) : base(name)
        {
            this.spriteSheet = ss.Name;
            this.texture = tex;
        }

        public override void Draw(SpriteBatch sb, Vector2 pos)
        {
            sb.Draw(texture, new Vector2(pos.X, pos.Y), null, new Color(1f, 1f, 1f, 0.7f),
                        0, new Vector2(texture.Width / 2, texture.Height / 2), 1, SpriteEffects.None, 0);
            base.Draw(sb, pos);
        }
    }

    public class EntityBrush : Brush
    {
        public GameObject entity;

        public EntityBrush(GameObject entity, string name) : base(name)
        {
            this.entity = entity;
            
        }

        public override void Draw(SpriteBatch sb, Vector2 pos)
        {
            var bb = entity.getBoundingBox();
            
            Primitives.Instance.drawBox(sb, new Rectangle((int)pos.X, (int)pos.Y, bb.Width, bb.Height), Color.Red, 5);
            base.Draw(sb, pos);
        }
    }

}

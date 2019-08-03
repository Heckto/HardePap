using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Sprite
{
    public abstract class LivingSpriteObject : SpriteObject
    {
        public abstract int MaxHealth { get; }

        public virtual int CurrentHealth { get; protected set; }

        public virtual bool IsAlive => CurrentHealth > 0 && !(CurrentAnimation.AnimationName == "Dead" && CurrentAnimation.AnimationState == AnimationState.Finished);

        public virtual bool ShouldDraw => IsAlive;

        public LivingSpriteObject(ContentManager contentManager) : base(contentManager)
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            CurrentHealth = MaxHealth;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsAlive)
                return;

            if(CurrentHealth <= 0)
            {
                SetAnimation("Dead");
                return;
            }

            ManagedUpdate(gameTime);
        }

        public virtual void ManagedUpdate(GameTime gameTime)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsAlive)
                ManagedDraw(spriteBatch);
        }

        protected virtual void ManagedDraw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}

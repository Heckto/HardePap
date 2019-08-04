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

        public virtual bool IsAlive { get; protected set; }

        public virtual bool Dying { get; protected set; }

        public virtual bool ShouldDraw => IsAlive;

        public LivingSpriteObject(ContentManager contentManager) : base(contentManager)
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            CurrentHealth = MaxHealth;
            IsAlive = true;
        }

        public override void Update(GameTime gameTime)
        {
            if(IsAlive && (CurrentHealth > 0 && !Dying))
                ManagedUpdate(gameTime);

            if(CurrentHealth <= 0 || Dying)
            {
                OnDeath();
            }

            base.Update(gameTime);
        }

        public virtual void ManagedUpdate(GameTime gameTime)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (ShouldDraw)
                ManagedDraw(spriteBatch);
        }

        protected virtual void ManagedDraw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public virtual void DealDamage(SpriteObject sender, int damage)
        {
            CurrentHealth -= damage;
        }

        protected virtual void OnDeath()
        {
            if (!Dying)
            {
                Dying = true;
                SetAnimation("Dead");
            }
            else if (CurrentAnimation.AnimationName == "Dead" && CurrentAnimation.AnimationState == AnimationState.Finished)
            {
                World.Remove(CollisionBox);
                IsAlive = false;
            }
        }
    }
}

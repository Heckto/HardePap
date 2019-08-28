using Game1.Sprite.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game1.DataContext;
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

        public virtual float InvulnerabilityTime => 1000f;

        public virtual float InvulnerabilityTimer { get; protected set; }

        public bool HandleInput { get; set; } = true;

        public LivingSpriteObject(ContentManager contentManager,GameContext context) : base(contentManager, context)
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

            if (InvulnerabilityTimer > 0)
            {
                var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                InvulnerabilityTimer -= delta;
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
            base.Draw(spriteBatch, InvulnerabilityTimer > 0 ? AnimationEffect.FlashWhite : AnimationEffect.None);
        }

        public virtual void DealDamage(SpriteObject sender, int damage)
        {
            if (InvulnerabilityTimer > 0)
                return;

            CurrentHealth -= damage;

            if(CurrentHealth > 0)
            {
                InvulnerabilityTimer = InvulnerabilityTime;
            }
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
                context.lvl.CollisionWorld.Remove(CollisionBox);
                IsAlive = false;
            }
        }
    }
}

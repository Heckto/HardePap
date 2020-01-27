using Game1.GameObjects.Sprite.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game1.GameObjects.Levels;
using Game1.DataContext;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Game1.GameObjects.Sprite
{
    public abstract class LivingSpriteObject : SpriteObject , IUpdateableItem
    {
        [XmlIgnore]
        public abstract int MaxHealth { get; }

        [XmlIgnore]
        public virtual int CurrentHealth { get; protected set; }

        [XmlIgnore]
        public Controller2D controller;

        [XmlIgnore]
        public virtual bool Dying { get; protected set; }

        [XmlIgnore]
        public virtual float InvulnerabilityTime => 1000f;

        [XmlIgnore]
        public virtual float InvulnerabilityTimer { get; protected set; }

        [XmlIgnore]
        public bool HandleInput = true;

        [XmlIgnore]
        public Vector2 velocity;

        public LivingSpriteObject() { }

        public LivingSpriteObject(GameContext context) : base(context)
        {
            Initialize();
        }

        public override void Initialize()
        {
            CurrentHealth = MaxHealth;
            IsAlive = true;
        }

        public override void Update(GameTime gameTime, Level lvl)
        {
            if(CurrentHealth <= 0 || Dying && IsAlive)
            {
                OnDeath();
            }

            if (InvulnerabilityTimer > 0)
            {
                var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                InvulnerabilityTimer -= delta;
            }

            base.Update(gameTime, lvl);
        }

        //protected virtual void ManagedDraw(SpriteBatch spriteBatch)
        //{
        //    base.Draw(spriteBatch, InvulnerabilityTimer > 0 ? AnimationEffect.FlashWhite : AnimationEffect.None);
        //}

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

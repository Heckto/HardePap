using Game1.GameObjects.Sprite.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Game1.DataContext;
using tainicom.Aether.Physics2D.Dynamics;
using Game1.GameObjects.Levels;
using System.Xml.Serialization;
using System.ComponentModel;
using AuxLib;
using Game1.Rendering;

namespace Game1.GameObjects.Sprite
{
    public abstract class SpriteObject : GameObject , IEditableGameObject
    {
        

        public abstract Vector2 Size
        {
            get;
        }

        [XmlIgnore]
        public Body CollisionBox;

        [XmlIgnore]
        public GameContext context;

        [XmlIgnore]
        protected Color Color { get; set; }

        [XmlIgnore]
        protected Dictionary<string, SpriteAnimation> Animations = new Dictionary<string, SpriteAnimation>();

        [XmlIgnore]
        protected SpriteAnimation CurrentAnimation;

        private FaceDirection dir = FaceDirection.Right;

        [Browsable(false)]
        public FaceDirection Direction
        {
            get { return dir; }
            set
            {
                if (dir != value)
                    dir = value;
            }
        }

        public Vector2 colBodySize = new Vector2(90,100);

        public SpriteObject() {
            IsAlive = true;
        }

        public SpriteObject(GameContext context)
        {
            //LoadContent();
            IsAlive = true;
            this.context = context;
        }

        public override void Initialize()
        {
            Material = RenderMaterial.DefaultMaterial;
        }

        public override void LoadContent(ContentManager contentManager)
        {
            base.LoadContent(contentManager);
        }

        public virtual void LoadFromSheet(string fileLocation, ContentManager content)
        {
            var config = SpriteConfig.Deserialize(fileLocation);
            var frameDictionary = SpriteAnimationFrameSpriteSheet.FromDefinitionFile(config.SpritesheetDefinitionFile, content);

            foreach (var animation in config.Animations)
            {
                Animations.Add(animation.AnimationName, new SpriteAnimation(animation, frameDictionary));
            }
        }        

        public virtual void Update(GameTime gameTime, Level lvl)
        {
            CurrentAnimation.Update(gameTime);
        }


        public virtual void Draw(SpriteBatch spriteBatch, AnimationEffect animationEffect)
        {
            var flip = (Direction == FaceDirection.Left);

            if(CurrentAnimation != null)
                CurrentAnimation.Draw(spriteBatch, (flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None), Transform.Position, 0, 0.5f, Color, animationEffect);
        }

        public void SetAnimation(string name)
        {
            if (Animations.ContainsKey(name))
            {
                var animation = Animations[name];
                if (CurrentAnimation != animation)
                {
                    animation.Reset();
                    CurrentAnimation = animation;
                }
            }
        }

        public enum FaceDirection
        {
            Left,
            Right
        }

        #region Editor

        public override Rectangle getBoundingBox()
        {
            
            return new Rectangle((int)Transform.Position.X,(int)Transform.Position.Y,(int)Size.X,(int)Size.Y);
        }

        [XmlIgnore]
        Vector2[] polygon;
        public override GameObject clone()
        {
            var result = (SpriteObject)this.MemberwiseClone();
            result.CustomProperties = new SerializableDictionary(CustomProperties);
            //result.polygon = (Vector2[])polygon.Clone();
            result.hovering = false;
            return result;
        }

        public override void OnTransformed()
        {
            polygon = new Vector2[4];
            //transform =
            //    Matrix.CreateTranslation(new Vector3(-Origin.X, -Origin.Y, 0.0f)) *
            //    ///Matrix.CreateScale(Scale.X, Scale.Y, 1) *
            //    //Matrix.CreateRotationZ(Rotation) *
            //    Matrix.CreateTranslation(new Vector3(transform.Position, 0.0f));
            
            var leftTop = new Vector2(0, 0);
            var rightTop = new Vector2(Size.X, 0);
            var leftBottom = new Vector2(0, Size.Y);
            var rightBottom = new Vector2(Size.X, Size.Y);

            Transform.GetLocalMatrix(out var mat);
            // Transform all four corners into work space
            leftTop = Vector2.Transform(leftTop, mat);
            rightTop = Vector2.Transform(rightTop, mat);
            leftBottom = Vector2.Transform(leftBottom, mat);
            rightBottom = Vector2.Transform(rightBottom, mat);

            polygon[0] = leftTop;
            polygon[1] = rightTop;
            polygon[3] = leftBottom;
            polygon[2] = rightBottom;

            // Find the minimum and maximum extents of the rectangle in world space
            var min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            var max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            // Return as a rectangle
            boundingrectangle = new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }


        public override void onMouseButtonDown(Vector2 mouseworldpos)
        {
            hovering = false;
            //MainForm.Instance.picturebox.Cursor = Cursors.SizeAll;
            base.onMouseButtonDown(mouseworldpos);
        }


        public override void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;           
            var c = hovering ? new Color(255, 0, 0, 228) : Color;

            var flip = (Direction == FaceDirection.Left);

            if (CurrentAnimation != null)
                CurrentAnimation.Draw(sb, (flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None), Transform.Position, 0, 0.5f, c, AnimationEffect.None);


            Primitives.Instance.drawBox(sb, getBoundingBox(),c, 5);
        }

        public override void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color)
        {
            var poly = new Vector2[4];
            Vector2.Transform(polygon, ref matrix, poly);

            Primitives.Instance.drawPolygon(sb, poly, color, 2);
            foreach (var p in poly)
            {
                Primitives.Instance.drawCircleFilled(sb, p, 4, color);
            }
            var origin = Vector2.Transform(Transform.Position, matrix);
            Primitives.Instance.drawBoxFilled(sb, origin.X - 5, origin.Y - 5, 10, 10, color);
        }

        public override bool contains(Vector2 worldpos)
        {
            return boundingrectangle.Contains((int)worldpos.X, (int)worldpos.Y);
        }
        #endregion
    }
}

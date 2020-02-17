using AuxLib;
using AuxLib.Debug;
using Game1.DataContext;
using Game1.GameObjects.Levels;
using Game1.GameObjects.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace Game1.GameObjects.Obstacles
{
    [Editable("Entities")]
    public class MovingPlatform : SpriteObject, IUpdateableItem
    {
        private PlatformController cntlr;

        public List<Vector2> WayPoints { get; set; }

        private Vector2 defSize = new Vector2(200,100);
        public Category collidesWith { get; set; }

        private GameObject movingObj;

        public MovingPlatform() {
            WayPoints = new List<Vector2>();
        }

        public MovingPlatform(GameContext context, World world, Category cat = Category.None) : base(context)
        {

            collidesWith = cat;
            WayPoints = new List<Vector2>();
        }

        public override Vector2 Size
        {
            get { return defSize; }
        }


        public override void LoadContent(ContentManager contentManager)
        {
            IsAlive = true;
            
        }

        public override void Initialize()
        {
            colBodySize = Size;
            CollisionBox = context.lvl.CollisionWorld.CreateRectangle((float)ConvertUnits.ToSimUnits(Size.X), (float)ConvertUnits.ToSimUnits(Size.Y), 1, ConvertUnits.ToSimUnits(Transform.Position), 0, BodyType.Kinematic);
            CollisionBox.SetCollisionCategories(collidesWith);
            CollisionBox.Tag = this;
            cntlr = new PlatformController(CollisionBox, Category.Cat2)
            {
                speed = 0.2f,
                easeAmount = 1.7f,
                waitTime = 0.5f,
                globalWaypoints = WayPoints.ToArray()
            };

            if (CustomProperties.ContainsKey("tex"))
            {
                var texture = CustomProperties["tex"].value.ToString();
                movingObj = context.lvl.getItemByName(texture);
                movingObj.Transform.Position = Transform.Position;
            }

            base.Initialize();
        }

        public override Rectangle getBoundingBox()
        {

            return new Rectangle((int)Transform.Position.X, (int)Transform.Position.Y, (int)Size.X, (int)Size.Y);
        }

        public override void Update(GameTime gameTime, Level lvl)
        {
            var oldPos = Transform.Position;
            cntlr.Update(gameTime);
            Transform.Position = ConvertUnits.ToDisplayUnits(CollisionBox.Position);

            var d = Transform.Position - oldPos;

            if (movingObj != null)
                movingObj.Transform.Position += d;
        }

        public override void drawInEditor(SpriteBatch sb)
        {
            base.drawInEditor(sb);

            if (WayPoints.Count > 0)
            {
                Primitives.Instance.drawLine(sb, Transform.Position, WayPoints[0], Color.Blue, 3);
                for (var idx = 0; idx < WayPoints.Count - 1; idx++)
                {
                    var s1 = WayPoints[idx];
                    var s2 = WayPoints[idx + 1];
                    Primitives.Instance.drawLine(sb, s1, s2, Color.Blue, 3);
                }
            }

        }

    }
}

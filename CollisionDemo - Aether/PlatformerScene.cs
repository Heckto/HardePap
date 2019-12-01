using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Game666
{

    public class PlatformerScene
    {

        protected KeyboardState keyState;
        protected KeyboardState oldState;
        protected MouseState prevMouseState;

        private DebugView _debugView;

        protected World World { get; set; }
        private Player player;
        public Camera camera;

        private GraphicsDevice device;

        private KeyboardState prevState;

        public List<MovingPlatform> platformList = new List<MovingPlatform>();

        public PlatformerScene()
        {
            

        }

        private SpriteBatch spriteBatch;

        private SpriteFont font;

        private List<Rectangle> rectangleList = new List<Rectangle>();
        private List<Vector2[]> polyLineList = new List<Vector2[]>();
        public void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            this.font = content.Load<SpriteFont>("DiagnosticsFont");

            device = graphics;
            _debugView.LoadContent(graphics, content);
            camera = new Camera(graphics.Viewport,new Rectangle(0,0,2400,1600));
            camera.SetViewTarget(player.controller);
        }
        public void Initialize()
        {
            this.World = new World();
            _debugView = new DebugView(World);
            _debugView.AppendFlags(DebugViewFlags.Shape);           

            player = new Player(new Vector2(400, 75), new Vector2(20, 60), World);
            
            //Ground / Gravity
            TestLevel1();
            // Rect walls
            TestLevel2();
            //Slopes
            TestLevel3();
            // Max. Slopes
            TestLevel4();
            //Objects on slopes
            TestLevel5();
            // Curved Slopes
            TestLevel6();
            // Descending slopes
            TestLevel7();
            // Collision flags / filtering
            TestLevel8();
            // Moving platforms;
            TestLevel9();
            // Moving pushing platforms
            TestLevel10();
            //One way platforms UP / DOWN 
            TestLevel11();
            //One way moving platforms UP /DOWN
            TestLevel12();
            // Wall climbin
            TestLevel13();
            // Sliding slopes
            TestLevel14();






            foreach (var r in rectangleList)
            {
                var origin = new Vector2(r.Width / 2, r.Height / 2);
                var b = World.CreateRectangle(ConvertUnits.ToSimUnits(r.Width), ConvertUnits.ToSimUnits(r.Height), 1f, new Vector2(ConvertUnits.ToSimUnits(r.X + origin.X), ConvertUnits.ToSimUnits(r.Y + origin.Y)));
                b.BodyType = BodyType.Static;                
                b.SetCollisionCategories(Category.Cat2);
            }
            foreach (var polyLine in polyLineList)
            {
                for (var pIdx = 0; pIdx < polyLine.Length - 1; pIdx++)
                {
                    var e = World.CreateEdge(ConvertUnits.ToSimUnits(polyLine[pIdx]), ConvertUnits.ToSimUnits(polyLine[pIdx + 1]));
                    e.SetCollisionCategories(Category.Cat2);
                }
            }
        }

        public void TestLevel1()
        {
           rectangleList.Add(new Rectangle(0, 780, 1600, 20));
            rectangleList.Add(new Rectangle(1700, 780, 800, 20));

            rectangleList.Add(new Rectangle(0, 1580, 2400, 20));
        }

        public void TestLevel2()
        {
            rectangleList.Add(new Rectangle(200, 600, 20, 200));
            rectangleList.Add(new Rectangle(800, 600, 20, 200));
        }

        public void TestLevel3()
        {
            Vector2[] points = new Vector2[] {
                new Vector2(0,400),
                new Vector2(200, 600)
                };
            polyLineList.Add(points);

            Vector2[] points2 = new Vector2[] {
                new Vector2(820,600),
                new Vector2(1000, 400)
                };

            polyLineList.Add(points2);
        }

        public void TestLevel4()
        {
            Vector2[] points = new Vector2[] {
                new Vector2(800,400),
                new Vector2(850, 0)
                };
            polyLineList.Add(points);
        }

        public void TestLevel5()
        {
            rectangleList.Add(new Rectangle(100, 400, 100, 20));
            rectangleList.Add(new Rectangle(0, 300, 20, 200));
            rectangleList.Add(new Rectangle(1180, 0, 20, 700));
        }

        public void TestLevel6()
        {
            Vector2[] points = new Vector2[] {
                new Vector2(200,400),
                new Vector2(250,400),
                new Vector2(300,350),
                new Vector2(400,350)
                };
            polyLineList.Add(points);
        }

        public void TestLevel7()
        {
            Vector2[] points = new Vector2[] {
                new Vector2(600,350),
                new Vector2(650,350),
                new Vector2(700,400),
                new Vector2(800,400)
                };
            polyLineList.Add(points);
        }

        public void TestLevel8()
        {
            var r = new Rectangle(500, 680, 100, 100);
            var origin = new Vector2(r.Width / 2, r.Height / 2);
            var b = World.CreateRectangle(ConvertUnits.ToSimUnits(r.Width), ConvertUnits.ToSimUnits(r.Height), 1f, new Vector2(ConvertUnits.ToSimUnits(r.X + origin.X), ConvertUnits.ToSimUnits(r.Y + origin.Y)));
            b.BodyType = BodyType.Static;
            b.SetCollisionCategories(Category.Cat20);
        }

        public void TestLevel9()
        {
            var loc = new Vector2(500, 500);
            Vector2[] wayPoints2 = new Vector2[]
            {
                new Vector2(500, 400),
                new Vector2(500, 500),
                new Vector2(300, 500)
            };
            var platform = new MovingPlatform(new Vector2(100, 20), loc, World, wayPoints2);
            platformList.Add(platform);
        }

        public void TestLevel10()
        {
            var loc = new Vector2(600, 680);
            Vector2[] wayPoints2 = new Vector2[]
            {
                new Vector2(400, 730),
                new Vector2(800, 730)                
            };
            var platform = new MovingPlatform(new Vector2(20, 100), loc, World, wayPoints2);
            platformList.Add(platform);
        }

        public void TestLevel11()
        {
            for (var idx = 0; idx < 2; idx++)
            { 
                var r = new Rectangle(900, 200 + (idx * 150), 100, 10);
                var origin = new Vector2(r.Width / 2, r.Height / 2);
                var b = World.CreateRectangle(ConvertUnits.ToSimUnits(r.Width), ConvertUnits.ToSimUnits(r.Height), 1f, new Vector2(ConvertUnits.ToSimUnits(r.X + origin.X), ConvertUnits.ToSimUnits(r.Y + origin.Y)));
                b.BodyType = BodyType.Static;
                b.SetCollisionCategories(Category.Cat4 | Category.Cat5);
            }
            
        }

        public void TestLevel12()
        {
            var loc = new Vector2(100, 100);
            Vector2[] wayPoints2 = new Vector2[]
            {
                new Vector2(100, 100),
                new Vector2(100, 300)
            };
            var platform = new MovingPlatform(new Vector2(100, 20), loc, World, wayPoints2,Category.Cat4 | Category.Cat5);
            platformList.Add(platform);
        }

        public void TestLevel13()
        {
            var r = new Rectangle(1000, 100, 20, 800);
            var origin = new Vector2(r.Width / 2, r.Height / 2);
            var b = World.CreateRectangle(ConvertUnits.ToSimUnits(r.Width), ConvertUnits.ToSimUnits(r.Height), 1f, new Vector2(ConvertUnits.ToSimUnits(r.X + origin.X), ConvertUnits.ToSimUnits(r.Y + origin.Y)));
            b.BodyType = BodyType.Static;
            b.SetCollisionCategories(Category.Cat2);
        }

        public void TestLevel14()
        {
            Vector2[] points = new Vector2[] {
                new Vector2(1600,780),
                new Vector2(1750,1400)
                };
            polyLineList.Add(points);
        }

        public void Update(GameTime time)
        {
            var state = Keyboard.GetState();
            if (state.IsKeyUp(Keys.R) && prevState.IsKeyDown(Keys.R))
            {
                player.Kill();
                player = new Player(new Vector2(400, 75), new Vector2(20, 48), World);
                camera.SetViewTarget(player.controller);
            }

            foreach (var entity in platformList)
                entity.Update(time);

            player.Update(time);







            camera.UpdateCamera(time);


            prevState = state;
        }

        public void Draw(SpriteBatch sb, SpriteFont font)
        {
            var projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(sb.GraphicsDevice.Viewport.Width), ConvertUnits.ToSimUnits(sb.GraphicsDevice.Viewport.Height), 0f, 0f, 1f);
            _debugView.RenderDebugData(projection, camera.getScaledViewMatrix());


            var projection2 = Matrix.CreateOrthographicOffCenter(0f, sb.GraphicsDevice.Viewport.Width, sb.GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            _debugView.BeginCustomDraw(projection2, camera.getViewMatrix());

            foreach (var ray in player.controller.castList)
                _debugView.DrawSegment(ray.from, ray.to, Color.Blue);

            foreach (var p in platformList)
            {
                foreach (var ray in p.controller.castList)
                {
                    _debugView.DrawSegment(ray.from, ray.to, Color.White);
                }
            }


            var areaPoints = new Vector2[] {
                ConvertUnits.ToDisplayUnits(new Vector2(camera.focusArea.left,camera.focusArea.top)),
                ConvertUnits.ToDisplayUnits(new Vector2(camera.focusArea.right, camera.focusArea.top)),
                ConvertUnits.ToDisplayUnits(new Vector2(camera.focusArea.right, camera.focusArea.bottom)),
                ConvertUnits.ToDisplayUnits(new Vector2(camera.focusArea.left, camera.focusArea.bottom))
            };            
            _debugView.DrawSolidPolygon(areaPoints, 4, Color.Red);

            _debugView.DrawPoint(ConvertUnits.ToDisplayUnits(camera.focusPosition), 3, Color.White);

            _debugView.DrawPoint(camera.Position, 3, Color.Pink);

            var cameraBounds = new Vector2[] {
                new Vector2(camera.Bounds.Left,camera.Bounds.Top),
                new Vector2(camera.Bounds.Right, camera.Bounds.Top),
                new Vector2(camera.Bounds.Right, camera.Bounds.Bottom),
                new Vector2(camera.Bounds.Left, camera.Bounds.Bottom)
            };

            _debugView.DrawPolygon(cameraBounds, 4, Color.Green);

            _debugView.EndCustomDraw();
        }        
    }

    public static class ConvertUnits
    {
        private static float _displayUnitsToSimUnitsRatio = 100f;
        private static float _simUnitsToDisplayUnitsRatio = 1 / _displayUnitsToSimUnitsRatio;

        public static void SetDisplayUnitToSimUnitRatio(float displayUnitsPerSimUnit)
        {
            _displayUnitsToSimUnitsRatio = displayUnitsPerSimUnit;
            _simUnitsToDisplayUnitsRatio = 1 / displayUnitsPerSimUnit;
        }

        public static float ToDisplayUnits(float simUnits)
        {
            return simUnits * _displayUnitsToSimUnitsRatio;
        }

        public static float ToDisplayUnits(int simUnits)
        {
            return simUnits * _displayUnitsToSimUnitsRatio;
        }

        public static Vector2 ToDisplayUnits(Vector2 simUnits)
        {
            return simUnits * _displayUnitsToSimUnitsRatio;
        }

        public static void ToDisplayUnits(ref Vector2 simUnits, out Vector2 displayUnits)
        {
            Vector2.Multiply(ref simUnits, _displayUnitsToSimUnitsRatio, out displayUnits);
        }

        public static Vector3 ToDisplayUnits(Vector3 simUnits)
        {
            return simUnits * _displayUnitsToSimUnitsRatio;
        }

        public static Vector2 ToDisplayUnits(float x, float y)
        {
            return new Vector2(x, y) * _displayUnitsToSimUnitsRatio;
        }

        public static void ToDisplayUnits(float x, float y, out Vector2 displayUnits)
        {
            displayUnits = Vector2.Zero;
            displayUnits.X = x * _displayUnitsToSimUnitsRatio;
            displayUnits.Y = y * _displayUnitsToSimUnitsRatio;
        }

        public static float ToSimUnits(float displayUnits)
        {
            return displayUnits * _simUnitsToDisplayUnitsRatio;
        }

        public static float ToSimUnits(double displayUnits)
        {
            return (float)displayUnits * _simUnitsToDisplayUnitsRatio;
        }

        public static float ToSimUnits(int displayUnits)
        {
            return displayUnits * _simUnitsToDisplayUnitsRatio;
        }

        public static Vector2 ToSimUnits(Vector2 displayUnits)
        {
            return displayUnits * _simUnitsToDisplayUnitsRatio;
        }

        public static Vector3 ToSimUnits(Vector3 displayUnits)
        {
            return displayUnits * _simUnitsToDisplayUnitsRatio;
        }

        public static void ToSimUnits(ref Vector2 displayUnits, out Vector2 simUnits)
        {
            Vector2.Multiply(ref displayUnits, _simUnitsToDisplayUnitsRatio, out simUnits);
        }

        public static Vector2 ToSimUnits(float x, float y)
        {
            return new Vector2(x, y) * _simUnitsToDisplayUnitsRatio;
        }

        public static Vector2 ToSimUnits(double x, double y)
        {
            return new Vector2((float)x, (float)y) * _simUnitsToDisplayUnitsRatio;
        }

        public static void ToSimUnits(float x, float y, out Vector2 simUnits)
        {
            simUnits = Vector2.Zero;
            simUnits.X = x * _simUnitsToDisplayUnitsRatio;
            simUnits.Y = y * _simUnitsToDisplayUnitsRatio;
        }
    }
}




using System;
using Humper;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using AuxLib;
using System.Diagnostics;
using tainicom.Aether.Physics2D;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Game666
{
	

    public enum Tags
    {
        StaticBlock = 1,
        PolyLine = 2,
        Player = 4,
        Trigger = 8,
        JumpThroughBlock=16
    }

    public enum Activity { None,Idle, Jumping, Running };

    public class PlatformerScene : WorldScene
	{

        protected KeyboardState keyState;
        protected KeyboardState oldState;

        private const int MaxJump = 2;
        private int JumpCnt = 0;
        private DebugView _debugView;

        private Vector2 playerSize = new Vector2(20, 48);
		public PlatformerScene()
		{
            

        }

        private Body upperBody;
        private RevoluteJoint motor;

        private List<Rectangle> rectangleList = new List<Rectangle>();
        private List<Vector2[]> polyLineList = new List<Vector2[]>();
        private Activity oldActivity = Activity.Idle;
        private Activity activity = Activity.Idle;

        private Vector2 jumpForce;
        private float jumpDelayTime;

        private const float nextJumpDelayTime = 0f;
        private const float runSpeed = 40;
        private const float jumpImpulse = -20;
        private const float restitution = 0.0f;
        private const float friction = 0.1f;

        public void LoadContent(GraphicsDevice graphics,ContentManager content)
        {
            _debugView.LoadContent(graphics, content);
        }
        public override void Initialize()
		{
            
            
            this.World = new World();
            _debugView = new DebugView(World);
            
            World.Gravity = new Vector2(0, 10f);

			this.SpawnPlayer(playerSize.X,playerSize.Y,new Vector2(400,75));

            // Map
            rectangleList.Add(new Rectangle(0,0, 20, 300));
            rectangleList.Add(new Rectangle(120, 200, 50, 20));

            polyLineList.Add( new Vector2[] {
                         new Vector2(170,200),
                         new Vector2(230, 175),
                         new Vector2(270, 200),
                         new Vector2(320, 250),
                         new Vector2(350, 200),
                         new Vector2(370, 200),
                         });
            rectangleList.Add(new Rectangle(370, 200, 50, 20));
            rectangleList.Add(new Rectangle(0, 300, 400, 20));
            rectangleList.Add(new Rectangle(380, 320, 20, 80));
            rectangleList.Add(new Rectangle(380, 400, 300, 20));

            polyLineList.Add(new Vector2[] {
                         new Vector2(380,50),
                         new Vector2(450, 50)
                         });

            polyLineList.Add(new Vector2[] {
                         new Vector2(380,100),
                         new Vector2(450, 100)
                         });

            polyLineList.Add(new Vector2[] {
                         new Vector2(380,150),
                         new Vector2(450, 150)
                         });

            rectangleList.Add(new Rectangle(420, 200, 200, 20));
            rectangleList.Add(new Rectangle(680, 220, 20, 200));
            rectangleList.Add(new Rectangle(680, 200, 200, 20));

            rectangleList.Add(new Rectangle(520, 100, 200, 20));
            rectangleList.Add(new Rectangle(400, 300, 280, 100));
            rectangleList.Add(new Rectangle(0, 780, 1200, 20));
            rectangleList.Add(new Rectangle(1180, 0, 20, 780));
            rectangleList.Add(new Rectangle(1100, 100, 20, 580));
            rectangleList.Add(new Rectangle(900, 80, 220, 20));
            rectangleList.Add(new Rectangle(900, 280, 200, 20));

            
            polyLineList.Add(new Vector2[] {
                         new Vector2(900,280),
                         new Vector2(920, 260),
                         new Vector2(940, 250),
                         new Vector2(945, 240),
                         new Vector2(1000, 230),
                         new Vector2(1050, 230),
                         new Vector2(1080, 210),
                         new Vector2(1100, 200)
                         });

            rectangleList.Add(new Rectangle(700, 400, 200, 20));
            polyLineList.Add(new Vector2[] {
                         new Vector2(900,400),
                         new Vector2(950, 450)
                         });

            foreach (var r in rectangleList)
            {
                var origin = new Vector2(r.Width / 2, r.Height / 2);
                var b = World.CreateRectangle(ConvertUnits.ToSimUnits(r.Width), ConvertUnits.ToSimUnits(r.Height), 1f, new Vector2(ConvertUnits.ToSimUnits(r.X+origin.X), ConvertUnits.ToSimUnits(r.Y+origin.Y)));
                b.BodyType = BodyType.Static;
                b.SetCollisionCategories(Category.Cat2);
            }
            foreach(var polyLine in polyLineList)
            {
                for (var pIdx = 0; pIdx < polyLine.Length - 1; pIdx++)
                {
                    var e = World.CreateEdge(ConvertUnits.ToSimUnits(polyLine[pIdx]), ConvertUnits.ToSimUnits(polyLine[pIdx + 1]));
                    e.SetCollisionCategories(Category.Cat3);
                }
            }
            
        }

		private void SpawnPlayer(float width,float height,Vector2 position)
		{
            float upperBodyHeight = height - (width / 2);
            var mass = 80;            
            upperBody = World.CreateRectangle((float)ConvertUnits.ToSimUnits(width), (float)ConvertUnits.ToSimUnits(upperBodyHeight), mass / 2);
            upperBody.BodyType = BodyType.Dynamic;
            upperBody.SetRestitution(restitution);
            upperBody.SetFriction(friction);
            //also shift it up a tiny bit to keey the new object's center correct
            upperBody.Position = ConvertUnits.ToSimUnits(position - (Vector2.UnitY * (width / 4)));
            var centerOffset = position.Y - (float)ConvertUnits.ToDisplayUnits(upperBody.Position.Y); //remember the offset from the center for drawing

            upperBody.FixedRotation = true;
            upperBody.SetCollisionCategories(Category.Cat1);

            upperBody.OnCollision += UpperBody_OnCollision;
            //Create a wheel as wide as the whole object
            var wheel = World.CreateCircle((float)ConvertUnits.ToSimUnits(width / 2), mass / 2);
            //And position its center at the bottom of the upper body
            wheel.Position = upperBody.Position + ConvertUnits.ToSimUnits(Vector2.UnitY * (upperBodyHeight / 2));
            wheel.BodyType = BodyType.Dynamic;
            wheel.SetRestitution(restitution);
            wheel.SetFriction(friction);

            //These two bodies together are width wide and height high :)
            //So lets connect them together
            
            motor = JointFactory.CreateRevoluteJoint(World, upperBody, wheel, Vector2.Zero);
            motor.MotorEnabled = true;
            motor.MaxMotorTorque = 10000f; //set this higher for some more juice
            motor.MotorSpeed = 0;           

            //Make sure the two fixtures don't collide with each other
            //wheel.CollisionFilter.IgnoreCollisionWith(fixture);
            //fixture.CollisionFilter.IgnoreCollisionWith(wheel);

            //Set the friction of the wheel to float.MaxValue for fast stopping/starting
            //or set it higher to make the character slip.
            wheel.SetFriction(float.MaxValue);
            wheel.SetCollisionCategories(Category.Cat1);
            wheel.OnCollision += Wheel_OnCollision;
        }

        private bool UpperBody_OnCollision(Fixture sender, Fixture other, tainicom.Aether.Physics2D.Dynamics.Contacts.Contact contact)
        {
            
            //if (other.CollisionCategories.HasFlag(Category.Cat3) && sender.Body.LinearVelocity.Y <= 0)
              //  return false;                
            return true;
        }

        private bool Wheel_OnCollision(Fixture sender, Fixture other, tainicom.Aether.Physics2D.Dynamics.Contacts.Contact contact)
        {            
                //Check if we are both jumping this frame and last frame
                //so that we ignore the initial collision from jumping away from 
                //the ground
            if (activity == Activity.Jumping && oldActivity == Activity.Jumping)
            {
                activity = Activity.None;
            }
            return true;
        
        }

        public override void Update(GameTime time)
		{
            World.Step((float)(time.ElapsedGameTime.TotalMilliseconds * 0.001));
            HandleInput(time);
        }

		private Vector2 velocity = Vector2.Zero;
		private KeyboardState state;

        public override void Draw(SpriteBatch sb, SpriteFont font)
        {
            foreach(var r in rectangleList)
            {
                var o = new Vector2(r.Width / 2, r.Height / 2);
                sb.DrawRectangle(r, Color.Blue);
            }

            var projection = Matrix.CreateOrthographicOffCenter(0f,ConvertUnits.ToSimUnits(sb.GraphicsDevice.Viewport.Width),ConvertUnits.ToSimUnits(sb.GraphicsDevice.Viewport.Height), 0f, 0f,1f);
            _debugView.RenderDebugData(ref projection);

        }

        protected void HandleInput(GameTime gameTime)
        {
            oldActivity = activity;
            keyState = Keyboard.GetState();

            HandleJumping(keyState, oldState, gameTime);

            if (activity != Activity.Jumping)
            {
                HandleRunning(keyState, oldState, gameTime);
            }

            if (activity != Activity.Jumping && activity != Activity.Running)
            {
                HandleIdle(keyState, oldState, gameTime);
            }

            oldState = keyState;
        }

        private void HandleJumping(KeyboardState state, KeyboardState oldState, GameTime gameTime)
        {
            if (jumpDelayTime < 0)
            {
                jumpDelayTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (state.IsKeyUp(Keys.Down) && oldState.IsKeyDown(Keys.Up) && activity != Activity.Jumping)
            {
                if (jumpDelayTime >= 0)
                {
                    //motor.MotorSpeed = 0;
                    jumpForce.X = upperBody.LinearVelocity.X;
                    jumpForce.Y = jumpImpulse;
                    upperBody.ApplyLinearImpulse(jumpForce, upperBody.Position);
                    jumpDelayTime = -nextJumpDelayTime;
                    activity = Activity.Jumping;
                }
            }

            if (activity == Activity.Jumping)
            {
                if (keyState.IsKeyDown(Keys.Right))
                {
                    if (upperBody.LinearVelocity.X < 0)
                    {
                        upperBody.LinearVelocity = new Vector2(-upperBody.LinearVelocity.X * 5, upperBody.LinearVelocity.Y);
                    }
                    else
                        upperBody.LinearVelocity = new Vector2(-1, upperBody.LinearVelocity.Y);
                }
                else if (keyState.IsKeyDown(Keys.Left))
                {
                    if (upperBody.LinearVelocity.X > 0)
                    {
                        upperBody.LinearVelocity = new Vector2(-upperBody.LinearVelocity.X * 5, upperBody.LinearVelocity.Y);
                    }
                    else
                        upperBody.LinearVelocity = new Vector2(1, upperBody.LinearVelocity.Y);
                }
            }


        }

        private void HandleRunning(KeyboardState state, KeyboardState oldState, GameTime gameTime)
        {
            if (keyState.IsKeyDown(Keys.Right))
            {
                motor.MotorSpeed = runSpeed;
                activity = Activity.Running;
            }
            else if (keyState.IsKeyDown(Keys.Left))
            {
                motor.MotorSpeed = -runSpeed;
                activity = Activity.Running;
            }

            if (keyState.IsKeyUp(Keys.Left) && keyState.IsKeyUp(Keys.Right))
            {
                motor.MotorSpeed = 0;
                activity = Activity.None;
            }
        }

        private void HandleIdle(KeyboardState state, KeyboardState oldState, GameTime gameTime)
        {
            if (activity == Activity.None)
            {
                activity = Activity.Idle;
            }
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



using System;
using AuxLib.CollisionDetection;
using AuxLib.CollisionDetection.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using AuxLib;

namespace Game666
{

    public enum Tags
    {
        StaticBlock = 1,
        PolyLine = 2,
        MoveableBlock = 4,
        Player = 8,
        Trigger = 16,
        JumpThroughBlock=32
    }

    public class PlatformerScene : WorldScene
	{
        private const int MaxJump = 2;
        private int JumpCnt = 0;
		public PlatformerScene() {}

        private MoveableBody movingBox;

		private MoveableBody player1;

        public void CreateLevel1()
        {
            this.World.CreateRectangle(200, 400, 400, 20).AddTags(Tags.StaticBlock);


            Vector2f[] points2 = new Vector2f[] {
                new Vector2f(200,250),
                new Vector2f(600, 300)
                };
            this.World.CreatePolyLine(points2).AddTags(Tags.PolyLine,Tags.JumpThroughBlock);

            Vector2f[] points = new Vector2f[] {
                new Vector2f(600,400),
                new Vector2f(700, 350),
                new Vector2f(800, 350)
                };
            this.World.CreatePolyLine(points).AddTags(Tags.PolyLine);

            

            Vector2f[] points4 = new Vector2f[] {
                new Vector2f(750,250),
                new Vector2f(1000, 300)

                };
            this.World.CreatePolyLine(points4).AddTags(Tags.PolyLine);

            Vector2f[] points3 = new Vector2f[] {
                new Vector2f(0,600),
                new Vector2f(200, 400)
                };
            this.World.CreatePolyLine(points3).AddTags(Tags.PolyLine);

            this.World.CreateRectangle(800, 350, 400, 20).AddTags(Tags.StaticBlock);

            this.World.CreateRectangle(800, 150, 200, 20).AddTags(Tags.StaticBlock,Tags.JumpThroughBlock);

            movingBox = (MoveableBody)this.World.CreateMoveableBody(300, 150, 100, 20);
            movingBox.AddTags(Tags.MoveableBlock);
            movingBox.Velocity = Vector2f.UnitX * 0.05f; 
        }


        public void CreateLevel2()
        {
            // Map
            this.World.CreateRectangle(0, 0, 20, 300).AddTags(Tags.StaticBlock);

            this.World.CreateRectangle(120, 200, 50, 20).AddTags(Tags.StaticBlock);

            Vector2f[] points = new Vector2f[] {
                new Vector2f(170,200),
                new Vector2f(230, 175),
                new Vector2f(270, 200),
                new Vector2f(320, 250),
                new Vector2f(350, 200),
                new Vector2f(370, 200),
                };
            this.World.CreatePolyLine(points).AddTags(Tags.PolyLine);
            this.World.CreateRectangle(370, 200, 50, 20).AddTags(Tags.StaticBlock);
            this.World.CreateRectangle(0, 300, 400, 20).AddTags(Tags.StaticBlock);
            this.World.CreateRectangle(380, 320, 20, 80).AddTags(Tags.StaticBlock);

            this.World.CreateRectangle(380, 400, 300, 20).AddTags(Tags.StaticBlock);

            Vector2f[] points2 = new Vector2f[] {
                new Vector2f(380,50),
                new Vector2f(450, 50)
                };
            this.World.CreatePolyLine(points2).AddTags(Tags.PolyLine);

            Vector2f[] points3 = new Vector2f[] {
                new Vector2f(380,100),
                new Vector2f(450, 100)
                };
            this.World.CreatePolyLine(points3).AddTags(Tags.PolyLine);

            Vector2f[] points4 = new Vector2f[] {
                new Vector2f(380,150),
                new Vector2f(450, 150)
                };
            this.World.CreatePolyLine(points4).AddTags(Tags.PolyLine);

            this.World.CreateRectangle(420, 200, 200, 20).AddTags(Tags.StaticBlock);
            this.World.CreateRectangle(680, 220, 20, 200).AddTags(Tags.StaticBlock);
            this.World.CreateRectangle(680, 200, 200, 20).AddTags(Tags.StaticBlock);

            this.World.CreateRectangle(520, 100, 200, 20).AddTags(Tags.JumpThroughBlock);

            this.World.CreateRectangle(400, 300, 280, 100).AddTags(Tags.Trigger);

            this.World.CreateRectangle(0, 780, 1200, 20).AddTags(Tags.StaticBlock);
            this.World.CreateRectangle(1180, 0, 20, 780).AddTags(Tags.StaticBlock);

            this.World.CreateRectangle(1100, 100, 20, 580).AddTags(Tags.StaticBlock);
            this.World.CreateRectangle(900, 80, 220, 20).AddTags(Tags.StaticBlock);

            this.World.CreateRectangle(900, 280, 200, 20).AddTags(Tags.StaticBlock);

            Vector2f[] points5 = new Vector2f[] {
                new Vector2f(900,280),
                new Vector2f(920, 260),
                new Vector2f(940, 250),
                new Vector2f(945, 240),
                new Vector2f(1000, 230),
                new Vector2f(1050, 230),
                new Vector2f(1080, 210),
                new Vector2f(1100, 200)
                };
            this.World.CreatePolyLine(points5).AddTags(Tags.PolyLine);

            this.World.CreateRectangle(700, 400, 200, 20).AddTags(Tags.StaticBlock);

            


            Vector2f[] points6 = new Vector2f[] {
                new Vector2f(900,400),
                new Vector2f(1000, 450)
                };
            this.World.CreatePolyLine(points6).AddTags(Tags.PolyLine);
        }

		public override void Initialize()
		{
            var rect = new Rectangle(0, 0, 1200, 800);
			this.World = new World(rect);

			this.SpawnPlayer();
            CreateLevel1();
            //CreateLevel1();

        }

		private void SpawnPlayer()
		{
			if(this.player1 != null)
				this.World.Remove(this.player1);

            this.player1 = (MoveableBody)this.World.CreateMoveableBody(400, 75, 20, 48).AddTags(Tags.Player);
            this.player1.Velocity = Vector2f.Zero;

            player1.onCollisionResponse += HandleCollision;
		}

		public override void Update(GameTime time)
		{
			var delta = (float)time.ElapsedGameTime.TotalMilliseconds;
			UpdatePlayer(this.player1, delta, Keys.Left, Keys.Up, Keys.Right, Keys.Down);

            
            if ((movingBox.X < 50 && movingBox.Velocity.X < 0) || (movingBox.X > 300 && movingBox.Velocity.X > 0))
            {
                movingBox.Velocity.X *= -1;
            }

            movingBox.Move(movingBox.X + movingBox.Velocity.X * delta, movingBox.Y,delta);

        }

		//private Vector2 velocity = Vector2.Zero;
		private KeyboardState state;
        
		private void UpdatePlayer(MoveableBody player, float delta, Keys left, Keys up, Keys right, Keys down)
		{

            player.Velocity.X = 0;

			var k = Keyboard.GetState();
			if (k.IsKeyDown(right))
                player.Velocity.X += 0.25f;
			if (k.IsKeyDown(left))
                player.Velocity.X -= 0.25f;
            if (state.IsKeyUp(up) && k.IsKeyDown(up) && JumpCnt++ < MaxJump)
            {
                player.MountedBody = null;
                player.Velocity.Y -= 0.5f;
            }

            if (state.IsKeyUp(Keys.S) && k.IsKeyDown(Keys.S))
            {
                SpawnPlayer();
            }



            var move = (player).Move(player.X + delta * player.Velocity.X, player.Y + delta * player.Velocity.Y,delta);

            // Testing if on ground
            if (move.Hits.Any((c) => c.Box.HasTag(Tags.PolyLine, Tags.StaticBlock, Tags.JumpThroughBlock, Tags.MoveableBlock) && (c.Normal.Y < 0)))
            {
                if (move.Hits.Count() > 1)
                    JumpCnt = 0;
                var mounted = move.Hits.Where(elem => elem.Normal.Y < 0);
                if (mounted.Any())
                {
                    player.MountedBody = mounted.First().Box;
                }
                //player.
                JumpCnt = 0;
            }
            else if (move.Hits.Any((c) => c.Box.HasTag(Tags.PolyLine, Tags.StaticBlock, Tags.JumpThroughBlock, Tags.MoveableBlock) && (c.Normal.Y > 0)))
            {
                //velocity.Y = 0;
            }
            else
                player.MountedBody = null;

            Console.WriteLine(player.MountedBody);

            player.Velocity.Y += delta * 0.001f;

            
                 

            Console.WriteLine(player.Velocity.Y);
            player.Data = player.Velocity;

			state = k;
		}

        public CollisionResponses HandleCollision(ICollision collision)
        {
            if (collision.Other.HasTag(Tags.Trigger))
            {
                return CollisionResponses.Cross;
            }
            else if (collision.Other.HasTag(Tags.JumpThroughBlock) && collision.Hit.Normal.Y > 0)
            {
                return CollisionResponses.Cross;
            }
            else if (collision.Other.HasTag(Tags.JumpThroughBlock) && collision.Hit.Normal.Y < 0)
            {
                if (player1.Velocity.Y < 0)
                {
                    return CollisionResponses.Cross;
                }
                else {
                    player1.Velocity.Y = 0;
                    return CollisionResponses.Slide;
                }
            }
            else if (collision.Hit.Normal.Y < 0)
            {
                player1.Velocity.Y = 0;
                return CollisionResponses.Slide;
            }
            else if (collision.Hit.Normal.Y > 0)
            {
                //velocity.Y = delta * 0.001f;
                if (collision.Hit.Box.HasTag(Tags.JumpThroughBlock))
                {
                    return CollisionResponses.Cross;
                }
                else
                {
                    player1.Velocity.Y = 0;
                    return CollisionResponses.Touch;
                }
            }

            else if (Math.Abs(collision.Hit.Normal.X) >= 1)
            {
                //velocity.Y = delta * 0.001f;
                JumpCnt = 0;
                return CollisionResponses.Slide;
            }

            return CollisionResponses.Slide;
        }
    }

    
}


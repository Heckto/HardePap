using System;
using Humper;
using Game1.CollisionDetection;
using Game1.CollisionDetection.Responses;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using AuxLib;
using System.Diagnostics;

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

    public class PlatformerScene : WorldScene
	{
        

        private const int MaxJump = 2;
        private int JumpCnt = 0;
		public PlatformerScene()
		{
		}

		private MoveableBody player1;
        

		private Vector2f platformVelocity = Vector2f.UnitX * 0.05f;

		public override void Initialize()
		{
			this.World = new World(1200, 800);

			this.SpawnPlayer();

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
            this.World.CreateRectangle(1180,0, 20, 780).AddTags(Tags.StaticBlock);

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
                new Vector2f(950, 450)
                };
            this.World.CreatePolyLine(points6).AddTags(Tags.PolyLine);

        }

		private void SpawnPlayer()
		{
			if(this.player1 != null)
				this.World.Remove(this.player1);

            //this.player1 = (MoveableBody)this.World.CreateMoveableBody(400, 75, 10, 24).AddTags(Tags.Player);
            this.player1 = (MoveableBody)this.World.CreateMoveableBody(400, 75, 10, 48).AddTags(Tags.Player);
            this.velocity = Vector2.Zero;
		}

		public override void Update(GameTime time)
		{
			var delta = (float)time.ElapsedGameTime.TotalMilliseconds;
			UpdatePlayer(this.player1, delta, Keys.Left, Keys.Up, Keys.Right, Keys.Down);
		}

		private Vector2 velocity = Vector2.Zero;
		private KeyboardState state;
        
		private void UpdatePlayer(MoveableBody player, float delta, Keys left, Keys up, Keys right, Keys down)
		{            

			velocity.X = 0;

			var k = Keyboard.GetState();
			if (k.IsKeyDown(right))
				velocity.X += 0.1f;
			if (k.IsKeyDown(left))
				velocity.X -= 0.1f;
            if (state.IsKeyUp(up) && k.IsKeyDown(up) && JumpCnt++ < MaxJump)
            {
                player.Grounded = false;
                velocity.Y -= 0.5f;
            }

            if (state.IsKeyUp(Keys.S) && k.IsKeyDown(Keys.S))
            {
                SpawnPlayer();
            }


                //if (onPlatform)
                //	velocity += platformVelocity;

                //if (timeInRed > 0)
                //	velocity.Y *= 0.75f;

                // Moving player
                var move = (player).Move(player.X + delta * velocity.X, player.Y + delta * velocity.Y, (collision) =>
			{
				if (collision.Other.HasTag(Tags.Trigger))
				{
					return CollisionResponses.Cross;
				}
                if (collision.Other.HasTag(Tags.JumpThroughBlock) && collision.Hit.Normal.Y > 0)
                {
                    return CollisionResponses.Cross;
                }
                if (collision.Other.HasTag(Tags.JumpThroughBlock) && collision.Hit.Normal.Y < 0)
                {
                    return CollisionResponses.Slide;
                }
                if (collision.Hit.Normal.Y < 0)
                {
                    return CollisionResponses.Slide;
                }
                if (collision.Hit.Normal.Y > 0)
                {
                    velocity.Y = delta * 0.001f;
                    return CollisionResponses.Touch;
                }

                if (Math.Abs(collision.Hit.Normal.X) >= 1)
                {
                    velocity.Y = delta * 0.001f;
                    JumpCnt = 0;
                    return CollisionResponses.Slide;
                }

                return CollisionResponses.Slide;
			});

            // Testing if on ground
            if (move.Hits.Any((c) => c.Box.HasTag(Tags.PolyLine,Tags.StaticBlock, Tags.JumpThroughBlock) && (c.Normal.Y < 0)))
			{
                player.Grounded = true;
                velocity.Y = delta * 0.001f;
                JumpCnt = 0;
            }
            else
                velocity.Y += delta * 0.001f;

			player.Data = velocity;

			state = k;
		}
	}
}


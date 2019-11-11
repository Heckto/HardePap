using AuxLib.CollisionDetection.Responses;
using AuxLib;

namespace AuxLib.CollisionDetection
{


	public class SlideResponse : ICollisionResponse
	{
		public SlideResponse(ICollision collision)
		{
			var velocity = (collision.Goal.Center - collision.Origin.Center);
			var normal = collision.Hit.Normal;
			var dot = collision.Hit.Remaining * (velocity.X * normal.Y + velocity.Y * normal.X);
			var slide = new Vector2f(normal.Y, normal.X) * dot;

			this.Destination = new RectangleF(collision.Hit.Position + slide, collision.Goal.Size);
		}

		public RectangleF Destination { get; private set; }
	}
}


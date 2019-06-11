using Game1.CollisionDetection.Base;

namespace Game1.CollisionDetection.Responses
{
	

	public class TouchResponse : ICollisionResponse
	{
		public TouchResponse(ICollision collision)
		{
			this.Destination = new RectangleF(collision.Hit.Position, collision.Goal.Size);
		}

		public RectangleF Destination { get; private set; }
	}
}


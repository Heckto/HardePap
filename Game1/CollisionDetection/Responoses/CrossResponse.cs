using Game1.CollisionDetection.Base;

namespace Game1.CollisionDetection.Responses
{
	

	public class CrossResponse : ICollisionResponse
	{
		public CrossResponse(ICollision collision)
		{
			this.Destination = collision.Goal;
		}

		public RectangleF Destination { get; private set; }
	}
}


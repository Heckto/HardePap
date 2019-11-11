using AuxLib;

namespace AuxLib.CollisionDetection.Responses
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


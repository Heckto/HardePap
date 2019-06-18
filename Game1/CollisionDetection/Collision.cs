using Game1.CollisionDetection.Base;

namespace Game1.CollisionDetection
{
    public class Collision : ICollision
    {
        public Collision()
        {
        }

        public IBox Box { get; set; }

        public IShape Other { get { return this.Hit?.Box; } }

        public RectangleF Origin { get; set; }

        public RectangleF Goal { get; set; }

        public IHit Hit { get; set; }

        public bool HasCollided => this.Hit != null;
    }
}


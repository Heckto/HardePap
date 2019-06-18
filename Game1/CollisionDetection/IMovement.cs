using System.Collections.Generic;
using Game1.CollisionDetection.Base;
namespace Game1.CollisionDetection
{
        public interface IMovement
    {
        IEnumerable<IHit> Hits { get; }

        bool HasCollided { get; }

        RectangleF Origin { get; }

        RectangleF Goal { get; }

        RectangleF Destination { get; }
    }
}


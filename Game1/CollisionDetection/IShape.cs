using Game1.CollisionDetection.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.CollisionDetection
{
    public interface IShape
    {
        /// <summary>
        /// Gets or sets custom user data attached to this box.
        /// </summary>
        /// <value>The data.</value>
        object Data { get; set; }


        #region Tags

        IHit Resolve(RectangleF origin, RectangleF destination);
        IHit Resolve(Vector2 origin, Vector2 destination);
        //Hit Resolve(RectangleF origin, RectangleF destination, RectangleF other);
        //Hit Resolve(Vector2 origin, Vector2 destination, RectangleF other);
        IHit Resolve(Vector2 point);

        /// <summary>
        /// Adds the tags to the box.
        /// </summary>
        /// <returns>The tags.</returns>
        /// <param name="newTags">New tags.</param>
        IShape AddTags(params Enum[] newTags);

        /// <summary>
        /// Removes the tags from the box.
        /// </summary>
        /// <returns>The tags.</returns>
        /// <param name="newTags">New tags.</param>
        IShape RemoveTags(params Enum[] newTags);

        /// <summary>
        /// Indicates whether the box has at least one of the given tags.
        /// </summary>
        /// <returns><c>true</c>, if tag was hased, <c>false</c> otherwise.</returns>
        /// <param name="values">Values.</param>
        bool HasTag(params Enum[] values);

        /// <summary>
        /// Indicates whether the box has all of the given tags.
        /// </summary>
        /// <returns><c>true</c>, if tags was hased, <c>false</c> otherwise.</returns>
        /// <param name="values">Values.</param>
        bool HasTags(params Enum[] values);

        #endregion
    }
}

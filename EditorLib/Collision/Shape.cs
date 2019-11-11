using System;
using System.Collections.Generic;
using System.Linq;
using AuxLib;
using System.Text;
using System.Threading.Tasks;

namespace AuxLib.CollisionDetection
{
    public abstract class Shape : IShape
    {
        protected World world;
        public object Data { get; set; }

        
        #region Tags

        private Enum tags;

        public IShape AddTags(params Enum[] newTags)
        {
            foreach (var tag in newTags)
            {
                this.AddTag(tag);
            }

            return this;
        }

        public IShape RemoveTags(params Enum[] newTags)
        {
            foreach (var tag in newTags)
            {
                this.RemoveTag(tag);
            }

            return this;
        }

        private void AddTag(Enum tag)
        {
            if (tags == null)
            {
                tags = tag;
            }
            else
            {
                var t = this.tags.GetType();
                var ut = Enum.GetUnderlyingType(t);

                if (ut != typeof(ulong))
                    this.tags = (Enum)Enum.ToObject(t, Convert.ToInt64(this.tags) | Convert.ToInt64(tag));
                else
                    this.tags = (Enum)Enum.ToObject(t, Convert.ToUInt64(this.tags) | Convert.ToUInt64(tag));
            }
        }

        private void RemoveTag(Enum tag)
        {
            if (tags != null)
            {
                var t = this.tags.GetType();
                var ut = Enum.GetUnderlyingType(t);

                if (ut != typeof(ulong))
                    this.tags = (Enum)Enum.ToObject(t, Convert.ToInt64(this.tags) & ~Convert.ToInt64(tag));
                else
                    this.tags = (Enum)Enum.ToObject(t, Convert.ToUInt64(this.tags) & ~Convert.ToUInt64(tag));
            }
        }

        public bool HasTag(params Enum[] values)
        {
            return (tags != null) && values.Any((value) => this.tags.HasFlag(value));
        }

        public bool HasTags(params Enum[] values)
        {
            return (tags != null) && values.All((value) => this.tags.HasFlag(value));
        }

        public abstract IHit Resolve(RectangleF origin, RectangleF destination, MoveableBody box);
        public abstract IHit Resolve(Vector2f origin, Vector2f destination);
        public abstract IHit Resolve(Vector2f point);



        #endregion
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.Levels;

namespace Game1.Levels
{
    public interface IUpdateableItem
    {
        void Update(GameTime gameTime, Level lvl);
    }
}

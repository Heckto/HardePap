using Game1.DataContext;
using Game1.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Controllers
{
    public abstract class CharacterController
    {
        protected readonly LivingSpriteObject controlledObject;
        protected readonly GameContext gameContext;


        public CharacterController(LivingSpriteObject obj, GameContext context)
        {
            controlledObject = obj;
            gameContext = context;
             
        }

        public abstract void DoInput();
    }
}

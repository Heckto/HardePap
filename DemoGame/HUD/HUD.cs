using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.HUD
{
   

    public class HeadsUpDisplay
    {
        public bool Enabled { get; set; }
        public bool Visible { get; set; }

        public Dictionary<string,HUDComponent> componentList = new Dictionary<string,HUDComponent>();

        public HeadsUpDisplay()
        {
            Visible = true;
            Enabled = true;
        }

        public void Draw(SpriteBatch spriteBatch,GameTime gameTime)
        {
            if (Visible)
            {
                foreach (var component in componentList.Values)
                    component.Draw(spriteBatch,gameTime);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                foreach (var component in componentList.Values)
                    component.Update(gameTime);
            }

            var toRemove = componentList.Where(pair => !pair.Value.IsAlive)
                         .Select(pair => pair.Key)
                         .ToList();

            foreach (var key in toRemove)
            {
                componentList.Remove(key);
            }
        }

        public void AddHUDComponent(string componentName,HUDComponent component)
        {
            componentList.Add(componentName,component);
        }

        public void RemoveHUDComponent(string componentName, HUDComponent component)
        {
            if (componentList.ContainsKey(componentName))
                componentList.Remove(componentName);
        }
    }
}

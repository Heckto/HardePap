using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuxLib.Debug
{
    public class DebugMonitor
    {
        private const int itemSize = 20;
        private Dictionary<string, object> debugData;
        private SpriteFont font;
        public DebugMonitor()
        {
            debugData = new Dictionary<string, object>();
        }

        public void Initialize(SpriteFont font)
        {
            this.font = font;
        }

        public void AddDebugValue(string name,object value)
        {
            debugData[name] = value;
        }

        public void RemoveDebugValue(string key)
        {
            if (debugData.ContainsKey(key))
                debugData.Remove(key);
        }

        public void Clear()
        {
            debugData.Clear();
        }
       

        public void Draw(SpriteBatch spriteBatch)
        {
            var itemCnt = debugData.Count();
            if (itemCnt > 0)
            {
                var strings = new List<string>();
                var maxLength = 0f;
                var maxHeight = 0f;
                foreach (var item in debugData)
                {
                    
                    var itemText = String.Empty;
                    
                    itemText = $"{item.Key} : { item.Value}";
                    strings.Add(itemText);
                    
                    var textSize = font.MeasureString(itemText);

                    maxLength = Math.Max(maxLength, textSize.X);
                    maxHeight = Math.Max(maxHeight, textSize.Y);
                }

                var vectSize = itemCnt * maxHeight + 3f * maxHeight;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Primitives.Instance.drawBoxFilled(spriteBatch,new Rectangle(50, 50, 400, (int)vectSize), Color.Black);
                var vertIdx = 70;
                var idx = 0;
                foreach (var item in strings)
                {
                    spriteBatch.DrawString(font, item, new Vector2(60, idx++ * itemSize + vertIdx), Color.White);
                }
                spriteBatch.End();
            }

        }
    }
}

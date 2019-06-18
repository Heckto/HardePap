using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Game1.Helpers
{
    public class DebugMonitor
    {
        private const int itemSize = 20;
        private Dictionary<string, MonitorItem> debugMonitors;

        public DebugMonitor()
        {
            debugMonitors = new Dictionary<string, MonitorItem>();
        }

        public void AddDebugValue(object sender,string key,string alias = "")
        {
            var item = new MonitorItem()
            {
                t = sender.GetType(),
                DebugObject = sender,
                Alias = alias
            };
            debugMonitors[key] = item;
        }

        public void RemoveDebugValue(string key)
        {
            if (debugMonitors.ContainsKey(key))
                debugMonitors.Remove(key);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var item in debugMonitors)
            {                
                var f = item.Value.t.GetField(item.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                item.Value.DebugValue = f.GetValue(item.Value.DebugObject);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {            
            var itemCnt = debugMonitors.Count();
            if (itemCnt > 0)
            {
                var strings = new List<string>();
                var maxLength = 0f;
                var maxHeight = 0f;
                foreach (var item in debugMonitors)
                {
                    var itemText = String.Empty;
                    if (!String.IsNullOrEmpty(item.Value.Alias))
                        itemText = $"{item.Value.Alias} : { item.Value.DebugValue}";
                    else
                        itemText = $"{item.Key} : { item.Value.DebugValue}";
                    strings.Add(itemText);
                    var textSize = spriteBatch.MeasureString(itemText);

                    maxLength = Math.Max(maxLength, textSize.X);
                    maxHeight = Math.Max(maxHeight, textSize.Y);
                }

                var vectSize = itemCnt * maxHeight + 2f * maxHeight;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.DrawRectangle(new Rectangle(50, 50,400, (int)vectSize), Color.Black, 0.5f);
                var vertIdx = 70;
                var idx = 0;
                foreach(var item in strings)
                {
                    spriteBatch.DrawString(item, 60, idx++ * itemSize + vertIdx, Color.White, 1.0f);                    
                }
                spriteBatch.End();
            }

        }
    }

    public class MonitorItem
    {
        public Type t { get; set; }
        public object DebugObject { get; set; }
        public object DebugValue { get; set; }
        public string Alias { get; set; } = String.Empty;

    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelEditor.Items
{
    public interface IEditableItem
    {
        Item clone();

        string getNamePrefix();
        void OnTransformed();
        bool contains(Vector2 worldpos);
        void onMouseButtonDown(Vector2 mouseworldpos);

        bool CanScale();
        Vector2 getScale();
        void setScale(Vector2 scale);
        void drawInEditor(SpriteBatch sb);
        void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color);
    }
}

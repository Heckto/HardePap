using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Game1.GameObjects.Levels;
using System;

namespace Game1.GameObjects
{
    public interface IUpdateableItem
    {
        void Update(GameTime gameTime, Level lvl);
    }

    public interface IDrawableItem
    {
        void Draw(SpriteBatch sb);
    }


    public interface IEditableGameObject
    {
        GameObject clone();
        string getNamePrefix();
        void OnTransformed();
        bool contains(Vector2 worldpos);
        void onMouseButtonDown(Vector2 mouseworldpos);

        //bool CanRotate();
        //float getRotation();
        //void setRotation(float rotation);
        //bool CanScale();
        //Vector2 getScale();
        //void setScale(Vector2 scale);
        void drawInEditor(SpriteBatch sb);
        void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color);

        bool onMouseOver(Vector2 mouseworldpos, out string msg);
        void onMouseOut();
        void onMouseButtonUp(Vector2 mouseworldpos);
        //void setPosition(Vector2 pos);
    }

    public interface IUndoable
    {
        IUndoable cloneforundo();
        void makelike(IUndoable other);

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditableAttribute : Attribute
    {
        string cat { get; set; } = "DEFAULT";

        public EditableAttribute(string _cat)
        {
            this.cat = _cat;
        }
    }
}

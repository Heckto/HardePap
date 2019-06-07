using System;
namespace LevelEditor
{
    public interface IUndoable
    {

        IUndoable cloneforundo();

        void makelike(IUndoable other);
        
    }
}

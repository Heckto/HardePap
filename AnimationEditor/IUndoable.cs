using System;
namespace AnimationEditor
{
    public interface IUndoable
    {

        IUndoable cloneforundo();

        void makelike(IUndoable other);
        
    }
}

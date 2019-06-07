using System;
using System.Collections.Generic;

namespace LevelEditor
{
    public enum CommandType
    {
        Transform, Add, Delete, NameChange, OrderChange, WholeLevel
    }

    public class Command
    {
        public String Description;
        public CommandType ComType;
        public List<IUndoable> ObjectsBefore = new List<IUndoable>();
        public List<IUndoable> ObjectsAfter = new List<IUndoable>();

        public Command(string description)
        {
            ComType = CommandType.WholeLevel;
            Description = description;
            ObjectsBefore.Add(MainForm.Instance.picturebox.level.cloneforundo());
        }

        public List<IUndoable> Undo()
        {
            switch (ComType)
            {
                case CommandType.WholeLevel:
                    MainForm.Instance.picturebox.level = (Level)ObjectsBefore[0];
                    MainForm.Instance.picturebox.getSelectionFromLevel();
                    MainForm.Instance.picturebox.updatetreeview();
                    break;
            }
            return null;
        }

        public List<IUndoable> Redo()
        {
            switch (ComType)
            {
                case CommandType.WholeLevel:
                    MainForm.Instance.picturebox.level = (Level)ObjectsAfter[0];
                    MainForm.Instance.picturebox.getSelectionFromLevel();
                    MainForm.Instance.picturebox.updatetreeview();
                    break;
            }
            return null;
        }

        public void saveAfterState()
        {
            ObjectsAfter.Add(MainForm.Instance.picturebox.level.cloneforundo());
        }

    }
}

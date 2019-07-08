using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationEditor
{
    public class KeyFrame
    {
        public Frame frame;
        public int Duration;

        public string Name { get { return frame.Name; } }


        public KeyFrame(Frame f)
        {
            frame = f;
            Duration = 0;
        }

        public KeyFrame() { }
    }
}

using Microsoft.Xna.Framework;
using System.Threading;

namespace Game1.Scripting
{
    // Bitch you know my name .... now gimme my money ...
    public class FrameNotifyer
    {
        public GameTime gameTime { get; set; }
        public CancellationTokenSource token { get; set; }
    }
}

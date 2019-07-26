using AuxLib.ScreenManagement.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxLib.ScreenManagement
{
    public interface IGameState
    {
        GameState Value { get; }
    //    void LoadContent();
    }

    public interface IGameStateManager
    {
        event EventHandler OnStateChange;
        GameState State { get; }
        void PopState();
        void PushState(GameState state);
        void PushState(GameState newState, Transition transition);
        bool ContainsState(GameState state);
        void ChangeState(GameState newState);
        void ChangeState(GameState newState, Transition transition);
    }    
}

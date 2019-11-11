using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using AuxLib.ScreenManagement.Transitions;

namespace AuxLib.ScreenManagement
{
    public class GameStateManager : DrawableGameComponent, IGameStateManager
    {
        private Stack<GameState> states = new Stack<GameState>();
        public event EventHandler OnStateChange;
        private int initialDrawOrder = 1000;
        private int drawOrder;
        public GameStateManager(Game game) : base(game) { }
        private Transition activeTransition;

        public void PopState()
        {
            RemoveState();
            drawOrder -= 100;
            //Let everyone know we just changed states
            OnStateChange?.Invoke(this, null);
        }
        private void RemoveState()
        {
            var oldState = states.Peek();
            //Unregister the event for this state
            OnStateChange -= oldState.StateChanged;
            //remove the state from our game components
            //Game.Components.Remove(oldState.Value);
            states.Pop();
        }

        public void PushState(GameState newState)
        {
            drawOrder += 100;
            newState.DrawOrder = drawOrder;            
            AddState(newState);
            //Let everyone know we just changed states
            OnStateChange?.Invoke(this, null);
        }

        public void PushState(GameState newState,Transition transition)
        {
            drawOrder += 100;
            newState.DrawOrder = drawOrder;

            if (activeTransition != null)
                return;

            activeTransition = transition;
            activeTransition.StateChanged += (sender, args) =>
            {

                ChangeState(newState);
                //Let everyone know we just changed states
                OnStateChange?.Invoke(this, null);
            };
            activeTransition.Completed += (sender, args) =>
            {
                activeTransition.Dispose();
                activeTransition = null;
            };
        }

        private void AddState(GameState state)
        {
            state.Initialize();
            states.Push(state);
           
            //Register the event for this state
            OnStateChange += state.StateChanged;
        }

        public void ChangeState(GameState newState)
        {
            while (states.Count > 0)
                RemoveState();
            newState.DrawOrder = drawOrder = initialDrawOrder;
            AddState(newState);
            OnStateChange?.Invoke(this, null);
        }

        public void ChangeState(GameState newState, Transition transition)
        {
            while (states.Count > 0)
                RemoveState();
            newState.DrawOrder = drawOrder = initialDrawOrder;
            activeTransition = transition;
            activeTransition.StateChanged += (sender, args) =>
            {
                AddState(newState);
                //Let everyone know we just changed states
                OnStateChange?.Invoke(this, null);
            };
            activeTransition.Completed += (sender, args) =>
            {
                activeTransition.Dispose();
                activeTransition = null;
            };
            
        }

        public bool ContainsState(GameState state)
        {
            return (states.Contains(state));
        }

        public GameState State
        {
            get { return (states.Peek()); }
        }

        public override void Draw(GameTime gameTime)
        {
            
            var statesList = states.ToArray();
            for(var i=statesList.Length-1;i >= 0; i--)
            {

                statesList[i].Draw(gameTime);
                if (statesList[i].BlockDrawing)
                    break;
            }
            activeTransition?.Draw(gameTime);
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            activeTransition?.Update(gameTime);
        
            var statesList = states.ToArray();
            foreach (var state in statesList)
            {
                state.Update(gameTime);
                if (state.BlockUpdating)
                    break;
            }
            base.Update(gameTime);
        }       


    }

}

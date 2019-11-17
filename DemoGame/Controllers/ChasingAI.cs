using AuxLib.Input;
using Game1.Controllers;
using Game1.DataContext;
using Game1.Sprite;
using Game1.Sprite.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Controllers
{
    //public class ChasingAI : CharacterController
    //{

    //    LivingSpriteObject obj;
    //    GameContext context;
    //    public ChasingAI(GameContext context, Zombie1 entity)
    //    {
    //        this.context = context;
    //        obj = entity;
    //    }

    //    public void HandleInput(GameContext context, Zombie1 entity)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    //public void HandleKeyInput(float delta, IInputHandler Input)
    //    //{            
    //    //    if (CurrentState == CharState.GroundAttack && entity.CurrentAnimation.AnimationName == "Attack")
    //    //    {
    //    //        if (CurrentAnimation.AnimationState != AnimationState.Running)
    //    //            CurrentState = CharState.Grounded;
    //    //        return;
    //    //    }

    //    //    var trajectoryX = Trajectory.X;
    //    //    var trajectoryY = Trajectory.Y;
    //    //    if (CurrentState == CharState.Grounded && Math.Abs(player1.Position.X - Position.X) < 150)
    //    //    {
    //    //        Trajectory = new Vector2(0f, Trajectory.Y);
    //    //        CurrentState = CharState.GroundAttack;
    //    //        return;
    //    //    }

    //    //    if (player1.Position.X < Position.X)
    //    //    {
    //    //        if (Trajectory.X > 0)
    //    //            trajectoryX = 0;
    //    //        else
    //    //            trajectoryX = acc * friction * delta;

    //    //        Direction = FaceDirection.Left;
    //    //    }
    //    //    else if (player1.Position.X > Position.X)
    //    //    {
    //    //        if (Trajectory.X < 0)
    //    //            trajectoryX = 0;
    //    //        else
    //    //            trajectoryX = -acc * friction * delta;

    //    //        Direction = FaceDirection.Right;
    //    //    }
    //    //    else if (Trajectory.X != 0)
    //    //    {
    //    //        trajectoryX = 0;
    //    //    }

    //    //    if (Math.Abs(player1.Position.Y - Position.Y) > 300)
    //    //    {
    //    //        if (CurrentState == CharState.Grounded)
    //    //        {
    //    //            trajectoryY = -jumpForce;
    //    //            CurrentState = CharState.Air;
    //    //            CollisionBox.MountedBody = null;

    //    //        }
    //    //    }

    //    //    Trajectory = new Vector2(trajectoryX, trajectoryY);
    //    //}
    //}
}

using AuxLib.Input;
using Game1.DataContext;
using Game1.Sprite;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Controllers
{

    //public class ManuelCharacterController : CharacterController
    //{
    //    private readonly IInputHandler Input;

    //    public ManuelCharacterController(LivingSpriteObject obj, GameContext context) : base(obj, context)
    //    {
    //        Input = context.input;

    //    }

    //    public override void DoInput()
    //    {
    //        var keyLeft = Input.IsPressed(0, Buttons.LeftThumbstickLeft, Keys.Left);
    //        var keyRight = Input.IsPressed(0, Buttons.LeftThumbstickRight, Keys.Right);

    //        var isKeyJump = Input.IsPressed(0, Buttons.A, Keys.Space);
    //        var wasKeyJump = Input.WasPressed(0, Buttons.A, Keys.Space);

    //        var isKeyAttack = Input.IsPressed(0, Buttons.X, Keys.LeftControl);
    //        var isKeyThrow = Input.WasPressed(0, Buttons.Y, Keys.LeftShift);

    //        //if ((CurrentState == CharState.GroundAttack && CurrentAnimation.AnimationName == "Attack") ||
    //        //    (CurrentState == CharState.GroundThrow && CurrentAnimation.AnimationName == "Throw"))
    //        //{
    //        //    if (CurrentAnimation.AnimationState == AnimationState.Running)
    //        //        return;
    //        //    else
    //        //        CurrentState = CharState.Grounded;
    //        //}
    //        //else if ((CurrentState == CharState.JumpAttack && CurrentAnimation.AnimationName == "JumpAttack") ||
    //        //    (CurrentState == CharState.JumpThrow && CurrentAnimation.AnimationName == "JumpThrow"))
    //        //{
    //        //    if (CurrentAnimation.AnimationState == AnimationState.Finished)
    //        //        CurrentState = CharState.Air;
    //        //}


            
    //        var trajectoryX = controlledObject.Trajectory.X;
    //        var trajectoryY = controlledObject.Trajectory.Y;
    //        if (keyLeft)
    //        {
    //            if (controlledObject.Trajectory.X > 0)
    //                trajectoryX = 0;
    //            else
    //                trajectoryX = acc * friction * delta;

    //            if (CurrentState != CharState.JumpAttack && CurrentState != CharState.JumpThrow)
    //                Direction = FaceDirection.Left;
    //        }
    //        else if (keyRight)
    //        {
    //            if (controlledObject.Trajectory.X < 0)
    //                trajectoryX = 0;
    //            else
    //                trajectoryX = -acc * friction * delta;

    //            if (CurrentState != CharState.JumpAttack && CurrentState != CharState.JumpThrow)
    //                Direction = FaceDirection.Right;
    //        }
    //        else if (controlledObject.Trajectory.X != 0)
    //        {
    //            trajectoryX = 0;
    //        }

    //        if (wasKeyJump)
    //        {
    //            if (CurrentState == CharState.Grounded)
    //            {
    //                trajectoryY = -jumpForce;
    //                CurrentState = CharState.Air;
    //                CollisionBox.MountedBody = null;
    //                JumpCnt++;
    //            }
    //            else if (CurrentState == CharState.Air && JumpCnt > 0 && JumpCnt < MaxJumpCount)
    //            {
    //                if (Trajectory.Y < 0)
    //                    trajectoryY = -1 * jumpForce;
    //                else
    //                    trajectoryY -= jumpForce;
    //                JumpCnt++;
    //            }
    //        }
    //        else if (isKeyJump)
    //        {
    //            if (CurrentState == CharState.Air && Trajectory.Y > 0)
    //            {
    //                CurrentState = CharState.Glide;
    //            }
    //        }

    //        if (CurrentState == CharState.Air || CurrentState == CharState.Glide)
    //        {
    //            if (isKeyAttack)
    //            {
    //                CurrentState = CharState.JumpAttack;
    //            }
    //            else if (isKeyThrow)
    //            {
    //                var location = new Vector2(Position.X, Position.Y + (trajectoryY * 50)); // Adding something since the kunai spawns before the animation
    //                thrownObjects.Add(new Obstacles.Kunai(location, Direction, context)); // null shouldnt be a problem here since the texture should be loaded already
    //                CurrentState = CharState.JumpThrow;
    //            }
    //            else
    //            {
    //                if (CurrentState == CharState.Glide && !isKeyJump)
    //                    CurrentState = CharState.Air;

    //                var multiplier = CurrentState == CharState.Air ? 1f : 0.00001f;

    //                trajectoryY += delta * gravity * multiplier;
    //            }
    //        }
    //        else if (CurrentState == CharState.Grounded)
    //        {
    //            if (isKeyAttack)
    //            {
    //                trajectoryX = 0;
    //                CurrentState = CharState.GroundAttack;
    //            }
    //            else if (isKeyThrow)
    //            {
    //                trajectoryX = 0;
    //                var location = new Vector2(Position.X, Position.Y);
    //                thrownObjects.Add(new Obstacles.Kunai(location, Direction, context)); // null shouldnt be a problem here since the texture should be loaded already
    //                CurrentState = CharState.GroundThrow;
    //            }
    //            else if (Math.Abs(Trajectory.Y) > 0.5)
    //            {
    //                CurrentState = CharState.Air;
    //            }
    //        }

    //        Trajectory = new Vector2(trajectoryX, trajectoryY);
    //    }
    //}
}

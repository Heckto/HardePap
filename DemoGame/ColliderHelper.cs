using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace Game1
{
    public class ColliderTriggerHelper
    {
        Body body;
        World world;
        Category triggerMask;

        /// <summary>
        /// stores all the active intersection pairs that occured in the current frame
        /// </summary>
        HashSet<Fixture> _activeTriggerIntersections = new HashSet<Fixture>();

        /// <summary>
        /// stores the previous frames intersection pairs so that we can detect exits after moving this frame
        /// </summary>
        HashSet<Fixture> _previousTriggerIntersections = new HashSet<Fixture>();

        public delegate void OnEnterZoneDelgate(Fixture body);
        public event OnEnterZoneDelgate onEnterZone;

        public delegate void OnExitZoneDelgate(Fixture body);
        public event OnExitZoneDelgate onExitZone;

        public ColliderTriggerHelper(Body entity, Category _triggerMask)
        {
            body = entity;
            world = entity.World;
            triggerMask = _triggerMask;
        }


        /// <summary>
        /// update should be called AFTER Entity is moved. It will take care of any ITriggerListeners that the Collider overlaps.
        /// </summary>
        public void Update()
        {
            var t = body.GetTransform();
            body.FixtureList[0].Shape.ComputeAABB(out var aabb, ref t, 0);

            world.QueryAABB((f) =>
            {
                if ((f.CollisionCategories & triggerMask) == f.CollisionCategories)
                {
                    var shouldReportTriggerEvent = !_activeTriggerIntersections.Contains(f) &&
                                                       !_previousTriggerIntersections.Contains(f);
                    if (shouldReportTriggerEvent)
                        onEnterZone?.Invoke(f);

                    _activeTriggerIntersections.Add(f);
                }
                return true;
            }, ref aabb);

            CheckForExitedColliders();
        }


        void CheckForExitedColliders()
        {
            // remove all the triggers that we did interact with this frame leaving us with the ones we exited
            _previousTriggerIntersections.ExceptWith(_activeTriggerIntersections);

            foreach (var pair in _previousTriggerIntersections)
                onExitZone?.Invoke(pair);

            // clear out the previous set cause we are done with it for now
            _previousTriggerIntersections.Clear();

            // add in all the currently active triggers
            _previousTriggerIntersections.UnionWith(_activeTriggerIntersections);

            // clear out the active set in preparation for the next frame
            _activeTriggerIntersections.Clear();
        }
       
    }
}

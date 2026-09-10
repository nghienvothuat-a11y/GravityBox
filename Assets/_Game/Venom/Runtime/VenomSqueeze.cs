using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Local aperture sensing. It supplies a lane and a material response,
    /// never moves a body or changes collision shapes.</summary>
    public sealed class VenomSqueeze
    {
        public Vector3 Axis { get; private set; }
        public Vector3 Throat { get; private set; }
        public float Amount { get; private set; }
        private float probeAt, keepUntil;
        private bool threading;

        public void Step(VenomLevelController level, int group, Vector3 centre, Vector3 command, float dt)
        {
            float time = level.Organism.SimulationTime;
            bool allowOutlet=level.SplitVault!=null;
            bool moving = command.sqrMagnitude > .04f;
            if (!moving || Vector3.Dot(Axis,command.normalized) < .75f) { keepUntil = 0; threading = false; }
            if (threading)
            {
                bool tailThrough = true;
                for(int i=0;i<CohesiveOrganism.ParticleCount;i++)
                    if(!level.Organism.Escaped[i] && level.Organism.Groups[i]==group &&
                        Vector3.Dot(level.Organism.Bodies[i].position-Throat,Axis)<.04f) tailThrough=false;
                if(tailThrough) threading=false;
                else keepUntil=time+.25f;
            }
            if (moving && !threading && time >= probeAt)
            {
                probeAt = time + .12f;
                Vector3 axis = command.normalized, side = Vector3.Cross(level.NavigationUp,axis);
                float radius = level.MatterProfile.ParticleRadius + .002f;
                float best = float.PositiveInfinity;
                for (int offset = -9; offset <= 9; offset++)
                {
                    Vector3 lane = centre + side*(offset*.005f);
                    if (!level.Locomotion.Navigator.Clear(lane-axis*.025f,lane+axis*.05f,radius,allowOutlet)) continue;
                    for (int step = -2; step <= 4; step++)
                    {
                        Vector3 throat = lane + axis*(step*.0125f);
                        // Two opposing walls distinguish a slit from an ordinary
                        // single wall or the edge of the round exit.
                        if (level.NavigationFree(throat+side*.03f,radius,true) ||
                            level.NavigationFree(throat-side*.03f,radius,true)) continue;
                        if (!level.Locomotion.Navigator.Clear(throat-axis*.02f,throat+axis*.03f,radius,allowOutlet)) continue;
                        float score = Mathf.Abs(offset)*.005f + Mathf.Abs(step)*.001f;
                        if (score >= best) continue;
                        best = score; Axis = axis; Throat = throat; keepUntil = time + .25f; threading=true;
                    }
                }
            }
            Amount = Mathf.MoveTowards(Amount,moving && time < keepUntil ? 1 : 0,dt*4);
        }

        public Vector3 Velocity(VenomLevelController level, Vector3 position, Vector3 command, float speed)
        {
            Vector3 side = Vector3.Cross(level.NavigationUp,Axis);
            float error = Vector3.Dot(Throat-position,side);
            // Leave room for staggered particles. Forcing every centre onto the
            // exact same line creates a granular arch against the outer wall.
            float lateral = Mathf.Sign(error)*Mathf.Max(0,Mathf.Abs(error)-.002f);
            // A shoulder facing the solid lip slides sideways first. Continuing
            // to press forward here locks a contact arch across a narrow slit.
            bool clear = level.Locomotion.Navigator.Clear(position,position+Axis*.035f,
                level.MatterProfile.ParticleRadius-.0003f,true);
            Vector3 flow = Axis*(clear ? speed*.7f : 0) + side*Mathf.Clamp(lateral*10,-speed,speed);
            // The front slows so the tail catches up, but keeps moving to leave
            // room for incoming tissue. A fixed gathering point can clog itself.
            if (Vector3.Dot(position-Throat,Axis) > .035f)
                flow = Axis*(speed*.25f) + side*Mathf.Clamp(lateral*2,-speed,speed);
            return Vector3.Lerp(command*speed,Vector3.ClampMagnitude(flow,speed),Amount);
        }
    }
}

using System.Collections.Generic;
using GravityBox.Simulation;

namespace GravityBox.Gameplay
{
    public interface IBallMechanism
    {
        void Bind(IReadOnlyList<BallController> balls);
    }
}

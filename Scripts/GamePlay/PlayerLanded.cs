using SaveYourself.Core;
using SaveYourself.Mechanics;

namespace SaveYourself.Gameplay
{
    /// <summary>
    /// Fired when the player character lands after being airborne.
    /// </summary>
    /// <typeparam name="PlayerLanded"></typeparam>
    public class PlayerLanded : Simulation.Event<PlayerLanded>
    {
        public Player player;

        public override void Execute()
        {

        }
    }
}
using SaveYourself.Core;
using SaveYourself.Mechanics;

namespace SaveYourself.Gameplay
{
    /// <summary>
    /// Fired when the player performs a Jump.
    /// </summary>
    /// <typeparam name="PlayerJumped"></typeparam>
    public class PlayerJumped : Simulation.Event<PlayerJumped>
    {
        public Player player;

        public override void Execute()
        {

        }
    }
}
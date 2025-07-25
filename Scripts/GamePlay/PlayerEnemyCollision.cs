using SaveYourself.Core;
using SaveYourself.Mechanics;
using SaveYourself.Model;
using UnityEngine;
using static SaveYourself.Core.Simulation;

namespace SaveYourself.Gameplay
{

    /// <summary>
    /// Fired when a Player collides with an Enemy.
    /// </summary>
    /// <typeparam name="EnemyCollision"></typeparam>
    public class PlayerEnemyCollision : Simulation.Event<PlayerEnemyCollision>
    {
        public EnemyController enemy;
        public Player player;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            var willHurtEnemy = player.Bounds.center.y >= enemy.Bounds.max.y;

            if (willHurtEnemy)
            {

            }
            else
            {
                Schedule<PlayerDeath>();
            }
        }
    }
}
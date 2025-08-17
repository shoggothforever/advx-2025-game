using System.Collections.Generic;
namespace SaveYourself.Core
{
    public static partial class Simulation
    {
        /// <summary>
        /// This class provides a container for creating singletons for any other class,
        /// within the scope of the Simulation. It is typically used to hold the simulation
        /// models and configuration classes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        static class InstanceRegister<T> where T : class, new()
        {
            public static T instance = new T();
        }
        /// <summary>
        /// This class provides a container for creating singletons for any other levelLogic,
        /// within the scope of the Simulation. It is typically used to hold the simulation
        /// models and configuration LevelRegister.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        static class LevelRegister<T> where T:ILevelLogic,new()
        {
            public static T instance = new T();
        }
    }
}
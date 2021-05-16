using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InputCommands
{
    /// <summary>
    /// Base command that passes float to method 
    /// from transform to be inherited.
    /// </summary>
    public class Command
    {
        public virtual void Execute(Transform t, float f) {}
    }

    /// <summary>
    /// Turn command that passes f to turn on t.DrillCharacterController
    /// </summary>
    public class Turn : Command
    {
        public override void Execute(Transform t, float f)
        {
            t.GetComponent<DrillCharacterController>().Turn(f);
        }
    }

    /// <summary>
    /// Accelerate command that passes f to turn on t.DrillCharacterController
    /// </summary>
    public class Accelerate : Command
    {
        public override void Execute(Transform t, float f)
        {
            t.GetComponent<DrillCharacterController>().Accelerate(f);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

    bool halt = false;

    #region External Vars
    protected float mass = 20f;
    #endregion

    #region Internal Velocity Computation
    public Vector2 Velocity { get; private set;}
    public float AngMom { get; private set;}
    protected bool underGround { get; private set;}
    protected Vector2 intrinsicVelocity;
    Vector2 extrinsicVelocity;
    Vector2 displacement;
    Vector2 externalForces;
    float externalTorque;
    #endregion

    float viscosityLand;
    float gravity;
    float fixedDeltaTimeInv;

    protected new Collider2D collider;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

    const float minDisplacementDistanceSquared = 0.005f*0.005f;

    protected virtual void Awake()
    {
        fixedDeltaTimeInv = 1/Time.fixedDeltaTime;
    }
    public virtual void Reset() 
    {
        AngMom = 0f;
        externalTorque = 0f;
        underGround = transform.position.y <= 0.5 * transform.localScale.y;
        Velocity = Vector2.zero;
        intrinsicVelocity = Vector2.zero;
        extrinsicVelocity = Vector2.zero;
        displacement = Vector2.zero;
        externalForces = Vector2.zero;
        viscosityLand = GameManager.Instance.ViscosityLand;
        gravity = GameManager.Instance.Gravity;
    }

    public void Freeze() {halt = true;}

    public void Unfreeze() {halt = false;}

    public void AddForce(Vector2 force) 
    {
        if(!halt) externalForces += force;
    }

    /// <summary>Adds force such that it pushes object to additionally add that velocity</summary>
    public void AddForceRelToVel(Vector2 force) 
    {
        if(!halt) externalForces += force*fixedDeltaTimeInv;
    }
    
    /// <summary>Apply torque to object.</summary>
    public void AddTorque(float torque) 
    {
        if(!halt) externalTorque += torque*500f;
    }

    ///<Summary>Set intrinsic velocity that is not governed by physics,
    ///e.g., eratic behaviour such as instantaneous speed from input. 
    ///Called at the beginning of PhysicsObject.FixedUpdate()</Summary>
    protected virtual void CustomMovement() { }

    /// <Summary>
    /// Final velocity computation, gets intrinsic velocity from CustomMovement()
    /// and consumes whatever is stored in externalForces with friction underground
    /// and gravity above ground.
    /// </Summary>
    void FixedUpdate()
    {
        if(!halt)
        {
            CustomMovement();

            // Update current state
            underGround = transform.position.y <= 0.5 * transform.localScale.y;

            // Gravity
            if(!underGround)
                AddForce(mass*gravity*Vector2.down*Time.fixedDeltaTime);

            // External Forces
            extrinsicVelocity += externalForces * Time.fixedDeltaTime;

            // Drag
            if(underGround)
            {
                if(extrinsicVelocity.sqrMagnitude > 0.0025)
                    extrinsicVelocity -= extrinsicVelocity*Time.fixedDeltaTime*viscosityLand;
                else
                    extrinsicVelocity = Vector2.zero;
            }

            // Move
            Velocity = intrinsicVelocity + extrinsicVelocity;
            displacement = Velocity * Time.fixedDeltaTime;
            if(displacement.sqrMagnitude < minDisplacementDistanceSquared)
                displacement = Vector2.zero;
            transform.Translate(displacement, Space.World);

            // Rotate
            AngMom = externalTorque * Time.fixedDeltaTime;
            transform.Rotate(-AngMom * transform.forward * Time.fixedDeltaTime);
            
            // Reset external forces
            externalForces = Vector2.zero;
            externalTorque = 0f;
        }
    }
}
using System;
using System.Collections;
using UnityEngine;

public class DrillCharacterController : PhysicsObject
{
    // These are public to allow modification in unity.
    #region gameplayvars
    public float speed = 2f;
    public float turnSpeed = 20f;
    // Turn Radius is decreasing with velocity squared, with vel.sqrMag = upper maximally decreased and = lower minimally decreased
    public float turnUpperSpeedBoundSqr = 7f*7f;
    public float turnLowerSpeedBoundSqr = 1f;
    // Acceleration
    public float acc = 0.2f;
    // Decceleration
    public float brake = 0.1f;
    // Threshold (squared) at which one deals more damage.
    public float highSpeedThresholdSqr = 21f;
    public float maxHealth = 4f;
    #endregion

    #region internalvars
    bool debug = false;
    // inputs are set via InputController/Command Pattern
    public float input_turn { get; private set; }
    float input_acc;
    public Vector2 PositionOnSpawn {get; set;}
    public int PlayerNumber {get; set;} = -1;
    public int LivesLeft {get; set; }
    /// <summary>
    /// Indicates whether a drill is fast and should deal more damage.
    /// </summary>
    public bool highSpeed { get; private set; }
    float currentHealth;
    float timeUnderground;
    float velocitySqrMagnitude;
    Coroutine lastColliderTouchedCoroutine;
    Collider2D lastColliderTouched;
    /// <summary>
    /// Number of rigidbodies staying inside this character.
    /// </summary>
    /// Used to check if a "new" collision takes place.
    int triggerStaying;
    bool invulnerable;
    Renderer healthrenderer;
    Color healthcolor;
    ContactFilter2D contactFilter;
    RaycastHit2D firstBodypartHit; 

    Renderer[] childRenderer;
    ParticleSystem undergroundPS;
    ParticleSystem hitEffect;
    ParticleSystem drillsparksPS;
    readonly float blinkTime = 0.1f;
    readonly WaitForSeconds blinkWait = new WaitForSeconds(0.1f);
    readonly WaitForSeconds invulnTime = new WaitForSeconds(1f);
    readonly WaitForSeconds holdLastColliderTime = new WaitForSeconds(1f);
    readonly Countdown countdown = new Countdown();
    #endregion

    #region coroutines 
    /// <summary>
    /// Periodically disable renderer to make character blink.
    /// </summary>
    /// <param name="duration">Duration of the effect.</param>
    IEnumerator DoBlinks(float duration) 
    {
        while (duration > 0f) {
            for (int i = 0; i < childRenderer.Length; i++)
                childRenderer[i].enabled = !childRenderer[i].enabled;
            duration -= blinkTime;
            yield return blinkWait;
        }

        //make sure renderer is enabled when we exit
        for (int i = 0; i < childRenderer.Length; i++)
            childRenderer[i].enabled = true;
    }

    IEnumerator MakeInvulnerable()
    {
        invulnerable = true;
        yield return invulnTime;
        invulnerable = false;
    }

    /// <summary>
    /// Freezes game for 20ms on hit. For extra juicyness.
    /// </summary>
    IEnumerator FreezeTimeOnHit()
    {
        Time.timeScale = 0f;
        while(true)
        {
            float pauseEndTime = Time.realtimeSinceStartup + 0.02f;
            while (Time.realtimeSinceStartup < pauseEndTime)
            {
                yield return 0;
            }
            break;
        }
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Disable game object after 100ms to allow scoring system to update.
    /// TODO This is a bit hacky.
    /// </summary>
    IEnumerator DisableGameObject()
    {
        yield return blinkTime;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Hold the last collider that has hurt this character for a set amount of time
    /// for scoring purposes.
    /// </summary>
    /// <param name="col">Other collider that has hurt this character.</param>
    IEnumerator SetLastColliderTouched(Collider2D col)
    {
        lastColliderTouched = col;
        yield return holdLastColliderTime;
        lastColliderTouched = null;
    }
    #endregion

    protected override void Awake() 
    {
        if (PlayerNumber == -1)
            throw new Exception("Set playerNumber before enabling. (Is the gameobject in the character prefab enabled which it shouldn't be?)");

        base.Awake();

        contactFilter.NoFilter();

        childRenderer = transform.Find("hitbox").GetComponentsInChildren<Renderer>();
        for (int i = 0; i < childRenderer.Length; i++)
            childRenderer[i].material.color = GameManager.Instance.playerColors[PlayerNumber];

        hitEffect = transform.Find("hit").GetComponent<ParticleSystem>();
        drillsparksPS = transform.Find("drillsparks").GetComponent<ParticleSystem>();
        drillsparksPS.Stop();
        undergroundPS = transform.Find("underground particles").GetComponent<ParticleSystem>();
        undergroundPS.Stop(withChildren:true);

        healthcolor = new Color(255f,0f,0f,0f);
        healthrenderer = transform.Find("_healthbar").gameObject.AddComponent<MeshRenderer>();
        healthrenderer.material = new Material(Shader.Find("Transparent/VertexLit"));
        healthrenderer.material.color = healthcolor;
        healthrenderer.material.SetColor("_SpecColor", new Vector4(0f,0f,0f,0f));

        Freeze();
    }

    /// <summary>
    /// Resets the character to its spawn location/setup. Ends with player freezed.
    /// </summary>
    public override void Reset()
    {
        StopAllCoroutines();
        base.Reset();
        drillsparksPS.Stop();
        undergroundPS.Stop(withChildren:true);
        transform.position = PositionOnSpawn;
        transform.rotation = Quaternion.identity;
        currentHealth = maxHealth;
        for (int i = 0; i < childRenderer.Length; i++)
            childRenderer[i].enabled = true;
        undergroundPS.Stop(withChildren:true);
        if (healthrenderer != null)
        {
            healthcolor.a = 0f;
            healthrenderer.material.color = healthcolor;
        }
        input_turn = 0f;
        input_acc = 0f;
        lastColliderTouchedCoroutine = null;
        lastColliderTouched = null;
        triggerStaying = 0;
        invulnerable = false;
        highSpeed = false;
        Freeze();
    }

    /// <summary>
    /// Resets the character to its spawn location/setup. Ends with countdown and a callback to begin playing.
    /// </summary>
    public void Respawn() 
    {
        gameObject.SetActive(true);
        Reset();
        StartCoroutine(DoBlinks(0.5f));
        StartCoroutine(MakeInvulnerable());
        StartCoroutine(countdown.Start(0.5f, Unfreeze));
    }

    /// <summary>
    /// Just used for debug mode.
    /// </summary>
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            debug = !debug;
            if(debug)
                Time.timeScale = 0.1f;
            else
            {
                Time.timeScale = 1f;
            }
        }
    }

    protected override void CustomMovement()
    {
        velocitySqrMagnitude = Velocity.sqrMagnitude;

        // Rotate
        AddTorque(
            input_turn*turnSpeed*(
                turnUpperSpeedBoundSqr-Mathf.Min(
                    Mathf.Max(velocitySqrMagnitude,turnLowerSpeedBoundSqr)-turnLowerSpeedBoundSqr,
                    turnUpperSpeedBoundSqr
                )
            )/turnUpperSpeedBoundSqr);

        // Set highspeed
        if(velocitySqrMagnitude > highSpeedThresholdSqr && Mathf.Sign(Vector2.Dot(Velocity,-transform.up)) == 1)
            highSpeed = true;
        else
            highSpeed = false;

        // If object has broken surface
        if(underGround && transform.position.y > 0.5 * transform.localScale.y)
        {
            // Stop underground particles
            undergroundPS.Stop(withChildren:true);
            drillsparksPS.Clear();
            drillsparksPS.Stop();
            // Let physics engine handle the intrinsic velocity from here
            // Thus we convert intrinsic to extrinsic and we return because
            // no additional extrinsic forces will be applied in the air
            AddForceRelToVel(intrinsicVelocity);
            intrinsicVelocity = Vector2.zero;
            timeUnderground = 0.0f;
            return;
        }

        // If object is entering ground, play underground particles
        if (!underGround && transform.position.y <= 0.5 * transform.localScale.y)
        {
            undergroundPS.Play(withChildren:true);
        }
        
        if(underGround)
        {
            if(highSpeed)
            {
                if(!drillsparksPS.IsAlive())
                    drillsparksPS.Play();
            } else {
                drillsparksPS.Stop();
            }

            timeUnderground += Time.fixedDeltaTime;

            // Set underground forward motion as a mix of forces and intrinsic velocity
            AddForceRelToVel(-transform.up*speed*0.12f);
            intrinsicVelocity = -transform.up*speed*0.2f*Mathf.Min(timeUnderground,1f);

            // Handle acceleration/braking
            if (Mathf.Abs(input_acc) >= 0.01f)
            {
                if (input_acc > 0)
                    AddForceRelToVel(-transform.up*acc);
                else
                {
                    AddForceRelToVel(transform.up*brake);
                }
            }
        }
    }

    /// <summary>
    /// Check which rigidbody hit this character at which hitbox and act accordingly.
    /// </summary>
    /// <param name="other">Other collider that has hit this character.</param>
    public void BodyPartHit(Collider2D other)
    {
        /// Only do this if there are not already other rigidbodies inside us.
        /// This is done because we have two rigidbodies per character which could trigger this function 
        /// and we only want to execute this once.
        if (triggerStaying == 0)
        {
            /// We determine which body part was hit first with a circlecast from the position of "other"
            /// cast to our position
            int hits = Physics2D.CircleCast(other.transform.position, 0.4f*transform.localScale.x, transform.position-other.transform.position, contactFilter, hitBuffer, 0.5f); 

            if(hits > 0)
            {
                // Get first body part hit
                for (int i = 0; i < hits; i++)
                {
                    if(hitBuffer[i].transform.CompareTag(transform.tag))
                    {
                        firstBodypartHit = hitBuffer[i];
                        break;
                    }
                }

                // Push us!
                AddForce(5f*(transform.position-other.transform.position).normalized/Time.fixedDeltaTime);
                
                // Set lastColliderTouched using a coroutine for scoring evaluation
                if (lastColliderTouched) StopCoroutine(lastColliderTouchedCoroutine);
                lastColliderTouchedCoroutine = StartCoroutine(SetLastColliderTouched(other));

                // Deal damage!
                // Calculation:
                // body hit?
                //      Deal at least 1 damage
                //      Deal 1 additional damage if other.highSpeed    
                // drill hit?
                //      Deal 1 damage if other.highSpeed && !this.highSpeed
                if(!invulnerable)
                {
                    int additionalHighspeedDamage = other.GetComponent<DrillCharacterPart>().DrillController.highSpeed ? 1 : 0;
                    if(firstBodypartHit.transform.name == "body")
                    {
                        StartCoroutine(MakeInvulnerable());
                        Hurt(1 + additionalHighspeedDamage);
                        StartCoroutine(FreezeTimeOnHit());
                    }
                    else
                    {
                        if (additionalHighspeedDamage == 1 && !highSpeed)
                        {
                            StartCoroutine(MakeInvulnerable());
                            Hurt(1);
                            StartCoroutine(FreezeTimeOnHit());
                        }
                    }
                }
            }
        }
    }

    /// <summary>Add 1 to triggerStaying counter.</summary>
    public void TriggerEnter() { triggerStaying += 1; }
    /// <summary>Subtract 1 from triggerStaying counter.</summary>
    public void TriggerExit() { triggerStaying -= 1; }

    /// <summary>
    /// Damage health, update healthbar, play hit effect & report to scoring system.
    /// </summary>
    /// <param name="dmg">Damage dealt.</param>
    public void Hurt(float dmg) 
    {
        currentHealth = Mathf.Max(0f, currentHealth - dmg);
        if (currentHealth <= 0f) {
            LivesLeft -= 1;
            for (int i = 0; i < childRenderer.Length; i++)
                childRenderer[i].enabled = false;
            StartCoroutine(DisableGameObject());
            GameManager.Instance.LostLife(PlayerNumber, lastColliderTouched);
            return;
        }
        else
            hitEffect.Play();
        
        if ((maxHealth-currentHealth)/maxHealth >= 0.5f)
            healthcolor.a = (maxHealth-currentHealth)/maxHealth;
        else
            healthcolor.a = 0f;
        healthrenderer.material.color = healthcolor;
        StartCoroutine(DoBlinks(1f));
    }

    /// <summary>
    /// Set tag of every child in hitbox. Useful when initializing characters at runtime.
    /// </summary>
    /// <param name="s"></param>
    public void SetTag(string s)
    {
        transform.tag = s;
        foreach (Transform child in transform.Find("hitbox"))
        {
            child.tag = s;
        }
    }

    /// <summary>
    /// Heal by amount and update healthbar.
    /// </summary>
    /// <param name="amount">Heal amount.</param>
    public void Heal(float amount) 
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if ((maxHealth-currentHealth)/maxHealth >= 0.5f)
            healthcolor.a = (maxHealth-currentHealth)/maxHealth;
        else
            healthcolor.a = 0f;
        healthrenderer.material.color = healthcolor;
    }

    /// <summary>
    /// Set input_turn; to be called from command.
    /// </summary>
    public void Turn(float f) 
    {
        input_turn = f;
    }

    /// <summary>
    /// Set input_accelerate; to be called from command.
    /// </summary>
    public void Accelerate(float f) 
    {
        input_acc = f;
    }
}

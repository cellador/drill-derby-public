using UnityEngine;

/// <summary>
/// Every drill character shares this script. It notifies the controller
/// which part is being hit.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DrillCharacterPart : MonoBehaviour
{
    Rigidbody2D rb2d;
    readonly string characterTag = "Character";

    // This is public to let other characters know about 
    // drillControllers "highSpeed" state for damage calculation.
    public DrillCharacterController DrillController {get; private set;}

    // Start is called before the first frame update
    void Awake()
    {
        rb2d = transform.GetComponent<Rigidbody2D>();
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.simulated = true;

        GetComponent<Collider2D>().isTrigger = true;

        DrillController = transform.parent.parent.GetComponent<DrillCharacterController>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(!other.CompareTag(tag) && other.tag.Contains(characterTag))
        {
            DrillController.BodyPartHit(other);
            DrillController.TriggerEnter();
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if(!other.CompareTag(tag) && other.tag.Contains(characterTag))
            DrillController.TriggerExit();
    }
}

using UnityEngine;

/// <summary>
/// Heal, Hurt, knock back or add score to player that enters.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ActionOnEnter : MonoBehaviour
{
    [Range(-50,4)]public float changeHealthAmount = 0f;
    [Range(0,5)]public int addToScoreAmount = 0;
    public bool knockBack = false;
    public bool disableOnEnter = false;

    readonly string characterTag = "Character";
    readonly string bodypart = "drill";

    void Awake() 
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Rigidbody2D>().simulated = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag.Contains(characterTag) && other.transform.name == bodypart)
        {
    		DrillCharacterController dcc = other.GetComponent<DrillCharacterPart>().DrillController;
            if(changeHealthAmount > 0)
                dcc.Heal(changeHealthAmount);

            if(changeHealthAmount < 0)
                dcc.Hurt(-changeHealthAmount);

            if(addToScoreAmount > 0)
                GameManager.Instance.AddToScore(dcc.PlayerNumber, addToScoreAmount);

            if(knockBack)
                dcc.AddForce(
                    8f*(other.transform.position-transform.position).normalized/Time.fixedDeltaTime);
            
            if(disableOnEnter)
                gameObject.SetActive(false);
        }
    }
}

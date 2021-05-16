using UnityEngine;

/// <summary>
/// Attach to object such that it will periodically rocket towards nearest player.
/// </summary>
public class Missile : MonoBehaviour
{
    public float speed = 0.2f;

    float timer;
    int nPlayers;
    Vector2 currentMoveDir;
    Vector2 targetMoveDir;

    void OnEnable()
    {
        timer = 0.0f;
        nPlayers = GameManager.Instance.NumPlayers;
        currentMoveDir = Vector2.zero;
    }

    /// <summary>
    /// Shoot towards nearest player position. Target position updates every second.
    /// </summary>
    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        Vector2 move = new Vector2(-transform.position.x+GameManager.Instance.Players[0].transform.position.x,
                                    -transform.position.y+GameManager.Instance.Players[0].transform.position.y);
        float distance = move.magnitude;

        for (int i = 1; i < nPlayers; i++)
        {
            Vector2 move2 = new Vector2(transform.position.x-GameManager.Instance.Players[1].position.x,
                                        transform.position.y-GameManager.Instance.Players[1].position.y);
            float distance2 = move2.magnitude;
            if (distance2 < distance) 
            {
                distance = distance2;
            }
        }

        move = move.normalized;

        if (timer > 1f)
        {
            targetMoveDir = speed*move*Mathf.Min(10f,distance*1.1f);
            timer = 0.0f;
        }

        currentMoveDir = Time.fixedDeltaTime*Vector2.Lerp(Vector2.zero, targetMoveDir, timer*timer);

        
        transform.position = new Vector3(transform.position.x + currentMoveDir.x,
                                        transform.position.y + currentMoveDir.y,
                                        transform.position.z);
    }
}

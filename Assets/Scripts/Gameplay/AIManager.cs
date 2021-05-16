/// <summary>
/// Handles movement for non-player characters. 
/// E.g., player 2 when cold starting into game scene.
/// </summary>
public class AIManager : Singleton<AIManager>
{    
    InputCommands.Turn turn;
    bool[] isPlayerAI = {false,false,false,false};

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        turn = new InputCommands.Turn();
    }
    
    void Update() 
    {
        for (int i = 0; i < 4; i++)
        {
            if(isPlayerAI[i]) turn.Execute(GameManager.Instance.Players[i], 1F);
        }
    }

    public void ToggleAI(int i)
    {
        isPlayerAI[i] = !isPlayerAI[i];
    }
}

using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the current map, spawns health kits and rocks, fits camera etc. 
/// Needs to part of any game scene to allow cold starting.
/// </summary>
public class WorldManager : MonoBehaviour
{
    public float viscosityLand = 3f;
    public float gravity = 9.81f;
    // Where the players will spawn.
    public Vector2 spawnPos = new Vector2(0f,5f);
    public Vector3 cameraPos = new Vector3(0f,-2f,-10f);
    public Vector2 cameraWorldSize = new Vector2(10f,5f);
    public Vector2 rockSizeRange = new Vector2(0.5f,3f);

    // Detect screen size changes
    Vector2 lastScreenSize;

    // Call Singletons in awake once such that game starts even without main menu.
    GameManager gameManager;
    InputManager inputManager;
    CameraController cameraController;

    GameObject healthPack;
    readonly GameObject[] rocks = new GameObject[4];
    readonly WaitForSeconds healthRespawnWait = new WaitForSeconds(30f);
    
    IEnumerator SpawnHealth() 
    {
        while (true) {
            yield return healthRespawnWait;
            if (!healthPack.activeSelf)
            {
                healthPack.transform.position = new Vector3(Random.Range(-8f,8f),Random.Range(-6f,-0.5f),-2f);
                healthPack.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Checks if the window has been resized every 300ms and fits camera accordingly.
    /// </summary>
    IEnumerator CheckForScreenResize(){
        lastScreenSize = new Vector2(Screen.width, Screen.height);

        while(true){
            float pauseEndTime = Time.realtimeSinceStartup + 0.3f;
            while (Time.realtimeSinceStartup < pauseEndTime)
            {
                yield return 0;
            }
            if( lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height ){
                lastScreenSize = new Vector2(Screen.width, Screen.height);
                cameraController.FitCamera(cameraPos, cameraWorldSize);
            }
        }
    }

    void Awake()
    {
        // Call Singletons once such that game starts even without main manu
        gameManager = GameManager.Instance;
        inputManager = InputManager.Instance;

        cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();

        healthPack = Instantiate(Resources.Load<GameObject>("HealthPack"),Vector3.zero, Quaternion.identity);
        healthPack.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            rocks[i] = Instantiate(Resources.Load<GameObject>("Rock"),Vector3.zero, Quaternion.identity);
            rocks[i].SetActive(false);
        }
    }

    void Start()
    {
        StartCoroutine(SpawnHealth());
        cameraController.FitCamera(cameraPos, cameraWorldSize);
        StartCoroutine(CheckForScreenResize());
    }

    /// <summary>
    /// Shuffles rocks around.
    /// Is called at the beginning of each round.
    /// </summary>
    public void NewWorld()
    {
        for (int i = 0; i < 4; i++)
            rocks[i].SetActive(false);
        
        for (int i = 0; i < Random.Range(0,5); i++)
        {
            rocks[i].transform.position = new Vector2(Random.Range(-8f,8f),Random.Range(-6f,-3.5f));
            rocks[i].transform.localScale = Vector3.one * Random.Range(rockSizeRange[0],rockSizeRange[1]);
            rocks[i].SetActive(true);
        }
    }
}

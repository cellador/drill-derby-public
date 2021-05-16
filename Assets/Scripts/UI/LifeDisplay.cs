using UnityEngine;

/// <summary>
/// Used to display lives left on the UI.
/// </summary>
public class LifeDisplay : MonoBehaviour
{
    int nLives = 5;

    GameObject[] lives;

    void Awake()
    {
        lives = new GameObject[5];
        for (int i = 0; i < 5; i++)
            lives[i] = transform.Find("LiveCounter").GetChild(i).gameObject;
    }

    /// <summary>
    /// Show n lives left.
    /// </summary>
    /// <param name="n"></param>
    public void SetLives(int n)
    {
        if (n < nLives)
            for (int i = n; i < nLives; i++)
                lives[i].SetActive(false);
        if (n > nLives)
            for (int i = nLives; i < n; i++)
                lives[i].SetActive(true);
        nLives = n;
    }
}
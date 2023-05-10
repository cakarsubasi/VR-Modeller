using UnityEngine;

public class GameManager : MonoBehaviour
{
    bool extrude = false;

    public bool Extrude { get => extrude; set => extrude = value; }


    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }
}

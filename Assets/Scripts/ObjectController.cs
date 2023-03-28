using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public static ObjectController Instance;
    List<GameObject> selectedGameobject = new();
    float scaleFactor = 0.01f;
    bool scaleDecreaseButtonDown, scaleIncreaseButtonDown = false;

    public List<GameObject> SelectedGameobject { get => selectedGameobject; set => selectedGameobject = value; }
    public bool ScaleDecreaseButtonDown { get => scaleDecreaseButtonDown; set => scaleDecreaseButtonDown = value; }
    public bool ScaleIncreaseButtonDown { get => scaleIncreaseButtonDown; set => scaleIncreaseButtonDown = value; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (ScaleIncreaseButtonDown && !ScaleDecreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.localScale += new Vector3(scaleFactor, scaleFactor, scaleFactor);
            }
        }

        if (!ScaleIncreaseButtonDown && ScaleDecreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                if (gameObject.transform.localScale.x > 0)
                    gameObject.transform.localScale -= new Vector3(scaleFactor, scaleFactor, scaleFactor);
            }
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Persist : MonoBehaviour
{
    // Start is called before the first frame update
    private static Player_Persist instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

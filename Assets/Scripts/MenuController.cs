using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    // Start is called before the first frame update
    public void ContinueGame()
    {
        GameManager.Instance.LoadGame();
    }
}

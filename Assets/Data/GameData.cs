using System;
using UnityEngine;

[Serializable]
public class GameData
{
    public int playerHP;

    public float playerPosX;
    public float playerPosY;

    public string currentScene;
    public bool isDogUnlocked;
    public string lastPortalID;
}
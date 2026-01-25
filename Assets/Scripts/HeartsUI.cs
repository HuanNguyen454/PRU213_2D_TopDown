using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Player_Health playerHealth;

    [Header("Heart Sprites")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartEmpty;

    [Header("Prefab")]
    [SerializeField] private Image heartPrefab;

    [Header("Config")]
    [SerializeField] private int hpPerHeart = 20; // 1 tim = bao nhiêu HP (100 HP => 5 tim n?u hpPerHeart=20)

    private readonly List<Image> hearts = new();
    private int lastHP = -1;
    private int lastMaxHP = -1;

    private void Start()
    {
        Rebuild();
        Refresh();
    }

    private void Update()
    {
        if (playerHealth == null) return;

        if (playerHealth.MaxHP != lastMaxHP)
        {
            Rebuild();
            Refresh();
        }
        else if (playerHealth.CurrentHP != lastHP)
        {
            Refresh();
        }
    }

    private void Rebuild()
    {
        // Clear old
        foreach (Transform child in transform) Destroy(child.gameObject);
        hearts.Clear();

        lastMaxHP = playerHealth.MaxHP;

        int heartCount = Mathf.CeilToInt((float)lastMaxHP / hpPerHeart);
        for (int i = 0; i < heartCount; i++)
        {
            Image img = Instantiate(heartPrefab, transform);
            hearts.Add(img);
        }
    }

    private void Refresh()
    {
        lastHP = playerHealth.CurrentHP;

        int halfHeartHP = hpPerHeart / 2;

        for (int i = 0; i < hearts.Count; i++)
        {
            int heartStartHP = i * hpPerHeart;
            int hpInThisHeart = Mathf.Clamp(lastHP - heartStartHP, 0, hpPerHeart);

            if (hpInThisHeart >= hpPerHeart) hearts[i].sprite = heartFull;
            else if (hpInThisHeart >= halfHeartHP) hearts[i].sprite = heartHalf;
            else hearts[i].sprite = heartEmpty;
        }
    }
}

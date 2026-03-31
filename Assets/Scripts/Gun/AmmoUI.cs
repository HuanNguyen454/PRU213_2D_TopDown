using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private Player_Gun playerGun;

    private void Start()
    {
        HideUI();
        TryBindCurrentPlayer();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerSpawned += HandlePlayerSpawned;

        TryBindCurrentPlayer();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerSpawned -= HandlePlayerSpawned;

        UnbindCurrentGun();
    }

    private void HandlePlayerSpawned(Player_Health playerHealth)
    {
        if (playerHealth == null)
        {
            SetPlayerGun(null);
            return;
        }

        Player_Gun gun = playerHealth.GetComponent<Player_Gun>();
        SetPlayerGun(gun);
    }

    private void TryBindCurrentPlayer()
    {
        if (playerGun != null)
        {
            SetPlayerGun(playerGun);
            return;
        }

        if (GameManager.Instance != null)
        {
            Player_Health currentPlayer = GameManager.Instance.GetCurrentPlayer();
            if (currentPlayer != null)
            {
                Player_Gun gun = currentPlayer.GetComponent<Player_Gun>();
                SetPlayerGun(gun);
                return;
            }
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Player_Gun gun = player.GetComponent<Player_Gun>();
            SetPlayerGun(gun);
        }
        else
        {
            HideUI();
        }
    }

    public void SetPlayerGun(Player_Gun gun)
    {
        UnbindCurrentGun();

        playerGun = gun;

        if (playerGun != null)
        {
            playerGun.OnAmmoChanged += UpdateAmmoUI;
            UpdateAmmoUI(playerGun.CurrentAmmo, playerGun.ReserveAmmo);
        }
        else
        {
            HideUI();
        }
    }

    private void UnbindCurrentGun()
    {
        if (playerGun != null)
            playerGun.OnAmmoChanged -= UpdateAmmoUI;
    }

    private void UpdateAmmoUI(int currentAmmo, int reserveAmmo)
    {
        if (ammoText == null) return;

        if (playerGun == null || !playerGun.HasGun)
        {
            HideUI();
            return;
        }

        ammoText.gameObject.SetActive(true);

        if (playerGun.IsReloading)
            ammoText.text = $"Reloading...\n{currentAmmo}/{reserveAmmo}";
        else
            ammoText.text = $"{currentAmmo}/{reserveAmmo}";
    }

    private void HideUI()
    {
        if (ammoText != null)
            ammoText.gameObject.SetActive(false);
    }
}
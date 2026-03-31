using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Volume UI")]
    [SerializeField] private GameObject volumePanel;
    [SerializeField] private Slider volumeSlider;

    private bool isPaused = false;

    private void Start()
    {
        // Set slider đúng với volume hiện tại
        if (volumeSlider != null && AudioManager.Instance != null)
        {
            volumeSlider.value = AudioManager.Instance.GetVolume();

            // đảm bảo không bị add nhiều listener
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // đảm bảo panel volume tắt ban đầu
        if (volumePanel != null)
            volumePanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseCanvas.SetActive(true);

        // cập nhật lại slider mỗi lần mở pause (tránh lệch)
        if (volumeSlider != null && AudioManager.Instance != null)
        {
            volumeSlider.value = AudioManager.Instance.GetVolume();
        }
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseCanvas.SetActive(false);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void ToggleVolume()
    {
        if (volumePanel == null) return;

        volumePanel.SetActive(!volumePanel.activeSelf);
    }

    private void OnVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(value);
        }
    }
}
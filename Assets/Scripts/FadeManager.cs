using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("UI")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private GameObject loadingGroup;

    [Header("Config")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float loadDelay = 0.1f;

    public bool IsFading { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // đảm bảo ban đầu trong suốt
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        if (loadingGroup != null)
            loadingGroup.SetActive(false);
    }

    private void Update()
    {
        //// xoay loading icon
        //if (loadingGroup != null && loadingGroup.activeSelf)
        //{
        //    loadingGroup.transform.Rotate(0, 0, -200f * Time.unscaledDeltaTime);
        //}
    }

    // =========================
    // FADE OUT + LOADING
    // =========================
    public IEnumerator FadeOutWithLoading()
    {
        if (fadeImage == null) yield break;

        IsFading = true;

        if (loadingGroup != null)
            loadingGroup.SetActive(true);

        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / fadeDuration;

            // ease-in
            p = p * p;

            c.a = p;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;

        // delay sau khi đen hoàn toàn
        yield return new WaitForSecondsRealtime(loadDelay);
    }

    // =========================
    // FADE IN
    // =========================
    public IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / fadeDuration;

            // ease-out
            p = 1f - (1f - p) * (1f - p);

            c.a = 1f - p;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;

        if (loadingGroup != null)
            loadingGroup.SetActive(false);

        IsFading = false;
    }
}
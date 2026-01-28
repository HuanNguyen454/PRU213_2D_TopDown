using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private AudioClip hoverClip;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip == null) return;

        audioSource.PlayOneShot(hoverClip);
    }
}

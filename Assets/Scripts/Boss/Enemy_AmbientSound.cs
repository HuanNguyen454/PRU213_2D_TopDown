using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Enemy_AmbientSound : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private float volume = 1f;

    [Header("Timing")]
    [SerializeField] private float minDelay = 2f;
    [SerializeField] private float maxDelay = 5f;
    [SerializeField] private bool playOnStart = true;

    private Coroutine soundRoutine;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // ??i thành 1f n?u mu?n 3D
            audioSource.volume = volume;
        }
    }

    private void OnEnable()
    {
        if (playOnStart)
            StartAmbientLoop();
    }

    private void OnDisable()
    {
        StopAmbientLoop();
    }

    public void StartAmbientLoop()
    {
        if (soundRoutine != null)
            StopCoroutine(soundRoutine);

        soundRoutine = StartCoroutine(AmbientLoopRoutine());
    }

    public void StopAmbientLoop()
    {
        if (soundRoutine != null)
        {
            StopCoroutine(soundRoutine);
            soundRoutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private IEnumerator AmbientLoopRoutine()
    {
        while (true)
        {
            if (audioSource != null && ambientClip != null)
            {
                audioSource.clip = ambientClip;
                audioSource.volume = volume;
                audioSource.Play();

                yield return new WaitForSeconds(ambientClip.length);
            }

            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);
        }
    }
}

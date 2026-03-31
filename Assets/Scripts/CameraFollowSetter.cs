using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class CameraFollowSetter : MonoBehaviour
{
    private CinemachineVirtualCamera cam;

    private void Awake()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetCameraTarget();
    }

    private void Start()
    {
        SetCameraTarget();
    }

    void SetCameraTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && cam != null)
        {
            cam.Follow = player.transform;
            cam.LookAt = player.transform;
        }
    }
}
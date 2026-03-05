using UnityEngine;
using Cinemachine;

public class CameraFollowSetter : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) return;

        CinemachineVirtualCamera cam = GetComponent<CinemachineVirtualCamera>();

        if (cam != null)
        {
            cam.Follow = player.transform;
            cam.LookAt = player.transform;
        }
    }
}
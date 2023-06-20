using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private List<CinemachineVirtualCameraBase> myCameras;

    private int currentCameraIndex = 0;

    private void Update()
    {
        // Check for input to switch cameras
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Disable the current camera
            DisableCurrentCamera();

            // Switch to the next camera
            currentCameraIndex = (currentCameraIndex + 1) % myCameras.Count;

            // Enable the new camera
            EnableCurrentCamera();
        }
    }

    public void EnableCurrentCamera()
    {
        myCameras[currentCameraIndex].Priority = 100;
    }

    private void DisableCurrentCamera()
    {
        myCameras[currentCameraIndex].Priority = 1;
    }

    public void AddCamera(CinemachineVirtualCameraBase camera, bool primary = false)
    {
        myCameras.Add(camera);

        if (primary)
        {
            DisableCurrentCamera();
            currentCameraIndex = myCameras.Count - 1;
            EnableCurrentCamera();
        }
    }
}

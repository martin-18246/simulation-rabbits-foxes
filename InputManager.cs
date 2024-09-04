using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float orbitingDistance;
    public float orbitingHeight;

    public float timePerRevolution;
    private float startTime;

    private float angleRadians;

    public enum CameraMode
    {
        MouseMovement,
        Orbiting,
    }

    public CameraMode cameraMode;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        cameraMode = CameraMode.MouseMovement;


    }

    // Update is called once per frame
    void Update()
    {
        ChangeCameraMode();

        if (cameraMode == CameraMode.Orbiting) Orbit();
    }

    private void ChangeCameraMode()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (cameraMode != CameraMode.Orbiting) cameraMode = CameraMode.Orbiting; else cameraMode = CameraMode.MouseMovement;
            transform.position = new Vector3(orbitingDistance, orbitingHeight, 0f);
        }
        if (Input.GetKeyDown(KeyCode.M)) cameraMode = CameraMode.MouseMovement;
    }


    private float CalculateAngleRadiansBasedOnTime()
    {
        return ( ( (Time.time - startTime) % timePerRevolution ) / timePerRevolution) * 2 * Mathf.PI;
    }

    private void Orbit()
    {
        angleRadians = CalculateAngleRadiansBasedOnTime();
        float xPosition = Mathf.Sin(angleRadians) * orbitingDistance;
        float zPosition = Mathf.Cos(angleRadians) * orbitingDistance;

        transform.position = new Vector3(xPosition, orbitingHeight, zPosition);
        transform.LookAt(Vector3.zero);
    }

}

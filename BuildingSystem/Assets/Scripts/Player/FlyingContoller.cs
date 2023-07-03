using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingContoller : MonoBehaviour
{
    [Header("Flying Settings")]
    public float speed = 5.0f;
    public Vector2 lookSensitivity = Vector2.one;

    [Header("References")]
    private Camera playerCam;
    private BuildController buildController;

    //use this vec to store temp inputs, saving inits 
    private Vector3 inputVec;

    private void Start()
    {
        playerCam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        buildController = GetComponent<BuildController>();
    }

    void Update()
    {
        Move();

        //lock rotation when building
        if (!buildController.currentlyBuilding) { Look(); }

        MangeBuildMode();
    }

    private void Look()
    {
        inputVec = Vector3.up * -Input.GetAxis("Mouse Y") + Vector3.right * Input.GetAxis("Mouse X");

        //rotate player obj
        transform.Rotate(Vector3.up, inputVec.x * lookSensitivity.x * Time.deltaTime);

        //look up and down through the camera
        float upAngle = playerCam.transform.eulerAngles.x + inputVec.y * lookSensitivity.y * Time.deltaTime;
        upAngle = Mathf.Clamp(upAngle, 0, 80);

        playerCam.transform.rotation = Quaternion.Euler(upAngle, 0, 0);
    }

    private void Move()
    {
        //wasd movement: forward in the cam's direction and left-right using the players orientation 
        inputVec = playerCam.transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");      
        transform.Translate(inputVec.normalized * speed * Time.deltaTime, Space.World);
    }

    private void MangeBuildMode()
    {
        if (Input.GetKeyDown(KeyCode.B)) {
            //toggle building 
            bool building = !buildController.currentlyBuilding;

            //setup cursor
            Cursor.lockState = (building) ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = building;

            //set building flag
            buildController.currentlyBuilding = building;
        }
    }

}

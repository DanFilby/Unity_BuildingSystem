using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class FlyingContoller : MonoBehaviour
{
    [Header("Flying Settings")]
    public float moveSpeed = 5.0f;
    public float riseSpeed = 2.5f;
    public Vector2 lookSensitivity = Vector2.one;

    [Header("References")]
    private Camera playerCam;
    private BuildController buildController;

    //use this vec to store temp inputs
    private Vector3 inputVec;
    private float lookYAngle = 0.0f;

    private void Start()
    {
        playerCam = GetComponentInChildren<Camera>();
        playerCam.transform.localRotation = Quaternion.Euler(0, 0, 0);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        buildController = GetComponent<BuildController>();
    }

    void Update()
    {
        //lock movement and looking when building
        if (!buildController.CurrentlyBuilding) {
            Move();
            Look();
            Rise_Descend();
        }

        MangeBuildMode();
    }

    private void Look()
    {
        inputVec = Vector3.up * -Input.GetAxis("Mouse Y") + Vector3.right * Input.GetAxis("Mouse X");

        //rotate player obj
        transform.Rotate(Vector3.up, inputVec.x * lookSensitivity.x * Time.deltaTime);

        //look up and down through the camera
        lookYAngle = Mathf.Clamp(lookYAngle + inputVec.y * lookSensitivity.y * Time.deltaTime, -25, 85);
        playerCam.transform.localRotation = Quaternion.Euler(lookYAngle, 0, 0) ;
    }

    private void Move()
    {
        //wasd movement: forward in the cam's direction and left-right using the players orientation 
        inputVec = playerCam.transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");      
        transform.Translate(inputVec.normalized * moveSpeed * Time.deltaTime, Space.World);
    }

    private void Rise_Descend()
    {
        inputVec = Vector3.up * (Input.GetKey(KeyCode.Space) ? 1 : 0) + Vector3.down * (Input.GetKey(KeyCode.LeftControl) ? 1 : 0);
        transform.Translate(inputVec * riseSpeed * Time.deltaTime, Space.World);
    }

    private void MangeBuildMode()
    {
        if (Input.GetKeyDown(KeyCode.B)) {
            //toggle building 
            bool building = !buildController.CurrentlyBuilding;

            //setup cursor
            Cursor.lockState = (building) ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = building;

            //set building flag
            buildController.CurrentlyBuilding = building;
        }
    }

}

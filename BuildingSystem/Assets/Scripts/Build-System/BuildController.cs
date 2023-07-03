using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildController : MonoBehaviour
{
    [Header("Building Settings")]
    public LayerMask placingLayerMask;
    public LayerMask buildingLayerMask;

    [Header("References")]
    private Camera playerCamera;
    public List<GameObject> AllBuildObjects;

    //materials to show whether the player can place the obj at its current pos
    public Material guideMat_Valid;
    public Material guideMat_Invalid;

    [Header("UI References")]
    public TMPro.TextMeshProUGUI buildingModeText;
    public List<UI_BuildObj> buildObjButtons; 

    private bool currentlyBuilding;
    private GameObject selectedBuildObj;
    private GameObject guideBuildObj;


    public bool CurrentlyBuilding
    {
        get { return currentlyBuilding; }
        set { currentlyBuilding = value;    //set as usual then call either activate or disable funcs
            if (currentlyBuilding) { ActivateBuildMode(); }
            else { DisableBuildMode(); }
        }
    }


    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        currentlyBuilding = false;
    }

    void Update()
    {
        if(currentlyBuilding && guideBuildObj != null) {
            Building();
        }

        if (Input.GetMouseButtonDown(0)) {
            if(CursorWorldPos(out Vector3 pos)) {
            }
        }

    }

    private void Building()
    {
        Vector3 pointerPos;

        if (!CursorWorldPos(out pointerPos)) {
            //pointing ray doesn't hit valid building surface
            guideBuildObj.GetComponent<Renderer>().enabled = false;
            return;
        }

        guideBuildObj.transform.position = pointerPos;
        bool validPlacement = CheckBuildValid();

        //enable renderer, and set according to validity
        guideBuildObj.GetComponent<Renderer>().enabled = true;
        guideBuildObj.GetComponent<Renderer>().material = (validPlacement) ? guideMat_Valid : guideMat_Invalid;

        if(Input.GetMouseButtonDown (0)) {


        }

    }



    private bool CheckBuildValid()
    {
        Bounds colliderBounds = guideBuildObj.GetComponent<Collider>().bounds;

        if(Physics.CheckBox(colliderBounds.center, colliderBounds.extents, guideBuildObj.transform.rotation, buildingLayerMask)) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// finds world pos of where the cursor is pointing
    /// </summary>
    public bool CursorWorldPos(out Vector3 hitPos)
    {
        hitPos = Vector3.zero; 

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f, placingLayerMask)) {
            hitPos = hit.point;
            return true;
        }

        return false;   
    }

    private void ActivateBuildMode()
    {
        buildingModeText.text = "Building";
    }

    private void DisableBuildMode()
    {
        buildingModeText.text = "Viewing";
    }

    public void BUTTON_ChangeBuildingObject(int _index)
    {
        //update button's ui to show selected, also check index valid
        if (!ManageButtonsUI(_index)) { return; }

        //destroys previous guide objects
        ClearCurrentSelectedObjs();

        //'none' button selected
        if (_index == 0) {
            return;
        }

        //find the game object from the id
        selectedBuildObj = AllBuildObjects.Find(x => x.GetComponent<BuildingObject>().obj_Id == _index);

        guideBuildObj = Instantiate(selectedBuildObj, Vector3.zero, Quaternion.identity);     
    }

    /// <summary>
    /// updates the button's ui to show the one selected, as well as checking for a valid index
    /// </summary>
    /// <returns> true if given index is valid </returns>
    private bool ManageButtonsUI(int _index)
    {
        //update the buttons ui to show the one clicked as selected
        UI_BuildObj selectedBuildObjButton = null;

        foreach (var button in buildObjButtons) {
            if (button.ID == _index) {
                selectedBuildObjButton = button;
            }
            else { button.ShowUnSelected(); }
        }

        //check index was valid 
        if (selectedBuildObjButton == null) {
            return false; 
        }

        selectedBuildObjButton.ShowSelected();
        return true;
    }

    private void ClearCurrentSelectedObjs()
    {
        selectedBuildObj = null;

        if(guideBuildObj != null) {
            Destroy(guideBuildObj);
            guideBuildObj = null;
        }
    }



}
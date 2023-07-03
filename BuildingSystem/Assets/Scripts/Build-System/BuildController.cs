using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildController : MonoBehaviour
{
    [Header("Building Settings")]
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
        if (Input.GetMouseButtonDown(0)) {
            if(CursorWorldPos(out Vector3 pos)) {
            }
        }

    }

    public bool CursorWorldPos(out Vector3 hitPos)
    {
        hitPos = Vector3.zero; 

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, buildingLayerMask)) {
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

        ClearCurrentSelectedObjs();

        //'none' button selected
        if (_index == 0) {
            return;
        }

        //find the game object from the id
        selectedBuildObj = AllBuildObjects.Find(x => x.GetComponent<BuildingObject>().obj_Id == _index);

        guideBuildObj = Instantiate(selectedBuildObj, Vector3.zero, Quaternion.identity);
        
    }

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
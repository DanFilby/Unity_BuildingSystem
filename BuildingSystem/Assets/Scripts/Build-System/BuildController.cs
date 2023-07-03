using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public GameObject canvasObj;
    public TMPro.TextMeshProUGUI buildingModeText;
    public List<UI_BuildObj> buildObjButtons;

    //ui raycasting
    GraphicRaycaster UIRaycaster;
    PointerEventData CursorEventData;
    EventSystem CanvasEventSystem;

    private bool currentlyBuilding;

    private GameObject selectedBuildObj;
    private Vector3 selectedBuildObjPosOffset;
    private Vector3 currentScale = Vector3.one;

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
        UIRaycaster = canvasObj.GetComponent<GraphicRaycaster>();
        CanvasEventSystem = canvasObj.GetComponent<EventSystem>();

        playerCamera = GetComponentInChildren<Camera>();
        currentlyBuilding = false;
    }

    void Update()
    {
        if(currentlyBuilding && guideBuildObj != null) {
            Building();
        }

        if(Input.mouseScrollDelta.y != 0) {
            currentScale += Vector3.one * Input.mouseScrollDelta.y * 2.0f * Time.deltaTime;
            guideBuildObj.transform.localScale = currentScale;
            selectedBuildObjPosOffset = new Vector3(0, guideBuildObj.GetComponent<Collider>().bounds.extents.y, 0);
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

        guideBuildObj.transform.position = pointerPos + selectedBuildObjPosOffset;
        bool validPlacement = CheckBuildValid();

        //enable renderer, and set according to validity
        guideBuildObj.GetComponent<Renderer>().enabled = true;
        guideBuildObj.GetComponent<Renderer>().material = (validPlacement) ? guideMat_Valid : guideMat_Invalid;

        if(Input.GetMouseButtonDown (0) && validPlacement) {
            BuildObj();
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

    private void BuildObj()
    {
        GameObject g = Instantiate(selectedBuildObj, guideBuildObj.transform.position, guideBuildObj.transform.rotation);
        g.transform.localScale = guideBuildObj.transform.localScale;
        g.layer = LayerMask.NameToLayer("building");
    }


    /// <summary>
    /// finds world pos of where the cursor is pointing
    /// </summary>
    public bool CursorWorldPos(out Vector3 hitPos)
    {
        hitPos = Vector3.zero; 

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f, placingLayerMask) && !HitUI()) {
            hitPos = hit.point;
            return true;
        }

        return false;   
    }

    /// <summary>
    /// checks if the mouse is pointing over any UI elements
    /// </summary>
    private bool HitUI()
    {
        CursorEventData = new PointerEventData(CanvasEventSystem);
        CursorEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        UIRaycaster.Raycast(CursorEventData, results);

        return (results.Count > 0);
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
        Debug.Log(_index);

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

        selectedBuildObjPosOffset = new Vector3(0, guideBuildObj.GetComponent<Collider>().bounds.extents.y, 0);
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
            if (button.GetId() == _index) {
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
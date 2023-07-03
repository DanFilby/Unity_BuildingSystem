using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UI_BuildObj;

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
    private Vector3 currentRotOffset = Vector3.zero;

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
            ManageBuildPlacing();
            Building();
        }
        
    }

    private void ManageBuildPlacing()
    {
        //edit scale
        if (Input.mouseScrollDelta.y != 0) {
            currentScale += Vector3.one * Input.mouseScrollDelta.y * 2.0f * Time.deltaTime;
            guideBuildObj.transform.localScale = currentScale;
            selectedBuildObjPosOffset = new Vector3(0, guideBuildObj.GetComponent<Collider>().bounds.extents.y, 0);
        }

        //edit rotation
        if (Input.GetKeyDown(KeyCode.E)) {
            currentRotOffset.y += 45;
            guideBuildObj.transform.localEulerAngles = currentRotOffset;
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            currentRotOffset.y += -45;
            guideBuildObj.transform.localEulerAngles = currentRotOffset;
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

        SnapToNearbyBuildings(ref pointerPos);

        bool validPlacement = CheckBuildValid();

        //enable renderer, and set according to validity
        guideBuildObj.GetComponent<Renderer>().enabled = true;
        guideBuildObj.GetComponent<Renderer>().material = (validPlacement) ? guideMat_Valid : guideMat_Invalid;

        if(Input.GetMouseButtonDown (0) && validPlacement) {
            BuildObj();
        }

    }
    //TODO: snapping roation and scale, save rotaion and use all dimensions in scale   

    private void SnapToNearbyBuildings(ref Vector3 worldPos)
    {
        //check for nearby buildings close to the cursor 
        Collider[] nearbyBuildings = Physics.OverlapBox(worldPos, guideBuildObj.GetComponent<Collider>().bounds.extents * 4.0f,
            guideBuildObj.transform.rotation, buildingLayerMask);

        if(nearbyBuildings.Length > 0) {
            //find the closest building near the cursor pointer
            Collider closestBuilding = FindClosestCollider(worldPos, nearbyBuildings);

            //calculate the angle between both points
            Vector3 offset = closestBuilding.transform.position - worldPos;
            float angle = (Mathf.Atan2(offset.z, offset.x)) * Mathf.Rad2Deg + 180;

            //set the world position as the nearby building
            worldPos = closestBuilding.transform.position;

            //add offset in each direction using the angle calculated
            if (angle >= 135 && angle <= 225) {
                worldPos.x -= closestBuilding.GetComponent<BuildingObject>().Radius + selectedBuildObjPosOffset.y;
            }
            else if ((angle >= 0 && angle <= 45) || (angle > 305 && angle < 360)) {
                worldPos.x += closestBuilding.GetComponent<BuildingObject>().Radius + selectedBuildObjPosOffset.y;
            }
            else if (angle >= 45 && angle <= 135) {
                worldPos.z += closestBuilding.GetComponent<BuildingObject>().Radius + selectedBuildObjPosOffset.y;
            }
            else if (angle >= 225 && angle <= 305) {
                worldPos.z -= closestBuilding.GetComponent<BuildingObject>().Radius + selectedBuildObjPosOffset.y;
            }

            //update guide object's positon
            guideBuildObj.transform.position = worldPos;
        }
    }

    private bool CheckBuildValid()
    {
        Bounds colliderBounds = guideBuildObj.GetComponent<Collider>().bounds ;

        if(Physics.CheckBox(colliderBounds.center, colliderBounds.extents - Vector3.one * 0.01f, guideBuildObj.transform.rotation, buildingLayerMask)) {
            return false;
        }

        return true;
    }

    private void BuildObj()
    {
        GameObject g = Instantiate(selectedBuildObj, guideBuildObj.transform.position, guideBuildObj.transform.rotation);
        g.transform.localScale = guideBuildObj.transform.localScale;
        g.layer = LayerMask.NameToLayer("building");
        g.GetComponent<BuildingObject>().Radius = selectedBuildObjPosOffset.y;
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
        if (_index == (int)ButtonType.None || _index == (int)ButtonType.Edit || _index == (int)ButtonType.Delete) {
            ClearBuildingAdjustments();
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

    private void ClearBuildingAdjustments()
    {
        currentRotOffset = Vector3.zero;
        currentScale = Vector3.one;
    }

    Collider FindClosestCollider(Vector3 worldPos, Collider[] nearbyBuildings)
    {
        Collider closestBuilding = null; float closestDist = Mathf.Infinity;
        foreach (var c in nearbyBuildings) {
            float dist = Vector3.Distance(worldPos, c.transform.position);
            if (dist < closestDist) {
                closestDist = dist;
                closestBuilding = c;
            }
        }
        return closestBuilding;
    }

}
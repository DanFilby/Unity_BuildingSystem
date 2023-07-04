using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UI_BuildObj;

public class BuildController : MonoBehaviour
{
    enum BuildState { none, edit, destroy, building, wallBuilding}

    [Header("Building Settings")]
    public LayerMask placingLayerMask;
    public LayerMask buildingLayerMask;
    public LayerMask snappingLayerMask;

    [Header("References")]
    private Camera playerCamera;
    public List<GameObject> AllBuildObjects;
    public CastleBuilding castleBuilder;

    //materials to show whether the player can place the obj at its current pos
    public Material guideMat_Valid;
    public Material guideMat_Invalid;
    public Material regularBuildObjMat;

    [Header("UI References")]
    public GameObject canvasObj;
    public TMPro.TextMeshProUGUI buildingModeText;
    public List<UI_BuildObj> buildObjButtons;

    //ui raycasting
    GraphicRaycaster UIRaycaster;
    PointerEventData CursorEventData;
    EventSystem CanvasEventSystem;

    //controller flags
    private bool controllerActive;
    private BuildState curBuildState;

    //selected building object prefab and bounds
    private GameObject selectedBuildObj;
    private Vector3 selectedBuildObjBoundsExtent;

    //settings for current building object
    private Vector3 currentScale = Vector3.one;
    private Vector3 currentRotOffset = Vector3.zero;

    //actual object used to show where the new build will be
    private GameObject guideBuildObj;

    //editing building
    private GameObject currentEditHoverObj;
    private bool currentlyEditingObj;

    //destroying building
    private GameObject currentDestroyHoverObj;

    //wall buidling
    private bool placedFirstTower = false;
    private Vector3 firstTowerPos;

    public bool CurrentlyBuilding
    {
        get { return controllerActive; }
        set { controllerActive = value;    //set as usual then call either activate or disable funcs
            if (controllerActive) { ActivateController(); }
            else { DisableController(); }
        }
    }


    void Start()
    {
        UIRaycaster = canvasObj.GetComponent<GraphicRaycaster>();
        CanvasEventSystem = canvasObj.GetComponent<EventSystem>();

        playerCamera = GetComponentInChildren<Camera>();
        controllerActive = false;
    }

    void Update()
    {
        if(controllerActive) {
            if(curBuildState == BuildState.building && guideBuildObj != null) {
                AdjustBuildObject();
                Building();
            }
            else if(curBuildState == BuildState.wallBuilding) {
                AdjustWallBuilding();
                WallBuilding();
            }
            else if(curBuildState == BuildState.edit) {
                Editing();
            }
            else if(curBuildState == BuildState.destroy) {
                Destroying();
            }        
        }
        
    }

    private void AdjustBuildObject()
    {
        //edit scale from scroll wheel
        if (Input.mouseScrollDelta.y != 0) {
            currentScale += Vector3.one * Input.mouseScrollDelta.y * 2.0f * Time.deltaTime;
            guideBuildObj.transform.localScale = currentScale;
            selectedBuildObjBoundsExtent = guideBuildObj.GetComponent<Collider>().bounds.extents;
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
        guideBuildObj.transform.position = pointerPos + selectedBuildObjBoundsExtent;

        SnapToNearbyBuildings(ref pointerPos);

        bool validPlacement = CheckBuildValid();

        //enable renderer, and set according to validity
        guideBuildObj.GetComponent<Renderer>().enabled = true;
        guideBuildObj.GetComponent<Renderer>().material = (validPlacement) ? guideMat_Valid : guideMat_Invalid;

        if(Input.GetMouseButtonDown (0) && validPlacement) {

            if (currentlyEditingObj) {
                BuildEditingObj();
            }
            else {
                BuildObj();
            }
        }

    }

    private void AdjustWallBuilding()
    {
        //edit scale from scroll wheel
        if (Input.mouseScrollDelta.y != 0) {
            currentScale += Vector3.one * Input.mouseScrollDelta.y * 2.0f * Time.deltaTime;

            if (guideBuildObj != null) { Destroy(guideBuildObj); }
            guideBuildObj = castleBuilder.CreateWall(Vector3.zero, currentScale.x);
            selectedBuildObjBoundsExtent = guideBuildObj.GetComponent<Collider>().bounds.extents;
        }
    }

    private bool CheckWallValid()
    {
        Bounds colliderBounds = guideBuildObj.GetComponent<Collider>().bounds;

        if (Physics.CheckBox(colliderBounds.center, colliderBounds.extents - Vector3.one * 0.01f,
            guideBuildObj.transform.rotation, buildingLayerMask)) {
            return false;
        }

        return true;
    }

    private void WallBuilding()
    {
        Vector3 pointerPos;
        if (!CursorWorldPos(out pointerPos)) {
            //pointing ray doesn't hit valid building surface
            guideBuildObj.GetComponent<Renderer>().enabled = false;
            return;
        }

        //once the first tower is placed, show how the finished wall would be built with the guide object
        if (placedFirstTower) {
            if (guideBuildObj != null) { Destroy(guideBuildObj); }
            guideBuildObj = castleBuilder.CreateWall(firstTowerPos, pointerPos, currentScale.y);
            guideBuildObj.transform.position += Vector3.up * guideBuildObj.GetComponent<Collider>().bounds.extents.y * currentScale.y;
        }
        else {
            guideBuildObj.transform.position = pointerPos + selectedBuildObjBoundsExtent * currentScale.y;
        }

        //TODO: Fix valid check
        //bool validPlacement = CheckWallValid();
        bool validPlacement = true;

        //enable renderer, and set according to validity
        guideBuildObj.GetComponent<Renderer>().enabled = true;
        guideBuildObj.GetComponent<Renderer>().material = (validPlacement) ? guideMat_Valid : guideMat_Invalid;

        if (Input.GetMouseButtonDown(0) && validPlacement) {
            //place the wall's starting point
            if(placedFirstTower == false) {
                firstTowerPos = pointerPos;
                placedFirstTower = true;
            }
            //build the full wall
            else if(placedFirstTower == true) {
                Destroy(guideBuildObj);
                GameObject g = castleBuilder.CreateWall(firstTowerPos, pointerPos, currentScale.y);

                guideBuildObj.GetComponent<Renderer>().material = regularBuildObjMat;
                g.transform.position += Vector3.up * guideBuildObj.GetComponent<Collider>().bounds.extents.y * currentScale.y;
                g.layer = 9;

                //reset guide to single wall
                guideBuildObj = castleBuilder.CreateWall(Vector3.zero, currentScale.x);
                selectedBuildObjBoundsExtent = guideBuildObj.GetComponent<Collider>().bounds.extents;
                placedFirstTower = false;
            }
        }
    }

    private void Editing()
    {
        if (PointingAtBuilding(out GameObject buildingHit, out _)) {
            //reset material of previous hover obj
            if(currentEditHoverObj != buildingHit && currentEditHoverObj != null) { currentEditHoverObj.GetComponent<Renderer>().material = regularBuildObjMat; }

            buildingHit.GetComponent<Renderer>().material = guideMat_Valid;
            currentEditHoverObj = buildingHit;

            if (Input.GetMouseButtonDown(0) && !currentlyEditingObj) {
                guideBuildObj = currentEditHoverObj;
                curBuildState = BuildState.building;
                currentScale = guideBuildObj.transform.localScale;
                selectedBuildObjBoundsExtent = guideBuildObj.GetComponent<BuildingObject>().BoundsExtent;
                selectedBuildObj = guideBuildObj;
                guideBuildObj.layer = 8;
                currentlyEditingObj = true;
            }

        }
        //reset the colour 
        else if (currentEditHoverObj != null) {
            currentEditHoverObj.GetComponent<Renderer>().material = regularBuildObjMat;
            currentEditHoverObj = null;
        }

    }

    private void Destroying()
    {
        if (PointingAtBuilding(out GameObject buildingHit, out _)) {
            //reset material of previous hover obj
            if (currentDestroyHoverObj != buildingHit && currentDestroyHoverObj != null) { currentDestroyHoverObj.GetComponent<Renderer>().material = regularBuildObjMat; }

            buildingHit.GetComponent<Renderer>().material = guideMat_Invalid;
            currentDestroyHoverObj = buildingHit;

            if (Input.GetMouseButtonDown(0) && !currentlyEditingObj) {
                Destroy(buildingHit);
                currentDestroyHoverObj = null;
            }

        }
        //reset the colour 
        else if (currentDestroyHoverObj != null) {
            currentDestroyHoverObj.GetComponent<Renderer>().material = regularBuildObjMat;
            currentDestroyHoverObj = null;
        }
    }

    //TODO: snapping roation
    private void SnapToNearbyBuildings(ref Vector3 worldPos)
    {
        //check for nearby buildings close to the cursor 
        Collider[] nearbyBuildings = Physics.OverlapBox(worldPos, Vector3.one * 2.0f,
            guideBuildObj.transform.rotation, snappingLayerMask);

        if(nearbyBuildings.Length > 0) {
            //find the closest building near the cursor pointer
            Collider closestBuilding = FindClosestCollider(worldPos, nearbyBuildings);

            //first, test whether object should be placed above an exsisting one
            if(PointingAtBuilding(out _, out RaycastHit hitInfo) && CheckBuildAbove(hitInfo.normal)) {
                worldPos = closestBuilding.transform.position;
                worldPos.y += closestBuilding.GetComponent<BuildingObject>().BoundsExtent.y + selectedBuildObjBoundsExtent.y;
            }
            else {
                //calculate the angle between both points
                Vector3 offset = closestBuilding.transform.position - worldPos;
                float angle = (Mathf.Atan2(offset.z, offset.x)) * Mathf.Rad2Deg + 180;

                //set the world position as the nearby building, with the current objects height
                worldPos = closestBuilding.transform.position;
                worldPos.y += selectedBuildObjBoundsExtent.y - closestBuilding.GetComponent<BuildingObject>().BoundsExtent.y;

                //add offset in each direction using the angle calculated
                if ((angle >= 0 && angle <= 45) || (angle > 305 && angle < 360)) {
                    worldPos.x += closestBuilding.GetComponent<BuildingObject>().BoundsExtent.x + selectedBuildObjBoundsExtent.x;
                }
                else if (angle >= 45 && angle <= 135) {
                    worldPos.z += closestBuilding.GetComponent<BuildingObject>().BoundsExtent.z + selectedBuildObjBoundsExtent.z;
                }
                else if (angle >= 135 && angle <= 225) {
                    worldPos.x -= closestBuilding.GetComponent<BuildingObject>().BoundsExtent.x + selectedBuildObjBoundsExtent.x;
                }
                else if (angle >= 225 && angle <= 305) {
                    worldPos.z -= closestBuilding.GetComponent<BuildingObject>().BoundsExtent.z + selectedBuildObjBoundsExtent.z;
                }
            }
            //update guide object's positon
            guideBuildObj.transform.position = worldPos;
        }
    }

    private bool CheckBuildAbove(Vector3 hitNormal)
    {
        float upAngle = Vector3.Dot(Vector3.up, hitNormal.normalized);
        return upAngle > 0.6f;
    }

    private bool CheckBuildValid()
    {
        Bounds colliderBounds = guideBuildObj.GetComponent<Collider>().bounds ;

        if(Physics.CheckBox(colliderBounds.center, colliderBounds.extents - Vector3.one * 0.01f,
            guideBuildObj.transform.rotation, buildingLayerMask)) {
            return false;
        }

        return true;
    }

    private void BuildObj()
    {
        GameObject g = Instantiate(selectedBuildObj, guideBuildObj.transform.position, guideBuildObj.transform.rotation);
        g.transform.localScale = guideBuildObj.transform.localScale;
        g.layer = LayerMask.NameToLayer("building");
        g.GetComponent<BuildingObject>().BoundsExtent = selectedBuildObjBoundsExtent;
        g.GetComponent<BuildingObject>().rotationalOffset = currentRotOffset;
    }

    private void BuildEditingObj()
    {
        GameObject g = Instantiate(guideBuildObj, guideBuildObj.transform.position, guideBuildObj.transform.rotation);
        g.transform.localScale = guideBuildObj.transform.localScale;
        g.layer = LayerMask.NameToLayer("building");
        g.GetComponent<BuildingObject>().BoundsExtent = selectedBuildObjBoundsExtent;
        g.GetComponent<BuildingObject>().rotationalOffset = currentRotOffset;
        g.GetComponent<Renderer>().material = regularBuildObjMat;

        ClearCurrentSelectedObjs();
        curBuildState = BuildState.edit;
        StartCoroutine(DelayChangeEditBool());
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

    public bool BuildingHit(out RaycastHit hitPos)
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitPos, 1000.0f, buildingLayerMask) && !HitUI()) {
            return true;
           }
        return false;
    }

    public bool PointingAtBuilding(out GameObject building, out RaycastHit hitPoint)
    {
        building = null;

        if (BuildingHit(out hitPoint)) {
            building = hitPoint.collider.gameObject;
            return true;
        }
        else {
            return false;
        }
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

    private void ActivateController()
    {
        buildingModeText.text = "Building";
        curBuildState = BuildState.building;
    }

    private void DisableController()
    {
        buildingModeText.text = "Viewing";
        curBuildState = BuildState.none;
        if (guideBuildObj != null) { Destroy(guideBuildObj); }
    }

    public void BUTTON_ChangeBuildingObject(int _index)
    {
        //update button's ui to show selected, also check index valid
        if (!ManageButtonsUI(_index)) { return; }

        //destroys previous guide objects
        ClearCurrentSelectedObjs();

        //a none build object button selected (none, edit, destory)
        if (_index != (int)ButtonType.BuildingObject && Enum.IsDefined(typeof(ButtonType), _index)) {
            AlternateBuildSettings(_index);
            return;
        }

        curBuildState = BuildState.building;

        //find the game object from the id
        selectedBuildObj = AllBuildObjects.Find(x => x.GetComponent<BuildingObject>().obj_Id == _index);
        guideBuildObj = Instantiate(selectedBuildObj, Vector3.zero, Quaternion.identity);

        selectedBuildObjBoundsExtent = guideBuildObj.GetComponent<Collider>().bounds.extents;
    }

    private void AlternateBuildSettings(int _index)
    {
        ClearBuildingAdjustments();

        if(_index == (int)ButtonType.None) {
            curBuildState = BuildState.none;
        }
        else if (_index == (int)ButtonType.Edit) {
            curBuildState = BuildState.edit;
        }
        else if (_index == (int)ButtonType.Delete) {
            curBuildState = BuildState.destroy;
        }
        else if (_index == (int)ButtonType.CastleWall) {
            curBuildState = BuildState.wallBuilding;
            guideBuildObj = castleBuilder.CreateWall(Vector3.zero, 1.0f); ;
            selectedBuildObjBoundsExtent = guideBuildObj.GetComponent<Collider>().bounds.extents;
            return;
        }
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

    IEnumerator DelayChangeEditBool()
    {
        for (int i = 0; i < 3; i++) {
            yield return new WaitForEndOfFrame();
        }
        currentlyEditingObj = false;
    }

}
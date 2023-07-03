using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildController : MonoBehaviour
{
    [Header("Building Settings")]
    public LayerMask buildingLayerMask;

    public GameObject debugPrefab;

    [Header("References")]
    public TMPro.TextMeshProUGUI buildingModeText;
    public List<UI_BuildObj> buildObjButtons;
    private Camera playerCamera;

    private bool currentlyBuilding;
    
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
                Instantiate(debugPrefab, pos, Quaternion.identity); 
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
        UI_BuildObj selectedBuildObj = null;
        foreach (var button in buildObjButtons)
        {
            if (button.index == _index) {
                selectedBuildObj = button;
            }
            else { button.UnSelect(); }
        }
        selectedBuildObj.Select();


    }

}
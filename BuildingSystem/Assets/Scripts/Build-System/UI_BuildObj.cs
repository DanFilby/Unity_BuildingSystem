using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildObj : MonoBehaviour
{
    public enum ButtonType { BuildingObject = 1, CastleWall = 20, None = 90, Edit = 91, Delete = 92,  }

    public ButtonType buttonType;

    [Header("References")]
    private BuildController buildController;
    public BuildingObject buildObj;

    [Header("UI References")]
    public Image selectedImg;

    private void Start()
    {
        //setup button on click event to call the build controller with the objects id
        buildController = FindObjectOfType<BuildController>();
        Debug.Log("Made it");

        GetComponent<Button>().onClick.AddListener(delegate { buildController.BUTTON_ChangeBuildingObject(GetId()); });
    }

    public int GetId()
    {
        if(buttonType == ButtonType.BuildingObject) {
            return buildObj.obj_Id;
        }
        return (int)buttonType;
    }


    public void ShowUnSelected()
    {
        selectedImg.enabled = false;
    }

    public void ShowSelected()
    {
        selectedImg.enabled = true;
    }

}

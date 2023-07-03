using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildObj : MonoBehaviour
{
    [Header("References")]
    private BuildController buildController;
    public BuildingObject buildObj;

    [Header("UI References")]
    public Image selectedImg;

    public int ID { get { return (buildObj != null) ? buildObj.obj_Id : 0; } }

    private void Start()
    {
        //setup button on click event to call the build controller with the objects id
        buildController = FindObjectOfType<BuildController>();

        if(buildObj != null) {
            GetComponent<Button>().onClick.AddListener(delegate { buildController.BUTTON_ChangeBuildingObject(ID); });
        }
        else {
            GetComponent<Button>().onClick.AddListener(delegate { buildController.BUTTON_ChangeBuildingObject(0); });
        }
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

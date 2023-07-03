using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildController : MonoBehaviour
{
    [Header("References")]
    public TMPro.TextMeshProUGUI buildingModeText;

    private bool currentlyBuilding;
    
    //custom property normal get, setting the buildmode calls activate or disable buildmode functions 
    public bool CurrentlyBuilding
    {
        get { return currentlyBuilding; }
        set { currentlyBuilding = value;
            if (currentlyBuilding) { ActivateBuildMode(); }
            else { DisableBuildMode(); }
        }
    }


    void Start()
    {
        currentlyBuilding = false;
    }

    void Update()
    {
        

    }

    private void ActivateBuildMode()
    {
        buildingModeText.text = "Building";


    }

    private void DisableBuildMode()
    {
        buildingModeText.text = "Viewing";

    }

}

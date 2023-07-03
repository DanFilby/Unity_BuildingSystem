using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildObj : MonoBehaviour
{
    public Image selectedImg;
    public int index;

    public void UnSelect()
    {
        selectedImg.enabled = false;
    }

    public void Select()
    {
        selectedImg.enabled = true;
    }

}

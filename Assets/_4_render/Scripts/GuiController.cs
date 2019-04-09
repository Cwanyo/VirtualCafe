using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GuiController : MonoBehaviour {

    [Header("Wall")]
    public GameObject m_guiWall;
    public GameObject m_wallController;
    private bool guiwall = false;

    [Header("Object")]
    public GameObject m_guiObject;
    public GameObject m_objectController;
    private bool guiobject = false;

    [Header("ObjectDectection")]
    public GameObject m_guiObjectDectection;
    public GameObject m_objectDectectionController;
    public MeshRenderer m_pointCloudMeshRenderer;
    public GameObject m_showLog;
    private bool guiObjectDectection = false;


    public void Start () {
    }

    public void ShowWall()
    {
        m_guiWall.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        guiwall = true;
        m_wallController.SetActive(guiwall);
        HideObject();
    }

    public void HideWall()
    {
        m_guiWall.GetComponent<RectTransform>().anchoredPosition = new Vector2(200, 0);
        guiwall = false;
        m_wallController.SetActive(guiwall);
    }

    public void ShowObject()
    {
        m_guiObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        guiobject = true;
        m_objectController.SetActive(guiobject);
        HideWall();
    }

    public void HideObject()
    {
        m_guiObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, 0);
        guiobject = false;
        m_objectController.SetActive(guiobject);
    }

    public void ShowObjectDetection()
    {
        m_guiObjectDectection.GetComponent<RectTransform>().anchoredPosition = new Vector2(150, 170);
        guiObjectDectection = true;
        m_objectDectectionController.SetActive(guiObjectDectection);
        m_pointCloudMeshRenderer.enabled = guiObjectDectection;
        m_showLog.SetActive(guiObjectDectection);
    }

    public void HideObjectDetection()
    {
        m_guiObjectDectection.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, 170);
        guiObjectDectection = false;
        m_objectDectectionController.SetActive(guiObjectDectection);
        m_pointCloudMeshRenderer.enabled = guiObjectDectection;
        m_showLog.SetActive(guiObjectDectection);
    }
    
    public void Button_Wall()
    {
        if (guiwall)
        {
            HideWall();
        }
        else
        {
            ShowWall();
        }
    }

    public void Button_Object()
    {
        if (guiobject)
        {
            HideObject();
        }
        else
        {
            ShowObject();
        }
    }


    public void Button_ObjectDectection()
    {
        if (guiObjectDectection)
        {
            HideObjectDetection();
        }
        else
        {
            ShowObjectDetection();
        }
    }
}

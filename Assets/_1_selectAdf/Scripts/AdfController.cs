using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Tango;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdfController : MonoBehaviour, ITangoLifecycle {

    [Header("UI Elements")]
    public GameObject m_guiSelectAdf;
    public Button m_loadButton;

    [Header("Area Description Loader")]
    public GameObject m_listElement;
    public RectTransform m_listContentParent;
    public ToggleGroup m_toggleGroup;

    //Tango Manager
    private TangoApplication m_tangoApplication;
    private string m_savedUUID;


    //Save file path
    private string m_meshSavePath;



    public void Start () {
        //Create folder if needed
        m_meshSavePath = Application.persistentDataPath + "/PointData";
        Directory.CreateDirectory(m_meshSavePath);
        Debug.Log(m_meshSavePath);

        m_tangoApplication = FindObjectOfType<TangoApplication>();

        if (m_tangoApplication != null)
        {
            m_tangoApplication.Register(this);
            if (AndroidHelper.IsTangoCorePresent())
            {
                m_tangoApplication.RequestPermissions();
            }
        }
        else
        {
            Debug.Log("No Tango Manager found in scene.");
        }
    }
	
	public void Update () {
        if (Input.GetKey(KeyCode.Escape))
        {
            AndroidHelper.AndroidQuit();
        }
    }

    public void OnDestroy()
    {
        Debug.Log("OnDestroy 1_selectAdf");
        if (m_tangoApplication != null)
        {
            m_tangoApplication.Unregister(this);
        }
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
        if (permissionsGranted)
        {
            Debug.Log("granted");
            _PopulateAreaDescriptionUIList();
        }
        else
        {
            AndroidHelper.ShowAndroidToastMessage("Permissions Needed");
            Application.Quit();
        }
    }

    public void OnTangoServiceConnected()
    {
    }

    public void OnTangoServiceDisconnected()
    {
    }

    private void _PopulateAreaDescriptionUIList()
    {
        // Load Area Descriptions.
        foreach (Transform t in m_listContentParent.transform)
        {
            Destroy(t.gameObject);
        }
        // Update Tango space Area Description list.
        AreaDescription[] areaDescriptionList = AreaDescription.GetList();
        if (areaDescriptionList == null)
        {
            return;
        }

        foreach (AreaDescription areaDescription in areaDescriptionList)
        {
            GameObject newElement = Instantiate(m_listElement) as GameObject;
            MeshOcclusionAreaDescriptionListElement listElement = newElement.GetComponent<MeshOcclusionAreaDescriptionListElement>();
            listElement.m_toggle.group = m_toggleGroup;
            listElement.m_areaDescriptionName.text = areaDescription.GetMetadata().m_name;
            listElement.m_areaDescriptionUUID.text = areaDescription.m_uuid;
            // Check if there is an associated Area Description mesh.
            bool hasPointData = File.Exists(m_meshSavePath + "/" + areaDescription.m_uuid + ".xml") ? true : false;
            listElement.m_hasMeshData.gameObject.SetActive(hasPointData);
            // Ensure the lambda makes a copy of areaDescription.
            AreaDescription lambdaParam = areaDescription;
            listElement.m_toggle.onValueChanged.AddListener((value) => _OnToggleChanged(lambdaParam, value));
            newElement.transform.SetParent(m_listContentParent.transform, false);
        }
        Debug.Log("ADF : done PopulateAreaDescriptionUIList");
    }

    private void _OnToggleChanged(AreaDescription item, bool value)
    {
        if (value)
        {
            m_savedUUID = item.m_uuid;
            if (File.Exists(m_meshSavePath + "/" + item.m_uuid + ".xml"))
            {
                m_loadButton.interactable = true;
            }
            else
            {
                m_loadButton.interactable = false;
            }
        }
        else
        {
            m_savedUUID = null;
            m_loadButton.interactable = false;
        }
    }

    public void Button_CreateNew()
    {
        //Dismiss Room Picker list
        m_guiSelectAdf.SetActive(false);
        m_tangoApplication.Unregister(this);

        //Load scene
        SceneManager.LoadScene("2_ScanRoom");
    }

    public void Button_Load()
    {
        //Dismiss Room Picker list
        m_guiSelectAdf.SetActive(false);
        m_tangoApplication.Unregister(this);

        AdfData.uuid = m_savedUUID;
        //Load scene
        SceneManager.LoadScene("4_Render");
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Tango;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomController : MonoBehaviour, ITangoLifecycle, ITangoPose
{

    [Header("UI Elements")]
    public GameObject m_relocalizeImage;

    [Header("Object Prefabs")]
    public GameObject m_gameObjectCorner;
    public GameObject m_gameObjectFloor;
    public GameObject m_gameObjectCeiling;

    [Header("Room Render")]
    public RoomRender roomRender;

    private TangoPointCloud m_pointCloud;
    private TangoApplication m_tangoApplication;

    private bool m_initialized = false;
    private string m_meshSavePath;

    private List<GameObject> v_roomVertices = new List<GameObject>();

    public void Start()
    {
        m_meshSavePath = Application.persistentDataPath + "/PointData";
        m_relocalizeImage.SetActive(true);
        m_tangoApplication = FindObjectOfType<TangoApplication>();
        m_pointCloud = FindObjectOfType<TangoPointCloud>();

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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("1_SelectAdf");
        }
    }

    public void OnDestroy()
    {
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
            m_tangoApplication.m_areaDescriptionLearningMode = false;
            m_tangoApplication.Startup(AreaDescription.ForUUID(AdfData.uuid));
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

    public void OnTangoPoseAvailable(TangoPoseData poseData)
    {
        if (poseData.framePair.baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION &&
        poseData.framePair.targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE &&
        poseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
        {
            //Debug.Log("loop closure is detected");
            if (!m_initialized)
            {
                Debug.Log("initialized");
                m_initialized = true;
                m_relocalizeImage.SetActive(false);
                _LoadMarker();
                roomRender.v_roomVertices = v_roomVertices;
                roomRender.enabled = true;
            }
        }
    }

    private void _LoadMarker()
    {
        string path = m_meshSavePath + "/" + AdfData.uuid + ".xml";

        XmlSerializer serializer = new XmlSerializer(typeof(List<MarkerData>));
        FileStream stream = new FileStream(path, FileMode.Open);

        List<MarkerData> xmlDataList = serializer.Deserialize(stream) as List<MarkerData>;

        if (xmlDataList == null)
        {
            Debug.Log("AndroidInGameController._LoadMarkerFromDisk(): xmlDataList is null");
            return;
        }

        foreach (MarkerData m in xmlDataList)
        {
            GameObject temp = null;
            
            if (m.m_type == -1)
            {
                temp = Instantiate(m_gameObjectCorner, m.m_position, m.m_orientation) as GameObject;
            }
            else if (m.m_type == -2)
            {
                temp = Instantiate(m_gameObjectFloor, m.m_position, m.m_orientation) as GameObject;
            }
            else if (m.m_type == -3)
            {
                temp = Instantiate(m_gameObjectCeiling, m.m_position, m.m_orientation) as GameObject;
            }

            temp.tag = "marker";
            v_roomVertices.Add(temp);

        }
    }
}

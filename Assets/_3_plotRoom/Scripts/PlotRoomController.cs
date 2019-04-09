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

public class PlotRoomController : MonoBehaviour, ITangoLifecycle, ITangoDepth, ITangoPose
{

    [Header("UI Elements")]
    public GameObject m_guiMarkCorner;
    public GameObject m_guiMarkFloor;
    public GameObject m_guiMarkCeiling;
    public GameObject m_relocalizeImage;
    public Text m_showPoint;

    [Header("Object Prefabs")]
    public GameObject m_gameObjectCorner;
    public GameObject m_gameObjectFloor;
    public GameObject m_gameObjectCeiling;

    private TangoARPoseController m_arPoseController;
    private TangoPointCloud m_pointCloud;
    private TangoApplication m_tangoApplication;

    private string cur_uuid;
    private List<GameObject> v_roomVertices = new List<GameObject>();

    //Stage
    private bool m_initialized = false;
    private int max_corner = 4;
    private bool done_corner = false;
    private bool done_floor = false;
    private bool done_ceiling = false;

    private bool m_findPointwaitingForDepth;

    public void Start()
    {
        m_relocalizeImage.SetActive(true);
        m_tangoApplication = FindObjectOfType<TangoApplication>();
        m_pointCloud = FindObjectOfType<TangoPointCloud>();
        m_arPoseController = FindObjectOfType<TangoARPoseController>();

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
        if (Input.GetKey(KeyCode.Escape))
        {
            AndroidHelper.AndroidQuit();
        }
        m_showPoint.text = "Points : " + v_roomVertices.Count;
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
            AreaDescription[] list = AreaDescription.GetList();
            AreaDescription mostRecent = null;
            AreaDescription.Metadata mostRecentMetadata = null;
            if (list.Length > 0)
            {
                // Find and load the most recent Area Description
                mostRecent = list[0];
                mostRecentMetadata = mostRecent.GetMetadata();
                foreach (AreaDescription areaDescription in list)
                {
                    AreaDescription.Metadata metadata = areaDescription.GetMetadata();
                    if (metadata.m_dateTime > mostRecentMetadata.m_dateTime)
                    {
                        mostRecent = areaDescription;
                        mostRecentMetadata = metadata;
                    }
                }

                Debug.Log("Loaded : UUID=" + mostRecent.m_uuid+" Name="+ mostRecentMetadata.m_name);

                cur_uuid = mostRecent.m_uuid;
                m_tangoApplication.m_areaDescriptionLearningMode = false;
                m_tangoApplication.Startup(mostRecent);
            }
            else
            {
                // No Area Descriptions available.
                Debug.Log("No area descriptions available.");
            }
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

    public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
    {
        m_findPointwaitingForDepth = false;
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
                m_guiMarkCorner.SetActive(true);
            }
        }
    }

    private IEnumerator _WaitForDepthForPoint(Vector2 touchPosition)
    {
        m_findPointwaitingForDepth = true;

        m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);

        while (m_findPointwaitingForDepth)
        {
            yield return null;
        }

        m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);

        Camera cam = Camera.main;
        int pointIndex = m_pointCloud.FindClosestPoint(cam, touchPosition, 5);

        if(pointIndex > -1)
        {
            Debug.Log("found pount");
            Vector3 tempPoint = m_pointCloud.m_points[pointIndex];

            if (!done_corner)
            {
                //Corner
                GameObject gtemp = Instantiate(m_gameObjectCorner, tempPoint, new Quaternion(0, 0, 0, 0)) as GameObject;
                gtemp.name = "point" + v_roomVertices.Count;
                UpdatePointMarker(gtemp, -1);
                v_roomVertices.Add(gtemp);
                Debug.Log("Status : added at" + tempPoint);

                /*if(v_roomVertices.Count == max_corner)
                {
                    done_corner = true;
                    m_guiMarkCorner.SetActive(false);
                    m_guiMarkFloor.SetActive(true);
                    Debug.Log("Status : done corner");
                }*/
            }else if(done_corner && (!done_floor))
            {
                //Floor
                if (GameObject.Find("pointfloor") != null)
                {
                    Destroy(GameObject.Find("pointfloor"));
                    v_roomVertices.RemoveAt(v_roomVertices.Count - 1);
                }
                GameObject gtemp = Instantiate(m_gameObjectFloor, tempPoint, new Quaternion(0, 0, 0, 0)) as GameObject;
                gtemp.name = "pointfloor";
                UpdatePointMarker(gtemp, -2);
                v_roomVertices.Add(gtemp);
                Debug.Log("Status : floor at" + tempPoint);
            } else if(done_corner && done_floor && (!done_ceiling)){
                //Ceiling
                if (GameObject.Find("pointceiling") != null)
                {
                    Destroy(GameObject.Find("pointceiling"));
                    v_roomVertices.RemoveAt(v_roomVertices.Count - 1);
                }
                GameObject gtemp = Instantiate(m_gameObjectCeiling, tempPoint, new Quaternion(0, 0, 0, 0)) as GameObject;
                gtemp.name = "pointceiling";
                UpdatePointMarker(gtemp, -3);
                v_roomVertices.Add(gtemp);
                Debug.Log("Status : ceiling at" + tempPoint);
            }
            
        }

    }

    public void UpdatePointMarker(GameObject pointObj, int type)
    {
        PointMarker pmScript = pointObj.GetComponent<PointMarker>();
        pmScript.m_type = type;
        pmScript.m_timestamp = (float)m_arPoseController.m_poseTimestamp;
        Matrix4x4 uwTDevice = Matrix4x4.TRS(m_arPoseController.m_tangoPosition, m_arPoseController.m_tangoRotation, Vector3.one);
        Matrix4x4 uwTMarker = Matrix4x4.TRS(pointObj.transform.position, pointObj.transform.rotation, Vector3.one);
        pmScript.m_deviceTMaker = Matrix4x4.Inverse(uwTDevice) * uwTMarker;
    }

    public void _SaveObjectMarkerToDisk()
    {
        List<MarkerData> xmlDataList = new List<MarkerData>();

        foreach (GameObject obj in v_roomVertices)
        {
            MarkerData temp = new MarkerData();
            temp.m_type = obj.GetComponent<PointMarker>().m_type;
            temp.m_position = obj.transform.position;
            temp.m_orientation = obj.transform.rotation;
            xmlDataList.Add(temp);
        }
        string path = Application.persistentDataPath + "/PointData/" + cur_uuid + ".xml";
        XmlSerializer serializer = new XmlSerializer(typeof(List<MarkerData>));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, xmlDataList);
        }
        Debug.Log("saved");
    }

    public void Button_Reset()
    {
        m_tangoApplication.Unregister(this);
        SceneManager.LoadScene("2_ScanRoom");
    }

    public void Button_Mark()
    {
        StartCoroutine(_WaitForDepthForPoint(new Vector2(Screen.width / 2, Screen.height / 2)));
    }

    public void Button_UndoCorner()
    {
        if(v_roomVertices.Count > 0)
        {
            //remove red point
            Destroy(GameObject.Find("point" + (v_roomVertices.Count - 1)));
            v_roomVertices.RemoveAt(v_roomVertices.Count - 1);
            Debug.Log("Status : undo");
        }
        else
        {
            Debug.Log("Status : empty");
        }
    }

    public void Button_DoneCorner()
    {
        done_corner = true;
        m_guiMarkCorner.SetActive(false);
        m_guiMarkFloor.SetActive(true);
        Debug.Log("Status : done corner");
    }

    public void Button_DoneFloor()
    {
        done_floor = true;
        m_guiMarkFloor.SetActive(false);
        m_guiMarkCeiling.SetActive(true);
        Debug.Log("Status : done floor");
    }

    public void Button_DoneCeiling()
    {
        done_ceiling = true;
        m_guiMarkCeiling.SetActive(false);
        Debug.Log("Status : done ceiling");

        //Write XML
        _SaveObjectMarkerToDisk();

        m_tangoApplication.Unregister(this);
        SceneManager.LoadScene("1_SelectAdf");
    }
}

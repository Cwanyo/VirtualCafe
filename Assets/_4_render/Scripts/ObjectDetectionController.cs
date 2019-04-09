using Tango;
using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine.UI;

public class ObjectDetectionController : MonoBehaviour, ITangoLifecycle, ITangoPointCloud
{

    private TangoApplication m_tangoApplication;
    private TangoDeltaPoseController m_tangoPoseController;

    private int m_pointsCount = 0;

    private Boolean capture = false;

    string folderpath;

    private List<Vector3> points = new List<Vector3>();

    private static List<Scene> sceneQueue = new List<Scene>();

    private Thread networkThread;
    private int maxConnctionAttmp = 5;
    private static int ConnectionAttmp = 0;

    //connection
    private string hostIP = null;
    private int port = -1;

    private string UI_FLOAT_FORMAT = "F3";

    [Header("Object")]
    public GameObject m_phoneBox;

    //
    private Ray ray;
    private RaycastHit hit;

    [Header("Effect")]
    public GameObject m_fireEffect;

    public Text m_showLog;
    private string log_mess = "";

    public void Start()
    {

        folderpath = Application.persistentDataPath + "/tempScenes";
        networkThread = new Thread(SocketConnection);
        if (!Directory.Exists(folderpath))
        {
            Debug.Log("wvn : created folder");
            Directory.CreateDirectory(folderpath);
        }

        m_tangoPoseController = FindObjectOfType<TangoDeltaPoseController>();
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
    public void Update()
    {
        //Update log
        m_showLog.text = log_mess;

        //check thread running or not

        if (sceneQueue.Count > 0 && (ConnectionAttmp != maxConnctionAttmp))
        {
            if (sceneQueue[0].status == 1)
            {
                //then use the result

                DisplayObject(sceneQueue[0]);
                //delete temp file
                File.Delete(sceneQueue[0].filePath);
                //and pop out
                sceneQueue.RemoveAt(0);
                ConnectionAttmp = 0;
            }
            else if (sceneQueue[0].status == 0 && (networkThread.ThreadState != ThreadState.Running))
            {
                //then make a connection
                //Debug.Log(String.Format("wvn : net create new thread at %s::%d ",hostIP,port));
                if (hostIP != null && port != -1)
                {
                    Debug.Log("wvn : net create new thread at " + hostIP + "::" + port + " #" + ConnectionAttmp);
                    log_mess = "wvn : net create new thread at " + hostIP + "::" + port+" #"+ ConnectionAttmp;
                    networkThread.Abort();
                    networkThread = new Thread(SocketConnection);
                    networkThread.Start();
                }
                else
                {
                    Debug.Log("wvn : net did not input the host ip and port");
                    log_mess = "wvn : net did not input the host ip and port";
                }

            }
        }
        else if (ConnectionAttmp == maxConnctionAttmp)
        {
            Debug.Log("wvn : net give up connection and clear queue");
            log_mess = "wvn : net give up connection and clear queue";
            DeleteTempFile();
            sceneQueue.Clear();
            ConnectionAttmp = 0;
        }


        // TODO - add object effect
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(1))
#elif UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
        {
#if UNITY_EDITOR
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#elif UNITY_ANDROID
            ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#endif
            if (Physics.Raycast(ray, out hit, Mathf.Infinity) && hit.collider.name == "P2PRO_JOIN")
            {
                Debug.Log("Hit : " + hit.collider.name);
                Debug.Log("Hit : " + hit.collider.transform.parent.name);
                GameObject fire = Instantiate(m_fireEffect, hit.collider.transform.position, hit.collider.transform.rotation) as GameObject;
                fire.transform.parent = hit.collider.transform;
            }
        }
    }

    public void DisplayObject(Scene s)
    {
        if (s.output == null)
        {
            Debug.Log("wvn : object not found");
            log_mess = "wvn : object not found";
            return;
        }
        Debug.Log("wvn : object found");
        log_mess = "wvn : object found";
        Debug.Log(s.output.DR.ToString("F4"));
        Debug.Log(s.output.OR.ToString("F4"));
        Debug.Log(s.output.SC.ToString("F4"));
        Debug.Log(s.output.DCO.ToString("F4"));
        Debug.Log(s.output.ICP.ToString("F4"));
        GameObject vDevice = null;
        GameObject dObject = null;
        if (s.output.type == 1)
        {
            GameObject g = GameObject.FindWithTag("phoneBox");
            if (g != null)
            {
                Debug.Log("wvn : remove phonebox");
                Destroy(g);
            }
            vDevice = new GameObject("vDeive");
            vDevice.tag = "phoneBox";
            dObject = Instantiate(m_phoneBox) as GameObject;
            dObject.tag = "object";
            //put object in vis device
            dObject.transform.parent = vDevice.transform;
        }
        /*
         * FIND OBJECT POSITION
         * position = scenePclCentroid + (offset databaseUnityCentroid - databasePclCentroid)
         * invert x(position)
         */
        dObject.transform.localPosition = new Vector3((s.output.SC.x + s.output.DCO.x) * -1, s.output.SC.y + s.output.DCO.y, s.output.SC.z + s.output.DCO.z);
        /*
         * FIND OBJECT ROTATION
         * Invert rotation(icp)* databaseUnityRotation 
         */
        dObject.transform.localRotation = Quaternion.Inverse(Matrix2.ExtractRotationFromMatrix(ref s.output.ICP)) * s.output.OR;

        /*
         * Position and Rotation of device at capture
         */
        vDevice.transform.localPosition = s.devicePosition;
        vDevice.transform.localRotation = s.deviceRotation;

        /*
         * Inverse device rotation from database to base. care only x and z rotation
         */
        Vector3 temp = s.output.DR.eulerAngles;
        temp.y = 0;
        dObject.transform.localRotation = Quaternion.Euler(temp) * dObject.transform.localRotation;

        /*
         * Align object to device rotation
         */
        Quaternion camA = Quaternion.Euler(new Vector3(vDevice.transform.localEulerAngles.x, 0, vDevice.transform.localEulerAngles.z));
        dObject.transform.localRotation = Quaternion.Inverse(camA) * dObject.transform.localRotation;

        string c = "OutCam -> t: " + vDevice.transform.localPosition.ToString(UI_FLOAT_FORMAT) + " r: " + vDevice.transform.localRotation.ToString(UI_FLOAT_FORMAT) + " | " + vDevice.transform.localRotation.eulerAngles.ToString(UI_FLOAT_FORMAT);
        Debug.Log(c);
        string o = "OutObj -> t: " + dObject.transform.localPosition.ToString(UI_FLOAT_FORMAT) + " r: " + dObject.transform.localRotation.ToString(UI_FLOAT_FORMAT) + " | " + dObject.transform.localRotation.eulerAngles.ToString(UI_FLOAT_FORMAT);
        Debug.Log(o);

    }

    public void Input_IpAndPort(String s)
    {
        string[] separators = { "::" };
        string[] re = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        if (re.Length == 2)
        {
            networkThread.Abort();

            hostIP = re[0];
            port = Int32.Parse(re[1]);

            Debug.Log("wvn : net change hostIp and port to" + hostIP + "::" + port.ToString());
            log_mess = "wvn : net change hostIp and port to" + hostIP + "::" + port.ToString();
        }

    }

    void SocketConnection()
    {
        NetworkController n = new NetworkController(hostIP, port, 5);

        if (!n.Connect())
        {
            Debug.Log("wvn : net fail connecting");
            log_mess = "wvn : net fail connecting";
            ConnectionAttmp++;
            return;
        }

        if (!n.SendScene(sceneQueue[0].filePath))
        {
            Debug.Log("wvn : net fail sending - " + sceneQueue[0].filePath);
            log_mess = "wvn : net fail sending - " + sceneQueue[0].filePath;
            return;
        }

        if (!n.ShutdownSend())
        {
            Debug.Log("wvn : net fail shutdownsend");
            log_mess = "wvn : net fail shutdownsend";
            return;
        }

        if (!n.ReceiveResult())
        {
            Debug.Log("wvn : net fail reveiving");
            log_mess = "wvn : net fail reveiving";
            return;
        }

        if (!n.ShutdownReceive())
        {
            Debug.Log("wvn : net fail shutdownreceive");
            log_mess = "wvn : net fail shutdownreceive";
            return;
        }

        if (!n.Close())
        {
            Debug.Log("wvn : net fail close");
            log_mess = "wvn : net fail close";
            return;
        }

        //get output
        n.getOutput(ref sceneQueue[0].output);
        //success
        sceneQueue[0].status = 1;
    }

    public void DeleteTempFile()
    {
        if (sceneQueue.Count > 0)
        {
            foreach (Scene s in sceneQueue)
            {
                File.Delete(s.filePath);
            }
        }
    }

    public void OnDestroy()
    {
        networkThread.Abort();
        DeleteTempFile();
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

    public void OnTangoPointCloudAvailable(TangoPointCloudData pointCloud)
    {
        m_pointsCount = pointCloud.m_numPoints;
        if (pointCloud.m_numPoints > 0)
        {
            Vector3 devicePos = new Vector3();
            Quaternion deviceRot = new Quaternion();
            if (capture)
            {
                devicePos = m_tangoPoseController.transform.localPosition;
                deviceRot = m_tangoPoseController.transform.localRotation;
            }

            for (int i = 0; i < m_pointsCount; ++i)
            {
                Vector3 point = pointCloud[i];

                if (capture)
                {
                    points.Add(point);
                }
            }

            if (capture)
            {
                capture = false;

                string path = SaveFile(points);

                sceneQueue.Add(new Scene(path, devicePos, deviceRot));

                points.Clear();
            }



        }
    }

    private string SaveFile(List<Vector3> points)
    {
        string[] lines = new string[11];
        lines[0] = "# .PCD v0.7 - Point Cloud Data file format";
        lines[1] = "VERSION 0.7";
        lines[2] = "FIELDS x y z";
        lines[3] = "SIZE 4 4 4";
        lines[4] = "TYPE F F F";
        lines[5] = "COUNT 1 1 1";
        lines[6] = String.Format("WIDTH {0}", points.Count);
        lines[7] = "HEIGHT 1";
        lines[8] = "VIEWPOINT 0 0 0 1 0 0 0";
        lines[9] = String.Format("POINTS {0}", points.Count);
        lines[10] = "DATA ascii";

        string filename = "tmp" + DateTime.Now.ToString("ddHHmmssfff"); ;
        string path = folderpath + "/" + filename + ".pcd";
        Debug.Log("Saved at " + path);
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path))
        {
            //header
            foreach (string line in lines)
            {
                file.WriteLine(line);
            }
            //points
            foreach (Vector3 point in points)
            {
                file.WriteLine(String.Format("{0} {1} {2}", point.x, point.y, point.z));
            }
        }

        return path;
    }

    public void Button_Capture()
    {
        Debug.Log("wvn : capture");
        capture = true;
    }
}

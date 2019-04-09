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

public class AreaScanController : MonoBehaviour, ITangoLifecycle
{

    [Header("UI Elements")]
    public GameObject m_guiAreaScan;

    [Header("AreaScan Elements")]
    public Button m_ButtonDone;

    private TangoApplication m_tangoApplication;
    private AreaDescription m_curAreaDescription;

    private string m_guiTextInputContents;
    private bool m_guiTextInputResult;
    private bool m_displayGuiTextInput;
    private Thread m_saveThread;
    private bool done_save = false;

    public void Start () {
        m_guiAreaScan.SetActive(true);
        m_tangoApplication = FindObjectOfType<TangoApplication>();
        //Debug.Log(Application.persistentDataPath);
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

        if ((!done_save) && m_saveThread != null && m_saveThread.ThreadState != ThreadState.Running)
        {
            done_save = true;
            Debug.Log("Done Thread Save");

            m_guiAreaScan.SetActive(false);
            m_tangoApplication.Unregister(this);
            AdfData.uuid = m_curAreaDescription.m_uuid;

            SceneManager.LoadScene("3_PlotRoom");
        }
    }

    public void OnGUI()
    {
        if (m_displayGuiTextInput)
        {
            Rect textBoxRect = new Rect(100,
                                        Screen.height - 200,
                                        Screen.width - 200,
                                        100);

            Rect okButtonRect = textBoxRect;
            okButtonRect.y += 100;
            okButtonRect.width /= 2;

            Rect cancelButtonRect = okButtonRect;
            cancelButtonRect.x = textBoxRect.center.x;

            GUI.SetNextControlName("TextField");
            GUIStyle customTextFieldStyle = new GUIStyle(GUI.skin.textField);
            customTextFieldStyle.alignment = TextAnchor.MiddleCenter;
            m_guiTextInputContents =
                GUI.TextField(textBoxRect, m_guiTextInputContents, customTextFieldStyle);
            GUI.FocusControl("TextField");

            if (GUI.Button(okButtonRect, "OK")
                || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
            {
                m_displayGuiTextInput = false;
                m_guiTextInputResult = true;
            }
            else if (GUI.Button(cancelButtonRect, "Cancel"))
            {
                m_displayGuiTextInput = false;
                m_guiTextInputResult = false;
            }
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
            m_curAreaDescription = null;
            m_tangoApplication.m_areaDescriptionLearningMode = true;
            m_tangoApplication.Startup(m_curAreaDescription);
            m_guiAreaScan.SetActive(true);
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

    private IEnumerator _DoSaveCurrentAreaDescription()
    {
#if UNITY_EDITOR
        if (m_displayGuiTextInput || m_saveThread != null)
        {
            yield break;
        }

        m_displayGuiTextInput = true;
        m_guiTextInputContents = "Unnamed";
        while (m_displayGuiTextInput)
        {
            yield return null;
        }
        bool saveConfirmed = m_guiTextInputResult;
#else
        if (TouchScreenKeyboard.visible || m_saveThread != null)
        {
            yield break;
        }
        
        TouchScreenKeyboard kb = TouchScreenKeyboard.Open("Unnamed");
        while (!kb.done && !kb.wasCanceled)
        {
            yield return null;
        }

        bool saveConfirmed = kb.done;
#endif
        if (saveConfirmed)
        {
            if (m_tangoApplication.m_areaDescriptionLearningMode)
            {
                m_saveThread = new Thread(delegate ()
                {
                    m_curAreaDescription = AreaDescription.SaveCurrent();
                    AreaDescription.Metadata metadata = m_curAreaDescription.GetMetadata();
#if UNITY_EDITOR
                    metadata.m_name = m_guiTextInputContents;
#else
                    metadata.m_name = kb.text;
#endif
                    m_curAreaDescription.SaveMetadata(metadata);
                });
                m_saveThread.Start();
            }
        }
    }

    public void Button_Reset()
    {
        SceneManager.LoadScene("2_ScanRoom");
    }

    public void Button_Done()
    {
        StartCoroutine(_DoSaveCurrentAreaDescription());
    }

}

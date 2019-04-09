using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class ObjectController : MonoBehaviour {

    [Header("Object prefabs")]
    public GameObject [] m_object;

    [Header("UI Elements")]
    public ToggleGroup m_objectToggleGroup;

    private GameObject gObj = null;
    private Plane objPlane;
    private Vector3 m0;

	public void Update () {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 wordPos;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                wordPos = hit.point;
            }
            else
            {
                wordPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            StartCoroutine(PopObject(getCurToggle(), wordPos, Quaternion.identity));
        }
#elif UNITY_ANDROID
        if(Input.touchCount > 0 && Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector3 wordPos;
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                wordPos = hit.point;
            }
            else
            {
                wordPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            }
            StartCoroutine(PopObject(getCurToggle(), wordPos, Quaternion.identity));
        }
#endif
    }

    public GameObject getCurToggle()
    {
        string curname = m_objectToggleGroup.ActiveToggles().FirstOrDefault().name;

        for (int i = 0; i < m_object.Length; i++)
        {
            Debug.Log(m_object[i].name);
            if (curname == m_object[i].name)
            {
                return m_object[i];
            }
            Debug.Log(i);
        }
        return null;
    }

    public Ray GenerateMouseRay(Vector3 touchPos)
    {
        Vector3 mousePosFar = new Vector3(touchPos.x, touchPos.y, Camera.main.farClipPlane);
        Vector3 mousePosNear = new Vector3(touchPos.x, touchPos.y, Camera.main.nearClipPlane);

        Vector3 mousePosF = Camera.main.ScreenToWorldPoint(mousePosFar);
        Vector3 mousePosN = Camera.main.ScreenToWorldPoint(mousePosNear);

        Ray mr = new Ray(mousePosN, mousePosF - mousePosN);
        return mr;
    }

    private IEnumerator PopObject(GameObject type,Vector3 pos,Quaternion qua)
    {
        yield return new WaitForSeconds(0.1f);
        if(type.name == "box")
        {
            //type.GetComponent<Renderer>().sharedMaterial.color = new Color(Random.value, Random.value, Random.value, 1.0f);
            Material m = new Material(Shader.Find("Standard"));
            m.color = new Color(Random.value, Random.value, Random.value, 1.0f); 
            type.GetComponent<Renderer>().material = m;
        }

        Instantiate(type, pos, qua);

    }

}

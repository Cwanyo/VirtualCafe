using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class WallController : MonoBehaviour {

    [Header("Wall Textures")]
    public Material m_matInit;
    public Material [] m_mat;

    [Header("UI Elements")]
    public ToggleGroup m_wallToggleGroup;

    private Ray ray;
    private RaycastHit hit;

	public void Start () {
	}
	
	public void Update () {
#if UNITY_EDITOR
        if(Input.GetMouseButtonDown(1))
#elif UNITY_ANDROID
        if (Input.touchCount > 1 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
        {
#if UNITY_EDITOR
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#elif UNITY_ANDROID
            ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#endif
            if (Physics.Raycast(ray,out hit,Mathf.Infinity) && hit.transform.tag == "room")
            {
                Debug.Log("Hit : "+hit.collider.name);
                /*switch (getCurToggle().ToLower())
                {
                    case "none":
                        Debug.Log("change none");
                        hit.collider.GetComponent<Renderer>().material = m_matInit;
                        break;
                    case "brick":
                        Debug.Log("change brick");
                        hit.collider.GetComponent<Renderer>().material = m_matBrick;
                        break;
                }*/
               for(int i = 0; i < m_mat.Length; i++)
                {
                    if (getCurToggle().ToLower() == m_mat[i].name)
                    {
                        hit.collider.GetComponent<Renderer>().material = m_mat[i];
                        i = m_mat.Length;
                    }
                    //Debug.Log(i);
                }
            }
        }

	}

    public string getCurToggle()
    {
        return m_wallToggleGroup.ActiveToggles().FirstOrDefault().name;
    }
}

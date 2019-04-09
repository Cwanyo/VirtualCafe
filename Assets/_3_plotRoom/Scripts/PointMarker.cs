using UnityEngine;
using System.Collections;

public class PointMarker : MonoBehaviour {

    /// <summary>
    /// 
    /// corner = -1
    /// floor = -2
    /// ceiling = -3
    /// 
    /// </summary>

    public int m_type = 0;

    public float m_timestamp = -1.0f;

    public Matrix4x4 m_deviceTMaker = new Matrix4x4();


}

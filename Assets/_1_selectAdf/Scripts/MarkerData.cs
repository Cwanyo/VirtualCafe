using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

[System.Serializable]
public class MarkerData
{

    [XmlElement("type")]
    public int m_type;

    [XmlElement("position")]
    public Vector3 m_position;

    [XmlElement("orientation")]
    public Quaternion m_orientation;

}

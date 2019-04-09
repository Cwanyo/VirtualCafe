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

public class RoomRender : MonoBehaviour {

    [Header("Material")]
    public Material mat;

    private float minX = float.MaxValue;
    private float maxX = float.MinValue;
    private float minZ = float.MaxValue;
    private float maxZ = float.MinValue;

    [HideInInspector]
    public List<GameObject> v_roomVertices;

    public void Start()
    {
        ProcessWallRender();
        ProcessFloorRender();
        ProcessCeilingRender();

        RemoveMarker();
    }

    public void RemoveMarker()
    {
        GameObject[] marker = GameObject.FindGameObjectsWithTag("marker");
        for (int i = 0; i < marker.Length; i++)
        {
            Destroy(marker[i]);
        }
    }

    public void ProcessWallRender()
    {
        for (int i = 0; i < v_roomVertices.Count - 2; i++)
        {
            GameObject wall = new GameObject("wall" + i);
            wall.tag = "room";
            MeshFilter mf = wall.AddComponent(typeof(MeshFilter)) as MeshFilter;
            MeshRenderer mr = wall.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            MeshCollider mc = wall.AddComponent(typeof(MeshCollider)) as MeshCollider;

            Mesh m = new Mesh();

            if (v_roomVertices.Count - 3 == i)
            {
                m.vertices = new Vector3[]
                {
                    GetPositionYonButtom(v_roomVertices[i]),
                    GetPositionYonTop(v_roomVertices[i]),
                    GetPositionYonTop(v_roomVertices[0]),
                    GetPositionYonButtom(v_roomVertices[0]),
                };
            }
            else
            {
                m.vertices = new Vector3[]
                {
                    GetPositionYonButtom(v_roomVertices[i]),
                    GetPositionYonTop(v_roomVertices[i]),
                    GetPositionYonTop(v_roomVertices[i+1]),
                    GetPositionYonButtom(v_roomVertices[i+1]),
                };
            }

            m.uv = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0),
            };

            m.triangles = new int[]
            {
                0,1,2,0,2,3
            };

            mf.mesh = m;
            mr.material = mat;
            m.RecalculateBounds();
            m.RecalculateNormals();
            mc.sharedMesh = m;

            //find min/max X and Z for floor and ceiling
            if (minX > v_roomVertices[i].transform.position.x)
            {
                minX = v_roomVertices[i].transform.position.x;
            }
            if (minZ > v_roomVertices[i].transform.position.z)
            {
                minZ = v_roomVertices[i].transform.position.z;
            }
            if (maxX < v_roomVertices[i].transform.position.x)
            {
                maxX = v_roomVertices[i].transform.position.x;
            }
            if (maxZ < v_roomVertices[i].transform.position.z)
            {
                maxZ = v_roomVertices[i].transform.position.z;
            }
        }
    }

    public void ProcessFloorRender()
    {
        GameObject wall = new GameObject("floor");
        wall.tag = "room";
        MeshFilter mf = wall.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer mr = wall.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        MeshCollider mc = wall.AddComponent(typeof(MeshCollider)) as MeshCollider;

        Mesh m = new Mesh();

        m.vertices = new Vector3[]
        {
            new Vector3(minX,v_roomVertices[v_roomVertices.Count - 2].transform.position.y,minZ),
            new Vector3(minX,v_roomVertices[v_roomVertices.Count - 2].transform.position.y,maxZ),
            new Vector3(maxX,v_roomVertices[v_roomVertices.Count - 2].transform.position.y,maxZ),
            new Vector3(maxX,v_roomVertices[v_roomVertices.Count - 2].transform.position.y,minZ)
        };

        m.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),
        };

        m.triangles = new int[]
        {
                0,1,2,0,2,3
        };

        mf.mesh = m;
        mr.material = mat;
        m.RecalculateBounds();
        m.RecalculateNormals();
        mc.sharedMesh = m;

    }

    public void ProcessCeilingRender()
    {
        GameObject wall = new GameObject("ceiling");
        wall.tag = "room";
        MeshFilter mf = wall.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer mr = wall.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        MeshCollider mc = wall.AddComponent(typeof(MeshCollider)) as MeshCollider;

        Mesh m = new Mesh();

        m.vertices = new Vector3[]
        {
            new Vector3(minX,v_roomVertices[v_roomVertices.Count - 1].transform.position.y,minZ),
            new Vector3(maxX,v_roomVertices[v_roomVertices.Count - 1].transform.position.y,minZ),
            new Vector3(maxX,v_roomVertices[v_roomVertices.Count - 1].transform.position.y,maxZ),
            new Vector3(minX,v_roomVertices[v_roomVertices.Count - 1].transform.position.y,maxZ)
        };

        m.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),
        };

        m.triangles = new int[]
        {
                0,1,2,0,2,3
        };

        mf.mesh = m;
        mr.material = mat;
        m.RecalculateBounds();
        m.RecalculateNormals();
        mc.sharedMesh = m;

    }

    //Get position of corner at Y direction on buttom
    public Vector3 GetPositionYonButtom(GameObject v)
    {
        return new Vector3(v.transform.position.x, v_roomVertices[v_roomVertices.Count - 2].transform.position.y, v.transform.position.z);
    }

    //Get position of corner at Y direction on top
    public Vector3 GetPositionYonTop(GameObject v)
    {
        return new Vector3(v.transform.position.x, v_roomVertices[v_roomVertices.Count - 1].transform.position.y, v.transform.position.z);
    }
}

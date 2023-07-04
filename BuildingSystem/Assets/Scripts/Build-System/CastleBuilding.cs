using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class CastleBuilding : MonoBehaviour
{
    public float wallWidth = 2.0f;
    public float wallHeight = 5.0f;
    public Material wallMaterial;

    private List<Vector3> vertices;
    private List<int> triangles;
    private MeshFilter meshFilter;


    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        //CreateWall(new Vector3(0, 0, 0), new Vector3(-10, 0, -25), 1.0f);
    }

    public GameObject CreateWall(Vector3 position, float scale)
    {
        //setup game object
        GameObject wallObj = new GameObject("Castle Wall");
        MeshFilter mfilter = wallObj.AddComponent<MeshFilter>();
        wallObj.AddComponent<MeshRenderer>().material = wallMaterial;
        wallObj.layer = 8;

        //generate mesh for the wall
        mfilter.mesh = CreateWallMesh(0);
        wallObj.AddComponent<MeshCollider>();

        wallObj.transform.localScale = new Vector3(scale, scale, 1);

        return wallObj;
    }

    public GameObject CreateWall(Vector3 startPoint, Vector3 endPoint, float scale)
    {
        //setup game object
        GameObject wallObj = new GameObject("Castle Wall" + (Random.value * 1000).ToString());
        MeshFilter mfilter = wallObj.AddComponent<MeshFilter>();
        wallObj.AddComponent<MeshRenderer>().material = wallMaterial;
        wallObj.layer = 8;

        //generate mesh for the wall
        float wallLength = Vector3.Distance(startPoint, endPoint);
        mfilter.mesh = CreateWallMesh(wallLength);

        wallObj.AddComponent<MeshCollider>();

        Vector3 offset = startPoint - endPoint;
        float angle = (Mathf.Atan2(offset.x, offset.z)) * Mathf.Rad2Deg + 180;

        wallObj.transform.position = startPoint;
        wallObj.transform.localRotation = Quaternion.Euler(0, angle , 0);
        wallObj.transform.localScale = new Vector3(scale, scale, 1);

        return wallObj;
    }

    private Mesh CreateWallMesh(float wallLength)
    {
        return CreateWallMesh(new Vector3(0, 0, wallLength - wallWidth / 2));
    }

    private Mesh CreateWallMesh(Vector3 lengthMultiplier)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        float xMultiplier = wallWidth + lengthMultiplier.x;
        float yMultiplier = wallHeight + lengthMultiplier.y;
        float zMultiplier = wallWidth + lengthMultiplier.z;

        //unit cube vertices and triangles
        vertices.AddRange(new List<Vector3> {
            new Vector3(0,0,0), new Vector3(1 * xMultiplier,0,0),
            new Vector3(0,0,1 * zMultiplier), new Vector3(1 * xMultiplier,0,1 * zMultiplier),
            new Vector3(0,1 * yMultiplier,0), new Vector3(1 * xMultiplier,1 * yMultiplier,0),
            new Vector3(0,1 * yMultiplier,1 * zMultiplier), new Vector3(1 * xMultiplier,1 * yMultiplier,1 * zMultiplier),
        });
        triangles.AddRange(new List<int>{
            0,1,2, 1,3,2, //bot face
            5,4,6, 7,5,6, //top face
            1,0,4, 5,1,4, //right face
            0,2,4, 2,6,4, //left face
            3,1,5, 7,3,5, //far right face
            2,3,6, 3,7,6, //far left face
        });

        //centre the mesh 
        Vector3 centering = new Vector3(wallWidth / 2.0f, wallHeight / 2.0f, wallWidth / 2.0f);
        for (int i = 0; i < vertices.Count; i++) {
            vertices[i] -= centering;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();   
        mesh.RecalculateBounds();

        return mesh;
    }

}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleBuilding : MonoBehaviour
{
    public float wallWidth = 2.0f;
    public Material wallMaterial;


    private List<Vector3> vertices;
    private List<int> triangles;
    private MeshFilter meshFilter;



    void Start()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        meshFilter = GetComponent<MeshFilter>();

        CreateWall(new Vector3(0, 0, 0), new Vector3(-10, 0, -25));

    }

    public void CreateWall(Vector3 startPoint, Vector3 endPoint)
    {
        //setup game object
        GameObject wallObj = new GameObject();
        MeshFilter mfilter = wallObj.AddComponent<MeshFilter>();
        wallObj.AddComponent<MeshRenderer>().material = wallMaterial;

        //generate mesh for the wall
        float wallLength = Vector3.Distance(startPoint, endPoint);
        mfilter.mesh = CreateWallMesh(wallLength);

        wallObj.AddComponent<BoxCollider>();

        Vector3 offset = startPoint - endPoint;
        float angle = (Mathf.Atan2(offset.x, offset.z)) * Mathf.Rad2Deg + 180;

        wallObj.transform.localRotation = Quaternion.Euler(0, angle , 0);
    }

    private Mesh CreateWallMesh(float wallLength)
    {
        return CreateWallMesh(new Vector3(0, 0, wallLength - wallWidth / 2));
    }

    private Mesh CreateWallMesh(Vector3 lengthMultiplier)
    {
        float xMultiplier = wallWidth + lengthMultiplier.x;
        float yMultiplier = 5.0f + lengthMultiplier.y;
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
        for (int i = 0; i < vertices.Count; i++) {
            vertices[i] -= Vector3.one * (wallWidth / 2.0f);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();   
        mesh.RecalculateBounds();

        return mesh;
    }

}



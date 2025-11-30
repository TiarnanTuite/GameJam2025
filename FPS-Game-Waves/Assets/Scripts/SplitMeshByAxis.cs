using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplitMeshByAxis : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis axis = Axis.X;
    public float splitPosition = 0f; // local-space position to split at

    [ContextMenu("Split Mesh By Axis")]
    void Split()
    {
        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();
        var mesh = mf.sharedMesh;
        if (mesh == null)
        {
            Debug.LogWarning("No mesh to split.");
            return;
        }

        if (!mesh.isReadable)
        {
            Debug.LogWarning("Mesh is not readable. Enable Read/Write in model import settings.");
            return;
        }

        var vertices = mesh.vertices;
        var normals  = mesh.normals;
        var uvs      = mesh.uv;
        var triangles = mesh.triangles;

        List<int> triLeft = new List<int>();
        List<int> triRight = new List<int>();

        // helper to pick axis component
        System.Func<Vector3, float> axisValue = v =>
        {
            switch (axis)
            {
                case Axis.X: return v.x;
                case Axis.Y: return v.y;
                default:     return v.z;
            }
        };

        // split triangles by triangle centroid in local space
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i + 0];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            Vector3 c = (vertices[i0] + vertices[i1] + vertices[i2]) / 3f;
            if (axisValue(c) <= splitPosition)
            {
                triLeft.Add(i0); triLeft.Add(i1); triLeft.Add(i2);
            }
            else
            {
                triRight.Add(i0); triRight.Add(i1); triRight.Add(i2);
            }
        }

        if (triLeft.Count == 0 || triRight.Count == 0)
        {
            Debug.LogWarning("Split resulted in an empty side. Adjust splitPosition or axis.");
            return;
        }

        CreatePart("part_left", triLeft, vertices, normals, uvs, mr.sharedMaterial);
        CreatePart("part_right", triRight, vertices, normals, uvs, mr.sharedMaterial);

        gameObject.SetActive(false);
        Debug.Log("Mesh split complete.");
    }

    void CreatePart(string nameSuffix, List<int> tris, Vector3[] srcVerts, Vector3[] srcNormals, Vector2[] srcUVs, Material mat)
    {
        var newGO = new GameObject(gameObject.name + "_" + nameSuffix);
        newGO.transform.SetParent(transform.parent, worldPositionStays: false);
        newGO.transform.localPosition = transform.localPosition;
        newGO.transform.localRotation = transform.localRotation;
        newGO.transform.localScale = transform.localScale;

        var mf = newGO.AddComponent<MeshFilter>();
        var mr = newGO.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        // map original vertex index -> new index for this part
        Dictionary<int, int> indexMap = new Dictionary<int, int>();
        List<Vector3> newVerts = new List<Vector3>();
        List<Vector3> newNormals = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();
        List<int> newTris = new List<int>();

        for (int i = 0; i < tris.Count; i++)
        {
            int origIdx = tris[i];
            if (!indexMap.TryGetValue(origIdx, out int newIdx))
            {
                newIdx = newVerts.Count;
                indexMap[origIdx] = newIdx;
                newVerts.Add(srcVerts[origIdx]);
                if (srcNormals != null && srcNormals.Length > origIdx) newNormals.Add(srcNormals[origIdx]);
                else newNormals.Add(Vector3.up);
                if (srcUVs != null && srcUVs.Length > origIdx) newUVs.Add(srcUVs[origIdx]);
                else newUVs.Add(Vector2.zero);
            }
            newTris.Add(newIdx);
        }

        Mesh newMesh = new Mesh();
        newMesh.name = gameObject.name + "_" + nameSuffix + "_mesh";
        newMesh.SetVertices(newVerts);
        newMesh.SetNormals(newNormals);
        newMesh.SetUVs(0, newUVs);
        newMesh.SetTriangles(newTris, 0);
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        mf.sharedMesh = newMesh;
    }
}
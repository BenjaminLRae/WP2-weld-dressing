using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelSystem;

public class SceneManager : MonoBehaviour
{
    //public GPUVoxelizer voxelizer;

    public GameObject objectToVoxelise;
    public ComputeShader voxeliseShader;

    public int CPUVoxeliseResolution = 10;

    List<Voxel_t> voxels;
    float unit;

    public GameObject voxelPrefab;
    public GameObject voxelParent;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button("Test CPU Voxelisation")]
    public void TestCPUVoxelisation()
    {
        try
        {
            Mesh meshToVoxelise;
            meshToVoxelise = objectToVoxelise.GetComponent<MeshFilter>().sharedMesh;

            //List<Voxel_t> voxels;
            //float unit;

            CPUVoxelizer.Voxelize(
                meshToVoxelise,
                CPUVoxeliseResolution,
                out voxels,
                out unit
            );

            Debug.Log("voxels List size: " + voxels.Count);
            Debug.Log("Unit: " + unit);
            foreach (Voxel_t voxel in voxels)
            {
                Debug.Log(voxel.position);
            }

        }
        catch (Exception e)
        {
            Debug.Log("Error getting mesh");
        }

        
    }

    [Button("Test voxel instantiation")]
    public void TestVoxelInstantiation()
    {
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        foreach (Voxel_t voxel in voxels)
        {
            Instantiate(voxelPrefab, voxel.position, rotation, voxelParent.transform);
        }
    }

    [Button("Test GPU voxelisation")]
    public void TestGPUVoxelisation()
    {
        Mesh meshToVoxelise;
        meshToVoxelise = objectToVoxelise.GetComponent<MeshFilter>().sharedMesh;

        GPUVoxelData data = GPUVoxelizer.Voxelize(
            voxeliseShader,
            meshToVoxelise,
            64,
            true
        );

        bool useUV = false;

        GetComponent<MeshFilter>().sharedMesh = VoxelMesh.Build(data.GetData(), data.UnitLength, useUV);

        RenderTexture volumeTexture = GPUVoxelizer.BuildTexture3D(
            voxeliseShader,
            data,
            RenderTextureFormat.ARGBFloat,
            FilterMode.Bilinear
        );

        data.Dispose();
    }
}

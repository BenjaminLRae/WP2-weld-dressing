using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VoxelSystem;

public class SceneManager : MonoBehaviour
{
    //public GPUVoxelizer voxelizer;

    public GameObject objectToVoxelise;
    public ComputeShader voxeliseShader;

    public int CPUVoxeliseResolution = 10;
    [Tooltip("Used to provide padding between voxels, a percentage of unitlength")]
    public float unitLengthModifier = 0.9f; // used to provide padding around voxels, a percentage

    public List<Voxel_t> voxels;

    //[ShowNativeProperty]
    //public int VoxelsCount => voxels.Count;

    public float unitLength; // side length of generated voxels

    public GameObject voxelPrefab;
    public GameObject voxelParent;

    public Material transparentMaterial;
    public Material nonTransparentMaterial;

    public UnityEngine.UI.Slider resolutionSlider;
    public TextMeshProUGUI resolutionDisplay;
    public TextMeshProUGUI voxelsCountDisplay;
    public TextMeshProUGUI estimatedVolume;

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
                out unitLength
            );

            Debug.Log("voxels List size: " + voxels.Count);
            Debug.Log("Unit: " + unitLength);
            foreach (Voxel_t voxel in voxels)
            {
                //Debug.Log(voxel.position);
            }

            voxelsCountDisplay.text = voxels.Count.ToString();

        }
        catch (Exception e)
        {
            Debug.Log("Error getting mesh, error " + e);
        }        
    }

    [Button("Test voxel instantiation")]
    public void TestVoxelInstantiation()
    {
        DestroyVoxels();

        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        float sideLength = unitLength * unitLengthModifier;
        voxelPrefab.transform.localScale = new Vector3(sideLength, sideLength, sideLength);

        foreach (Voxel_t voxel in voxels)
        {
            Instantiate(voxelPrefab, voxel.position, rotation, voxelParent.transform);
        }

        // Scale voxelParent to size of original gameobject
        voxelParent.transform.localScale = objectToVoxelise.transform.localScale;

        float voxelVolume = unitLength * unitLength * unitLength;
        float totalVolume = voxelVolume * voxels.Count;

        estimatedVolume.text = totalVolume.ToString();
    }

    //[Button("Test GPU voxelisation")]
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

    [Button("Destroy generated voxels")]
    public void DestroyVoxels()
    {
        Transform[] createdVoxels = voxelParent.transform.GetComponentsInChildren<Transform>();
        Debug.Log("Found " + createdVoxels.Length + " created voxels under VoxelParent");

        for (int i = 0; i < createdVoxels.Length; i++)
        {
            if (createdVoxels[i].gameObject != voxelParent)
            {
                DestroyImmediate(createdVoxels[i].gameObject);
            }
        }
        voxelParent.transform.localScale = Vector3.one; // reset scale of voxel parent
    }

    [Button("Make voxels transparent")]
    public void SetVoxelsToTransparent()
    {
        Transform[] createdVoxels = voxelParent.transform.GetComponentsInChildren<Transform>();

        for (int i = 0; i < createdVoxels.Length; i++)
        {
            if (createdVoxels[i].gameObject != voxelParent)
            {
                MeshRenderer mr = createdVoxels[i].gameObject.GetComponent<MeshRenderer>();
                mr.material = transparentMaterial;
            }
        }
    }

    [Button("Make voxels solid")]
    public void SetVoxelsToSolid()
    {
        Transform[] createdVoxels = voxelParent.transform.GetComponentsInChildren<Transform>();

        for (int i = 0; i < createdVoxels.Length; i++)
        {
            if (createdVoxels[i].gameObject != voxelParent)
            {
                MeshRenderer mr = createdVoxels[i].gameObject.GetComponent<MeshRenderer>();
                mr.material = nonTransparentMaterial;
            }
        }
    }

    public void SetResolutionWithSlider()
    {
        resolutionDisplay.text = resolutionSlider.value.ToString();
        CPUVoxeliseResolution = (int)resolutionSlider.value;
    }
}

﻿using UnityEngine;

public class DrawOutline3 : PostEffectsBase
{
    public Camera additionalCamera;

    public Shader drawOccupied;
    private Material occupiedMaterial = null;
    public Material OccupiedMaterial { get { return CheckShaderAndCreateMaterial(drawOccupied, ref occupiedMaterial); } }

    public Color outlineColor = Color.green;
    [Range(0, 10)]
    public int outlineWidth = 4;
    [Range(0, 9)]
    public int iterations = 1;

    public GameObject[] targets;

    private MeshFilter[] meshFilters;
    private RenderTexture tempRT;

    private void Awake()
    {
        SetupAddtionalCamera();
    }

    private void SetupAddtionalCamera()
    {
        additionalCamera.CopyFrom(MainCamera);
        additionalCamera.clearFlags = CameraClearFlags.Color;
        additionalCamera.backgroundColor = Color.black;
        additionalCamera.cullingMask = 1 << LayerMask.NameToLayer("PostEffect");       // 标记渲染"PostEffect"层的物体
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (TargetMaterial != null && drawOccupied != null && additionalCamera != null && targets != null)
        {
            tempRT = RenderTexture.GetTemporary(source.width, source.height, 0);
            additionalCamera.targetTexture = tempRT;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null)
                    continue;
                meshFilters = targets[i].GetComponentsInChildren<MeshFilter>();
                for (int j = 0; j < meshFilters.Length; j++)
                    Graphics.DrawMesh(meshFilters[j].sharedMesh, meshFilters[j].transform.localToWorldMatrix, OccupiedMaterial, LayerMask.NameToLayer("PostEffect"), additionalCamera); // 描绘选中物体的所占面积
            }
            additionalCamera.Render();  // 需要调用渲染函数，才能及时把描绘物体渲染到纹理中

            TargetMaterial.SetTexture("_SceneTex", source);
            TargetMaterial.SetColor("_Color", outlineColor);
            TargetMaterial.SetInt("_Width", outlineWidth);
            TargetMaterial.SetInt("_Iterations", iterations);

            // 使用描边混合材质实现描边效果
            Graphics.Blit(tempRT, destination, TargetMaterial);

            tempRT.Release();
        }
        else
            Graphics.Blit(source, destination);
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class ColorDataSO : ScriptableObject
{
    public Material BlockMaterial;
    public Material GateMaterial;
    public Material ScreenMaterial;
    public GameObject objEffect;
    public Color ColorEffect;
    public TColor ColorType;


    public ParticleSystem CreateEffect(Vector3 Pos, Vector3 Rota)
    {
        ParticleSystem particleSystem = Instantiate(objEffect, Pos, Quaternion.Euler(Rota)).GetComponent<ParticleSystem>();

        /*     var mainModule = particleSystem.main;
             mainModule.startColor = ColorEffect;*/


        // Đổi material trực tiếp cho Renderer
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();

        renderer.material = GateMaterial;


        return particleSystem;
    }

    public TColor GetColorType()
    {
        return ColorType;
    }
    public virtual void SetBlockColor(BlockController blockController)
    {
        blockController.SetMaterialBlock(BlockMaterial);
    }
    public virtual void SetScreenColor(BlockController blockController)
    {
        blockController.SetMaterialScreen(0, BlockMaterial);
    }

    public virtual void SetGateColor(GateController gateController)
    {
        gateController.SetMaterial(GateMaterial);
    }


    public virtual void SetScrewnBlockColor(BlockController blockController)
    {
        blockController.SetMaterialScreen(1, ScreenMaterial);
    }

    public bool IsContainColor(ColorDataSO color)
    {
        if (color == this) return true;
        return false;
    }
}


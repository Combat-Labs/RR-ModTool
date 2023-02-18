using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CachedMaterial
{
    public Material Material { get; set; }
    public Material tempMaterial;
    public int activeCount { get; set; } = 0;

    public CachedMaterial(Material origin)
    {
        Material = (origin);
    }

    ~CachedMaterial()
    {
        if (tempMaterial != null)
        {
            UnityEngine.Object.Destroy(tempMaterial);
        }
    }
}

[Serializable]
public class MaterialTarget : IEquatable<MaterialTarget>
{
    public Renderer Renderer;
    public int MaterialIndex = 0;

    public static Dictionary<MaterialTarget, CachedMaterial> CacheCount =
        new Dictionary<MaterialTarget, CachedMaterial>();

    public static void ResetCache()
    {
        if (CacheCount.Count > 0)
        {
            //GameClient.Log(LogLevel.Warning, "MaterialTarget.ResetCache called and CacheCount was not zero. Maybe some material targets were leaking! Check it out.");
        }

        CacheCount.Clear();
    }

    public bool Equals(MaterialTarget rhs)
    {
        return rhs != null && (Renderer == rhs.Renderer && MaterialIndex == rhs.MaterialIndex);
    }

    public override bool Equals(object compare)
    {
        return compare is MaterialTarget && Equals(compare as MaterialTarget);
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public void MarkActive()
    {
        if (Renderer.sharedMaterials.Length > MaterialIndex)
        {
            CachedMaterial current;
            if (CacheCount.TryGetValue(this, out current))
            {
                if (current.activeCount == 0 && current.Material != null)
                {
                    // When we start an animation and there are no others active,
                    // create a copy of the material.
                    // We will restore the original when all animations have finished.
                    var materials = Renderer.sharedMaterials;

                    if (materials[MaterialIndex] != current.Material)
                    {
                        UnityEngine.Object.Destroy(materials[MaterialIndex]);
                    }

                    if (current.tempMaterial != null)
                    {
                        UnityEngine.Object.Destroy(current.tempMaterial);
                    }

                    current.tempMaterial = new Material(current.Material);

                    materials[MaterialIndex] = current.tempMaterial;

                    Renderer.sharedMaterials = materials;
                }

                current.activeCount++;
            }
        }
    }

    public void Cache()
    {
        if (Renderer.sharedMaterials.Length > MaterialIndex)
        {
            CachedMaterial current;
            if (CacheCount.TryGetValue(this, out current))
            {
                Debug.LogError("Attempted to cache the same material twice!");
                return;
            }
            else
            {
                // Cache a copy of the original, and then use that copy from now on.
                if (Renderer.sharedMaterials[MaterialIndex] != null)
                {
                    Material newMaterial = (Renderer.sharedMaterials[MaterialIndex]);
                    current = new CachedMaterial(newMaterial);
                    // var materials = Renderer.sharedMaterials;
                    // Renderer.sharedMaterials = materials;
                    CacheCount[this] = current;
                }
                else
                {
                    Debug.LogError("Null material found on: ");
                }
            }
        }
    }

    public void Restore()
    {
        if (Renderer.sharedMaterials.Length > MaterialIndex)
        {
            CachedMaterial current;
            if (CacheCount.TryGetValue(this, out current))
            {
                if (current.activeCount <= 0)
                {
                    Debug.LogError("Tried to restore a material when there were no active controllers modifying it");
                }
                else
                {
                    current.activeCount--;
                }

                if (current.activeCount == 0)
                {
                    var materials = Renderer.sharedMaterials;
                    if (materials[MaterialIndex] != current.Material)
                    {
                        UnityEngine.Object.Destroy(materials[MaterialIndex]);
                    }

                    if (current.tempMaterial != null)
                    {
                        UnityEngine.Object.Destroy(current.tempMaterial);
                    }

                    materials[MaterialIndex] = current.Material;
                    Renderer.sharedMaterials = materials;
                }
                else
                {
                    //NOTE(Tazz): Possible leak
                    /*var materials = Renderer.sharedMaterials;
                    if (materials[MaterialIndex] != current.Material) { UnityEngine.Object.Destroy(materials[MaterialIndex]); }
                    if (current.tempMaterial != null) { UnityEngine.Object.Destroy(current.tempMaterial); }
                    
                    materials[MaterialIndex] = current.Material;
                    Renderer.sharedMaterials = materials;*/
                }
            }
        }
    }

    public void SetFloat(string shaderVariableName, float value)
    {
        CachedMaterial current;
        if (CacheCount.TryGetValue(this, out current))
        {
            if (current.activeCount <= 0)
            {
                Debug.Log("Incorrectly modifying " + shaderVariableName + " to " + value + " on original material!");
            }
        }

        //var materials = Renderer.sharedMaterials;
        if (ValidMaterial(MaterialIndex))
        {
            if (shaderVariableName == "_Transparency")
            {
                Renderer.sharedMaterials[MaterialIndex].SetOverrideTag("RenderType", "Transparent");
                Renderer.sharedMaterials[MaterialIndex].SetOverrideTag("Queue", "Transparent");
                Renderer.sharedMaterials[MaterialIndex]
                    .SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                Renderer.sharedMaterials[MaterialIndex]
                    .SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                Renderer.sharedMaterials[MaterialIndex].SetInt("_ZWrite", 0);
            }

            Renderer.sharedMaterials[MaterialIndex].SetFloat(shaderVariableName, value);
            //Renderer.sharedMaterials = materials;
        }
    }

    public void SetColor(string shaderVariableName, Color value)
    {
        CachedMaterial current;
        if (CacheCount.TryGetValue(this, out current))
        {
            if (current.activeCount <= 0)
            {
                Debug.Log("Incorrectly modifying " + shaderVariableName + " to " + value + " on original material!");
            }
        }

        //var materials = Renderer.sharedMaterials;
        if (ValidMaterial(MaterialIndex))
        {
            Renderer.sharedMaterials[MaterialIndex].SetColor(shaderVariableName, value);
            Renderer.sharedMaterials[MaterialIndex].EnableKeyword("_EMISSION");
            //Renderer.sharedMaterials = materials;
        }
    }

    public void SetTexture(string shaderVariableName, Texture value)
    {
        CachedMaterial current;
        //debugplus.Log("Set Texture");
        if (CacheCount.TryGetValue(this, out current))
        {
            if (current.activeCount <= 0)
            {
                Debug.Log("Incorrectly modifying " + shaderVariableName + " to " + value + " on original material!");
            }
        }

        var materials = Renderer.sharedMaterials;
        if (ValidMaterial(MaterialIndex))
        {
            materials[MaterialIndex].SetTexture(shaderVariableName, value);
            Renderer.sharedMaterials = materials;
        }
    }

    public void SetMaterial(string shaderVariableName, Material value, bool adoptTextureAndColor = false)
    {
        CachedMaterial current;
        //debugplus.Log("Set Material");
        if (CacheCount.TryGetValue(this, out current))
        {
            if (current.activeCount <= 0)
            {
                Debug.Log("Incorrectly modifying " + shaderVariableName + " to " + value + " on original material!");
            }
        }

        if (value && ValidMaterial(MaterialIndex))
        {
            var materials = Renderer.sharedMaterials;
            if (adoptTextureAndColor)
            {
                value = new Material(value);
                if (value.HasProperty("_Color") && materials[MaterialIndex].HasProperty("_Color"))
                    value.color = materials[MaterialIndex].color;
                if (value.HasProperty("_MainTex") && materials[MaterialIndex].HasProperty("_MainTex"))
                    value.mainTexture = materials[MaterialIndex].mainTexture;
            }

            materials[MaterialIndex] = value;
            Renderer.sharedMaterials = materials;
        }
    }

    private bool ValidMaterial(int index)
    {
        return Renderer.sharedMaterials.Length > index && Renderer.sharedMaterials[index];
    }
}

[Serializable]
public class MaterialTargetData
{
    public string Id;
    public List<MaterialTarget> Materials = new List<MaterialTarget>();

    public void MarkActive()
    {
        foreach (var material in Materials)
        {
            material.MarkActive();
        }
    }

    public void Cache()
    {
        foreach (var material in Materials)
        {
            material.Cache();
        }
    }

    public void Restore()
    {
        foreach (var material in Materials)
        {
            material.Restore();
        }
    }

    public void SetFloat(string shaderVariableName, float value)
    {
        foreach (var material in Materials)
        {
            material.SetFloat(shaderVariableName, value);
        }
    }

    public void SetColor(string shaderVariableName, Color value)
    {
        foreach (var material in Materials)
        {
            material.SetColor(shaderVariableName, value);
        }
    }

    public void SetTexture(string shaderVariableName, Texture value)
    {
        foreach (var material in Materials)
        {
            material.SetTexture(shaderVariableName, value);
        }
    }

    public void SetMaterial(string shaderVariableName, Material value, bool adoptColorAndTexture = false)
    {
        foreach (var material in Materials)
        {
            material.SetMaterial(shaderVariableName, value, adoptColorAndTexture);
        }
    }
}

[Serializable]
public class ColorId
{
    public string Id;
    [ColorUsageAttribute(true, true)] public Color Color;
}

[Serializable]
public class ColorGradientId
{
    public string Id;
    public Gradient Gradient = new Gradient();
    public AnimationCurve Multiplier = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

    public Color Evaluate(float percent)
    {
        return Gradient.Evaluate(percent) * Multiplier.Evaluate(percent);
    }
}

[Serializable]
public class TextureId
{
    public string Id;
    public Texture Texture;
}

[Serializable]
public class MaterialId
{
    public string Id;
    public Material Material;
}

[Serializable]
public class PrefabId
{
    public string Id;
    public GameObject Prefab;
    public PreloadType type = PreloadType.EFFECT;
    public int count = 8;
}

public class MaterialTargetsData : MonoBehaviour
{
    public void MarkActive(string Id)
    {
        foreach (var target in Targets)
        {
            if (target.Id == Id)
            {
                target.MarkActive();
            }
        }
    }

    public void MarkAllActive()
    {
        foreach (var target in AllTargets)
        {
            target.MarkActive();
        }
    }

    public void Cache(string Id)
    {
        foreach (var target in Targets)
        {
            if (target.Id == Id)
            {
                target.Cache();
            }
        }
    }

    public void CacheAll()
    {
        foreach (var target in AllTargets)
        {
            target.Cache();
        }
    }

    public void Restore(string Id)
    {
        foreach (var target in Targets)
        {
            if (target.Id == Id)
            {
                target.Restore();
            }
        }
    }

    public void RestoreAll()
    {
        foreach (var target in AllTargets)
        {
            target.Restore();
        }
    }

    public void SetFloat(string id, string shaderVariableName, float value)
    {
        foreach (var target in Targets)
        {
            if (target.Id == id)
            {
                target.SetFloat(shaderVariableName, value);
            }
        }
    }

    public void SetFloatAll(string shaderVariableName, float value)
    {
        foreach (var target in AllTargets)
        {
            target.SetFloat(shaderVariableName, value);
        }
    }

    public void SetColor(string id, string shaderVariableName, Color value)
    {
        foreach (var target in Targets)
        {
            if (target.Id == id)
            {
                target.SetColor(shaderVariableName, value);
            }
        }
    }

    public void SetColorAll(string shaderVariableName, Color value)
    {
        foreach (var target in AllTargets)
        {
            target.SetColor(shaderVariableName, value);
        }
    }

    public void SetTexture(string id, string shaderVariableName, Texture value)
    {
        foreach (var target in Targets)
        {
            if (target.Id == id)
            {
                target.SetTexture(shaderVariableName, value);
            }
        }
    }

    public void SetTextureAll(string shaderVariableName, Texture value)
    {
        foreach (var target in AllTargets)
        {
            target.SetTexture(shaderVariableName, value);
        }
    }

    public void SetMaterial(string id, string shaderVariableName, Material value)
    {
        foreach (var target in Targets)
        {
            if (target.Id == id)
            {
                target.SetMaterial(shaderVariableName, value);
            }
        }
    }

    public void SetMaterialAll(string shaderVariableName, Material value, bool adoptColorAndTexture = false)
    {
        foreach (var target in AllTargets)
        {
            target.SetMaterial(shaderVariableName, value, adoptColorAndTexture);
        }
    }

    static ColorGradientId nullGradient = new ColorGradientId();

    public ColorGradientId GradientForId(string id)
    {
        for (int i = 0; i < ColorGradients.Count; ++i)
        {
            if (ColorGradients[i].Id == id)
            {
                return ColorGradients[i];
            }
        }

        return nullGradient;
    }

    public Color ColorForId(string id)
    {
        for (int i = 0; i < Colors.Count; ++i)
        {
            if (Colors[i].Id == id)
            {
                return Colors[i].Color;
            }
        }

        Color result;
        if (standardColors.TryGetValue(id, out result))
        {
            return result;
        }

        return Color.clear;
    }

    public Texture TextureForId(string id)
    {
        for (int i = 0; i < Textures.Count; ++i)
        {
            if (Textures[i].Id == id)
            {
                return Textures[i].Texture;
            }
        }

        Texture result;
        if (standardTextures.TryGetValue(id, out result))
        {
            return result;
        }

        return null;
    }

    public Material MaterialForId(string id)
    {
        for (int i = 0; i < Materials.Count; ++i)
        {
            if (Materials[i].Id == id)
            {
                return Materials[i].Material;
            }
        }

        return null;
    }

    //https://docs.unity3d.com/ScriptReference/Color.html standard colors for fallback
    private static Dictionary<string, Color> standardColors = new Dictionary<string, Color>
    {
        {"white", Color.white},
        {"black", Color.black},
        {"red", Color.red},
        {"green", Color.green},
        {"blue", Color.blue},
        {"yellow", Color.yellow},
        {"clear", Color.clear},
        {"cyan", Color.cyan},
        {"gray", Color.gray},
        {"magenta", Color.magenta},
        {"orange", new Color(1, .5f, 0)},
        {"bluish", new Color(.1f, .3f, 1)},
        {"vortex", new Color(0, .6f, 1)},
        {"steel", new Color(0.7f, .9f, 1)},
        {"hitInvuln", new Color(1, .5f, .5f)},
        {"grabInvuln", new Color(0.6f, 1, .6f)},
        {"sethPurple", new Color(0.7f, 0.65f, 1)},
        {"redOrange", new Color(1, .2f, 0)},
        {"redOrange2", new Color(1, .4f, 0)}
    };

    private static Dictionary<string, Texture> standardTextures = null; //need to init in awake to let textures load.

    private void Awake()
    {
        if (standardTextures == null)
        {
            standardTextures = new Dictionary<string, Texture>
            {
                {"null", null},
                {"white", Texture2D.whiteTexture},
                {"black", Texture2D.blackTexture}
            };
        }

        AllTargets = new List<MaterialTarget>();
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            for (int i = 0; i < renderer.sharedMaterials.Length; ++i)
            {
                AllTargets.Add(new MaterialTarget {MaterialIndex = i, Renderer = renderer});
            }
        }
    }

    private List<MaterialTarget> AllTargets = new List<MaterialTarget>();

    public List<MaterialTargetData> Targets = new List<MaterialTargetData>();
    public List<ColorId> Colors = new List<ColorId>();
    public List<ColorGradientId> ColorGradients = new List<ColorGradientId>();
    public List<TextureId> Textures = new List<TextureId>();
    public List<MaterialId> Materials = new List<MaterialId>();
}
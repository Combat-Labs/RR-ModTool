using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class IEquipmentData : ScriptableObject
{
    [JsonProperty] public ushort ID = 0;
    [JsonProperty] public string backendID = "";
    [JsonProperty] public string[] packIds = Array.Empty<String>();

    [JsonProperty] public virtual string itemName { get; set; }

    [JsonProperty] public bool isDefault = false;
    [JsonProperty] public bool hideWhenNotOwned = false;
    [JsonProperty] public bool enabled = true;
    [JsonProperty] public bool isBaseGameItem = true;
    public bool idWasSet = false;

    [HideInInspector] [JsonProperty] public string fileName;

    [HideInInspector] [JsonProperty] public string localizationKey = null;


    [HideInInspector]
    [JsonProperty]
    public virtual string localAssetId
    {
        get { return fileName; }
    }

    public bool IsMod = true;
}

public enum CharacterID
{
    Any = -2,
    Random = -1,
    None = 0,
    Kidd = 1,
    Ashani = 2,
    Xana = 3,
    Raymer = 4,
    Zhurong = 5,
    AfiGalu = 6,
    Weishan = 7, // Weishan
    Ezzie = 8,
    Seth = 9, // Ezzie
    Velora = 10, //,,,
    Reina = 11,
    Torment = 12
}

[System.Serializable]
public class Bone
{
    // Data
    public Transform transform;
    public BodyPart bodyPart;
    public bool hasOffset = false;
    public Vector3 offset;

    public Vector3 GetBonePosition()
    {
        if (!hasOffset)
        {
            if (transform == null)
            {
                Debug.LogError("Bone '" + bodyPart +
                               "' has no mapped transform; assign the bone data in the character editor.");
                return new Vector3();
            }

            return transform.position;
        }

        return transform.position + transform.rotation * offset;
    }

    public Quaternion GetBoneRotation()
    {
        return transform.rotation;
    }

    public void Load(Bone other)
    {
        this.transform = other.transform;
        this.bodyPart = other.bodyPart;
        this.hasOffset = other.hasOffset;
        this.offset = other.offset;
    }
}

public enum PreloadType
{
    EFFECT,
    ARTICLE,
    PROJECTILE,
    WEAPON_TRAIL
}

public interface IDefaultableData
{
    bool IsDefaultData { get; }
}

public enum BodyPart
{
    none,
    head,
    upperTorso,
    lowerTorso,
    leftUpperArm,
    rightUpperArm,
    leftForearm,
    rightForearm,
    leftHand,
    rightHand,
    leftThigh,
    rightThigh,
    leftCalf,
    rightCalf,
    leftFoot,
    rightFoot,
    root,
    leftLittleTip,
    leftRingTip,
    leftMiddleTip,
    leftIndexTip,
    leftThumbTip,
    rightLittleTip,
    rightRingTip,
    rightMiddleTip,
    rightIndexTip,
    rightThumbTip,
    rightToes,
    leftToes,

    //	(EML) Anything below here is not part of the default humanoid rig
    //	It won't be mirrored by default so the client manually
    //	mirrors it in the bonecontroller when the game gets the bone position
    weaponGun,
    throwBone,
    swordHilt,
    swordBlade1,
    swordBlade2,
    swordEnd,
    shield,
    hatBase,
    hatMiddle,
    hatTip,
    chain00End,
    chain05,
    chain10,
    chain15,
    chain20,
    chain30,
    chain40,
    chain50,
    chain60,
    chain70,
    chain80,
    chain90,
    chainBase,
    tailBase,
    tail1,
    tail2,
    tail3,
    tailEnd,
    weaponMiscL1,
    weaponMiscR1,
    weaponMiscL2,
    weaponMiscR2,
}

public struct BodyPartComparer : IEqualityComparer<BodyPart>
{
    public bool Equals(BodyPart x, BodyPart y)
    {
        return x == y;
    }

    public int GetHashCode(BodyPart obj)
    {
        return (int)obj;
    }
}
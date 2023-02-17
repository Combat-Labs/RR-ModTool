using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SkinData : ScriptableObject, IDefaultableData
{
	public Sprite battlePortrait { get { return battlePortraitFile.obj; } }
	public SpriteFile battlePortraitFile = new SpriteFile();

	public Sprite battlePortraitGrey { get { return battlePortraitGreyFile.obj; } }
	public SpriteFile battlePortraitGreyFile = new SpriteFile();

	public Sprite crewsHudPortrait { get { return crewsHudPortraitFile.obj; } }
	public SpriteFile crewsHudPortraitFile = new SpriteFile();

	public GameObject characterPrefab { get { return characterPrefabFile.obj; } }
	public GameObjectFile characterPrefabFile = new GameObjectFile();

	public GameObject combinedPrefab { get { return combinedPrefabFile.obj; } }
	public GameObjectFile combinedPrefabFile = new GameObjectFile();

	public GameObject partnerPrefab { get { return partnerPrefabFile.obj; } }
	public GameObjectFile partnerPrefabFile = new GameObjectFile();

	public GameObject combinedPartnerPrefab { get { return combinedPartnerPrefabFile.obj; } }
	public GameObjectFile combinedPartnerPrefabFile = new GameObjectFile();

	public GameObject lastModelImportFBX { get { return lastModelImportFBXFile.obj; } }
	public GameObjectFile lastModelImportFBXFile = new GameObjectFile();

	public SkinDefinition skinDefinition { get { return skinDefinitionFile.obj; } }
	public SkinDefinitionFile skinDefinitionFile = new SkinDefinitionFile();

	public string skinDefinitionFileName;

	public bool overridePortraitOffset = false;
	public Vector2Int portraitOffset = Vector2Int.zero;
	public bool overrideVictoryPortraitOffset = false;
	public Vector2Int victoryPortraitOffset = Vector2Int.zero;
	public Vector2 victoryPortraitScale = Vector2.one;

	public bool enabled { get { return skinDefinition.enabled; } }
    public bool nemesis { get { return skinDefinition.nemesis; } }
    public bool demoEnabled { get { return skinDefinition.demoEnabled; } }

	public string skinName { get { return skinDefinition == null ? "??? " : skinDefinition.skinName; } }
	public string uniqueKey { get { return skinDefinition.uniqueKey; } }
	public bool isDefault { get { if (skinDefinition == null) Debug.Log(skinDefinitionFileName); return skinDefinition.isDefault; } }

	public float lastModelImportScale = 1;

	public List<MaterialId> weaponTrails = new List<MaterialId>();

	public List<PrefabId> uniquePrefabs = new List<PrefabId>();

	public GameObject CharacterPrefab
	{
		get
		{
			if (combinedPrefab != null)
			{
				return combinedPrefab;
			}
			else
			{
				return characterPrefab;
			}
		}
	}

	public GameObject PartnerPrefab
	{
		get
		{
			if (combinedPartnerPrefab != null)
			{
				return combinedPartnerPrefab;
			}
			else
			{
				return partnerPrefab;
			}
		}
	}

	public bool IsDefaultData { get { return isDefault; } }

	public Material WeaponTrailForId(string id)
	{
		for (int i = 0; i < weaponTrails.Count; ++i)
		{
			if (weaponTrails[i].Id == id)
			{
				return weaponTrails[i].Material;
			}
		}
		return null;
	}

	public GameObject GetUniquePrefabFromID(string id, GameObject defaultPrefab)
	{
		for (int i = 0; i < uniquePrefabs.Count; ++i)
		{
			if (uniquePrefabs[i].Id == id && uniquePrefabs[i].Prefab != null)
			{
				return uniquePrefabs[i].Prefab;
			}
		}
		return defaultPrefab;
	}
	

}

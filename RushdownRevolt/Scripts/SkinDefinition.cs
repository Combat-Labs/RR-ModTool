using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Serialization;

[JsonObject(MemberSerialization.OptIn)]
public class SkinDefinition : IEquipmentData
{
	[JsonProperty]
	[FormerlySerializedAs("id")]
	public CharacterID CharacterID = CharacterID.None;
	[JsonProperty]
	public string uniqueKey;
	[JsonProperty]
	public string skinName;
	[JsonProperty]
    public bool nemesis = false;
    [JsonProperty]
    public string skinGroupName = "Modded";
    
    [JsonProperty]
    public string skinGroupLocalizationKey;
    
    [JsonProperty]
    public bool defaultable = true;

    [FormerlySerializedAs("numInCategory")]
    [JsonProperty] 
    public int priority = 0;
    public bool demoEnabled = false;
    public SkinColors skinColor = new SkinColors();
    
	public override string itemName
	{
		get { return skinName; }
		set { skinName = value; }
	}

	public bool IsDefaultData { get { return isDefault; } }
	
	public bool isValidFilePath
	{
		get { return AddressableResourceExists(uniqueKey); }
	}
	
	public bool AddressableResourceExists(object key) {
		foreach (var l in Addressables.ResourceLocators) {
			IList<IResourceLocation> locs;
			if (l.Locate(key, typeof(SkinData), out locs))
				return true;
		}
		return false;
	}
	
	private static string RemoveSpecialCharacters(string str)
	{
		return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
	}
	
	[HideInInspector]
	[JsonProperty]
	public override string localAssetId
	{
		get { return RemoveSpecialCharacters(uniqueKey.Replace(" ", "")); }
	}
	
	//StaticDB Stuff
	public bool _enabled
	{
		get { return enabled; }
	}

	public string _friendlyName
	{
		get { return skinName;  }
	}

	
}

[System.Serializable]
public class SkinDefinitionFile : UnityFileLink<SkinDefinition> { }

[System.Serializable]
public class SkinColors
{
	public Color primaryColor = Color.gray;
	public Color secondaryColor = Color.cyan;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;

[Serializable]
public class UnityFileLink
{
	public virtual void SyncData(string ownerFilePath = null) { }
	public virtual string GetFilePath() { return null; }
	public virtual UnityEngine.Object GetObject() { return null; }
}

[Serializable]
public class UnityFileLink<T> : UnityFileLink where T : UnityEngine.Object
{
	[SerializeField]
	private UnityEngine.Object reference;

	[SerializeField]
	private string _FILE_PATH; // File path is authoritative for regenerating the reference

	public T obj { get { return (reference == null) ? null : (T)reference; } }

//#if !UNITY_EDITOR
	public void RuntimeOverrideWithMemoryObject(T obj)
	{
		reference = obj;
	}
//#endif

#if UNITY_EDITOR
	public void SetObject(UnityEngine.Object obj)
	{
		if (reference != obj)
		{
			reference = obj;

			if (reference == null)
			{
				_FILE_PATH = null;
			}
			else
			{
				_FILE_PATH = UnityEditor.AssetDatabase.GetAssetPath(obj);
			}
		}
	}

	public override UnityEngine.Object GetObject()
	{
		return reference;
	}

	public override void SyncData(string ownerFilePath = null)
	{
		if (!string.IsNullOrEmpty(_FILE_PATH))
		{
			if (reference == null)
			{
				T objectFile = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(_FILE_PATH);

				if (objectFile == null)
				{
					Debug.LogErrorFormat("UNITY FILE ERROR, unable to automatically repair link to: {0}, from object {1}", _FILE_PATH, ownerFilePath);
				}
				else
				{
					reference = objectFile;

					saveChangesToDisk(ownerFilePath);
				}
			}
			else
			{
				string path = UnityEditor.AssetDatabase.GetAssetPath(reference);

				if (_FILE_PATH != path)
				{
					Debug.Log("Updated file path from: " + _FILE_PATH + " to " + path);

					_FILE_PATH = path;
					
					saveChangesToDisk(ownerFilePath);
				}
			}
		}
	}

	public bool IsValid()
	{
		if (!string.IsNullOrEmpty(_FILE_PATH))
		{
			if (reference == null)
			{
				return false;
			}
		}

		return true;
	}

	private void saveChangesToDisk(string ownerFilePath)
	{
		if (ownerFilePath != null)
		{
			UnityEditor.AssetDatabase.ForceReserializeAssets(new List<string>() { ownerFilePath });
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.AssetDatabase.Refresh();
		}
	}
#endif
}

[Serializable]
public class SpriteFile : UnityFileLink<Sprite> { }

[Serializable]
public class GameObjectFile : UnityFileLink<GameObject> { }
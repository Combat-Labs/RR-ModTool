using UnityEditor;
using UnityEngine;
using RushdownRevolt.ModTool;

namespace RushdownRevolt.ModTool.Editor
{
    [CustomEditor(typeof(BoneData))]
    [CanEditMultipleObjects]
    public class BoneDataEditor : UnityEditor.Editor
    {
        public GameObject destinationParent = null;
        public override void OnInspectorGUI()
        {
            BoneData boneData = null;

            boneData = Selection.activeGameObject.GetComponent<BoneData>();
		
            destinationParent = (GameObject)EditorGUILayout.ObjectField("Desired Bones Parent (usually root)",destinationParent, typeof(GameObject), true);

            if (GUILayout.Button("Remap bone data"))
            {
                int boneIndex = 0;

                if (destinationParent != null && boneData != null)
                {
                    Transform[] bonelist = destinationParent.GetComponentsInChildren<Transform>();
				
                    foreach (Bone bone in boneData.Bones)
                    {
					
                        Debug.Log("Looking For " + bone.transform.name);
					
                        Transform prev = bone.transform;

                        foreach (Transform newBone in bonelist)
                        {
                            if (newBone.name == prev.name)
                            {
                                Debug.Log("Found " + boneIndex + " " + bone.transform.name);
                                bone.transform = newBone;
                                break;
                            }
							
                        }

                        boneIndex++;
                    }
                }
			
            }
		
            DrawDefaultInspector();
        }
    }
}
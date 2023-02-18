using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(BoneData))]
[CanEditMultipleObjects]
public class boneDataEditor : Editor
{
    public GameObject destinationParent = null;

    public override void OnInspectorGUI()
    {
        BoneData boneData = null;

        boneData = Selection.activeGameObject.GetComponent<BoneData>();

        destinationParent = (GameObject)EditorGUILayout.ObjectField("Desired Bones Parent (usually root)",
            destinationParent, typeof(GameObject), true);

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
#endif

public class BoneData : MonoBehaviour
{
    public bool PreviewInvertRotation { get; set; }
    public bool PreviewMirror { get; set; }

    public List<Bone> Bones = new List<Bone>();

    public List<Transform> HiddenProps = new List<Transform>();

    public Bone GetBone(BodyPart bodyPart)
    {
        foreach (Bone bone in Bones)
        {
            if (bone.bodyPart == bodyPart)
            {
                return bone;
            }
        }

        return null;
    }

    public void OnAnimatorMove()
    {
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.applyRootMotion = false;
        }
    }

    public void AutoConfigure(Animator animator, Transform root, Transform throwBone)
    {
        Dictionary<HumanBodyBones, BodyPart> boneMap = new Dictionary<HumanBodyBones, BodyPart>
        {
            {HumanBodyBones.Head, BodyPart.head},
            {HumanBodyBones.Chest, BodyPart.upperTorso},
            {
                HumanBodyBones.Spine,
                BodyPart.lowerTorso
            },
            {
                HumanBodyBones.LeftUpperArm,
                BodyPart.leftUpperArm
            },
            {
                HumanBodyBones.RightUpperArm,
                BodyPart.rightUpperArm
            },
            {
                HumanBodyBones.LeftLowerArm,
                BodyPart.leftForearm
            },
            {
                HumanBodyBones.RightLowerArm,
                BodyPart.rightForearm
            },
            {
                HumanBodyBones.LeftHand,
                BodyPart.leftHand
            }, // Circle
            {
                HumanBodyBones.RightHand,
                BodyPart.rightHand
            }, // Circle

            {
                HumanBodyBones.LeftUpperLeg,
                BodyPart.leftThigh
            },
            {
                HumanBodyBones.RightUpperLeg,
                BodyPart.rightThigh
            },
            {
                HumanBodyBones.LeftLowerLeg,
                BodyPart.leftCalf
            },
            {
                HumanBodyBones.RightLowerLeg,
                BodyPart.rightCalf
            },
            {
                HumanBodyBones.LeftFoot,
                BodyPart.leftFoot
            },
            {
                HumanBodyBones.RightFoot,
                BodyPart.rightFoot
            },
            {
                HumanBodyBones.LeftLittleDistal,
                BodyPart.leftLittleTip
            },
            {
                HumanBodyBones.LeftRingDistal,
                BodyPart.leftRingTip
            },
            {
                HumanBodyBones.LeftMiddleDistal,
                BodyPart.leftMiddleTip
            },
            {
                HumanBodyBones.LeftIndexDistal,
                BodyPart.leftIndexTip
            },
            {
                HumanBodyBones.LeftThumbDistal,
                BodyPart.leftThumbTip
            },
            {
                HumanBodyBones.RightLittleDistal,
                BodyPart.rightLittleTip
            },
            {
                HumanBodyBones.RightRingDistal,
                BodyPart.rightRingTip
            },
            {
                HumanBodyBones.RightMiddleDistal,
                BodyPart.rightMiddleTip
            },
            {
                HumanBodyBones.RightIndexDistal,
                BodyPart.rightIndexTip
            },
            {
                HumanBodyBones.RightThumbDistal,
                BodyPart.rightThumbTip
            },
            {
                HumanBodyBones.LeftToes,
                BodyPart.leftToes
            },
            {
                HumanBodyBones.RightToes,
                BodyPart.rightToes
            }
        };

        List<Bone> boneList = new List<Bone>();
        foreach (HumanBodyBones bone in boneMap.Keys)
        {
            Transform boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform != null)
            {
                Bone newHurtBox = new Bone();
                BodyPart bodyPart = boneMap[bone];
                newHurtBox.bodyPart = bodyPart;
                newHurtBox.transform = boneTransform;
                boneList.Add(newHurtBox);
            }
        }

        Bone rootBone = new Bone();
        rootBone.bodyPart = BodyPart.root;
        rootBone.transform = root;
        boneList.Add(rootBone);

        Bone throwBox = new Bone();
        throwBox.bodyPart = BodyPart.throwBone;
        throwBox.transform = throwBone;
        boneList.Add(throwBox);

        Bones = boneList;
    }
}
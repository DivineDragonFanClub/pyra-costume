﻿using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace UTJ.Jobs
{
    using SpringJobManagerButton = SpringJobManagerInspector.InspectorButton<SpringJobManager>;

    [CustomEditor(typeof(SpringJobManager))]
    [CanEditMultipleObjects]
    public class SpringJobManagerInspector : Editor
    {
        public class InspectorButton<T>
        {
            public InspectorButton(string label, System.Action<T> onPress)
            {
                Label = label;
                OnPress = onPress;
            }

            public string Label { get; set; }
            public System.Action<T> OnPress { get; set; }

            public void Show(T target)
            {
                if (GUILayout.Button(Label)) { OnPress(target); }
            }
        }

        public override void OnInspectorGUI()
        {
            if (targets.Length == 1)
            {
                // Only show buttons if one component is selected
                if (actionButtons == null || actionButtons.Length == 0)
                {
                    actionButtons = new[] {
                        new SpringJobManagerButton("Display Spring Bone window", ShowSpringWindow),
                        new SpringJobManagerButton("Select all Spring Bones", SelectAllBones),
                        new SpringJobManagerButton("Update Spring Bone List", UpdateBoneList)
                    };
                }

                EditorGUILayout.Space();
                var manager = (SpringJobManager)target;
                for (int buttonIndex = 0; buttonIndex < actionButtons.Length; buttonIndex++)
                {
                    actionButtons[buttonIndex].Show(manager);
                }
                EditorGUILayout.Space();
                var boneCount = (manager.SortedBones != null) ? manager.SortedBones.Length : 0;
                GUILayout.Label("Bones: " + boneCount);
                EditorGUILayout.Space();
            }

            base.OnInspectorGUI();
        }

        // private

        private SpringJobManagerButton[] actionButtons;

        private static void ShowSpringWindow(SpringJobManager manager)
        {
            SpringBoneWindow.ShowWindow();
        }

        private static void SelectAllBones(SpringJobManager manager)
        {
            var bones = manager.GetComponentsInChildren<SpringBone>(true);
            Selection.objects = bones.Select(item => item.gameObject).ToArray();
        }

        private static void UpdateBoneList(SpringJobManager manager)
        {
            SpringBoneSetupUTJ.FindAndAssignSpringBones(manager, true);
            CachedJobParam(manager);
            EditorUtility.SetDirty(manager);
        }

        private static int GetObjectDepth(Transform inObject) {
            var depth = 0;
            var currentObject = inObject;
            while (currentObject != null) {
                currentObject = currentObject.parent;
                ++depth;
            }
            return depth;
        }

        public static Transform GetPivotTransform(SpringBone bone)
        {
            if (bone.pivotNode == null)
            {
                bone.pivotNode = bone.transform.parent ?? bone.transform;
            }
            return bone.pivotNode;
        }

        private static SpringBone[] FindSpringBones(SpringJobManager manager, bool includeInactive = false) {
            var unsortedSpringBones = manager.GetComponentsInChildren<SpringBone>(includeInactive);
            var boneDepthList = unsortedSpringBones
                .Select(bone => new { bone, depth = GetObjectDepth(bone.transform) })
                .ToList();
            boneDepthList.Sort((a, b) => a.depth.CompareTo(b.depth));
            return boneDepthList.Select(item => item.bone).ToArray();
        }

        private static void CachedJobParam(SpringJobManager manager) {
            manager.SortedBones = FindSpringBones(manager);
            var nSpringBones = manager.SortedBones.Length;

            manager.jobProperties = new SpringBoneProperties[nSpringBones];
            manager.initLocalRotations = new Quaternion[nSpringBones];
            manager.jobColProperties = new SpringColliderProperties[nSpringBones];
            //manager.jobLengthProperties = new LengthLimitProperties[nSpringBones][];
            var jobLengthPropertiesList = new List<LengthLimitProperties>();

            for (var i = 0; i < nSpringBones; ++i) {
                SpringBone springBone = manager.SortedBones[i];
                //springBone.index = i;

                var root = springBone.transform;
                var parent = root.parent;

                //var childPos = ComputeChildBonePosition(springBone);
                var childPos = springBone.ComputeChildPosition();
                var childLocalPos = root.InverseTransformPoint(childPos);
                var boneAxis = Vector3.Normalize(childLocalPos);

                var worldPos = root.position;
                //var worldRot = root.rotation;

                var springLength = Vector3.Distance(worldPos, childPos);
                var currTipPos = childPos;
                var prevTipPos = childPos;

                // Length Limit
                var targetCount = springBone.lengthLimitTargets.Length;
                //manager.jobLengthProperties[i] = new LengthLimitProperties[targetCount];
                if (targetCount > 0) {
                    for (int m = 0; m < targetCount; ++m) {
                        var targetRoot = springBone.lengthLimitTargets[m];
                        int targetIndex = -1;
                        // NOTE: 
                        //if (targetRoot.TryGetComponent<SpringBone>(out var targetBone))
                            //targetIndex = targetBone.index;
                        var prop = new LengthLimitProperties {
                            targetIndex = targetIndex,
                            target = Vector3.Magnitude(targetRoot.position - childPos),
                        };
                        jobLengthPropertiesList.Add(prop);
                    }
                }

                // ReadOnly
                int parentIndex = -1;
                int pivotIndex = -1;

                Matrix4x4 pivotLocalMatrix = Matrix4x4.identity;
                if (parent.TryGetComponent<SpringBone>(out var parentBone))
                {
                    parentIndex = Array.FindIndex(manager.SortedBones, b => b == parentBone);
                    pivotIndex = parentIndex;
                }

                var pivotTransform = GetPivotTransform(springBone);
                var pivotBone = pivotTransform.GetComponentInParent<SpringBone>();
                if (pivotBone != null)
                {
//        var nsbEnabledJobField = nsb.GetType().GetField("enabledJobSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
// 
                    // NOTE: PivotがSpringBoneの子供に置かれている場合の対処
                    if (pivotBone.transform != pivotTransform) {
                        // NOTE: 1個上の親がSpringBoneとは限らない
                        //pivotLocalMatrix = Matrix4x4.TRS(pivotTransform.localPosition, pivotTransform.localRotation, Vector3.one);
                        pivotLocalMatrix = Matrix4x4.Inverse(pivotBone.transform.localToWorldMatrix) * pivotTransform.localToWorldMatrix;
                    }
                }

                // ReadOnly
                manager.jobProperties[i] = new SpringBoneProperties {
                    stiffnessForce = springBone.stiffnessForce,
                    dragForce = springBone.dragForce,
                    springForce = springBone.springForce,
                    windInfluence = springBone.windInfluence,
                    angularStiffness = springBone.angularStiffness,
                    yAngleLimits = new AngleLimitComponent {
                        active = springBone.yAngleLimits.active,
                        min = springBone.yAngleLimits.min,
                        max = springBone.yAngleLimits.max,
                    },
                    zAngleLimits = new AngleLimitComponent {
                        active = springBone.zAngleLimits.active,
                        min = springBone.zAngleLimits.min,
                        max = springBone.zAngleLimits.max,
                    },
                    radius = springBone.radius,
                    boneAxis = boneAxis,
                    springLength = springLength,
                    localPosition = root.localPosition,
                    initialLocalRotation = root.localRotation,
                    parentIndex = parentIndex,

                    pivotIndex = pivotIndex,
                    pivotLocalMatrix = pivotLocalMatrix,
                };

                manager.initLocalRotations[i] = root.localRotation;

                // turn off SpringBone component to let Job work
                springBone.enabled = false;
                //springBone.enabledJobSystem = true;
            }

            // Colliders
            manager.jobColliders = manager.GetComponentsInChildren<SpringCollider>(true);
            int nColliders = manager.jobColliders.Length;
            for (int i = 0; i < nColliders; ++i) {
                //manager.jobColliders[i].index = i;
                var comp = new SpringColliderProperties() {
                    type = manager.jobColliders[i].type,
                    radius = manager.jobColliders[i].radius,
                    width = manager.jobColliders[i].width,
                    height = manager.jobColliders[i].height,
                };
                manager.jobColProperties[i] = comp;
            }

            // LengthLimits
            manager.jobLengthProperties = jobLengthPropertiesList.ToArray();
        }

        public void DrawAngleLimits(SpringBone bone, AngleLimits angleLimits, float drawScale)
        {
            if (angleLimits.active)
            {
                var pivot = bone.GetPivotTransform();
                var forward = -pivot.right;
                var side = (angleLimits == bone.yAngleLimits) ? -pivot.up : -pivot.forward;
                DrawLimits(angleLimits, bone.transform.position, side, forward, drawScale);
            }
        }

        public void DrawLimits
        (
            AngleLimits angleLimits,
            Vector3 origin,
            Vector3 sideVector,
            Vector3 forwardVector,
            float drawScale
        )
        {
            DrawAngleLimit(origin, sideVector, forwardVector, angleLimits.min, drawScale);
            DrawAngleLimit(origin, sideVector, forwardVector, angleLimits.max, drawScale);
        }

        public static void DrawAngleLimit
        (
            Vector3 origin,
            Vector3 sideVector,
            Vector3 forwardVector,
            float angleLimit,
            float scale
        )
        {
            const int BaseIterationCount = 3;

            var lastPoint = origin + scale * forwardVector;
            var iterationCount = (Mathf.RoundToInt(Mathf.Abs(angleLimit) / 45f) + 1) * BaseIterationCount;
            var deltaAngle = angleLimit / iterationCount;
            var angle = deltaAngle;
            for (var iteration = 0; iteration < iterationCount; ++iteration)
            {
                var newPoint = origin + scale * GetAngleVector(sideVector, forwardVector, angle);
                UnityEditor.Handles.DrawLine(lastPoint, newPoint);
                lastPoint = newPoint;
                angle += deltaAngle;
            }
            UnityEditor.Handles.DrawLine(origin, lastPoint);
        }

        public static Vector3 GetAngleVector(Vector3 sideVector, Vector3 forwardVector, float degrees)
        {
            var radians = Mathf.Deg2Rad * degrees;
            return Mathf.Sin(radians) * sideVector + Mathf.Cos(radians) * forwardVector;
        }

        private List<SpringBone> selectedBones;
        private Vector3[] boneLines;
        public Color boneColor = Color.yellow;
        public Color colliderColor = Color.gray;
        public Color collisionColor = Color.red;
        public Color groundCollisionColor = Color.green;

        private static IList<Transform> GetValidChildren(Transform parent)
        {
            // Ignore SpringBonePivots
            var childCount = parent.childCount;
            var children = new List<Transform>(childCount);
            for (int childIndex = 0; childIndex < childCount; childIndex++)
            {
                var child = parent.GetChild(childIndex);
                if (child.GetComponent<SpringBonePivot>() == null)
                {
                    children.Add(child);
                }
            }
            return children;
        }

        public Vector3 ComputeChildPosition(SpringBone bone)
        {
            var children = GetValidChildren(bone.transform);
            var childCount = children.Count;

            if (childCount == 0)
            {
                // This should never happen
                Debug.LogWarning("SpringBone「" + name + "」に有効な子供がありません");
                return bone.transform.position + bone.transform.right * -0.1f;
            }

            if (childCount == 1)
            {
                return children[0].position;
            }

            var initialTailPosition = new Vector3(0f, 0f, 0f);
            var averageDistance = 0f;
            var selfPosition = bone.transform.position;
            for (int childIndex = 0; childIndex < childCount; childIndex++)
            {
                var childPosition = children[childIndex].position;
                initialTailPosition += childPosition;
                averageDistance += (childPosition - selfPosition).magnitude;
            }

            averageDistance /= childCount;
            initialTailPosition /= childCount;
            var selfToInitial = initialTailPosition - selfPosition;
            selfToInitial.Normalize();
            initialTailPosition = selfPosition + averageDistance * selfToInitial;
            return initialTailPosition;
        }

        public void DrawSpringBoneCollision(SpringBone bone)
        {
            var childPosition = ComputeChildPosition(bone);
            var worldRadius = bone.transform.TransformDirection(bone.radius, 0f, 0f).magnitude;
            // For picking
            Gizmos.DrawSphere(childPosition, worldRadius);

            UnityEditor.Handles.DrawWireDisc(childPosition, Vector3.up, worldRadius);
            UnityEditor.Handles.DrawWireDisc(childPosition, Vector3.right, worldRadius);
            UnityEditor.Handles.DrawWireDisc(childPosition, Vector3.forward, worldRadius);
            //UnityEditor.Handles.RadiusHandle(Quaternion.identity, childPosition, worldRadius);
        }

        private void DrawBones()
        {
            var manager = (SpringJobManager)target;
            var springBones = FindSpringBones(manager);
            float angleLimitDrawScale = 0.05f;

            // Draw each item by color to reduce Material.SetPass calls
            var boneCount = springBones.Length;
            IList<SpringBone> bonesToDraw = springBones;
            if (SpringBoneWindow.settings.onlyShowSelectedBones)
            {
                if (selectedBones == null) { selectedBones = new List<SpringBone>(boneCount); }
                selectedBones.Clear();
                var selection = UnityEditor.Selection.gameObjects;
                for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
                {
                    var bone = springBones[boneIndex];
                    var pivot = (bone.pivotNode != null) ? bone.pivotNode.gameObject : null;
                    if (selection.Contains(bone.gameObject) || selection.Contains(pivot))
                    {
                        selectedBones.Add(springBones[boneIndex]);
                    }
                }
                bonesToDraw = selectedBones;
                boneCount = bonesToDraw.Count;
            }

            UnityEditor.Handles.color = new Color(0.2f, 1f, 0.2f);
            for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
            {
                var bone = bonesToDraw[boneIndex];
                DrawAngleLimits(bone, bone.yAngleLimits, angleLimitDrawScale);
            }

            UnityEditor.Handles.color = new Color(0.7f, 0.7f, 1f);
            for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
            {
                var bone = bonesToDraw[boneIndex];
                DrawAngleLimits(bone, bone.zAngleLimits, angleLimitDrawScale);
            }

            var linePointCount = boneCount * 2;
            if (boneLines == null || boneLines.Length != linePointCount)
            {
                boneLines = new Vector3[linePointCount];
            }

            var pointIndex = 0;
            for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
            {
                var bone = bonesToDraw[boneIndex];
                var origin = bone.transform.position;
                var pivotForward = -bone.GetPivotTransform().right;
                boneLines[pointIndex] = origin;
                boneLines[pointIndex + 1] = origin + angleLimitDrawScale * pivotForward;
                pointIndex += 2;
            }
            UnityEditor.Handles.color = Color.gray;
            UnityEditor.Handles.DrawLines(boneLines);

            pointIndex = 0;
            for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
            {
                var bone = bonesToDraw[boneIndex];
                boneLines[pointIndex] = bone.transform.position;
                boneLines[pointIndex + 1] = bone.ComputeChildPosition();
                pointIndex += 2;
            }
            UnityEditor.Handles.color = boneColor;
            UnityEditor.Handles.DrawLines(boneLines);

            if (SpringBoneWindow.settings.showBoneSpheres)
            {
                Gizmos.color = new Color(0f, 0f, 0f, 0f);
                for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
                {
                    DrawSpringBoneCollision(bonesToDraw[boneIndex]);
                }
            }

            if (SpringBoneWindow.settings.showBoneNames)
            {
                for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
                {
                    var bone = bonesToDraw[boneIndex];
                    UnityEditor.Handles.Label(bone.transform.position, bone.name);
                }
            }
        }
    }
}
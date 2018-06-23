using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor;


namespace Smooth
{
    [CustomEditor(typeof(SmoothSync))]
    public class SmoothSyncEditor : Editor
    {
        bool showExtrapolation = false;
        bool showThresholds = false;
        bool showWhatToSync = false;
        bool showCompressions = false;
        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();

            SmoothSync myTarget = (SmoothSync)target;

            if (myTarget.childObjectToSync)
            {
                Color oldColor = GUI.contentColor;
                GUI.contentColor = Color.green;
                EditorGUILayout.LabelField("Syncing child", myTarget.childObjectToSync.name);
                GUI.contentColor = oldColor;
            }

            GUIContent contentWhenToUpdateTransform = new GUIContent("When to Update Transform", "Update will have smoother results but FixedUpdate might be better for physics.");
            myTarget.whenToUpdateTransform = (SmoothSync.WhenToUpdateTransform)EditorGUILayout.EnumPopup(contentWhenToUpdateTransform, myTarget.whenToUpdateTransform);
            GUIContent contentInterpolationBackTime = new GUIContent("Interpolation Back Time", "How much time in the past non-owned objects should be. This is so if you hit a latency spike, you still have a buffer of the interpolationBackTime of known States before you start extrapolating into the unknown. Increasing will make interpolation more likely to be used, decreasing will make extrapolation more likely to be used. In seconds.");
            myTarget.interpolationBackTime = EditorGUILayout.FloatField(contentInterpolationBackTime, myTarget.interpolationBackTime);
            GUIContent contentSendRate = new GUIContent("Send Rate", "How many times per second to send network updates.");
            myTarget.sendRate = EditorGUILayout.FloatField(contentSendRate, myTarget.sendRate);
            GUIContent contentPositionLerpSpeed = new GUIContent("Position Easing Speed", "How fast to ease to the new position on non-owned objects. 0 is never, 1 is instant.");
            myTarget.positionLerpSpeed = EditorGUILayout.Slider(contentPositionLerpSpeed, myTarget.positionLerpSpeed, 0.0f, 1.0f);
            GUIContent contentRotationLerpSpeed = new GUIContent("Rotation Easing Speed", "How fast to ease to the new rotation on non-owned objects. 0 is never, 1 is instant.");
            myTarget.rotationLerpSpeed = EditorGUILayout.Slider(contentRotationLerpSpeed, myTarget.rotationLerpSpeed, 0.0f, 1.0f);
            GUIContent contentScaleLerpSpeed = new GUIContent("Scale Easing Speed", "How fast to ease to the new scale on non-owned objects. 0 is never, 1 is instant.");
            myTarget.scaleLerpSpeed = EditorGUILayout.Slider(contentScaleLerpSpeed, myTarget.scaleLerpSpeed, 0.0f, 1.0f);
            GUIContent contentNetworkChannel = new GUIContent("Network Channel", "The channel to send network updates on.");
            myTarget.networkChannel = EditorGUILayout.IntField(contentNetworkChannel, myTarget.networkChannel);

            GUIContent contentChildObjectToSync = new GUIContent("Child Object to Sync", "Set this to sync a child object, leave blank to sync this object. Must leave one blank to sync the parent in order to sync children.");
            myTarget.childObjectToSync = (GameObject)EditorGUILayout.ObjectField(contentChildObjectToSync, myTarget.childObjectToSync, typeof(GameObject), true);

            GUIContent contentVariablesToSync = new GUIContent("Variables to Sync", "Fine tune what variables to sync.");
            showWhatToSync = EditorGUILayout.Foldout(showWhatToSync, contentVariablesToSync);
            if (showWhatToSync)
            {
                EditorGUI.indentLevel++;
                GUIContent contentSyncPosition = new GUIContent("Sync Position", "Fine tune what variables to sync.");
                myTarget.syncPosition = (SyncMode)EditorGUILayout.EnumPopup(contentSyncPosition, myTarget.syncPosition);
                GUIContent contentSyncRotation = new GUIContent("Sync Rotation", "Fine tune what variables to sync");
                myTarget.syncRotation = (SyncMode)EditorGUILayout.EnumPopup(contentSyncRotation, myTarget.syncRotation);
                GUIContent contentSyncScale = new GUIContent("Sync Scale", "Fine tune what variables to sync");
                myTarget.syncScale = (SyncMode)EditorGUILayout.EnumPopup(contentSyncScale, myTarget.syncScale);
                GUIContent contentSyncVelocity = new GUIContent("Sync Velocity", "Fine tune what variables to sync");
                myTarget.syncVelocity = (SyncMode)EditorGUILayout.EnumPopup(contentSyncVelocity, myTarget.syncVelocity);
                GUIContent contentSyncAngularVelocity = new GUIContent("Sync Angular Velocity", "Fine tune what variables to sync");
                myTarget.syncAngularVelocity = (SyncMode)EditorGUILayout.EnumPopup(contentSyncAngularVelocity, myTarget.syncAngularVelocity);
                EditorGUI.indentLevel--;
            }

            GUIContent contentExtrapolation = new GUIContent("Extrapolation", "Extrapolation is going into the unknown based on information we had in the past. Generally, you'll want extrapolation to help fill in missing information during lag spikes.");
            showExtrapolation = EditorGUILayout.Foldout(showExtrapolation, contentExtrapolation);
            if (showExtrapolation)
            {
                EditorGUI.indentLevel++;
                GUIContent contentExtrapolationMode = new GUIContent("Extrapolation Mode", "None: No extrapolation. Limited: Some extrapolation. Unlimited: Unlimited extrapolation.");
                myTarget.extrapolationMode = (SmoothSync.ExtrapolationMode) EditorGUILayout.EnumPopup(contentExtrapolationMode, myTarget.extrapolationMode);
                if (myTarget.extrapolationMode == SmoothSync.ExtrapolationMode.Limited)
                {
                    GUIContent contentUseExtrapolationTimeLimit = new GUIContent("Use Extrapolation Time Limit", "Whether or not you want to use extrapolationTimeLimit. You can use only the extrapolationTimeLimit and save a distance check every extrapolation frame.");
                    GUIContent contentUseExtrapolationDistanceLimit = new GUIContent("Use Extrapolation Distance Limit", "Whether or not you want to use extrapolationDistanceLimit. You can use only the extrapolationTimeLimit and save a distance check every extrapolation frame.");
                    GUIContent contentExtrapolationDistanceLimit = new GUIContent("Extrapolation Distance Limit", "How much distance into the future a non-owned object is allowed to extrapolate. In distance units.");
                    GUIContent contentExtrapolationTimeLimit = new GUIContent("Extrapolation Time Limit", "How much time into the future a non-owned object is allowed to extrapolate. In seconds.");
                    myTarget.useExtrapolationTimeLimit = EditorGUILayout.Toggle(contentUseExtrapolationTimeLimit, myTarget.useExtrapolationTimeLimit);
                    myTarget.extrapolationTimeLimit = EditorGUILayout.FloatField(contentExtrapolationTimeLimit, myTarget.extrapolationTimeLimit);
                    myTarget.useExtrapolationDistanceLimit = EditorGUILayout.Toggle(contentUseExtrapolationDistanceLimit, myTarget.useExtrapolationDistanceLimit);
                    myTarget.extrapolationDistanceLimit = EditorGUILayout.FloatField(contentExtrapolationDistanceLimit, myTarget.extrapolationDistanceLimit);
                }
                EditorGUI.indentLevel--;
            }

            GUIContent contentThresholds = new GUIContent("Thresholds", "Use thresholds to control when to send and set the transform.");
            showThresholds = EditorGUILayout.Foldout(showThresholds, contentThresholds);
            if (showThresholds)
            {
                EditorGUI.indentLevel++;
                GUIContent contentSnapPositionThreshold = new GUIContent("Snap Position Threshold", "If the position is more than snapThreshold units from the target position, it will jump to the target position immediately instead of easing. Set to 0 to not use at all. In distance units.");
                myTarget.snapPositionThreshold = EditorGUILayout.FloatField(contentSnapPositionThreshold, myTarget.snapPositionThreshold);
                GUIContent contentSnapRotationThreshold = new GUIContent("Snap Rotation Threshold", "If the rotation is more than snapThreshold units from the target rotation, it will jump to the target rotation immediately instead of easing. Set to 0 to not use at all. In degrees.");
                myTarget.snapRotationThreshold = EditorGUILayout.FloatField(contentSnapRotationThreshold, myTarget.snapRotationThreshold);
                GUIContent contentSnapScaleThreshold = new GUIContent("Snap Scale Threshold", "If the scale is more than snapThreshold units from the target scale, it will jump to the target scale immediately instead of easing. Set to 0 to not use at all. In degrees.");
                myTarget.snapScaleThreshold = EditorGUILayout.FloatField(contentSnapScaleThreshold, myTarget.snapScaleThreshold);

                GUIContent contentSendPositionThreshold = new GUIContent("Send Position Threshold", "A synced object's position is only sent if it is off from the last sent position by more than the threshold. In distance units.");
                myTarget.sendPositionThreshold = EditorGUILayout.FloatField(contentSendPositionThreshold, myTarget.sendPositionThreshold);
                GUIContent contentSendRotationThreshold = new GUIContent("Send Rotation Threshold", "A synced object's rotation is only sent if it is off from the last sent rotation by more than the threshold. In degrees.");
                myTarget.sendRotationThreshold = EditorGUILayout.FloatField(contentSendRotationThreshold, myTarget.sendRotationThreshold);
                GUIContent contentSendScaleThreshold = new GUIContent("Send Scale Threshold", "A synced object's scale is only sent if it is off from the last sent scale by more than the threshold. In distance units.");
                myTarget.sendScaleThreshold = EditorGUILayout.FloatField(contentSendScaleThreshold, myTarget.sendScaleThreshold);
                GUIContent contentSendVelocityThreshold = new GUIContent("Send Velocity Threshold", "A synced object's velocity is only sent if it is off from the last sent velocity by more than the threshold. In distance per second.");
                myTarget.sendVelocityThreshold = EditorGUILayout.FloatField(contentSendVelocityThreshold, myTarget.sendVelocityThreshold);
                GUIContent contentSendAngularVelocityThreshold = new GUIContent("Send Angular Velocity Threshold", "A synced object's angular velocity is only sent if it is off from the last sent angular velocity by more than the threshold. In degrees per second.");
                myTarget.sendAngularVelocityThreshold = EditorGUILayout.FloatField(contentSendAngularVelocityThreshold, myTarget.sendAngularVelocityThreshold);

                GUIContent contentReceivedPositionThreshold = new GUIContent("Received Position Threshold", "A synced object's position is only updated if it is off from the target position by more than the threshold. Set to 0 to always update. Usually keep at 0 unless you notice problems with backtracking on stops. In distance units.");
                myTarget.receivedPositionThreshold = EditorGUILayout.FloatField(contentReceivedPositionThreshold, myTarget.receivedPositionThreshold);
                GUIContent contentReceivedRotationThreshold = new GUIContent("Received Rotation Threshold", "A synced object's rotation is only updated if it is off from the target rotation by more than the threshold. Set to 0 to always update. Usually keep at 0 unless you notice problems with backtracking on stops. In degrees.");
                myTarget.receivedRotationThreshold = EditorGUILayout.FloatField(contentReceivedRotationThreshold, myTarget.receivedRotationThreshold);
                EditorGUI.indentLevel--;
            }

            GUIContent contentCompression = new GUIContent("Compression", "Convert floats sent over the network to Halfs, which use half as much bandwidth but are also half as precise. It'll start becoming noticeably inaccurate over ~500.");
            showCompressions = EditorGUILayout.Foldout(showCompressions, contentCompression);
            if (showCompressions)
            {
                EditorGUI.indentLevel++;
                GUIContent contentCompressPosition = new GUIContent("Compress Position", "Compress floats to save bandwidth.");
                myTarget.isPositionCompressed = EditorGUILayout.Toggle(contentCompressPosition, myTarget.isPositionCompressed);
                GUIContent contentCompressRotation = new GUIContent("Compress Rotation", "Compress floats to save bandwidth.");
                myTarget.isRotationCompressed = EditorGUILayout.Toggle(contentCompressRotation, myTarget.isRotationCompressed);
                GUIContent contentCompressScale = new GUIContent("Compress Scale", "Compress floats to save bandwidth.");
                myTarget.isScaleCompressed = EditorGUILayout.Toggle(contentCompressScale, myTarget.isScaleCompressed);
                GUIContent contentCompressVelocity = new GUIContent("Compress Velocity", "Compress floats to save bandwidth.");
                myTarget.isVelocityCompressed = EditorGUILayout.Toggle(contentCompressVelocity, myTarget.isVelocityCompressed);
                GUIContent contentCompressAngularVelocity = new GUIContent("Compress Angular Velocity", "Compress floats to save bandwidth.");
                myTarget.isAngularVelocityCompressed = EditorGUILayout.Toggle(contentCompressAngularVelocity, myTarget.isAngularVelocityCompressed);
                EditorGUI.indentLevel--;
            }
            EditorUtility.SetDirty(myTarget);
        }
    }
}
/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using EnergyBarToolkit;

namespace EnergyBarToolkit {

[CustomEditor(typeof(RepeatedRenderer3D))]
public class RepeatRenderer3DInspector : EnergyBarInspectorBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================

    private SerializedProperty textureIcon;
    private SerializedProperty textureSlot;
    
    private SerializedProperty tintIcon;
    private SerializedProperty tintSlot;
    
    private SerializedProperty repeatCount;
    private SerializedProperty repeatPositionDelta;
    
    private SerializedProperty growType;
    private SerializedProperty fillDirection;
    
//    private SerializedProperty effectBlink;
//    private SerializedProperty effectBlinkValue;
//    private SerializedProperty effectBlinkRatePerSecond;
//    private SerializedProperty effectBlinkColor;

    // ===========================================================
    // Constructors (Including Static Constructors)
    // ===========================================================

    // ===========================================================
    // Getters / Setters
    // ===========================================================

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    public override void OnEnable() {
        base.OnEnable();
        
        textureIcon = serializedObject.FindProperty("textureIcon");
        textureSlot = serializedObject.FindProperty("textureSlot");
        
        tintIcon = serializedObject.FindProperty("tintIcon");
        tintSlot = serializedObject.FindProperty("tintSlot");
        
        repeatCount = serializedObject.FindProperty("repeatCount");
        repeatPositionDelta = serializedObject.FindProperty("repeatPositionDelta");
        
        growType = serializedObject.FindProperty("growType");
        fillDirection = serializedObject.FindProperty("fillDirection");
        
    }
    
    public override void OnInspectorGUI() {
        serializedObject.Update();
        
        var t = target as RepeatedRenderer3D;
        
        if (MadGUI.Foldout("Textures", true)) {
            MadGUI.BeginBox();
            
            EditorGUILayout.BeginHorizontal();
            MadGUI.PropertyField(textureIcon, "Icon");
            EditorGUILayout.PropertyField(tintIcon, new GUIContent(""), new GUILayoutOption[] { GUILayout.Width(50) });
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            MadGUI.PropertyField(textureSlot, "Slot");
            EditorGUILayout.PropertyField(tintSlot, new GUIContent(""), new GUILayoutOption[] { GUILayout.Width(50) });
            EditorGUILayout.EndHorizontal();
            
            CheckTextureIsReadable(t.textureIcon);
            CheckTextureIsReadable(t.textureSlot);
            
            MadGUI.EndBox();
        }
        
        if (showPositionAndSize && MadGUI.Foldout("Position & Size", true)) {
            MadGUI.BeginBox();
            
            if (MadGUI.Button("Make Pixel-Perfect")) {
                t.transform.localPosition = MadMath.Round(t.transform.localPosition);
                t.transform.localScale = new Vector3(1, 1, 1);
                EditorUtility.SetDirty(t);
            }
            
            if (!IsAnchored()) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Anchor");
                if (GUILayout.Button("Create")) {
                    CreateAnchor();
                }
                EditorGUILayout.EndHorizontal();
            } else {
                var anchor = GetAnchor();
                var serAnchor = new SerializedObject(anchor);
                MadAnchorInspector.DrawInspector(serAnchor);
            }

            EditorGUI.BeginChangeCheck();
            t.pivot = (EnergyBar3DBase.Pivot) EditorGUILayout.EnumPopup("Pivot Point", t.pivot);
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(t);
            }
            
            MadGUI.PropertyField(guiDepth, "GUI Depth");
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Appearance", false)) {
            MadGUI.BeginBox();
            
            MadGUI.PropertyField(repeatCount, "Repeat Count");
            MadGUI.PropertyFieldVector2(repeatPositionDelta, "Icon Distance");
            
            MadGUI.PropertyField(growType, "Grow Type");
            MadGUI.ConditionallyEnabled(growType.enumValueIndex == (int) RepeatedRenderer3D.GrowType.Fill, () => {
                MadGUI.PropertyField(fillDirection, "Fill Direction");
            });
            
            FieldLabel();
            
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Effects", false)) {
            MadGUI.BeginBox();
            
            FieldSmoothEffect();
            
            MadGUI.EndBox();
        }
        
        EditorGUILayout.Space();
        
        serializedObject.ApplyModifiedProperties();
    }

    // ===========================================================
    // Methods
    // ===========================================================

    

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace
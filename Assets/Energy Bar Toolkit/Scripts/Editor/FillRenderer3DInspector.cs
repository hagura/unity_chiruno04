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

[CustomEditor(typeof(FilledRenderer3D))]
public class FillRenderer3DInspector : EnergyBarInspectorBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================

    private SerializedProperty textureBar;
    
    private SerializedProperty textureBarColorType;
    private SerializedProperty textureBarColor;
    private SerializedProperty textureBarGradient;
    
    private SerializedProperty growDirection;
    
    private SerializedProperty radialOffset;
    private SerializedProperty radialLength;
    
    private SerializedProperty effectBurn;
    private SerializedProperty effectBurnTextureBar;
    private SerializedProperty effectBurnTextureBarColor;
    
    private SerializedProperty effectBlink;
    private SerializedProperty effectBlinkValue;
    private SerializedProperty effectBlinkRatePerSecond;
    private SerializedProperty effectBlinkColor;
    
    private SerializedProperty effectFollow;
    private SerializedProperty effectFollowObject;
    private SerializedProperty effectFollowScaleX;
    private SerializedProperty effectFollowScaleY;
    private SerializedProperty effectFollowScaleZ;
    private SerializedProperty effectFollowRotation;
    private SerializedProperty effectFollowColor;
    
    private FilledRenderer3D script;

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
        
        textureBar = serializedObject.FindProperty("textureBar");
        
        textureBarColorType = serializedObject.FindProperty("textureBarColorType");
        textureBarColor = serializedObject.FindProperty("textureBarColor");
        textureBarGradient = serializedObject.FindProperty("textureBarGradient");
        
        growDirection = serializedObject.FindProperty("growDirection");
        
        radialOffset = serializedObject.FindProperty("radialOffset");
        radialLength = serializedObject.FindProperty("radialLength");
        
        effectBurn = serializedObject.FindProperty("effectBurn");
        effectBurnTextureBar = serializedObject.FindProperty("effectBurnTextureBar");
        effectBurnTextureBarColor = serializedObject.FindProperty("effectBurnTextureBarColor");
        
        effectBlink = serializedObject.FindProperty("effectBlink");
        effectBlinkValue = serializedObject.FindProperty("effectBlinkValue");
        effectBlinkRatePerSecond = serializedObject.FindProperty("effectBlinkRatePerSecond");
        effectBlinkColor = serializedObject.FindProperty("effectBlinkColor");
        
        effectFollow = serializedObject.FindProperty("effectFollow");
        effectFollowObject = serializedObject.FindProperty("effectFollowObject");
        effectFollowScaleX = serializedObject.FindProperty("effectFollowScaleX");
        effectFollowScaleY = serializedObject.FindProperty("effectFollowScaleY");
        effectFollowScaleZ = serializedObject.FindProperty("effectFollowScaleZ");
        effectFollowRotation = serializedObject.FindProperty("effectFollowRotation");
        effectFollowColor = serializedObject.FindProperty("effectFollowColor");
    }
    
    public override void OnInspectorGUI() {
        serializedObject.Update();
        
        script = target as FilledRenderer3D;
        
        if (MadGUI.Foldout("Textures", true)) {
            MadGUI.BeginBox();
            FieldBackgroundTextures();
            
            EditorGUILayout.PropertyField(textureBar, new GUIContent("Bar Texture"));         
            CheckTextureIsReadable(script.textureBar);
            CheckTextureFilterTypeNotPoint(script.textureBar);
            
            FieldForegroundTextures();
            
            FieldPremultipliedAlpha();
            MadGUI.EndBox();
        }
        
        if (showPositionAndSize && MadGUI.Foldout("Position & Size", true)) {
            MadGUI.BeginBox();
            
            if (MadGUI.Button("Make Pixel-Perfect")) {
                script.transform.localPosition = MadMath.Round(script.transform.localPosition);
                script.transform.localScale = new Vector3(1, 1, 1);
                EditorUtility.SetDirty(script);
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
            script.pivot = (EnergyBar3DBase.Pivot) EditorGUILayout.EnumPopup("Pivot Point", script.pivot);
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(script);
            }
            MadGUI.PropertyField(guiDepth, "GUI Depth");
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Appearance", false)) {
            MadGUI.BeginBox();
            
            var dir = (EnergyBarRenderer.GrowDirection) growDirection.enumValueIndex;
        
            if (dir == EnergyBarRenderer.GrowDirection.ColorChange) {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(textureBarColorType, new GUIContent("Bar Color Type"));
            EditorGUI.indentLevel++;
                switch ((EnergyBarRenderer.ColorType)textureBarColorType.enumValueIndex) {
                    case EnergyBarRenderer.ColorType.Solid:
                        EditorGUILayout.PropertyField(textureBarColor, new GUIContent("Bar Color"));
                        break;
                        
                    case EnergyBarRenderer.ColorType.Gradient:
                        EditorGUILayout.PropertyField(textureBarGradient, new GUIContent("Bar Gradient"));
                        break;
                }
                
            EditorGUI.indentLevel--;
            
            GUI.enabled = true;
            
            EditorGUILayout.PropertyField(growDirection, new GUIContent("Grow Direction"));
            
            if (dir == EnergyBarRenderer.GrowDirection.RadialCW || dir == EnergyBarRenderer.GrowDirection.RadialCCW) {
                MadGUI.Indent(() => {
                    EditorGUILayout.Slider(radialOffset, -1, 1, "Offset");
                    EditorGUILayout.Slider(radialLength, 0, 1, "Length");
                });
            } else if (dir == EnergyBarRenderer.GrowDirection.ColorChange) {
                EditorGUILayout.PropertyField(textureBarGradient, new GUIContent("Bar Gradient"));
            }
            
            FieldLabel();
            
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Effects", false)) {
            MadGUI.BeginBox();
            
            FieldSmoothEffect();
            
            MadGUI.PropertyFieldToggleGroup2(effectBurn, "Burn Out", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyField(effectBurnTextureBar, "Burn Texture Bar");
                    MadGUI.PropertyField(effectBurnTextureBarColor, "Burn Color");
                });
            });
            
            MadGUI.PropertyFieldToggleGroup2(effectBlink, "Blink", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyField(effectBlinkValue, "Value");
                    MadGUI.PropertyField(effectBlinkRatePerSecond, "Rate (per second)");
                    MadGUI.PropertyField(effectBlinkColor, "Color");
                });
            });
            
            MadGUI.PropertyFieldToggleGroup2(effectFollow, "Sprite Follow", () => {
                MadGUI.Indent(() => {
                    if (GUI.enabled && !GrowDirectionSupportedByFollowEffect()) {
                        MadGUI.Error("This effect cannot be used with selected grow direction. "
                            + "Please read manual for more information.");
                    }
                
                    MadGUI.PropertyField(effectFollowObject, "Texture or GameObject",
                        "This can be Texture, MadSprite or any other GameObject.");
                        
                    if (!VerifyFollowObject()) {
                        MadGUI.Error("You can put here only Texture2D or GameObject.");
                    }
                        
                    MadGUI.PropertyField(effectFollowColor, "Color");
                    MadGUI.PropertyField(effectFollowRotation, "Rotation");
                    if (MadGUI.Foldout("Scale", false)) {
                        MadGUI.Indent(() => {
                            MadGUI.PropertyField(effectFollowScaleX, "X");
                            MadGUI.PropertyField(effectFollowScaleY, "Y");
                            MadGUI.PropertyField(effectFollowScaleZ, "Z");
                        });
                    }
                    
                    
                });
            });
            
            MadGUI.EndBox();
        }
        
        EditorGUILayout.Space();
        
        serializedObject.ApplyModifiedProperties();
    }

    // ===========================================================
    // Methods
    // ===========================================================

    private bool GrowDirectionSupportedByFollowEffect() {
        EnergyBarBase.GrowDirection gd = (EnergyBarBase.GrowDirection) growDirection.enumValueIndex;
        return script.GrowDirectionSupportedByFollowEffect(gd);
    }
    
    private bool VerifyFollowObject() {
        var obj = effectFollowObject.objectReferenceValue;
        return obj == null || obj is Texture2D || obj is GameObject || obj is MadSprite;
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace
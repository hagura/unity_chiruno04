/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

public abstract class EnergyBar3DBase : EnergyBarBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================

    public virtual Pivot pivot {
        get {
            return _pivot;
        }
        
        set {
            _pivot = value;
        }
    }
    [SerializeField]
    private Pivot _pivot = Pivot.Center;
    
    
    // label
    public MadFont labelFont;
    public float labelScale = 32;
    
    public Pivot labelPivot = Pivot.Center;
    
    [SerializeField]
    private MadText labelSprite;

    //
    // editor properties
    //
    
    // determines if this bar is selectable in the editor
    public bool editorSelectable = true;

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    // ===========================================================
    // Methods
    // ===========================================================
    
    #if UNITY_EDITOR
    void OnDrawGizmos() {
        
        // Draw the gizmo
        Gizmos.matrix = transform.localToWorldMatrix;
        
        Gizmos.color = (UnityEditor.Selection.activeGameObject == gameObject)
            ? Color.green : new Color(1, 1, 1, 0.2f);
        
        var childSprites = MadTransform.FindChildren<MadSprite>(transform);
        Bounds totalBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool totalBoundsSet = false;
        
        foreach (var sprite in childSprites) {
            Rect boundsRect = sprite.GetBounds();
            boundsRect = MadMath.Translate(boundsRect, sprite.transform.localPosition);
            Bounds bounds = new Bounds(boundsRect.center, new Vector2(boundsRect.width, boundsRect.height));
            
            if (!totalBoundsSet) {
                totalBounds = bounds;
                totalBoundsSet = true;
            } else {
                totalBounds.Encapsulate(bounds);
            }
        }
        
        
        Gizmos.DrawWireCube(totalBounds.center, totalBounds.size);
        
        if (editorSelectable) {
            // Make the widget selectable
            Gizmos.color = Color.clear;
            Gizmos.DrawCube(totalBounds.center,
                            new Vector3(totalBounds.size.x, totalBounds.size.y, 0.01f * (guiDepth + 1)));
        }
    }
    #endif
    
    protected virtual void Update() {
        UpdateLabel();
        UpdateAnchor();
    }
    
    void UpdateLabel() {
        if (labelSprite == null) {
            return;
        }
        
        labelSprite.scale = labelScale;
        labelSprite.pivotPoint = Translate(labelPivot);
        labelSprite.transform.localPosition = LabelPositionPixels;
        
        labelSprite.text = LabelFormatResolve(labelFormat);
        labelSprite.tint = ComputeColor(labelColor);
    }
    
    protected int RebuildLabel(int depth) {
        if (labelSprite != null) {
            MadGameObject.SafeDestroy(labelSprite.gameObject);
        }
        
        if (labelEnabled && labelFont != null) {
            labelSprite = MadTransform.CreateChild<MadText>(transform, "label");
            labelSprite.font = labelFont;
            labelSprite.guiDepth = depth++;
            
            #if !MAD_DEBUG
            labelSprite.gameObject.hideFlags = HideFlags.HideInHierarchy;
            #endif
        }
        
        // after build we must update label at least once to make it visible
        UpdateLabel();
        
        return depth;
    }
    
    void UpdateAnchor() {
        // rewriting anchor objects to make them easy accessible
        var anchor = MadTransform.FindParent<MadAnchor>(transform, 1);
        if (anchor != null) {
            anchorObject = anchor.anchorObject;
            anchorCamera = anchor.anchorCamera;
        }
    }
    
    // ===========================================================
    // Static Methods
    // ===========================================================

    protected static MadSprite.PivotPoint Translate(Pivot pivot) {
        switch (pivot) {
            case Pivot.Left:
                return MadSprite.PivotPoint.Left;
            case Pivot.Top:
                return MadSprite.PivotPoint.Top;
            case Pivot.Right:
                return MadSprite.PivotPoint.Right;
            case Pivot.Bottom:
                return MadSprite.PivotPoint.Bottom;
            case Pivot.TopLeft:
                return MadSprite.PivotPoint.TopLeft;
            case Pivot.TopRight:
                return MadSprite.PivotPoint.TopRight;
            case Pivot.BottomRight:
                return MadSprite.PivotPoint.BottomRight;
            case Pivot.BottomLeft:
                return MadSprite.PivotPoint.BottomLeft;
            case Pivot.Center:
                return MadSprite.PivotPoint.Center;
            default:
                Debug.Log("Unknown pivot point: " + pivot);
                return MadSprite.PivotPoint.Center;
        }
    }
    
    protected static Vector2 PivotOffset(Pivot pivot) {
        switch (pivot) {
            case Pivot.Left:
                return new Vector2(0f, -0.5f);
            case Pivot.Top:
                return new Vector2(-0.5f, -1f);
            case Pivot.Right:
                return new Vector2(-1f, -0.5f);
            case Pivot.Bottom:
                return new Vector2(-0.5f, 0f);
            case Pivot.TopLeft:
                return new Vector2(0f, -1f);
            case Pivot.TopRight:
                return new Vector2(-1f, -1f);
            case Pivot.BottomRight:
                return new Vector2(-1f, 0f);
            case Pivot.BottomLeft:
                return new Vector2(0f, 0f);
            case Pivot.Center:
                return new Vector2(-0.5f, -0.5f);
            default:
                Debug.Log("Unknown pivot point: " + pivot);
                return Vector2.zero;
        }
    }

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
    
    // this intentionally shadows base declaration. The other Pivot order is just bad...
    public enum Pivot {
        Left,
        Top,
        Right,
        Bottom,
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft,
        Center,
    }
    
    public enum BarType {
        Filled,
        Repeated,
    }
}

} // namespace
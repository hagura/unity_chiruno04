/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

[ExecuteInEditMode]
[RequireComponent(typeof(EnergyBar))]
public class RepeatedRenderer3D : EnergyBar3DBase {

    // ===========================================================
    // Constants
    // ===========================================================
    
    // how much depth values will be reserved for single energy bar
    private const int DepthSpace = 32;

    // ===========================================================
    // Fields
    // ===========================================================
    
    // textures
    public Texture2D textureIcon;
    public Texture2D textureSlot;
    
    // appearance
    public Color tintIcon = Color.white;
    public Color tintSlot = Color.white;
    
    public int repeatCount = 5;
    public Vector2 repeatPositionDelta = new Vector2(32, 0);
    
    public GrowType growType;
    public MadSprite.FillType fillDirection;
    
    //
    // helpers
    //
    private float actualDisplayValue;
    
    // sprite references
    
    [SerializeField]
    private List<MadSprite> slotSprites = new List<MadSprite>();
    
    [SerializeField]
    private List<MadSprite> iconSprites = new List<MadSprite>();
    
    // rebuild
    
    [SerializeField]
    private int lastRebuildHash;
    
    private bool dirty = true;

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    public override Pivot pivot {
        get {
            return base.pivot;
        }
        set {
            bool rebuild = base.pivot != value;
            base.pivot = value;
            if (rebuild) {
                Rebuild();
            }
        }
    }
    
    public override Rect DrawAreaRect {
        get {
            Rect firstIconBounds;
            Vector2 iconSize;
        
            if (iconSprites != null && iconSprites.Count > 0 && iconSprites[0] != null) {
                firstIconBounds = iconSprites[0].GetBounds();
                iconSize = new Vector2(firstIconBounds.width, firstIconBounds.height);
            } else if (textureIcon != null) {
                // if icon sprites are not set, this means that rebuild is not done yet
                // trying to calculate it using textures sizes
                iconSize = new Vector2(textureIcon.width, textureIcon.height);
                firstIconBounds = new Rect(-iconSize.x / 2, iconSize.y / 2, iconSize.x, iconSize.y);
            } else {
                return new Rect();
            }
            
            var overallSize = ComputeDrawAreaSize(iconSize);
            var offset = LocalIconOffset(iconSize);
            
            return new Rect(
                firstIconBounds.x + offset.x, firstIconBounds.y + offset.y,
                overallSize.x, overallSize.y);
        }
    }

    // ===========================================================
    // Methods
    // ===========================================================

    new void Start() {
        base.Start();
    }

    #region Update

    protected override void Update() {
        base.Update();
    
        if (RebuildNeeded()) {
            Rebuild();
        }
        
        if (effectSmoothChange) {
            EnergyBarCommons.SmoothDisplayValue(ref actualDisplayValue, energyBar.ValueF, effectSmoothChangeSpeed);
        } else {
            actualDisplayValue = energyBar.ValueF;
        }
        
        UpdateBar();
        UpdateVisible();
    }
    
    void UpdateBar() {
        if (iconSprites == null || iconSprites.Count != repeatCount) {
            return;
        }
    
        float displayIconCountF = actualDisplayValue * repeatCount;
        int visibleIconCount = (int) Mathf.Floor(displayIconCountF);     // icons painted at full visibility
        float middleIconValue = displayIconCountF - visibleIconCount;
        
        for (int i = 0; i < repeatCount; ++i) {
            var sprite = iconSprites[i];
        
            if (i < visibleIconCount) {
                // this is visible sprite
                SetSpriteVisible(sprite);
            } else if (i > visibleIconCount) {
                // this is invisible sprite
                SetSpriteInvisible(sprite);
            } else {
                // this is partly-visible sprite
                SetSpriteVisible(sprite);
                
                switch (growType) {
                    case GrowType.None:
                        if (middleIconValue == 0) {
                            SetSpriteInvisible(sprite);
                        }
                        break;
                    case GrowType.Fade:
                        sprite.tint = Color.Lerp(IconTintTransparent, tintIcon, middleIconValue);
                        break;
                    case GrowType.Grow:
                        sprite.transform.localScale = new Vector3(middleIconValue, middleIconValue, middleIconValue);
                        break;
                    case GrowType.Fill:
                        sprite.fillType = fillDirection;
                        sprite.fillValue = middleIconValue;
                        break;
                        
                    default:
                        Debug.Log("Unknown grow type: " + growType);
                        break;
                }
            }
            
        }
    }
    
    void SetSpriteVisible(MadSprite sprite) {
        sprite.tint = tintIcon;
        sprite.fillValue = 1;
        sprite.transform.localScale = new Vector3(1, 1, 1);
        sprite.visible = true;
    }
    
    void SetSpriteInvisible(MadSprite sprite) {
        sprite.visible = false;
    }
    
    void UpdateVisible() {
        bool visible = IsVisible();
        
        if (!visible) {
            // make all sprites invisible
            // no need to make the oposite, sprites are visible by default
            foreach (var sprite in iconSprites) {
                sprite.visible = visible;
            }
            
            foreach (var sprite in slotSprites) {
                sprite.visible = visible;
            }
        }
    }
    
    Color IconTintTransparent {
        get {
            return new Color(tintIcon.r, tintIcon.g, tintIcon.g, 0);
        }
    }
    
    #endregion
    
    #region Rebuild
    
    bool RebuildNeeded() {
        var hash = new MadHashCode();
        hash.AddEnumerable(texturesBackground);
        hash.Add(textureIcon);
        hash.Add(textureSlot);
        hash.Add(repeatCount);
        hash.Add(repeatPositionDelta);
        hash.Add(guiDepth);
        hash.Add(growType);
        hash.Add(fillDirection);
        hash.Add(labelEnabled);
        hash.Add(labelFont);
        hash.Add(pivot);
        
        int hashNumber = hash.GetHashCode();
        
        if (hashNumber != lastRebuildHash || dirty) {
            lastRebuildHash = hashNumber;
            dirty = false;
            return true;
        } else {
            return false;
        }
    }
    
    void Rebuild() {
        #if MAD_DEBUG
        Debug.Log("rebuilding " + this, this);
        #endif
        
        RebuildClear();
        int depth = RebuildBuild();
        RebuildLabel(depth);
        
        UpdateBar();
    }
    
    void RebuildClear() {
        RemoveSprites(iconSprites);
        RemoveSprites(slotSprites);
    }
    
    void RemoveSprites(List<MadSprite> sprites) {
        foreach (var sprite in sprites) {
            RemoveSprite(sprite);
        }
        
        sprites.Clear();
    }
    
    void RemoveSprite(MadSprite sprite) {
        if (sprite != null) {
            MadGameObject.SafeDestroy(sprite.gameObject);
        }
    }
    
    int RebuildBuild() {
        int depth = guiDepth * DepthSpace;
        depth = BuildTextures(depth);
        return depth;
    }
    
    int BuildTextures(int depth) {
        for (int i = 0; i < repeatCount; ++i) {
        
            // creating slot
            if (textureSlot != null) {
                string name = string.Format("slot_{0:D2}", i + 1);
                var sprite = MadTransform.CreateChild<MadSprite>(transform, name);
                sprite.texture = textureSlot;
                sprite.transform.localPosition = repeatPositionDelta * i;
                sprite.transform.localPosition += (Vector3) LocalIconOffset(sprite.size);
                
                sprite.guiDepth = depth++;
                
                #if !MAD_DEBUG
                sprite.gameObject.hideFlags = HideFlags.HideInHierarchy;
                #endif
                
                slotSprites.Add(sprite);
            }
            
            // creating icon
            if (textureIcon != null) {
                string name = string.Format("icon_{0:D2}", i + 1);
                var sprite = MadTransform.CreateChild<MadSprite>(transform, name);
                sprite.texture = textureIcon;
                sprite.transform.localPosition = repeatPositionDelta * i;
                sprite.transform.localPosition += (Vector3) LocalIconOffset(sprite.size);
                sprite.guiDepth = depth++;
                
                #if !MAD_DEBUG
                sprite.gameObject.hideFlags = HideFlags.HideInHierarchy;
                #endif
                
                iconSprites.Add(sprite);
            }
        }
        
        return depth;
    }
    
    Vector2 LocalIconOffset(Vector2 iconSize) {
        Vector2 offset = new Vector2(iconSize.x / 2, iconSize.y / 2); // start with bottom-left
        
        Vector2 drawAreaSize = ComputeDrawAreaSize(iconSize);
        float x = drawAreaSize.x;
        float y = drawAreaSize.y;
        float x2 = x / 2;
        float y2 = y / 2;
    
        switch (pivot) {
            case Pivot.Left:
                return offset + new Vector2(0, -y2);
            case Pivot.Top:
                return offset + new Vector2(-x2, -y);
            case Pivot.Right:
                return offset + new Vector2(-x, -y2);
            case Pivot.Bottom:
                return offset + new Vector2(-x2, 0);
            case Pivot.TopLeft:
                return offset + new Vector2(0, -y);
            case Pivot.TopRight:
                return offset + new Vector2(-x, -y);
            case Pivot.BottomRight:
                return offset + new Vector2(-x, 0);
            case Pivot.BottomLeft:
                return offset;
            case Pivot.Center:
                return offset + new Vector2(-x2, -y2);
            default:
                Debug.Log("Unknown pivot point: " + pivot);
                break;
        }
        
        return Vector2.zero;
    }
    
    Vector2 ComputeDrawAreaSize(Vector2 iconSize) {
        return new Vector2(
            iconSize.x + (repeatPositionDelta.x * (repeatCount - 1)),
            iconSize.y + (repeatPositionDelta.y * (repeatCount - 1))
        );
    }
    
    #endregion

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
    
    public enum GrowType {
        None,
        Grow,
        Fade,
        Fill
    }
    
    public enum CutDirection {
        LeftToRight,
        TopToBottom,
        RightToLeft,
        BottomToTop
    }

}

} // namespace
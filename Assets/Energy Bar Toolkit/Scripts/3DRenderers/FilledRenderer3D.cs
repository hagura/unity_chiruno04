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
public class FilledRenderer3D : EnergyBar3DBase {

    // ===========================================================
    // Constants
    // ===========================================================
    
    // how much depth values will be reserved for single energy bar
    private const int DepthSpace = 32;
    
    // ===========================================================
    // Fields
    // ===========================================================
    
    
    //
    // textures
    //
    public Texture2D textureBar;
    
    //
    // appearance
    //
    public ColorType textureBarColorType;
    public Color textureBarColor = Color.white;
    public Gradient textureBarGradient;
    
    public GrowDirection growDirection = GrowDirection.LeftToRight;
    
    public float radialOffset;
    public float radialLength = 1;
    
    //
    // effect
    //
    
    // burn effect
    public bool effectBurn = false;                 // bar draining will display 'burn' effect
    public Texture2D effectBurnTextureBar;
    public Color effectBurnTextureBarColor = Color.red;
    private float burnDisplayValue;
    
    // blink effect
    public bool effectBlink = false;
    public float effectBlinkValue = 0.2f;
    public float effectBlinkRatePerSecond = 1f;
    public Color effectBlinkColor = new Color(1, 1, 1, 0);
    
    // sprite follow effect
    public bool effectFollow = false;
    public UnityEngine.Object effectFollowObject; // MadSprite, Texture2D or 
    public AnimationCurve effectFollowScaleX;
    public AnimationCurve effectFollowScaleY;
    public AnimationCurve effectFollowScaleZ;
    public AnimationCurve effectFollowRotation;
    public Gradient effectFollowColor;
    
    [SerializeField] private bool effectFollowDefaultsSet = false;
    // when texture is set, the sprite is created
    [SerializeField] private MadSprite effectFollowSprite;
    
    
    //
    // others
    //
    
#region Fields others
    private bool visible;

    [SerializeField]
    private int lastRebuildHash;
    
    private bool dirty = true;
    
    // sprite references
    
    [SerializeField]
    private MadSprite spriteBar;
    
    [SerializeField]
    private MadSprite spriteBurnBar;
    
    [SerializeField]
    private List<MadSprite> spriteObjectsBg = new List<MadSprite>();
    
    [SerializeField]
    private List<MadSprite> spriteObjectsFg = new List<MadSprite>();
#endregion
    
    // ===========================================================
    // Properties
    // ===========================================================
    
    public override Pivot pivot {
        get {
            return base.pivot;
        }
        set {
            bool update = base.pivot != value;
            base.pivot = value;
            if (update) {
                UpdatePivot();
            }
        }
    }
    
    float _burnDisplayValue;
    float ValueFBurn {
        get {
            EnergyBarCommons.SmoothDisplayValue(
                    ref _burnDisplayValue, ValueF2, effectSmoothChangeSpeed);
            _burnDisplayValue = Mathf.Max(_burnDisplayValue, ValueF2);
            return _burnDisplayValue;
        }
    }
    
    float _actualDisplayValue;
    float ValueF2 {
    
        get {
        
            if (effectBurn) {
                if (effectSmoothChange) {
                    // in burn mode smooth primary bar only when it's increasing
                    if (ValueF > _actualDisplayValue) {
                        EnergyBarCommons.SmoothDisplayValue(ref _actualDisplayValue, ValueF, effectSmoothChangeSpeed);
                    } else {
                        _actualDisplayValue = energyBar.ValueF;
                    }
                } else {
                    _actualDisplayValue = energyBar.ValueF;
                }
                
            } else {
                if (effectSmoothChange) {
                    EnergyBarCommons.SmoothDisplayValue(ref _actualDisplayValue, ValueF, effectSmoothChangeSpeed);
                } else {
                    _actualDisplayValue = energyBar.ValueF;
                }
            }
            
            return _actualDisplayValue;
        }
    }
    
    bool Blink {
        get; set;
    }
    
    // return current bar color based on color settings and effect
    float _effectBlinkAccum;
    Color BarColor {
        get {
            Color outColor = Color.white;
            
            if (growDirection == EnergyBarBase.GrowDirection.ColorChange) {
                outColor = textureBarGradient.Evaluate(energyBar.ValueF);
            } else {
                switch (textureBarColorType) {
                    case ColorType.Solid:
                        outColor = textureBarColor;
                        break;
                    case ColorType.Gradient:
                        outColor = textureBarGradient.Evaluate(energyBar.ValueF);
                        break;
                    default:
                        MadDebug.Assert(false, "Unkwnown option: " + textureBarColorType);
                        break;
                }
            }
            
            if (Blink) {
                outColor = effectBlinkColor;
            }
            
            return ComputeColor(outColor);
        }
    }
    
    Color BurnColor {
        get {
            Color outColor = effectBurnTextureBarColor;
            if (Blink) {
                outColor = new Color(0, 0, 0, 0);
            }
            
            return ComputeColor(outColor);
        }
    }
    
    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    public override Rect DrawAreaRect {
        get {
            if (spriteBar != null) {
                return spriteBar.GetBounds();
            } else if (textureBar != null) {
                // if there's no sprite set, but texture bar is set then this means that rebuild
                // is not done yet. Trying to calculate bounds manually.
                Vector2 offset = PivotOffset(pivot);
                float w = textureBar.width;
                float h = textureBar.height;
                return new Rect(offset.x * w, (1 - offset.y) * h, w, h);
            } else {
                return new Rect();
            }
        }
    }

    // ===========================================================
    // Methods
    // ===========================================================
    
    protected override void OnEnable() {
        base.OnEnable();
        if (!effectFollowDefaultsSet) {
            effectFollowScaleX = AnimationCurve.Linear(0, 1, 1, 1);
            effectFollowScaleY = AnimationCurve.Linear(0, 1, 1, 1);
            effectFollowScaleZ = AnimationCurve.Linear(0, 1, 1, 1);
            effectFollowRotation = AnimationCurve.Linear(0, 0, 1, 0);
            effectFollowDefaultsSet = true;
        }
    }
    
    protected override void Update() {
        base.Update();
    
        if (effectBlink) {
            Blink = EnergyBarCommons.Blink(
                ValueF, effectBlinkValue, effectBlinkRatePerSecond, ref _effectBlinkAccum);
        } else {
            Blink = false;
        }
    
        if (RebuildNeeded()) {
            Rebuild();
        }
        
        UpdateVisible();
        UpdateBar();
        UpdateColors();
        UpdatePivot();
        UpdateFollowEffect();
    }
    
    void UpdateVisible() {
        visible = IsVisible();
    }
    
    void UpdateBar() {
        if (effectBurn && spriteBurnBar != null) {
            spriteBurnBar.visible = visible;
            spriteBurnBar.tint = BurnColor;
            spriteBurnBar.fillValue = ValueFBurn;
        }
        
        if (spriteBar != null) {
            spriteBar.visible = visible;
            spriteBar.tint = BarColor;
            spriteBar.fillValue = ValueF2;
        }
    }
    
    void UpdateColors() {
        UpdateTextureColors(spriteObjectsBg, texturesBackground);
        UpdateTextureColors(spriteObjectsFg, texturesForeground);
    }
    
    void UpdateTextureColors(List<MadSprite> sprites, Tex[] textures) {
        if (sprites.Count != textures.Length) {
            Debug.LogWarning("Different number of sprites and registered textures. (" + sprites.Count + ", " + textures.Length + ")");
            return;
        }
        
        for (int i = 0; i < sprites.Count; i++) {
            var sprite = sprites[i];
            Tex texture = textures[i];
            
            sprite.visible = visible;
            sprite.tint = ComputeColor(texture.color);
        }
    }
    
    void UpdatePivot() {
        var pivot = Translate(this.pivot);
        foreach (var sprite in spriteObjectsBg) {
            sprite.pivotPoint = pivot;
        }
        
        foreach (var sprite in spriteObjectsFg) {
            sprite.pivotPoint = pivot;
        }
        
        if (spriteBar != null) {
            spriteBar.pivotPoint = pivot;
        }
        
        if (spriteBurnBar != null) {
            spriteBurnBar.pivotPoint = pivot;
        }
    }
    
    void UpdateFollowEffect() {
        if (!effectFollow) {
            return;
        }
        
        Color color = effectFollowColor.Evaluate(ValueF2);
        float scaleX = effectFollowScaleX.Evaluate(ValueF2);
        float scaleY = effectFollowScaleY.Evaluate(ValueF2);
        float scaleZ = effectFollowScaleZ.Evaluate(ValueF2);
        float rotation = effectFollowRotation.Evaluate(ValueF2) * 360;
        
        if (effectFollowSprite != null) {
            effectFollowSprite.transform.localPosition = EdgePosition();
            effectFollowSprite.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            effectFollowSprite.tint = color;
            effectFollowSprite.transform.localEulerAngles = new Vector3(0, 0, rotation);
        } else if (effectFollowObject != null && effectFollowObject is GameObject) {
            var worldPos = spriteBar.transform.TransformPoint(EdgePosition());
            GameObject obj = effectFollowObject as GameObject;
            obj.transform.position = worldPos;
            obj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            if (obj.renderer != null) {
                obj.renderer.sharedMaterial.color = color;
            }
            obj.transform.localEulerAngles = new Vector3(0, 0, rotation);
        }
        
        
    }
    
    bool RebuildNeeded() {
        var hash = new MadHashCode();
        hash.AddEnumerable(texturesBackground);
        hash.Add(textureBar);
        hash.AddEnumerable(texturesForeground);
        hash.Add(guiDepth);
        hash.Add(growDirection);
        hash.Add(effectBurn);
        hash.Add(labelEnabled);
        hash.Add(labelFont);
        hash.Add(effectFollow);
        hash.Add(effectFollowObject);
        
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
    
        if (spriteObjectsBg.Count == 0 && spriteObjectsFg.Count == 0) {
            // in previous version sprites were created without reference in spriteObjects
            // is spriteObjects is empty it's better to assume, that references are not created yet
            // and background objects may exist
            var children = MadTransform.FindChildren<MadSprite>(transform,
                (s) => s.name.StartsWith("bg_") || s.name.StartsWith("fg_") || s.name == "label",
                0);
            foreach (var child in children) {
                MadGameObject.SafeDestroy(child.gameObject);
            }
        } else {
            foreach (var sprite in spriteObjectsBg) {
                if (sprite != null) {
                    MadGameObject.SafeDestroy(sprite.gameObject);
                }
            }
            
            foreach (var sprite in spriteObjectsFg) {
                if (sprite != null) {
                    MadGameObject.SafeDestroy(sprite.gameObject);
                }
            }
            
            spriteObjectsBg.Clear();
            spriteObjectsFg.Clear();
        }
        
        if (spriteBar != null) {
            MadGameObject.SafeDestroy(spriteBar.gameObject);
        }
        
        if (spriteBurnBar != null) {
            MadGameObject.SafeDestroy(spriteBurnBar.gameObject);
        }
        
        if (effectFollowSprite != null) {
            MadGameObject.SafeDestroy(effectFollowSprite.gameObject);
        }
        
        int nextDepth = BuildTextures(texturesBackground, "bg_", guiDepth * DepthSpace, ref spriteObjectsBg);
        
        if (textureBar != null) {
        
            if (effectBurn) {
                spriteBurnBar = MadTransform.CreateChild<MadSprite>(transform, "bar_effect_burn");
#if !MAD_DEBUG
                spriteBurnBar.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
                spriteBurnBar.guiDepth = nextDepth++;
                spriteBurnBar.texture = textureBar;
                
                spriteBurnBar.fillType = ToFillType(growDirection);
            }
        
            spriteBar = MadTransform.CreateChild<MadSprite>(transform, "bar");
#if !MAD_DEBUG
            spriteBar.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
            spriteBar.guiDepth = nextDepth++;
            spriteBar.texture = textureBar;
            
            spriteBar.fillType = ToFillType(growDirection);
        }
        
        nextDepth = BuildTextures(texturesForeground, "fg_", nextDepth, ref spriteObjectsFg);
        
        if (effectFollow) {
            if (effectFollowObject != null && effectFollowObject is Texture2D) {
                effectFollowSprite = MadTransform.CreateChild<MadSprite>(transform, "bar_effect_follow");
                effectFollowSprite.texture = effectFollowObject as Texture2D;
                effectFollowSprite.guiDepth = nextDepth++;
                #if !MAD_DEBUG
                effectFollowSprite.gameObject.hideFlags = HideFlags.HideInHierarchy;
                #endif
            }
        }
        
        nextDepth = RebuildLabel(nextDepth);
    }
    
    MadSprite.FillType ToFillType(GrowDirection growDirection) {
        switch (growDirection) {
            case GrowDirection.LeftToRight:
                return MadSprite.FillType.LeftToRight;
            case GrowDirection.RightToLeft:
                return MadSprite.FillType.RightToLeft;
            case GrowDirection.TopToBottom:
                return MadSprite.FillType.TopToBottom;
            case GrowDirection.BottomToTop:
                return MadSprite.FillType.BottomToTop;
            case GrowDirection.ExpandHorizontal:
                return MadSprite.FillType.ExpandHorizontal;
            case GrowDirection.ExpandVertical:
                return MadSprite.FillType.ExpandVertical;
            case GrowDirection.RadialCW:
                return MadSprite.FillType.RadialCW;
            case GrowDirection.RadialCCW:
                return MadSprite.FillType.RadialCCW;
            case GrowDirection.ColorChange:
                return MadSprite.FillType.None;
            default:
                MadDebug.Assert(false, "Unkwnown grow direction: " + growDirection);
                return MadSprite.FillType.None;
        }
    }
    
    int BuildTextures(Tex[] textures, string prefix, int startDepth, ref List<MadSprite> sprites) {
        
        int counter = 0;
        foreach (var texture in textures) {
            if (texture.texture == null) {
                continue;
            }
        
            string name = string.Format("{0}{1:D2}", prefix, counter + 1);
            var sprite = MadTransform.CreateChild<MadSprite>(transform, name);
#if !MAD_DEBUG
            sprite.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
            
            sprite.guiDepth = startDepth + counter;
            sprite.texture = texture.texture;
            sprite.tint = texture.color;
            
            sprites.Add(sprite);
            
            counter++;
        }
        
        return startDepth + counter;
    }
    
    public bool GrowDirectionSupportedByFollowEffect(GrowDirection growDirection) {
        switch (growDirection) {
            case EnergyBarBase.GrowDirection.LeftToRight:
            case EnergyBarBase.GrowDirection.RightToLeft:
            case EnergyBarBase.GrowDirection.TopToBottom:
            case EnergyBarBase.GrowDirection.BottomToTop:
                return true;
            default:
                return false;
        }
    }
    
    Vector2 EdgePosition() {
        if (spriteBar == null) {
            return Vector2.zero;
        }
        
        float left = spriteBar.liveLeft * spriteBar.size.x;
        float right = spriteBar.liveRight * spriteBar.size.x;
        float top = spriteBar.liveTop * spriteBar.size.y;
        float bottom = spriteBar.liveBottom * spriteBar.size.y;
        float width = right - left;
        float height = top - bottom;
        float centerX = left + width / 2;
        float centerY = bottom + height / 2;
        
        Vector2 pos;
        
        switch (growDirection) {
        case GrowDirection.LeftToRight:
            pos = new Vector2(left + width * ValueF2, centerY);
            break;
        case GrowDirection.RightToLeft:
            pos = new Vector2(left + width * (1 - ValueF2), centerY);
            break;
        case GrowDirection.BottomToTop:
            pos = new Vector2(centerX, bottom + height * (1 - ValueF2));
            break;
        case GrowDirection.TopToBottom:
            pos = new Vector2(centerX, bottom + height * ValueF2);
            break;
        default:
            pos = Vector2.zero;
            break;
        }
        
        var offset = ComputeOffset();
        
        return new Vector2(pos.x - offset.x, -pos.y + offset.y);
    }
    
    Vector2 ComputeOffset() {
        var dar = DrawAreaRect;
        switch (pivot) {
            case Pivot.Left:
                return new Vector2(0, dar.height / 2);
            case Pivot.Top:
                return new Vector2(dar.width / 2, 0);
            case Pivot.Right:
                return new Vector2(dar.width, dar.height / 2);
            case Pivot.Bottom:
                return new Vector2(dar.width / 2, dar.height);
            case Pivot.TopLeft:
                return Vector2.zero;
            case Pivot.TopRight:
                return new Vector2(dar.width, 0);
            case Pivot.BottomRight:
                return new Vector2(dar.width, dar.height);
            case Pivot.BottomLeft:
                return new Vector2(0, dar.height);
            case Pivot.Center:
                return new Vector2(dar.width / 2, dar.height / 2);
            default:
                Debug.LogError("Unknown pivot: " + pivot);
                return Vector2.zero;
        }
    }
    
    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace
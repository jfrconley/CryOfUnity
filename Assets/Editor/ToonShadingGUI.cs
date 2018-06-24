using UnityEngine;
using UnityEditor;

public class ToonShadingGUI : ShaderGUI {

    Material target;
    MaterialEditor editor;
    MaterialProperty[] properties;

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        this.target = editor.target as Material;
        this.editor = editor;
        this.properties = properties;
        RenderMain();
        RenderSpecular();
        RenderToonRamp();
        RenderRimLight();
    }

    void RenderMain()
    {
        GUILayout.Label("Main Maps", EditorStyles.boldLabel);

        MaterialProperty mainTex = FindProperty("_MainTex");
        editor.TexturePropertySingleLine(MakeLabel(mainTex, "Albedo (RGB)"), mainTex, FindProperty("_Color"));
        RenderNormal();
        //RenderEmission();
        RenderOcclusion();
        editor.TextureScaleOffsetProperty(mainTex);
    }

    void RenderNormal()
    {
        MaterialProperty normal = FindProperty("_NormalMap");
        editor.TexturePropertySingleLine(MakeLabel(normal), normal, normal.textureValue ? FindProperty("_NormalScale") : null);
    }

    void RenderToonRamp()
    {
        GUILayout.Label("Toon Ramp", EditorStyles.boldLabel);
        MaterialProperty ramp = FindProperty("_Ramp");
        editor.TexturePropertySingleLine(MakeLabel(ramp), ramp);
        editor.ColorProperty(FindProperty("_HColor"), "Highlight");
        editor.ColorProperty(FindProperty("_SColor"), "Shadow");
        editor.RangeProperty(FindProperty("_Saturation"), "Saturation");
    }

    void RenderRimLight()
    {
        GUILayout.Label("Rim Lighting", EditorStyles.boldLabel);
        editor.ColorProperty(FindProperty("_RimColor"), "Rim Color");
        editor.RangeProperty(FindProperty("_RimMin"), "Rim Min");
        editor.RangeProperty(FindProperty("_RimMax"), "Rim Max");
    }

    void RenderSpecular()
    {
        GUILayout.Label("Specular", EditorStyles.boldLabel);
        editor.RangeProperty(FindProperty("_SpecAmount"), "Specular");
        editor.RangeProperty(FindProperty("_Roughness"), "Roughness");
    }

    void RenderEmission()
    {
        MaterialProperty map = FindProperty("_EmissionMap");
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine( MakeLabel(map, "Emission (RGB)"), map, FindProperty("_Emission") );
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_EMISSION_MAP", map.textureValue);
        }
    }

    void RenderOcclusion()
    {
        MaterialProperty map = FindProperty("_OcclusionMap");
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(
            MakeLabel(map, "Occlusion (G)"), map,
            map.textureValue ? FindProperty("_OcclusionStrength") : null
        );
        /*if (EditorGUI.EndChangeCheck())
        {
            SetKeyword("_OCCLUSION_MAP", map.textureValue);
        }*/
    }

    void SetKeyword(string keyword, bool state)
    {
        if (state)
        {
            target.EnableKeyword(keyword);
        }
        else
        {
            target.DisableKeyword(keyword);
        }
    }

    MaterialProperty FindProperty(string name)
    {
        return FindProperty(name, properties);
    }

    static GUIContent staticLabel = new GUIContent();

    static GUIContent MakeLabel(string text, string tooltip = null)
    {
        staticLabel.text = text;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    static GUIContent MakeLabel( MaterialProperty property, string tooltip = null )
    {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }
}

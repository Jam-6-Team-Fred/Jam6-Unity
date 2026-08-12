// Decompiled with JetBrains decompiler
// Type: UnityEditor.StandardShaderGUI
// Assembly: UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C1DA2D3B-3136-4FE3-9064-9500FCB0EFE5
// Assembly location: C:\Program Files\Unity\Hub\Editor\2019.4.39f1\Editor\Data\Managed\UnityEditor.dll
// XML documentation location: C:\Program Files\Unity\Hub\Editor\2019.4.39f1\Editor\Data\Managed\UnityEditor.xml

using System;
using System.Linq; // CHANGED
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
  internal class StandardShaderGUI : ShaderGUI
  {
    private MaterialProperty blendMode = (MaterialProperty) null;
    private MaterialProperty albedoMap = (MaterialProperty) null;
    private MaterialProperty albedoColor = (MaterialProperty) null;
    private MaterialProperty alphaCutoff = (MaterialProperty) null;
    private MaterialProperty specularMap = (MaterialProperty) null;
    private MaterialProperty specularColor = (MaterialProperty) null;
    private MaterialProperty metallicMap = (MaterialProperty) null;
    private MaterialProperty metallic = (MaterialProperty) null;
    private MaterialProperty smoothness = (MaterialProperty) null;
    private MaterialProperty smoothnessScale = (MaterialProperty) null;
    private MaterialProperty smoothnessMapChannel = (MaterialProperty) null;
    private MaterialProperty highlights = (MaterialProperty) null;
    private MaterialProperty reflections = (MaterialProperty) null;
    private MaterialProperty bumpScale = (MaterialProperty) null;
    private MaterialProperty bumpMap = (MaterialProperty) null;
    private MaterialProperty occlusionStrength = (MaterialProperty) null;
    private MaterialProperty occlusionMap = (MaterialProperty) null;
    private MaterialProperty heigtMapScale = (MaterialProperty) null;
    private MaterialProperty heightMap = (MaterialProperty) null;
    private MaterialProperty emissionColorForRendering = (MaterialProperty) null;
    private MaterialProperty emissionMap = (MaterialProperty) null;
    private MaterialProperty detailMask = (MaterialProperty) null;
    private MaterialProperty detailAlbedoMap = (MaterialProperty) null;
    private MaterialProperty detailNormalMapScale = (MaterialProperty) null;
    private MaterialProperty detailNormalMap = (MaterialProperty) null;
    private MaterialProperty uvSetSecondary = (MaterialProperty) null;
    
    // CHANGED
    private MaterialProperty _VertexColorAlbedo = (MaterialProperty) null;
    private MaterialProperty _VertexColorSpecular = (MaterialProperty) null;
    private MaterialProperty _VertexOcclusion = (MaterialProperty) null;
    
    private MaterialEditor m_MaterialEditor;
    private StandardShaderGUI.WorkflowMode m_WorkflowMode = StandardShaderGUI.WorkflowMode.Specular;
    private bool m_FirstTimeApply = true;

    public void FindProperties(MaterialProperty[] props)
    {
      this.blendMode = ShaderGUI.FindProperty("_Mode", props);
      this.albedoMap = ShaderGUI.FindProperty("_MainTex", props);
      this.albedoColor = ShaderGUI.FindProperty("_Color", props);
      this.alphaCutoff = ShaderGUI.FindProperty("_Cutoff", props);
      this.specularMap = ShaderGUI.FindProperty("_SpecGlossMap", props, false);
      this.specularColor = ShaderGUI.FindProperty("_SpecColor", props, false);
      this.metallicMap = ShaderGUI.FindProperty("_MetallicGlossMap", props, false);
      this.metallic = ShaderGUI.FindProperty("_Metallic", props, false);
      this.m_WorkflowMode = this.specularMap == null || this.specularColor == null ? (this.metallicMap == null || this.metallic == null ? StandardShaderGUI.WorkflowMode.Dielectric : StandardShaderGUI.WorkflowMode.Metallic) : StandardShaderGUI.WorkflowMode.Specular;
      this.smoothness = ShaderGUI.FindProperty("_Glossiness", props);
      this.smoothnessScale = ShaderGUI.FindProperty("_GlossMapScale", props, false);
      this.smoothnessMapChannel = ShaderGUI.FindProperty("_SmoothnessTextureChannel", props, false);
      this.highlights = ShaderGUI.FindProperty("_SpecularHighlights", props, false);
      this.reflections = ShaderGUI.FindProperty("_GlossyReflections", props, false);
      this.bumpScale = ShaderGUI.FindProperty("_BumpScale", props);
      this.bumpMap = ShaderGUI.FindProperty("_BumpMap", props);
      this.heigtMapScale = ShaderGUI.FindProperty("_Parallax", props);
      this.heightMap = ShaderGUI.FindProperty("_ParallaxMap", props);
      this.occlusionStrength = ShaderGUI.FindProperty("_OcclusionStrength", props);
      this.occlusionMap = ShaderGUI.FindProperty("_OcclusionMap", props);
      this.emissionColorForRendering = ShaderGUI.FindProperty("_EmissionColor", props);
      this.emissionMap = ShaderGUI.FindProperty("_EmissionMap", props);
      this.detailMask = ShaderGUI.FindProperty("_DetailMask", props);
      this.detailAlbedoMap = ShaderGUI.FindProperty("_DetailAlbedoMap", props);
      this.detailNormalMapScale = ShaderGUI.FindProperty("_DetailNormalMapScale", props);
      this.detailNormalMap = ShaderGUI.FindProperty("_DetailNormalMap", props);
      this.uvSetSecondary = ShaderGUI.FindProperty("_UVSec", props);
      
      // CHANGED
      this._VertexColorAlbedo = ShaderGUI.FindProperty("_VertexColorAlbedo", props, false);
      this._VertexColorSpecular = ShaderGUI.FindProperty("_VertexColorSpecular", props, false);
      this._VertexOcclusion = ShaderGUI.FindProperty("_VertexOcclusion", props, false);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
      this.FindProperties(props);
      this.m_MaterialEditor = materialEditor;
      Material target = materialEditor.target as Material;
      if (this.m_FirstTimeApply)
      {
        StandardShaderGUI.MaterialChanged(target, this.m_WorkflowMode, false);
        this.m_FirstTimeApply = false;
        
        // CHANGED
        if (_VertexColorAlbedo == null)
        {
          Debug.Log("Switching to Outer Wilds Standard shader");
          var shader = AssetDatabase.FindAssets("t:shader Standard")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(x => x.EndsWith("Standard.asset") || x.EndsWith("Standard_0.asset"))
            .Select(AssetDatabase.LoadAssetAtPath<Shader>)
            .FirstOrDefault(x => x.FindPropertyIndex("_VertexColorAlbedo") != -1);
          if (shader != null)
          {
            materialEditor.SetShader(shader, false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
          }
        }
      }
      this.ShaderPropertiesGUI(target);
    }

    public void ShaderPropertiesGUI(Material material)
    {
      EditorGUIUtility.labelWidth = 0.0f;
      EditorGUI.BeginChangeCheck();
      
      // CHANGED
      if (_VertexColorAlbedo != null)
      {
        m_MaterialEditor.ShaderProperty(_VertexColorAlbedo, _VertexColorAlbedo.displayName);
        m_MaterialEditor.ShaderProperty(_VertexColorSpecular, _VertexColorSpecular.displayName);
        m_MaterialEditor.ShaderProperty(_VertexOcclusion, _VertexOcclusion.displayName);
      }
      else
      {
        EditorGUILayout.HelpBox("Cannot find Outer Wilds Standard shader.\nThis material will be displayed incorrectly in editor.", MessageType.Warning);
      }
      EditorGUILayout.Space(16);
      
      bool overrideRenderQueue = this.BlendModePopup();
      GUILayout.Label(StandardShaderGUI.Styles.primaryMapsText, EditorStyles.boldLabel);
      this.DoAlbedoArea(material);
      this.DoSpecularMetallicArea();
      this.DoNormalArea();
      this.m_MaterialEditor.TexturePropertySingleLine(StandardShaderGUI.Styles.heightMapText, this.heightMap, (UnityEngine.Object) this.heightMap.textureValue != (UnityEngine.Object) null ? this.heigtMapScale : (MaterialProperty) null);
      this.m_MaterialEditor.TexturePropertySingleLine(StandardShaderGUI.Styles.occlusionText, this.occlusionMap, (UnityEngine.Object) this.occlusionMap.textureValue != (UnityEngine.Object) null ? this.occlusionStrength : (MaterialProperty) null);
      this.m_MaterialEditor.TexturePropertySingleLine(StandardShaderGUI.Styles.detailMaskText, this.detailMask);
      this.DoEmissionArea(material);
      EditorGUI.BeginChangeCheck();
      this.m_MaterialEditor.TextureScaleOffsetProperty(this.albedoMap);
      if (EditorGUI.EndChangeCheck())
        this.emissionMap.textureScaleAndOffset = this.albedoMap.textureScaleAndOffset;
      EditorGUILayout.Space();
      GUILayout.Label(StandardShaderGUI.Styles.secondaryMapsText, EditorStyles.boldLabel);
      this.m_MaterialEditor.TexturePropertySingleLine(StandardShaderGUI.Styles.detailAlbedoText, this.detailAlbedoMap);
      this.m_MaterialEditor.TexturePropertySingleLine(StandardShaderGUI.Styles.detailNormalMapText, this.detailNormalMap, this.detailNormalMapScale);
      this.m_MaterialEditor.TextureScaleOffsetProperty(this.detailAlbedoMap);
      this.m_MaterialEditor.ShaderProperty(this.uvSetSecondary, StandardShaderGUI.Styles.uvSetLabel.text);
      GUILayout.Label(StandardShaderGUI.Styles.forwardText, EditorStyles.boldLabel);
      if (this.highlights != null)
        this.m_MaterialEditor.ShaderProperty(this.highlights, StandardShaderGUI.Styles.highlightsText);
      if (this.reflections != null)
        this.m_MaterialEditor.ShaderProperty(this.reflections, StandardShaderGUI.Styles.reflectionsText);
      EditorGUILayout.Space();
      GUILayout.Label(StandardShaderGUI.Styles.advancedText, EditorStyles.boldLabel);
      this.m_MaterialEditor.RenderQueueField();
      if (EditorGUI.EndChangeCheck())
      {
        foreach (Material target in this.blendMode.targets)
          StandardShaderGUI.MaterialChanged(target, this.m_WorkflowMode, overrideRenderQueue);
      }
      this.m_MaterialEditor.EnableInstancingField();
      this.m_MaterialEditor.DoubleSidedGIField();
    }

    internal void DetermineWorkflow(MaterialProperty[] props)
    {
      if (ShaderGUI.FindProperty("_SpecGlossMap", props, false) != null && ShaderGUI.FindProperty("_SpecColor", props, false) != null)
        this.m_WorkflowMode = StandardShaderGUI.WorkflowMode.Specular;
      else if (ShaderGUI.FindProperty("_MetallicGlossMap", props, false) != null && ShaderGUI.FindProperty("_Metallic", props, false) != null)
        this.m_WorkflowMode = StandardShaderGUI.WorkflowMode.Metallic;
      else
        this.m_WorkflowMode = StandardShaderGUI.WorkflowMode.Dielectric;
    }

    public override void AssignNewShaderToMaterial(
      Material material,
      Shader oldShader,
      Shader newShader)
    {
      if (material.HasProperty("_Emission"))
        material.SetColor("_EmissionColor", material.GetColor("_Emission"));
      base.AssignNewShaderToMaterial(material, oldShader, newShader);
      if ((UnityEngine.Object) oldShader == (UnityEngine.Object) null || !oldShader.name.Contains("Legacy Shaders/"))
      {
        StandardShaderGUI.SetupMaterialWithBlendMode(material, (StandardShaderGUI.BlendMode) material.GetFloat("_Mode"), true);
      }
      else
      {
        StandardShaderGUI.BlendMode blendMode = StandardShaderGUI.BlendMode.Opaque;
        if (oldShader.name.Contains("/Transparent/Cutout/"))
          blendMode = StandardShaderGUI.BlendMode.Cutout;
        else if (oldShader.name.Contains("/Transparent/"))
          blendMode = StandardShaderGUI.BlendMode.Fade;
        material.SetFloat("_Mode", (float) blendMode);
        this.DetermineWorkflow(MaterialEditor.GetMaterialProperties((UnityEngine.Object[]) new Material[1]
        {
          material
        }));
        StandardShaderGUI.MaterialChanged(material, this.m_WorkflowMode, true);
      }
    }

    private bool BlendModePopup()
    {
      EditorGUI.showMixedValue = this.blendMode.hasMixedValue;
      StandardShaderGUI.BlendMode floatValue = (StandardShaderGUI.BlendMode) this.blendMode.floatValue;
      EditorGUI.BeginChangeCheck();
      StandardShaderGUI.BlendMode blendMode = (StandardShaderGUI.BlendMode) EditorGUILayout.Popup(StandardShaderGUI.Styles.renderingMode, (int) floatValue, StandardShaderGUI.Styles.blendNames);
      bool flag = EditorGUI.EndChangeCheck();
      if (flag)
      {
        this.m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
        this.blendMode.floatValue = (float) blendMode;
      }
      EditorGUI.showMixedValue = false;
      return flag;
    }

    private void DoNormalArea()
    {
      this.m_MaterialEditor.TexturePropertySingleLine(StandardShaderGUI.Styles.normalMapText, this.bumpMap, (UnityEngine.Object) this.bumpMap.textureValue != (UnityEngine.Object) null ? this.bumpScale : (MaterialProperty) null);
      if ((double) this.bumpScale.floatValue == 1.0 || !InternalEditorUtility.IsMobilePlatform(EditorUserBuildSettings.activeBuildTarget) || !this.m_MaterialEditor.HelpBoxWithButton(EditorGUIUtility.TrTextContent("Bump scale is not supported on mobile platforms"), EditorGUIUtility.TrTextContent("Fix Now")))
        return;
      this.bumpScale.floatValue = 1f;
    }

    private void DoAlbedoArea(Material material)
    {
      this.m_MaterialEditor.TexturePropertySingleLine(StandardShaderGUI.Styles.albedoText, this.albedoMap, this.albedoColor);
      if ((int) material.GetFloat("_Mode") != 1)
        return;
      this.m_MaterialEditor.ShaderProperty(this.alphaCutoff, StandardShaderGUI.Styles.alphaCutoffText.text, 3);
    }

    private void DoEmissionArea(Material material)
    {
      if (!this.m_MaterialEditor.EmissionEnabledProperty())
        return;
      bool flag = (UnityEngine.Object) this.emissionMap.textureValue != (UnityEngine.Object) null;
      this.m_MaterialEditor.TexturePropertyWithHDRColor(StandardShaderGUI.Styles.emissionText, this.emissionMap, this.emissionColorForRendering, false);
      float maxColorComponent = this.emissionColorForRendering.colorValue.maxColorComponent;
      if ((UnityEngine.Object) this.emissionMap.textureValue != (UnityEngine.Object) null && !flag && (double) maxColorComponent <= 0.0)
        this.emissionColorForRendering.colorValue = Color.white;
      this.m_MaterialEditor.LightmapEmissionFlagsProperty(2, true);
    }

    private void DoSpecularMetallicArea()
    {
      bool flag1 = false;
      if (this.m_WorkflowMode == StandardShaderGUI.WorkflowMode.Specular)
      {
        flag1 = (UnityEngine.Object) this.specularMap.textureValue != (UnityEngine.Object) null;
        this.m_MaterialEditor.TexturePropertySingleLine(StandardShaderGUI.Styles.specularMapText, this.specularMap, flag1 ? (MaterialProperty) null : this.specularColor);
      }
      else if (this.m_WorkflowMode == StandardShaderGUI.WorkflowMode.Metallic)
      {
        flag1 = (UnityEngine.Object) this.metallicMap.textureValue != (UnityEngine.Object) null;
        this.m_MaterialEditor.TexturePropertySingleLine(StandardShaderGUI.Styles.metallicMapText, this.metallicMap, flag1 ? (MaterialProperty) null : this.metallic);
      }
      bool flag2 = flag1;
      if (this.smoothnessMapChannel != null && (int) this.smoothnessMapChannel.floatValue == 1)
        flag2 = true;
      int labelIndent1 = 2;
      this.m_MaterialEditor.ShaderProperty(flag2 ? this.smoothnessScale : this.smoothness, flag2 ? StandardShaderGUI.Styles.smoothnessScaleText : StandardShaderGUI.Styles.smoothnessText, labelIndent1);
      int labelIndent2 = labelIndent1 + 1;
      if (this.smoothnessMapChannel == null)
        return;
      this.m_MaterialEditor.ShaderProperty(this.smoothnessMapChannel, StandardShaderGUI.Styles.smoothnessMapChannelText, labelIndent2);
    }

    public static void SetupMaterialWithBlendMode(
      Material material,
      StandardShaderGUI.BlendMode blendMode,
      bool overrideRenderQueue)
    {
      int num1 = -1;
      int num2 = 5000;
      int num3 = -1;
      switch (blendMode)
      {
        case StandardShaderGUI.BlendMode.Opaque:
          material.SetOverrideTag("RenderType", "");
          material.SetInt("_SrcBlend", 1);
          material.SetInt("_DstBlend", 0);
          material.SetInt("_ZWrite", 1);
          material.DisableKeyword("_ALPHATEST_ON");
          material.DisableKeyword("_ALPHABLEND_ON");
          material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
          num1 = -1;
          num2 = 2449;
          num3 = -1;
          break;
        case StandardShaderGUI.BlendMode.Cutout:
          material.SetOverrideTag("RenderType", "TransparentCutout");
          material.SetInt("_SrcBlend", 1);
          material.SetInt("_DstBlend", 0);
          material.SetInt("_ZWrite", 1);
          material.EnableKeyword("_ALPHATEST_ON");
          material.DisableKeyword("_ALPHABLEND_ON");
          material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
          num1 = 2450;
          num2 = 2500;
          num3 = 2450;
          break;
        case StandardShaderGUI.BlendMode.Fade:
          material.SetOverrideTag("RenderType", "Transparent");
          material.SetInt("_SrcBlend", 5);
          material.SetInt("_DstBlend", 10);
          material.SetInt("_ZWrite", 0);
          material.DisableKeyword("_ALPHATEST_ON");
          material.EnableKeyword("_ALPHABLEND_ON");
          material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
          num1 = 2501;
          num2 = 3999;
          num3 = 3000;
          break;
        case StandardShaderGUI.BlendMode.Transparent:
          material.SetOverrideTag("RenderType", "Transparent");
          material.SetInt("_SrcBlend", 1);
          material.SetInt("_DstBlend", 10);
          material.SetInt("_ZWrite", 0);
          material.DisableKeyword("_ALPHATEST_ON");
          material.DisableKeyword("_ALPHABLEND_ON");
          material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
          num1 = 2501;
          num2 = 3999;
          num3 = 3000;
          break;
      }
      if (!overrideRenderQueue && material.renderQueue >= num1 && material.renderQueue <= num2)
        return;
      if (!overrideRenderQueue)
        Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, (UnityEngine.Object) null, "Render queue value outside of the allowed range ({0} - {1}) for selected Blend mode, resetting render queue to default", (object) num1, (object) num2);
      material.renderQueue = num3;
    }

    private static StandardShaderGUI.SmoothnessMapChannel GetSmoothnessMapChannel(Material material) => (int) material.GetFloat("_SmoothnessTextureChannel") == 1 ? StandardShaderGUI.SmoothnessMapChannel.AlbedoAlpha : StandardShaderGUI.SmoothnessMapChannel.SpecularMetallicAlpha;

    private static void SetMaterialKeywords(
      Material material,
      StandardShaderGUI.WorkflowMode workflowMode)
    {
      StandardShaderGUI.SetKeyword(material, "_NORMALMAP", (bool) (UnityEngine.Object) material.GetTexture("_BumpMap") || (bool) (UnityEngine.Object) material.GetTexture("_DetailNormalMap"));
      switch (workflowMode)
      {
        case StandardShaderGUI.WorkflowMode.Specular:
          StandardShaderGUI.SetKeyword(material, "_SPECGLOSSMAP", (bool) (UnityEngine.Object) material.GetTexture("_SpecGlossMap"));
          break;
        case StandardShaderGUI.WorkflowMode.Metallic:
          StandardShaderGUI.SetKeyword(material, "_METALLICGLOSSMAP", (bool) (UnityEngine.Object) material.GetTexture("_MetallicGlossMap"));
          break;
      }
      StandardShaderGUI.SetKeyword(material, "_PARALLAXMAP", (bool) (UnityEngine.Object) material.GetTexture("_ParallaxMap"));
      StandardShaderGUI.SetKeyword(material, "_DETAIL_MULX2", (bool) (UnityEngine.Object) material.GetTexture("_DetailAlbedoMap") || (bool) (UnityEngine.Object) material.GetTexture("_DetailNormalMap"));
      MaterialEditor.FixupEmissiveFlag(material);
      bool state = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == MaterialGlobalIlluminationFlags.None;
      StandardShaderGUI.SetKeyword(material, "_EMISSION", state);
      if (!material.HasProperty("_SmoothnessTextureChannel"))
        return;
      StandardShaderGUI.SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", StandardShaderGUI.GetSmoothnessMapChannel(material) == StandardShaderGUI.SmoothnessMapChannel.AlbedoAlpha);
    }

    private static void MaterialChanged(
      Material material,
      StandardShaderGUI.WorkflowMode workflowMode,
      bool overrideRenderQueue)
    {
      StandardShaderGUI.SetupMaterialWithBlendMode(material, (StandardShaderGUI.BlendMode) material.GetFloat("_Mode"), overrideRenderQueue);
      StandardShaderGUI.SetMaterialKeywords(material, workflowMode);
    }

    private static void SetKeyword(Material m, string keyword, bool state)
    {
      if (state)
        m.EnableKeyword(keyword);
      else
        m.DisableKeyword(keyword);
    }

    private enum WorkflowMode
    {
      Specular,
      Metallic,
      Dielectric,
    }

    public enum BlendMode
    {
      Opaque,
      Cutout,
      Fade,
      Transparent,
    }

    public enum SmoothnessMapChannel
    {
      SpecularMetallicAlpha,
      AlbedoAlpha,
    }

    private static class Styles
    {
      public static GUIContent uvSetLabel = EditorGUIUtility.TrTextContent("UV Set");
      public static GUIContent albedoText = EditorGUIUtility.TrTextContent("Albedo", "Albedo (RGB) and Transparency (A)");
      public static GUIContent alphaCutoffText = EditorGUIUtility.TrTextContent("Alpha Cutoff", "Threshold for alpha cutoff");
      public static GUIContent specularMapText = EditorGUIUtility.TrTextContent("Specular", "Specular (RGB) and Smoothness (A)");
      public static GUIContent metallicMapText = EditorGUIUtility.TrTextContent("Metallic", "Metallic (R) and Smoothness (A)");
      public static GUIContent smoothnessText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness value");
      public static GUIContent smoothnessScaleText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness scale factor");
      public static GUIContent smoothnessMapChannelText = EditorGUIUtility.TrTextContent("Source", "Smoothness texture and channel");
      public static GUIContent highlightsText = EditorGUIUtility.TrTextContent("Specular Highlights", "Specular Highlights");
      public static GUIContent reflectionsText = EditorGUIUtility.TrTextContent("Reflections", "Glossy Reflections");
      public static GUIContent normalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
      public static GUIContent heightMapText = EditorGUIUtility.TrTextContent("Height Map", "Height Map (G)");
      public static GUIContent occlusionText = EditorGUIUtility.TrTextContent("Occlusion", "Occlusion (G)");
      public static GUIContent emissionText = EditorGUIUtility.TrTextContent("Color", "Emission (RGB)");
      public static GUIContent detailMaskText = EditorGUIUtility.TrTextContent("Detail Mask", "Mask for Secondary Maps (A)");
      public static GUIContent detailAlbedoText = EditorGUIUtility.TrTextContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
      public static GUIContent detailNormalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
      public static string primaryMapsText = "Main Maps";
      public static string secondaryMapsText = "Secondary Maps";
      public static string forwardText = "Forward Rendering Options";
      public static string renderingMode = "Rendering Mode";
      public static string advancedText = "Advanced Options";
      public static readonly string[] blendNames = Enum.GetNames(typeof (StandardShaderGUI.BlendMode));
    }
  }
}

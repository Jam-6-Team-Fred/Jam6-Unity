using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WorldSpaceMask : Mask
{
	[NonSerialized]
	private Material m_WorldMaskMaterial;

	[NonSerialized]
	private Material m_WorldUnmaskMaterial;

	protected override void OnDisable()
	{
		base.OnDisable();
		StencilMaterial.Remove(m_WorldMaskMaterial);
		m_WorldMaskMaterial = null;
		StencilMaterial.Remove(m_WorldUnmaskMaterial);
		m_WorldUnmaskMaterial = null;
		MaskUtilities.NotifyStencilStateChanged(this);
	}

	public override Material GetModifiedMaterial(Material baseMaterial)
	{
		if (!MaskEnabled())
		{
			return baseMaterial;
		}
		Transform transform = MaskUtilities.FindRootSortOverrideCanvas(base.transform);
		if (transform.GetComponent<Canvas>().renderMode != RenderMode.WorldSpace)
		{
			return base.GetModifiedMaterial(baseMaterial);
		}
		int stencilDepth = MaskUtilities.GetStencilDepth(base.transform, transform);
		if (stencilDepth >= 4)
		{
			Debug.LogError("World-space stencil masks cannot be used with a depth greater than 4.", base.gameObject);
			return baseMaterial;
		}
		int num = 1 << stencilDepth;
		int num2 = 15;
		if (stencilDepth == 0)
		{
			Material worldMaskMaterial = StencilMaterial.Add(baseMaterial, num, StencilOp.Replace, CompareFunction.Always, base.showMaskGraphic ? ColorWriteMask.All : ((ColorWriteMask)0), num2, num2);
			StencilMaterial.Remove(m_WorldMaskMaterial);
			m_WorldMaskMaterial = worldMaskMaterial;
			Material worldUnmaskMaterial = StencilMaterial.Add(baseMaterial, num, StencilOp.Zero, CompareFunction.Always, (ColorWriteMask)0, num2, num2);
			StencilMaterial.Remove(m_WorldUnmaskMaterial);
			m_WorldUnmaskMaterial = worldUnmaskMaterial;
			base.graphic.canvasRenderer.popMaterialCount = 1;
			base.graphic.canvasRenderer.SetPopMaterial(m_WorldUnmaskMaterial, 0);
			return m_WorldMaskMaterial;
		}
		Material worldMaskMaterial2 = StencilMaterial.Add(baseMaterial, num | (num - 1), StencilOp.Replace, CompareFunction.Equal, base.showMaskGraphic ? ColorWriteMask.All : ((ColorWriteMask)0), num - 1, num | (num - 1));
		StencilMaterial.Remove(m_WorldMaskMaterial);
		m_WorldMaskMaterial = worldMaskMaterial2;
		base.graphic.canvasRenderer.hasPopInstruction = true;
		Material worldUnmaskMaterial2 = StencilMaterial.Add(baseMaterial, num - 1, StencilOp.Replace, CompareFunction.Equal, (ColorWriteMask)0, num - 1, num | (num - 1));
		StencilMaterial.Remove(m_WorldUnmaskMaterial);
		m_WorldUnmaskMaterial = worldUnmaskMaterial2;
		base.graphic.canvasRenderer.popMaterialCount = 1;
		base.graphic.canvasRenderer.SetPopMaterial(m_WorldUnmaskMaterial, 0);
		return m_WorldMaskMaterial;
	}
}

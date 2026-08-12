using System;
using System.Collections.Generic;
using UnityEngine;

public static class SystemDisplay
{
	private static bool s_initialized;

	private static List<AspectRatio> s_availableAspectRatios;

	private static List<Resolution> s_unknownAspectResolutions;

	private static List<Resolution> s_twentyoneNineResolutions;

	private static List<Resolution> s_sixteenNineResolutions;

	private static List<Resolution> s_sixteenTenResolutions;

	private static List<Resolution> s_fourThreeResolutions;

	private static List<Resolution> s_fiveFourResolutions;

	private static Resolution _bestAvailableResolution;

	private static void Initialize()
	{
		if (s_availableAspectRatios == null)
		{
			s_availableAspectRatios = new List<AspectRatio>();
		}
		else
		{
			s_availableAspectRatios.Clear();
		}
		if (s_unknownAspectResolutions == null)
		{
			s_unknownAspectResolutions = new List<Resolution>();
		}
		else
		{
			s_unknownAspectResolutions.Clear();
		}
		if (s_twentyoneNineResolutions == null)
		{
			s_twentyoneNineResolutions = new List<Resolution>();
		}
		else
		{
			s_twentyoneNineResolutions.Clear();
		}
		if (s_sixteenNineResolutions == null)
		{
			s_sixteenNineResolutions = new List<Resolution>();
		}
		else
		{
			s_sixteenNineResolutions.Clear();
		}
		if (s_sixteenTenResolutions == null)
		{
			s_sixteenTenResolutions = new List<Resolution>();
		}
		else
		{
			s_sixteenTenResolutions.Clear();
		}
		if (s_fourThreeResolutions == null)
		{
			s_fourThreeResolutions = new List<Resolution>();
		}
		else
		{
			s_fourThreeResolutions.Clear();
		}
		if (s_fiveFourResolutions == null)
		{
			s_fiveFourResolutions = new List<Resolution>();
		}
		else
		{
			s_fiveFourResolutions.Clear();
		}
		int num = -1;
		Resolution bestAvailableResolution = default(Resolution);
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			if (num == -1)
			{
				bestAvailableResolution = resolution;
				num = resolution.width * resolution.height;
			}
			else if (resolution.width * resolution.height > bestAvailableResolution.width * bestAvailableResolution.height)
			{
				bestAvailableResolution = resolution;
				num = resolution.width * resolution.height;
			}
			AspectRatio aspectRatioFromResolution = GetAspectRatioFromResolution(resolution);
			List<Resolution> list = null;
			switch (aspectRatioFromResolution)
			{
			case AspectRatio.UNKNOWN:
				list = s_unknownAspectResolutions;
				break;
			case AspectRatio.TWENTYONE_NINE:
				list = s_twentyoneNineResolutions;
				break;
			case AspectRatio.SIXTEEN_NINE:
				list = s_sixteenNineResolutions;
				break;
			case AspectRatio.SIXTEEN_TEN:
				list = s_sixteenTenResolutions;
				break;
			case AspectRatio.FOUR_THREE:
				list = s_fourThreeResolutions;
				break;
			case AspectRatio.FIVE_FOUR:
				list = s_fiveFourResolutions;
				break;
			default:
				Debug.LogWarning("ASPECT RATIO NOT PROPERLY HANDLED; ADDING TO UNKNOWN");
				list = s_unknownAspectResolutions;
				break;
			}
			if (!ContainsEquivalentResolution(list, resolution))
			{
				list.Add(resolution);
			}
		}
		if (num != -1)
		{
			_bestAvailableResolution = bestAvailableResolution;
		}
		if (s_unknownAspectResolutions.Count > 0)
		{
			s_availableAspectRatios.Add(AspectRatio.UNKNOWN);
		}
		if (s_twentyoneNineResolutions.Count > 0)
		{
			s_availableAspectRatios.Add(AspectRatio.TWENTYONE_NINE);
		}
		if (s_sixteenNineResolutions.Count > 0)
		{
			s_availableAspectRatios.Add(AspectRatio.SIXTEEN_NINE);
		}
		if (s_sixteenTenResolutions.Count > 0)
		{
			s_availableAspectRatios.Add(AspectRatio.SIXTEEN_TEN);
		}
		if (s_fiveFourResolutions.Count > 0)
		{
			s_availableAspectRatios.Add(AspectRatio.FIVE_FOUR);
		}
		if (s_fourThreeResolutions.Count > 0)
		{
			s_availableAspectRatios.Add(AspectRatio.FOUR_THREE);
		}
		s_initialized = true;
	}

	public static Resolution GetDefaultResolution()
	{
		if (!s_initialized)
		{
			Initialize();
		}
		return _bestAvailableResolution;
	}

	public static bool IsResolutionAvailable(Resolution r)
	{
		return IsResolutionAvailable(r.width, r.height);
	}

	public static bool IsResolutionAvailable(float width, float height)
	{
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			if (width == (float)resolution.width && height == (float)resolution.height)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsResolutionAvailable(float width, float height, int refreshRate)
	{
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			if (width == (float)resolution.width && height == (float)resolution.height && refreshRate == resolution.refreshRate)
			{
				return true;
			}
		}
		return false;
	}

	public static Resolution[] GetResolutionsWithAspect(AspectRatio aspectRatio)
	{
		if (!s_initialized)
		{
			Initialize();
		}
		switch (aspectRatio)
		{
		case AspectRatio.UNKNOWN:
			return SortResolutions(s_unknownAspectResolutions);
		case AspectRatio.TWENTYONE_NINE:
			return SortResolutions(s_twentyoneNineResolutions);
		case AspectRatio.SIXTEEN_NINE:
			return SortResolutions(s_sixteenNineResolutions);
		case AspectRatio.SIXTEEN_TEN:
			return SortResolutions(s_sixteenTenResolutions);
		case AspectRatio.FOUR_THREE:
			return SortResolutions(s_fourThreeResolutions);
		case AspectRatio.FIVE_FOUR:
			return SortResolutions(s_fiveFourResolutions);
		default:
			return new Resolution[0];
		}
	}

	public static bool AreResolutionsAvailableWithAspect(AspectRatio aspectRatio)
	{
		if (!s_initialized)
		{
			Initialize();
		}
		switch (aspectRatio)
		{
		case AspectRatio.UNKNOWN:
			return s_unknownAspectResolutions.Count > 0;
		case AspectRatio.TWENTYONE_NINE:
			return s_twentyoneNineResolutions.Count > 0;
		case AspectRatio.SIXTEEN_NINE:
			return s_sixteenNineResolutions.Count > 0;
		case AspectRatio.SIXTEEN_TEN:
			return s_sixteenTenResolutions.Count > 0;
		case AspectRatio.FOUR_THREE:
			return s_fourThreeResolutions.Count > 0;
		case AspectRatio.FIVE_FOUR:
			return s_fiveFourResolutions.Count > 0;
		default:
			return false;
		}
	}

	public static AspectRatio GetAspectRatioFromResolution(Resolution res)
	{
		return GetAspectRatioFromResolution(res.width, res.height);
	}

	public static AspectRatio GetAspectRatioFromResolution(int width, int height)
	{
		float num = Convert.ToSingle(width) / Convert.ToSingle(height);
		if (num <= 1.2f || num >= 2.7f)
		{
			return AspectRatio.UNKNOWN;
		}
		AspectRatio result = AspectRatio.UNKNOWN;
		float num2 = float.PositiveInfinity;
		float num3 = Math.Abs(num - 1.25f);
		if (num3 < num2)
		{
			result = AspectRatio.FIVE_FOUR;
			num2 = num3;
		}
		num3 = Math.Abs(num - 1.3333334f);
		if (num3 < num2)
		{
			result = AspectRatio.FOUR_THREE;
			num2 = num3;
		}
		num3 = Math.Abs(num - 1.6f);
		if (num3 < num2)
		{
			result = AspectRatio.SIXTEEN_TEN;
			num2 = num3;
		}
		num3 = Math.Abs(num - 1.7777778f);
		if (num3 < num2)
		{
			result = AspectRatio.SIXTEEN_NINE;
			num2 = num3;
		}
		num3 = Math.Abs(num - 2.3333333f);
		if (num3 < num2)
		{
			result = AspectRatio.TWENTYONE_NINE;
			num2 = num3;
		}
		return result;
	}

	public static AspectRatio[] GetAvailableAspectRatioList()
	{
		if (!s_initialized)
		{
			Initialize();
		}
		return s_availableAspectRatios.ToArray();
	}

	private static Resolution[] SortResolutions(List<Resolution> listResolutions)
	{
		listResolutions.Sort(CompareResolutionsByBestOption);
		return listResolutions.ToArray();
	}

	private static bool ContainsEquivalentResolution(List<Resolution> list, Resolution resolutionToCheck)
	{
		for (int i = 0; i < list.Count; i++)
		{
			bool num = list[i].width == resolutionToCheck.width;
			bool flag = list[i].height == resolutionToCheck.height;
			if (num && flag)
			{
				return true;
			}
		}
		return false;
	}

	private static int CompareResolutionsByBestOption(Resolution r1, Resolution r2)
	{
		float num = r1.height * r1.width;
		float num2 = r2.height * r2.width;
		if (num > num2)
		{
			return 1;
		}
		if (num < num2)
		{
			return -1;
		}
		return 0;
	}
}

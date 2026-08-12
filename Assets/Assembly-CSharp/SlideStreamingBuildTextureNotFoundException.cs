using System;

public class SlideStreamingBuildTextureNotFoundException : Exception
{
	public string slideTextureName;

	public string slideCollectionName;

	public override string Message => "Unable to find texture " + slideTextureName + " for " + slideCollectionName + " in any slide streaming texture table.";

	public SlideStreamingBuildTextureNotFoundException(string slideTex, string slideColl)
	{
		slideTextureName = slideTex;
		slideCollectionName = slideColl;
	}
}

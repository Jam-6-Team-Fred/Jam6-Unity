using UnityEngine;
using UnityEditor;
using System.Collections;

public class FogTexGen : EditorWindow {
	private int _sizeX = 64;
	private int _sizeY = 64;
	private int _sizeZ = 64;

	private AnimationCurve densityCurve = AnimationCurve.EaseInOut(0.0f, 1.0f, 1.0f, 0.0f );
	private string _path = "Assets/";

	private float _NumSamples = 100.0f;

	[MenuItem( "Tools/Asset/Fog Lookup Texture Generator", false )]
	public static void CreateWindow()
	{
		FogTexGen window = EditorWindow.GetWindow<FogTexGen>( true, "Atmosphere Lookup Texture Generator" );

		window.minSize = new Vector2( 400.0f, 200.0f );
		window.maxSize = window.minSize;

		window.Show();
	}

	private void OnGUI()
	{
		Rect gradientRect = EditorGUILayout.GetControlRect( true, 80.0f );
		densityCurve = EditorGUI.CurveField( gradientRect, "Fog Density by Altitude", densityCurve );
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		EditorGUIUtility.labelWidth = 48.0f;
		EditorGUILayout.LabelField( "Texture Dimensions" );
		EditorGUILayout.BeginHorizontal();
		{
			_sizeX = Mathf.Clamp( EditorGUILayout.IntField( "Width", _sizeX ), 0, 4096 );
			_sizeY = Mathf.Clamp( EditorGUILayout.IntField( "Height", _sizeY ), 0, 4096 );
			_sizeZ = Mathf.Clamp( EditorGUILayout.IntField( "Depth", _sizeZ ), 0, 4096 );
		}
		EditorGUILayout.EndHorizontal();

		int textureSize = ( ( _sizeX*_sizeY*_sizeZ ) * 4 ) / 1024;
		EditorGUILayout.HelpBox( "Texture size: " + textureSize + " KB (" + ((float)textureSize/1024.0f).ToString("G2") + " MB)", MessageType.Info );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Generate Lookup Texture" ) )
		{
			GenerateTexture();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}

	private void GenerateTexture()
	{
		Color[] pixels = new Color[_sizeX*_sizeY*_sizeZ];

		EditorUtility.DisplayProgressBar( "Baking", "Texture Bake: 0%", 0.0f );
		int percentComplete = 0;
		int index = 0;
		for( int z = 0; z < _sizeZ; z++ )
		{
			for( int y = 0; y < _sizeY; y++ )
			{
				for( int x = 0; x < _sizeX; x++ )
				{
					float u = (float)x / (float)(_sizeX-1);
					float v = (float)y / (float)(_sizeY-1);
					float w = (float)z / (float)(_sizeZ-1);
					pixels[index] = SampleColor( u, v, w );
					index++;
				}

				int newPercentComplete = (int)( ( (float)index / (float)(_sizeX*_sizeY*_sizeZ) ) * 100.0f );
				if( percentComplete != newPercentComplete )
				{
					percentComplete = newPercentComplete;
					EditorUtility.DisplayProgressBar( "Baking", "Texture Bake: " + percentComplete + "%", (float)index / (float)(_sizeX*_sizeY*_sizeZ) );
				}
			}
		}
		EditorUtility.ClearProgressBar();
		
		_path = EditorUtility.SaveFilePanelInProject( "Save 3D Texture", "FogLookup", "asset", "Save 3D Texture", _path );
		if( _path != null && _path.Length != 0 )
		{
			Texture3D tex = AssetDatabase.LoadAssetAtPath( _path, typeof( Texture3D ) ) as Texture3D;
			if( tex == null )
			{
				tex = new Texture3D( _sizeX, _sizeY, _sizeZ, TextureFormat.RGBA32, false );
				tex.wrapMode = TextureWrapMode.Clamp;
				AssetDatabase.CreateAsset( tex, _path );
			}
			if( tex.width != _sizeX || tex.height != _sizeY || tex.depth != _sizeZ )
			{
				Texture3D newTex = new Texture3D( _sizeX, _sizeY, _sizeZ, TextureFormat.RGBA32, false );
				newTex.SetPixels( pixels );
				newTex.wrapMode = TextureWrapMode.Clamp;
				EditorUtility.CopySerialized( newTex, tex );
				DestroyImmediate( newTex );
			}
			tex.SetPixels( pixels );
			EditorUtility.SetDirty( tex );
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		else
		{
			_path = "Assets/";
		}
	}

	private Color SampleColor( float u, float v, float w )
	{
		float viewAngle = Mathf.Lerp( Mathf.PI, 0.0f, u );
		float viewHeight = Mathf.Lerp( 0.0f, 1.0f, v );
		
		Vector3 camPos = new Vector3( 0.0f, 0.0f, viewHeight );
		Vector3 farPos = Quaternion.AngleAxis( viewAngle * Mathf.Rad2Deg, Vector3.up ) * Vector3.forward;
		Vector3 lookDir = ( farPos - camPos ).normalized;

		float distFar = Vector3.Distance( camPos, farPos );
		distFar = Mathf.Lerp( distFar, 0.0f, w );

		float sampleLength = distFar / _NumSamples;
		Vector3 sampleRay = lookDir * sampleLength;
		Vector3 samplePoint = camPos + sampleRay * 0.5f;

		float accumDensity = 0.0f;
		for( int i = 0; i < (int)_NumSamples; i++ ) {
			float altitude = samplePoint.magnitude;
			float density = densityCurve.Evaluate( altitude );
			accumDensity += density * sampleLength;
			samplePoint += sampleRay;
		}
			
		if( accumDensity > 1.0f )
		{	//RGBS encoding
			Vector2 rg = EncodeFloatRG( 1.0f );
			Vector2 ba = EncodeFloatRG( 1.0f / accumDensity );
			return new Color( rg.x, rg.y, ba.x, ba.y );
		}
		else
		{
			Vector2 rg = EncodeFloatRG( accumDensity );
			Vector2 ba = EncodeFloatRG( 1.0f );
			return new Color( rg.x, rg.y, ba.x, ba.y );
		}
	}

	// Encoding/decoding [0..1) floats into 8 bit/channel RG. Note that 1.0 will not be encoded properly.
	Vector2 EncodeFloatRG( float v ) {
		v = Mathf.Clamp( v, 0.0f, 0.999f );
		Vector2 kEncodeMul = new Vector2( 1.0f, 255.0f );
		float kEncodeBit = 1.0f/255.0f;
		Vector2 enc = kEncodeMul * v;
		enc.x = frac( enc.x );
		enc.y = frac( enc.y );
		enc.x -= enc.y * kEncodeBit;
		return enc;
	}
	float DecodeFloatRG( Vector2 enc ) {
		Vector2 kDecodeDot = new Vector2( 1.0f, 1.0f/255.0f );
		return Vector2.Dot( enc, kDecodeDot );
	}

	private static float frac( float v ) {
		return ( v - Mathf.Floor( v ) );
	}
}

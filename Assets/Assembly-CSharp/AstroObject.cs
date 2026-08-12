using UnityEngine;

public class AstroObject : MonoBehaviour
{
	public enum Type
	{
		None = 0,
		Star = 1,
		Planet = 2,
		Moon = 3,
		Comet = 4,
		SpaceStation = 5,
		Satellite = 6
	}

	public enum Name
	{
		None = 0,
		CustomString = 1,
		Sun = 2,
		CaveTwin = 3,
		TowerTwin = 4,
		TimberHearth = 5,
		BrittleHollow = 6,
		GiantsDeep = 7,
		DarkBramble = 8,
		Comet = 9,
		WhiteHole = 10,
		WhiteHoleTarget = 11,
		QuantumMoon = 12,
		RingWorld = 13,
		ProbeCannon = 14,
		TimberMoon = 15,
		VolcanicMoon = 16,
		Eye = 17,
		HourglassTwins = 18,
		SunStation = 19,
		DreamWorld = 20,
		MapSatellite = 21
	}

	[SerializeField]
	private Type _type;

	[SerializeField]
	private Name _name;

	[SerializeField]
	private string _customName;

	[SerializeField]
	private AstroObject _primaryBody;

	[SerializeField]
	private AstroObject _moon;

	[SerializeField]
	private AstroObject _satellite;

	[SerializeField]
	private Sector _rootSector;

	[SerializeField]
	private GravityVolume _gravityVolume;

	[SerializeField]
	private SandLevelController _sandLevelController;

	private OWRigidbody _owRigidbody;

	private void Awake()
	{
		Locator.RegisterAstroObject(this);
		_owRigidbody = GetComponent<OWRigidbody>();
	}

	public OWRigidbody GetOWRigidbody()
	{
		return _owRigidbody;
	}

	public Type GetAstroObjectType()
	{
		return _type;
	}

	public Name GetAstroObjectName()
	{
		return _name;
	}

	public string GetCustomName()
	{
		return _customName;
	}

	public AstroObject GetPrimaryBody()
	{
		return _primaryBody;
	}

	public void SetPrimaryBody(AstroObject astroObject)
	{
		_primaryBody = astroObject;
	}

	public AstroObject GetMoon()
	{
		return _moon;
	}

	public AstroObject GetSatellite()
	{
		return _satellite;
	}

	public Sector GetRootSector()
	{
		return _rootSector;
	}

	public GravityVolume GetGravityVolume()
	{
		return _gravityVolume;
	}

	public SandLevelController GetSandLevelController()
	{
		return _sandLevelController;
	}

	public static Type GetType(Name name)
	{
		switch (name)
		{
		case Name.BrittleHollow:
			return Type.Planet;
		case Name.Comet:
			return Type.Comet;
		case Name.DarkBramble:
			return Type.Planet;
		case Name.GiantsDeep:
			return Type.Planet;
		case Name.CaveTwin:
			return Type.Planet;
		case Name.TowerTwin:
			return Type.Planet;
		case Name.ProbeCannon:
			return Type.Satellite;
		case Name.QuantumMoon:
			return Type.Moon;
		case Name.Sun:
			return Type.Star;
		case Name.TimberHearth:
			return Type.Planet;
		case Name.TimberMoon:
			return Type.Moon;
		case Name.VolcanicMoon:
			return Type.Moon;
		case Name.WhiteHole:
			return Type.None;
		case Name.WhiteHoleTarget:
			return Type.None;
		case Name.Eye:
			return Type.None;
		default:
			return Type.None;
		}
	}

	public static string AstroObjectNameToString(Name name)
	{
		switch (name)
		{
		case Name.BrittleHollow:
			return UITextLibrary.GetString(UITextType.LocationBH);
		case Name.Comet:
			return UITextLibrary.GetString(UITextType.LocationCo);
		case Name.DarkBramble:
			return UITextLibrary.GetString(UITextType.LocationDB);
		case Name.GiantsDeep:
			return UITextLibrary.GetString(UITextType.LocationGD);
		case Name.CaveTwin:
			return UITextLibrary.GetString(UITextType.LocationCT);
		case Name.TowerTwin:
			return UITextLibrary.GetString(UITextType.LocationTT);
		case Name.ProbeCannon:
			return UITextLibrary.GetString(UITextType.LocationOPC);
		case Name.QuantumMoon:
			return UITextLibrary.GetString(UITextType.LocationQM);
		case Name.Sun:
			return UITextLibrary.GetString(UITextType.LocationSun);
		case Name.TimberHearth:
			return UITextLibrary.GetString(UITextType.LocationTH);
		case Name.TimberMoon:
			return UITextLibrary.GetString(UITextType.LocationTHMoon);
		case Name.VolcanicMoon:
			return UITextLibrary.GetString(UITextType.LocationBHMoon);
		case Name.WhiteHole:
			return UITextLibrary.GetString(UITextType.LocationWH);
		case Name.WhiteHoleTarget:
			return "ERROR";
		case Name.Eye:
			return UITextLibrary.GetString(UITextType.LocationEye);
		case Name.SunStation:
			return UITextLibrary.GetString(UITextType.LocationSS);
		case Name.RingWorld:
			return UITextLibrary.GetString(UITextType.LocationIP);
		case Name.DreamWorld:
			return "ERROR";
		default:
			return string.Empty;
		}
	}

	public static Name StringIDToAstroObjectName(string str)
	{
		switch (str)
		{
		case "BRITTLE_HOLLOW":
			return Name.BrittleHollow;
		case "COMET":
			return Name.Comet;
		case "DARK_BRAMBLE":
			return Name.DarkBramble;
		case "GIANTS_DEEP":
			return Name.GiantsDeep;
		case "CAVE_TWIN":
			return Name.CaveTwin;
		case "TOWER_TWIN":
			return Name.TowerTwin;
		case "ORBITAL_PROBE_CANNON":
			return Name.ProbeCannon;
		case "QUANTUM_MOON":
			return Name.QuantumMoon;
		case "SUN":
			return Name.Sun;
		case "TIMBER_HEARTH":
			return Name.TimberHearth;
		case "TIMBER_MOON":
			return Name.TimberMoon;
		case "VOLCANIC_MOON":
			return Name.VolcanicMoon;
		case "WHITE_HOLE":
			return Name.WhiteHole;
		case "WHITE_HOLE_TARGET":
			return Name.WhiteHoleTarget;
		case "EYE_OF_THE_UNIVERSE":
			return Name.Eye;
		case "SUN_STATION":
			return Name.SunStation;
		case "INVISIBLE_PLANET":
		case "RINGWORLD":
			return Name.RingWorld;
		case "DREAMWORLD":
			return Name.DreamWorld;
		default:
			return Name.None;
		}
	}
}

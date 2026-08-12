using UnityEngine;
using UnityEngine.InputSystem;

public class OWInputProcessorInitializer : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		InputSystem.RegisterProcessor<OWAxisProcessor>();
		InputSystem.RegisterProcessor<OWDoubleAxisProcessor>();
		InputSystem.RegisterInteraction<OWInputInteraction>();
	}
}

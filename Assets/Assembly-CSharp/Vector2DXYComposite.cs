using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

[DisplayStringFormat("{horizontal}*{vertical}")]
public class Vector2DXYComposite : InputBindingComposite<Vector2>
{
	[InputControl(layout = "Axis")]
	public int horizontal;

	[InputControl(layout = "Axis")]
	public int vertical;

	public float scaleFactor = 1f;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		InputSystem.RegisterBindingComposite<Vector2DXYComposite>();
	}

	public override Vector2 ReadValue(ref InputBindingCompositeContext context)
	{
		float x = context.ReadValue<float>(horizontal);
		float y = context.ReadValue<float>(vertical);
		return new Vector2(x, y) * scaleFactor;
	}
}

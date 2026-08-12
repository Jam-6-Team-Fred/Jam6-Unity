using UnityEngine;
using UnityEngine.UI;

public abstract class CreditsEntry : MonoBehaviour
{
	public enum Style
	{
		Content = 0,
		Header = 1,
		Title = 2
	}

	[SerializeField]
	private Text[] _columns;

	[SerializeField]
	private float _topMargin;

	[SerializeField]
	private float _bottomMargin;

	private Graphic _topMarginObject;

	private Graphic _bottomMarginObject;

	public abstract void SetColumnSpacing(float spacing);

	public void SetContents(string[] columnTexts)
	{
		for (int i = 0; i < _columns.Length; i++)
		{
			_columns[i].text = columnTexts[i].Trim();
		}
	}
}

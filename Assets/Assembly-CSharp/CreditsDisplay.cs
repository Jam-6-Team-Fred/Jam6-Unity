using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsDisplay
{
	private enum State
	{
		FADE_IN = 0,
		HOLD = 1,
		FADE_OUT = 2,
		WAIT = 3,
		DONE = 4
	}

	private State m_state;

	private float m_stateTimer;

	private float m_fadeInTime;

	private float m_fadeOutTime;

	private float m_holdTime;

	private List<Text> m_text;

	private float m_screenWidth;

	private float m_screenHeight;

	private CreditsData.CreditsPage m_page;

	private Transform m_parent;

	private Dictionary<string, Text> m_textProto;

	private float m_ySpacing;

	private float m_textHeight;

	private float m_yOffset;

	private float m_alpha;

	private AnimationCurve m_fadeCurve;

	public CreditsDisplay(CreditsData.CreditsPage page, Transform parent, Dictionary<string, Text> textProto, float ySpacing, float fadeInTime, float fadeOutTime, AnimationCurve fadeCurve)
	{
		m_page = page;
		m_parent = parent;
		m_textProto = textProto;
		m_ySpacing = ySpacing;
		m_state = State.FADE_IN;
		m_stateTimer = 0f;
		m_textHeight = 0f;
		m_holdTime = page.m_displayTime;
		UpdateSize();
		m_fadeInTime = fadeInTime;
		m_fadeOutTime = fadeOutTime;
		m_yOffset = 0f;
		m_fadeCurve = fadeCurve;
		m_alpha = -1f;
	}

	public void UpdateSize()
	{
		Destroy();
		m_parent.transform.localPosition = Vector3.zero;
		m_screenWidth = Screen.width;
		m_screenHeight = Screen.height;
		m_text = new List<Text>();
		float num = 0f;
		for (int i = 0; i < m_page.m_lines.Count; i++)
		{
			Text text = null;
			if (m_textProto.ContainsKey(m_page.m_lines[i].m_style))
			{
				text = m_textProto[m_page.m_lines[i].m_style];
			}
			if (null == text)
			{
				Debug.LogError("Credits line \"" + m_page.m_lines[i].m_string + "\" has illegal style \"" + m_page.m_lines[i].m_style + "\"");
				text = m_textProto["Text"];
			}
			num += m_page.m_lines[i].m_yOffset;
			string[] array = m_page.m_lines[i].m_string.Split('#');
			int num2 = array.Length;
			float num3 = 1920f;
			for (int j = 0; j < num2; j++)
			{
				float num4 = num3 / (float)(num2 + 1);
				num4 *= (float)(j + 1);
				num4 -= 0.5f * num3;
				Text text2 = Object.Instantiate(text);
				text2.material = new Material(text.material);
				text2.transform.SetParent(m_parent, worldPositionStays: false);
				RectTransform rectTransform = text2.rectTransform;
				Vector2 anchoredPosition = new Vector2(num4, 0f - num - 15f);
				rectTransform.anchoredPosition = anchoredPosition;
				text2.text = array[j];
				text2.enabled = true;
				m_text.Add(text2);
				if (num2 - 1 == j)
				{
					num += text2.preferredHeight + m_ySpacing;
				}
			}
		}
		m_textHeight = num;
		m_holdTime = m_page.m_displayTime;
		if (m_page.m_scrollSpeed > 0f)
		{
			m_holdTime += m_textHeight / m_page.m_scrollSpeed;
		}
		switch (m_page.m_yAlign)
		{
		case CreditsData.CreditsPage.YAlign.TOP:
			num = -0.5f * m_screenHeight;
			break;
		case CreditsData.CreditsPage.YAlign.MIDDLE:
			num = -0.5f * num;
			if (m_page.m_scrollSpeed > 0f)
			{
				m_holdTime += 0.5f * (float)Screen.height / m_page.m_scrollSpeed;
			}
			break;
		case CreditsData.CreditsPage.YAlign.BOTTOM:
			num = 0.5f * m_screenHeight;
			if (m_page.m_scrollSpeed > 0f)
			{
				m_holdTime += (float)Screen.height / m_page.m_scrollSpeed;
			}
			break;
		}
		for (int k = 0; k < m_text.Count; k++)
		{
			RectTransform rectTransform2 = m_text[k].rectTransform;
			Vector2 anchoredPosition2 = rectTransform2.anchoredPosition;
			anchoredPosition2.y -= num;
			rectTransform2.anchoredPosition = anchoredPosition2;
		}
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		if (State.DONE != m_state && ((float)Screen.width != m_screenWidth || (float)Screen.height != m_screenHeight))
		{
			UpdateSize();
		}
		m_yOffset += m_page.m_scrollSpeed * deltaTime;
		m_parent.transform.localPosition = new Vector3(0f, m_yOffset, 0f);
		switch (m_state)
		{
		case State.FADE_IN:
			m_stateTimer += deltaTime;
			if (m_stateTimer >= m_fadeInTime)
			{
				SetAlpha(1f);
				m_stateTimer = 0f;
				m_state = State.HOLD;
			}
			else
			{
				SetAlpha(m_fadeCurve.Evaluate(m_stateTimer / m_fadeInTime));
			}
			break;
		case State.HOLD:
			SetAlpha(1f);
			m_stateTimer += deltaTime;
			if (m_stateTimer >= m_holdTime)
			{
				m_stateTimer = m_fadeOutTime;
				m_state = State.FADE_OUT;
			}
			break;
		case State.FADE_OUT:
			m_stateTimer -= deltaTime;
			if (m_stateTimer <= 0f)
			{
				SetAlpha(0f);
				m_stateTimer += m_fadeOutTime;
				if (m_page.m_waitTime > 0f)
				{
					m_stateTimer = m_page.m_waitTime;
					m_state = State.WAIT;
				}
				else
				{
					m_state = State.DONE;
				}
			}
			else
			{
				SetAlpha(m_fadeCurve.Evaluate(m_stateTimer / m_fadeOutTime));
			}
			break;
		case State.WAIT:
			m_stateTimer -= deltaTime;
			if (m_stateTimer <= 0f)
			{
				m_state = State.DONE;
			}
			break;
		case State.DONE:
			SetAlpha(0f);
			break;
		}
	}

	public void Destroy()
	{
		if (m_text != null)
		{
			for (int i = 0; i < m_text.Count; i++)
			{
				Object.Destroy(m_text[i].gameObject);
			}
			m_text = null;
		}
	}

	private void SetAlpha(float alpha)
	{
		if (alpha == m_alpha)
		{
			return;
		}
		foreach (Text item in m_text)
		{
			Color color = item.material.color;
			color.a = alpha;
			item.material.color = color;
			Outline component = item.GetComponent<Outline>();
			if (null != component)
			{
				color = component.effectColor;
				color.a = alpha;
				component.effectColor = color;
			}
			if (alpha > 0f)
			{
				item.gameObject.SetActive(value: true);
			}
			else
			{
				item.gameObject.SetActive(value: false);
			}
		}
		m_alpha = alpha;
	}

	public bool IsDone()
	{
		return m_state == State.DONE;
	}
}

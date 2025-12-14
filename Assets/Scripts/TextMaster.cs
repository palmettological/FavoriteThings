using TMPro;
using UnityEditor.Search;
using UnityEngine;

public class TextMaster : MonoBehaviour
{
    public float m_shadowTime = 0.5f;
    public float m_shadowSpacing = 20.0f;
    public float m_shuffleTime = 2.0f;
    public float m_shuffleInterval = 0.1f;

    private string m_targetName;

    private TextMeshProUGUI m_text;
    private TextMeshProUGUI m_shadowText;

    private float m_showStartTime = -1.0f;
    private float m_showEndTime = -1.0f;

    private float m_shuffleStartTime = -1.0f;
    private float m_shuffleEndTime = -1.0f;
    private float m_nextShuffle = -1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_text = GetComponent<TextMeshProUGUI>();
        m_shadowText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= m_shuffleStartTime && Time.time <= m_shuffleEndTime)
        {
            m_text.gameObject.SetActive(true);
            m_shadowText.gameObject.SetActive(false);

            if (Time.time > m_nextShuffle)
            {
                m_text.text = GameLogic.m_shuffleNames[UnityEngine.Random.Range(0, GameLogic.m_shuffleNames.Count)];
                m_nextShuffle = Time.time + m_shuffleInterval;
            }
        }

		if (Time.time >= m_showStartTime && Time.time <= m_showEndTime)
        {
			m_text.text = m_targetName;
			m_shadowText.text = m_targetName;

			float percent = (Time.time - m_showStartTime) / (m_showEndTime - m_showStartTime);
            float clamped = Mathf.Clamp01(percent);

            Debug.Log(percent);

            var color = m_shadowText.color;
            color.a = 1.0f - clamped;
            m_shadowText.color = color;

            m_shadowText.characterSpacing = Mathf.Lerp(0.0f, m_shadowSpacing, clamped);

            m_text.gameObject.SetActive(true);

            if (percent >= 1.0f)
            {
                m_shadowText.gameObject.SetActive(false);
            }
            else
            {
                m_shadowText.gameObject.SetActive(true);
            }
        }
    }

    public void Show(string name)
    {
        m_shuffleEndTime = m_shuffleStartTime = -1.0f;
        m_targetName = name;
        m_text.text = m_targetName;
        m_shadowText.text = m_targetName;

        m_showStartTime = Time.time;
        m_showEndTime = m_showStartTime + m_shadowTime;

        m_text.gameObject.SetActive(true);
        m_shadowText.gameObject.SetActive(true);
    }

	public void ShuffleShow(string name)
	{
		m_targetName = name;

		m_text.text = "";
		m_shadowText.text = "";

        m_shuffleStartTime = Time.time;
        m_shuffleEndTime = m_shuffleStartTime + m_shuffleTime;
		m_showStartTime = m_shuffleEndTime;
		m_showEndTime = m_showStartTime + m_shadowTime;

        m_nextShuffle = -1.0f;

		m_text.gameObject.SetActive(true);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}
}

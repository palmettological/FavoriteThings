using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Player
{
	public string m_name;
	public int m_giftCount = 0;
	public bool m_presented = false;


	public Player()
	{
	}

	public Player(Player other)
	{
		m_name = other.m_name;
		m_giftCount = other.m_giftCount;
		m_presented = other.m_presented;
	}
}

public class Round
{
	public int m_presenter = -1;
	public int[] m_receivers;
}

public class GameLogic : MonoBehaviour
{
    public int m_giftCount = 4;
	public int m_maxDifference = 2;
	public List<string> names;

	public TextMaster m_presenterName;
	public Transform m_receiverNames;

	public GameObject m_presenter;
	public GameObject m_recipients;
	public GameObject m_start;
	public GameObject m_merryChristmas;

	public int m_seed = -1;
	public bool m_debug = false;

	private List<Player> m_players = new List<Player>();
	private Round[] m_rounds;

	private int m_round = -1;

	private const float PanelWidth = 400f;
	private const float Padding = 10f;
	private const float LineHeight = 32f;

	private GUIStyle m_guiStyle;
	private GUIStyle m_guiStyleGreen;

	private int m_stageCount = 0;

	static public List<string> m_shuffleNames = new List<string>();

	private void Awake()
	{
		names ??= new List<string>();
	}

	private void Start()
	{
		if (m_seed >= 0)
		{
			Random.InitState(m_seed);
		}

		ComputeTheGame();
		ClearTheBoard();
	}

	void OnGUI()
	{
		if (!m_debug)
			return;
		if (m_guiStyle == null)
		{
			m_guiStyle = new GUIStyle(GUI.skin.label);
			m_guiStyle.fontSize = 18;              // ← change this
			m_guiStyle.alignment = TextAnchor.MiddleLeft;
			m_guiStyle.normal.textColor = Color.white;

			m_guiStyleGreen = new GUIStyle(GUI.skin.label);
			m_guiStyleGreen.fontSize = 18;              // ← change this
			m_guiStyleGreen.alignment = TextAnchor.MiddleLeft;
			m_guiStyleGreen.normal.textColor = Color.green;
		}

		float x = Screen.width - PanelWidth - Padding;
		float y = Padding;

		// Optional background box
		GUI.Box(
			new Rect(x - 5, y - 5, PanelWidth + 10, m_players.Count * LineHeight + 30),
			"Players"
		);

		y += 20f;

		for (int i = 0; i < m_players.Count; i++)
		{
			Player p = m_players[i];

			string line =
				$"{i}. {p.m_name}  |  Gifts: {p.m_giftCount}  |  Presented: {p.m_presented}";

			GUI.Label(
				new Rect(x, y, PanelWidth, LineHeight),
				line,
				m_players[i].m_presented ? m_guiStyleGreen : m_guiStyle
			);

			y += LineHeight;
		}
	}

	private void ClearTheBoard()
	{
		//m_presenterTitle.text = "";
		m_presenterName.gameObject.SetActive(false);

		for (int i = 0; i < m_receiverNames.childCount; ++i)
		{
			var child = m_receiverNames.GetChild(i);
			child.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		var keyboard = Keyboard.current;
		if (keyboard == null)
			return;

		if (keyboard.spaceKey.wasPressedThisFrame)
		{
			OnSpace();
		}

		if (keyboard.leftArrowKey.wasPressedThisFrame)
		{
			OnLeft();
		}

		if (keyboard.rightArrowKey.wasPressedThisFrame)
		{
			OnRight();
		}
	}

	private void OnSpace()
	{
		IncrementStageCount();
	}

	private void OnLeft()
	{
		DecrementStageCount();
	}
	private void OnRight()
	{
		IncrementStageCount();
	}

	private void IncrementStageCount()
	{
		if (m_round < 0)
		{
			m_round = 0;
			m_stageCount = 0;
		}
		else
		{
			m_stageCount++;
			if (m_stageCount > m_giftCount)
			{
				m_stageCount = 0;
				m_round++;
			}
		}

		ShowRound();
	}
	private void DecrementStageCount()
	{
		m_round--;
		m_stageCount = 0;

		ShowRound();
	}


	private void ComputeTheGame()
	{
		// build the players
		foreach (var s in names)
		{
			var player = new Player
			{
				m_name = s,
				m_giftCount = 0
			};

			m_players.Add(player);

			m_shuffleNames.Add(s);
		}

		var first = m_players[0];
		m_players.RemoveAt(0);

		for (int i = m_players.Count - 1; i > 0; i--)
		{
			int j = Random.Range(0, i + 1);
			(m_players[i], m_players[j]) = (m_players[j], m_players[i]);
		}
		m_players.Insert(0, first);


		// make the rounds
		m_rounds = new Round[m_players.Count];
		for (int i = 0; i < m_rounds.Length; i++)
		{
			var round = new Round();

			round.m_presenter = -1;

			round.m_receivers = new int[m_giftCount];
			for (int j = 0; j < m_giftCount; j++)
			{
				round.m_receivers[j] = -1;
			}

			m_rounds[i] = round;
		}

		// assign the presenters
		for (int i = 0; i < m_players.Count; i++)
		{
			m_rounds[i].m_presenter = i;
			m_players[i].m_presented = true;
			if (i > 0)
			{
				m_rounds[i - 1].m_receivers[0] = i;
				//m_players[i].m_giftCount++;
			}
		}

		// assign the rest of the receivers
		var shuffled = new List<int>();
		for (int i = 0; i < m_players.Count; ++i)
		{
			shuffled.Add(i);
		}

		// host needs to go in somewhere
		int where = UnityEngine.Random.Range(1, m_rounds.Length);
		m_rounds[where].m_receivers[1] = 0;

		// fill'er up
		for (int pass = 0; pass < 3; ++pass)
		{
			for (int i = shuffled.Count - 1; i > 0; i--)
			{
				int j = Random.Range(0, i + 1);
				(shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
			}

			for (int p = 0; p < shuffled.Count; ++p)
			{
				var index = shuffled[p];

				PlaceIndex(index);
			}
		}

		// stats
		while (ComputeGiftCounts() > 0)
		{
			Debug.Log("replacin");
			FixOneNull();
		}
	}

	private void FixOneNull()
	{
		int p = -1;
		for (int i = 0; i < m_players.Count; ++i)
		{
			if (m_players[i].m_giftCount != m_giftCount)
			{
				p = i;
				break;
			}
		}

		for (int i = 0; i < m_rounds.Length; ++i)
		{
			var roundA = m_rounds[i];
			for (int j = 0; j < m_giftCount; ++j)
			{
				int index = roundA.m_receivers[j];

				if (index < 0)
				{
					for (int i2 = 0; i2 < m_rounds.Length; ++i2)
					{
						var roundB = m_rounds[i2];
						if (roundB.m_presenter == p)
							continue;

						bool alreadyHere = false;
						for (int j2 = 1; j2 < m_giftCount; ++j2)
						{
							if (roundB.m_receivers[j2] == p)
							{
								alreadyHere = true;
								break;
							}
						}

						if (!alreadyHere)
						{
							for (int j2 = 1; j2 < m_giftCount; ++j2)
							{
								if (roundB.m_receivers[j2] != p)
								{
									m_rounds[i].m_receivers[j] = roundB.m_receivers[j2];
									roundB.m_receivers[j2] = p;
									return;
								}
							}
						}
					}
				}
			}
		}
	}

	private int ComputeGiftCounts()
	{
		for (int i = 0; i < m_players.Count; ++i)
			m_players[i].m_giftCount = 0;

		for (int i = 0; i < m_rounds.Length; ++i)
		{
			for (int j = 0; j < m_giftCount; ++j)
			{
				int index = m_rounds[i].m_receivers[j];

				if (index >= 0)
					m_players[index].m_giftCount++;
			}
		}

		int notDone = 0;

		for (int i = 0; i < m_players.Count; ++i)
		{
			if (m_players[i].m_giftCount != m_giftCount)
				++notDone;
		}

		return notDone;
	}

	private void PlaceIndex(int index)
	{
		bool found = false;
		for (int i = 0; i < m_rounds.Length && !found; ++i)
		{
			if (i == index)
				continue;

			var round = m_rounds[i];
			for (int j = 0; j < m_giftCount && !found; ++j)
			{
				if (round.m_receivers[j] == index)
					break;

				if (round.m_receivers[j] == -1)
				{
					round.m_receivers[j] = index;
					found = true;
				}
			}
		}
	}

	private void ShowRound()
	{
		if (m_round < 0)
		{
			m_presenter.SetActive(false);
			m_recipients.SetActive(false);
			m_start.SetActive(true);
			m_merryChristmas.SetActive(false);
			m_round = -1;
			m_stageCount = 0;
			return;
		}

		if (m_round >= m_rounds.Length)
		{
			m_presenter.SetActive(false);
			m_recipients.SetActive(false);
			m_start.SetActive(false);
			m_merryChristmas.SetActive(true);
			m_round = m_rounds.Length;
			m_stageCount = 0;
			return;
		}

		m_presenter.SetActive(true);
		m_recipients.SetActive(true);
		m_start.SetActive(false);
		m_merryChristmas.SetActive(false);

		var round = m_rounds[m_round];

		if (m_stageCount == 0)
		{
			string name;
			if (round.m_presenter >= 0)
				name = m_players[round.m_presenter].m_name;
			else
				name = "null";
			m_presenterName.Show(name);

			for (int i = 0; i < m_receiverNames.childCount && i < round.m_receivers.Length; ++i)
			{
				var text = m_receiverNames.GetChild(i).GetComponent<TextMaster>();
				text.Hide();
			}
		}
		else
		{
			for (int i = 0; i < m_receiverNames.childCount && i < round.m_receivers.Length; ++i)
			{
				if (i + 1 == m_stageCount)
				{
					var text = m_receiverNames.GetChild(i).GetComponent<TextMaster>();

					if (round.m_receivers[i] < 0)
					{
						text.Show("null");
					}
					else
					{
						text.ShuffleShow(m_players[round.m_receivers[i]].m_name);
					}
				}
			}
		}
	}
}


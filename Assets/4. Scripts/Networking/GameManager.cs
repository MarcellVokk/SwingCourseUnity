using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int maxMessages = 25;

    List<Message> messageList = new List<Message>();

    public GameObject chatPanel, textObject;
    public TMP_InputField chatUserInputField;
    public static TMP_InputField chatUserInputFieldStatic;

    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public static Dictionary<string, Item> items = new Dictionary<string, Item>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    public PlayerManager localPlayer;

    public GameObject rarity_uncommon;
    public GameObject rarity_common;
    public GameObject rarity_rare;
    public GameObject rarity_epic;
    public GameObject rarity_legendary;

    public static GameObject _rarity_uncommon;
    public static GameObject _rarity_common;
    public static GameObject _rarity_rare;
    public static GameObject _rarity_epic;
    public static GameObject _rarity_legendary;

    public Material mat_uncommon;
    public Material mat_common;
    public Material mat_rare;
    public Material mat_epic;
    public Material mat_legendary;

    public static Material _mat_uncommon;
    public static Material _mat_common;
    public static Material _mat_rare;
    public static Material _mat_epic;
    public static Material _mat_legendary;

    public void AddChatMessage(string text, Color color)
	{
        if(messageList.Count >= maxMessages)
		{
            Destroy(messageList[0].textObject.gameObject);
            messageList.Remove(messageList[0]);
		}

        Message newMessage = new Message();
        newMessage.text = text;
        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<TextMeshProUGUI>();
        newMessage.textObject.SetText(text);
        newMessage.textObject.color = color;

        messageList.Add(newMessage);
	}

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

	private void Start()
	{
        chatUserInputFieldStatic = chatUserInputField;

        _rarity_uncommon = rarity_uncommon;
        _rarity_common = rarity_common;
        _rarity_rare = rarity_rare;
        _rarity_epic = rarity_epic;
        _rarity_legendary = rarity_legendary;

        _mat_uncommon = mat_uncommon;
        _mat_common = mat_common;
        _mat_rare = mat_rare;
        _mat_epic = mat_epic;
        _mat_legendary = mat_legendary;
    }

	public static void ChatFocus(bool focus)
	{
		if (focus)
		{
            chatUserInputFieldStatic.ActivateInputField();
            EventSystem.current.SetSelectedGameObject(chatUserInputFieldStatic.gameObject, null);
        }
		else
		{
            chatUserInputFieldStatic.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(chatUserInputFieldStatic.gameObject, null);
        }
	}

	private async void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
            AddChatMessage(chatUserInputField.text, Color.white);

            if (chatUserInputField.text == "/zoomIn")
			{
                StartCoroutine(AnimationManagger.ViewZoomInNumerator());
			}
            else if (chatUserInputField.text == "/fbt1")
            {
                FirebaseManagger.SetStringData("ipCheckService", "https://ipinfo.io/ip");
            }
            else if (chatUserInputField.text == "/fbt2")
            {
                FirebaseManagger.GetListData("servers", result =>
                {
                    foreach(string s in result)
                    {
                        Debug.Log(s);
                    }
                });
            }
            else if (chatUserInputField.text == "/fbt3")
            {
                FirebaseManagger.RemoveFromList("Servers", "92.249.134.109");
            }
            else
			{
                ClientSend.ChatMessage(chatUserInputField.text);
            }

            chatUserInputField.text = "";
            chatUserInputField.ActivateInputField();
            EventSystem.current.SetSelectedGameObject(chatUserInputField.gameObject, null);
        }
	}

	/// <summary>Spawns a player.</summary>
	/// <param name="_id">The player's ID.</param>
	/// <param name="_name">The player's name.</param>
	/// <param name="_position">The player's starting position.</param>
	/// <param name="_rotation">The player's starting rotation.</param>
	public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation, bool isMounted, string attachedEntityUuid)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
            localPlayer = _player.GetComponent<PlayerManager>();
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
            _player.GetComponent<PlayerManager>().targetTransform = _position;
        }

        _player.GetComponent<PlayerManager>().isMounted = isMounted;

        if (attachedEntityUuid != "")
        {
            _player.transform.parent = EnvironmentManagger.existingEntities[attachedEntityUuid].transform;
        }

        if (isMounted)
        {
            if (EnvironmentManagger.existingEntities[attachedEntityUuid].GetComponent<Entity>().mountPosition != null)
            {
                _player.transform.localPosition = EnvironmentManagger.existingEntities[attachedEntityUuid].GetComponent<Entity>().mountPosition.localPosition;
                _player.transform.localRotation = EnvironmentManagger.existingEntities[attachedEntityUuid].GetComponent<Entity>().mountPosition.localRotation;
            }
            else
            {
                _player.transform.localPosition = Vector3.zero;
                _player.transform.localRotation = Quaternion.identity;
            }
        }

        _player.GetComponent<PlayerManager>().Initialize(_id, _username);
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
}

[System.Serializable]
public class Message
{
    public string text;
    public TextMeshProUGUI textObject;
}

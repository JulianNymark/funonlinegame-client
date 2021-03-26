using Godot;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class WebsocketManager : Godot.Node2D
{
	private string SERVER_URL = "ws://localhost:8765";
	private WebSocketClient client = null;

	private MessageHandler messageHandler = new MessageHandler();

	private string playerId;
	private Dictionary<string, Player> playerNodes = new Dictionary<string, Player>(); // TODO: move actual data out of the websocket handler (this is not SRP)
	private PackedScene playerScene = ResourceLoader.Load<PackedScene>("res://scenes/Player.tscn"); // TODO: move out (SRP)

	private void _closed()
	{
		Debug.WriteLine("closed connection");
		SetProcess(false);
	}

	private void _error()
	{
		Debug.WriteLine("there was an error:");
		this._closed();
	}

	private void _connected()
	{
		Debug.WriteLine("connected");
	}

	private void _on_data()
	{
		Debug.WriteLine("received data");
		var jsonDataBytes = client.GetPeer(1).GetPacket();
		string jsonString = System.Text.Encoding.UTF8.GetString(jsonDataBytes, 0, jsonDataBytes.Length);

		messageHandler.handle(jsonString, this); // TODO: move this out (SRP), pass it to some better named DataManager or sumth...
	}

	private void ConnectToServer()
	{
		this.client = new WebSocketClient();
		this.client.Connect("connection_closed", this, "_closed");
		this.client.Connect("connection_error", this, "_error");
		this.client.Connect("connection_established", this, "_connected");
		this.client.Connect("data_received", this, "_on_data");

		var error = client.ConnectToUrl(SERVER_URL);
		if (error != Error.Ok)
		{
			Debug.WriteLine($"error: {error.ToString()}");
			SetProcess(false);
		}
	}

	// TODO: move this out (SRP)
	public void CreatePlayer(PlayerData playerData)
	{
		var playerNode = playerScene.Instance() as Player;
		playerNode.Init(playerData);
		// playerNode.Call("Init", playerData); // WIP: parameters lost across godot "node-instance-attached-script-method-call"? it's (null)
		this.AddChild(playerNode); // synchronize this with playerStates! (be careful!)
		this.playerNodes.Add(playerData.uuid, playerNode);

		playerNode.Position = new Vector2();
	}

	// TODO: move this out (SRP)
	public void DestroyPlayer(string playerUuid)
	{
		var playerNode = this.playerNodes[playerUuid];
		this.RemoveChild(playerNode);
		this.playerNodes.Remove(playerUuid);
	}

	// TODO: move this out (SRP)
	public void MovePlayer(PlayerMove playerMove)
	{
		var playerNode = this.playerNodes[playerMove.uuid];
		var vec = new Vector2(playerMove.pos.x, playerMove.pos.y);

		var playerData = playerNode.Get("playerData") as PlayerData;

		playerData.pos.x += vec.x;
		playerData.pos.y += vec.y;

		// this.playerStates.Add(playerMove.uuid, playerData);
	}

	// TODO: move this out (SRP)
	public void AssociateId(PlayerId playerId)
	{
		this.playerId = playerId.uuid;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Debug.WriteLine("hello from csharp!");
		this.ConnectToServer();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (playerId != null)
		{
			var horizontal = Input.GetActionStrength("right") - Input.GetActionStrength("left");
			var vertical = Input.GetActionStrength("down") - Input.GetActionStrength("up");

			var playerMove = new PlayerMove();
			playerMove.pos = new Position();
			playerMove.pos.x = horizontal;
			playerMove.pos.y = vertical;
			playerMove.uuid = playerId;

			sendMessage("req_move_player", playerMove);

		}
		client.Poll(); // actually makes client calls (buffered)
	}

	public void sendMessage(string messageType, object messageData)
	{
		var messageWrapper = new MessageWrapper();
		messageWrapper.type = messageType;
		messageWrapper.data = messageData;
		var messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(messageWrapper);
		client.GetPeer(1).PutPacket(messageJson.ToUTF8());
	}
}



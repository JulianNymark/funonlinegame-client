using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Polygon2D : Godot.Polygon2D
{
	private string SERVER_URL = "ws://localhost:8765";
	private WebSocketClient client = null;

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
		client.GetPeer(1).PutPacket("test packet".ToUTF8());
	}

	private void _on_data()
	{
		Debug.WriteLine("received data");
		var jsonDataBytes = client.GetPeer(1).GetPacket();
		string jsonString = System.Text.Encoding.UTF8.GetString(jsonDataBytes, 0, jsonDataBytes.Length);
		PlayerData playerData = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerData>(jsonString);
		this.Position = new Vector2(playerData.pos.x, playerData.pos.y);
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Debug.WriteLine("hello from csharp!");
		this.ConnectToServer();

		this.Position += new Vector2(0, 0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		client.Poll();
		this.Position += new Vector2(1, 1);
		// fetch new position from some stream! (websocket, Enet...)
		// assign position to the position of this Node
	}
}



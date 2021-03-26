using System.Diagnostics;
using Newtonsoft.Json.Linq;

// handle messages of the shape:
// {
//   "type": string,
//   "data": ...
// }
// the shape of "data" varies based on the type, but knowing "type" we can serialize
// the data to a known type.

public class MessageHandler
{
    public void handle(string jsonString, WebsocketManager ws)
    {
        JToken token = JToken.Parse(jsonString);
        var messageType = token["type"];

        Debug.WriteLine($"received {messageType.ToString()} message");
        switch (messageType.ToString())
        {
            case "associate_id":
                {
                    var playerId = token["data"].ToObject<PlayerId>();
                    ws.AssociateId(playerId);
                    return;
                }
            case "create_player":
                {
                    var playerData = token["data"].ToObject<PlayerData>();
                    ws.CreatePlayer(playerData);
                    return;
                }
            case "move_player":
                {
                    var playerMove = token["data"].ToObject<PlayerMove>();
                    ws.MovePlayer(playerMove);
                    return;
                }
        }
    }
}
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

        switch (messageType.ToString())
        {
            case "create_player":
                Debug.WriteLine("received 'create_player' message");
                var playerData = token["data"].ToObject<PlayerData>();
                ws.CreatePlayer(playerData);
                return;
            case "move_player":
                Debug.WriteLine("received 'move_player' message");
                return;
        }

        // MessageData messageData = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageData>(jsonString);
        // JsonConvert

        // PlayerData
    }
}
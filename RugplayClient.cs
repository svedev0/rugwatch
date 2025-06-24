using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace rugwatch;

internal class RugplayClient(string wsUrl)
{
	public readonly Queue<Trade> TradesQueue = new();

	private readonly WebSocket _ws = new(wsUrl);
	private bool _shouldReconnect = false;

	public void Connect()
	{
		_shouldReconnect = true;
		_ws.OnMessage += HandleWsReceived;
		_ws.OnClose += Reconnect;
		_ws.Connect();

		if (_ws.ReadyState != WebSocketState.Open)
		{
			throw new Exception("Failed to connect to WebSocket.");
		}

		Subscribe();
	}

	public void Disconnect()
	{
		_shouldReconnect = false;
		_ws.OnMessage -= HandleWsReceived;

		if (_ws.ReadyState is not WebSocketState.Closing or WebSocketState.Closed)
		{
			_ws.Close();
		}
	}

	private void Reconnect(object? sender, CloseEventArgs e)
	{
		Logger.LogWarning("WebSocket connection closed.");

		if (!_shouldReconnect ||
			_ws.ReadyState is WebSocketState.Connecting or WebSocketState.Open)
		{
			return;
		}

		Logger.LogInfo("Reconnecting...");
		_ws.Connect();

		if (_ws.ReadyState != WebSocketState.Open)
		{
			Task.Delay(1000).Wait();
		}
	}

	private void Subscribe()
	{
		_ws.Send("{\"type\":\"subscribe\",\"channel\":\"trades:all\"}");
		_ws.Send("{\"type\":\"set_coin\",\"coinSymbol\":\"@global\"}");
	}

	private void SendPong()
	{
		if (_ws.ReadyState == WebSocketState.Open)
		{
			_ws.Send("{\"type\":\"pong\"}");
		}
	}

	private void HandleWsReceived(object? sender, MessageEventArgs e)
	{
		if (!e.IsText)
		{
			return;
		}

		if (!TryParseJson(e.Data, out JObject? jsonObj) || jsonObj is null)
		{
			return;
		}

		string messageType = $"{jsonObj["type"]}";
		switch (messageType)
		{
			case "ping":
				SendPong();
				break;

			case "all-trades":
				if (!TryGetTradeData(e.Data, out RequestData? data) || data is null)
				{
					break;
				}

				TradesQueue.Enqueue(new Trade()
				{
					Timestamp = DateTimeFromUnix(data.Timestamp),
					TradeType = data.Type,
					CoinSymbol = data.CoinSymbol,
					CoinName = data.CoinName,
					CoinAmount = data.Amount,
					CoinPrice = data.Price,
					TradeValue = data.TotalValue,
					Username = data.Username,
					UserId = int.TryParse(data.UserId, out int uid) ? uid : 0
				});
				break;

			default:
				break;
		}
	}

	private static bool TryParseJson(string json, out JObject? jsonObj)
	{
		try
		{
			jsonObj = JObject.Parse(json);
			if (jsonObj is not null)
			{
				return true;
			}

			jsonObj = null;
			return false;
		}
		catch
		{
			jsonObj = null;
			return false;
		}
	}

	private static bool TryGetTradeData(string json, out RequestData? data)
	{
		try
		{
			RequestRoot? root = JsonConvert.DeserializeObject<RequestRoot>(json);
			if (root is not null && root.Data is not null)
			{
				data = root.Data;
				return true;
			}

			data = null;
			return false;
		}
		catch
		{
			data = null;
			return false;
		}
	}

	private static DateTimeOffset DateTimeFromUnix(long unixTimestamp)
	{
		return DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp);
	}
}

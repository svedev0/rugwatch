using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace rugwatch;

[PrimaryKey("Id")]
internal class Trade
{
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; init; }
	public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.MinValue;
	public string TradeType { get; init; } = string.Empty;
	public string CoinSymbol { get; init; } = string.Empty;
	public string CoinName { get; init; } = string.Empty;
	public double CoinAmount { get; init; } = 0.0d;
	public double CoinPrice { get; init; } = 0.0d;
	public double TradeValue { get; init; } = 0.0d;
	public string Username { get; init; } = string.Empty;
	public int UserId { get; init; } = 0;
}

// JSON response root
internal class RequestRoot
{
	[JsonProperty("type")]
	public string Type { get; init; } = string.Empty;

	[JsonProperty("data")]
	public RequestData Data { get; init; } = new RequestData();
}

// JSON response data
internal class RequestData
{
	[JsonProperty("type")]
	public string Type { get; init; } = string.Empty;

	[JsonProperty("username")]
	public string Username { get; init; } = string.Empty;

	[JsonProperty("userImage")]
	public string UserImage { get; init; } = string.Empty;

	[JsonProperty("amount")]
	public double Amount { get; init; } = 0.0D;

	[JsonProperty("coinSymbol")]
	public string CoinSymbol { get; init; } = string.Empty;

	[JsonProperty("coinName")]
	public string CoinName { get; init; } = string.Empty;

	[JsonProperty("coinIcon")]
	public string CoinIcon { get; init; } = string.Empty;

	[JsonProperty("totalValue")]
	public double TotalValue { get; init; } = 0.0D;

	[JsonProperty("price")]
	public double Price { get; init; } = 0.0D;

	[JsonProperty("timestamp")]
	public long Timestamp { get; init; } = 0L;

	[JsonProperty("userId")]
	public string UserId { get; init; } = string.Empty;
}

using System.Collections.Generic;
using PPTcliente.Domain.Models;

namespace PPTClient.Domain.Models;

public class PlayerState
{
    public int Score { get; set; }

    public List<Card> Deck { get; set; } = new();

    public List<Card> Hand { get; set; } = new();
}
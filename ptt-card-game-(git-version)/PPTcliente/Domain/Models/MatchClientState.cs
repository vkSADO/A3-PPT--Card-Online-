using PPTClient.Domain.Models;

namespace PPTClient.Application.State;

public class MatchClientState
{
    public PlayerState LocalPlayer { get; set; }
    public PlayerState Opponent { get; set; }

    public int RoundNumber { get; set; }

    public bool RoundResolved { get; set; }
}
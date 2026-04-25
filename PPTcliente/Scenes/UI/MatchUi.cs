using Godot;
using System;
using System.Text.Json;
using PPTservidor.Domain;
using PPTservidor.Domain.Enums;
using PPTservidor.Domain.Models;
using PPTservidor.Application.Services;
using System.Diagnostics.CodeAnalysis;

public partial class MatchUi : Control
{
    private MatchLogic _logic = new MatchLogic();

    private NetworkManager _networkManager;
    private string _matchId;
    private string _myPlayerId;
    
    // Referencia da UI

    [Export] private Label _playerName1;
    [Export] private Label _playerName2;
    [Export] private Label _scorePlayer1;
    [Export] private Label _scorePlayer2;
    [Export] private Label _quatityCardDeck;
    [Export] private Label _labelLog;
    [Export] private TextureButton _btnPedra, _btnPapel, _btnTesoura;
    [Export] private Button _btnAccuser, _btnAnnuncement, _btnFinilized;
    [Export] private Control _panelCardAccused;

    private string _cardSelected;



    public override void _Ready()
    {
        _networkManager = GetNode<NetworkManager>("/root/NetworkManager");

        // CONECTANDO O SINAL DA REDE
        _networkManager.MatchStateUpdated += OnMatchStateUpdated;

        // Conectando butões de ações
        _btnPedra.Pressed += () => SendPlay(CardType.Rock);
        _btnPapel.Pressed += () => SendPlay(CardType.Paper);
        _btnTesoura.Pressed += () => SendPlay(CardType.Scissors);
        _btnAccuser.Pressed += () => SendAccusation(true);
        _btnAnnuncement.Pressed += () => SendAnnuncement();


        // Seleção da carta anunciada
        _btnPapel.Pressed += OnPaperAnnuncied;
        _btnPedra.Pressed += OnRockAnnuncied;
        _btnTesoura.Pressed += OnScissorsAnnuncied;


       
        




    }

    private void SendAnnuncement()
    {
        _panelCardAccused.Visible = true;
    }


    public void Setup(string marchId, string playerId)
    {
        _matchId = marchId;
        _myPlayerId = playerId;

    }


    private void UpdateUI(MatchState match)
    {
        var p1 = match.GetPlayer(_myPlayerId);
        var p2 = match.GetPlayer(_matchId);

        _scorePlayer1.Text = p1.Score.ToString();
        _scorePlayer2.Text = p2.Score.ToString();

        // Gerenciar fase
        switch (match.CurrentPhase)
        {
            case MatchPhase.AnnouncementPhase:
                if(!p1.SelectedCard.HasValue)
                {
                    _labelLog.Text = p1.SelectedCard.HasValue.ToString();
                }
                else
                {
                    _labelLog.Text = p1.SelectedCard.Value.ToString();
                }
                break;
            
            case MatchPhase.GameOver:
                _labelLog.Text = p1.Score >= 5 ? "VOCÊ VENCEU!" : "VOCÊ PERDEU!";
                break;

        }
        
        
    }

    private void SendPlay(CardType realCard)
    {
        CardType card = new CardType();

        if(_cardSelected == "Pedra")
            card = CardType.Rock;
        else if(_cardSelected == "Papel")
            card = CardType.Paper;
        else if(_cardSelected == "Tesoura")
            card = CardType.Scissors;
    
        CardType announced = (CardType)card;
        realCard = card;
        _networkManager.SubmitPlay(_matchId, _myPlayerId, realCard, announced);
    }
    private void SendAccusation(bool accuse)
    {
        _networkManager.SubmitAccusation(_matchId, _myPlayerId, accuse);
        _btnAccuser.Visible = false;
        _labelLog.Text = "Aguardando revelação...";
    }

    private void OnMatchStateUpdated(string json)
    {
        var match = JsonSerializer.Deserialize<MatchState>(json);
        UpdateUI(match);
    }


    private void OnScissorsAnnuncied()
    {
        _labelLog.Text = $"Tesoura, {(int)CardType.Scissors}";
        _panelCardAccused.Visible = false;
    }


    private void OnRockAnnuncied()
    {
        _labelLog.Text = $"Pedra, {(int)CardType.Rock}";
        _panelCardAccused.Visible = false;
    }


    private void OnPaperAnnuncied()
    {
        _labelLog.Text = $"Papel, {(int)CardType.Paper}";
        _panelCardAccused.Visible = false;
    }
   

}

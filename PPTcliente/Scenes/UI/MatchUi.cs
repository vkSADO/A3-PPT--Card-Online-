using Godot;
using System;
using System.Text.Json;
using PPTservidor.Domain;
using PPTservidor.Domain.Enums;
using PPTservidor.Domain.Models;
using PPTservidor.Application.Services;
using System.Collections.Generic;

public partial class MatchUi : Control
{
    [Export] public Godot.Collections.Array<Texture2D> CardTextures { get; set; } = new();

    private NetworkManager _networkManager;
    private string _matchId;
    private string _myPlayerId;
    
    // Referencias da UI
    [Export] private Label _playerName1;
    [Export] private Label _playerName2;
    [Export] private Label _scorePlayer1;
    [Export] private Label _scorePlayer2;
    [Export] private Label _quatityCardDeck;
    [Export] private Label _labelLog;
    
    // Supondo que estes botões agora fiquem DENTRO do _panelCardAccused
    [Export] private TextureButton _btnPedra, _btnPapel, _btnTesoura; 
    [Export] private Button _btnAccuser, _btnAnnuncement, _btnFinilized;
    [Export] private Control _panelCardAccused;

    [Export] private TextureRect _CardSelectedTextureArea;
    [Export] private TextureRect _OpponentSelectedTextureArea;

    public override void _Ready()
    {
        _networkManager = GetNode<NetworkManager>("/root/NetworkManager");

        // CONECTANDO O SINAL DA REDE
        _networkManager.MatchStateUpdated += OnMatchStateUpdated;

        // Conectando os botões de Anúncio (O que o jogador vai gritar que tem)
        _btnPedra.Pressed += () => SendAnnouncement(CardType.Rock);
        _btnPapel.Pressed += () => SendAnnouncement(CardType.Paper);
        _btnTesoura.Pressed += () => SendAnnouncement(CardType.Scissors);

        // Botão de Acusação
        _btnAccuser.Visible = true;
        _btnAccuser.Pressed += () => SendAccusation(true);
        
        // Esconde os painéis no início
        _panelCardAccused.Visible = false;

        _btnAnnuncement.Pressed += () => OpenPanelSelectedCard();
    
    }

    private void OpenPanelSelectedCard()
    {
        _panelCardAccused.Visible = true;

    }


    public void Setup(string matchId, string playerId, string initialMatchJson)
    {
        _matchId = matchId;
        _myPlayerId = playerId;

        // Força a atualização da interface no momento em que a cena é instanciada
        var match = JsonSerializer.Deserialize<MatchState>(initialMatchJson);
        UpdateUI(match);
    }

    private void UpdateUI(MatchState match)
    {
        var me = match.GetPlayer(_myPlayerId);
        var opponent = match.GetOpponent(_myPlayerId);

        // Atualiza Pontuações
        _scorePlayer1.Text = me.Score.ToString();
        _playerName1.Text = me.Id;
        _quatityCardDeck.Text = me.Deck.Count.ToString();


        if (opponent != null)
        {
            _scorePlayer2.Text = opponent.Score.ToString();
            _playerName2.Text = opponent.Id;
            _quatityCardDeck.Text = opponent.Deck.Count.ToString();
            _OpponentSelectedTextureArea.Texture = CardTextures[3];

        }

        // Gerencia as Fases e a Interface
        switch (match.CurrentPhase)
        {
            case MatchPhase.AnnouncementPhase:
                _btnAccuser.Visible = false; // Garante que o botão de acusar está escondido

                if (me.SelectedCard.HasValue)
                {
                    if (!me.AnnouncedCard.HasValue)
                    {
                        // O servidor deu a carta, libera o painel para ele escolher o blefe
                        _labelLog.Text = $"O servidor sacou: {me.SelectedCard.Value}. O que vai anunciar?";
                        _panelCardAccused.Visible = true; 
                        if(me.SelectedCard.Value == CardType.Rock)
                        {
                            _CardSelectedTextureArea.Texture = CardTextures[0];
                        }
                        else if(me.SelectedCard.Value == CardType.Paper)
                        {
                            _CardSelectedTextureArea.Texture = CardTextures[1];
                        }
                        else
                        {
                            _CardSelectedTextureArea.Texture = CardTextures[2];
                        }
                    }
                    else
                    {
                        // Já escolheu o blefe, aguarda o outro
                        _labelLog.Text = $"Você anunciou {me.AnnouncedCard.Value}. Aguardando oponente...";
                        _panelCardAccused.Visible = false;
                    }
                }
                break;
            
            case MatchPhase.AccusationPhase:
                _panelCardAccused.Visible = false;
                
                // Se o oponente já anunciou a carta dele, mostramos e liberamos o botão de acusar
                if (opponent != null && opponent.AnnouncedCard.HasValue)
                {
                    _labelLog.Text = $"Oponente anunciou {opponent.AnnouncedCard.Value}. É blefe?";
                    _btnAccuser.Visible = true;
                }
                break;

            case MatchPhase.GameOver:
                _panelCardAccused.Visible = false;
                _btnAccuser.Visible = false;
                _labelLog.Text = me.Score >= 5 ? "VOCÊ VENCEU!" : "VOCÊ PERDEU!";
                break;
        }
    }

    // Método único e limpo para enviar a jogada
    private void SendAnnouncement(CardType announcedCard)
    {
        _panelCardAccused.Visible = false;
        _networkManager.SubmitPlay(_matchId, _myPlayerId, announcedCard);
        _labelLog.Text = "Carta anunciada foi : " + announcedCard.ToString();
        GD.Print("Carta anunciada foi : " + announcedCard.ToString() + "Player: " + _myPlayerId);
    }

    private void SendAccusation(bool accuse)
    {
        _networkManager.SubmitAccusation(_matchId, _myPlayerId, accuse);
        _btnAccuser.Visible = false;
        _labelLog.Text = "Voce acuou o jogar de esta blefando";

    }

    private void OnMatchStateUpdated(string json)
    {
        var match = JsonSerializer.Deserialize<MatchState>(json);
        UpdateUI(match);
    }
}
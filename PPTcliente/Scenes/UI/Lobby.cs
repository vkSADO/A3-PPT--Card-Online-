using Godot;
using PPTservidor.Domain.Models;
using System;
using System.Text.Json;

public partial class Lobby : Control
{
    private NetworkManager _networkManager;
    private Label _statusLabel;
    private Button _findMatchButton;
    private string _myPlayerId; // Adicione esta linha

    public override void _Ready()
    {
        // 1. Pega a referência do Autoload de rede
        _networkManager = GetNode<NetworkManager>("/root/NetworkManager");
        
        // 2. Pega as referências dos nós da UI
        _statusLabel = GetNode<Label>("Container/VBoxContainer/ServerStatusMessage");
        _findMatchButton = GetNode<Button>("Container/VBoxContainer/FindMatchButton");

        // 3. Conecta o clique do botão
        _findMatchButton.Pressed += OnFindMatchPressed;

        // 4. Conecta os sinais que vêm do servidor (via NetworkManager)
        _networkManager.WaitingForOpponent += OnWaitingForOpponent;
        _networkManager.MatchStarted += OnMatchStarted;
    }

    private void OnFindMatchPressed()
    {
        // Como ainda não temos o login do Google pronto, 
        // vamos gerar um ID falso (Guid) para simular um jogador único neste teste.
        string myFakeId = Guid.NewGuid().ToString(); 
        _myPlayerId = myFakeId; // Armazena o ID gerado

        _statusLabel.Text = "Conectando ao servidor...";
        _findMatchButton.Disabled = true; // Impede que o jogador clique várias vezes
        
        _networkManager.FindMatch(myFakeId);
    }

    private void OnWaitingForOpponent()
    {
        _statusLabel.Text = "Aguardando oponente entrar na fila...";
    }

    private void OnMatchStarted(string matchStateJson)
    {
        _statusLabel.Text = "Partida Encontrada! O jogo vai começar.";
        
        // Imprime o JSON no console do Godot para vermos os dados que o servidor mandou
        GD.Print("Estado da partida recebido: " + matchStateJson);
        
        // TODO: Futuramente, aqui faremos a troca de cena para a "Mesa" do jogo

        var match = JsonSerializer.Deserialize<MatchState>(matchStateJson);
    
        // Instancia a cena de jogo
        var gameScene = GD.Load<PackedScene>("res://PPTcliente/Scenes/Game/game.tscn").Instantiate();
        
        // Obtém o MatchUI da cena instanciada
        var matchUi = gameScene.GetNode<MatchUi>("UI/MatchUI");
        
        // Passa os IDs necessários (o ID do player deve ser o mesmo usado no FindMatch)
        matchUi.Setup(match.MatchId, _myPlayerId, matchStateJson); // Use a variável de instância

        // Troca a cena
        GetTree().Root.AddChild(gameScene);
        GetTree().CurrentScene = gameScene;
        this.QueueFree(); // Remove o Lobby
    }
    
    public override void _ExitTree()
    {
        // Boa prática: desconectar sinais ao destruir a tela
        if (_networkManager != null)
        {
            _networkManager.WaitingForOpponent -= OnWaitingForOpponent;
            _networkManager.MatchStarted -= OnMatchStarted;
        }
    }
}

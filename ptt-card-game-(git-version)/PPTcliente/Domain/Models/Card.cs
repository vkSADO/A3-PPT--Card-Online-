using System;
using PPTcliente.Domain.Enums;
namespace PPTcliente.Domain.Models;
public class Card
{
    private CardaType Type{ get;}

    public Card(CardaType type)
    {
        Type = type;
    }

    
}
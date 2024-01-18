using Application.Admins.Interfaces;
using Domain.Entities;

namespace Bot.Services;

public class ApplicationService
{
    
    public static async Task SendApplication()
    {

    }

    private static string GetHasPetsStringValue(Client client)
    {
        if (client.HasPets == true) return "Да";
        else return "Нет";
    }
}

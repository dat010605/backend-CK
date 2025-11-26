using MiniOrderAPI.Models; // Để nhận diện class User

namespace MiniOrderAPI.Services;

public interface ITokenService
{
    string CreateToken(User user);
}
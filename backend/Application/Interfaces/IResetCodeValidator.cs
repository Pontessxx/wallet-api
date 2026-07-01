namespace Application.Interfaces;

public interface IResetCodeValidator
{
    bool IsValid(string? resetCode);
}
namespace Application.Services;

public class ResetCodeValidator : IResetCodeValidator
{
    private const int ResetCodeLength = 6;

    public bool IsValid(string? resetCode)
        => !string.IsNullOrWhiteSpace(resetCode)
           && resetCode.Length == ResetCodeLength
           && resetCode.All(char.IsDigit);
}
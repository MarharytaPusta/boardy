namespace Boardy.Application.Authentication.Admin;

public class LogoutAdminResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static LogoutAdminResult Success() => new() { IsSuccess = true };
    public static LogoutAdminResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
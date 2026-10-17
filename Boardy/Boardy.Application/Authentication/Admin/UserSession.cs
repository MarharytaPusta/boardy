
namespace Boardy.Application.Authentication.Admin
{
    public class UserSession : IUserSession
    {
        public bool IsAdminMode { get; private set; } = false;

        public event Action? OnAdminStateChanged;

        public void SetAdminMode(bool isAdmin)
        {
            if(IsAdminMode != isAdmin)
            {
                IsAdminMode = isAdmin;
                OnAdminStateChanged?.Invoke();
            }
        }
    }
}

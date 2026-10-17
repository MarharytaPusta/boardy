using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.Authentication.Admin;

public interface IUserSession
{
    bool IsAdminMode { get; }
    event Action? OnAdminStateChanged;
    void SetAdminMode(bool isAdmin);
}

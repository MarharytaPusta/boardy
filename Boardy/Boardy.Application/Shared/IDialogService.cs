using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.Shared;

public interface IDialogService
{
    bool Confirm(string title, string message);
}

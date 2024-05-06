using Octokit.Bot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Octokit.Bot
{
    public interface IHookHandler
    {
        Task Handle(EventContext eventContext);
    }
}
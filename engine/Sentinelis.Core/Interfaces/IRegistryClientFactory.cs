using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinelis.Core.Interfaces
{
    public interface IRegistryClientFactory
    {
        IRegistryClient? GetClient(string ecosystem);
    }
}
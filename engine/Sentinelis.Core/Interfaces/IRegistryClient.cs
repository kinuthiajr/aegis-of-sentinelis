using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// The package for fetching package metadata from the NPM,NuGet,Cargo. 
// This is used to check for security vulnerabilities in dependencies.

namespace Sentinelis.Core.Interfaces
{
    public interface IRegistryClient
    {
        Task<DateTime?> GetVersionPublishDateAsync(string packageName, string version);
    }
}
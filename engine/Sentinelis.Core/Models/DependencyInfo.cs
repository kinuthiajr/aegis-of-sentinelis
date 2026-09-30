using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinelis.Core.Models
{

    // The universal passport for all languages with its package 

    public record DependencyInfo
    (
        string Ecosystem,
        string Name,
        string Version,
        string? ResolvedUrl = null,
        bool HasInstallScript = false
    );
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentinelis.Core.Models;

namespace Sentinelis.Core.Interfaces
{
    public interface IDependencyParser
    {
        // The parser checks the TargetPath (e.g., looks for package-lock.json or Cargo.lock).
        // If it finds its file, it returns the parsed dependencies. If not, it returns empty.
        IEnumerable<DependencyInfo> Parse(string targetPath);
    }
}
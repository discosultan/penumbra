using System.Diagnostics;
using NUnit.Framework;
using Penumbra.Utilities;

namespace Penumbra.Tests
{
    [SetUpFixture]
    public class Setup
    {
        [SetUp]
        public void Initialize()
        {
            Logger.Add(new DelegateLogger(msg => Debug.WriteLine(msg)));
        }
    }
}

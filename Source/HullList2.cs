using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Penumbra
{
    internal class HullList2
    {
        private readonly ObservableCollection<Hull> _rawHulls;

        public HullList2(ObservableCollection<Hull> rawHulls)
        {
            _rawHulls = rawHulls;
        }

        public List<Hull> ResolvedHulls { get; } = new List<Hull>();

        public void Resolve()
        {
            ResolvedHulls.Clear();
            for (int i = 0; i < _rawHulls.Count; i++)
            {
                Hull rawHull = _rawHulls[i];



                ResolvedHulls.Add(rawHull);
            }
        }
    }
}

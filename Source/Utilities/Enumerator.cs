using System;
using System.Collections;
using System.Collections.Generic;

namespace Penumbra.Utilities
{    
    // Ref: ILSpy System.Collections.Generic List<T>.Enumerator
    public struct Enumerator<T> : IEnumerator<T>
    {
        private readonly List<T> _list;
        private int _index;        

        /// <summary>Gets the element at the current position of the enumerator.</summary>
        /// <returns>The element in the <see cref="T:System.Collections.Generic.List`1" /> at the current position of the enumerator.</returns>        
        public T Current { get; private set; }

        /// <summary>Gets the element at the current position of the enumerator.</summary>
        /// <returns>The element in the <see cref="T:System.Collections.Generic.List`1" /> at the current position of the enumerator.</returns>
        /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>        
        object IEnumerator.Current
        {            
            get
            {
                if (_index == 0 || _index == _list.Count + 1)
                {
                    throw new InvalidOperationException();                    
                }
                return Current;
            }
        }

        internal Enumerator(List<T> list)
        {
            _list = list;
            _index = 0;            
            Current = default(T);
        }

        /// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.List`1.Enumerator" />.</summary>        
        public void Dispose()
        {
        }

        /// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
        /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>        
        public bool MoveNext()
        {
            List<T> list = _list;
            if (_index < list.Count)
            {
                Current = list[_index];
                _index++;
                return true;
            }
            _index = _list.Count + 1;
            Current = default(T);
            return false;
        }

        /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>        
        void IEnumerator.Reset()
        {
            _index = 0;
            Current = default(T);
        }
    }
}

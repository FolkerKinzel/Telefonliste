using System;
using FK.Collections;

namespace FK.Telefonliste
{
    [Serializable()]
    public class HashCollection : DCValueTypeObservable<int>
    {
        public HashCollection(int capacity) : base(capacity) { }

        public HashCollection(HashCollection coll) : base(coll) { }
    }
}

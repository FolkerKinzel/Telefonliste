using System;
using FK.Collections;

namespace FK.Telefonliste
{
    [Serializable()]
    public class NameCollection : DCValueTypeObservable<int>
    {
        public NameCollection(int capacity) : base(capacity) { }

        public NameCollection(NameCollection coll) : base(coll) { }
    }
}

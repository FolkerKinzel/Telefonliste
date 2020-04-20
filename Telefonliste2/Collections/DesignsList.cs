using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FK.Telefonliste
{
    [Serializable()]
    public class DesignsList : FK.Collections.ValueTypeObservable<string>
    {
        public DesignsList(int capacity) : base(capacity + 1)
        {
            this.Add(App.STANDARD_DESIGN_NAME);
        }

        //public DesignsList(IList<string> coll)
        //    : base(coll.Count + 1)
        //{
        //    this.Add(App.STANDARD_DESIGN_NAME);
        //    this.AddRange(coll);
        //}


        //public void SortCollection()
        //{
        //    this.Sort((x, y) => x.CompareTo(y));

        //    int i = this.IndexOf(App.STANDARD_DESIGN_NAME);

        //    this.Move(i, 0);
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scan2BIM_WIP
{
    public static class Ext
    {
        public static IEnumerable<Tuple<T1,T2>> ToTupleIE<T1,T2>(this IEnumerable<T1> list1, IEnumerable<T2> list2) 
        {
            var e1 = list1.GetEnumerator();
            var e2 = list2.GetEnumerator();
            while(e1.MoveNext() && e2.MoveNext()) {
                yield return new Tuple<T1, T2>(e1.Current, e2.Current);
            }
        }

        public static IEnumerable<T3> DuoSelect<T1, T2, T3>(this IEnumerable<T1> list1, IEnumerable<T2> list2, Func<T1,T2,T3> f) where T3 : new()
        {
            foreach( var t in ToTupleIE(list1, list2))
            {
                yield return f(t.Item1, t.Item2);
            }
        }
    }
}

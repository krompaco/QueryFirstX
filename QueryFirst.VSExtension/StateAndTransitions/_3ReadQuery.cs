using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst.VSExtension
{
    public class _3ReadQuery
    {
        public State Go(State state, string queryText)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (queryText == null)
                throw new ArgumentNullException(nameof(queryText));
            state._3InitialQueryText = queryText;
            return state;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueryFirst
{
    public class _2ExtractNamesFromUserPartialClass
    {
        // user partial class is deprecated. We need to get all this from query config
        public State Go(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            //if (userPartialClass == null)
            //    throw new ArgumentNullException(nameof(userPartialClass));

            //var match = Regex.Match(userPartialClass, @"(?im)partial class\s*(?'class'[^:\s]+)(\s*:\s*(?'interface'\S*))?");
            //state._2ResultClassName = match.Groups["class"].Value;
            //state._2ResultInterfaceName = match.Groups["interface"].Value;
            ////state._2UserPartialFullText = userPartialClass;
            //if (string.IsNullOrEmpty(state._2ResultInterfaceName))
            //    state._2ResultInterfaceName = state._2ResultClassName;
            //state._2Namespace = Regex.Match(userPartialClass, "(?im)^namespace (\\S+)").Groups[1].Value;
            
            // Life is simpler at the console...
            state._2Namespace = 
            return state;
        }
    }
}

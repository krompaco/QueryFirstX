using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueryFirst
{
    public class _4ExtractNamesFromUserPartialClass
    {
        // user partial class is deprecated. We need to get all this from query config
        public State Go(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            //if (userPartialClass == null)
            //    throw new ArgumentNullException(nameof(userPartialClass));

            //var match = Regex.Match(userPartialClass, @"(?im)partial class\s*(?'class'[^:\s]+)(\s*:\s*(?'interface'\S*))?");
            //state._4ResultClassName = match.Groups["class"].Value;
            //state._4ResultInterfaceName = match.Groups["interface"].Value;
            ////state._4UserPartialFullText = userPartialClass;
            //if (string.IsNullOrEmpty(state._4ResultInterfaceName))
            //    state._4ResultInterfaceName = state._4ResultClassName;
            //state._4Namespace = Regex.Match(userPartialClass, "(?im)^namespace (\\S+)").Groups[1].Value;

            // Life is simpler at the console...
            state._4Namespace = state._3Config.Namespace;
            state._4ResultClassName = state._4ResultClassName ?? state._1BaseName + "Results";
            state._4ResultInterfaceName = state._3Config.ResultInterfaceName ?? state._4ResultClassName;
            return state;
        }
    }
}

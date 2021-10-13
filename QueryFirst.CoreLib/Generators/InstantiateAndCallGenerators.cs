using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryFirst
{
    public class InstantiateAndCallGenerators
    {
        public IQfTextFileWriter FileWriter { get; set; }
        private TinyIoCContainer _tiny = TinyIoCContainer.Current;
        public IEnumerable<QfTextFile> Go(State _state)
        {
            var returnVal = new List<QfTextFile>();
            var generators = _state._3Config.Generators.Select(generator =>
            {
                if (_tiny.CanResolve<IGenerator>(generator.Name))
                    return _tiny.Resolve<IGenerator>(generator.Name);
                else
                {
                    Console.WriteLine($"Cannot resolve generator with RegistrationName {generator.Name}");
                    return null;
                }
            }).Where(gen => gen != null);

            foreach (var generator in generators)
            {
                returnVal.Add( generator.Generate(_state));
            }
            return returnVal;
        }

    }
}

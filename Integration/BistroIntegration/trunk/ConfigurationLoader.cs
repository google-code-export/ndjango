using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using StructureMap;
using NDjango.Interfaces;
using System.Collections;

namespace NDjango.BistroIntegration
{
    public class ConfigurationLoader
    {
        private IContainer container;

        public ConfigurationLoader()
        {
            container = 
                new Container(x =>
                         {
                             x.Scan(y =>
                                        {
                                            y.TheCallingAssembly();
                                            y.ExcludeType<BistroUrlTag>();
                                            y.AssembliesFromPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin"));
                                            y.AddAllTypesOf<ISimpleFilter>();
                                            y.AddAllTypesOf<ITag>();
                                            y.WithDefaultConventions();
                                        });
                         });
        }

        private void PopulateList<T>(IList list, Action<IList, string, T> insertOp)
        {
            var components = container.GetAllInstances<T>();

            foreach (var component in components)
            {
                var attributes = component.GetType().GetCustomAttributes(typeof(NameAttribute), false) as NameAttribute[];
                if (attributes.Length == 0)
                    throw new ApplicationException(String.Format("The type {0} is not marked with a Name attribute.", component.GetType()));

                foreach (var attribute in attributes)
                    insertOp(list, attribute.Name, component);
            }
        }

        public IList<Tag> GetTags()
        {
            var retVal = new List<Tag>();

            PopulateList<ITag>(retVal, (list, name, tag) => list.Add(new Tag(name, tag)));

            return retVal;
        }

        public IList<Filter> GetFilters()
        {
            var retVal = new List<Filter>();

            PopulateList<ISimpleFilter>(retVal, (list, name, filter) => list.Add(new Filter(name, filter)));
            PopulateList<IFilter>(retVal, (list, name, filter) => list.Add(new Filter(name, filter)));

            return retVal;
        }
    }
}

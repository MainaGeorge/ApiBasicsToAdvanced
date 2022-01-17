using System.Collections.Generic;

namespace Entities.LinkModels
{
    public class LinkCollectionWrapper<T> : LinkResourceBase
    {
        public List<T> Values { get; }

        public LinkCollectionWrapper()
        {

        }

        public LinkCollectionWrapper(List<T> values)
        {
            Values = values;
        }
    }
}

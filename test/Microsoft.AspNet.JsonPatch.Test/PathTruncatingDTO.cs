using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.JsonPatch.Test
{
    public class PathTruncatingDTO
    {
        public List<DownDoc> DownDocs { get; set; }
    }

    public class DownDoc
    {
        public List<int> Integers { get; set; }
    }
}

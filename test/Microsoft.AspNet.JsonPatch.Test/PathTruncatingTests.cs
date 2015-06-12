using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.JsonPatch.Test
{
    using Microsoft.AspNet.JsonPatch.Operations;

    using Xunit;

    public class PathTruncatingTests
    {
        [Fact]
        public void PathTruncateTest()
        {
            DownDoc downDoc = new DownDoc() { Integers = new List<int> { 1 } };
            PathTruncatingDTO upDoc = new PathTruncatingDTO() { DownDocs = new List<DownDoc> { downDoc } };


            Operation<PathTruncatingDTO> operation = new Operation<PathTruncatingDTO>("replace", "DownDocs/0/Integers/0", null, 3);
            JsonPatchDocument<PathTruncatingDTO> jsonPatch = new JsonPatchDocument<PathTruncatingDTO>();
            jsonPatch.Operations.Add(operation);
            jsonPatch.ApplyTo(upDoc);

            Assert.Equal(3, upDoc.DownDocs[0].Integers[0]);

        }
    }
}

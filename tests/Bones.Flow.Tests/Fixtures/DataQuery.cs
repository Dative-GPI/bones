using System;
using System.Collections.Generic;

namespace Bones.Flow.Tests
{
    public class DataQuery : IDateBoundedRequest<string>, IActorRequest
    {
        public DateTime DateMin { get; set; }

        public DateTime DateMax  { get; set; }

        public string ActorId  { get; set; }
    }
}
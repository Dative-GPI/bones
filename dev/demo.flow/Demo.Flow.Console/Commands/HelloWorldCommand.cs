using Bones.Flow;
using Demo.Flow.Console.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Demo.Flow.Console.Commands
{
    public class HelloWorldCommand : IRequest, IWriteLineRequest
    {
        public string Message { get; set; } = "Hello world";

        public SmallTest SmallTest { get; set; } = new SmallTest();
        public string Test => throw new System.NotImplementedException();
        public HttpContext Context { get; set; } = new DefaultHttpContext();
    }

    public class SmallTest
    {
        public string SmallText {get; set;} = "small text";
    }
}
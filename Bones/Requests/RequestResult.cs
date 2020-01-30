using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Bones.Exceptions;

namespace Bones.Requests
{
    public class RequestResult
    {
        private RequestResult()
        {
        }

        public bool Succeed { get; set; }
        public object Result { get; set; }
        public IEnumerable<ICustomError> Errors { get; set; }
        public Caller Caller { get; set; }

        public void EnsureSuccess()
        {
            if (!Succeed)
            {
                var messages = Errors.Select(e => e.ToString());
                var message = String.Join("\n", messages);

                var exceptions = Errors.Select(
                    e => e.Exception
                );

                switch (exceptions.Count())
                {
                    case 0:
                        throw new InvalidOperationException("RequestResult failed should always contains errors if used as a result of RequestHandler<T>");
                    case 1:
                        throw exceptions.First();
                    default:
                        throw new AggregateException(exceptions);
                }
            }
        }

        public TResult EnsureSuccess<TResult>()
        {
            this.EnsureSuccess();

            if (Result == null)
            {
                throw new NullResultException();
            }

            if (!(Result is TResult))
            {
                throw new InvalidCastException($"Cannot convert {Result.GetType()} to {typeof(TResult)}");
            }
            return (TResult)Result;
        }

        public static RequestResult Fail(IEnumerable<ICustomError> errors = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath]string filePath = "",
            [CallerLineNumber]int lineNumber = 0)
        {
            if (errors == null) errors = Enumerable.Empty<ICustomError>();

            return new RequestResult()
            {
                Succeed = false,
                Errors = errors,
                Caller = new Caller()
                {
                    MemberName = memberName,
                    FilePath = filePath,
                    LineNumber = lineNumber
                }
            };
        }

        public static RequestResult Success(object result = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath]string filePath = "",
            [CallerLineNumber]int lineNumber = 0)
        {
            return new RequestResult()
            {
                Succeed = true,
                Result = result,
                Caller = new Caller()
                {
                    MemberName = memberName,
                    FilePath = filePath,
                    LineNumber = lineNumber
                }
            };
        }
    }

    public class Caller
    {
        public Caller()
        {

        }

        public string MemberName { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }

        public override string ToString()
        {
            return $"{FilePath}:{LineNumber} - {MemberName}";
        }
    }
}
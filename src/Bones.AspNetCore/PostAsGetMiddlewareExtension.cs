using Microsoft.AspNetCore.Builder;

namespace Bones.AspNetCore
{
    public static class PostAsGetMiddlewareExtension
    {
        public static IApplicationBuilder UsePostAsGetMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PostAsGetMiddleware>();
        }
    }
}

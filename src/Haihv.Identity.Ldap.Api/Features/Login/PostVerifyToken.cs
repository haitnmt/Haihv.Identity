using Carter;
using Haihv.Identity.Ldap.Api.Exceptions;
using Haihv.Identity.Ldap.Api.Services;
using MediatR;

namespace Haihv.Identity.Ldap.Api.Features.Login;

public static class PostVerifyToken
{
    public record Query : IRequest<bool>;

    public class Handler(
        IHttpContextAccessor httpContextAccessor,
        TokenProvider tokenProvider) : IRequestHandler<Query, bool>
    {
        public async Task<bool> Handle(Query request, CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext
                              ?? throw new InvalidOperationException("HttpContext không khả dụng");
            // Lấy thông tin đăng nhập từ context header Bearer token
            var accessToken =  httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            if (!await tokenProvider.VerifyAccessToken(accessToken, cancellationToken))
                throw new InvalidTokenException("Token không hợp lệ");
            return true;
        }
    }
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/verify", async (ISender sender) =>
                {
                    // Không cần try-catch ở đây vì đã có middleware xử lý exception toàn cục
                    var response = await sender.Send(new Query());
                    return response ? Results.Ok("Token hợp lệ!") : Results.Unauthorized();
                })
                .WithTags("Login")
                .RequireAuthorization();
        }
    }
}
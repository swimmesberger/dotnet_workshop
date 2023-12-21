using System.Security.Claims;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);
var authentication = builder.Services.AddAuthentication(options => {
    //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = BasicAuthenticationDefaults.AuthenticationScheme;
});
authentication.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
authentication.AddBasic(options => {
    options.AllowInsecureProtocol = true;
    options.Realm = "CAP";
    options.Events = new BasicAuthenticationEvents {
        OnValidateCredentials = HandleBasicAuthentication
    };
});

builder.Services.AddAuthorization(options => {
    // require authentication for every route
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();
app.MapReverseProxy();
app.Run();
return;

static Task HandleBasicAuthentication(ValidateCredentialsContext context) {
    if (context.Username == "test" && context.Password == "test") {
        var claims = new[] {
            new Claim(
                ClaimTypes.NameIdentifier,
                context.Username,
                ClaimValueTypes.String,
                context.Options.ClaimsIssuer),
            new Claim(
                ClaimTypes.Name,
                context.Username,
                ClaimValueTypes.String,
                context.Options.ClaimsIssuer)
        };

        context.Principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, context.Scheme.Name));
        context.Success();
    }
    return Task.CompletedTask;
}
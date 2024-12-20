using Microsoft.AspNetCore.Components.Authorization;
using Zcrypta.Entities.Models;

namespace Zcrypta.Admin.Authentication
{
    public interface ICustomAuthStateProvider
    {
        Task<AuthenticationState> GetAuthenticationStateAsync();
        Task MarkUserAsAuthenticated(LoginResponseModel model);
        Task MarkUserAsLoggedOut();
    }
}
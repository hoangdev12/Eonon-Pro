using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace WebBTL.Extension
{
    public static class IdentityExtensions
    {
        public static string GetClaimValue(this IIdentity identity, string claimType)
        {
            var claim = ((ClaimsIdentity)identity)?.FindFirst(claimType);
            return claim?.Value ?? string.Empty;
        }
    }
}
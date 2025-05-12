namespace UCMS.Services.Utils
{
    //Fixme: Remove after constant url
    public class UrlBuilder
    {
        public string? BuildUrl(IHttpContextAccessor httpContextAccessor, string relativePath)
        {
            if (relativePath != null) 
                return $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}/{relativePath}";
            return null;
        }
    }
}

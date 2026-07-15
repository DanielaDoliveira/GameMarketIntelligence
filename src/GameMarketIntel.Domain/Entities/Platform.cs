using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace GameMarketIntel.Domain.Entities;

public sealed class Platform
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Family { get; private set; }
    public string? Manutacturer { get; private set; }
    public string? ImageUrl { get; private set; }
    private Platform()
    {
        
    }

    public Platform(string name, string? family = null , string? manutacturer = null, string? imageUrl=null)
    {


        Id = Guid.NewGuid();
        Name = name.Trim();
        Family = NormalizeOptionalText(family);
        Manutacturer = NormalizeOptionalText(manutacturer);
        ImageUrl = ValidateAndNormalizeOptionalUrl(imageUrl);
    }

    private string? ValidateAndNormalizeOptionalUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return null;
        
        var isValidAbsoluteUrl = Uri.TryCreate(imageUrl, UriKind.Absolute,out var parsedUrl);

        if (!isValidAbsoluteUrl || parsedUrl is null)
            throw new ArgumentException("The platform image URL must be a valid absolute URL", nameof(imageUrl));
       
        var isHttpOrHttps = parsedUrl.Scheme == Uri.UriSchemeHttp || parsedUrl.Scheme == Uri.UriSchemeHttps;
       
        if (!isHttpOrHttps)
            throw new ArgumentException("The platform image URL must be use HTTP or HTTPS.", nameof(imageUrl));
      
        return parsedUrl.ToString();
        

    }

    private string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
        
    
}

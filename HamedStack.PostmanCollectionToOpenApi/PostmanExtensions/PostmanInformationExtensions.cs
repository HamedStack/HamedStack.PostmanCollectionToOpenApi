// ReSharper disable UnusedMember.Global

using HamedStack.PostmanCollectionToOpenApi.Models;

namespace HamedStack.PostmanCollectionToOpenApi.PostmanExtensions;

public static class PostmanInformationExtensions
{
    public static OpenApiInfo ToOpenApiInfo(this PostmanInformation information, PostmanToOpenApiSettings setting)
    {
        if (information is null)
        {
            throw new ArgumentNullException(nameof(information));
        }

        if (setting is null)
        {
            throw new ArgumentNullException(nameof(setting));
        }

        var info = new OpenApiInfo
        {
            Description = information.Description?.String,
            Version = information.Version.HasValue
                ? information.Version.Value.String
                : setting.DefaultVersion,
            Title = information.Name
        };

        return info;
    }
}
﻿using HamedStack.PostmanCollectionToOpenApi.Models;

namespace HamedStack.PostmanCollectionToOpenApi.Common;

public static class JsonExtensions
{
    public static IList<JTokenDetail> GetJTokenDetails(this JToken jToken, Dictionary<string, string>? variables = null)
    {
        variables ??= new Dictionary<string, string>();
        var fields = new List<JTokenDetail>();
        var queue = new Queue<JToken>();
        queue.Enqueue(jToken);
        while (queue.Count > 0)
        {
            var currentToken = queue.Dequeue();
            var path = currentToken.Path.SplitPath();
            var key = path.Length == 1 && string.IsNullOrEmpty(path[0])
                ? string.Empty
                : path.Select(x => x.RemoveArrayIndex()).Aggregate((a, b) => a + "." + b);
            var parent = path.Length > 1 ? path.Take(path.Length - 1).ToArray() : null;
            var last = path.Length == 0 ? null : path.Last();
            var parentKey = path.Length <= 1
                ? string.Empty
                : path.Take(path.Length - 1).Select(x => x.RemoveArrayIndex()).Aggregate((a, b) => a + "." + b);
            var tokenString = currentToken.ToString();
            var value = currentToken.ToObject<object>();
            var isArrayOrObject = currentToken.Type is JTokenType.Array or JTokenType.Object;
            switch (currentToken.Type)
            {
                // If the token is a JObject, push its properties onto the stack
                case JTokenType.Object:
                    fields
                        .Add(new JTokenDetail
                        {
                            SharedKey = key,
                            SharedParentKey = parentKey,
                            Path = path,
                            Value = value,
                            JToken = currentToken,
                            JTokenString = tokenString,
                            JTokenType = currentToken.Type,
                            IsObjectOrArray = isArrayOrObject,
                            Parents = parent,
                            LastPathItem = last
                        });
                    foreach (var prop in currentToken.Children<JProperty>())
                    {
                        queue.Enqueue(prop.Value);
                    }

                    break;
                // If the token is an array, push its elements onto the stack
                case JTokenType.Array:
                    fields
                        .Add(new JTokenDetail
                        {
                            SharedKey = key,
                            SharedParentKey = parentKey,
                            Path = path,
                            Value = value,
                            JToken = currentToken,
                            JTokenType = currentToken.Type,
                            JTokenString = currentToken.ToString(),
                            IsObjectOrArray = isArrayOrObject,
                            Parents = parent,
                            LastPathItem = last
                        });
                    foreach (var child in currentToken.Children())
                    {
                        queue.Enqueue(child);
                    }

                    break;
                // Otherwise, the token is a leaf node, so add its name and value to the list
                default:
                    fields
                        .Add(new JTokenDetail
                        {
                            SharedKey = key,
                            SharedParentKey = parentKey,
                            Path = path,
                            Value = value,
                            JToken = currentToken,
                            JTokenType = currentToken.Type,
                            JTokenString = currentToken.ToString(),
                            IsObjectOrArray = isArrayOrObject,
                            Parents = parent,
                            LastPathItem = last
                        });
                    break;
            }
        }

        return fields;
    }

    internal static bool IsPrimitive(this JTokenType type)
    {
        switch (type)
        {
            case JTokenType.Boolean:
            case JTokenType.Date:
            case JTokenType.Float:
            case JTokenType.Guid:
            case JTokenType.Integer:
            case JTokenType.String:
            case JTokenType.TimeSpan:
            case JTokenType.Undefined:
            case JTokenType.Null:
            case JTokenType.Uri:
                return true;

            default:
                return false;
        }
    }

    internal static IOpenApiAny? ToOpenApiAny(this JTokenType jTokenType, object? value)
    {
        switch (jTokenType)
        {
            case JTokenType.Integer:
                return new OpenApiLong(Convert.ToInt64(value));

            case JTokenType.Float:
                return new OpenApiDouble(Convert.ToDouble(value));

            case JTokenType.Boolean:
                return new OpenApiBoolean(Convert.ToBoolean(value));

            case JTokenType.Null:
                return new OpenApiNull();

            case JTokenType.Date:
                return new OpenApiDate(Convert.ToDateTime(value));

            case JTokenType.TimeSpan:
                return new OpenApiDateTime(Convert.ToDateTime(value));

            case JTokenType.None:
            case JTokenType.Object:
            case JTokenType.Array:
            case JTokenType.Constructor:
            case JTokenType.Property:
            case JTokenType.Comment:
            case JTokenType.Bytes:
            case JTokenType.Undefined:
                return null;

            case JTokenType.Raw:
            case JTokenType.Guid:
            case JTokenType.String:
            case JTokenType.Uri:
                return new OpenApiString(Convert.ToString(value));

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
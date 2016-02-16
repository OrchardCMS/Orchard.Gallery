using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;

public class ApiRoutes : IHttpRouteProvider {

    public void GetRoutes(ICollection<RouteDescriptor> routes) {
        foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
            routes.Add(routeDescriptor);
        }
    }

    public IEnumerable<RouteDescriptor> GetRoutes() {
        return new[] {
            new HttpRouteDescriptor {
                Name = "FeedService",
                Priority = -10,
                RouteTemplate = "api/FeedService/Packages()",
                Defaults = new {
                    area = "Orchard.Gallery",
                    controller = "FeedService",
                    action = "GetPackages"
                },
            },
            new HttpRouteDescriptor {
                Name = "FeedService2",
                Priority = -10,
                RouteTemplate = "api/FeedService/Packages",
                Defaults = new {
                    area = "Orchard.Gallery",
                    controller = "FeedService",
                    action = "GetPackages"
                },
            },
            new HttpRouteDescriptor {
                Name = "FeedServiceCount",
                Priority = -10,
                RouteTemplate = "api/FeedService/Packages()/$count",
                Defaults = new {
                    area = "Orchard.Gallery",
                    controller = "FeedService",
                    action = "GetCount"
                },
            },
            new HttpRouteDescriptor {
                Name = "FeedServiceManifest",
                Priority = -10,
                RouteTemplate = "api/FeedService",
                Defaults = new {
                    area = "Orchard.Gallery",
                    controller = "FeedService",
                    action = "GetManifest"
                },
            }
        };
    }
}

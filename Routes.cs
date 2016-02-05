using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Gallery {
    public class Routes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var route in GetRoutes()) {
                routes.Add(route);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Packages/Modules", new RouteValueDictionary {
                            {"area", "Orchard.Gallery"},
                            {"controller", "Package"},
                            {"action", "Index"},
                            {"type", "Module" }
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Gallery"}
                        },
                        new MvcRouteHandler()
                    )
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Packages/Themes", new RouteValueDictionary {
                            {"area", "Orchard.Gallery"},
                            {"controller", "Package"},
                            {"action", "Index"},
                            {"type", "Theme" }
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Gallery"}
                        },
                        new MvcRouteHandler()
                    )
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Packages/{id}", new RouteValueDictionary {
                            {"area", "Orchard.Gallery"},
                            {"controller", "Package"},
                            {"action", "Display"},
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Gallery"}
                        },
                        new MvcRouteHandler()
                    )
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Packages/{id}/{version}", new RouteValueDictionary {
                            {"area", "Orchard.Gallery"},
                            {"controller", "PackageVersion"},
                            {"action", "Display"},
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Gallery"}
                        },
                        new MvcRouteHandler()
                    )
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Download/{id}/{version}", new RouteValueDictionary {
                            {"area", "Orchard.Gallery"},
                            {"controller", "PackageVersion"},
                            {"action", "Download"},
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Gallery"}
                        },
                        new MvcRouteHandler()
                    )
                }
            };
        }
    }
}
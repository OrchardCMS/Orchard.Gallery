using System;
using System.Web.Mvc;
using Orchard.Environment;

namespace Orchard.Gallery.Services {
    public class PackagePartFormatterEvents : IOrchardShellEvents {
        private readonly IWorkContextAccessor _workContextAccessor;

        public PackagePartFormatterEvents(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public void Activated() {
            System.Web.Http.GlobalConfiguration.Configuration.Formatters.Insert(0, new PackagePartFormatter(_workContextAccessor));
        }

        public void Terminating() {
        }
    }
}
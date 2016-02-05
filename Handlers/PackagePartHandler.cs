using System;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Handlers {
    public class PackagePartHandler : ContentHandler {
        public PackagePartHandler(IRepository<PackagePartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            OnIndexing<PackagePart>((context, packagePart) => {

                context.DocumentIndex
                    .Add("package-download-count", packagePart.DownloadCount).Store()
                    .Add("package-extension-type", packagePart.ExtensionType.ToString().ToLowerInvariant()).Store()
                    .Add("package-id", packagePart.PackageId.ToLowerInvariant()).Analyze().Store()
                    .Add("package-summary", packagePart.Summary).Analyze()
                ;
            });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var packagePart = context.ContentItem.As<PackagePart>();

            if (packagePart == null)
                return;

            if (!String.IsNullOrWhiteSpace(packagePart.PackageId)) {
                context.Metadata.Identity.Add("package-id", packagePart.PackageId);
            }

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Gallery"},
                {"Controller", "Package"},
                {"Action", "Display"},
                {"id", packagePart.PackageId}
            };
        }
    }
}

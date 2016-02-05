using System;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Gallery.Models;
using Orchard.Gallery.Utils;

namespace Orchard.Gallery.Handlers {
    public class PackageVersionPartHandler : ContentHandler {
        public PackageVersionPartHandler(IRepository<PackageVersionPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            OnPublished<PackageVersionPart>(UpdateStorage);
            OnImported<PackageVersionPart>(UpdateStorage);
            OnRestored<PackageVersionPart>(UpdateStorage);
        }

        public void UpdateStorage(ContentContextBase context, PackageVersionPart part) {
            var version = SemVersion.Parse(part.Version);
            part.Record.VersionMajor = version.Major;
            part.Record.VersionMinor = version.Minor;
            part.Record.VersionPatch = version.Patch;

            // Update package information
            var container = part.CommonPart.Container.As<PackagePart>();
            if (container != null) {
                part.Record.PackageVersionId = container.PackageId.ToLowerInvariant() + "/" + part.Version;

                if (String.IsNullOrEmpty(container.LatestVersion) || SemVersion.Parse(container.LatestVersion) < version) {
                    container.LatestVersionUtc = part.CommonPart.ModifiedUtc.Value;
                    container.LatestVersion = part.Version;
                }
            }
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<PackageVersionPart>();

            if (part == null)
                return;

            var container = part.CommonPart.Container.As<PackagePart>();

            if (container == null)
                return;

            if (!String.IsNullOrWhiteSpace(container.PackageId)) {
                context.Metadata.Identity.Add("package-version-id", container.PackageId.ToLowerInvariant() + "/" + part.Version);
            }

            context.Metadata.DisplayText = container.TitlePart.Title + " " + part.Version;

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Gallery"},
                {"Controller", "PackageVersion"},
                {"Action", "Display"},
                {"id", container.PackageId},
                {"version", part.Version}
            };
        }
    }
}

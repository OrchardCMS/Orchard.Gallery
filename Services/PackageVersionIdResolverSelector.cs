using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Services {
    public class PackageVersionIdentityResolverSelector : IIdentityResolverSelector {
        private readonly IContentManager _contentManager;

        public PackageVersionIdentityResolverSelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IdentityResolverSelectorResult GetResolver(ContentIdentity contentIdentity) {
            if (contentIdentity.Has("package-version-id")) {
                return new IdentityResolverSelectorResult {
                    Priority = 0,
                    Resolve = ResolveIdentity
                };
            }

            return null;
        }

        private IEnumerable<ContentItem> ResolveIdentity(ContentIdentity identity) {
            var packageVersionId = identity.Get("package-version-id");

            if (packageVersionId == null) {
                return null;
            }

            return _contentManager
                .Query<PackageVersionPart, PackageVersionPartRecord>(VersionOptions.Latest)
                .Where(p => p.PackageVersionId == packageVersionId)
                .List<ContentItem>();
        }
    }
}
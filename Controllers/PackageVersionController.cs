using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Gallery.Models;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Mvc;
using Orchard.Themes;

namespace Orchard.Gallery.Controllers {
    public class PackageVersionController : Controller {
        private readonly IIndexManager _indexManager;
        private readonly IOrchardServices _orchardService;

        public PackageVersionController(
            IOrchardServices orchardService,
            IIndexManager indexManager
            ) {
            _orchardService = orchardService;
            _indexManager = indexManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Themed]
        public ActionResult Display(string id, string version) {

            if (String.IsNullOrWhiteSpace(id) || String.IsNullOrWhiteSpace(version)) {
                return HttpNotFound();
            }

            var packageVersionId = id.ToLowerInvariant() + "/" + version;

            var packageVersion = _orchardService.ContentManager
                .Query<PackageVersionPart, PackageVersionPartRecord>()
                            .Where(p => p.PackageVersionId == packageVersionId)
                            .List()
                            .FirstOrDefault();

            if (packageVersion == null) {
                return HttpNotFound();
            }

            if (!_orchardService.Authorizer.Authorize(Core.Contents.Permissions.ViewContent, packageVersion, T("Cannot view package"))) {
                return new HttpUnauthorizedResult();
            }

            var model = _orchardService.ContentManager.BuildDisplay(packageVersion);
            return new ShapeResult(this, model);
        }

        public ActionResult Download(string id, string version) {

            if (String.IsNullOrWhiteSpace(id) || String.IsNullOrWhiteSpace(version)) {
                return HttpNotFound();
            }

            var packageVersionId = id.ToLowerInvariant() + "/" + version;

            var packageVersion = _orchardService.ContentManager.Query<PackageVersionPart, PackageVersionPartRecord>()
                            .Where(p => p.PackageVersionId == packageVersionId)
                            .List()
                            .FirstOrDefault();

            if (packageVersion == null) {
                return HttpNotFound();
            }

            var package = packageVersion.CommonPart.Container.As<PackagePart>();
            if (package == null) {
                return HttpNotFound();
            }

            packageVersion.DownloadCount++;
            package.DownloadCount++;

            _orchardService.ContentManager.Publish(package.ContentItem);

            return Redirect(packageVersion.PackageUrl);
        }

        ISearchBuilder GetSearchBuilder() {
            return _indexManager
                .GetSearchIndexProvider()
                .CreateSearchBuilder("PackageVersions")
                ;
        }
    }


}
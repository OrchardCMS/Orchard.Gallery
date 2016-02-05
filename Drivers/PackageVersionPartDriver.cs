using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Gallery.Models;
using Orchard.Gallery.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Gallery.Drivers {
    public class PackageVersionPartDriver : ContentPartDriver<PackageVersionPart> {
        private readonly IOrchardServices _orchardServices;

        public PackageVersionPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(PackageVersionPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_PackageVersion_Fields", () => shapeHelper.Parts_PackageVersion_Fields(Package: part)),
                ContentShape("Parts_PackageVersion_Fields_Summary", () => shapeHelper.Parts_PackageVersion_Fields_Summary(Package: part)),
                ContentShape("Parts_PackageVersion_Fields_SummaryAdmin", () => shapeHelper.Parts_PackageVersion_Fields_SummaryAdmin(Package: part))
            );
        }

        protected override DriverResult Editor(PackageVersionPart part, dynamic shapeHelper) {
            return ContentShape("Parts_PackageVersion_Fields_Edit", () => {
                
                var model = new EditPackageVersionViewModel {
                    PackageParts = GetPackagePartsForUser(),
                    PackageVersionPart = part,
                    PackageId = part.CommonPart.Container != null ? part.CommonPart.Container.Id : -1
                };

                if(!model.PackageParts.Any()) {
                    _orchardServices.Notifier.Error(T("You need to create a Package first."));
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts/PackageVersion.Fields", Model: model, Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(PackageVersionPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new EditPackageVersionViewModel {
            };

            updater.TryUpdateModel(model, Prefix, new string[] { "PackageId" }, null);

            // Ensure the use owns the package for this package version
            var packagesForUser = GetPackagePartsForUser();
            if(!packagesForUser.Any(x => x.Id == model.PackageId)) {
                updater.AddModelError("", T("You are not allowed to add a version to this package."));
            }

            model.PackageVersionPart = part;
            model.PackageVersionPart.CommonPart.Container = _orchardServices.ContentManager.Get(model.PackageId);

            var exclude = _orchardServices.Authorizer.Authorize(Permissions.ManageGallery)
                ? null
                : new string[] { "PackageVersionPart.DownloadCount" }
                ;

            updater.TryUpdateModel(model, Prefix, null, exclude);

            Version version;
            if (!Version.TryParse(model.PackageVersionPart.Version, out version)) {
                updater.AddModelError("PackageVersionPart.Version", T("Invalid version."));
            }

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(PackageVersionPart part, ExportContentContext context) {
            var partElement = context.Element(part.PartDefinition.Name);

            partElement.SetAttributeValue("DownloadCount", part.DownloadCount);
            partElement.SetAttributeValue("PackageUrl", part.PackageUrl);
            partElement.SetAttributeValue("Version", part.Version);
        }

        protected override void Importing(PackageVersionPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "DownloadCount", value => {
                part.DownloadCount = Int32.Parse(value);
            });

            context.ImportAttribute(part.PartDefinition.Name, "PackageUrl", value => {
                part.PackageUrl = value;
            });

            context.ImportAttribute(part.PartDefinition.Name, "Version", value => {
                part.Version = value;
            });
        }

        private IEnumerable<PackagePart> GetPackagePartsForUser() {
            if (_orchardServices.Authorizer.Authorize(Permissions.ManageGallery)) {
                return _orchardServices
                    .ContentManager
                    .Query<PackagePart, PackagePartRecord>(VersionOptions.Latest)
                    .List()
                    .OrderBy(x => x.TitlePart.Title);
            }
            else {
                return _orchardServices
                    .ContentManager
                    .Query<PackagePart, PackagePartRecord>(VersionOptions.Latest)
                    .Where<CommonPartRecord>(x => x.OwnerId == _orchardServices.WorkContext.CurrentUser.Id)
                    .List()
                    .OrderBy(x => x.TitlePart.Title);
            }
        }
    }
}

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Gallery.Models;
using Orchard.Localization;

namespace Orchard.Gallery.Drivers {
    public class PackagePartDriver : ContentPartDriver<PackagePart> {
        private readonly IOrchardServices _orchardServices;

        public PackagePartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(PackagePart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Package_Fields", () => shapeHelper.Parts_Package_Fields(Package: part)),
                ContentShape("Parts_Package_Fields_Summary", () => shapeHelper.Parts_Package_Fields_Summary(Package: part)),
                ContentShape("Parts_Package_Fields_SummaryAdmin", () => shapeHelper.Parts_Package_Fields_SummaryAdmin(Package: part)),
                ContentShape("Parts_Package_PackageVersions", () => {
                    var versions = _orchardServices.ContentManager
                        .Query<PackageVersionPart, PackageVersionPartRecord>()
                        .Where<CommonPartRecord>(x => x.Container.Id == part.Id)
                        .List()
                        .OrderByDescending(x => x.Record.VersionMajor)
                        .ThenByDescending(x => x.Record.VersionMinor)
                        .ThenByDescending(x => x.Record.VersionPatch)
                        .ToList();

                    return shapeHelper.Parts_Package_PackageVersions(Package: part, PackageVersions: versions);
                })
            );
        }

        protected override DriverResult Editor(PackagePart part, dynamic shapeHelper) {
            return ContentShape("Parts_Package_Fields_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/Package.Fields", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PackagePart part, IUpdateModel updater, dynamic shapeHelper) {

            var included = _orchardServices.Authorizer.Authorize(Permissions.ManageGallery)
                ? new string[] { "PackageId", "Summary", "ExtensionType", "License", "LicenseUrl", "ProjectUrl", "DownloadCount" }
                : new string[] { "PackageId", "Summary", "ExtensionType", "License", "LicenseUrl", "ProjectUrl" }
                ;

            updater.TryUpdateModel(part, Prefix, included, null);

            // Ensure the package id is unique and valid.

            if (!Regex.IsMatch(part.PackageId, @"^[A-Za-z0-9][A-Za-z0-9\.]+$")) {
                updater.AddModelError("PackageId", T("The package id can only contain alpha-numeric characters and dots."));
            }

            // Ensure a package with the same title doesn't already exist.
            else {
                var existingPackage = _orchardServices.ContentManager
                    .Query<PackagePart, PackagePartRecord>(VersionOptions.Published)
                    .Where(x => x.PackageId == part.PackageId && x.Id != part.Id)
                    .Slice(0, 1)
                    .FirstOrDefault();

                if (existingPackage != null) {
                    updater.AddModelError("title", T("A package with the same package id already exists."));
                }
            }

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(PackagePart part, ExportContentContext context) {
            var partElement = context.Element(part.PartDefinition.Name);

            partElement.SetAttributeValue("DownloadCount", part.DownloadCount);
            partElement.SetAttributeValue("ExtensionType", part.ExtensionType);
            partElement.SetAttributeValue("License", part.License);
            partElement.SetAttributeValue("LicenseUrl", part.LicenseUrl);
            partElement.SetAttributeValue("PackageId", part.PackageId);
            partElement.SetAttributeValue("ProjectUrl", part.ProjectUrl);
            partElement.SetAttributeValue("Summary", part.Summary);
        }

        protected override void Importing(PackagePart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "DownloadCount", value => {
                part.DownloadCount = Int32.Parse(value);
            });
            
            context.ImportAttribute(part.PartDefinition.Name, "ExtensionType", value => {
                part.ExtensionType = (PackagePart.ExtensionTypes)Enum.Parse(typeof(PackagePart.ExtensionTypes), value, true);
            });

            context.ImportAttribute(part.PartDefinition.Name, "License", value => {
                part.License = value;
            });

            context.ImportAttribute(part.PartDefinition.Name, "LicenseUrl", value => {
                part.LicenseUrl = value;
            });

            context.ImportAttribute(part.PartDefinition.Name, "PackageId", value => {
                part.PackageId = value;
            });

            context.ImportAttribute(part.PartDefinition.Name, "ProjectUrl", value => {
                part.ProjectUrl = value;
            });

            context.ImportAttribute(part.PartDefinition.Name, "Summary", value => {
                part.Summary = value;
            });
        }
    }
}

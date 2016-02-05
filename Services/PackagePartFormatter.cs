using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.DisplayManagement;
using Orchard.Gallery.Models;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;

namespace Orchard.Gallery.Services {
    public class PackagePartFormatter : MediaTypeFormatter {
        public static XNamespace atomns = "http://www.w3.org/2005/Atom";
        public static XNamespace dns = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        public static XNamespace mns = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        private readonly IWorkContextAccessor _workContextAccessor;

        public PackagePartFormatter(IWorkContextAccessor workContextAccessor) {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/atom+xml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));

            SupportedEncodings.Add(System.Text.Encoding.UTF8);

            _workContextAccessor = workContextAccessor;
        }

        public override bool CanReadType(Type type) {
            return false;
        }

        public override bool CanWriteType(Type type) {
            if (type == typeof(PackagePart)) {
                return true;
            }
            else {
                Type enumerableType = typeof(IEnumerable<PackagePart>);
                return enumerableType.IsAssignableFrom(type);
            }
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType) {
            base.SetDefaultContentHeaders(type, headers, new MediaTypeHeaderValue("application/xml"));
        }

        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext) {
            var workContext = _workContextAccessor.GetContext();
            var baseUrl = workContext.CurrentSite.BaseUrl;
            var urlHelper = workContext.Resolve<UrlHelper>();

            var httpContext = workContext.HttpContext;
            var shapeDisplay = workContext.Resolve<IShapeDisplay>();
            var shapeFactory = workContext.Resolve<IShapeFactory>();
            
            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes")
            );

            var feed = new XElement(atomns + "feed",
                new XAttribute(XNamespace.Xml + "base", "http://packages.orchardproject.net/FeedService.svc/"),
                new XAttribute(XNamespace.Xmlns + "d", "http://schemas.microsoft.com/ado/2007/08/dataservices"),
                new XAttribute(XNamespace.Xmlns + "m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"),
                new XAttribute("xmlns", "http://www.w3.org/2005/Atom")
            );

            document.Add(feed);

            feed.Add(
                new XElement(atomns + "title", "Packages", new XAttribute("type", "text")),
                new XElement(atomns + "id", "http://packages.orchardproject.net/FeedService.svc/Packages"),
                new XElement(atomns + "updated", DateTime.UtcNow.ToString("o")),
                new XElement(atomns + "link",
                    new XAttribute("rel", "self"),
                    new XAttribute("title", "Packages"),
                    new XAttribute("href", "Packages")
                )
            );

            var packages = value as IEnumerable<PackagePart>;
            if (packages != null) {
                foreach(var package in packages) {
                    feed.Add(CreatePackage(package, urlHelper, baseUrl, shapeDisplay, shapeFactory));
                }
            }
            else {
                var package = value as PackagePart;
                if (package == null) {
                    throw new InvalidOperationException("Cannot serialize type");
                }
                feed.Add(CreatePackage(package, urlHelper, baseUrl, shapeDisplay, shapeFactory));
            }

            var xml = document.ToString();

            using (var writer = new StreamWriter(writeStream)) {
                await writer.WriteAsync(xml);
            }
        }

        private XElement CreatePackage(PackagePart package, UrlHelper urlHelper, string baseUrl, IShapeDisplay shapeDisplay, dynamic shapeFactory) {
            var element = new XElement(atomns + "entry");

            dynamic content = package.ContentItem;
            string iconUrl = null;

            if (content.Package.Icon != null && content.Package.Icon.FirstMediaUrl != null) {
                iconUrl = (string)content.Package.Icon.FirstMediaUrl;
                iconUrl = shapeDisplay.Display(shapeFactory.ResizeMediaUrl(Path: iconUrl, Width: 64, Heigth: 64));
            }

            var screenshots = new XElement(atomns + "link",
                    new XAttribute("rel", "http://schemas.microsoft.com/ado/2007/08/dataservices/related/Screenshots"),
                    new XAttribute("type", "application/atom+xml;type=feed"),
                    new XAttribute("title", "Screenshots"),
                    new XAttribute("href", "Packages(Id='" + package.PackageId + "')/Screenshots")
                    );


            foreach (var media in (IEnumerable<dynamic>)content.Package.Screenshots.MediaParts) {

                string screenshotUrl = media.MediaUrl;
                screenshotUrl = shapeDisplay.Display(shapeFactory.ResizeMediaUrl(Path: screenshotUrl, Width: 164, Heigth: 128));

                screenshots.Add(
                    new XElement(mns + "inline",
                        new XElement(atomns + "feed",
                            new XElement(atomns + "title", "Screenshots", new XAttribute("type", "text")),
                            new XElement(atomns + "id", urlHelper.MakeAbsolute("/FeedService/Packages(Id='" + package.PackageId + "')/Screenshots", baseUrl)),
                            new XElement(atomns + "link",
                                new XAttribute("rel", "self"),
                                new XAttribute("title", "Screenshots"),
                                new XAttribute("href", "Packages(Id='" + package.PackageId + "')/Screenshots")
                                ),
                            new XElement(atomns + "entry",
                                new XElement(atomns + "id", urlHelper.MakeAbsolute("/FeedService.svc/Screenshots(" + (string)media.Id.ToString() + ")", baseUrl)),
                                new XElement(atomns + "title", media.ContentItem.TitlePart.Title, new XAttribute("type", "text")),
                                new XElement(atomns + "content", new XAttribute("type", "application/xml"),
                                    new XElement(mns + "properties",
                                        new XElement(dns + "Id", media.ContentItem.Id, new XAttribute(mns + "type", "Edm.Int32")),
                                        new XElement(dns + "PublishedPackageId", package.PackageId),
                                        new XElement(dns + "ScreenshotUri", urlHelper.MakeAbsolute(screenshotUrl, baseUrl)),
                                        new XElement(dns + "Caption", new XAttribute(mns + "null", "true"))
                                    )
                                )
                            )
                        )
                    )
                );
            }

            element.Add(
                new XElement(atomns + "id", urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(package), baseUrl)),
                new XElement(atomns + "title", package.TitlePart.Title, new XAttribute("type", "text")),
                new XElement(atomns + "summary", package.Summary, new XAttribute("type", "text")),
                new XElement(atomns + "updated", package.LatestVersionUtc.ToString("o")),
                new XElement(atomns + "author",
                    new XElement(atomns + "name", package.CommonPart.Owner.UserName)
                    ),
                screenshots,
                // edit-media
                // edit
                //new XElement(atomns + "category",
                //    new XAttribute("term", "Gallery.Infrastructure.FeedModels.PublishedPackage"),
                //    new XAttribute("scheme", "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme")
                //    ),
                new XElement(atomns + "content",
                    new XAttribute("type", "application/zip"),
                    new XAttribute("src", urlHelper.MakeAbsolute(urlHelper.Action("Download", "PackageVersion", new { id = package.PackageId, version = package.LatestVersion, area = "Orchard.Gallery" }), baseUrl))
                    ),
                new XElement(mns + "properties",
                    new XElement(dns + "Id", package.PackageId),
                    new XElement(dns + "Version", package.LatestVersion),
                    new XElement(dns + "Title", package.TitlePart.Title),
                    new XElement(dns + "Authors", package.CommonPart.Owner.UserName),
                    new XElement(dns + "PackageType", package.ExtensionType),
                    new XElement(dns + "Summary", package.Summary),
                    new XElement(dns + "Description", package.BodyPart.Text),
                    new XElement(dns + "Copyright", "", new XAttribute(mns + "null", "true")),
                    new XElement(dns + "PackageHashAlgorithm", ""),
                    new XElement(dns + "PackageHash", ""),
                    new XElement(dns + "PackageSize", new XAttribute(mns + "type", "Edm.Int64"), "0"),
                    new XElement(dns + "Price", "0", new XAttribute(mns + "type", "Edm.Decimal")),
                    new XElement(dns + "RequireLicenseAcceptance", "false", new XAttribute(mns + "type", "Edm.Boolean")),
                    new XElement(dns + "IsLatestVersion", "true", new XAttribute(mns + "type", "Edm.Boolean")),
                    new XElement(dns + "VersionRating", "5", new XAttribute(mns + "type", "Edm.Double")),
                    new XElement(dns + "VersionRatingsCount", "0", new XAttribute(mns + "type", "Edm.Int32")),
                    new XElement(dns + "VersionDownloadCount", package.DownloadCount, new XAttribute(mns + "type", "Edm.Int32")),
                    new XElement(dns + "Created", package.CommonPart.CreatedUtc.Value.ToString("o"), new XAttribute(mns + "type", "Edm.DateTime")),
                    new XElement(dns + "LastUpdated", package.LatestVersionUtc.ToString("o"), new XAttribute(mns + "type", "Edm.DateTime")),
                    new XElement(dns + "Published", package.CommonPart.PublishedUtc.Value.ToString("o"), new XAttribute(mns + "type", "Edm.DateTime")),
                    new XElement(dns + "ExternalPackageUrl", "", new XAttribute(mns + "null", "true")),
                    new XElement(dns + "ProjectUrl", package.ProjectUrl),
                    new XElement(dns + "LicenseUrl", package.LicenseUrl, new XAttribute(mns + "null", "true")),
                    new XElement(dns + "IconUrl", iconUrl),
                    new XElement(dns + "Rating", "5", new XAttribute(mns + "type", "Edm.Double")),
                    new XElement(dns + "RatingsCount", "0", new XAttribute(mns + "type", "Edm.Int32")),
                    new XElement(dns + "DownloadCount", package.DownloadCount, new XAttribute(mns + "type", "Edm.Int32")),
                    new XElement(dns + "Categories", ""),
                    new XElement(dns + "Tags", new XAttribute(XNamespace.Xml + "space", "preserve"), String.Join(" ", package.TagsPart.CurrentTags.ToArray())),
                    new XElement(dns + "Dependencies", ""),
                    new XElement(dns + "ReportAbuseUrl", ""),
                    new XElement(dns + "GalleryDetailsUrl", "")
                    )
            );

            return element;
        }
       

    }
}
 
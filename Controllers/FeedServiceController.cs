using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Gallery.Models;
using Orchard.Gallery.Services;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Mvc.Extensions;

namespace Orchard.Gallery.Controllers {
    public class FeedServiceController : ApiController {

        private readonly IIndexManager _indexManager;
        private readonly IOrchardServices _orchardService;
        private readonly dynamic _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly IWorkContextAccessor _workContextAccessor;

        public FeedServiceController(
            IOrchardServices orchardService,
            IIndexManager indexManager,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay,
            IWorkContextAccessor workContextAccessor
            ) {
            _orchardService = orchardService;
            _indexManager = indexManager;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;
            _workContextAccessor = workContextAccessor;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        
        public HttpResponseMessage GetManifest() {
            var manifest = new XDocument();
            XNamespace xmlns = "http://www.w3.org/2007/app";
            XNamespace atomns = "http://www.w3.org/2005/Atom";

            var service = new XElement(PackagePartFormatter.atomns + "service",
               new XAttribute(XNamespace.Xml + "base", "http://packages.orchardproject.net/FeedService.svc/"),
               new XAttribute(XNamespace.Xmlns + "atom", atomns),
               new XAttribute(XNamespace.Xmlns + "app", "http://www.w3.org/2007/app"),
               new XAttribute("xmlns", xmlns)
            );

            manifest.Add(service);

            service.Add(
                new XElement(xmlns + "workspace",
                    new XElement(atomns + "title", "Default"),
                    new XElement(xmlns + "collection",
                        new XAttribute("href", "Packages"),
                        new XElement(atomns + "title", "Default")
                    )
                )
            );

            return new HttpResponseMessage() { Content = new StringContent(manifest.ToString(), Encoding.UTF8, "application/xml") };
        }

        public IEnumerable<PackagePart> GetPackages(
            [FromUri(Name = "$filter")] string filter = "",
            [FromUri(Name = "$orderby")] string orderby = "",
            [FromUri(Name = "$skip")] int skip = 0,
            [FromUri(Name = "$top")] int top = 10,
            [FromUri(Name = "$expand")] string expand = "") {


            var searchBuilder = GetSearchBuilder(filter, orderby, skip, top);

            var pageOfResults = searchBuilder.Slice(skip, top).Search();

            var foundIds = pageOfResults.Select(searchHit => searchHit.ContentItemId).ToList();

            var foundItems = _orchardService.ContentManager.GetMany<IContent>(foundIds, VersionOptions.Published, new QueryHints()).ToList();

            return foundItems.Select(x => x.As<PackagePart>()).ToArray();
        }

        public int GetCount([FromUri(Name = "$filter")] string filter = "") {
            var searchBuilder = GetSearchBuilder(filter, "", 0, 0);

            var count = searchBuilder.Count();

            return count;
        }

        private string GetExtensionType(string filter) {
            if (filter.Contains("PackageType eq 'Module'")) {
                return "Module";
            }

            if (filter.Contains("PackageType eq 'Theme'")) {
                return "Theme";
            }

            return "";
        }

        private string GetPackageId(string filter) {

            // tolower(Id) eq 'orchard.module.contrib.googleanalytics'
            var index = filter.IndexOf("tolower(Id) eq ");
            if (index != -1) {
                return filter.Substring(index + 15).Replace("'", "");
            }

            return null;
        }

        private string GetSearchTerms(string filter) {
            string q = "";
            int end;
            
            var index = filter.IndexOf("substringof('");
            if (index != -1) {
                end = filter.IndexOf("'", index + 13);

                q = filter.Substring(index + 13, end - index - 13);

                if(q == "null") {
                    return "";
                }
            }
            
            return q;
        }

        ISearchBuilder GetSearchBuilder(string filter, string orderby, int skip, int top) {
            var searchBuilder = _indexManager
                .GetSearchIndexProvider()
                .CreateSearchBuilder("Packages")
                ;

            var q = GetSearchTerms(filter);
            var type = GetExtensionType(filter);
            var packageId = GetPackageId(filter);

            if (!String.IsNullOrWhiteSpace(q)) {
                searchBuilder.Parse(
                    defaultFields: new[] { "body", "title", "tags", "package-id" },
                    query: q,
                    escape: true
                );
            }

            if (!String.IsNullOrWhiteSpace(type)) {
                searchBuilder.WithField("package-extension-type", type.ToLowerInvariant()).NotAnalyzed().ExactMatch();
            }

            if (!String.IsNullOrEmpty(packageId)) {
                searchBuilder.WithField("package-id", packageId).ExactMatch();
                searchBuilder.Slice(0, 1);
            }

            // Only apply custom order if there is no search filter. Otherwise some oddly related packages
            // might appear at the top.
            if (String.IsNullOrWhiteSpace(q) && !String.IsNullOrWhiteSpace(orderby)) {
                switch (orderby.Split(',')[0].ToLowerInvariant()) {
                    case "downloadcount desc":
                    case "download":
                        searchBuilder.SortByInteger("package-download-count");
                        break;
                    case "ratings":
                    case "title":
                        searchBuilder.SortByString("title").Ascending();
                        break;
                    default:
                        // Order by relevance by default.
                        break;
                }
            }

            return searchBuilder;
        }
    }
}
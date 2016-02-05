using System.Collections.Generic;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.ViewModels {
    public class EditPackageVersionViewModel {
        public PackageVersionPart PackageVersionPart { get; set; }
        public IEnumerable<PackagePart> PackageParts { get; set; }
        public int PackageId { get; set; }
    }
}
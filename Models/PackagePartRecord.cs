using Orchard.ContentManagement.Records;

namespace Orchard.Gallery.Models {
    public class PackagePartRecord : ContentPartRecord {
        public virtual string PackageId { get; set; }
    }
}

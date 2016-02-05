using Orchard.ContentManagement.Records;

namespace Orchard.Gallery.Models {
    public class PackageVersionPartRecord : ContentPartRecord {
        public virtual int VersionMajor { get; set; }
        public virtual int VersionMinor { get; set; }
        public virtual string VersionPatch { get; set; }
        public virtual string PackageVersionId { get; set; }
    }
}

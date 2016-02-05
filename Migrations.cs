using Orchard.Data.Migration;

namespace Orchard.Gallery {
    public class Migrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("PackagePartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("PackageId", c => c.WithLength(1024))
                );

            SchemaBuilder.CreateTable("PackageVersionPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("VersionMajor", c => c.WithDefault(0))
                    .Column<int>("VersionMinor", c => c.WithDefault(0))
                    .Column<string>("VersionPatch", c => c.WithLength(255))
                    .Column<string>("PackageVersionId", c => c.WithLength(1024))
                );

            return 1;
        }
    }
}

using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Gallery {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageGallery = new Permission { Description = "Managing the Gallery", Name = "ManageGallery" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageGallery,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageGallery }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageGallery }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                },
                new PermissionStereotype {
                    Name = "Author"
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Gallery.Utils {
    /// <summary>
    /// Implements Semver 1.0
    /// </summary>
    public class SemVersion : IComparable<SemVersion> {
        public static SemVersion Parse(string value) {
            var version = new SemVersion();

            var segments = value.Split('.');
            if (segments.Length > 0) {
                int major;
                if (int.TryParse(segments[0], out major)) {
                    version.Major = major;
                }
                else {
                    throw new ArgumentException("Invalid major value in version: " + value);
                }
            }

            if (segments.Length > 1) {
                int minor;
                if (int.TryParse(segments[1], out minor)) {
                    version.Minor = minor;
                }
                else {
                    throw new ArgumentException("Invalid minor value in version: " + value);
                }
            }

            if (segments.Length > 2) {
                version.Patch = String.Join(".", segments.Skip(2).ToArray());
            }

            return version;
        }

        public static bool TryParse(string version, out SemVersion semver) {
            try {
                semver = Parse(version);
                return true;
            }
            catch (Exception) {
                semver = null;
                return false;
            }
        }

        private SemVersion() {
            Patch = "";
        }

        public SemVersion(int major, int minor, string patch) : this() {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public int Major { get; set; }
        public int Minor { get; set; }
        public string Patch { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = (SemVersion)obj;

            // do string comparison by reference (possible because strings are interned in ctor)
            return
                Major == other.Major &&
                Minor == other.Minor &&
                Patch == other.Patch;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() {
            unchecked {
                int result = this.Major.GetHashCode();
                result = result * 31 + Minor.GetHashCode();
                result = result * 31 + Patch.GetHashCode();
                return result;
            }
        }

        public static int Compare(SemVersion versionA, SemVersion versionB) {
            if (ReferenceEquals(versionA, null)) {
                return ReferenceEquals(versionB, null) ? 0 : -1;
            }

            return versionA.CompareTo(versionB);
        }

        public int CompareTo(SemVersion other) {
            if (ReferenceEquals(other, null))
                return 1;

            int compare = 0;

            if ((compare = Major.CompareTo(other.Major)) != 0) {
                return compare;
            }

            if ((compare = Minor.CompareTo(other.Minor)) != 0) {
                return compare;
            }

            return Patch.CompareTo(other.Patch);
        }

        /// <summary>
        /// Implicit conversion from string to SemVersion.
        /// </summary>
        /// <param name="version">The semantic version.</param>
        /// <returns>The SemVersion object.</returns>
        public static implicit operator SemVersion(string version) {
            return SemVersion.Parse(version);
        }

        /// <summary>
        /// The override of the equals operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is equal to right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator ==(SemVersion left, SemVersion right) {
            return SemVersion.Equals(left, right);
        }

        /// <summary>
        /// The override of the un-equal operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is not equal to right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator !=(SemVersion left, SemVersion right) {
            return !SemVersion.Equals(left, right);
        }

        /// <summary>
        /// The override of the greater operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is greater than right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator >(SemVersion left, SemVersion right) {
            return SemVersion.Compare(left, right) > 0;
        }

        /// <summary>
        /// The override of the greater than or equal operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is greater than or equal to right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator >=(SemVersion left, SemVersion right) {
            return left == right || left > right;
        }

        /// <summary>
        /// The override of the less operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is less than right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator <(SemVersion left, SemVersion right) {
            return SemVersion.Compare(left, right) < 0;
        }

        /// <summary>
        /// The override of the less than or equal operator. 
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is less than or equal to right <c>true</c>, else <c>false</c>.</returns>
        public static bool operator <=(SemVersion left, SemVersion right) {
            return left == right || left < right;
        }
    }
}
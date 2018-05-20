using SafeApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SafeAuthenticator
{
    public class PermissionSetModel
    {
        public bool Delete { get; set; }
        public bool Insert { get; set; }
        public bool ManagePermissions { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
    }

    public class ContainerPermissionsModel
    {
        private string _containerName;
        public string ContainerName
        {
            get => _containerName.StartsWith("apps/") ? "App Container" : _containerName;
            set => _containerName = value;
        }
        public PermissionSetModel Access { get; set; }
    }

    public class RegisteredAppModel : IComparable, IEquatable<RegisteredAppModel>
    {
        public AppExchangeInfo AppInfo { get; }

        public string AppName => AppInfo.Name;
        public string AppVendor => AppInfo.Vendor;
        public string AppId => AppInfo.Id;
        public List<ContainerPermissionsModel> Containers { get; }

        public RegisteredAppModel(AppExchangeInfo appInfo, IEnumerable<ContainerPermissions> containers)
        {
            AppInfo = appInfo;
            Containers = containers.Select(
              x => new ContainerPermissionsModel
              {
                  Access = new PermissionSetModel
                  {
                      Read = x.Access.Read,
                      Insert = x.Access.Insert,
                      Update = x.Access.Update,
                      Delete = x.Access.Delete,
                      ManagePermissions = x.Access.ManagePermissions
                  },
                  ContainerName = x.ContName
              }).ToList();
        }

        public int CompareTo(object obj)
        {
            if (!(obj is RegisteredAppModel other))
            {
                throw new NotSupportedException();
            }

            return string.CompareOrdinal(AppInfo.Name, other.AppInfo.Name);
        }

        public bool Equals(RegisteredAppModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || AppInfo.Id.Equals(other.AppInfo.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && ((RegisteredAppModel)obj).AppInfo.Id == AppInfo.Id;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}

﻿namespace SAFE.DotNET.Auth.Models {
  public class PermissionSetModel {
    public bool Delete { get; set; }
    public bool Insert { get; set; }
    public bool ManagePermissions { get; set; }
    public bool Read { get; set; }
    public bool Update { get; set; }
  }

  public class ContainerPermissionsModel {
    private string _containerName;

    public string ContainerName {
      get => _containerName.StartsWith("apps/") ? "App Container" : _containerName;
      set => _containerName = value;
    }

    public PermissionSetModel Access { get; set; }
  }
}

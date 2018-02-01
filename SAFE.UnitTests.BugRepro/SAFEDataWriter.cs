using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils;
using SAFE.DotNET.Native;
using SAFE.EventStore.Models;
using SAFE.DotNET.Models;
using Newtonsoft.Json;

namespace SAFE.EventStore.Services
{
    /// <summary>
    /// This EventSourcing protocol
    /// stores references to ImmutableData
    /// in the entries of a MutableData
    /// respresenting a stream.
    /// 
    /// TODO: Implement shared access to mds between apps.
    /// </summary>
    public class SAFEDataWriter : IDisposable
    {
        const string VERSION_KEY = "version";
        const string METADATA_KEY = "metadata";
        const string PROTOCOL = "IData";

        readonly string _protocolId = $"{PROTOCOL}/";

        readonly string AppContainerPath = $"apps/{AppSession.AppId}";

        string DbIdForProtocol(string databaseId)
        {
            return $"{_protocolId}{databaseId}";
        }

        #region Init

        public SAFEDataWriter()
        { }

        public void Dispose()
        {
            FreeState();
            GC.SuppressFinalize(this);
        }

        ~SAFEDataWriter()
        {
            FreeState();
        }

        void FreeState()
        {
            Session.FreeApp();
        }

        #endregion Init

        async Task<List<byte>> GetMdXorName(string plainTextId)
        {
            return await NativeUtils.Sha3HashAsync(plainTextId.ToUtfBytes());
        }

        public async Task Write_x(string databaseId)
        {
            databaseId = DbIdForProtocol(databaseId);

            if (databaseId.Contains(".") || databaseId.Contains("@"))
                throw new NotSupportedException("Unsupported characters '.' and '@'.");

            // Check if account exits first and return error
            var dstPubIdDigest = await GetMdXorName(databaseId);
        }

        public async Task Write_0(string databaseId)
        {
            databaseId = DbIdForProtocol(databaseId);

            if (databaseId.Contains(".") || databaseId.Contains("@"))
                throw new NotSupportedException("Unsupported characters '.' and '@'.");

            // Check if account exits first and return error
            var dstPubIdDigest = await GetMdXorName(databaseId);
            using (var dstPubIdMDataInfoH = await MDataInfo.NewPublicAsync(dstPubIdDigest, 15001))
            {
                // no action
            }
        }

        public async Task Write_1(string databaseId)
        {
            databaseId = DbIdForProtocol(databaseId);

            if (databaseId.Contains(".") || databaseId.Contains("@"))
                throw new NotSupportedException("Unsupported characters '.' and '@'.");

            // Check if account exits first and return error
            var dstPubIdDigest = await GetMdXorName(databaseId);
            using (var dstPubIdMDataInfoH = await MDataInfo.NewPublicAsync(dstPubIdDigest, 15001))
            {
                var accountExists = false;
                try
                {
                    var keysH = await MData.ListKeysAsync(dstPubIdMDataInfoH);
                    keysH.Dispose();
                    accountExists = true;
                }
                catch (Exception)
                {
                    // ignored - acct not found
                }
                if (accountExists)
                {
                    throw new Exception("Id already exists.");
                }
            }

            // no action
        }

        public async Task Write_2(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                // no action
            }
        }

        public async Task Write_3(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                // no action
            }
        }

        public async Task Write_4(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    // no action
                }
            }
        }

        public async Task Write_5(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        // no action
                    }
                }
            }
        }

        public async Task Write_6(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        await MDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
                    }
                }
            }
        }

        public async Task Write_7(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        await MDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
                    }

                    // Create Md for holding categories
                    var categoriesMDataInfoH = await MDataInfo.RandomPrivateAsync(15001);
                    
                    // no action
                }
            }
        }

        public async Task Write_8(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        await MDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
                    }

                    // Create Md for holding categories
                    var categoriesMDataInfoH = await MDataInfo.RandomPrivateAsync(15001);
                    await MData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------
                    
                    // no action
                }
            }
        }

        public async Task Write_9(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        await MDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
                    }

                    // Create Md for holding categories
                    var categoriesMDataInfoH = await MDataInfo.RandomPrivateAsync(15001);
                    await MData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

                    var serializedCategoriesMdInfo = await MDataInfo.SerialiseAsync(categoriesMDataInfoH);
                }
            }
        }


        public async Task Write_10(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        await MDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
                    }

                    // Create Md for holding categories
                    var categoriesMDataInfoH = await MDataInfo.RandomPrivateAsync(15001);
                    await MData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

                    var serializedCategoriesMdInfo = await MDataInfo.SerialiseAsync(categoriesMDataInfoH);

                    // Finally update App Container (store db info to it)
                    var database = new Database
                    {
                        DbId = databaseId,
                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
                    };

                    var serializedDb = JsonConvert.SerializeObject(database);
                    using (var appContH = await AccessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
                    {
                        // no action
                    }
                }
            }
        }

        public async Task Write_11(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        await MDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
                    }

                    // Create Md for holding categories
                    var categoriesMDataInfoH = await MDataInfo.RandomPrivateAsync(15001);
                    await MData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

                    var serializedCategoriesMdInfo = await MDataInfo.SerialiseAsync(categoriesMDataInfoH);

                    // Finally update App Container (store db info to it)
                    var database = new Database
                    {
                        DbId = databaseId,
                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
                    };

                    var serializedDb = JsonConvert.SerializeObject(database);
                    using (var appContH = await AccessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
                    {
                        var dbIdCipherBytes = await MDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
                        // no action
                    }
                }
            }
        }

        public async Task Write_12(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        await MDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
                    }

                    // Create Md for holding categories
                    var categoriesMDataInfoH = await MDataInfo.RandomPrivateAsync(15001);
                    await MData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

                    var serializedCategoriesMdInfo = await MDataInfo.SerialiseAsync(categoriesMDataInfoH);

                    // Finally update App Container (store db info to it)
                    var database = new Database
                    {
                        DbId = databaseId,
                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
                    };

                    var serializedDb = JsonConvert.SerializeObject(database);
                    using (var appContH = await AccessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
                    {
                        var dbIdCipherBytes = await MDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
                        var dbCipherBytes = await MDataInfo.EncryptEntryValueAsync(appContH, serializedDb.ToUtfBytes());
                    }
                }
            }
        }


        public async Task Write_13(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        await MDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
                    }

                    // Create Md for holding categories
                    var categoriesMDataInfoH = await MDataInfo.RandomPrivateAsync(15001);
                    await MData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

                    var serializedCategoriesMdInfo = await MDataInfo.SerialiseAsync(categoriesMDataInfoH);

                    // Finally update App Container (store db info to it)
                    var database = new Database
                    {
                        DbId = databaseId,
                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
                    };

                    var serializedDb = JsonConvert.SerializeObject(database);
                    using (var appContH = await AccessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
                    {
                        var dbIdCipherBytes = await MDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
                        var dbCipherBytes = await MDataInfo.EncryptEntryValueAsync(appContH, serializedDb.ToUtfBytes());
                        using (var appContEntryActionsH = await MDataEntryActions.NewAsync())
                        {
                            // no action
                        }
                    }
                }
            }
        }

        public async Task Write_14(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        await MDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
                    }

                    // Create Md for holding categories
                    var categoriesMDataInfoH = await MDataInfo.RandomPrivateAsync(15001);
                    await MData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

                    var serializedCategoriesMdInfo = await MDataInfo.SerialiseAsync(categoriesMDataInfoH);

                    // Finally update App Container (store db info to it)
                    var database = new Database
                    {
                        DbId = databaseId,
                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
                    };

                    var serializedDb = JsonConvert.SerializeObject(database);
                    using (var appContH = await AccessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
                    {
                        var dbIdCipherBytes = await MDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
                        var dbCipherBytes = await MDataInfo.EncryptEntryValueAsync(appContH, serializedDb.ToUtfBytes());
                        using (var appContEntryActionsH = await MDataEntryActions.NewAsync())
                        {
                            await MDataEntryActions.InsertAsync(appContEntryActionsH, dbIdCipherBytes, dbCipherBytes);
                            // no action
                        }
                    }
                }
            }
        }

        public async Task Write_15(string databaseId)
        {
            await Write_1(databaseId);

            // Create Self Permissions
            using (var categorySelfPermSetH = await MDataPermissionSet.NewAsync())
            {
                await Task.WhenAll(
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
                    MDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

                using (var streamTypesPermH = await MDataPermissions.NewAsync())
                {
                    using (var appSignPkH = await Crypto.AppPubSignKeyAsync())
                    {
                        await MDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
                    }

                    // Create Md for holding categories
                    var categoriesMDataInfoH = await MDataInfo.RandomPrivateAsync(15001);
                    await MData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

                    var serializedCategoriesMdInfo = await MDataInfo.SerialiseAsync(categoriesMDataInfoH);

                    // Finally update App Container (store db info to it)
                    var database = new Database
                    {
                        DbId = databaseId,
                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
                    };

                    var serializedDb = JsonConvert.SerializeObject(database);
                    using (var appContH = await AccessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
                    {
                        var dbIdCipherBytes = await MDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
                        var dbCipherBytes = await MDataInfo.EncryptEntryValueAsync(appContH, serializedDb.ToUtfBytes());
                        using (var appContEntryActionsH = await MDataEntryActions.NewAsync())
                        {
                            await MDataEntryActions.InsertAsync(appContEntryActionsH, dbIdCipherBytes, dbCipherBytes);
                            await MData.MutateEntriesAsync(appContH, appContEntryActionsH); // <----------------------------------------------    Commit ------------------------
                        }
                    }
                }
            }
        }
       
    }
}
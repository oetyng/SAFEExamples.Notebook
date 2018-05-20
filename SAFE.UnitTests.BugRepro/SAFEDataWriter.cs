//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using SAFE.EventStore.Models;
//using SAFE.DotNET.Models;
//using Newtonsoft.Json;
//using System.Linq;
//using SAFE.EventSourcing.Models;
//using SAFE.SystemUtils;
//using System.Collections.Concurrent;

//namespace SAFE.EventStore.Services
//{
//    /// <summary>
//    /// This EventSourcing protocol
//    /// stores references to ImmutableData
//    /// in the entries of a MutableData
//    /// respresenting a stream.
//    /// 
//    /// TODO: Implement shared access to mds between apps.
//    /// </summary>
//    public class SAFEDataWriter : IDisposable
//    {
//        #region Init

//        const string VERSION_KEY = "version";
//        const string METADATA_KEY = "metadata";
//        const string PROTOCOL = "IData";

//        readonly string _protocolId = $"{PROTOCOL}/";

//        readonly string AppContainerPath;

//        string DbIdForProtocol(string databaseId)
//        {
//            return $"{_protocolId}{databaseId}";
//        }

//        Session _session;
//        MDataInfo _mDataInfo;
//        MData _mData;
//        MDataPermissionSet _mDataPermissionSet;
//        MDataPermissions _mDataPermissions;
//        Crypto _crypto;
//        AccessContainer _accessContainer;
//        MDataEntryActions _mDataEntryActions;
//        IData _iData;
//        CipherOpt _cipherOpt;

//        //EventStoreImDProtocol _protocol;

//        public SAFEDataWriter(string appId, Session session)
//        {
//            //_protocol = new EventStoreImDProtocol(appId, session);

//            AppContainerPath = $"apps/{appId}";

//            _session = session;
//            _mDataInfo = new MDataInfo(session);
//            _mData = new MData(session);
//            _mDataPermissionSet = new MDataPermissionSet(session);
//            _mDataPermissions = new MDataPermissions(session);
//            _crypto = new Crypto(session);
//            _accessContainer = new AccessContainer(session);
//            _mDataEntryActions = new MDataEntryActions(session);
//            _mDataEntries = new MDataEntries(session);
//            _iData = new IData(session);
//            _cipherOpt = new CipherOpt(session);
//        }

//        public void Dispose()
//        {
//            FreeState();
//            GC.SuppressFinalize(this);
//        }

//        ~SAFEDataWriter()
//        {
//            FreeState();
//        }

//        void FreeState()
//        {
//            _session.FreeApp();
//        }

//        #endregion Init
        
//        #region Incremental Writes

//        async Task<List<byte>> GetMdXorName(string plainTextId)
//        {
//            return await NativeUtils.Sha3HashAsync(plainTextId.ToUtfBytes());
//        }

//        public async Task<List<byte>> Write_1(string databaseId)
//        {
//            databaseId = DbIdForProtocol(databaseId);

//            if (databaseId.Contains(".") || databaseId.Contains("@"))
//                throw new NotSupportedException("Unsupported characters '.' and '@'.");
            
//            return await GetMdXorName(databaseId);
//        }

//        public async Task Write_2(string databaseId)
//        {
//            var dstPubIdDigest = await Write_1(databaseId);

//            using (var dstPubIdMDataInfoH = await _mDataInfo.NewPublicAsync(dstPubIdDigest, 15001))
//            {
//                // no action
//            }
//        }

//        public async Task Write_3(string databaseId)
//        {
//            databaseId = DbIdForProtocol(databaseId);

//            if (databaseId.Contains(".") || databaseId.Contains("@"))
//                throw new NotSupportedException("Unsupported characters '.' and '@'.");

//            // Check if account exits first and return error
//            var dstPubIdDigest = await GetMdXorName(databaseId);
//            using (var dstPubIdMDataInfoH = await _mDataInfo.NewPublicAsync(dstPubIdDigest, 15001))
//            {
//                var accountExists = false;
//                try
//                {
//                    var keysH = await _mData.ListKeysAsync(dstPubIdMDataInfoH);
//                    keysH.Dispose();
//                    accountExists = true;
//                }
//                catch (Exception)
//                {
//                    // ignored - acct not found
//                }
//                if (accountExists)
//                {
//                    throw new Exception("Id already exists.");
//                }
//            }

//            // no action
//        }

//        public async Task Write_4(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                // no action
//            }
//        }

//        public async Task Write_5(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                // no action
//            }
//        }

//        public async Task Write_6(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    // no action
//                }
//            }
//        }

//        public async Task Write_7(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        // no action
//                    }
//                }
//            }
//        }

//        public async Task Write_8(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }
//                }
//            }
//        }

//        public async Task Write_9(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }

//                    // Create Md for holding categories
//                    var categoriesMDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
                    
//                    // no action
//                }
//            }
//        }

//        public async Task Write_10(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }

//                    // Create Md for holding categories
//                    var categoriesMDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                    await _mData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------
                    
//                    // no action
//                }
//            }
//        }

//        public async Task Write_11(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }

//                    // Create Md for holding categories
//                    var categoriesMDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                    await _mData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

//                    var serializedCategoriesMdInfo = await _mDataInfo.SerialiseAsync(categoriesMDataInfoH);
//                }
//            }
//        }


//        public async Task Write_12(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }

//                    // Create Md for holding categories
//                    var categoriesMDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                    await _mData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

//                    var serializedCategoriesMdInfo = await _mDataInfo.SerialiseAsync(categoriesMDataInfoH);

//                    // Finally update App Container (store db info to it)
//                    var database = new Database
//                    {
//                        DbId = databaseId,
//                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
//                    };

//                    var serializedDb = JsonConvert.SerializeObject(database);
//                    using (var appContH = await _accessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
//                    {
//                        // no action
//                    }
//                }
//            }
//        }

//        public async Task Write_13(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }

//                    // Create Md for holding categories
//                    var categoriesMDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                    await _mData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

//                    var serializedCategoriesMdInfo = await _mDataInfo.SerialiseAsync(categoriesMDataInfoH);

//                    // Finally update App Container (store db info to it)
//                    var database = new Database
//                    {
//                        DbId = databaseId,
//                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
//                    };

//                    var serializedDb = JsonConvert.SerializeObject(database);
//                    using (var appContH = await _accessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
//                    {
//                        var dbIdCipherBytes = await _mDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
//                        // no action
//                    }
//                }
//            }
//        }

//        public async Task Write_14(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }

//                    // Create Md for holding categories
//                    var categoriesMDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                    await _mData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

//                    var serializedCategoriesMdInfo = await _mDataInfo.SerialiseAsync(categoriesMDataInfoH);

//                    // Finally update App Container (store db info to it)
//                    var database = new Database
//                    {
//                        DbId = databaseId,
//                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
//                    };

//                    var serializedDb = JsonConvert.SerializeObject(database);
//                    using (var appContH = await _accessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
//                    {
//                        var dbIdCipherBytes = await _mDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
//                        var dbCipherBytes = await _mDataInfo.EncryptEntryValueAsync(appContH, serializedDb.ToUtfBytes());
//                    }
//                }
//            }
//        }


//        public async Task Write_15(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }

//                    // Create Md for holding categories
//                    var categoriesMDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                    await _mData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

//                    var serializedCategoriesMdInfo = await _mDataInfo.SerialiseAsync(categoriesMDataInfoH);

//                    // Finally update App Container (store db info to it)
//                    var database = new Database
//                    {
//                        DbId = databaseId,
//                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
//                    };

//                    var serializedDb = JsonConvert.SerializeObject(database);
//                    using (var appContH = await _accessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
//                    {
//                        var dbIdCipherBytes = await _mDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
//                        var dbCipherBytes = await _mDataInfo.EncryptEntryValueAsync(appContH, serializedDb.ToUtfBytes());
//                        using (var appContEntryActionsH = await _mDataEntryActions.NewAsync())
//                        {
//                            // no action
//                        }
//                    }
//                }
//            }
//        }

//        public async Task Write_16(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }

//                    // Create Md for holding categories
//                    var categoriesMDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                    await _mData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

//                    var serializedCategoriesMdInfo = await _mDataInfo.SerialiseAsync(categoriesMDataInfoH);

//                    // Finally update App Container (store db info to it)
//                    var database = new Database
//                    {
//                        DbId = databaseId,
//                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
//                    };

//                    var serializedDb = JsonConvert.SerializeObject(database);
//                    using (var appContH = await _accessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
//                    {
//                        var dbIdCipherBytes = await _mDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
//                        var dbCipherBytes = await _mDataInfo.EncryptEntryValueAsync(appContH, serializedDb.ToUtfBytes());
//                        using (var appContEntryActionsH = await _mDataEntryActions.NewAsync())
//                        {
//                            await _mDataEntryActions.InsertAsync(appContEntryActionsH, dbIdCipherBytes, dbCipherBytes);
//                            // no action
//                        }
//                    }
//                }
//            }
//        }

//        public async Task Write_17(string databaseId)
//        {
//            await Write_3(databaseId);

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }

//                    // Create Md for holding categories
//                    var categoriesMDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                    await _mData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

//                    var serializedCategoriesMdInfo = await _mDataInfo.SerialiseAsync(categoriesMDataInfoH);

//                    // Finally update App Container (store db info to it)
//                    var database = new Database
//                    {
//                        DbId = databaseId,
//                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
//                    };

//                    var serializedDb = JsonConvert.SerializeObject(database);
//                    using (var appContH = await _accessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
//                    {
//                        var dbIdCipherBytes = await _mDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
//                        var dbCipherBytes = await _mDataInfo.EncryptEntryValueAsync(appContH, serializedDb.ToUtfBytes());
//                        using (var appContEntryActionsH = await _mDataEntryActions.NewAsync())
//                        {
//                            await _mDataEntryActions.InsertAsync(appContEntryActionsH, dbIdCipherBytes, dbCipherBytes);
//                            await _mData.MutateEntriesAsync(appContH, appContEntryActionsH); // <----------------------------------------------    Commit ------------------------
//                        }
//                    }
//                }
//            }
//        }

//        #endregion Incremental Writes


//        // Creates db with address to category MD
//        public async Task CreateDbAsync(string databaseId)
//        {
//            databaseId = DbIdForProtocol(databaseId);

//            if (databaseId.Contains(".") || databaseId.Contains("@"))
//                throw new NotSupportedException("Unsupported characters '.' and '@'.");

//            // Check if account exits first and return error
//            var dstPubIdDigest = await GetMdXorName(databaseId);
//            using (var dstPubIdMDataInfoH = await _mDataInfo.NewPublicAsync(dstPubIdDigest, 15001))
//            {
//                var accountExists = false;
//                try
//                {
//                    var keysH = await _mData.ListKeysAsync(dstPubIdMDataInfoH);
//                    keysH.Dispose();
//                    accountExists = true;
//                }
//                catch (Exception)
//                {
//                    // ignored - acct not found
//                }
//                if (accountExists)
//                {
//                    throw new Exception("Id already exists.");
//                }
//            }

//            // Create Self Permissions
//            using (var categorySelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kUpdate),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kDelete),
//                    _mDataPermissionSet.AllowAsync(categorySelfPermSetH, MDataAction.kManagePermissions));

//                using (var streamTypesPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamTypesPermH, appSignPkH, categorySelfPermSetH);
//                    }

//                    // Create Md for holding categories
//                    var categoriesMDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                    await _mData.PutAsync(categoriesMDataInfoH, streamTypesPermH, NativeHandle.Zero); // <----------------------------------------------    Commit ------------------------

//                    var serializedCategoriesMdInfo = await _mDataInfo.SerialiseAsync(categoriesMDataInfoH);

//                    // Finally update App Container (store db info to it)
//                    var database = new Database
//                    {
//                        DbId = databaseId,
//                        Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }, // Points to Md holding stream types
//                                                                                                           //Archive = new DataArray {Type = "Buffer", Data = serializedDatabaseMdInfo},
//                                                                                                           //DataEncPk = categoryEncPk.ToHexString(),
//                                                                                                           //DataEncSk = categoryEncSk.ToHexString()
//                    };

//                    var serializedDb = JsonConvert.SerializeObject(database);
//                    using (var appContH = await _accessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
//                    {
//                        var dbIdCipherBytes = await _mDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
//                        var dbCipherBytes = await _mDataInfo.EncryptEntryValueAsync(appContH, serializedDb.ToUtfBytes());
//                        using (var appContEntryActionsH = await _mDataEntryActions.NewAsync())
//                        {
//                            await _mDataEntryActions.InsertAsync(appContEntryActionsH, dbIdCipherBytes, dbCipherBytes);
//                            await _mData.MutateEntriesAsync(appContH, appContEntryActionsH); // <----------------------------------------------    Commit ------------------------
//                        }
//                    }
//                }
//            }
//        }

//        MDataEntries _mDataEntries;

//        public async Task<Result<ReadOnlyStream>> GetStreamAsync(string databaseId, string streamKey, int newSinceVersion = -1)
//        {
//            var (streamName, streamId) = GetKeyParts(streamKey);
//            var batches = new List<EventBatch>();

//            var database = await GetDataBase(databaseId);
//            var dbCategoriesEntries = await GetCategoriesEntries(database); // Get all categories
//            var categoryEntry = dbCategoriesEntries.SingleOrDefault(s => s.Item1.ToUtfString() == streamName);
//            if (categoryEntry.Item1 == null || categoryEntry.Item2 == null)
//                return Result.Fail<ReadOnlyStream>("Stream does not exist!");

//            // Here we get all streams of the category
//            // We get the category md, whose entries contains all streamKeys of the category
//            // (up to 998 though, and then the next 998 can be found when following link in key "next")
//            (List<byte>, List<byte>, ulong) streamEntry;
//            using (var category_MDataInfoH = await _mDataInfo.DeserialiseAsync(categoryEntry.Item2))
//            {
//                using (var categoryDataEntH = await _mData.ListEntriesAsync(category_MDataInfoH))  // get the entries of this specific category
//                {
//                    var streams = await _mDataEntries.ForEachAsync(categoryDataEntH); // lists all instances of this category (key: streamKey, value: serialized mdata info handle)

//                    try
//                    {
//                        streamEntry = streams.First(s => s.Item1.ToUtfString() == streamKey); // find the instance matching this streamKey
//                    }
//                    catch (InvalidOperationException ex)
//                    {
//                        return Result.Fail<ReadOnlyStream>("Stream does not exist!");
//                    }
//                }
//            }

//            using (var stream_MDataInfoH = await _mDataInfo.DeserialiseAsync(streamEntry.Item2))
//            {
//                using (var streamDataEntH = await _mData.ListEntriesAsync(stream_MDataInfoH)) // get the entries of this specific stream instance
//                {
//                    var eventBatchEntries = await _mDataEntries.ForEachAsync(streamDataEntH); // lists all eventbatches stored to this stream instance

//                    var bag = new ConcurrentBag<EventBatch>();
//                    var tasks1 = eventBatchEntries.Select(eventBatchEntry =>
//                        Task.Run(async () => // foreach event batch in stream
//                        {
//                            var key = eventBatchEntry.Item1.ToUtfString();
//                            if (METADATA_KEY == key || VERSION_KEY == key)
//                                return;

//                            // only fetch events more recent than version passed as argument
//                            var versionRange = key.Split('@');
//                            if (newSinceVersion >= int.Parse(versionRange.Last()))
//                                return; // this will speed up retrieval when we have a cached version of the stream locally (as we only request new events since last version)

//                            var jsonBatch = eventBatchEntry.Item2.ToUtfString();
//                            var batch = JsonConvert.DeserializeObject<StoredEventBatch>(jsonBatch);
//                            var eventDataBag = new ConcurrentBag<EventData>();

//                            var tasks2 = batch.Select(stored =>
//                                Task.Run(async () => // foreach event in event batch
//                                {
//                                    var eventData = await GetEventDataFromAddress(stored);
//                                    eventDataBag.Add(eventData);
//                                }));

//                            await Task.WhenAll(tasks2);

//                            bag.Add(new EventBatch(streamKey, eventDataBag.First().MetaData.CausationId, eventDataBag.OrderBy(c => c.MetaData.SequenceNumber).ToList()));
//                        }));

//                    await Task.WhenAll(tasks1);
//                    batches.AddRange(bag.OrderBy(c => c.Body.First().MetaData.SequenceNumber));
//                }
//            }

//            if (batches.Count == 0)
//                return Result.OK((ReadOnlyStream)new EmptyStream(streamName, streamId));
//            else
//                return Result.OK((ReadOnlyStream)new PopulatedStream(streamName, streamId, batches)); // also checks integrity of data structure (with regards to sequence nr)
//        }

//        public async Task<Result<bool>> StoreBatchAsync(string databaseId, EventBatch batch)
//        {
//            // Since the streams only have insert permissions,
//            // the version of it will increase in a deterministic manner,
//            // and we can use the sequenceNr of last event in batch, to derive
//            // a version number to supply to the network when mutating the MD.
//            try
//            {
//                var (streamVersion, mdEntryVersion) = await GetStreamVersionAsync(databaseId, batch.StreamKey);
//                if (streamVersion == -1)
//                    await CreateNewStreamAsync(databaseId, batch);
//                else
//                {
//                    var expectedVersion = batch.Body.First().MetaData.SequenceNumber - 1;
//                    if (streamVersion != expectedVersion)
//                        throw new InvalidOperationException($"Concurrency exception! Expected stream version {expectedVersion}, but found {streamVersion}.");

//                    return await StoreToExistingStream(databaseId, batch, mdEntryVersion); // todo: distinguish MD version exception result from other errors
//                }

//                return Result.OK(true);
//            }
//            catch (Exception ex)
//            {
//                return Result.Fail<bool>(ex.Message);
//            }
//        }

//        async Task CreateNewStreamAsync(string databaseId, EventBatch initBatch)
//        {
//            if (initBatch.Body.First().MetaData.SequenceNumber != 0)
//                throw new InvalidOperationException("First event in a new stream must start with sequence Nr 0!");

//            // Get the database

//            var database = await GetDataBase(databaseId);
//            var categories = await GetCategoriesEntries(database);

//            // Create Self Permissions to the MDs
//            using (var streamSelfPermSetH = await _mDataPermissionSet.NewAsync())
//            {
//                await Task.WhenAll(
//                    _mDataPermissionSet.AllowAsync(streamSelfPermSetH, MDataAction.kInsert),
//                    _mDataPermissionSet.AllowAsync(streamSelfPermSetH, MDataAction.kUpdate));

//                using (var streamPermH = await _mDataPermissions.NewAsync())
//                {
//                    using (var appSignPkH = await _crypto.AppPubSignKeyAsync())
//                    {
//                        await _mDataPermissions.InsertAsync(streamPermH, appSignPkH, streamSelfPermSetH);
//                    }

//                    var (streamName, streamId) = GetKeyParts(initBatch.StreamKey);

//                    // Create an MD, with one event batch in it, with entry type of "stream"
//                    using (var stream_EntriesH = await _mDataEntries.NewAsync())
//                    {
//                        var streamMetadataKey = METADATA_KEY.ToUtfBytes();
//                        var streamMetadata = new MDMetaData
//                        {
//                            { "type", "stream" },
//                            { "streamName", streamName },
//                            { "streamId", streamId.ToString() },
//                        }.Json().ToUtfBytes();
//                        await _mDataEntries.InsertAsync(stream_EntriesH, streamMetadataKey, streamMetadata);

//                        // First event batch in stream added
//                        var batchKey = GetBatchKey(initBatch);
//                        var jsonBatch = await GetJsonBatch(initBatch); // NB: stores to Immutable data!
//                        await _mDataEntries.InsertAsync(stream_EntriesH, batchKey.ToUtfBytes(), jsonBatch.ToUtfBytes());

//                        var versionKey = VERSION_KEY.ToUtfBytes();
//                        await _mDataEntries.InsertAsync(stream_EntriesH, versionKey, initBatch.Body.Last().MetaData.SequenceNumber.ToString().ToUtfBytes());

//                        var stream_MDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                        await _mData.PutAsync(stream_MDataInfoH, streamPermH, stream_EntriesH); // <----------------------------------------------    Commit ------------------------

//                        var serializedStream_MdInfo = await _mDataInfo.SerialiseAsync(stream_MDataInfoH); // Value

//                        var existingCategory = categories.SingleOrDefault(s => s.Item1.ToUtfString() == streamName);
//                        if (existingCategory.Item1 != null && existingCategory.Item2 != null)
//                        {
//                            using (var category_MDataInfoH = await _mDataInfo.DeserialiseAsync(existingCategory.Item2))
//                            {
//                                using (var category_EntriesH = await _mDataEntryActions.NewAsync())
//                                {
//                                    // create the insert action
//                                    await _mDataEntryActions.InsertAsync(category_EntriesH, initBatch.StreamKey.ToUtfBytes(), serializedStream_MdInfo);
//                                    await _mData.MutateEntriesAsync(category_MDataInfoH, category_EntriesH); // <----------------------------------------------    Commit ------------------------
//                                    return;
//                                }
//                            }
//                        }

//                        using (var category_EntriesH_1 = await _mDataEntries.NewAsync())
//                        {
//                            #region Create Category MD
//                            var catMetadataKey = METADATA_KEY.ToUtfBytes();
//                            var catMetadata = new MDMetaData
//                            {
//                                { "type", "category" },
//                                { "typeName", streamName }
//                            }.Json().ToUtfBytes();
//                            await _mDataEntries.InsertAsync(category_EntriesH_1, catMetadataKey, catMetadata);
//                            await _mDataEntries.InsertAsync(category_EntriesH_1, initBatch.StreamKey.ToUtfBytes(), serializedStream_MdInfo);

//                            var category_MDataInfoH = await _mDataInfo.RandomPrivateAsync(15001);
//                            await _mData.PutAsync(category_MDataInfoH, streamPermH, category_EntriesH_1); // <----------------------------------------------    Commit ------------------------

//                            var serializedCategory_MdInfo = await _mDataInfo.SerialiseAsync(category_MDataInfoH); // Value

//                            #endregion Create Category MD


//                            #region Insert new category to Stream Categories Directory MD

//                            using (var categoriesMDataInfoH = await _mDataInfo.DeserialiseAsync(database.Categories.Data))
//                            {
//                                using (var category_EntriesH_2 = await _mDataEntryActions.NewAsync())
//                                {
//                                    // create the insert action
//                                    await _mDataEntryActions.InsertAsync(category_EntriesH_2, streamName.ToUtfBytes(), serializedCategory_MdInfo);
//                                    await _mData.MutateEntriesAsync(categoriesMDataInfoH, category_EntriesH_2); // <----------------------------------------------    Commit ------------------------
//                                }

//                                var serializedCategoriesMdInfo = await _mDataInfo.SerialiseAsync(categoriesMDataInfoH);

//                                // Replace the database stream type info with the updated version
//                                database.Categories = new DataArray { Type = "Buffer", Data = serializedCategoriesMdInfo }; // Points to Md holding stream types

//                                // serialize and encrypt the database
//                                var serializedDb = JsonConvert.SerializeObject(database);
//                                using (var appContH = await _accessContainer.GetMDataInfoAsync(AppContainerPath)) // appContainerHandle
//                                {
//                                    var dbIdCipherBytes = await _mDataInfo.EncryptEntryKeyAsync(appContH, database.DbId.ToUtfBytes());
//                                    var dbCipherBytes = await _mDataInfo.EncryptEntryValueAsync(appContH, serializedDb.ToUtfBytes());
//                                    using (var appContEntryActionsH = await _mDataEntryActions.NewAsync())
//                                    {
//                                        // create the update action, will fail if the entry was updated from somewhere else since db was fetched. todo: reload the db and apply again
//                                        await _mDataEntryActions.UpdateAsync(appContEntryActionsH, dbIdCipherBytes, dbCipherBytes, database.Version + 1);

//                                        // Finally update App Container (store new db info to it)
//                                        await _mData.MutateEntriesAsync(appContH, appContEntryActionsH); // <----------------------------------------------    Commit ------------------------
//                                    }
//                                }
//                            }
//                            #endregion Insert new category to Stream Categories Directory MD
//                        }
//                    }
//                }
//            }
//        }

//        async Task<Result<bool>> StoreToExistingStream(string databaseId, EventBatch batch, ulong mdEntryVersion)
//        {
//            var database = await GetDataBase(databaseId); // Get the database
//            var dbCategoriesEntries = await GetCategoriesEntries(database); // Get all categories
//            var (streamName, streamId) = GetKeyParts(batch.StreamKey);
//            var categoryEntry = dbCategoriesEntries.Single(s => s.Item1.ToUtfString() == streamName);

//            // Get all streams of the category
//            // The category md, whose entries contains all streamKeys of the category 
//            // (up to 998 though, and then the next 998 can be found when following link in key "next")
//            (List<byte>, List<byte>, ulong) streamEntry;
//            using (var category_MDataInfoH = await _mDataInfo.DeserialiseAsync(categoryEntry.Item2))
//            {
//                using (var categoryDataEntH = await _mData.ListEntriesAsync(category_MDataInfoH))  // get the entries of this specific category
//                {
//                    var streams = await _mDataEntries.ForEachAsync(categoryDataEntH); // lists all instances of this type

//                    try
//                    {
//                        streamEntry = streams.First(s => s.Item1.ToUtfString() == batch.StreamKey); // find the instance matching this streamKey
//                    }
//                    catch (InvalidOperationException ex)
//                    {
//                        return Result.Fail<bool>("Stream does not exist!");
//                    }
//                }
//            }

//            // With ImD-Protocol, the entries of a stream, are the 
//            // serialized batch of addresses to ImmutableData
//            // which hold the actual event.
//            using (var stream_MDataInfoH = await _mDataInfo.DeserialiseAsync(streamEntry.Item2))
//            {
//                var batchKey = GetBatchKey(batch);
//                var jsonBatch = await GetJsonBatch(batch); // NB: stores to Immutable data!
//                var newStreamVersion = batch.Body.Last().MetaData.SequenceNumber.ToString().ToUtfBytes();
//                var newMdEntryVersion = mdEntryVersion + 1;
//                using (var streamEntryActionsH = await _mDataEntryActions.NewAsync())
//                {
//                    // create the insert action
//                    await _mDataEntryActions.InsertAsync(streamEntryActionsH, batchKey.ToUtfBytes(), jsonBatch.ToUtfBytes());
//                    await _mDataEntryActions.UpdateAsync(streamEntryActionsH, VERSION_KEY.ToUtfBytes(), newStreamVersion, newMdEntryVersion);
//                    // mdEntryVersion gives proper concurrency management of writes to streams.
//                    // We can now have multiple concurrent processes writing to the same stream and maintain version sequence intact

//                    try
//                    {
//                        // Finally update md (store batch to it)
//                        await _mData.MutateEntriesAsync(stream_MDataInfoH, streamEntryActionsH); // <----------------------------------------------    Commit ------------------------
//                    }
//                    catch (Exception ex)
//                    {
//                        if (ex.Message.Contains("InvalidSuccessor"))
//                        {
//                            var (_, mdVersion) = await GetStreamVersionAsync(databaseId, batch.StreamKey);
//                            return Result.Fail<bool>($"Concurrency exception! Expected MD entry version {mdEntryVersion} is not valid. Current version is {mdVersion}.");
//                        }
//                        return Result.Fail<bool>(ex.Message);
//                    }


//                    // the exception thrown from MutateEntriesAsync when wrong mdEntryVersion entered (i.e. stream modified since last read)
//                    // will have to be handled by the outer scope. Preferred way is to load latest version of stream (i.e. load new events)
//                    // and then apply the cmd to the aggregate again. This becomes a bit of a problem in case of doing outgoing requests from within the aggregate
//                    // as we might not want to do those requests again (depending on their idempotency at the remote recipient).
//                    // implementing logic where an outbound non-idempotent request has to be done, will be risky unless managed.
//                    // currently not aware of a well established pattern for this
//                    // one way could be to run it over multiple cmds, so that we first set the AR to be in a receiving state, 
//                    // => (SetReceiving), from here it should reject additional SetReceiving cmds, and only accept 
//                    // the cmd which performs the outbound request, if it is issued by the same causer (i.e. making it single threaded).
//                }
//            }

//            return Result.OK(true);
//        }

//        async Task<string> GetJsonBatch(EventBatch batch)
//        {
//            ConcurrentBag<object> imd = new ConcurrentBag<object>();

//            var tasks = batch.Body.Select(x =>
//                Task.Run(async () =>
//                {
//                    imd.Add(new StoredEvent
//                    {
//                        MetaData = x.MetaData,
//                        DataMapAddress = await StoreImmutableData(x.Payload)
//                    });
//                }));

//            await Task.WhenAll(tasks);

//            var jsonBatch = imd.ToList().Json(); // to list might not be necessary here

//            return jsonBatch;
//        }

//        async Task<List<byte>> StoreImmutableData(byte[] payload)
//        {
//            using (var cipherOptHandle = await _cipherOpt.NewPlaintextAsync())
//            {
//                using (var seWriterHandle = await _iData.NewSelfEncryptorAsync())
//                {
//                    await _iData.WriteToSelfEncryptorAsync(seWriterHandle, payload.ToList());
//                    var dataMapAddress = await _iData.CloseSelfEncryptorAsync(seWriterHandle, cipherOptHandle);
//                    //await _iData.SelfEncryptorWriterFreeAsync(seWriterHandle);
//                    return dataMapAddress;
//                }
//            }
//        }

//        class StoredEventBatch : List<StoredEvent>
//        { }

//        class StoredEvent
//        {
//            public MetaData MetaData { get; set; }
//            public List<byte> DataMapAddress { get; set; }
//        }

//        class MDMetaData : Dictionary<string, string>
//        {

//        }

//        public async Task<(int, ulong)> GetStreamVersionAsync(string databaseId, string streamKey)
//        {
//            var (streamName, streamId) = GetKeyParts(streamKey);
//            var batches = new List<EventBatch>();

//            var database = await GetDataBase(databaseId);
//            var dbCategoriesEntries = await GetCategoriesEntries(database); // Get all categories
//            var categoryEntry = dbCategoriesEntries.SingleOrDefault(s => s.Item1.ToUtfString() == streamName);
//            if (categoryEntry.Item1 == null || categoryEntry.Item2 == null)
//                return (-1, 0); // i.e. does not exist

//            // Here we get all streams of the category
//            // We get the category md, whose entries contains all streamKeys of the category
//            // (up to 998 though, and then the next 998 can be found when following link in key "next")
//            (List<byte>, List<byte>, ulong) streamEntry;
//            using (var category_MDataInfoH = await _mDataInfo.DeserialiseAsync(categoryEntry.Item2))
//            {
//                using (var categoryDataEntH = await _mData.ListEntriesAsync(category_MDataInfoH))  // get the entries of this specific category
//                {
//                    var streams = await _mDataEntries.ForEachAsync(categoryDataEntH); // lists all instances of this category (key: streamKey, value: serialized mdata info handle)

//                    try
//                    {
//                        streamEntry = streams.First(s => s.Item1.ToUtfString() == streamKey); // find the instance matching this streamKey
//                    }
//                    catch (InvalidOperationException ex)
//                    {
//                        return (-1, 0); // i.e. does not exist
//                    }
//                }
//            }

//            using (var stream_MDataInfoH = await _mDataInfo.DeserialiseAsync(streamEntry.Item2))
//            {
//                using (var streamDataEntH = await _mData.ListEntriesAsync(stream_MDataInfoH)) // get the entries of this specific stream instance
//                {
//                    var eventBatchEntries = await _mDataEntries.ForEachAsync(streamDataEntH); // lists all eventbatches stored to this stream instance

//                    var entry = eventBatchEntries.First(e => VERSION_KEY == e.Item1.ToUtfString());
//                    var versionString = entry.Item2.ToUtfString();
//                    var version = int.Parse(versionString);
//                    return (version, entry.Item3);
//                }
//            }
//        }


//        async Task<EventData> GetEventDataFromAddress(StoredEvent stored)
//        {
//            using (var seReaderHandle = await _iData.FetchSelfEncryptorAsync(stored.DataMapAddress))
//            {
//                var len = await _iData.SizeAsync(seReaderHandle);
//                var readData = await _iData.ReadFromSelfEncryptorAsync(seReaderHandle, 0, len);

//                var eventData = new EventData(readData.ToArray(),
//                    stored.MetaData.CorrelationId,
//                    stored.MetaData.CausationId,
//                    stored.MetaData.EventClrType,
//                    stored.MetaData.Id,
//                    stored.MetaData.Name,
//                    stored.MetaData.SequenceNumber,
//                    stored.MetaData.TimeStamp);

//                return eventData;
//            }
//        }

//        async Task<Database> GetDataBase(string databaseId)
//        {
//            databaseId = DbIdForProtocol(databaseId);

//            List<byte> content;
//            using (var appContH = await _accessContainer.GetMDataInfoAsync(AppContainerPath))
//            {
//                var dbIdCipherBytes = await _mDataInfo.EncryptEntryKeyAsync(appContH, databaseId.ToUtfBytes());
//                var entryValue = await _mData.GetValueAsync(appContH, dbIdCipherBytes);
//                var dbCipherBytes = entryValue.Item1;

//                content = await _mDataInfo.DecryptAsync(appContH, dbCipherBytes);

//                var database = JsonConvert.DeserializeObject<Database>(content.ToUtfString());

//                //database.Version = entryValue.Item2;
//                WriteToInternal(database, nameof(database.Version), entryValue.Item2);

//                return database;
//            }
//        }

//        async Task<List<(List<byte>, List<byte>, ulong)>> GetCategoriesEntries(Database database)
//        {
//            List<(List<byte>, List<byte>, ulong)> dbCategoriesEntries;
//            using (var dbCategoriesDataInfoH = await _mDataInfo.DeserialiseAsync(database.Categories.Data))
//            {
//                using (var dbCategoriesDataEntH = await _mData.ListEntriesAsync(dbCategoriesDataInfoH))
//                {
//                    dbCategoriesEntries = await _mDataEntries.ForEachAsync(dbCategoriesDataEntH);
//                }
//            }

//            return dbCategoriesEntries;
//        }

//        string GetBatchKey(EventBatch batch)
//        {
//            var batchKey = $"{batch.Body.First().MetaData.SequenceNumber}@{batch.Body.Last().MetaData.SequenceNumber}";
//            return batchKey;
//        }

//        (string, long) GetKeyParts(string streamKey)
//        {
//            var source = streamKey.Split('@');
//            var streamName = source[0];
//            var streamId = long.Parse(source[1]);
//            return (streamName, streamId);
//        }

//        void WriteToInternal<T>(T instance, string propertyName, object value)
//        {
//            var prop = typeof(T).GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
//            prop.SetValue(instance, value);
//        }
//    }
//}
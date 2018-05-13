using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CDN.Domain.Constants;
using CDN.Domain.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace CDN.Domain.Repositories.Implementations
{
    public class FileObjectRepository : IFileObjectRepository
    {
        private readonly IConfiguration _configuration;

        public FileObjectRepository(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CdnFileObject>> GetObjectsAsync(int? serverId, DateTime? lastAccessTime)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                var sqlText = @"SELECT * FROM CDN_FileObject WHERE 1 = 1 " +
                              $"{(lastAccessTime.HasValue ? "AND LastAccess<@LastAccess" : string.Empty)}" +
                              $"{(serverId.HasValue ? "AND ServerId=@ServerId" : string.Empty)}";

                var parameters = new DynamicParameters();

                if (serverId.HasValue) parameters.Add("@ServerId", serverId.Value);
                if (lastAccessTime.HasValue) parameters.Add("@LastAccess", lastAccessTime.Value);

                return await connection.QueryAsync<CdnFileObject>(sqlText, parameters, commandType: CommandType.Text);
            }
        }

        /// <inheritdoc />
        public async Task<CdnFileObject> GetObjectByKeyAsync(string objectKey, int? versionId, int? serverId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                var sqlText = @"SELECT * FROM CDN_FileObject
                                WHERE Id = @Id" 
                              + $"{(versionId.HasValue ? " AND VersionId=@VersionId" : string.Empty)}"
                              + $"{(serverId.HasValue ? " AND ServerId=@ServerId" : string.Empty)}";

                var parameters = new DynamicParameters();
                parameters.Add("@Id", objectKey);

                if (versionId.HasValue) parameters.Add("@VersionId", versionId.Value);
                if (serverId.HasValue) parameters.Add("@ServerId", serverId.Value);

                return await connection.QueryFirstOrDefaultAsync<CdnFileObject>(sqlText, parameters, commandType: CommandType.Text);
            }
        }

        public async Task AddObjectAsync(CdnFileObject fileObject)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                var sqlText = @"INSERT INTO CDN_FileObject(Id, VersionId, ServerId, Size, DateUploaded, LastAccess, UploadId)
                                VALUES (@Id, @VersionId, @ServerId, @Size, @DateUploaded, @LastAccess, @UploadId)";

                await connection.ExecuteAsync(sqlText,
                    new
                    {
                        fileObject.Id,
                        fileObject.VersionId,
                        fileObject.ServerId,
                        fileObject.Size,
                        fileObject.DateUploaded,
                        fileObject.LastAccess,
                        fileObject.UploadId
                    }, commandType: CommandType.Text);
            }
        }

        public async Task<CdnFileObject> UpdateObjectAsync(CdnFileObject fileObject)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                var sqlText = @"UPDATE CDN_FileObject
                                SET Id=@Id, VersionId=@VersionId, ServerId=@ServerId, Size=@Size, 
                                    DateUploaded=@DateUploaded, LastAccess=@LastAccess, UploadId=@UploadId
                                WHERE Id=@Id AND VersionId=@VersionId AND ServerId=@ServerId";

                await connection.ExecuteAsync(sqlText,
                    new
                    {
                        fileObject.Id,
                        fileObject.VersionId,
                        fileObject.ServerId,
                        fileObject.Size,
                        fileObject.DateUploaded,
                        fileObject.LastAccess,
                        fileObject.UploadId
                    }, commandType: CommandType.Text);

                return fileObject;
            }
        }

        public async Task DeleteObjectAsync(CdnFileObject fileObject)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                var sqlText = @"DELETE FROM CDN_FileObject
                                WHERE Id=@Id AND VersionId=@VersionId AND ServerId=@ServerId";

                await connection.ExecuteAsync(sqlText,
                    new
                    {
                        fileObject.Id,
                        fileObject.VersionId,
                        fileObject.ServerId
                    }, commandType: CommandType.Text);

            }
        }

        #region Private members
        
        private SqlConnection GetConnection()
        {
            var connection = new SqlConnection(_configuration[$"{ConfigurationConstants.CDN_SECTION_NAME}:DatabaseConnection"]);
            return connection;
        }

        #endregion
    }
}
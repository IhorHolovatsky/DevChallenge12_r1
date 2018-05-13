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
    public class CdnServerRepository : ICdnServerRepository
    {
        private readonly IConfiguration _configuration;

        public CdnServerRepository(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        ///<inheritdoc />
        public async Task<IEnumerable<CdnServer>> GetServersAsync(CdnServerRole? serverRole)
        {
            using (var connection = OpenConnection())
            {
                var sqlText = @"SELECT *, ServerRoleId as ServerRole FROM CDN_Server " + 
                              $"{(serverRole.HasValue ? "WHERE ServerRoleId=@ServerRoleId" : string.Empty)}";

                var parameters = new DynamicParameters();
                if (serverRole.HasValue)
                {
                    parameters.Add("@ServerRoleId", (int)serverRole.Value);
                }

                return await connection.QueryAsync<CdnServer>(sqlText, parameters, commandType: CommandType.Text);
            }
        }

        public CdnServer GetServerById(int serverId)
        {
            using (var connection = OpenConnection())
            {
                var sqlText = @"SELECT *, ServerRoleId as ServerRole FROM CDN_Server
                                WHERE Id = @Id";
                return connection.QueryFirstOrDefault<CdnServer>(sqlText, new { Id = serverId }, commandType: CommandType.Text);
            }
        }

        public void InsertOrUpdateServer(CdnServer server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            var serverFromDb = GetServerById(server.Id);

            if (serverFromDb == null)
            {
                using (var connection = OpenConnection())
                {
                    var sqlText = @"INSERT INTO CDN_Server(Id, Latitude, Longitude, FreeSpace, IpAddress, Host, ServerRoleId, IsOnline, Name)
                                    VALUES(@Id, @Latitude, @Longitude, @FreeSpace, @IpAddress, @Host, @ServerRoleId, @IsOnline, @Name)";
                    connection.Execute(sqlText,
                                       new
                                       {
                                           server.Id,
                                           server.Latitude,
                                           server.Longitude,
                                           server.FreeSpace,
                                           server.IpAddress,
                                           server.Host,
                                           ServerRoleId = (int)server.ServerRole,
                                           server.IsOnline,
                                           server.Name
                                       }, commandType: CommandType.Text);
                    return;
                }
            }

            using (var connection = OpenConnection())
            {
                var sqlText = @"UPDATE CDN_Server
                                SET Latitude = @Latitude, Longitude = @Longitude, FreeSpace = @FreeSpace, IpAddress = @IpAddress, 
                                    Host= @Host, ServerRoleId = @ServerRoleId, IsOnline = @IsOnline, Name = @Name
                                WHERE Id = @Id";
                connection.Execute(sqlText,
                                   new
                                   {
                                       server.Id,
                                       server.Latitude,
                                       server.Longitude,
                                       server.FreeSpace,
                                       server.IpAddress,
                                       server.Host,
                                       ServerRoleId = (int)server.ServerRole,
                                       server.IsOnline,
                                       server.Name
                                   }, commandType: CommandType.Text);
            }
        }


        #region Private methods

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(_configuration[$"{ConfigurationConstants.CDN_SECTION_NAME}:DatabaseConnection"]);
            connection.Open();
            return connection;
        }

        #endregion
    }
}
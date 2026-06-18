using System.Data;

namespace Shopwave.Modules.Stores.Application.Abstractions;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
using System;
using System.Data.Entity;
using System.Data.Entity.Core.Common;

namespace DevBoard.Data
{
    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            SetProviderFactory("System.Data.SQLite", System.Data.SQLite.SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", System.Data.SQLite.EF6.SQLiteProviderFactory.Instance);

            var providerServicesType = Type.GetType("System.Data.SQLite.EF6.SQLiteProviderServices, System.Data.SQLite.EF6");
            if (providerServicesType != null)
            {
                var instanceField = providerServicesType.GetField("Instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (instanceField != null)
                {
                    var instance = (DbProviderServices)instanceField.GetValue(null);
                    SetProviderServices("System.Data.SQLite", instance);
                    SetProviderServices("System.Data.SQLite.EF6", instance);
                }
            }
        }
    }
}

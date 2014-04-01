using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace SocialStream.Data.Repositories
{
	internal static class Sql
	{
		private static string GetConnectionString(string db)
		{
			return ConfigurationManager.ConnectionStrings[db].ConnectionString;
		}

		internal static void ExecuteNonQuery(string sql, string db, params SqlParameter[] parameters)
		{
			string connectionString = GetConnectionString(db);

			using (var connection = new SqlConnection(connectionString))
			{
				using (var command = new SqlCommand(sql, connection))
				{
					if (parameters != null)
					{
						command.Parameters.AddRange(parameters);
					}

					connection.Open();

					command.ExecuteNonQuery();
				}
			}
		}

		internal static IList<T> ExecuteReader<T>(string sql, string db, SqlParameter[] parameters,
			Func<SqlDataReader, T> processRowAction)
		{
			var result = new List<T>();

			string connectionString = GetConnectionString(db);

			using (var connection = new SqlConnection(connectionString))
			{
				using (var command = new SqlCommand(sql, connection))
				{
					if (parameters != null)
					{
						command.Parameters.AddRange(parameters);
					}

					connection.Open();

					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add(processRowAction(reader));
						}
					}
				}
			}

			return result;
		}
	}
}
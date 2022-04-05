using BorgLink.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Repositories
{
    public class JobRepository
    {
        private readonly SqlConnection _connection;

        public JobRepository(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        public bool DoesJobExist(Borg borg)
        {
            // Create the arguments
            var argumentsFalse = JsonConvert.SerializeObject(new string[] { JsonConvert.SerializeObject(borg), "false" });
            var argumentsTrue = JsonConvert.SerializeObject(new string[] { JsonConvert.SerializeObject(borg), "true" });

            // Open connection
            _connection.Open();

            // Define return
            var jobAlreadyExists = false;

            // Safe execute
            try
            {
                SqlCommand command = new SqlCommand($"Select Id from Hangfire.Job where (Arguments=@argumentsTrue or Arguments=@argumentsFalse) and StateName != 'Deleted'", _connection);
                command.Parameters.AddWithValue("@argumentsTrue", argumentsTrue);
                command.Parameters.AddWithValue("@argumentsFalse", argumentsFalse);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                        jobAlreadyExists = true;
                }
            }
            finally
            {
                _connection.Close();
            }

            return jobAlreadyExists;
        }
    }
}

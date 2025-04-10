using FutureTechAcademy.Models;
using Microsoft.Azure.Cosmos;

namespace FutureTechAcademy.Services
{
    // Services/CosmosDbService.cs
    public class CosmosDbService
    {
        private readonly Container _container;

        public CosmosDbService(IConfiguration config)
        {
            var client = new CosmosClient(
                config["CosmosDb:Endpoint"],
                config["CosmosDb:Key"]
            );

            var databaseName = config["CosmosDb:DatabaseName"];
            var containerName = config["CosmosDb:ContainerName"];
            var partitionKeyPath = "/id"; // assuming 'id' is the partition key

            var database = client.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();
            _container = database.Database
                .CreateContainerIfNotExistsAsync(containerName, partitionKeyPath)
                .GetAwaiter().GetResult()
                .Container;
        }

        public async Task AddStudentAsync(Student student)
        {
            await _container.CreateItemAsync(student, new PartitionKey(student.Id));
        }

        public async Task<Student> GetStudentByEmailAsync(string email)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.Email = @email")
                    .WithParameter("@email", email);

                var iterator = _container.GetItemQueryIterator<Student>(query);
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    return response.FirstOrDefault();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Student>> GetStudentsAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _container.GetItemQueryIterator<Student>(query);
            var results = new List<Student>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }
            return results;
        }

        public async Task<Student> GetStudentByIdAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Student>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task UpdateStudentAsync(Student student)
        {
            await _container.UpsertItemAsync(student, new PartitionKey(student.Id));
        }

        public async Task DeleteStudentAsync(string id)
        {
            await _container.DeleteItemAsync<Student>(id, new PartitionKey(id));
        }

    }

}

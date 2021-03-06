using System.Threading.Tasks;
using TestEnvironment.Docker.Containers;
using Xunit;

namespace TestEnvironment.Docker.Tests
{
    public class DockerEnvironmentTests
    {
        [Fact]
        public async Task CreateDockerEnvironment()
        {
            // Create the environment using builder pattern
            var environment = new DockerEnvironmentBuilder()
                .AddContainer("my-nginx", "nginx")
                .AddElasticsearchContainer("my-elastic")
                .AddMssqlContainer("my-mssql", "HelloK11tt_0")
                .Build();

            // Up it.
            await environment.Up();

            // Play with containers.
            var mssql = environment.GetContainer<MssqlContainer>("my-mssql");
            var elastic = environment.GetContainer<ElasticsearchContainer>("my-elastic");

            // Down it.
            await environment.Down();

            // Dispose (remove).
            environment.Dispose();
        }
    }
}

﻿using Docker.DotNet;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace TestEnvironment.Docker.Containers
{
    public sealed class MssqlContainer : Container
    {
        private const int AttemptsCount = 60;
        private const int DelayTime = 1000;

        private readonly string _saPassword;

        public MssqlContainer(DockerClient dockerClient, string name, string saPassword, string imageName = "microsoft/mssql-server-linux", string tag = "latest", Action<string> logger = null, bool isDockerInDocker = false)
            : base(dockerClient, name, imageName, tag,
                environmentVariables: new[] { ("ACCEPT_EULA", "Y"), ("SA_PASSWORD", saPassword), ("MSSQL_PID", "Express") },
                logger, isDockerInDocker)
        {
            _saPassword = saPassword;
        }

        protected override async Task WaitForReadiness(CancellationToken token = default)
        {
            var attempts = AttemptsCount;
            var isAlive = false;
            do
            {
                try
                {
                    using (var connection = new SqlConnection(GetConnectionString()))
                    using (var command = new SqlCommand("SELECT @@VERSION", connection))
                    {
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                    }

                    isAlive = true;
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is NotSupportedException || ex is SqlException)
                {
                    Logger?.Invoke(ex.Message);
                }

                if (!isAlive)
                {
                    attempts--;
                    await Task.Delay(DelayTime);
                }
            }
            while (!isAlive && attempts != 0);

            if (attempts == 0)
            {
                throw new TimeoutException("MSSQL didn't start");
            }
        }

        public string GetConnectionString() =>
            $"Data Source={(IsDockerInDocker ? IPAddress : "localhost")}, {(IsDockerInDocker ? 1433 : Ports[1433])}; UID=SA; pwd={_saPassword};";
    }
}

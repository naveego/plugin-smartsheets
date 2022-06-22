using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginSmartSheets.API.Factory;
using PluginSmartSheets.API.Read;
using PluginSmartSheets.API.Utility;
using PluginSmartSheets.DataContracts;
using PluginSmartSheets.Helper;
using Smartsheet.Api;
using Smartsheet.Api.Internal.Http;
using Smartsheet.Api.Models;
using Xunit;
using Record = Naveego.Sdk.Plugins.Record;

namespace PluginHubspotTest.Plugin
{
    public class PluginIntegrationTest
    {
        private Settings GetSettings()
        {
            return new Settings
                {
                    AccessToken = "",
                    IgnoreRowsWithoutKeyValues = true
                };
        }

        private ConnectRequest GetConnectSettings()
        {
            var settings = GetSettings();

            return new ConnectRequest
            {
                SettingsJson = JsonConvert.SerializeObject(settings),
            };
        }

        private async Task<Schema> GetTestSchema(string name = "test", string id = "test")
        {
            SmartsheetClient smartsheet = new SmartsheetBuilder()
                .SetHttpClient(new DefaultHttpClient())
                .SetAccessToken(GetSettings().AccessToken)
                .Build();
             
            IApiClient apiClient = new ApiClient(smartsheet, GetSettings());
            PaginatedResult<Sheet> sheets = await  apiClient.ListSheets();

            var schema = new Schema();
            
            for (var i = 0;
                i < sheets.TotalCount;
                i++)
            {
                if (id == sheets.Data[i].Id.ToString() || name == sheets.Data[i].Name)
                {
                    schema.Id = sheets.Data[i].Id.ToString();
                    schema.Name = sheets.Data[i].Name;
                    return schema;
                }
            }

            //No matching schema found - return first schema
            if (sheets.TotalCount > 0)
            {
                schema.Id = sheets.Data[0].Id.ToString();
                schema.Name = sheets.Data[0].Name;
                return schema;
            }
            else
            {
                return schema;
            }
        }

        [Fact]
        public async Task ConnectSessionTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSmartSheets.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = GetConnectSettings();
            var disconnectRequest = new DisconnectRequest();

            // act
            var response = client.ConnectSession(request);
            var responseStream = response.ResponseStream;
            var records = new List<ConnectResponse>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
                client.Disconnect(disconnectRequest);
            }

            // assert
            Assert.Single(records);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ConnectTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSmartSheets.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = GetConnectSettings();

            // act
            var response = client.Connect(request);

            // assert
            Assert.IsType<ConnectResponse>(response);
            Assert.Equal("", response.SettingsError);
            Assert.Equal("", response.ConnectionError);
            Assert.Equal("", response.OauthError);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasAllTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSmartSheets.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.All,
                SampleSize = 10
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.Equal(3, response.Schemas.Count);
            //
             var schema = response.Schemas[0];
             Assert.Equal($"3326348329543556", schema.Id);
             Assert.Equal("QA 1", schema.Name);
            // Assert.Equal($"", schema.Query);
            // Assert.Equal(10, schema.Sample.Count);
            // Assert.Equal(17, schema.Properties.Count);
            //
             var property = schema.Properties[0];
             Assert.Equal("614533442103172", property.Id);
             Assert.Equal("Primary Column", property.Name);
            // Assert.Equal("", property.Description);
            // Assert.Equal(PropertyType.String, property.Type);
            // Assert.False(property.IsKey);
            // Assert.True(property.IsNullable);
            //
            // var schema2 = response.Schemas[1];
            // Assert.Equal($"Custom Name", schema2.Id);
            // Assert.Equal("Custom Name", schema2.Name);
            // Assert.Equal($"", schema2.Query);
            // Assert.Equal(10, schema2.Sample.Count);
            // Assert.Equal(17, schema2.Properties.Count);
            //
            // var property2 = schema2.Properties[0];
            // Assert.Equal("field1", property2.Id);
            // Assert.Equal("field1", property2.Name);
            // Assert.Equal("", property2.Description);
            // Assert.Equal(PropertyType.String, property2.Type);
            // Assert.False(property2.IsKey);
            // Assert.True(property2.IsNullable);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasRefreshTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSmartSheets.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                SampleSize = 10,
                ToRefresh =
                {
                    await GetTestSchema("Project Launch Plan")
                }
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
             Assert.Equal(1, response.Schemas.Count);
            //
             var schema = response.Schemas[0];
             Assert.Equal("3326348329543556", schema.Id);
            // Assert.Equal("test", schema.Name);
            // Assert.Equal("", schema.Query);
             Assert.Equal(3, schema.Sample.Count);
             Assert.Equal(6, schema.Properties.Count);
            //
             var property = schema.Properties[0];
             Assert.Equal("614533442103172", property.Id);
             Assert.Equal("Primary Column", property.Name);
             Assert.Equal("", property.Description);
             Assert.Equal(PropertyType.String, property.Type);
             Assert.False(property.IsKey);
             Assert.True(property.IsNullable);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
        
        [Fact]
        public async Task DiscoverSchemasRefreshQueryTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSmartSheets.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                SampleSize = 10,
                ToRefresh =
                {
                    new Schema
                    {
                        Id = "Custom Table 1",
                        Query = "QA 1,QA 1 Same"
                    }
                }
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
             Assert.Equal(1, response.Schemas.Count);
            //
             var schema = response.Schemas[0];
             Assert.Equal("Custom Table 1", schema.Id);
            // Assert.Equal("test", schema.Name);
            // Assert.Equal("", schema.Query);
             Assert.Equal(6, schema.Sample.Count);
             Assert.Equal(6, schema.Properties.Count);
            //
             var property = schema.Properties[0];
             Assert.Equal("614533442103172", property.Id);
             Assert.Equal("Primary Column", property.Name);
             Assert.Equal("", property.Description);
             Assert.Equal(PropertyType.String, property.Type);
             Assert.False(property.IsKey);
             Assert.True(property.IsNullable);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ReadStreamTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSmartSheets.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var schema = await GetTestSchema("QA 1");

            var connectRequest = GetConnectSettings();

            var schemaRequest = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                ToRefresh = {schema}
            };
            
            // var schemaRequest = new DiscoverSchemasRequest
            // {
            //     Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
            //     ToRefresh =
            //     {
            //         new Schema
            //         {
            //             Id = "Custom Table 1",
            //             Query = "QA 1,QA 1 Same"
            //         }
            //     }
            // };

            var request = new ReadRequest()
            {
                DataVersions = new DataVersions
                {
                    JobId = "test"
                },
                JobId = "test",
            };

            // act
            client.Connect(connectRequest);
            var schemasResponse = client.DiscoverSchemas(schemaRequest);
            request.Schema = schemasResponse.Schemas[0];

            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Equal(3, records.Count);

            var record = JsonConvert.DeserializeObject<Dictionary<string, object>>(records[0].DataJson);
            Assert.Equal("data1", record["Primary Column"]);
            Assert.Equal("data2", record["Column2"]);
            Assert.Equal("3", record["Column3"]);
            Assert.Equal("col4", record["Column4"]);
            Assert.Equal("5", record["Column5"]);
            Assert.Equal("data6", record["Column6"]);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ReadStreamLimitTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSmartSheets.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var schema = await GetTestSchema();

            var connectRequest = GetConnectSettings();

            var schemaRequest = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                ToRefresh = {schema}
            };

            var request = new ReadRequest()
            {
                DataVersions = new DataVersions
                {
                    JobId = "test"
                },
                JobId = "test",
                Limit = 1
            };

            // act
            client.Connect(connectRequest);
            var schemasResponse = client.DiscoverSchemas(schemaRequest);
            request.Schema = schemasResponse.Schemas[0];

            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Equal(1, records.Count);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
[Fact]
        public async Task ReadStreamTableSchemaTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSmartSheets.Plugin.Plugin())},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var schemaRequest = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.All,
            };
            
            var schemaRequest2 = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                ToRefresh = { }
            };

            var request = new ReadRequest()
            {
                DataVersions = new DataVersions
                {
                    JobId = "test"
                },
                JobId = "test",
            };

            // act
            client.Connect(connectRequest);
            var schemasResponse = client.DiscoverSchemas(schemaRequest);
            schemaRequest2.ToRefresh.Add(schemasResponse.Schemas[0]);
            var schemasResponse2 = client.DiscoverSchemas(schemaRequest2);
            
            request.Schema = schemasResponse2.Schemas[0];

            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Equal(3, records.Count);

            var record = JsonConvert.DeserializeObject<Dictionary<string, object>>(records[0].DataJson);
            Assert.Equal("data1", record["Primary Column"]);
            Assert.Equal("data2", record["Column2"]);
            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
        // [Fact]
        // public async Task ReadStreamRealTimeTest()
        // {
        //     // setup
        //     Server server = new Server
        //     {
        //         Services = {Publisher.BindService(new PluginHubspot.Plugin.Plugin())},
        //         Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
        //     };
        //     server.Start();
        //
        //     var port = server.Ports.First().BoundPort;
        //
        //     var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
        //     var client = new Publisher.PublisherClient(channel);
        //
        //     var schema = await GetTestSchema();
        //
        //     var connectRequest = GetConnectSettings();
        //
        //     var schemaRequest = new DiscoverSchemasRequest
        //     {
        //         Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
        //         ToRefresh = {schema}
        //     };
        //
        //     var request = new ReadRequest()
        //     {
        //         DataVersions = new DataVersions
        //         {
        //             JobId = "test",
        //             JobDataVersion = 1
        //         },
        //         JobId = "test",
        //         RealTimeStateJson = JsonConvert.SerializeObject(new RealTimeState()),
        //         RealTimeSettingsJson = JsonConvert.SerializeObject(new RealTimeSettings()),
        //     };
        //
        //     // act
        //     var records = new List<Record>();
        //     try
        //     {
        //         client.Connect(connectRequest);
        //         var schemasResponse = client.DiscoverSchemas(schemaRequest);
        //         request.Schema = schemasResponse.Schemas[0];
        //
        //         var cancellationToken = new CancellationTokenSource();
        //         cancellationToken.CancelAfter(5000);
        //         var response = client.ReadStream(request, null, null, cancellationToken.Token);
        //         var responseStream = response.ResponseStream;
        //
        //
        //         while (await responseStream.MoveNext())
        //         {
        //             records.Add(responseStream.Current);
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Assert.Equal("Status(StatusCode=Cancelled, Detail=\"Cancelled\")", e.Message);
        //     }
        //
        //
        //     // assert
        //     Assert.Equal(3, records.Count);
        //
        //     var record = JsonConvert.DeserializeObject<Dictionary<string, object>>(records[0].DataJson);
        //     // Assert.Equal("~", record["tilde"]);
        //
        //     // cleanup
        //     await channel.ShutdownAsync();
        //     await server.ShutdownAsync();
        // }

        // [Fact]
        // public async Task WriteTest()
        // {
        //     // setup
        //     Server server = new Server
        //     {
        //         Services = {Publisher.BindService(new PluginSmartSheets.Plugin.Plugin())},
        //         Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
        //     };
        //     server.Start();
        //
        //     var port = server.Ports.First().BoundPort;
        //
        //     var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
        //     var client = new Publisher.PublisherClient(channel);
        //
        //     var schema = GetTestSchema("UpsertCompanies");
        //
        //     var connectRequest = GetConnectSettings();
        //
        //     var schemaRequest = new DiscoverSchemasRequest
        //     {
        //         Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
        //         ToRefresh = {schema}
        //     };
        //
        //     var records = new List<Record>()
        //     {
        //         {
        //             new Record
        //             {
        //                 Action = Record.Types.Action.Upsert,
        //                 CorrelationId = "test",
        //                 RecordId = "record1",
        //                 DataJson = "{\"createdate\":\"2021-05-06T16:55:49.689Z\",\"domain\":\"sample.com\",\"hs_lastmodifieddate\":\"2021-05-06T16:56:10.131Z\",\"hs_object_id\":\"6021949042\",\"name\":\"Updated Sample Company\",\"hs_unique_creation_key\":\"6021949042\"}",
        //             }
        //         },
        //         {
        //             new Record
        //             {
        //                 Action = Record.Types.Action.Upsert,
        //                 CorrelationId = "test",
        //                 RecordId = "record2",
        //                 DataJson = "{\"domain\":\"newsample.com\",\"name\":\"New Sample Company\"}",
        //             }
        //         }
        //     };
        //
        //     var recordAcks = new List<RecordAck>();
        //
        //     // act
        //     client.Connect(connectRequest);
        //
        //     var schemasResponse = client.DiscoverSchemas(schemaRequest);
        //
        //     var prepareWriteRequest = new PrepareWriteRequest()
        //     {
        //         Schema = schemasResponse.Schemas[0],
        //         CommitSlaSeconds = 1000,
        //         DataVersions = new DataVersions
        //         {
        //             JobId = "jobUnitTest",
        //             ShapeId = "shapeUnitTest",
        //             JobDataVersion = 1,
        //             ShapeDataVersion = 1
        //         }
        //     };
        //     client.PrepareWrite(prepareWriteRequest);
        //
        //     using (var call = client.WriteStream())
        //     {
        //         var responseReaderTask = Task.Run(async () =>
        //         {
        //             while (await call.ResponseStream.MoveNext())
        //             {
        //                 var ack = call.ResponseStream.Current;
        //                 recordAcks.Add(ack);
        //             }
        //         });
        //
        //         foreach (Record record in records)
        //         {
        //             await call.RequestStream.WriteAsync(record);
        //         }
        //
        //         await call.RequestStream.CompleteAsync();
        //         await responseReaderTask;
        //     }
        //
        //     // assert
        //     Assert.Equal(2, recordAcks.Count);
        //     Assert.Equal("", recordAcks[0].Error);
        //     Assert.Equal("test", recordAcks[0].CorrelationId);
        //
        //     // cleanup
        //     await channel.ShutdownAsync();
        //     await server.ShutdownAsync();
        // }
    }
}
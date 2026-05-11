using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Moq;

namespace MundialitoCorp.UnitTests.Api
{
    public class ApiTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly HttpClient Client;
        protected readonly Mock<ISender> MediatorMock;

        public ApiTestBase(WebApplicationFactory<Program> factory)
        {
            MediatorMock = new Mock<ISender>();
            Client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(MediatorMock.Object);
                });
            }).CreateClient();
        }
    }
}
using AccountOwnerServer;
using Entities.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Integration
{
    public class IntegrationTests: IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public IntegrationTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAllOwners_ReturnsOkResponse()
        {
            //Arrange 
            var client = this._factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/owner");
            response.EnsureSuccessStatusCode();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAllOwners_ReturnsAListOfOwners()
        {
            //Arrange
            var client = this._factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/owner");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var owners = JsonConvert.DeserializeObject<List<Owner>>(responseString);

            // Assert
            Assert.NotEmpty(owners);
        }
    }
}

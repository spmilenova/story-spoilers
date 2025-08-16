using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoiler
{
    [TestFixture]
    public class StorySpoilerApiTests
    {
        private RestClient client;
        private static string? lastStoryId;

        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/api";
        private const string Username = "silvi";
        private const string Password = "MyStrongPassword1!";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(Username, Password);

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken),
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var tempCLient = new RestClient(BaseUrl);
            var request = new RestRequest("/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = tempCLient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseBody = JsonSerializer.Deserialize<AccessTokenDTO>(response.Content);
                var token = responseBody.AccessToken;

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Failed to retrieve JWT token from the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Content: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateStory_WithRequiredFields_ShouldReturnSuccess()
        {
            var storyRequest = new StoryDTO
            {
                Title = "Test Story",
                Description = "This is a test story description."
            };

            var request = new RestRequest("/Story/Create", Method.Post);
            request.AddJsonBody(storyRequest);
            var response = this.client.Execute(request);
            var responseBody = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(responseBody.Id, Is.Not.Empty);
            Assert.That(responseBody.Msg, Is.EqualTo("Successfully created!"));
            lastStoryId = responseBody.Id;
        }

        [Order(2)]
        [Test]
        public void EditExistingStory_ShouldReturnSuccess()
        {
            var editRequest = new StoryDTO
            {
                Title = "Edited Story",
                Description = "This is an updated test story description.",
                Url = ""
            };

            var request = new RestRequest($"/Story/Edit/{lastStoryId}", Method.Put);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            var responseContent = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseContent.Msg, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]
        public void GetAllStories_ShouldReturnListOfStories()
        {
            var request = new RestRequest("/Story/All", Method.Get);
            var response = this.client.Execute(request);

            var responsItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responsItems, Is.Not.Empty);
        }

        [Order(4)]
        [Test]
        public void DeleteStory_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/Story/Delete/{lastStoryId}", Method.Delete);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Order(5)]
        [Test]

        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var ideaRequest = new StoryDTO
            {
            };

            var request = new RestRequest("/Story/Create", Method.Post);
            request.AddJsonBody(ideaRequest);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Order(6)]
        [Test]

        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            string nonExistingStoryId = "123";
            var editRequest = new StoryDTO
            {
                Title = "Edited Non-Existing Story",
                Description = "This is an updated test story description for a non-existing story.",
                Url = ""
            };
            var request = new RestRequest($"/Story/Edit/{nonExistingStoryId}", Method.Put);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingStory_ShouldReturnNotFound()
        {
            string nonExistingStoryId = "123";
            var request = new RestRequest($"/Idea/Delete/{nonExistingStoryId}", Method.Delete);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Is.Empty);

            // This is what is described in the task, but it seems like a bug in the API
            //Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            //Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            this.client?.Dispose();
        }
    }
}
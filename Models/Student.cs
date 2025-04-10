using Newtonsoft.Json;

namespace FutureTechAcademy.Models
{
    public class Student
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "student-" + Guid.NewGuid();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EnrolmentStatus { get; set; }
        public string ProfileImageUrl { get; set; }
        // Not stored - contains just the blob name
        [JsonIgnore]
        public string ProfileImageBlobName =>
            ProfileImageUrl?.Split('/').LastOrDefault();

    }
}

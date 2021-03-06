// Code generated by Microsoft (R) AutoRest Code Generator 1.0.1.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Microsoft.HealthVault.RestApi.Generated.Models
{
    using Microsoft.HealthVault;
    using Microsoft.HealthVault.RestApi;
    using Microsoft.HealthVault.RestApi.Generated;
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// An instance of a goal recommendation associated to a user
    /// </summary>
    public partial class GoalRecommendationInstance
    {
        /// <summary>
        /// Initializes a new instance of the GoalRecommendationInstance class.
        /// </summary>
        public GoalRecommendationInstance()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the GoalRecommendationInstance class.
        /// </summary>
        /// <param name="id">The identifier of the recommendation.</param>
        /// <param name="organizationId">The ID of the organization that
        /// manages this recommendation</param>
        /// <param name="organizationName">The name of the organization that
        /// manages this recommendation</param>
        /// <param name="acknowledged">Specifies if the user has used the
        /// recommendation to set a goal.</param>
        /// <param name="expirationDate">The expiration date of the
        /// recommendation in Universal Time Zone(UTC).</param>
        /// <param name="associatedGoal">The associated goal data</param>
        public GoalRecommendationInstance(string id = default(string), string organizationId = default(string), string organizationName = default(string), bool? acknowledged = default(bool?), System.DateTime? expirationDate = default(System.DateTime?), Goal associatedGoal = default(Goal))
        {
            Id = id;
            OrganizationId = organizationId;
            OrganizationName = organizationName;
            Acknowledged = acknowledged;
            ExpirationDate = expirationDate;
            AssociatedGoal = associatedGoal;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the identifier of the recommendation.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the organization that manages this
        /// recommendation
        /// </summary>
        [JsonProperty(PropertyName = "organizationId")]
        public string OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the organization that manages this
        /// recommendation
        /// </summary>
        [JsonProperty(PropertyName = "organizationName")]
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets or sets specifies if the user has used the recommendation to
        /// set a goal.
        /// </summary>
        [JsonProperty(PropertyName = "acknowledged")]
        public bool? Acknowledged { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the recommendation in Universal
        /// Time Zone(UTC).
        /// </summary>
        [JsonProperty(PropertyName = "expirationDate")]
        public System.DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the associated goal data
        /// </summary>
        [JsonProperty(PropertyName = "associatedGoal")]
        public Goal AssociatedGoal { get; set; }

    }
}

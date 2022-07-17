using FluentAssertions;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Vonk.Core.Common;
using Vonk.Core.Context;
using Vonk.Core.ElementModel;
using Vonk.Core.Repository;
using Vonk.Fhir.R4;
using Vonk.UnitTests.Framework.Helpers;
using Xunit;
using static Hl7.Fhir.Model.NamingSystem;
using Task = System.Threading.Tasks.Task;

namespace Vonk.Plugin.PreferredIdOperation.Tests
{
    public class PreferredIdOperationTests
    {
        private PreferredIdService _vonkPluginService;
        private ILogger<PreferredIdService> _logger = new Mock<ILogger<PreferredIdService>>().Object;
        private Mock<IAdministrationSearchRepository> _searchMock = new Mock<IAdministrationSearchRepository>();

        #region Setup Values

        //<base-url>/administration/R4/NamingSystem/$preferred-id?id=http://hl7.org/fhir/sid/us-ssn&type=oid
        private readonly string SOCIALSECURITYNUMBER_URI_REQUEST_VALUE = "http://hl7.org/fhir/sid/us-ssn";
        private readonly string SOCIALSECURITYNUMBER_OID_REQUEST_TYPE_VALUE = "Oid";
        private readonly string SOCIALSECURITYNUMBER_URS_REQUEST_TYPE_VALUE = "Urs";
        private readonly NamingSystemIdentifierType SOCIALSECURITYNUMBER_OID_RESPONSE_TYPE = NamingSystemIdentifierType.Oid;
        private readonly string SOCIALSECURITYNUMBER_OID_RESPONSE_VALUE = "2.16.840.1.113883.4.1";

        //<base-url>/administration/R4/NamingSystem/$preferred-id?id=2.16.840.1.113883.4.1&type=uri
        private readonly string SOCIALSECURITYNUMBER_OID_REQUEST_VALUE = "2.16.840.1.113883.4.1";
        private readonly string SOCIALSECURITYNUMBER_URI_REQUEST_TYPE_VALUE = "Uri";
        private readonly NamingSystemIdentifierType SOCIALSECURITYNUMBER_URI_RESPONSE_TYPE = NamingSystemIdentifierType.Uri;
        private readonly string SOCIALSECURITYNUMBER_URI_RESPONSE_VALUE = "http://hl7.org/fhir/sid/us-ssn";

        //<base-url>/administration/R4/NamingSystem/$preferred-id?id=1.3.160&type=uri
        private readonly string GLOBALTRADEITEMNUMBER_OID_REQUEST_VALUE = "1.3.160";
        private readonly string GLOBALTRADEITEMNUMBER_URI_REQUEST_TYPE_VALUE = "Uri";
        private readonly NamingSystemIdentifierType GLOBALTRADEITEMNUMBER_URI_RESPONSE_TYPE = NamingSystemIdentifierType.Uri;
        private readonly string GLOBALTRADEITEMNUMBER_URI_RESPONSE_VALUE = "https://www.gs1.org/gtin";

        //<base-url>/administration/R4/NamingSystem/$preferred-id?id=https://www.gs1.org/gtin&type=oid
        private readonly string GLOBALTRADEITEMNUMBER_URI_REQUEST_VALUE = "https://www.gs1.org/gtin";
        private readonly string GLOBALTRADEITEMNUMBER_OID_REQUEST_TYPE_VALUE = "Oid";
        private readonly NamingSystemIdentifierType GLOBALTRADEITEMNUMBER_OID_RESPONSE_TYPE = NamingSystemIdentifierType.Oid;
        private readonly string GLOBALTRADEITEMNUMBER_OID_RESPONSE_VALUE = "1.3.160";

        #endregion Setup Values

        #region Constructor

        public PreferredIdOperationTests()
        {
            _vonkPluginService = new PreferredIdService(_searchMock.Object, _logger);
        }

        #endregion Constructor

        #region US SocialSecurity Number Tests

        [Fact]
        public async Task PreferredIdOperationGETSocialSecurityNumberWithUriReturnValidOidValue()
        {
            var preferredIdResponse = CreateResponsePreferredIdSearch(SOCIALSECURITYNUMBER_OID_RESPONSE_TYPE, SOCIALSECURITYNUMBER_OID_RESPONSE_VALUE);
            var searchResult = new SearchResult(new List<IResource>() { preferredIdResponse }, 1, 1);
            _searchMock.Setup(repo => repo.Search(It.IsAny<IArgumentCollection>(), It.IsAny<SearchOptions>())).ReturnsAsync(searchResult);
            var context = CreateVonkContextGetWithArguments(SOCIALSECURITYNUMBER_URI_REQUEST_VALUE, SOCIALSECURITYNUMBER_OID_REQUEST_TYPE_VALUE);

            await _vonkPluginService.GetPreferredId(context);

            context.Response.HttpResult.Should().Be(StatusCodes.Status200OK, "$preferred-id should succeed with HTTP 200 - OK");
            context.Response.Payload.Should().NotBeNull("$preferred-id should return a payload");
            context.Response.Payload.ToPoco<Parameters>().Should().NotBeNull("$preferred-id payload should include a parameters object");
            context.Response.Payload.ToPoco<Parameters>().Parameter.Count.Should().Be(1, "$preferred-id payload should have exactly 1 parameter object");
            context.Response.Payload.ToPoco<Parameters>().Parameter[0].Value.ToString().Should().Be(SOCIALSECURITYNUMBER_OID_RESPONSE_VALUE, $"preferred-id should succeed with parameter value: {SOCIALSECURITYNUMBER_OID_RESPONSE_VALUE}");
        }

        [Fact]
        public async Task PreferredIdOperationGETSocialSecurityNumberWithUrsReturn400BadRequest()
        {
            var context = CreateVonkContextGetWithArguments(SOCIALSECURITYNUMBER_URI_REQUEST_VALUE, SOCIALSECURITYNUMBER_URS_REQUEST_TYPE_VALUE);

            await _vonkPluginService.GetPreferredId(context);

            context.Response.HttpResult.Should().Be(StatusCodes.Status400BadRequest, "$preferred-id should fail with HTTP 400 - Bad Request if requested identifier was not recognized");
            context.Response.Outcome.Should().NotBeNull("At least an OperationOutcome should be returned");
        }

        [Fact]
        public async Task PreferredIdOperationPOSTSocialSecurityNumberWithUriReturnValidOidValue()
        {
            var preferredIdResponse = CreateResponsePreferredIdSearch(SOCIALSECURITYNUMBER_OID_RESPONSE_TYPE, SOCIALSECURITYNUMBER_OID_RESPONSE_VALUE);
            var searchResult = new SearchResult(new List<IResource>() { preferredIdResponse }, 1, 1);
            _searchMock.Setup(repo => repo.Search(It.IsAny<IArgumentCollection>(), It.IsAny<SearchOptions>())).ReturnsAsync(searchResult);
            var context = CreateVonkContextPostWithPayload(SOCIALSECURITYNUMBER_URI_REQUEST_VALUE, SOCIALSECURITYNUMBER_OID_REQUEST_TYPE_VALUE);

            await _vonkPluginService.PostPreferredId(context);

            context.Response.HttpResult.Should().Be(StatusCodes.Status200OK, "$preferred-id should succeed with HTTP 200 - OK");
            context.Response.Payload.Should().NotBeNull("$preferred-id should return a payload");
            context.Response.Payload.ToPoco<Parameters>().Should().NotBeNull("$preferred-id payload should include a parameters object");
            context.Response.Payload.ToPoco<Parameters>().Parameter.Count.Should().Be(1, "$preferred-id payload should have exactly 1 parameter object");
            context.Response.Payload.ToPoco<Parameters>().Parameter[0].Value.ToString().Should().Be(SOCIALSECURITYNUMBER_OID_RESPONSE_VALUE, $"preferred-id should succeed with parameter value: {SOCIALSECURITYNUMBER_OID_RESPONSE_VALUE}");
        }


        [Fact]
        public async Task PreferredIdOperationGETSocialSecurityNumberWithOIDReturnValidUriValue()
        {
            var preferredIdResponse = CreateResponsePreferredIdSearch(SOCIALSECURITYNUMBER_URI_RESPONSE_TYPE, SOCIALSECURITYNUMBER_URI_RESPONSE_VALUE);
            var searchResult = new SearchResult(new List<IResource>() { preferredIdResponse }, 1, 1);
            _searchMock.Setup(repo => repo.Search(It.IsAny<IArgumentCollection>(), It.IsAny<SearchOptions>())).ReturnsAsync(searchResult);
            var context = CreateVonkContextGetWithArguments(SOCIALSECURITYNUMBER_OID_REQUEST_VALUE, SOCIALSECURITYNUMBER_URI_REQUEST_TYPE_VALUE);

            await _vonkPluginService.GetPreferredId(context);

            context.Response.HttpResult.Should().Be(StatusCodes.Status200OK, "$preferred-id should succeed with HTTP 200 - OK");
            context.Response.Payload.Should().NotBeNull("$preferred-id should return a payload");
            context.Response.Payload.ToPoco<Parameters>().Should().NotBeNull("$preferred-id payload should include a parameters object");
            context.Response.Payload.ToPoco<Parameters>().Parameter.Count.Should().Be(1, "$preferred-id payload should have exactly 1 parameter object");
            context.Response.Payload.ToPoco<Parameters>().Parameter[0].Value.ToString().Should().Be(SOCIALSECURITYNUMBER_URI_RESPONSE_VALUE, $"preferred-id should succeed with parameter value: {SOCIALSECURITYNUMBER_URI_RESPONSE_VALUE}");
        }

        #endregion US SocialSecurity Number Tests

        #region Global Trade Item Number Tests

        [Fact]
        public async Task PreferredIdOperationGETGlobalTradeItemNumberWithOIDReturnValidUriValue()
        {
            var preferredIdResponse = CreateResponsePreferredIdSearch(GLOBALTRADEITEMNUMBER_URI_RESPONSE_TYPE, GLOBALTRADEITEMNUMBER_URI_RESPONSE_VALUE);
            var searchResult = new SearchResult(new List<IResource>() { preferredIdResponse }, 1, 1);
            _searchMock.Setup(repo => repo.Search(It.IsAny<IArgumentCollection>(), It.IsAny<SearchOptions>())).ReturnsAsync(searchResult);
            var context = CreateVonkContextGetWithArguments(GLOBALTRADEITEMNUMBER_OID_REQUEST_VALUE, GLOBALTRADEITEMNUMBER_URI_REQUEST_TYPE_VALUE);

            await _vonkPluginService.GetPreferredId(context);

            context.Response.HttpResult.Should().Be(StatusCodes.Status200OK, "$preferred-id should succeed with HTTP 200 - OK");
            context.Response.Payload.Should().NotBeNull("$preferred-id should return a payload");
            context.Response.Payload.ToPoco<Parameters>().Should().NotBeNull("$preferred-id payload should include a parameters object");
            context.Response.Payload.ToPoco<Parameters>().Parameter.Count.Should().Be(1, "$preferred-id payload should have exactly 1 parameter object");
            context.Response.Payload.ToPoco<Parameters>().Parameter[0].Value.ToString().Should().Be(GLOBALTRADEITEMNUMBER_URI_RESPONSE_VALUE, $"preferred-id should succeed with parameter value: {GLOBALTRADEITEMNUMBER_URI_RESPONSE_VALUE}");
        }

        [Fact]
        public async Task PreferredIdOperationPOSTGlobalTradeItemNumberWithOIDReturnValidUriValue()
        {
            var preferredIdResponse = CreateResponsePreferredIdSearch(GLOBALTRADEITEMNUMBER_URI_RESPONSE_TYPE, GLOBALTRADEITEMNUMBER_URI_RESPONSE_VALUE);
            var searchResult = new SearchResult(new List<IResource>() { preferredIdResponse }, 1, 1);
            _searchMock.Setup(repo => repo.Search(It.IsAny<IArgumentCollection>(), It.IsAny<SearchOptions>())).ReturnsAsync(searchResult);
            var context = CreateVonkContextPostWithPayload(GLOBALTRADEITEMNUMBER_OID_REQUEST_VALUE, GLOBALTRADEITEMNUMBER_URI_REQUEST_TYPE_VALUE);

            await _vonkPluginService.PostPreferredId(context);

            context.Response.HttpResult.Should().Be(StatusCodes.Status200OK, "$preferred-id should succeed with HTTP 200 - OK");
            context.Response.Payload.Should().NotBeNull("$preferred-id should return a payload");
            context.Response.Payload.ToPoco<Parameters>().Should().NotBeNull("$preferred-id payload should include a parameters object");
            context.Response.Payload.ToPoco<Parameters>().Parameter.Count.Should().Be(1, "$preferred-id payload should have exactly 1 parameter object");
            context.Response.Payload.ToPoco<Parameters>().Parameter[0].Value.ToString().Should().Be(GLOBALTRADEITEMNUMBER_URI_RESPONSE_VALUE, $"preferred-id should succeed with parameter value: {GLOBALTRADEITEMNUMBER_URI_RESPONSE_VALUE}");
        }

        [Fact]
        public async Task PreferredIdOperationGETGlobalTradeItemNumberWithUriReturnValidOidValue()
        {
            var preferredIdResponse = CreateResponsePreferredIdSearch(GLOBALTRADEITEMNUMBER_OID_RESPONSE_TYPE, GLOBALTRADEITEMNUMBER_OID_RESPONSE_VALUE);
            var searchResult = new SearchResult(new List<IResource>() { preferredIdResponse }, 1, 1);
            _searchMock.Setup(repo => repo.Search(It.IsAny<IArgumentCollection>(), It.IsAny<SearchOptions>())).ReturnsAsync(searchResult);
            var context = CreateVonkContextGetWithArguments(GLOBALTRADEITEMNUMBER_URI_REQUEST_VALUE, GLOBALTRADEITEMNUMBER_OID_REQUEST_TYPE_VALUE);

            await _vonkPluginService.GetPreferredId(context);

            context.Response.HttpResult.Should().Be(StatusCodes.Status200OK, "$preferred-id should succeed with HTTP 200 - OK");
            context.Response.Payload.Should().NotBeNull("$preferred-id should return a payload");
            context.Response.Payload.ToPoco<Parameters>().Should().NotBeNull("$preferred-id payload should include a parameters object");
            context.Response.Payload.ToPoco<Parameters>().Parameter.Count.Should().Be(1, "$preferred-id payload should have exactly 1 parameter object");
            context.Response.Payload.ToPoco<Parameters>().Parameter[0].Value.ToString().Should().Be(GLOBALTRADEITEMNUMBER_OID_RESPONSE_VALUE, $"preferred-id should succeed with parameter value: {GLOBALTRADEITEMNUMBER_OID_RESPONSE_VALUE}");
        }

        #endregion Global Trade Item Number Tests

        #region Testing Help Methods

        private VonkTestContext CreateVonkContextPostWithPayload(string id, string type)
        {
            var context = new VonkTestContext(VonkInteraction.type_custom, "Fhir4.0");
            context.TestRequest.CustomOperation = "preferred-id";
            context.TestRequest.Method = "POST";

            var parameters = new Parameters();
            parameters.Add("id", new FhirUri(id));
            parameters.Add("type", new FhirString(type));

            context.TestRequest.Payload = new RequestPayload(true, parameters.ToIResource());

            return context;
        }

        private VonkTestContext CreateVonkContextGetWithArguments(string id, string type)
        {
            var context = new VonkTestContext(VonkInteraction.type_custom, "Fhir4.0");
            context.TestRequest.CustomOperation = "preferred-id";
            context.TestRequest.Method = "GET";

            context.Arguments.AddArguments(new[]
            {
                new Argument(ArgumentSource.Path, "id", id),
                new Argument(ArgumentSource.Path, "type", type)
            });

            return context;
        }

        private IResource CreateResponsePreferredIdSearch(NamingSystemIdentifierType? type, string value)
        {
            var namingSystem = new NamingSystem() { UniqueId = new List<UniqueIdComponent>() };
            namingSystem.UniqueId.Add(new UniqueIdComponent() { Type = type, ValueElement = new FhirString(value) });

            return namingSystem.ToTypedElement().ToIResource(VonkConstants.Model.FhirR4);
        }

        #endregion Testing Help Methods
    }
}
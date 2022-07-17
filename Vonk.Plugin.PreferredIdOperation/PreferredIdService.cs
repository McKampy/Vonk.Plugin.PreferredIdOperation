using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Vonk.Core.Common;
using Vonk.Core.Context;
using Vonk.Core.Repository;
using Vonk.Core.Support;
using static Hl7.Fhir.Model.NamingSystem;
using Task = System.Threading.Tasks.Task;

namespace Vonk.Plugin.PreferredIdOperation
{
    internal class PreferredIdService
    {
        private readonly ILogger<PreferredIdService> _logger;
        private readonly IAdministrationSearchRepository _searchRepository;

        public PreferredIdService(IAdministrationSearchRepository searchRepository, ILogger<PreferredIdService> logger)
        {
            Check.NotNull(searchRepository, nameof(searchRepository));
            Check.NotNull(logger, nameof(logger));
            _logger = logger;
            _searchRepository = searchRepository;
        }

        #region Get & Post Implementation

        /// <summary>
        /// /// Handle GET <base-url>/administration/R4/NamingSystem/$preferred-id?id=xxx&type=yyy
        /// </summary>
        /// <param name="vonkContext">IVonkContext for details of the request and providing the response</param>
        /// <returns></returns>
        public async Task GetPreferredId(IVonkContext vonkContext)
        {
            _logger.LogDebug("VonkPluginService - Begin $PreferredId");
            Check.NotNull(vonkContext, nameof(vonkContext));

            var (validRequest, request_id, request_type) = GetArgumentParameters(vonkContext.Arguments);
            if (validRequest)
            {
                await PreferredId(vonkContext, request_id, request_type);
            }
            else
            {
                SetErrorResponse(vonkContext, StatusCodes.Status400BadRequest, "Missing arguments in the request.");
            }

            vonkContext.Arguments.Handled();
            _logger.LogDebug("VonkPluginService - End $PreferredId");
        }

        /// <summary>
        /// /// Handle POST <base-url>/administration/R4/NamingSystem/$preferred-id
        /// </summary>
        /// <param name="vonkContext">IVonkContext for details of the request and providing the response</param>
        /// <returns></returns>
        public async Task PostPreferredId(IVonkContext vonkContext)
        {
            _logger.LogDebug("VonkPluginService - Begin $PreferredId");
            Check.NotNull(vonkContext, nameof(vonkContext));

            if (vonkContext.Request.GetRequiredPayload(vonkContext.Response, out var payload))
            {
                var (validRequest, request_id, request_type) = GetPayloadParameters(payload);
                if (validRequest)
                {
                    await PreferredId(vonkContext, request_id, request_type);
                }
                else
                {
                    SetErrorResponse(vonkContext, StatusCodes.Status400BadRequest, "Missing arguments in the request.");
                }
            }

            _logger.LogDebug("VonkPluginService - End $PreferredId");
        }

        #endregion Get & Post Implementation

        private async Task PreferredId(IVonkContext vonkContext, string request_id, string request_type)
        {
            var (validType, uniqueIdType) = GetNamingSystemIdentifierType(request_type);
            if (validType)
            {
                var searchResult = await _searchRepository.Search(GetSearchArgumentCollection(request_id), GetSearchOptions(vonkContext));
                if (searchResult.TotalCount > 0)
                {
                    var (validParameters, parameters) = GetParametersResult(uniqueIdType, searchResult);
                    if (validParameters)
                    {
                        SetPayloadResponse(vonkContext, StatusCodes.Status200OK, parameters);
                    }
                    else
                    {
                        SetErrorResponse(vonkContext, StatusCodes.Status404NotFound, "No, or multiple identifiers of the specified type found.");
                    }
                }
                else
                {
                    SetErrorResponse(vonkContext, StatusCodes.Status404NotFound, "Provided identifier was not found.");
                }
            }
            else
            {
                SetErrorResponse(vonkContext, StatusCodes.Status400BadRequest, "Provided identifier was not recognized.");
            }
        }

        private static (bool valid, Parameters parameters) GetParametersResult(NamingSystemIdentifierType uniqueIdType, SearchResult searchResult)
        {
            var namingSystemResource = searchResult.FirstOrDefault().ToPoco<NamingSystem>();
            var targetItems = namingSystemResource.UniqueId.Where(x => x.Type.Value.Equals(uniqueIdType)).ToList();
            var count = targetItems.Count();
            
            if ((count < 1) || (count > 1))
            {
                return (false, null);
            }
            else
            {
                return (true, new Parameters().Add("result", targetItems.First().ValueElement));
            }
        }

        private SearchOptions GetSearchOptions(IVonkContext vonkContext)
        {
            return SearchOptions.Latest(vonkContext.ServerBase, vonkContext.Request.Interaction, vonkContext.InformationModel);
        }

        private IArgumentCollection GetSearchArgumentCollection(string request_id)
        {
            return new ArgumentCollection(new IArgument[]
            {
                new Argument(ArgumentSource.Internal, ArgumentNames.resourceType, "NamingSystem"),
                new Argument(ArgumentSource.Internal, "value", request_id)
            });
        }

        private (bool valid, string request_id, string request_type) GetArgumentParameters(IArgumentCollection arguments)
        {
            var request_id = arguments.GetArgument("id") != null ? arguments.GetArgument("id").ArgumentValue : null;
            var request_type = arguments.GetArgument("type") != null ? arguments.GetArgument("type").ArgumentValue : null;
            var valid = (request_id != null && request_type != null);

            return (valid, request_id, request_type);
        }

        private (bool valid, string request_id, string request_type) GetPayloadParameters(IResource payload)
        {
            var parameters = payload.ToPoco<Parameters>();
            var request_id = parameters.GetSingle("id") != null ? parameters.GetSingle("id").Value.ToString() : null;
            var request_type = parameters.GetSingle("type") != null ? parameters.GetSingle("type").Value.ToString() : null;
            var valid = (request_id != null && request_type != null);

            return (valid, request_id, request_type);
        }

        private (bool valid, NamingSystemIdentifierType uniqueIdType) GetNamingSystemIdentifierType(string type)
        {
            var valid = false;
            var nsitype = NamingSystemIdentifierType.Other;
            var requestedtype = string.Concat(type[0].ToString().ToUpper(), type.Substring(1));

            if (!string.IsNullOrEmpty(type))
            {
                try
                {
                    if (Enum.TryParse<NamingSystemIdentifierType>(requestedtype, out nsitype))
                    {
                        valid = true;
                    }
                }
                catch { }
            }

            return (valid, nsitype);
        }

        private void SetErrorResponse(IVonkContext vonkContext, int status, string description)
        {
            VonkIssue issue = VonkIssue.PROCESSING_ERROR.CloneWithDetails(description);
            vonkContext.Response.HttpResult = status;
            vonkContext.Response.Outcome.AddIssue(issue);
        }

        private void SetPayloadResponse(IVonkContext vonkContext, int status200OK, Parameters parameters)
        {
            vonkContext.Response.HttpResult = StatusCodes.Status200OK;
            vonkContext.Response.Payload = parameters.ToTypedElement().ToIResource(vonkContext.InformationModel);
        }
    }
}
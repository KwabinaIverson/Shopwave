using Shopwave.Modules.Stores.Application.Commands.DocumentVerification;

namespace Shopwave.API.Endpoints.Requests.Store.Requests;

public record SubmitVerificationApiRequest(
    List<DocumentSubmissionItem> Documents
    );
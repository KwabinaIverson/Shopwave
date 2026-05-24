using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;

namespace Shopwave.Modules.Stores.Application.Commands.DocumentVerification;

public record SubmitVerificationBundleCommand(
    Guid StoreId,
    Guid CurrentUserId,
    List<DocumentSubmissionItem> Documents
    ) : ICommand<Result<Guid>>;
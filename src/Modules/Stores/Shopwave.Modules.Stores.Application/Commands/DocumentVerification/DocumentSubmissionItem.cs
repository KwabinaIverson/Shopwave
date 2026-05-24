using Shopwave.Modules.Stores.Domain.Enums;

namespace Shopwave.Modules.Stores.Application.Commands.DocumentVerification;

public record DocumentSubmissionItem(
    DocumentType Type,
    string FileUrl
    );
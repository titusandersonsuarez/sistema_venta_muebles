using NogalApi.Models.Communication;

namespace NogalApi.Services;

public interface ICommunicationService
{
    Task CrearContactoAsync(CreateContactMessageDto dto, CancellationToken cancellationToken = default);
    Task<ChatResponseDto> ResponderChatAsync(ChatRequestDto dto, CancellationToken cancellationToken = default);
}

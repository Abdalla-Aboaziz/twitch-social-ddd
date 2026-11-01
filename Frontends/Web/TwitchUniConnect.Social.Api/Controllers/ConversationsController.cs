using Microsoft.AspNetCore.Mvc;
using TwitchUniConnect.Social.Api.Contracts;
using TwitchUniConnect.Social.Application.Services;
using TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;

namespace TwitchUniConnect.Social.Api.Controllers;

[ApiController]
[Route("api/conversations")]
public sealed class ConversationsController(MessagingService messaging) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ConversationResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ConversationResponse>> Create(
        CreateConversationRequest request, CancellationToken cancellationToken)
    {
        var conversation = await messaging.CreateConversationAsync(
            Guid.NewGuid(), request.CreatorProfileId, request.ParticipantProfileIds, request.Title, cancellationToken);
        var response = ToResponse(conversation);
        return CreatedAtAction(nameof(Get), new
        {
            conversationId = conversation.Id,
            requesterProfileId = request.CreatorProfileId
        }, response);
    }

    [HttpGet("{conversationId:guid}")]
    [ProducesResponseType<ConversationResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ConversationResponse>> Get(
        Guid conversationId, Guid requesterProfileId, CancellationToken cancellationToken)
    {
        var conversation = await messaging.GetConversationAsync(conversationId, requesterProfileId, cancellationToken);
        return Ok(ToResponse(conversation));
    }

    [HttpGet("/api/profiles/{profileId:guid}/conversations")]
    [ProducesResponseType<IReadOnlyList<ConversationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConversationResponse>>> ListInbox(
        Guid profileId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var inbox = await messaging.ListConversationsAsync(profileId, page, pageSize, cancellationToken);
        return Ok(inbox.Select(ToResponse).ToArray());
    }

    [HttpPost("{conversationId:guid}/messages")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<MessageResponse>> SendMessage(
        Guid conversationId, SendMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await messaging.SendMessageAsync(
            conversationId, request.SenderProfileId, request.Content, cancellationToken);
        return Created($"/api/conversations/{conversationId}/messages", ToResponse(message));
    }

    [HttpGet("{conversationId:guid}/messages")]
    [ProducesResponseType<IReadOnlyList<MessageResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>> ListMessages(
        Guid conversationId,
        Guid requesterProfileId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var messages = await messaging.ListMessagesAsync(
            conversationId, requesterProfileId, page, pageSize, cancellationToken);
        return Ok(messages.Select(ToResponse).ToArray());
    }

    [HttpPut("{conversationId:guid}/messages/{messageId:guid}")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponse>> EditMessage(
        Guid conversationId, Guid messageId, EditMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await messaging.EditMessageAsync(
            conversationId, messageId, request.ActorProfileId, request.Content, cancellationToken);
        return Ok(ToResponse(message));
    }

    [HttpDelete("{conversationId:guid}/messages/{messageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMessage(
        Guid conversationId, Guid messageId, Guid actorProfileId, CancellationToken cancellationToken)
    {
        await messaging.DeleteMessageAsync(conversationId, messageId, actorProfileId, cancellationToken);
        return NoContent();
    }

    private static ConversationResponse ToResponse(Conversation conversation) =>
        new(conversation.Id, conversation.CreatorProfileId, conversation.Title, conversation.IsGroup,
            conversation.CreatedAtUtc, conversation.LastActivityAtUtc, conversation.ParticipantProfileIds.ToArray());

    private static MessageResponse ToResponse(Message message) =>
        new(message.Id, message.ConversationId, message.SenderProfileId,
            message.IsDeleted ? null : message.Content, message.IsDeleted,
            message.CreatedAtUtc, message.LastModifiedAtUtc);
}

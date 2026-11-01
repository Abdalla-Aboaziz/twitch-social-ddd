using Microsoft.AspNetCore.Mvc;
using TwitchUniConnect.Social.Api.Contracts;
using TwitchUniConnect.Social.Application.Services;
using TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;

namespace TwitchUniConnect.Social.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public sealed class ProfilesController(ProfileService profiles, FriendshipService friendships) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProfileResponse>> Create(
        CreateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await profiles.CreateAsync(
            Guid.NewGuid(), request.UserId, request.FirstName, request.LastName,
            request.EmailName, request.EmailDomain, request.PhoneNumber, cancellationToken);
        var response = ToResponse(profile);
        return CreatedAtAction(nameof(Get), new { id = profile.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProfileResponse>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(ToResponse(await profiles.GetAsync(id, cancellationToken)));

    [HttpPost("{senderId:guid}/friend-requests/{recipientId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SendFriendRequest(Guid senderId, Guid recipientId, CancellationToken cancellationToken)
    {
        await friendships.SendRequestAsync(senderId, recipientId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{recipientId:guid}/friend-requests/{senderId:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AcceptFriendRequest(Guid recipientId, Guid senderId, CancellationToken cancellationToken)
    {
        await friendships.RespondToRequestAsync(recipientId, senderId, true, cancellationToken);
        return NoContent();
    }

    [HttpPost("{recipientId:guid}/friend-requests/{senderId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RejectFriendRequest(Guid recipientId, Guid senderId, CancellationToken cancellationToken)
    {
        await friendships.RespondToRequestAsync(recipientId, senderId, false, cancellationToken);
        return NoContent();
    }

    private static ProfileResponse ToResponse(Profile profile) =>
        new(profile.Id, profile.UserId, profile.Name.FirstName, profile.Name.LastName,
            profile.Email.ToString(), profile.PhoneNumber, profile.Friends);
}

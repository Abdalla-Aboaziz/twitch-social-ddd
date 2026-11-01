using Microsoft.AspNetCore.Mvc;
using TwitchUniConnect.Social.Api.Contracts;
using TwitchUniConnect.Social.Application.Services;
using TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

namespace TwitchUniConnect.Social.Api.Controllers;

[ApiController]
[Route("api/posts")]
public sealed class PostsController(
    PostService posts,
    CommentService comments,
    InteractionService interactions) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<PostResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<PostResponse>> Create(CreatePostRequest request, CancellationToken cancellationToken)
    {
        var post = await posts.CreateAsync(Guid.NewGuid(), request.AuthorProfileId, request.Text, cancellationToken);
        return Created($"/api/posts/{post.Id}", ToResponse(post));
    }

    [HttpGet("{postId:guid}")]
    [ProducesResponseType<PostResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PostResponse>> Get(Guid postId, CancellationToken cancellationToken) =>
        Ok(ToResponse(await posts.GetAsync(postId, cancellationToken)));

    [HttpPut("{postId:guid}/text")]
    [ProducesResponseType<PostResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PostResponse>> EditText(
        Guid postId, EditPostTextRequest request, CancellationToken cancellationToken)
    {
        var post = await posts.EditTextAsync(postId, request.AuthorProfileId, request.Text, cancellationToken);
        return Ok(ToResponse(post));
    }

    [HttpPost("{postId:guid}/comments")]
    [ProducesResponseType<CommentResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CommentResponse>> AddComment(
        Guid postId, CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var comment = await comments.AddAsync(postId, request.AuthorProfileId, request.Message, cancellationToken);
        return Created($"/api/posts/{postId}/comments/{comment.Id}", ToResponse(comment));
    }

    [HttpPost("{postId:guid}/comments/{parentCommentId:guid}/replies")]
    [ProducesResponseType<CommentResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CommentResponse>> Reply(
        Guid postId, Guid parentCommentId, CreateReplyRequest request, CancellationToken cancellationToken)
    {
        var reply = await comments.ReplyAsync(postId, parentCommentId, request.AuthorProfileId, request.Message, cancellationToken);
        return Created($"/api/posts/{postId}/comments/{reply.Id}", ToResponse(reply));
    }

    [HttpPost("{postId:guid}/interactions")]
    [ProducesResponseType<PostResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PostResponse>> AddInteraction(
        Guid postId, CreateInteractionRequest request, CancellationToken cancellationToken)
    {
        var interaction = await interactions.AddAsync(
            postId, request.AuthorProfileId, ParseInteractionType(request.Type), cancellationToken);
        return Ok(ToResponse(interaction));
    }

    [HttpDelete("{postId:guid}/interactions/{authorProfileId:guid}/{type}")]
    [ProducesResponseType<PostResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PostResponse>> RemoveInteraction(
        Guid postId, Guid authorProfileId, string type, CancellationToken cancellationToken)
    {
        var post = await interactions.RemoveAsync(
            postId, authorProfileId, ParseInteractionType(type), cancellationToken);
        return Ok(ToResponse(post));
    }

    private static InteractionType ParseInteractionType(string type) => type.ToLowerInvariant() switch
    {
        "like" => InteractionType.Like,
        "dislike" => InteractionType.Dislike,
        "love" => InteractionType.Love,
        "haha" => InteractionType.Haha,
        "wow" => InteractionType.Wow,
        "sad" => InteractionType.Sad,
        "angry" => InteractionType.Angry,
        _ => throw new ArgumentException($"Unsupported interaction type '{type}'.", nameof(type))
    };

    private static PostResponse ToResponse(Post post) =>
        new(post.Id, post.AuthorProfileId, post.Type.Description, post.Text, post.DateCreated,
            post.LastModified, post.MediaUrl.ToArray(), post.NumberOfComments, post.NumberOfInteractions);

    private static CommentResponse ToResponse(Comment comment) =>
        new(comment.Id, comment.AuthorProfileId, comment.Message, comment.DateCreated, comment.Replies.ToArray());
}

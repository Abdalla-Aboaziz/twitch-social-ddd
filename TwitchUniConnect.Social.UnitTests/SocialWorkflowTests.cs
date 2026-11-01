using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Application.Services;
using TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;
using TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;
using Xunit;

namespace TwitchUniConnect.Social.UnitTests;

public sealed class SocialWorkflowTests
{
    [Fact]
    public async Task Social_workflows_enforce_domain_rules_and_persist_changes_through_unit_of_work()
    {
        var profiles = new FakeProfileRepository();
        var posts = new FakePostRepository();
        var unitOfWork = new FakeUnitOfWork();
        var profileService = new ProfileService(profiles, unitOfWork);

        var sender = await profileService.CreateAsync(
            Guid.NewGuid(), Guid.NewGuid(), "Ava", "Stone", "ava", "example.com", "+15550000001");
        var recipient = await profileService.CreateAsync(
            Guid.NewGuid(), Guid.NewGuid(), "Noah", "Reed", "noah", "example.com", "+15550000002");

        var postService = new PostService(posts, profiles, unitOfWork);
        var post = await postService.CreateAsync(Guid.NewGuid(), sender.Id, "First post");
        await postService.EditTextAsync(post.Id, sender.Id, "Edited post");

        var commentService = new CommentService(postService, unitOfWork);
        var comment = await commentService.AddAsync(post.Id, recipient.Id, "Nice post!");
        var reply = await commentService.ReplyAsync(post.Id, comment.Id, sender.Id, "Thanks!");

        var interactionService = new InteractionService(postService, unitOfWork);
        await interactionService.AddAsync(post.Id, recipient.Id, InteractionType.Love);

        var friendshipService = new FriendshipService(profiles, unitOfWork);
        await friendshipService.SendRequestAsync(sender.Id, recipient.Id);
        await friendshipService.RespondToRequestAsync(recipient.Id, sender.Id, accept: true);

        Assert.Equal("Edited post", post.Text);
        Assert.Equal(2, post.NumberOfComments);
        Assert.Contains(reply.Id, comment.Replies);
        Assert.Single(post.Interactions);
        Assert.Equal(InteractionType.Love, post.Interactions.Single().Type);
        Assert.Contains(recipient.Id, sender.Friends);
        Assert.Contains(sender.Id, recipient.Friends);
        Assert.Empty(recipient.PendingFriendRequest);
        Assert.NotEmpty(post.DomainEvents);
        Assert.True(unitOfWork.SaveCount >= 8);
    }

    [Fact]
    public void Post_rejects_empty_text_and_profile_rejects_self_friend_request()
    {
        var authorId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => Post.Create(Guid.NewGuid(), PostType.Text, " ", authorId));

        var profile = Profile.Create(
            Guid.NewGuid(),
            TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate.Name.Create("Mina", "Park"),
            Guid.NewGuid(),
            TwitchUniConnect.SharedKernel.Types.EmailAddress.Create("mina", "example.com"),
            "+15550000003");

        Assert.Throws<InvalidOperationException>(() => profile.SendFriendRequest(profile));
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        private readonly Dictionary<Guid, Profile> _profiles = [];

        public Task<Profile?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_profiles.GetValueOrDefault(id));

        public void Add(Profile profile) => _profiles.Add(profile.Id, profile);
    }

    private sealed class FakePostRepository : IPostRepository
    {
        private readonly Dictionary<Guid, Post> _posts = [];

        public Task<Post?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_posts.GetValueOrDefault(id));

        public void Add(Post post) => _posts.Add(post.Id, post);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }
}

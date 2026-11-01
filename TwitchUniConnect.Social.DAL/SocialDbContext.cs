using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TwitchUniConnect.SharedKernel.Types;
using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;
using TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;
using TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;

namespace TwitchUniConnect.Social.DAL;

public sealed class SocialDbContext(
    DbContextOptions<SocialDbContext> options,
    IDomainEventDispatcher domainEventDispatcher) : DbContext(options), IUnitOfWork
{
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureProfiles(modelBuilder);
        ConfigurePosts(modelBuilder);
        ConfigureMessaging(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = ChangeTracker.Entries<AggregateRoot<Guid>>()
            .Select(entry => entry.Entity)
            .ToArray();
        var events = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);
        if (events.Length > 0)
        {
            await domainEventDispatcher.DispatchAsync(events, cancellationToken);
            foreach (var aggregate in aggregates)
                aggregate.ClearDomainEvents();
        }

        return result;
    }

    private static void ConfigureProfiles(ModelBuilder modelBuilder)
    {
        var profile = modelBuilder.Entity<Profile>();
        profile.ToTable("Profiles");
        profile.HasKey(entity => entity.Id);
        profile.Ignore(entity => entity.DomainEvents);
        profile.Ignore(entity => entity.Friends);
        profile.Ignore(entity => entity.PendingFriendRequest);
        profile.Property(entity => entity.Id).ValueGeneratedNever();
        profile.Property(entity => entity.UserId).IsRequired();
        profile.Property(entity => entity.PhoneNumber).HasMaxLength(40).IsRequired();

        profile.OwnsOne(entity => entity.Name, name =>
        {
            name.Property(value => value.FirstName).HasMaxLength(100).IsRequired();
            name.Property(value => value.LastName).HasMaxLength(100).IsRequired();
            name.Property(value => value.Initials).HasMaxLength(4).IsRequired();
        });
        profile.Navigation(entity => entity.Name).IsRequired();

        profile.OwnsOne(entity => entity.Email, email =>
        {
            email.Property(value => value.EmailName).HasMaxLength(256).IsRequired();
            email.Property(value => value.Domain).HasMaxLength(256).IsRequired();
        });
        profile.Navigation(entity => entity.Email).IsRequired();

        var friends = profile.Property<List<Guid>>("_friends")
            .HasColumnName("FriendsJson")
            .HasConversion(
                values => JsonSerializer.Serialize(values, (JsonSerializerOptions?)null),
                json => DeserializeGuidList(json));
        friends.Metadata.SetValueComparer(new ValueComparer<List<Guid>>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            values => values.Aggregate(0, (hash, value) => HashCode.Combine(hash, value.GetHashCode())),
            values => values.ToList()));

        profile.OwnsMany<FriendRequest>("_pendingFriendRequests", request =>
        {
            request.ToTable("PendingFriendRequests");
            request.WithOwner().HasForeignKey("ProfileId");
            request.HasKey("ProfileId", nameof(FriendRequest.From), nameof(FriendRequest.To));
            request.Property(value => value.From).IsRequired();
            request.Property(value => value.To).IsRequired();
            request.Property(value => value.DateSent).IsRequired();
        });
        profile.Navigation("_pendingFriendRequests").UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigurePosts(ModelBuilder modelBuilder)
    {
        var post = modelBuilder.Entity<Post>();
        post.ToTable("Posts");
        post.HasKey(entity => entity.Id);
        post.Ignore(entity => entity.DomainEvents);
        post.Ignore(entity => entity.MediaUrl);
        post.Ignore(entity => entity.Interactions);
        post.Property(entity => entity.Id).ValueGeneratedNever();
        post.Property(entity => entity.Type)
            .HasConversion(
                type => type.Value,
                value => GetPostType(value));
        post.Property(entity => entity.Text).HasMaxLength(5000).IsRequired();
        post.Property(entity => entity.AuthorProfileId).IsRequired();
        post.Property(entity => entity.DateCreated).IsRequired();
        post.Property(entity => entity.LastModified).IsRequired();

        var mediaUrls = post.Property<List<Uri>>("_mediaUrls")
            .HasColumnName("MediaUrlsJson")
            .HasConversion(
                values => JsonSerializer.Serialize(values, (JsonSerializerOptions?)null),
                json => DeserializeUriList(json));
        mediaUrls.Metadata.SetValueComparer(new ValueComparer<List<Uri>>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            values => values.Aggregate(0, (hash, value) => HashCode.Combine(hash, value.GetHashCode())),
            values => values.ToList()));

        post.HasMany(entity => entity.Comments)
            .WithOne()
            .HasForeignKey("PostId")
            .OnDelete(DeleteBehavior.Cascade);
        post.Navigation(entity => entity.Comments).HasField("_comments").UsePropertyAccessMode(PropertyAccessMode.Field);

        post.OwnsMany<Interaction>("_interactions", interaction =>
        {
            interaction.ToTable("PostInteractions");
            interaction.WithOwner().HasForeignKey("PostId");
            interaction.HasKey("PostId", nameof(Interaction.AuthorId));
            interaction.Property(value => value.AuthorId).IsRequired();
            interaction.Property(value => value.Type)
                .HasConversion(
                    type => type.Value,
                    value => GetInteractionType(value));
        });
        post.Navigation("_interactions").UsePropertyAccessMode(PropertyAccessMode.Field);

        var comment = modelBuilder.Entity<Comment>();
        comment.ToTable("Comments");
        comment.HasKey(entity => entity.Id);
        comment.Ignore(entity => entity.Replies);
        comment.Property(entity => entity.Id).ValueGeneratedNever();
        comment.Property(entity => entity.AuthorProfileId).IsRequired();
        comment.Property(entity => entity.Message).HasMaxLength(2000).IsRequired();
        comment.Property(entity => entity.DateCreated).IsRequired();
        comment.Property(entity => entity.LastModified).IsRequired();
        var replies = comment.Property<List<Guid>>("_replies")
            .HasColumnName("RepliesJson")
            .HasConversion(
                values => JsonSerializer.Serialize(values, (JsonSerializerOptions?)null),
                json => DeserializeGuidList(json));
        replies.Metadata.SetValueComparer(new ValueComparer<List<Guid>>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            values => values.Aggregate(0, (hash, value) => HashCode.Combine(hash, value.GetHashCode())),
            values => values.ToList()));
    }

    private static void ConfigureMessaging(ModelBuilder modelBuilder)
    {
        var conversation = modelBuilder.Entity<Conversation>();
        conversation.ToTable("Conversations");
        conversation.HasKey(entity => entity.Id);
        conversation.Ignore(entity => entity.DomainEvents);
        conversation.Ignore(entity => entity.ParticipantProfileIds);
        conversation.Property(entity => entity.Id).ValueGeneratedNever();
        conversation.Property(entity => entity.CreatorProfileId).IsRequired();
        conversation.Property(entity => entity.Title).HasMaxLength(120);
        conversation.Property(entity => entity.CreatedAtUtc).IsRequired();
        conversation.Property(entity => entity.LastActivityAtUtc).IsRequired();

        conversation.OwnsMany(entity => entity.Participants, participant =>
        {
            participant.ToTable("ConversationParticipants");
            participant.WithOwner().HasForeignKey("ConversationId");
            participant.HasKey("ConversationId", nameof(ConversationParticipant.ProfileId));
            participant.Property(value => value.ProfileId).ValueGeneratedNever().IsRequired();
            participant.Property(value => value.JoinedAtUtc).IsRequired();
        });
        conversation.Navigation(entity => entity.Participants)
            .HasField("_participants")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        var message = modelBuilder.Entity<Message>();
        message.ToTable("Messages");
        message.HasKey(entity => entity.Id);
        message.Ignore(entity => entity.DomainEvents);
        message.Property(entity => entity.Id).ValueGeneratedNever();
        message.Property(entity => entity.ConversationId).IsRequired();
        message.Property(entity => entity.SenderProfileId).IsRequired();
        message.Property(entity => entity.Content).HasMaxLength(Message.MaximumContentLength).IsRequired();
        message.Property(entity => entity.CreatedAtUtc).IsRequired();
        message.Property(entity => entity.LastModifiedAtUtc).IsRequired();
        message.Property(entity => entity.IsDeleted).IsRequired();
        message.HasIndex(entity => new { entity.ConversationId, entity.CreatedAtUtc, entity.Id });
    }

    private static List<Guid> DeserializeGuidList(string json) =>
        JsonSerializer.Deserialize<List<Guid>>(json, (JsonSerializerOptions?)null) ?? new List<Guid>();

    private static List<Uri> DeserializeUriList(string json) =>
        JsonSerializer.Deserialize<List<Uri>>(json, (JsonSerializerOptions?)null) ?? new List<Uri>();

    private static PostType GetPostType(int value) => value switch
    {
        0 => PostType.Text,
        1 => PostType.Video,
        2 => PostType.Image,
        _ => throw new InvalidOperationException($"Unknown post type value '{value}'.")
    };

    private static InteractionType GetInteractionType(int value) => value switch
    {
        0 => InteractionType.Like,
        1 => InteractionType.Dislike,
        2 => InteractionType.Love,
        3 => InteractionType.Haha,
        4 => InteractionType.Wow,
        5 => InteractionType.Sad,
        6 => InteractionType.Angry,
        _ => throw new InvalidOperationException($"Unknown interaction type value '{value}'.")
    };
}

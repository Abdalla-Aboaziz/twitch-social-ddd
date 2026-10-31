using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

public class PostType : Enumeration
{
    public static readonly PostType Text = new PostType("Text", 0);
    public static readonly PostType Video = new PostType("Video", 1);
    public static readonly PostType Image = new PostType("Image", 2);

    private PostType(string description, int value) : base(description, value) { }



}

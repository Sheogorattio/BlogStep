using Blog.Models;

namespace Blog.Interfaces
{
    public interface ISubscriber
    {
        Task Subscribe(Subscriber subscriber);
        Task<bool> IsSubscribed(string email);
    }
}

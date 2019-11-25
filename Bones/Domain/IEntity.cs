
namespace Bones.Repository.Interfaces
{
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
using Bones.Repository.Interfaces;

namespace Bones.Domain
{
    public class FakeEntity<T> : IEntity<T>
    {
        public FakeEntity(T id)
        {
            Id = id;
        }
        
        public T Id { get; set; }

        public bool Disabled { get; set; }
    }
}
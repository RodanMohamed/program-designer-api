namespace ProgramDesigner.Domain
{
    public abstract class Entity
    {
        public int Id { get; protected set; }
        internal void AssignId(int id) => Id = id;


        public override bool Equals(object? obj)
        {
            if (obj is not Entity other) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            // Transient entities (Id == 0, not yet persisted) are never equal to each other.
            if (Id == 0 || other.Id == 0) return false;

            return Id == other.Id;
        }

        public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();
    }
}

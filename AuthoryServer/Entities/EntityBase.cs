namespace AuthoryServer.Entities
{
    public abstract class EntityBase
    {
        public AuthoryServer Server { get; protected set; }

        /// <summary>
        /// Contains the GridCell that is containing it.
        /// </summary>
        public GridCell GridCell { get; protected set; }

        /// <summary>
        /// The entity's logic will work around this variable.
        /// </summary>
        public long EntityTick { get; protected set; }

        /// <summary>
        /// The current position of the entity.
        /// </summary>
        public Vector3 Position { get; protected set; }

        /// <summary>
        /// The server will be able to lookup the Entity by this variable.
        /// </summary>
        public ushort Id { get; protected set; }

        public string Name { get; protected set; }
        public ModelType ModelType { get; protected set; }


        public void SetId(ushort id) => Id = id;

        public abstract void Tick();
        public abstract void SetGridCell(GridCell gridCell);
        public abstract void Interact(PlayerEntity player);
    }
}

namespace AuthoryServer.Entities.EntityDerived
{
    public class
        TeleportEntity : EntityBase
    {

        public float TeleportSize { get; set; }
        public int TeleportToMapIndex { get; set; }

        public float SqrTeleportSize => TeleportSize * TeleportSize;


        public TeleportEntity(AuthoryServer server, int teleportToMapIndex, Vector3 position, float teleportSize)
        {
            this.Name = string.Format($"Teleport Map{TeleportToMapIndex}");
            this.Server = server;

            this.TeleportToMapIndex = teleportToMapIndex;
            this.Position = position;
            this.TeleportSize = teleportSize;
            this.ModelType = ModelType.TeleportResource;
        }

        public override void Tick()
        {
        }

        public override void SetGridCell(GridCell gridCell)
        {
            GridCell = gridCell;
        }

        public override void Interact(PlayerEntity player)
        {
            if (Vector3.SqrDistance(player.Position, Position) < SqrTeleportSize)
            {
                AuthoryMaster.Instance.ChangeMapServer(player, Server, TeleportToMapIndex);
            }
        }
    }
}

namespace FileManage.DBModels
{
    public class UpdateEntity<TEntity> where TEntity : FileManage.DBModels.EntityBase
    {
        public TEntity entity { get; set; }

        public string[] properties { get; set; }
    }
}
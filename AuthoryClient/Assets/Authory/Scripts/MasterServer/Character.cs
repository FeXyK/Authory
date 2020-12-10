public class Character
{
    public int Id { get; set; }
    public string Name { get; set; }
    public byte Level { get; set; }
    public byte ModelType { get; set; }


    public Character() { }

    public Character(string name, byte level, byte modelType, int id)
    {
        this.Id = id;
        this.Name = name;
        this.Level = level;
        this.ModelType = modelType;
    }
}
namespace MusicCollection.MusicManager
{
    public class User
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }

        public User(string id, string name, string image)
        {
            ID = id;
            Name = name;
            Image = image;
        }
    }
}

using System.Text.Json;

namespace sharp
{
    class Category
    {
        public int cid { get; set; }
        public string name { get; set; }

        public Category() {}

        public Category(int cid, string name)
        {
            this.cid = cid;
            this.name = name;
        }

        public string AsJson() {
            return JsonSerializer.Serialize<Category>(this);
        }

        public static Category fromJson(string json)
        {
            return JsonSerializer.Deserialize<Category>(json);
        }
    }
}
namespace PRM_BE.Model
{
    public class Flower
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public String Category { get; set; }
    }
}

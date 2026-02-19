namespace Viralis.Data.Models
{
    public class SchoolAdministrator
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;

        // The SchoolAdmin who owns this school
        public Guid SchoolAdminId { get; set; }
        public ApplicationUser SchoolAdmin { get; set; } = null!;

        // Teachers that belong to this school
        public ICollection<ApplicationUser> Teachers { get; set; }
            = new List<ApplicationUser>();
    }
}

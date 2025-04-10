using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using cinema_app_back.Data;

namespace cinema_app_back.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20250408121532_AddShowtimeValidationTrigger")]
    partial class AddShowtimeValidationTrigger
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);
        }
    }
} 
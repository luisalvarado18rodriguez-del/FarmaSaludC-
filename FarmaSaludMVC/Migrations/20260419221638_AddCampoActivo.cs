using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmaSaludMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddCampoActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Reservas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Reservas");
        }
    }
}

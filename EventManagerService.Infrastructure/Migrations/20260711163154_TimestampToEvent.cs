using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagerService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TimestampToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "Events",
                type: "bytea",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Events");
        }
    }
}

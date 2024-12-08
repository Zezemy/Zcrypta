using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zcrypta.Migrations
{
    /// <inheritdoc />
    public partial class AddSignalStrategyProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPredefined",
                table: "SignalStrategies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Properties",
                table: "SignalStrategies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPredefined",
                table: "SignalStrategies");

            migrationBuilder.DropColumn(
                name: "Properties",
                table: "SignalStrategies");
        }
    }
}

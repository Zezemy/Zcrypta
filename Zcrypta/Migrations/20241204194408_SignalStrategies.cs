using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zcrypta.Migrations
{
    /// <inheritdoc />
    public partial class SignalStrategies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignalStrategies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategyType = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SignalSt__3214EC0711B64996", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradingPairs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Base = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Quote = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TradingP__3214EC076E5471E1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSignalStrategies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StrategyId = table.Column<long>(type: "bigint", nullable: false),
                    TradingPairId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserSign__3214EC07929FD2F1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSignalStrategies_SignalStrategies",
                        column: x => x.StrategyId,
                        principalTable: "SignalStrategies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserSignalStrategies_TradingPairs",
                        column: x => x.TradingPairId,
                        principalTable: "TradingPairs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSignalStrategies_StrategyId",
                table: "UserSignalStrategies",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSignalStrategies_TradingPairId",
                table: "UserSignalStrategies",
                column: "TradingPairId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSignalStrategies");

            migrationBuilder.DropTable(
                name: "SignalStrategies");

            migrationBuilder.DropTable(
                name: "TradingPairs");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SegundaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenBlacklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "token_blacklist",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    jti = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_token_blacklist", x => x.id);
                    table.ForeignKey(
                        name: "FK_token_blacklist_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_token_blacklist_user_id",
                table: "token_blacklist",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "token_blacklist");
        }
    }
}

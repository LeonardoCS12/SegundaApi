using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SegundaApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "passwordresettoken",
                columns: table => new
                {
                    Token = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwordresettoken", x => x.Token);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    last_login = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_superuser = table.Column<bool>(type: "boolean", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_joined = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    username = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    profile_picture = table.Column<string>(type: "text", nullable: true),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    nationality = table.Column<string>(type: "text", nullable: true),
                    occupation = table.Column<string>(type: "text", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    contact_phone_number = table.Column<string>(type: "text", nullable: true),
                    gender = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    address_number = table.Column<string>(type: "text", nullable: true),
                    address_interior_number = table.Column<string>(type: "text", nullable: true),
                    address_complement = table.Column<string>(type: "text", nullable: true),
                    address_neighborhood = table.Column<string>(type: "text", nullable: true),
                    address_zip_code = table.Column<string>(type: "text", nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<string>(type: "text", nullable: true),
                    password_reset_token = table.Column<string>(type: "text", nullable: true),
                    password_reset_token_expiration = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "passwordresettoken");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

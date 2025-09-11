using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pg.Explorer.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:QueryStatus", "Success,Error")
                .Annotation("Npgsql:Enum:QueryType", "Read,Write,Delete,Update");

            migrationBuilder.CreateTable(
                name: "connections",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    host = table.Column<string>(type: "text", nullable: false),
                    port = table.Column<int>(type: "integer", nullable: false),
                    database_name = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    encrypted_password = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "queries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<long>(type: "bigint", nullable: false),
                    query_body = table.Column<string>(type: "text", nullable: false),
                    query_type = table.Column<string>(type: "text", nullable: false),
                    executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    execution_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    query_status = table.Column<string>(type: "text", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    affected_rows = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_queries", x => x.id);
                    table.ForeignKey(
                        name: "fk_queries_connections_connection_id",
                        column: x => x.connection_id,
                        principalTable: "connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_queries_connection_id",
                table: "queries",
                column: "connection_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "queries");

            migrationBuilder.DropTable(
                name: "connections");
        }
    }
}

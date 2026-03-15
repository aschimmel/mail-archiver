using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailArchiver.Migrations
{
    /// <inheritdoc />
    public partial class MigrateV2603_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "M365AuthMode",
                schema: "mail_archiver",
                table: "MailAccounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OAuthAccessToken",
                schema: "mail_archiver",
                table: "MailAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OAuthAccessTokenExpiresAtUtc",
                schema: "mail_archiver",
                table: "MailAccounts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthRefreshToken",
                schema: "mail_archiver",
                table: "MailAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthTokenScope",
                schema: "mail_archiver",
                table: "MailAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthTokenType",
                schema: "mail_archiver",
                table: "MailAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OAuthConnectedAtUtc",
                schema: "mail_archiver",
                table: "MailAccounts",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "M365AuthMode",
                schema: "mail_archiver",
                table: "MailAccounts");

            migrationBuilder.DropColumn(
                name: "OAuthAccessToken",
                schema: "mail_archiver",
                table: "MailAccounts");

            migrationBuilder.DropColumn(
                name: "OAuthAccessTokenExpiresAtUtc",
                schema: "mail_archiver",
                table: "MailAccounts");

            migrationBuilder.DropColumn(
                name: "OAuthRefreshToken",
                schema: "mail_archiver",
                table: "MailAccounts");

            migrationBuilder.DropColumn(
                name: "OAuthTokenScope",
                schema: "mail_archiver",
                table: "MailAccounts");

            migrationBuilder.DropColumn(
                name: "OAuthTokenType",
                schema: "mail_archiver",
                table: "MailAccounts");

            migrationBuilder.DropColumn(
                name: "OAuthConnectedAtUtc",
                schema: "mail_archiver",
                table: "MailAccounts");
        }
    }
}

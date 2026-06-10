using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoodPlaylistApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Moods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: true),
                    Emoji = table.Column<string>(type: "text", nullable: true),
                    SeedGenres = table.Column<string>(type: "jsonb", nullable: false),
                    AudioFeatures = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    MoodId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tracks = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Moods_MoodId",
                        column: x => x.MoodId,
                        principalTable: "Moods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Moods_Name",
                table: "Moods",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_MoodId",
                table: "Playlists",
                column: "MoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_Title_UserId",
                table: "Playlists",
                columns: new[] { "Title", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserId",
                table: "Playlists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PublicId",
                table: "Users",
                column: "PublicId",
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO ""Moods"" (""Id"", ""AudioFeatures"", ""Color"", ""CreatedAt"", ""Emoji"", ""Name"", ""SeedGenres"", ""UpdatedAt"")
                VALUES
                ('019eacde-9f88-70d6-a414-d0ceefa144e4', '{""energy"": {""min"": 0.9}, ""valence"": {""max"": 0.4}, ""tempo"": {""min"": 130}}', '#8B0000', NOW(), '😡', 'Angry', '{""genres"": [""metal"", ""hard-rock"", ""punk""]}', NULL),
                ('019eacde-9f88-7156-a4bc-3117d6f041fb', '{""instrumentalness"": {""min"": 0.7}, ""energy"": {""target"": 0.5}, ""tempo"": {""target"": 90}}', '#8A2BE2', NOW(), '🎧', 'Focused', '{""genres"": [""classical"", ""jazz"", ""instrumental""]}', NULL),
                ('019eacde-9f88-718e-88ba-89af3adceda1', '{""valence"": {""max"": 0.3}, ""energy"": {""max"": 0.4}, ""acousticness"": {""min"": 0.5}}', '#1E90FF', NOW(), '😢', 'Sad', '{""genres"": [""sad"", ""acoustic"", ""melancholy"", ""piano""]}', NULL),
                ('019eacde-9f88-725b-bc8b-c5a3fb4bfb79', '{""valence"": {""min"": 0.7}, ""energy"": {""min"": 0.6}}', '#FFD700', NOW(), '😊', 'Happy', '{""genres"": [""pop"", ""dance"", ""dancehall"", ""happy""]}', NULL),
                ('019eacde-9f88-72e6-9139-8949c17d2dbd', '{""valence"": {""target"": 0.5}, ""energy"": {""max"": 0.5}, ""acousticness"": {""min"": 0.3}}', '#00CED1', NOW(), '🌌', 'Dreamy', '{""genres"": [""dream-pop"", ""shoegaze"", ""ambient""]}', NULL),
                ('019eacde-9f88-751d-8740-665eac7b0d83', '{""valence"": {""target"": 0.6}, ""acousticness"": {""target"": 0.5}, ""energy"": {""target"": 0.5}}', '#FF69B4', NOW(), '💕', 'Romantic', '{""genres"": [""romance"", ""r&b"", ""soul"", ""love""]}', NULL),
                ('019eacde-9f88-75cc-a22d-b2b960bcc257', '{""energy"": {""min"": 0.8}, ""danceability"": {""min"": 0.7}, ""tempo"": {""min"": 120}}', '#FF4500', NOW(), '⚡', 'Energetic', '{""genres"": [""rock"", ""hip-hop"", ""workout"", ""edm""]}', NULL),
                ('019eacde-9f88-789b-a2db-592d40face12', '{""energy"": {""max"": 0.5}, ""tempo"": {""max"": 100}, ""acousticness"": {""min"": 0.4}}', '#32CD32', NOW(), '🌿', 'Relaxed', '{""genres"": [""chill"", ""ambient"", ""lo-fi"", ""acoustic""]}', NULL);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "Moods");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

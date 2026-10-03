using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Quiniela.Model;

public partial class QuinielaDbContext : DbContext
{
    public QuinielaDbContext(DbContextOptions<QuinielaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Forecast> Forecasts { get; set; }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<Tournament> Tournaments { get; set; }

    public virtual DbSet<TournamentParticipant> TournamentParticipants { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<MatchKind>("user_25", "match_kind")
            .HasPostgresEnum<MatchStatus>("user_25", "match_status")
            .HasPostgresEnum<Sport>("user_25", "sport");

        modelBuilder.Entity<Forecast>(entity =>
        {
            entity.HasKey(e => e.ForecastId).HasName("forecasts_pkey");

            entity.ToTable("forecasts", "user_25");

            entity.HasIndex(e => new { e.UserId, e.MatchId }, "forecasts_user_id_match_id_key").IsUnique();

            entity.Property(e => e.ForecastId).HasColumnName("forecast_id");
            entity.Property(e => e.MatchId).HasColumnName("match_id");
            entity.Property(e => e.PredictedAwayScore).HasColumnName("predicted_away_score");
            entity.Property(e => e.PredictedHomeScore).HasColumnName("predicted_home_score");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("submitted_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Match).WithMany(p => p.Forecasts)
                .HasForeignKey(d => d.MatchId)
                .HasConstraintName("forecasts_match_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Forecasts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("forecasts_user_id_fkey");
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.MatchId).HasName("matches_pkey");

            entity.ToTable("matches", "user_25");

            entity.Property(e => e.MatchId).HasColumnName("match_id");
            entity.Property(e => e.MatchKind).HasColumnName("match_kind");
            entity.Property(e => e.AwayScore).HasColumnName("away_score");
            entity.Property(e => e.AwayTeamId).HasColumnName("away_team_id");
            entity.Property(e => e.HomeScore).HasColumnName("home_score");
            entity.Property(e => e.HomeTeamId).HasColumnName("home_team_id");
            entity.Property(e => e.PlayedAt).HasColumnName("played_at");
            entity.Property(e => e.Sport).HasColumnName("sport");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.StatsJson)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("stats_json");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");

            entity.HasOne(d => d.AwayTeam).WithMany(p => p.MatchAwayTeams)
                .HasForeignKey(d => d.AwayTeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("matches_away_team_id_fkey");

            entity.HasOne(d => d.HomeTeam).WithMany(p => p.MatchHomeTeams)
                .HasForeignKey(d => d.HomeTeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("matches_home_team_id_fkey");

            entity.HasOne(d => d.Tournament).WithMany(p => p.Matches)
                .HasForeignKey(d => d.TournamentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("matches_tournament_id_fkey");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("teams_pkey");

            entity.ToTable("teams", "user_25");

            entity.HasIndex(e => e.Name, "teams_name_key").IsUnique();

            entity.HasIndex(e => e.ShortName, "teams_short_name_key").IsUnique();

            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ShortName)
                .HasMaxLength(20)
                .HasColumnName("short_name");
            entity.Property(e => e.Sport).HasColumnName("sport");
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasKey(e => e.TournamentId).HasName("tournaments_pkey");

            entity.ToTable("tournaments", "user_25");

            entity.HasIndex(e => e.Name, "tournaments_name_key").IsUnique();

            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Season)
                .HasMaxLength(50)
                .HasColumnName("season");
            entity.Property(e => e.Sport).HasColumnName("sport");
        });

        modelBuilder.Entity<TournamentParticipant>(entity =>
        {
            entity.HasKey(e => e.ParticipantId).HasName("tournament_participants_pkey");

            entity.ToTable("tournament_participants", "user_25");

            entity.HasIndex(e => new { e.TournamentId, e.TeamId }, "tournament_participants_tournament_id_team_id_key").IsUnique();

            entity.Property(e => e.ParticipantId).HasColumnName("participant_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");

            entity.HasOne(d => d.Team).WithMany(p => p.TournamentParticipants)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("tournament_participants_team_id_fkey");

            entity.HasOne(d => d.Tournament).WithMany(p => p.TournamentParticipants)
                .HasForeignKey(d => d.TournamentId)
                .HasConstraintName("tournament_participants_tournament_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users", "user_25");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

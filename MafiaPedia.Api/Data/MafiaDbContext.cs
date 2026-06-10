using System;
using System.Collections.Generic;
using MafiaPedia.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MafiaPedia.Api.Data;

public partial class MafiaDbContext : DbContext
{
    public MafiaDbContext(DbContextOptions<MafiaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Club> Clubs { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Commentlike> Commentlikes { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Master> Masters { get; set; }

    public virtual DbSet<Play> Plays { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Playplayer> Playplayers { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Senario> Senarios { get; set; }

    public virtual DbSet<Side> Sides { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Club>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("club");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("comments");

            entity.HasIndex(e => e.CreatedAt, "IX_Comments_CreatedAt");

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "IX_Comments_Entity");

            entity.HasIndex(e => e.ParentCommentId, "IX_Comments_Parent");

            entity.HasIndex(e => e.UserId, "IX_Comments_User");

            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                .HasForeignKey(d => d.ParentCommentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Comments_Parent");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_User");
        });

        modelBuilder.Entity<Commentlike>(entity =>
        {
            entity.HasKey(e => new { e.CommentId, e.UserId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("commentlikes");

            entity.HasIndex(e => e.UserId, "FK_CommentLikes_User");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Comment).WithMany(p => p.Commentlikes)
                .HasForeignKey(d => d.CommentId)
                .HasConstraintName("FK_CommentLikes_Comment");

            entity.HasOne(d => d.User).WithMany(p => p.Commentlikes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_CommentLikes_User");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("event")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.ClubId, "clubId_idx");

            entity.Property(e => e.Name).HasMaxLength(20);

            entity.HasOne(d => d.Club).WithMany(p => p.Events)
                .HasForeignKey(d => d.ClubId)
                .HasConstraintName("fk_event_club");
        });

        modelBuilder.Entity<Master>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("master")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Play>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("play")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.EventId, "fk_play_club_idx");

            entity.HasIndex(e => e.MasterId, "fk_play_master_idx");

            entity.HasIndex(e => e.RoomId, "fk_play_room1_idx");

            entity.HasIndex(e => e.SenarioId, "fk_play_senario1_idx");

            entity.HasIndex(e => e.WinnersideId, "fk_play_side1_idx");

            entity.HasIndex(e => e.UserId, "fk_play_user1_idx");

            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.Desc).HasMaxLength(300);
            entity.Property(e => e.Link).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(50);

            entity.HasOne(d => d.Event).WithMany(p => p.Plays)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_play_event");

            entity.HasOne(d => d.Master).WithMany(p => p.Plays)
                .HasForeignKey(d => d.MasterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_play_master");

            entity.HasOne(d => d.Room).WithMany(p => p.Plays)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_play_room1");

            entity.HasOne(d => d.Senario).WithMany(p => p.Plays)
                .HasForeignKey(d => d.SenarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_play_senario1");

            entity.HasOne(d => d.User).WithMany(p => p.Plays)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_play_user1");

            entity.HasOne(d => d.Winnerside).WithMany(p => p.Plays)
                .HasForeignKey(d => d.WinnersideId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_play_side1");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("player")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.UserId, "user_id").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Desc).HasMaxLength(300);
            entity.Property(e => e.Mobile).HasMaxLength(11);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Picture).HasMaxLength(300);
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Player)
                .HasForeignKey<Player>(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("player_ibfk_1");
        });

        modelBuilder.Entity<Playplayer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("playplayer")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.PlayId, "fk_playplayer_play1_idx");

            entity.HasIndex(e => e.PlayerId, "fk_playplayer_player1_idx");

            entity.HasIndex(e => e.RoleId, "fk_playplayer_role1_idx");

            entity.HasOne(d => d.Play).WithMany(p => p.Playplayers)
                .HasForeignKey(d => d.PlayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_playplayer_play1");

            entity.HasOne(d => d.Player).WithMany(p => p.Playplayers)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_playplayer_player1");

            entity.HasOne(d => d.Role).WithMany(p => p.Playplayers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_playplayer_role1");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.IsRevoked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_revoked");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("refresh_tokens_ibfk_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("role")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.SenarioId, "fk_role_senario1_idx");

            entity.HasIndex(e => e.SideId, "fk_role_side1_idx");

            entity.Property(e => e.Name).HasMaxLength(20);

            entity.HasOne(d => d.Senario).WithMany(p => p.Roles)
                .HasForeignKey(d => d.SenarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_role_senario1");

            entity.HasOne(d => d.Side).WithMany(p => p.Roles)
                .HasForeignKey(d => d.SideId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_role_side1");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("room")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<Senario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("senario")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<Side>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("side")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("subscriptions");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Plan)
                .HasColumnType("enum('player_basic','player_pro','cafe_basic','cafe_pro')")
                .HasColumnName("plan");
            entity.Property(e => e.StartedAt)
                .HasColumnType("datetime")
                .HasColumnName("started_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subscriptions_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Mobile, "mobile").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(50)
                .HasColumnName("display_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt)
                .HasColumnType("datetime")
                .HasColumnName("last_login_at");
            entity.Property(e => e.Mobile)
                .HasMaxLength(11)
                .HasColumnName("mobile");
            entity.Property(e => e.MobileVerified)
                .HasDefaultValueSql("'0'")
                .HasColumnName("mobile_verified");
            entity.Property(e => e.OtpCode)
                .HasMaxLength(6)
                .HasColumnName("otp_code");
            entity.Property(e => e.OtpExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("otp_expires_at");
            entity.Property(e => e.Password).HasMaxLength(20);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(255)
                .HasColumnName("password_salt");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'player'")
                .HasColumnType("enum('admin','player','master','cafe_owner')")
                .HasColumnName("role");
            entity.Property(e => e.Username).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using System;
using System.Collections.Generic;
using MafiaPedia.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace MafiaPedia.Api.Data;

public partial class MafiaDbContext : DbContext
{
    public MafiaDbContext()
    {
    }

    public MafiaDbContext(DbContextOptions<MafiaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Club> Clubs { get; set; }

    public virtual DbSet<ClubClubplayer> ClubClubplayers { get; set; }

    public virtual DbSet<ClubOrder> ClubOrders { get; set; }

    public virtual DbSet<ClubOrderItem> ClubOrderItems { get; set; }

    public virtual DbSet<ClubSettlement> ClubSettlements { get; set; }

    public virtual DbSet<Clubplay> Clubplays { get; set; }

    public virtual DbSet<Clubplayer> Clubplayers { get; set; }

    public virtual DbSet<Clubplayplayer> Clubplayplayers { get; set; }

    public virtual DbSet<Clubuser> Clubusers { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Commentlike> Commentlikes { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Master> Masters { get; set; }

    public virtual DbSet<Masterlist> Masterlists { get; set; }

    public virtual DbSet<MasterlistClubplayer> MasterlistClubplayers { get; set; }

    public virtual DbSet<Nerkh> Nerkhs { get; set; }

    public virtual DbSet<Play> Plays { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Playplayer> Playplayers { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Senario> Senarios { get; set; }

    public virtual DbSet<SenarioRoleSet> SenarioRoleSets { get; set; }

    public virtual DbSet<Side> Sides { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=mafia;user=devuser;password=123456", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.38-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Club>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("club");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(300)
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.Logo)
                .HasMaxLength(300)
                .HasColumnName("logo");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .HasColumnName("phone");
            entity.Property(e => e.VatPercent)
                .HasColumnType("decimal(5,2)")
                .HasColumnName("vatPercent");
        });

        modelBuilder.Entity<ClubClubplayer>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("club_clubplayer");

            entity.HasIndex(e => e.ClubId, "fk_clubclubplayer_club_idx");

            entity.HasIndex(e => e.ClubplayerId, "fk_clubclubplayer_clubplayer_idx");

            entity.Property(e => e.ClubId).HasColumnName("clubId");
            entity.Property(e => e.ClubplayerId).HasColumnName("clubplayerId");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Club).WithMany()
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubclubplayer_club");

            entity.HasOne(d => d.Clubplayer).WithMany()
                .HasForeignKey(d => d.ClubplayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubclubplayer_clubplayer");
        });

        modelBuilder.Entity<ClubOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("club_order");

            entity.HasIndex(e => e.DeletedByUserId, "fk_cluborder_deletedby");

            entity.HasIndex(e => e.RegisteredByUserId, "fk_cluborder_registeredby");

            entity.HasIndex(e => new { e.ClubId, e.BusinessDate }, "idx_cluborder_businessdate");

            entity.HasIndex(e => e.ClubPlayerId, "idx_cluborder_clubplayer");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.SettledAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'open'")
                .HasColumnType("enum('open','partial','settled')");

            entity.HasOne(d => d.Club).WithMany(p => p.ClubOrders)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cluborder_club");

            entity.HasOne(d => d.ClubPlayer).WithMany(p => p.ClubOrders)
                .HasForeignKey(d => d.ClubPlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cluborder_clubplayer");

            entity.HasOne(d => d.DeletedByUser).WithMany(p => p.ClubOrderDeletedByUsers)
                .HasForeignKey(d => d.DeletedByUserId)
                .HasConstraintName("fk_cluborder_deletedby");

            entity.HasOne(d => d.RegisteredByUser).WithMany(p => p.ClubOrderRegisteredByUsers)
                .HasForeignKey(d => d.RegisteredByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cluborder_registeredby");
        });

        modelBuilder.Entity<ClubOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("club_order_item");

            entity.HasIndex(e => e.DeletedByUserId, "fk_orderitem_deletedby");

            entity.HasIndex(e => e.OrderId, "fk_orderitem_order");

            entity.HasIndex(e => e.ProductId, "fk_orderitem_product");

            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasDefaultValueSql("'1'");
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);

            entity.HasOne(d => d.DeletedByUser).WithMany(p => p.ClubOrderItems)
                .HasForeignKey(d => d.DeletedByUserId)
                .HasConstraintName("fk_orderitem_deletedby");

            entity.HasOne(d => d.Order).WithMany(p => p.ClubOrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orderitem_order");

            entity.HasOne(d => d.Product).WithMany(p => p.ClubOrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orderitem_product");
        });

        modelBuilder.Entity<ClubSettlement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("club_settlement");

            entity.HasIndex(e => e.ClubId, "fk_settlement_club");

            entity.HasIndex(e => e.CreatedByUserId, "fk_settlement_createdby");

            entity.HasIndex(e => e.DeletedByUserId, "fk_settlement_deletedby");

            entity.HasIndex(e => e.OrderId, "fk_settlement_order");

            entity.HasIndex(e => e.ClubPlayerId, "idx_settlement_clubplayer");

            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(300);
            entity.Property(e => e.PaymentMethod).HasColumnType("enum('cash','pos','card_to_card')");

            entity.HasOne(d => d.Club).WithMany(p => p.ClubSettlements)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_settlement_club");

            entity.HasOne(d => d.ClubPlayer).WithMany(p => p.ClubSettlements)
                .HasForeignKey(d => d.ClubPlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_settlement_clubplayer");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ClubSettlementCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_settlement_createdby");

            entity.HasOne(d => d.DeletedByUser).WithMany(p => p.ClubSettlementDeletedByUsers)
                .HasForeignKey(d => d.DeletedByUserId)
                .HasConstraintName("fk_settlement_deletedby");

            entity.HasOne(d => d.Order).WithMany(p => p.ClubSettlements)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_settlement_order");
        });

        modelBuilder.Entity<Clubplay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("clubplay")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.DeletedByUserId, "fk_clubplay_deletedby");

            entity.HasIndex(e => e.EventId, "fk_clubplay_event_idx");

            entity.HasIndex(e => e.MasterId, "fk_clubplay_master_idx");

            entity.HasIndex(e => e.NerkhId, "fk_clubplay_nerkh");

            entity.HasIndex(e => e.RoomId, "fk_clubplay_room_idx");

            entity.HasIndex(e => e.SenarioId, "fk_clubplay_senario_idx");

            entity.HasIndex(e => e.WinnersideId, "fk_clubplay_side_idx");

            entity.HasIndex(e => e.UserId, "fk_clubplay_user_idx");

            entity.HasIndex(e => e.IsDeleted, "idx_clubplay_isdeleted");

            entity.Property(e => e.BusinessDate).HasComputedColumnSql("if((cast(`DateTime` as time) < _utf8mb4'12:00:00'),(cast(`DateTime` as date) - interval 1 day),cast(`DateTime` as date))", true);
            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Desc).HasMaxLength(300);
            entity.Property(e => e.Link).HasMaxLength(300);
            entity.Property(e => e.PlayType)
                .HasColumnType("enum('normal','rank','superrank','etc')")
                .HasColumnName("playType");
            entity.Property(e => e.Status)
                .HasColumnType("enum('pending','notwinside','notrank','done')")
                .HasColumnName("status");
            entity.Property(e => e.Title).HasMaxLength(50);

            entity.HasOne(d => d.DeletedByUser).WithMany(p => p.ClubplayDeletedByUsers)
                .HasForeignKey(d => d.DeletedByUserId)
                .HasConstraintName("fk_clubplay_deletedby");

            entity.HasOne(d => d.Event).WithMany(p => p.Clubplays)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubplay_event");

            entity.HasOne(d => d.Master).WithMany(p => p.Clubplays)
                .HasForeignKey(d => d.MasterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubplay_master");

            entity.HasOne(d => d.Nerkh).WithMany(p => p.Clubplays)
                .HasForeignKey(d => d.NerkhId)
                .HasConstraintName("fk_clubplay_nerkh");

            entity.HasOne(d => d.Room).WithMany(p => p.Clubplays)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubplay_room");

            entity.HasOne(d => d.Senario).WithMany(p => p.Clubplays)
                .HasForeignKey(d => d.SenarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubplay_senario");

            entity.HasOne(d => d.User).WithMany(p => p.ClubplayUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubplay_user");

            entity.HasOne(d => d.Winnerside).WithMany(p => p.Clubplays)
                .HasForeignKey(d => d.WinnersideId)
                .HasConstraintName("fk_clubplay_side");
        });

        modelBuilder.Entity<Clubplayer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("clubplayer")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Mobile, "Mobile_UNIQUE").IsUnique();

            entity.HasIndex(e => e.UserId, "user_id_UNIQUE").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Desc).HasMaxLength(300);
            entity.Property(e => e.Mobile).HasMaxLength(11);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Picture).HasMaxLength(300);
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Clubplayer)
                .HasForeignKey<Clubplayer>(d => d.UserId)
                .HasConstraintName("fk_clubplayer_user");
        });

        modelBuilder.Entity<Clubplayplayer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("clubplayplayer")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.PlayId, "fk_clubplayplayer_play_idx");

            entity.HasIndex(e => e.PlayerId, "fk_clubplayplayer_player_idx");

            entity.HasIndex(e => e.RoleId, "fk_clubplayplayer_role_idx");

            entity.HasOne(d => d.Play).WithMany(p => p.Clubplayplayers)
                .HasForeignKey(d => d.PlayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubplayplayer_play");

            entity.HasOne(d => d.Player).WithMany(p => p.Clubplayplayers)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubplayplayer_player");

            entity.HasOne(d => d.Role).WithMany(p => p.Clubplayplayers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubplayplayer_role");
        });

        modelBuilder.Entity<Clubuser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("clubuser");

            entity.HasIndex(e => e.ClubId, "fk_clubuser_club_idx");

            entity.HasIndex(e => e.UserId, "fk_clubuser_user_idx");

            entity.HasIndex(e => new { e.UserId, e.ClubId }, "uq_clubuser_user_club").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClubId).HasColumnName("clubId");
            entity.Property(e => e.ClubuserRole)
                .HasColumnType("enum('cashier','master','supervisor','owner')")
                .HasColumnName("clubuserRole");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Club).WithMany(p => p.Clubusers)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubuser_club");

            entity.HasOne(d => d.User).WithMany(p => p.Clubusers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_clubuser_user");
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

            entity.HasIndex(e => e.DefaultClubId, "uq_event_default_per_club").IsUnique();

            entity.Property(e => e.DefaultClubId).HasComputedColumnSql("case when (`IsDefault` = 1) then `ClubId` else NULL end", true);
            entity.Property(e => e.Name).HasMaxLength(20);

            entity.HasOne(d => d.Club).WithMany(p => p.Events)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_event_club");
        });

        modelBuilder.Entity<Master>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("master")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.ClubId, "fk_master_club_idx");

            entity.HasIndex(e => e.UserId, "userId_UNIQUE").IsUnique();

            entity.Property(e => e.Bio).HasMaxLength(300);
            entity.Property(e => e.ClubId).HasColumnName("ClubID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Photo).HasMaxLength(300);
            entity.Property(e => e.RatePerGame).HasPrecision(10, 2);
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Club).WithMany(p => p.Masters)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_master_club");

            entity.HasOne(d => d.User).WithOne(p => p.Master)
                .HasForeignKey<Master>(d => d.UserId)
                .HasConstraintName("fk_master_user");
        });

        modelBuilder.Entity<Masterlist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("masterlist");

            entity.HasIndex(e => e.MasteId, "fk_masterlist_mast_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MasteId).HasColumnName("masteId");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");

            entity.HasOne(d => d.Maste).WithMany(p => p.Masterlists)
                .HasForeignKey(d => d.MasteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_masterlist_mast");
        });

        modelBuilder.Entity<MasterlistClubplayer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("masterlist_clubplayer");

            entity.HasIndex(e => e.MasterlistId, "fk_list_list_idx");

            entity.HasIndex(e => e.ClubplayerId, "fk_list_player_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClubplayerId).HasColumnName("clubplayerId");
            entity.Property(e => e.MasterlistId).HasColumnName("masterlistId");

            entity.HasOne(d => d.Clubplayer).WithMany(p => p.MasterlistClubplayers)
                .HasForeignKey(d => d.ClubplayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_list_player");

            entity.HasOne(d => d.Masterlist).WithMany(p => p.MasterlistClubplayers)
                .HasForeignKey(d => d.MasterlistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_list_list");
        });

        modelBuilder.Entity<Nerkh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("nerkh");

            entity.HasIndex(e => e.ClubId, "fk_nerkh_club");

            entity.HasIndex(e => e.DeletedByUserId, "fk_nerkh_deletedby");

            entity.HasIndex(e => e.DefaultClubId, "uq_nerkh_default_per_club").IsUnique();

            entity.Property(e => e.DefaultClubId).HasComputedColumnSql("case when ((`IsDefault` = 1) and (`IsDeleted` = 0)) then `ClubId` else NULL end", true);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.Name).HasMaxLength(45);
            entity.Property(e => e.Price).HasPrecision(10, 2);

            entity.HasOne(d => d.Club).WithMany(p => p.Nerkhs)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nerkh_club");

            entity.HasOne(d => d.DeletedByUser).WithMany(p => p.Nerkhs)
                .HasForeignKey(d => d.DeletedByUserId)
                .HasConstraintName("fk_nerkh_deletedby");
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
            entity.Property(e => e.Picture)
                .HasMaxLength(300)
                .HasColumnName("picture");
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

            entity.HasIndex(e => e.Mobile, "Mobile_UNIQUE").IsUnique();

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

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("product");

            entity.HasIndex(e => e.CategoryId, "fk_product_category");

            entity.HasIndex(e => e.ClubId, "fk_product_club");

            entity.HasIndex(e => e.DeletedByUserId, "fk_product_deletedby");

            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.Name).HasMaxLength(45);
            entity.Property(e => e.Price).HasPrecision(10, 2);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_category");

            entity.HasOne(d => d.Club).WithMany(p => p.Products)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_club");

            entity.HasOne(d => d.DeletedByUser).WithMany(p => p.Products)
                .HasForeignKey(d => d.DeletedByUserId)
                .HasConstraintName("fk_product_deletedby");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("product_category");

            entity.HasIndex(e => e.ClubId, "fk_prodcat_club");

            entity.HasIndex(e => e.DeletedByUserId, "fk_prodcat_deletedby");

            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(45);

            entity.HasOne(d => d.Club).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_prodcat_club");

            entity.HasOne(d => d.DeletedByUser).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.DeletedByUserId)
                .HasConstraintName("fk_prodcat_deletedby");
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
            entity.Property(e => e.Photo).HasMaxLength(300);

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

            entity.HasIndex(e => e.ClubId, "fk_room_club_idx");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.Name).HasMaxLength(20);

            entity.HasOne(d => d.Club).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_room_club");
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

        modelBuilder.Entity<SenarioRoleSet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("senario_role_set");

            entity.HasIndex(e => new { e.SenarioId, e.PlayerCount }, "uq_srs_senario_count").IsUnique();

            entity.Property(e => e.RoleIds).HasColumnType("json");

            entity.HasOne(d => d.Senario).WithMany(p => p.SenarioRoleSets)
                .HasForeignKey(d => d.SenarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_srs_senario");
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
                .HasDefaultValueSql("'user'")
                .HasColumnType("enum('admin','user','club')")
                .HasColumnName("role");
            entity.Property(e => e.Username).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

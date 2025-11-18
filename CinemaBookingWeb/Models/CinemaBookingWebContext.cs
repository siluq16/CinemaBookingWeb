using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingWeb.Models;

public partial class CinemaBookingWebContext : DbContext
{
    public CinemaBookingWebContext()
    {
    }

    public CinemaBookingWebContext(DbContextOptions<CinemaBookingWebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<ChiTietDoAn> ChiTietDoAns { get; set; }

    public virtual DbSet<DoAn> DoAns { get; set; }

    public virtual DbSet<GheNgoi> GheNgois { get; set; }

    public virtual DbSet<GiaVe> GiaVes { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }

    public virtual DbSet<LichChieu> LichChieus { get; set; }

    public virtual DbSet<Phim> Phims { get; set; }

    public virtual DbSet<PhongChieu> PhongChieus { get; set; }

    public virtual DbSet<Ve> Ves { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.TenDangNhap).HasName("PK__Account__55F68FC1E793A2E4");

            entity.ToTable("Account");

            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
            entity.Property(e => e.MaDoiTuong).HasMaxLength(20);
            entity.Property(e => e.MatKhau).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.VaiTro).HasMaxLength(20);

            entity.HasOne(d => d.MaDoiTuongNavigation).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.MaDoiTuong)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Account_KhachHang");
        });

        modelBuilder.Entity<ChiTietDoAn>(entity =>
        {
            entity.HasKey(e => new { e.MaHd, e.MaDoAn }).HasName("PK__ChiTietD__45F957E633151191");

            entity.ToTable("ChiTietDoAn");

            entity.Property(e => e.MaHd)
                .HasMaxLength(20)
                .HasColumnName("MaHD");
            entity.Property(e => e.MaDoAn).HasMaxLength(5);
            entity.Property(e => e.ThanhTien).HasColumnType("decimal(10, 0)");

            entity.HasOne(d => d.MaDoAnNavigation).WithMany(p => p.ChiTietDoAns)
                .HasForeignKey(d => d.MaDoAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaDoA__6B24EA82");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.ChiTietDoAns)
                .HasForeignKey(d => d.MaHd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDoA__MaHD__6A30C649");
        });

        modelBuilder.Entity<DoAn>(entity =>
        {
            entity.HasKey(e => e.MaDoAn).HasName("PK__DoAn__2DCF1067CDA7DE23");

            entity.ToTable("DoAn");

            entity.Property(e => e.MaDoAn).HasMaxLength(5);
            entity.Property(e => e.Anh).HasMaxLength(255);
            entity.Property(e => e.DonGia).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenDoAn).HasMaxLength(100);
        });

        modelBuilder.Entity<GheNgoi>(entity =>
        {
            entity.HasKey(e => e.MaGhe).HasName("PK__GheNgoi__3CD3C67B77D6BFFC");

            entity.ToTable("GheNgoi");

            entity.Property(e => e.MaGhe).HasMaxLength(20);
            entity.Property(e => e.HangGhe)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LoaiGhe).HasMaxLength(20);
            entity.Property(e => e.MaPhong).HasMaxLength(20);

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.GheNgois)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GheNgoi__MaPhong__534D60F1");
        });

        modelBuilder.Entity<GiaVe>(entity =>
        {
            entity.HasKey(e => e.MaGia).HasName("PK__GiaVe__3CD3DE5E1D57CAC1");

            entity.ToTable("GiaVe");

            entity.Property(e => e.MaGia).HasMaxLength(20);
            entity.Property(e => e.Gia).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.LoaiGhe).HasMaxLength(20);
            entity.Property(e => e.LoaiNgay).HasMaxLength(20);
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHd).HasName("PK__HoaDon__2725A6E0B1A9469F");

            entity.ToTable("HoaDon");

            entity.Property(e => e.MaHd)
                .HasMaxLength(20)
                .HasColumnName("MaHD");
            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.MaKm)
                .HasMaxLength(20)
                .HasColumnName("MaKM");
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.ThoiGianHetHan).HasColumnType("datetime");
            entity.Property(e => e.TongTien).HasColumnType("decimal(12, 0)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Ch? thanh toán");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoaDon__MaKH__619B8048");

            entity.HasOne(d => d.MaKmNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaKm)
                .HasConstraintName("FK__HoaDon__MaKM__628FA481");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKh).HasName("PK__KhachHan__2725CF1E89A4B086");

            entity.ToTable("KhachHang");

            entity.Property(e => e.MaKh)
                .HasMaxLength(20)
                .HasColumnName("MaKH");
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<KhuyenMai>(entity =>
        {
            entity.HasKey(e => e.MaKm).HasName("PK__KhuyenMa__2725CF156E0BB9E7");

            entity.ToTable("KhuyenMai");

            entity.Property(e => e.MaKm)
                .HasMaxLength(20)
                .HasColumnName("MaKM");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.SoTienGiam).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.TenKm)
                .HasMaxLength(100)
                .HasColumnName("TenKM");
        });

        modelBuilder.Entity<LichChieu>(entity =>
        {
            entity.HasKey(e => e.MaLich).HasName("PK__LichChie__728A9AE9717442CE");

            entity.ToTable("LichChieu");

            entity.Property(e => e.MaLich).HasMaxLength(20);
            entity.Property(e => e.MaPhim).HasMaxLength(20);
            entity.Property(e => e.MaPhong).HasMaxLength(20);

            entity.HasOne(d => d.MaPhimNavigation).WithMany(p => p.LichChieus)
                .HasForeignKey(d => d.MaPhim)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichChieu__MaPhi__4F7CD00D");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.LichChieus)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichChieu__MaPho__5070F446");
        });

        modelBuilder.Entity<Phim>(entity =>
        {
            entity.HasKey(e => e.MaPhim).HasName("PK__Phim__4AC03DE33D242480");

            entity.ToTable("Phim");

            entity.Property(e => e.MaPhim).HasMaxLength(20);
            entity.Property(e => e.DaoDien).HasMaxLength(100);
            entity.Property(e => e.DienVien).HasMaxLength(255);
            entity.Property(e => e.Poster).HasMaxLength(255);
            entity.Property(e => e.TenPhim).HasMaxLength(100);
            entity.Property(e => e.TheLoai).HasMaxLength(255);
            entity.Property(e => e.Trailer).HasMaxLength(255);
        });

        modelBuilder.Entity<PhongChieu>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__PhongChi__20BD5E5B12C9CE7C");

            entity.ToTable("PhongChieu");

            entity.Property(e => e.MaPhong).HasMaxLength(20);
            entity.Property(e => e.SoLuongGhe).HasComputedColumnSql("([SoHangGhe]*[SoGheMoiHang])", true);
            entity.Property(e => e.TenPhong).HasMaxLength(50);
        });

        modelBuilder.Entity<Ve>(entity =>
        {
            entity.HasKey(e => e.MaVe).HasName("PK__Ve__2725100FA272B935");

            entity.ToTable("Ve");

            entity.Property(e => e.MaVe).HasMaxLength(20);
            entity.Property(e => e.GiaVe).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.MaGhe).HasMaxLength(20);
            entity.Property(e => e.MaHd)
                .HasMaxLength(20)
                .HasColumnName("MaHD");
            entity.Property(e => e.MaLich).HasMaxLength(20);

            entity.HasOne(d => d.MaGheNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaGhe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ve__MaGhe__66603565");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaHd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ve__MaHD__6754599E");

            entity.HasOne(d => d.MaLichNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaLich)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ve__MaLich__656C112C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

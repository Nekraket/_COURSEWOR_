using Microsoft.EntityFrameworkCore;

namespace Курсовая.Models;

public partial class HealthcareManagementContext : DbContext
{
    public HealthcareManagementContext()
    {
    }

    public HealthcareManagementContext(DbContextOptions<HealthcareManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Аллергии> Аллергииs { get; set; }

    public virtual DbSet<АрхивРецептов> АрхивРецептовs { get; set; }

    public virtual DbSet<ВектораИзображений> ВектораИзображенийs { get; set; }

    public virtual DbSet<ЗафиксированныеСимптомы> ЗафиксированныеСимптомыs { get; set; }

    public virtual DbSet<ЗначенияИзмерения> ЗначенияИзмеренияs { get; set; }

    public virtual DbSet<Измерение> Измерениеs { get; set; }

    public virtual DbSet<ИконкиЛекарств> ИконкиЛекарствs { get; set; }

    public virtual DbSet<КатегорииОтслеживания> КатегорииОтслеживанияs { get; set; }

    public virtual DbSet<КатегорияАрхива> КатегорияАрхиваs { get; set; }

    public virtual DbSet<Лекарства> Лекарстваs { get; set; }

    public virtual DbSet<ЛичныйАрхив> ЛичныйАрхивs { get; set; }

    public virtual DbSet<НапоминаниеИзмерения> НапоминаниеИзмеренияs { get; set; }

    public virtual DbSet<НапоминаниеЛекарства> НапоминаниеЛекарстваs { get; set; }

    public virtual DbSet<НапоминаниеСимптомы> НапоминаниеСимптомыs { get; set; }

    public virtual DbSet<Периодичность> Периодичностьs { get; set; }

    public virtual DbSet<ПозицияЗаписи> ПозицияЗаписиs { get; set; }

    public virtual DbSet<ПолучателиУхода> ПолучателиУходаs { get; set; }

    public virtual DbSet<Пользователь> Пользовательs { get; set; }

    public virtual DbSet<Симптомы> Симптомыs { get; set; }

    public virtual DbSet<СоставВЛекарстве> СоставВЛекарствеs { get; set; }

    public virtual DbSet<СоставЛекарства> СоставЛекарстваs { get; set; }

    public virtual DbSet<СпособыПриёма> СпособыПриёмаs { get; set; }

    public virtual DbSet<ТипИзмерения> ТипИзмеренияs { get; set; }

    public virtual DbSet<ФиксацияПриёма> ФиксацияПриёмаs { get; set; }

    public virtual DbSet<ЦветИконкиЛекарств> ЦветИконкиЛекарствs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-3HCFBNF\\MSSQLSERVER01;Initial Catalog=Healthcare_Management;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Аллергии>(entity =>
        {
            entity.HasKey(e => e.PkIdАллергии).HasName("PK__Аллергии__A802622A1ABAABC8");

            entity.ToTable("Аллергии");

            entity.Property(e => e.PkIdАллергии).HasColumnName("PK_id_аллергии");
            entity.Property(e => e.FkIdПолучателя).HasColumnName("FK_id_получателя");
            entity.Property(e => e.FkIdПользователя).HasColumnName("FK_id_пользователя");
            entity.Property(e => e.Аллерген).HasMaxLength(200);

            entity.HasOne(d => d.FkIdПолучателяNavigation).WithMany(p => p.Аллергииs)
                .HasForeignKey(d => d.FkIdПолучателя)
                .HasConstraintName("FK_Аллергия_Получатель");

            entity.HasOne(d => d.FkIdПользователяNavigation).WithMany(p => p.Аллергииs)
                .HasForeignKey(d => d.FkIdПользователя)
                .HasConstraintName("FK_Аллергия_Пользователь");
        });

        modelBuilder.Entity<АрхивРецептов>(entity =>
        {
            entity.HasKey(e => e.PkIdАрхива).HasName("PK__Архив_ре__8B19FF2D4C524762");

            entity.ToTable("Архив_рецептов");

            entity.Property(e => e.PkIdАрхива).HasColumnName("PK_id_архива");
            entity.Property(e => e.FkIdПользователя).HasColumnName("FK_id_пользователя");
            entity.Property(e => e.ДатаЗагрузки)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Дата_загрузки");
            entity.Property(e => e.Изображение).IsUnicode(false);

            entity.HasOne(d => d.FkIdПользователяNavigation).WithMany(p => p.АрхивРецептовs)
                .HasForeignKey(d => d.FkIdПользователя)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_АрхивРецептов_Пользователь");
        });

        modelBuilder.Entity<ВектораИзображений>(entity =>
        {
            entity.HasKey(e => e.PkIdВектора).HasName("PK__Вектора___606B84CD0DC29AD9");

            entity.ToTable("Вектора_изображений");

            entity.HasIndex(e => e.Название, "UQ__Вектора___38DA8035DAC0EF21").IsUnique();

            entity.Property(e => e.PkIdВектора).HasColumnName("PK_id_вектора");
            entity.Property(e => e.FkIdКатегорииОтслеж).HasColumnName("FK_id_категорииОтслеж");
            entity.Property(e => e.Вектор).HasColumnType("text");
            entity.Property(e => e.Название)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.FkIdКатегорииОтслежNavigation).WithMany(p => p.ВектораИзображенийs)
                .HasForeignKey(d => d.FkIdКатегорииОтслеж)
                .HasConstraintName("FK_Вектора_Категории");
        });

        modelBuilder.Entity<ЗафиксированныеСимптомы>(entity =>
        {
            entity.HasKey(e => e.PkIdЗафСимптомы).HasName("PK__Зафиксир__57D6F5D7FD9D642B");

            entity.ToTable("Зафиксированные_симптомы");

            entity.Property(e => e.PkIdЗафСимптомы).HasColumnName("PK_id_ЗафСимптомы");
            entity.Property(e => e.FkIdНапоминанияСимптомы).HasColumnName("FK_id_напоминанияСимптомы");
            entity.Property(e => e.ДатаЗаписи)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Дата_записи");
            entity.Property(e => e.Заметка).HasMaxLength(500);
            entity.Property(e => e.ОценкаСамочувствия).HasColumnName("Оценка_самочувствия");

            entity.HasOne(d => d.FkIdНапоминанияСимптомыNavigation).WithMany(p => p.ЗафиксированныеСимптомыs)
                .HasForeignKey(d => d.FkIdНапоминанияСимптомы)
                .HasConstraintName("FK_ЗафиксированныеСимптомы_НапоминаниеСимптомы");
        });

        modelBuilder.Entity<ЗначенияИзмерения>(entity =>
        {
            entity.HasKey(e => e.PkIdЗначениеИзмерения).HasName("PK__Значения__6AEB6A74814CFC70");

            entity.ToTable("Значения_измерения");

            entity.Property(e => e.PkIdЗначениеИзмерения).HasColumnName("PK_id_значение_измерения");
            entity.Property(e => e.FkIdНапоминанияИзмерения).HasColumnName("FK_id_напоминанияИзмерения");
            entity.Property(e => e.ДатаЗаписи)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Дата_записи");
            entity.Property(e => e.Заметка).HasMaxLength(500);

            entity.HasOne(d => d.FkIdНапоминанияИзмеренияNavigation).WithMany(p => p.ЗначенияИзмеренияs)
                .HasForeignKey(d => d.FkIdНапоминанияИзмерения)
                .HasConstraintName("FK_ЗначенияИзмерения_НапоминаниеИзмерения");
        });

        modelBuilder.Entity<Измерение>(entity =>
        {
            entity.HasKey(e => e.PkIdИзмерения).HasName("PK__Измерени__FA9B95E3A9964D22");

            entity.ToTable("Измерение");

            entity.Property(e => e.PkIdИзмерения).HasColumnName("PK_id_измерения");
            entity.Property(e => e.FkIdПериодичности).HasColumnName("FK_id_периодичности");
            entity.Property(e => e.FkIdПозиции).HasColumnName("FK_id_позиции");
            entity.Property(e => e.FkIdТипИзмерения).HasColumnName("FK_id_ТипИзмерения");
            entity.Property(e => e.ЗаписейВДень).HasColumnName("Записей_в_день");

            entity.HasOne(d => d.FkIdПериодичностиNavigation).WithMany(p => p.Измерениеs)
                .HasForeignKey(d => d.FkIdПериодичности)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Измерение_Периодичность");

            entity.HasOne(d => d.FkIdПозицииNavigation).WithMany(p => p.Измерениеs)
                .HasForeignKey(d => d.FkIdПозиции)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Измерение_Позиция");

            entity.HasOne(d => d.FkIdТипИзмеренияNavigation).WithMany(p => p.Измерениеs)
                .HasForeignKey(d => d.FkIdТипИзмерения)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Измерение_Тип");
        });

        modelBuilder.Entity<ИконкиЛекарств>(entity =>
        {
            entity.HasKey(e => e.PkIdИконки).HasName("PK__Иконки_л__15E72EB2AF4DBC85");

            entity.ToTable("Иконки_лекарств");

            entity.Property(e => e.PkIdИконки).HasColumnName("PK_id_иконки");
            entity.Property(e => e.FkIdВектора).HasColumnName("FK_id_вектора");
            entity.Property(e => e.FkIdЦветИконки).HasColumnName("FK_id_ЦветИконки");

            entity.HasOne(d => d.FkIdВектораNavigation).WithMany(p => p.ИконкиЛекарствs)
                .HasForeignKey(d => d.FkIdВектора)
                .HasConstraintName("FK_Иконки_Вектор");

            entity.HasOne(d => d.FkIdЦветИконкиNavigation).WithMany(p => p.ИконкиЛекарствs)
                .HasForeignKey(d => d.FkIdЦветИконки)
                .HasConstraintName("FK_Иконки_Цвет");
        });

        modelBuilder.Entity<КатегорииОтслеживания>(entity =>
        {
            entity.HasKey(e => e.PkIdКатегорииОтслеж).HasName("PK__Категори__5A9756BD04596C37");

            entity.ToTable("Категории_отслеживания");

            entity.HasIndex(e => e.Тип, "UQ__Категори__AC85C27526CE9625").IsUnique();

            entity.Property(e => e.PkIdКатегорииОтслеж).HasColumnName("PK_id_категорииОтслеж");
            entity.Property(e => e.Тип)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<КатегорияАрхива>(entity =>
        {
            entity.HasKey(e => e.PkIdКатегорииАрхива).HasName("PK__Категори__87979DC78C09A190");

            entity.ToTable("Категория_архива");

            entity.Property(e => e.PkIdКатегорииАрхива).HasColumnName("PK_id_категорииАрхива");
            entity.Property(e => e.Название).HasMaxLength(100);
        });

        modelBuilder.Entity<Лекарства>(entity =>
        {
            entity.HasKey(e => e.PkIdЛекарства).HasName("PK__Лекарств__E322334D3A17A2A2");

            entity.ToTable("Лекарства");

            entity.Property(e => e.PkIdЛекарства).HasColumnName("PK_id_лекарства");
            entity.Property(e => e.FkIdИконки).HasColumnName("FK_id_иконки");
            entity.Property(e => e.FkIdЛичныйАрхив).HasColumnName("FK_id_личныйАрхив");
            entity.Property(e => e.FkIdПериодичности).HasColumnName("FK_id_периодичности");
            entity.Property(e => e.FkIdПозиции).HasColumnName("FK_id_позиции");
            entity.Property(e => e.FkIdСпособаПриёма).HasColumnName("FK_id_способаПриёма");
            entity.Property(e => e.ДлительностьПриёма).HasColumnName("Длительность_приёма");
            entity.Property(e => e.Комментарий).HasMaxLength(500);
            entity.Property(e => e.МинЗапас).HasColumnName("Мин_запас");
            entity.Property(e => e.Название).HasMaxLength(200);
            entity.Property(e => e.Назначение).HasMaxLength(500);
            entity.Property(e => e.НапоминанияЗапас).HasColumnName("Напоминания_запас");
            entity.Property(e => e.ПриёмовВДень).HasColumnName("Приёмов_в_день");
            entity.Property(e => e.ТекущийЗапас).HasColumnName("Текущий_запас");
            entity.Property(e => e.Фото).IsUnicode(false);

            entity.HasOne(d => d.FkIdИконкиNavigation).WithMany(p => p.Лекарстваs)
                .HasForeignKey(d => d.FkIdИконки)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Лекарства_Иконки");

            entity.HasOne(d => d.FkIdЛичныйАрхивNavigation).WithMany(p => p.Лекарстваs)
                .HasForeignKey(d => d.FkIdЛичныйАрхив)
                .HasConstraintName("FK_Лекарства_ЛичныйАрхив");

            entity.HasOne(d => d.FkIdПериодичностиNavigation).WithMany(p => p.Лекарстваs)
                .HasForeignKey(d => d.FkIdПериодичности)
                .HasConstraintName("FK_Лекарства_Периодичность");

            entity.HasOne(d => d.FkIdПозицииNavigation).WithMany(p => p.Лекарстваs)
                .HasForeignKey(d => d.FkIdПозиции)
                .HasConstraintName("FK_Лекарства_Позиция");

            entity.HasOne(d => d.FkIdСпособаПриёмаNavigation).WithMany(p => p.Лекарстваs)
                .HasForeignKey(d => d.FkIdСпособаПриёма)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Лекарства_СпособПриёма");
        });

        modelBuilder.Entity<ЛичныйАрхив>(entity =>
        {
            entity.HasKey(e => e.PkIdЛичногоАрхива).HasName("PK__Личный_а__356551642DAA5CB3");

            entity.ToTable("Личный_архив");

            entity.Property(e => e.PkIdЛичногоАрхива).HasColumnName("PK_id_личногоАрхива");
            entity.Property(e => e.FkIdКатегорииАрхива).HasColumnName("FK_id_категорииАрхива");
            entity.Property(e => e.FkIdПолучателя).HasColumnName("FK_id_получателя");
            entity.Property(e => e.FkIdПользователя).HasColumnName("FK_id_пользователя");

            entity.HasOne(d => d.FkIdКатегорииАрхиваNavigation).WithMany(p => p.ЛичныйАрхивs)
                .HasForeignKey(d => d.FkIdКатегорииАрхива)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ЛичныйАрхив_КатегорияАрхива");

            entity.HasOne(d => d.FkIdПолучателяNavigation).WithMany(p => p.ЛичныйАрхивs)
                .HasForeignKey(d => d.FkIdПолучателя)
                .HasConstraintName("FK_ЛичныйАрхив_Получатель");

            entity.HasOne(d => d.FkIdПользователяNavigation).WithMany(p => p.ЛичныйАрхивs)
                .HasForeignKey(d => d.FkIdПользователя)
                .HasConstraintName("FK_ЛичныйАрхив_Пользователь");
        });

        modelBuilder.Entity<НапоминаниеИзмерения>(entity =>
        {
            entity.HasKey(e => e.PkIdНапоминанияИзмерения).HasName("PK__Напомина__C03E83EEC9F81AA1");

            entity.ToTable("Напоминание_измерения");

            entity.Property(e => e.PkIdНапоминанияИзмерения).HasColumnName("PK_id_напоминанияИзмерения");
            entity.Property(e => e.FkIdИзмерения).HasColumnName("FK_id_измерения");

            entity.HasOne(d => d.FkIdИзмеренияNavigation).WithMany(p => p.НапоминаниеИзмеренияs)
                .HasForeignKey(d => d.FkIdИзмерения)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_НапоминаниеИзмерения_Измерение");
        });

        modelBuilder.Entity<НапоминаниеЛекарства>(entity =>
        {
            entity.HasKey(e => e.PkIdНапоминанияЛекарства).HasName("PK__Напомина__80F3C3544882C884");

            entity.ToTable("Напоминание_лекарства");

            entity.Property(e => e.PkIdНапоминанияЛекарства).HasColumnName("PK_id_напоминанияЛекарства");
            entity.Property(e => e.FkIdЛекарства).HasColumnName("FK_id_лекарства");

            entity.HasOne(d => d.FkIdЛекарстваNavigation).WithMany(p => p.НапоминаниеЛекарстваs)
                .HasForeignKey(d => d.FkIdЛекарства)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_НапоминаниеЛекарства_Лекарство");
        });

        modelBuilder.Entity<НапоминаниеСимптомы>(entity =>
        {
            entity.HasKey(e => e.PkIdНапоминанияСимптомы).HasName("PK__Напомина__3101EE9BEBD0E064");

            entity.ToTable("Напоминание_симптомы");

            entity.Property(e => e.PkIdНапоминанияСимптомы).HasColumnName("PK_id_напоминанияСимптомы");
            entity.Property(e => e.FkIdСимптомы).HasColumnName("FK_id_симптомы");

            entity.HasOne(d => d.FkIdСимптомыNavigation).WithMany(p => p.НапоминаниеСимптомыs)
                .HasForeignKey(d => d.FkIdСимптомы)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_НапоминаниеСимптомы_Симптрмы");
        });

        modelBuilder.Entity<Периодичность>(entity =>
        {
            entity.HasKey(e => e.PkIdПериодичности).HasName("PK__Периодич__FCBD11DEAD651804");

            entity.ToTable("Периодичность");

            entity.HasIndex(e => e.Название, "UQ__Периодич__38DA8035D56AB61E").IsUnique();

            entity.Property(e => e.PkIdПериодичности).HasColumnName("PK_id_периодичности");
            entity.Property(e => e.Название)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ПозицияЗаписи>(entity =>
        {
            entity.HasKey(e => e.PkIdПозиции).HasName("PK__Позиция___B17B9FAB4287C00F");

            entity.ToTable("Позиция_записи");

            entity.Property(e => e.PkIdПозиции).HasColumnName("PK_id_позиции");
            entity.Property(e => e.FkIdКатегорииОтслеж).HasColumnName("FK_id_категорииОтслеж");
            entity.Property(e => e.FkIdПолучателя).HasColumnName("FK_id_получателя");
            entity.Property(e => e.FkIdПользователя).HasColumnName("FK_id_пользователя");
            entity.Property(e => e.Активность).HasDefaultValue(true);
            entity.Property(e => e.ДатаСоздания)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Дата_создания");

            entity.HasOne(d => d.FkIdКатегорииОтслежNavigation).WithMany(p => p.ПозицияЗаписиs)
                .HasForeignKey(d => d.FkIdКатегорииОтслеж)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Позиция_Категория");

            entity.HasOne(d => d.FkIdПолучателяNavigation).WithMany(p => p.ПозицияЗаписиs)
                .HasForeignKey(d => d.FkIdПолучателя)
                .HasConstraintName("FK_Позиция_Получатель");

            entity.HasOne(d => d.FkIdПользователяNavigation).WithMany(p => p.ПозицияЗаписиs)
                .HasForeignKey(d => d.FkIdПользователя)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Позиция_Пользователь");
        });

        modelBuilder.Entity<ПолучателиУхода>(entity =>
        {
            entity.HasKey(e => e.PkIdПолучателя).HasName("PK__Получате__3BF6F6A95D6C983C");

            entity.ToTable("Получатели_ухода");

            entity.Property(e => e.PkIdПолучателя).HasColumnName("PK_id_получателя");
            entity.Property(e => e.FkIdПользователя).HasColumnName("FK_id_пользователя");
            entity.Property(e => e.Аватар).IsUnicode(false);
            entity.Property(e => e.Имя).HasMaxLength(100);

            entity.HasOne(d => d.FkIdПользователяNavigation).WithMany(p => p.ПолучателиУходаs)
                .HasForeignKey(d => d.FkIdПользователя)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Получатели_Пользователь");
        });

        modelBuilder.Entity<Пользователь>(entity =>
        {
            entity.HasKey(e => e.PkIdПользователя).HasName("PK__Пользова__1820F2FF37F33746");

            entity.ToTable("Пользователь");

            entity.HasIndex(e => e.Email, "UQ__Пользова__A9D105347604096D").IsUnique();

            entity.Property(e => e.PkIdПользователя).HasColumnName("PK_id_пользователя");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Аватар).IsUnicode(false);
            entity.Property(e => e.ДатаРождения).HasColumnName("Дата_рождения");
            entity.Property(e => e.Логин).HasMaxLength(50);
            entity.Property(e => e.Пол)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Симптомы>(entity =>
        {
            entity.HasKey(e => e.PkIdСимптомы).HasName("PK__Симптомы__2D54504371C7410B");

            entity.ToTable("Симптомы");

            entity.Property(e => e.PkIdСимптомы).HasColumnName("PK_id_симптомы");
            entity.Property(e => e.FkIdПериодичности).HasColumnName("FK_id_периодичности");
            entity.Property(e => e.FkIdПозиции).HasColumnName("FK_id_позиции");
            entity.Property(e => e.ЗаписейВДень).HasColumnName("Записей_в_день");
            entity.Property(e => e.Название).HasMaxLength(200);

            entity.HasOne(d => d.FkIdПериодичностиNavigation).WithMany(p => p.Симптомыs)
                .HasForeignKey(d => d.FkIdПериодичности)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_НапоминаниеСимптомы_Периодичность");

            entity.HasOne(d => d.FkIdПозицииNavigation).WithMany(p => p.Симптомыs)
                .HasForeignKey(d => d.FkIdПозиции)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_НапоминаниеСимптомы_Позиция");
        });

        modelBuilder.Entity<СоставВЛекарстве>(entity =>
        {
            entity.HasKey(e => e.PkIdСоставаВлекарстве).HasName("PK__Состав_в__D168BDFE6FF3540D");

            entity.ToTable("Состав_в_лекарстве");

            entity.Property(e => e.PkIdСоставаВлекарстве).HasColumnName("PK_id_составаВлекарстве");
            entity.Property(e => e.FkIdЛекарства).HasColumnName("FK_id_лекарства");
            entity.Property(e => e.FkIdСоставаЛекарства).HasColumnName("FK_id_составаЛекарства");

            entity.HasOne(d => d.FkIdЛекарстваNavigation).WithMany(p => p.СоставВЛекарствеs)
                .HasForeignKey(d => d.FkIdЛекарства)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_СоставВЛекарстве_Лекарство");

            entity.HasOne(d => d.FkIdСоставаЛекарстваNavigation).WithMany(p => p.СоставВЛекарствеs)
                .HasForeignKey(d => d.FkIdСоставаЛекарства)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_СоставВЛекарстве_Состав");
        });

        modelBuilder.Entity<СоставЛекарства>(entity =>
        {
            entity.HasKey(e => e.PkIdСостава).HasName("PK__Состав_л__40C16011337107FC");

            entity.ToTable("Состав_лекарства");

            entity.Property(e => e.PkIdСостава).HasColumnName("PK_id_состава");
            entity.Property(e => e.НазваниеСоставляющей)
                .HasMaxLength(200)
                .HasColumnName("Название_составляющей");
        });

        modelBuilder.Entity<СпособыПриёма>(entity =>
        {
            entity.HasKey(e => e.PkIdСпособаПриёма).HasName("PK__Способы___885783F9E227FE62");

            entity.ToTable("Способы_приёма");

            entity.HasIndex(e => e.Тип, "UQ__Способы___AC85C275262091CE").IsUnique();

            entity.Property(e => e.PkIdСпособаПриёма).HasColumnName("PK_id_способаПриёма");
            entity.Property(e => e.Тип)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ТипИзмерения>(entity =>
        {
            entity.HasKey(e => e.PkIdТипИзмерения).HasName("PK__Тип_изме__F65889534461FB43");

            entity.ToTable("Тип_измерения");

            entity.HasIndex(e => e.Название, "UQ__Тип_изме__38DA803544BB27E7").IsUnique();

            entity.Property(e => e.PkIdТипИзмерения).HasColumnName("PK_id_ТипИзмерения");
            entity.Property(e => e.FkIdВектор).HasColumnName("FK_id_вектор");
            entity.Property(e => e.ЕдИзмерения)
                .HasMaxLength(50)
                .HasColumnName("ед_измерения");
            entity.Property(e => e.Название)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.FkIdВекторNavigation).WithMany(p => p.ТипИзмеренияs)
                .HasForeignKey(d => d.FkIdВектор)
                .HasConstraintName("FK_Тип_змерение_Вектор");
        });

        modelBuilder.Entity<ФиксацияПриёма>(entity =>
        {
            entity.HasKey(e => e.PkIdФиксПриём).HasName("PK__Фиксация__D865B803BB425CBE");

            entity.ToTable("Фиксация_приёма");

            entity.Property(e => e.PkIdФиксПриём).HasColumnName("PK_id_фиксПриём");
            entity.Property(e => e.FkIdНапоминанияЛекарства).HasColumnName("FK_id_напоминанияЛекарства");
            entity.Property(e => e.ДатаПриёма)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Дата_приёма");

            entity.HasOne(d => d.FkIdНапоминанияЛекарстваNavigation).WithMany(p => p.ФиксацияПриёмаs)
                .HasForeignKey(d => d.FkIdНапоминанияЛекарства)
                .HasConstraintName("FK_Фиксация_НапоминаниеЛекарства");
        });

        modelBuilder.Entity<ЦветИконкиЛекарств>(entity =>
        {
            entity.HasKey(e => e.PkIdЦветИконки).HasName("PK__Цвет_ико__518C3382740EF449");

            entity.ToTable("Цвет_иконки_лекарств");

            entity.HasIndex(e => e.Цвет, "UQ__Цвет_ико__8447BADDE49ADD5A").IsUnique();

            entity.Property(e => e.PkIdЦветИконки).HasColumnName("PK_id_ЦветИконки");
            entity.Property(e => e.Цвет)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

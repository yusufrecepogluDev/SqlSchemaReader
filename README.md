---

# DbModelGenerator / English Version

[🇹🇷 Türkçe versiyona geçmek için tıklayın](#turkce-versiyon)

`DbModelGenerator` reads tables, columns, stored procedures and relationships from a SQL Server database and generates ready-to-use C# model classes and an Entity Framework Core `DbContext`.

## Highlights

- Targets: .NET 9
- Generates model classes, procedure result/parameter models and an EF Core `DbContext`.
- Simple service class generator available (`ServicesGenarator.cs`) that creates basic CRUD skeletons.
- Handles primary key detection, nullable columns, string lengths, navigation properties for foreign keys, unique indexes and delete behaviors.

## Requirements

- .NET 9 SDK
- SQL Server database
- (Optional) Entity Framework Core to use the generated `AppDbContext`

## Quick usage

```csharp
using DbModelGenerator;

var generator = new DbModelGenerator("SERVER_NAME", "DATABASE_NAME");

// Generate table models
generator.tableModelGenerator(@"C:\Output\Models", "MyApp.Models");

// Generate procedure models
generator.ProsedurModelGenerator(@"C:\Output\Procedures", "MyApp.ProcedureModels");

// Generate DbContext
generator.DBContextGenerator(@"C:\Output\DbContext", "MyApp.Data");

// Or generate all (multiple overloads exist)
generator.GenerateAll(@"C:\Output", "MyApp.Models");
```

Notes:
- Generated files overwrite existing files with the same name.
- Service classes can be generated using `ServicesGenarator` (produces simple CRUD stubs).

## Project structure (important files)

- `DbModelGenerator.cs` — main generator logic
- `ServicesGenarator.cs` — service class generator
- `Utils/GetSql.cs` — handles schema queries and logging
- `TypeMapper.cs`, `NameEditor.cs`, `EnglishInflector.cs` — helpers for type/name conversion

---

## Türkçe Versiyon

<a id="turkce-versiyon"></a>

# DbModelGenerator

`DbModelGenerator`, bir SQL Server veritabanındaki tabloları, sütunları, prosedürleri ve ilişkileri okuyarak kullanıma hazır C# model sınıfları ve bir Entity Framework Core `DbContext` üretir.

## Öne çıkanlar

- Hedef: .NET 9
- Model sınıfları, prosedür sonuç/parametre modelleri ve EF Core `DbContext` oluşturma
- `ServicesGenarator.cs` ile basit CRUD servis iskeleti üretme
- Birincil anahtar tespiti, nullable sütun işleme, string uzunlukları, foreign key navigasyon property'leri, unique index ve silme davranışlarını yönetme

## Gereksinimler

- .NET 9 SDK
- SQL Server veritabanı
- (İsteğe bağlı) Oluşturulan `AppDbContext` için Entity Framework Core

## Hızlı kullanım

```csharp
using DbModelGenerator;

var generator = new DbModelGenerator("SERVER_NAME", "DATABASE_NAME");

// Tablo modelleri oluştur
generator.tableModelGenerator(@"C:\Output\Models", "MyApp.Models");

// Prosedür modelleri oluştur
generator.ProsedurModelGenerator(@"C:\Output\Procedures", "MyApp.ProcedureModels");

// DbContext oluştur
generator.DBContextGenerator(@"C:\Output\DbContext", "MyApp.Data");

// Tümünü oluştur (birden fazla overload mevcut)
generator.GenerateAll(@"C:\Output", "MyApp.Models");
```

Notlar:
- Oluşturulan dosyalar aynı isimdeki mevcut dosyaların üzerine yazılır.
- Servis sınıfları `ServicesGenarator` ile basit iskelet olarak üretilebilir.

## Önemli dosyalar

- `DbModelGenerator.cs` — ana jeneratör
- `ServicesGenarator.cs` — servis sınıfı jeneratörü
- `Utils/GetSql.cs` — şema sorguları ve loglama
- `TypeMapper.cs`, `NameEditor.cs`, `EnglishInflector.cs` — yardımcılar

---

Repository: https://github.com/yusufrecepogluDev/SqlSchemaReader

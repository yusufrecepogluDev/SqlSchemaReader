---

# DbModelGenerator / English Version

[🇹🇷 Türkçe versiyona geçmek için tıklayın](#turkce-versiyon)

DbModelGenerator is a tool that reads tables, columns, stored procedures, and relationships from a SQL Server database and automatically generates C# class models and a `DbContext`. It produces classes ready for use with Entity Framework Core.

---

## Features

* Generate C# class models from database tables
* Generate result and parameter models from stored procedures
* Detect foreign key relationships and integrate them into `DbContext`
* Correctly handle nullable columns and map SQL types to C#
* Automatically write generated files to a specified folder

---

## Requirements

* .NET 6 or later
* SQL Server database
* Entity Framework Core (for DbContext usage)

---

## Installation

1. Clone the repository:

```bash
git clone https://github.com/YOUR_USERNAME/DbModelGenerator.git
```

2. Open and build the project in Visual Studio or via the `dotnet CLI`.

---

## Usage

```csharp
using DbModelGenerator;

var generator = new DbModelGenerator("SERVER_NAME", "DATABASE_NAME");

// Generate table models
generator.TabloModelGenerator(@"C:\\Models", "MyApp.Models");

// Generate stored procedure models
generator.ProsedurModelGenerator(@"C:\\Models", "MyApp.Models");

// Generate DbContext
generator.DBContextGenerator(@"C:\\Models", "MyApp.Models");
```

### Parameters

| Parameter    | Description                                     |
| ------------ | ----------------------------------------------- |
| `server`     | SQL Server name or endpoint                     |
| `database`   | Target database name                            |
| `klasorYolu` | Folder path where generated files will be saved |
| `_namespace` | Namespace for generated C# classes              |

---

## Structure

* `DbModelGenerator.cs` → Main class and methods (Table, Procedure, DbContext generation)
* `Utils/GetSql.cs` → Handles SQL Server connection and data retrieval
* `Models/` → Folder where generated models are stored
* `TypeMapper.cs` → Converts SQL types to C# types
* `NameEditor.cs` → Handles naming conventions (PascalCase, plural/singular)
* `EnglishInflector.cs` → Plural/singular conversion for English

---

## Example Output

```csharp
namespace MyApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
```

---

## Error Handling

* SQL connection errors or missing tables are printed to the console.
* Existing files will be overwritten when generating new ones.

---

## Contribution

* Fork the repository, make changes, and submit a pull request.
* Report issues or suggestions via the Issues tab.

---

## License

No specific license is assigned for this project. You do not need the author's permission to use, copy, or distribute it.
Just give it a star!


---

## Türkçe Versiyon

<a id="turkce-versiyon"></a>

# DbModelGenerator

DbModelGenerator, bir SQL Server veritabanındaki tabloları, sütunları, prosedürleri ve ilişkileri okuyarak otomatik olarak C# sınıf modelleri ve `DbContext` oluşturmanıza yardımcı olan bir araçtır. Entity Framework Core ile kullanıma hazır sınıflar üretir.

---

## Özellikler

* Tabloları okuyup C# sınıf modelleri oluşturma
* Prosedürleri okuyup hem sonuç hem de parametre modelleri oluşturma
* Foreign key ilişkilerini tanıyıp `DbContext`’e ekleme
* Nullable sütunları ve veri tiplerini doğru şekilde C# karşılığına çevirme
* Dosyaları belirtilen klasöre otomatik yazma

---

## Gereksinimler

* .NET 6 veya üzeri
* SQL Server veritabanı
* Entity Framework Core (DbContext kullanımı için)

---

## Kurulum

1. Projeyi klonlayın:

```bash
git clone https://github.com/YOUR_USERNAME/DbModelGenerator.git
```

2. Projeyi Visual Studio veya `dotnet CLI` ile açın ve derleyin.

---

## Kullanım

```csharp
using DbModelGenerator;

var generator = new DbModelGenerator("SERVER_NAME", "DATABASE_NAME");

// Model sınıfları oluştur
generator.TabloModelGenerator(@"C:\\Models", "MyApp.Models");

// Prosedür modelleri oluştur
generator.ProsedurModelGenerator(@"C:\\Models", "MyApp.Models");

// DbContext oluştur
generator.DBContextGenerator(@"C:\\Models", "MyApp.Models");
```

### Parametreler

| Parametre    | Açıklama                                           |
| ------------ | -------------------------------------------------- |
| `server`     | SQL Server adı veya bağlantı noktası               |
| `database`   | Hedef veritabanı adı                               |
| `klasorYolu` | Oluşturulacak dosyaların kaydedileceği klasör yolu |
| `_namespace` | Oluşturulacak C# sınıfları için namespace          |

---

## Yapısı

* `DbModelGenerator.cs` → Ana sınıf ve metotlar (Tablo, Prosedür, DbContext oluşturma)
* `Utils/GetSql.cs` → SQL Server ile bağlantı ve veri çekme işlemleri
* `Models/` → Üretilen modellerin saklanacağı klasör
* `TypeMapper.cs` → SQL tiplerini C# tiplerine dönüştürür
* `NameEditor.cs` → İsimlendirme düzenlemeleri (PascalCase, çoğul, tekil vb.)
* `EnglishInflector.cs` → İngilizce çoğul/tekil çevirici

---

## Örnek Çıktı

```csharp
namespace MyApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
```

---

## Hata Yönetimi

* SQL bağlantı hataları veya eksik tablolar konsola yazdırılır.
* Oluşturulacak dosyalar yazılmadan önce mevcutsa üzerine yazılır.

---

## Katkıda Bulunma

* Fork’layın, değişiklik yapın ve pull request gönderin.
* Hataları veya iyileştirme önerilerini Issues sekmesinden bildirin.

---

## Lisans

Bu proje için özel bir lisans belirtilmemiştir. Kullanım, kopyalama veya dağıtım için proje sahibinden izin almanız gerekmektedir.
Yıldızlamanız yeterlidir!
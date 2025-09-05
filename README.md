# DbModelGenerator

DbModelGenerator, bir SQL Server veritabanındaki tabloları, sütunları, prosedürleri ve ilişkileri okuyarak otomatik olarak C# sınıf modelleri ve `DbContext` oluşturmanıza yardımcı olan bir araçtır. Entity Framework Core ile kullanıma hazır sınıflar üretir.

---

## Özellikler

- Tabloları okuyup C# sınıf modelleri oluşturma
- Prosedürleri okuyup hem sonuç hem de parametre modelleri oluşturma
- Foreign key ilişkilerini tanıyıp `DbContext`’e ekleme
- Nullable sütunları ve veri tiplerini doğru şekilde C# karşılığına çevirme
- Dosyaları belirtilen klasöre otomatik yazma

---

## Gereksinimler

- .NET 6 veya üzeri
- SQL Server veritabanı
- Entity Framework Core (DbContext kullanımı için)

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
generator.TabloModelGenerator(@"C:\Models", "MyApp.Models");

// Prosedür modelleri oluştur
generator.ProsedurModelGenerator(@"C:\Models", "MyApp.Models");

// DbContext oluştur
generator.DBContextGenerator(@"C:\Models", "MyApp.Models");
```

### Parametreler

| Parametre      | Açıklama |
|----------------|----------|
| `server`       | SQL Server adı veya bağlantı noktası |
| `database`     | Hedef veritabanı adı |
| `klasorYolu`   | Oluşturulacak dosyaların kaydedileceği klasör yolu |
| `_namespace`   | Oluşturulacak C# sınıfları için namespace |

---

## Yapısı

- `DbModelGenerator.cs` → Ana sınıf ve metotlar (Tablo, Prosedür, DbContext oluşturma)  
- `Utils/GetSql.cs` → SQL Server ile bağlantı ve veri çekme işlemleri  
- `Models/` → Üretilen modellerin saklanacağı klasör  
- `TypeMapper.cs` → SQL tiplerini C# tiplerine dönüştürür  
- `NameEditor.cs` → İsimlendirme düzenlemeleri (PascalCase, çoğul, tekil vb.)  
- `EnglishInflector.cs` → İngilizce çoğul/tekil çevirici  

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

- SQL bağlantı hataları veya eksik tablolar konsola yazdırılır.  
- Oluşturulacak dosyalar yazılmadan önce mevcutsa üzerine yazılır.

---

## Katkıda Bulunma

- Fork’layın, değişiklik yapın ve pull request gönderin.  
- Hataları veya iyileştirme önerilerini Issues sekmesinden bildirin.

---

## Lisans

Bu proje için özel bir lisans belirtilmemiştir. Kullanım, kopyalama veya dağıtım için proje sahibinden izin almanız gerekmemektedir.
Yıldızlamanız yeterlidir!

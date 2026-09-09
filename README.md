# Survey App

Kullanıcıların anket oluşturmasına, yönetmesine ve cevaplamasına olanak tanıyan bir web uygulaması. Admin ve User olmak üzere iki rol içerir.

## Canlı Demo

**[surveyapp.ersinelmas.com](https://surveyapp.ersinelmas.com)**

**Demo Admin girişi** (anket/soru/şablon oluşturma dahil her şeyi deneyebilirsiniz; kayıtlı verilerin korunması için silme işlemleri bu hesapta kapalıdır):
- Email: `admin@surveyapp.com`
- Şifre: `GHZxGcNzDlkfWr5kKG9Q`

**Normal kullanıcı**: `/register` sayfasından kendi hesabınızı oluşturup anket doldurma akışını deneyebilirsiniz.

## Kullanılan Teknolojiler

**Backend**
- .NET 8 Web API, Clean Architecture (Core / Application / Infrastructure / API katmanları)
- Entity Framework Core 8 (Code-First, Migrations)
- PostgreSQL (Docker ile çalıştırılır)
- JWT tabanlı kimlik doğrulama (access + refresh token, rotation ile), rol bazlı yetkilendirme (Admin / User)
- BCrypt ile şifre hashleme
- Built-in rate limiting (auth endpoint'lerinde brute-force/spam koruması)
- xUnit + Moq ile unit testler

**Frontend**
- React 18 + TypeScript (Vite)
- Material-UI (MUI), özel bir görsel kimlik (tema/tipografi)
- React Router (routing, korumalı route'lar)
- Axios (JWT interceptor'lı, otomatik token yenileme)
- Context API (auth state yönetimi)

**Deployment**
- Docker (backend + Postgres), Oracle Cloud
- Cloudflare Pages (frontend), Cloudflare (DNS, HTTPS, reverse proxy)
- GitHub Actions ile otomatik deploy (`main`'e push'ta backend ve frontend otomatik güncellenir)

## Mimari Kararlar

- **Clean Architecture**: Core katmanı hiçbir şeye bağımlı değil (sadece entity ve interface'ler). Application katmanı use-case'leri ve DTO'ları içerir, framework/veritabanı detaylarından habersizdir. Infrastructure, EF Core ve dış servis implementasyonlarını barındırır. API katmanı en dışta, controller'lar ve DI konfigürasyonu burada.
- **Repository Pattern**: Her domain nesnesi (User, Survey, Question, AnswerTemplate) için ayrı bir repository interface'i (Core) ve implementasyonu (Infrastructure) var. Application katmanı sadece interface'lerle konuşuyor.
- **DTO ayrımı**: Her CRUD işlemi için ayrı request/response DTO'ları kullanıldı (örn. `CreateXRequest`, `UpdateXRequest`, `XDto`), entity'ler API sınırının dışına hiç çıkmıyor.
- **Veri bütünlüğü**: Kullanılan bir soru veya cevap şablonu silinemez (referential integrity kontrolü uygulama katmanında yapılıyor). Bir anketin kullanıcı ataması güncellenirken, daha önce tamamlanmış atamalar korunuyor, sadece fark eden kayıtlar eklenip/çıkarılıyor.
- **Güvenlik**: JWT access token'lar 60 dakika geçerli; süresi dolunca frontend, refresh token ile sessizce yeni bir token alır (kullanıcı fark etmez), refresh token da geçersizse otomatik çıkış yapılır. Refresh token'lar rotation ile korunur (her kullanımda eskisi geçersiz kılınıp yenisi üretilir).
- **Rol modeli**: Uygulama, içerik sahipliğini rol yerine kullanıcı bazlı sahiplik (`OwnerId`) üzerinden yönetecek şekilde yeniden tasarlanıyor — her kullanıcı kendi anket/soru/şablonunu oluşturur ve yönetir. `IsAdmin` bayrağı artık içerik yetkisiyle değil, sadece platform yönetimiyle (kullanıcı listesi vb.) ilgilidir; tek bir admin hesabı `AdminSeed:*` ortam değişkenleriyle oluşturulur.

## Kurulum ve Çalıştırma (Local Geliştirme)

### Gereksinimler
- .NET 8 SDK
- Node.js 18+
- Docker Desktop

### 1. Veritabanını başlat
```bash
docker compose up -d
```

### 2. Backend secrets'ını ayarla
JWT imzalama anahtarı ve admin seed şifresi kod/appsettings içinde tutulmaz, User Secrets üzerinden okunur. İlk kurulumda bir kere ayarlanması gerekir:
```bash
cd backend/src/SurveyApp.Api
dotnet user-secrets set "Jwt:Key" "<en az 32 karakterlik bir gizli anahtar>"
dotnet user-secrets set "AdminSeed:Password" "<admin kullanıcı için bir şifre>"
```

### 3. Backend'i çalıştır
```bash
cd backend
dotnet ef database update --project src/SurveyApp.Infrastructure --startup-project src/SurveyApp.Api
cd src/SurveyApp.Api
dotnet run
```
Backend `http://localhost:5092` üzerinde ayağa kalkar. Swagger arayüzü: `http://localhost:5092/swagger`

Uygulama ilk açılışta, `AdminSeed:Email` (appsettings.json, varsayılan `admin@surveyapp.com`) ve `AdminSeed:Password` (user secrets) ile otomatik bir admin kullanıcı oluşturur.

### 4. Frontend'i çalıştır
```bash
cd frontend
npm install
npm run dev
```
Frontend `http://localhost:5173` üzerinde ayağa kalkar.

### Testleri çalıştırma
```bash
cd backend
dotnet test tests/SurveyApp.Application.Tests/SurveyApp.Application.Tests.csproj
```

## Kullanım Akışı

1. Admin, önce **Cevap Şablonları** (şık kalıpları, 2-4 şık) tanımlar.
2. Ardından **Sorular**, bir cevap şablonuna bağlanarak oluşturulur.
3. **Anketler**, sorulardan seçilerek ve kullanıcılara atanarak oluşturulur (tarih aralığı ve aktif/pasif durumu ile).
4. Atanan kullanıcılar `/my-surveys` üzerinden aktif anketlerini görür, doldurur.
5. Admin, her anket için doldurma oranını ve soru bazında cevap dağılımını raporlama ekranından izler.

## Proje Yapısı

```
backend/
  src/
    SurveyApp.Core/            → Entity'ler, repository interface'leri
    SurveyApp.Application/     → DTO'lar, servisler (use-case'ler)
    SurveyApp.Infrastructure/  → EF Core, repository implementasyonları, JWT/hash servisleri
    SurveyApp.Api/             → Controller'lar, DI konfigürasyonu, Program.cs
  tests/
    SurveyApp.Application.Tests/ → xUnit + Moq unit testleri
frontend/
  src/
    api/         → axios instance ve endpoint çağrıları
    components/  → paylaşılan UI parçaları (Layout, ProtectedRoute)
    context/     → AuthContext, SnackbarContext (giriş durumu, bildirimler)
    hooks/       → paylaşılan custom hook'lar (useCrudPage)
    pages/       → her route'un sayfa component'i
    types/       → backend DTO'larının TypeScript karşılıkları
```

## Bilinen Sınırlamalar / Zaman Kısıtı Nedeniyle Basit Tutulan Noktalar

- Gizlilik politikası ve hesap silme akışı henüz eklenmedi (halka açık demo için planlanıyor).
- Anket raporlama ekranında filtreleme/arama (proje şartında opsiyonel olarak belirtilmiş) eklenmedi, temel istatistiklerle sınırlı tutuldu.
- Admin kullanıcı yönetimi (kullanıcı listeleme dışında ekleme/silme/rol değiştirme) ayrı bir ekran olarak sunulmadı; kullanıcılar `/register` üzerinden kendileri kayıt oluyor.

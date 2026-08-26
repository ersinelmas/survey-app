# Survey App
 
Kullanıcıların anket oluşturmasına, yönetmesine ve cevaplamasına olanak tanıyan bir web uygulaması. Admin ve User olmak üzere iki rol içerir.
 
## Kullanılan Teknolojiler
 
**Backend**
- .NET 8 Web API, Clean Architecture (Core / Application / Infrastructure / API katmanları)
- Entity Framework Core 8 (Code-First, Migrations)
- PostgreSQL (Docker ile çalıştırılır)
- JWT tabanlı kimlik doğrulama, rol bazlı yetkilendirme (Admin / User)
- BCrypt ile şifre hashleme

**Frontend**
- React 18 + TypeScript (Vite)
- Material-UI (MUI)
- React Router (routing, korumalı route'lar)
- Axios (JWT interceptor'lı)
- Context API (auth state yönetimi)
## Mimari Kararlar
 
- **Clean Architecture**: Core katmanı hiçbir şeye bağımlı değil (sadece entity ve interface'ler). Application katmanı use-case'leri ve DTO'ları içerir, framework/veritabanı detaylarından habersizdir. Infrastructure, EF Core ve dış servis implementasyonlarını barındırır. API katmanı en dışta, controller'lar ve DI konfigürasyonu burada.
- **Repository Pattern**: Her domain nesnesi (User, Survey, Question, AnswerTemplate) için ayrı bir repository interface'i (Core) ve implementasyonu (Infrastructure) var. Application katmanı sadece interface'lerle konuşuyor.
- **DTO ayrımı**: Her CRUD işlemi için ayrı request/response DTO'ları kullanıldı (örn. `CreateXRequest`, `UpdateXRequest`, `XDto`), entity'ler API sınırının dışına hiç çıkmıyor.
- **Veri bütünlüğü**: Kullanılan bir soru veya cevap şablonu silinemez (referential integrity kontrolü uygulama katmanında yapılıyor). Bir anketin kullanıcı ataması güncellenirken, daha önce tamamlanmış atamalar korunuyor, sadece fark eden kayıtlar eklenip/çıkarılıyor.
- **Güvenlik**: JWT token'lar 60 dakika geçerli, süresi dolan/geçersiz token ile yapılan istekler `401` döner ve frontend bunu yakalayıp otomatik olarak kullanıcıyı çıkışa yönlendirir.
## Kurulum ve Çalıştırma
 
### Gereksinimler
- .NET 8 SDK
- Node.js 18+
- Docker Desktop
### 1. Veritabanını başlat
```bash
docker compose up -d
```
 
### 2. Backend'i çalıştır
```bash
cd backend
dotnet ef database update --project src/SurveyApp.Infrastructure --startup-project src/SurveyApp.Api
cd src/SurveyApp.Api
dotnet run
```
Backend `http://localhost:5092` üzerinde ayağa kalkar. Swagger arayüzü: `http://localhost:5092/swagger`
 
Uygulama ilk açılışta otomatik olarak bir admin kullanıcı oluşturur (aşağıdaki giriş bilgilerine bakın).
 
### 3. Frontend'i çalıştır
```bash
cd frontend
npm install
npm run dev
```
Frontend `http://localhost:5173` üzerinde ayağa kalkar.
 
## Giriş Bilgileri
 
**Admin (otomatik oluşturulur):**
- Email: `admin@surveyapp.com`
- Şifre: `Admin123!`
**Normal kullanıcı:** `/register` sayfasından herhangi bir email/şifre ile kayıt olunabilir (varsayılan olarak `User` rolünde oluşturulur).
 
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
frontend/
  src/
    api/         → axios instance ve endpoint çağrıları
    components/  → paylaşılan UI parçaları (Layout, ProtectedRoute)
    context/     → AuthContext (giriş durumu, token yönetimi)
    pages/       → her route'un sayfa component'i
    types/       → backend DTO'larının TypeScript karşılıkları
```
 
## Bilinen Sınırlamalar / Zaman Kısıtı Nedeniyle Basit Tutulan Noktalar
 
- JWT secret key `appsettings.json`'da düz metin olarak tutuluyor; gerçek bir üretim ortamında environment variable veya bir secret manager (Azure Key Vault vb.) kullanılmalı.
- Refresh token mekanizması yok; token süresi dolunca kullanıcı tekrar giriş yapmak zorunda.
- Anket raporlama ekranında filtreleme/arama (proje şartında opsiyonel olarak belirtilmiş) eklenmedi, zaman kısıtı nedeniyle temel istatistiklerle sınırlı tutuldu.
- Admin kullanıcı yönetimi (kullanıcı listeleme dışında ekleme/silme) ayrı bir ekran olarak sunulmadı; kullanıcılar `/register` üzerinden kendileri kayıt oluyor.
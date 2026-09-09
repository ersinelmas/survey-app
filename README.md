# Survey App

Kullanıcıların kendi anket/soru/cevap şablonlarını oluşturup yönettiği ve anketleri hem atanan kullanıcılar hem de (isteğe bağlı) herkese açık bir link üzerinden üyeliksiz kişilerin doldurabildiği bir web uygulaması. İçerik erişimi role değil, **sahipliğe** dayanır: her kullanıcı yalnızca kendi oluşturduğu ve sistemin sunduğu varsayılan içerikleri görür/düzenler.

## Canlı Demo

**[surveyapp.ersinelmas.com](https://surveyapp.ersinelmas.com)**

`/register` sayfasından kendi hesabınızı oluşturup kendi anket/soru/şablonlarınızı oluşturabilir, anketlerinizi atayabilir ya da herkese açık link ile paylaşabilirsiniz. Platform yönetimi yetkisine (`IsAdmin`) sahip bir hesap sistemde mevcuttur ancak bilgileri artık herkese açık paylaşılmamaktadır.

## Kullanılan Teknolojiler

**Backend**
- .NET 8 Web API, Clean Architecture (Core / Application / Infrastructure / API katmanları)
- Entity Framework Core 8 (Code-First, Migrations)
- PostgreSQL (Docker ile çalıştırılır)
- JWT tabanlı kimlik doğrulama (access + refresh token, rotation ile), sahiplik bazlı yetkilendirme (`OwnerId` + platform yönetimi için `IsAdmin` bayrağı)
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
- **Sahiplik modeli**: İçerik erişimi role değil, kullanıcı bazlı sahipliğe (`OwnerId`) dayanır — her kullanıcı kendi anket/soru/cevap şablonunu oluşturur, yalnızca kendi içeriğini ve sistemin sunduğu varsayılanları (`OwnerId = null`) görür. Bir varsayılanı değiştirmek isteyen kullanıcı onu "kendime kopyala" ile kendi hesabına klonlar, orijinali etkilemez. `IsAdmin` bayrağı içerik yetkisiyle değil, platform yönetimiyle (kullanıcı listesi, moderasyon amaçlı üçüncü taraf içeriğine müdahale) ilgilidir; tek bir admin hesabı `AdminSeed:*` ortam değişkenleriyle oluşturulur.
- **Herkese açık anketler**: Bir anket sahibi, anketi atama yapmadan da bir link üzerinden herkese açabilir (`IsPublic`). Bu linkten doldurma üyelik gerektirmez; anket sahibi isterse "yanıtlamak için üyelik gerektir" seçeneğini açabilir. Üyeliksiz doldurmada mükerrer gönderim, tarayıcıya özel rastgele bir token ile iyi niyetli olarak sınırlandırılır (garanti değildir — bu, sektördeki benzer araçların da yaklaşımıdır).

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

1. Kullanıcı önce **Cevap Şablonları** (şık kalıpları, 2-4 şık) tanımlar — kendi şablonunu oluşturabilir veya sistemin sunduğu varsayılanlardan birini "kendime kopyala" ile alıp özelleştirebilir.
2. Ardından **Sorular**, bir cevap şablonuna bağlanarak oluşturulur (aynı şekilde kendi sorusu veya kopyalanmış bir varsayılan).
3. **Anketler**, sorulardan seçilerek oluşturulur (tarih aralığı ve aktif/pasif durumu ile). İki dağıtım yolu vardır:
   - **Atama**: belirli kullanıcılar seçilip ankete atanır, onlar `/my-surveys` üzerinden aktif anketlerini görüp doldurur.
   - **Herkese açık link**: anket sahibi "herkese açık link ile paylaş"ı açar, oluşan linki (`/public/surveys/{id}`) paylaşır; üyelik gerekmez (sahibi isterse zorunlu tutabilir).
4. Anket sahibi, her anket için doldurma oranını ve soru bazında cevap dağılımını raporlama ekranından izler (herkese açık anketlerde cevaplayan kişi giriş yapmamışsa "Anonim" olarak görünür).

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

- Hesap silme akışı henüz eklenmedi (KVKK "unutulma hakkı" için planlanıyor).
- Anket raporlama ekranında filtreleme/arama eklenmedi, temel istatistiklerle sınırlı tutuldu. Ayrıca herkese açık anketlerde tamamlanma oranı özeti (halka/sayaç) sadece atama bazlı katılımcıları sayar; anonim/üyeliksiz cevaplar soru bazlı dökümde görünür ama üstteki özet sayılara yansımaz.
- Cevap şablonlarında şık sayısı 2-4 ile sınırlı ve tek soru tipi (tek seçim) desteklenir; serbest metin, çoklu seçim, ölçek gibi soru tipleri planlanıyor.
- Admin kullanıcı yönetimi (tam liste dışında ekleme/silme) ayrı bir ekran olarak sunulmadı; kullanıcılar `/register` üzerinden kendileri kayıt oluyor. Anket ataması için kullanıcı seçimi, tüm kullanıcı dizinini ifşa etmemek adına en az 3 karakterlik email araması (`/api/users/search`, en fazla 10 sonuç) üzerinden yapılır; tam liste (`/api/users`) sadece platform admin'e açıktır.

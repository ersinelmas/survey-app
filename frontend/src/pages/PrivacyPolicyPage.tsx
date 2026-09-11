import { useNavigate } from 'react-router-dom';
import { Box, Container, Paper, Typography, Divider } from '@mui/material';
import BackButton from '../components/BackButton';

function PrivacyPolicyPage() {
    const navigate = useNavigate();

    return (
        <Container maxWidth="md" sx={{ py: { xs: 3, sm: 5 } }}>
            <BackButton label="Geri Dön" onClick={() => navigate(-1)} />

            <Paper sx={{ p: { xs: 3, sm: 5 } }}>
                <Typography variant="h5" sx={{ mb: 1 }}>
                    Gizlilik Politikası ve Aydınlatma Metni
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                    Son güncelleme: 11 Eylül 2026
                </Typography>

                <Divider sx={{ mb: 3 }} />

                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                    <Box>
                        <Typography variant="h6" sx={{ mb: 1 }}>1. Veri Sorumlusu</Typography>
                        <Typography variant="body2">
                            Bu uygulama Ersin Elmas tarafından geliştirilmiş ve işletilmektedir. 6698 sayılı Kişisel
                            Verilerin Korunması Kanunu ("KVKK") kapsamında veri sorumlusu Ersin Elmas'tır.
                        </Typography>
                    </Box>

                    <Box>
                        <Typography variant="h6" sx={{ mb: 1 }}>2. Toplanan Kişisel Veriler</Typography>
                        <Typography variant="body2">
                            Hesap oluşturduğunuzda email adresiniz ve şifrenizin geri döndürülemez şekilde
                            şifrelenmiş (hash'lenmiş) hâli saklanır; şifrenizin okunabilir hâli hiçbir yerde
                            tutulmaz. Ayrıca size atanan anketlere verdiğiniz cevaplar, hesabınızla ilişkilendirilmiş
                            şekilde saklanır. Bazı anket soruları serbest metin şeklinde açık uçlu cevap
                            isteyebilir; bu tür sorulara yazacağınız cevapların içeriği tamamen size bağlıdır — bu
                            alanlara isim, iletişim bilgisi gibi ek kişisel veriler yazmamanızı öneririz.
                        </Typography>
                    </Box>

                    <Box>
                        <Typography variant="h6" sx={{ mb: 1 }}>3. Verilerin İşlenme Amacı</Typography>
                        <Typography variant="body2">
                            Toplanan veriler yalnızca hesabınızın oluşturulması ve kimlik doğrulaması, size
                            atanan anketlerin sunulması, verdiğiniz cevapların kaydedilmesi ve anket sonuçlarının
                            anket sahibine raporlanması amacıyla işlenir.
                        </Typography>
                    </Box>

                    <Box>
                        <Typography variant="h6" sx={{ mb: 1 }}>4. Saklama ve Güvenlik</Typography>
                        <Typography variant="body2">
                            Şifreniz endüstri standardı bir algoritma (BCrypt) ile şifrelenerek saklanır. Oturum
                            açma denemeleri hız sınırlamasıyla korunur ve art arda başarısız denemelerde hesabınız
                            geçici olarak kilitlenir. Veriler Oracle Cloud altyapısında barındırılan sunucularda
                            saklanır ve trafiğiniz Cloudflare üzerinden şifreli (HTTPS) bağlantıyla iletilir.
                        </Typography>
                    </Box>

                    <Box>
                        <Typography variant="h6" sx={{ mb: 1 }}>5. Üçüncü Taraflarla Paylaşım</Typography>
                        <Typography variant="body2">
                            Kişisel verileriniz üçüncü taraflarla pazarlama amacıyla paylaşılmaz veya satılmaz.
                            Uygulama, altyapı hizmeti olarak Cloudflare (DNS, HTTPS, ağ koruması) ve Oracle Cloud
                            (sunucu barındırma) kullanır; sayfa yazı tiplerinin yüklenmesi için Google Fonts
                            servisine bağlanılır. Bu servisler yalnızca teknik altyapı sağlayıcısı olarak
                            kullanılır, herhangi bir reklam/analitik takip aracı kullanılmamaktadır.
                        </Typography>
                    </Box>

                    <Box>
                        <Typography variant="h6" sx={{ mb: 1 }}>6. Haklarınız</Typography>
                        <Typography variant="body2">
                            KVKK'nın 11. maddesi kapsamında; verilerinizin işlenip işlenmediğini öğrenme,
                            işlenmişse buna ilişkin bilgi talep etme, işlenme amacını öğrenme, yurt içinde veya
                            yurt dışında aktarıldığı üçüncü kişileri bilme, eksik/yanlış işlenmişse düzeltilmesini
                            isteme ve verilerinizin silinmesini/yok edilmesini talep etme haklarına sahipsiniz.
                        </Typography>
                        <Typography variant="body2" sx={{ mt: 1.5 }}>
                            Şifrenizi değiştirme ve hesabınızı silme işlemlerini "Hesabım" sayfası üzerinden,
                            email ile talepte bulunmanıza gerek kalmadan doğrudan kendiniz gerçekleştirebilirsiniz.
                            Hesabınızı sildiğinizde; sahibi olduğunuz anketler, sorular ve cevap şablonları
                            kalıcı olarak silinir (bu anketlere başka kullanıcıların verdiği cevaplar da bu
                            silme işlemiyle birlikte kaldırılır). Başka bir kullanıcının anketine sizin verdiğiniz
                            cevaplar ise anketin bütünlüğünü korumak amacıyla silinmez; bunun yerine hesabınızla
                            olan bağlantısı koparılarak sizinle ilişkilendirilemez hâle getirilir (anonimleştirilir).
                        </Typography>
                        <Typography variant="body2" sx={{ mt: 1.5 }}>
                            Hakkınızda işlenen verilerin bir kopyasını "Hesabım" sayfasındaki "Verilerimi İndir"
                            butonuyla JSON formatında indirebilirsiniz. Bunun dışındaki taleplerinizi (düzeltme,
                            itiraz vb.) aşağıdaki iletişim adresi üzerinden bize iletebilirsiniz.
                        </Typography>
                    </Box>

                    <Box>
                        <Typography variant="h6" sx={{ mb: 1 }}>7. İletişim</Typography>
                        <Typography variant="body2">
                            Sorularınız veya haklarınıza ilişkin talepleriniz için{' '}
                            <a href="mailto:contact@ersinelmas.com">contact@ersinelmas.com</a> adresinden bize
                            ulaşabilirsiniz.
                        </Typography>
                    </Box>
                </Box>
            </Paper>
        </Container>
    );
}

export default PrivacyPolicyPage;

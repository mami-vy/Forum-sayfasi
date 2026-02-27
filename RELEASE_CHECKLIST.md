# Yayin Oncesi Kontrol Listesi

## Yasal ve Gizlilik

- [ ] Gizlilik Politikasi, Kullanim Kosullari, Cerez Politikasi ve Aydinlatma Metni hukuk danismani tarafindan onaylandi.
- [ ] Kayit ekranindaki yasal onay metni guncel.
- [ ] Veri saklama sureleri ve silme proseduru dokumante edildi.

## Guvenlik

- [ ] Production ortaminda HTTPS ve HSTS aktif.
- [ ] Uygulama cookie ayarlari `HttpOnly`, `Secure`, `SameSite` olarak dogrulandi.
- [ ] CSP ve diger response security header'lari canli ortamda test edildi.
- [ ] Admin kullanicilari icin guclu parola politikasi ve MFA planlandi.

## Operasyon

- [ ] `ASPNETCORE_ENVIRONMENT=Production` ile smoke test yapildi.
- [ ] Veritabani migration'lari canliya alinmadan once yedek alindi.
- [ ] Loglama, hata izleme ve alarm mekanizmasi aktif.
- [ ] 500/404 hata sayfalari ve geri donus senaryolari test edildi.

## Fonksiyonel

- [ ] Kullanici ekle/duzenle/sil, rol atama, aktif-pasif, engelleme akislari test edildi.
- [ ] Giris gecmisi ve admin bildirimleri test edildi.
- [ ] Tema ayarlari degisince tum sayfalarda gorunum dogrulandi.

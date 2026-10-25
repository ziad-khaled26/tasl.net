# CinemaBooking

تطبيق تعليمي كامل باستخدام ASP.NET Core MVC وC# وRazor وEF Core وSQL Server. يحتوي الحل على مشروع ويب واحد باسم `CinemaBooking` ويستهدف `net10.0` مثل المشروع الأصلي. الواجهة بالإنجليزية والأسعار بالجنيه المصري EGP.

## التشغيل السريع

المطلوب: .NET 10 SDK، وVisual Studio يدعم .NET 10 مع حزمة تطوير ASP.NET، وSQL Server يمكن لمستخدم Windows الحالي الاتصال به وإنشاء قاعدة تطوير عليه. لا يحتاج تشغيل التطبيق إلى Node أو Gulp أو Python أو مفاتيح API.

الاتصال الافتراضي في `CinemaBooking/appsettings.json`:

```json
"ConnectionStrings": {
  "CinemaDb": "Server=.;Database=CinemaBooking_Development;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=False"
}
```

تم التحقق محليًا من SQL Server الافتراضي `.`. القاعدة المخصصة هي `CinemaBooking_Development`. لا تغير الاسم إلى قاعدة تخص مشروعًا آخر. `TrustServerCertificate=True` ملائم لاتصال التطوير المحلي المعتمد هنا.

من جذر الحل:

```powershell
dotnet restore CinemaBooking.slnx
dotnet tool restore
dotnet ef database update --project CinemaBooking --startup-project CinemaBooking --context CinemaDbContext
dotnet run --project CinemaBooking --launch-profile http
```

افتح `http://localhost:5207`. الإدارة متاحة من رابط **Administration** أو `http://localhost:5207/Admin`.

في Visual Studio افتح `CinemaBooking.slnx`، واجعل `CinemaBooking` مشروع بدء التشغيل، ثم اختر ملف التشغيل `http`. قبل التشغيل لأول مرة نفذ في Package Manager Console:

```powershell
Update-Database -Project CinemaBooking -StartupProject CinemaBooking -Context CinemaDbContext
```

إذا كنت تستخدم LocalDB بدل الخادم الافتراضي، غيّر الاتصال في `appsettings.Development.json` مع الحفاظ على اسم قاعدة مستقل:

```json
"ConnectionStrings": {
  "CinemaDb": "Server=(localdb)\\MSSQLLocalDB;Database=CinemaBooking_Development;Trusted_Connection=True;TrustServerCertificate=True"
}
```

يمكن أيضًا ضبط `ConnectionStrings__CinemaDb` كمتغير بيئة. تقرأ `CinemaDbContextFactory` ملف `appsettings.json` ثم ملف البيئة ومتغيراتها؛ تستخدم Development افتراضيًا لأداة EF، أو قيمة `ASPNETCORE_ENVIRONMENT` عند تحديدها. تأكد من استخدام البيئة والاتصال نفسيهما في أداة EF والتطبيق.

## المجلدات

```text
CinemaBooking.slnx
CinemaBooking/
  Models/                  الكيانات وعلاقاتها
  ViewModels/              نماذج إدخال وعرض البيانات والتحقق
  Data/                    DbContext وSeedData وDesign-time factory
    Migrations/            InitialCreate وملف ModelSnapshot
  Services/                قواعد الحجز والعروض والصور والتوقيت
  Controllers/             وحدات تحكم الزوار
  Views/                   صفحات الزوار والتخطيطات والأجزاء المشتركة
  Areas/Admin/Controllers/ وحدات تحكم الإدارة
  Areas/Admin/Views/       صفحات الإدارة والنماذج والحذف
  wwwroot/
    falcon/                الأصول المجمعة المختارة من القالب
    fonts/                 خطوط محلية وتراخيصها
    images/                صور تجريبية بديلة محلية
    uploads/               صور الإدارة بأسماء مولدة
    css/                   تنسيقات التطبيق والطباعة والاستجابة
    js/                    التفاعلات البسيطة وحساب المعاينة
Reference/                 ZIP الأصلي والقالب المستخرج للفحص
tests/                     اختبارات HTTP وSQL والمتصفح
tools/                     مولد الصور التجريبية
artifacts/verification/    نتائج الاختبارات ولقطات الشاشة والتذكرة PDF
```

لا توجد طبقة API أو SPA أو مستودعات عامة. تستخدم وحدات التحكم حقن الاعتماديات وعمليات EF غير المتزامنة، بينما تتولى الخدمات القواعد التي تحتاج أكثر من إجراء بسيط على البيانات.

## الهجرات

هجرة `InitialCreate` وملف الـ snapshot موجودان بالفعل وتم توليدهما بأداة EF، وطُبقت الهجرة محليًا. الإعداد الأول المعتاد هو **Update-Database** وليس إنشاء هجرة أخرى بنفس اسم `InitialCreate`. لا يستخدم التطبيق `EnsureCreated` ولا يطبق الهجرات تلقائيًا عند التشغيل؛ يطلب تطبيقها إذا وجد تغييرات معلقة.

أوامر التوليد الأصلية، للتوثيق فقط عند بدء تاريخ هجرات جديد في نسخة نظيفة:

```powershell
Add-Migration InitialCreate -Project CinemaBooking -StartupProject CinemaBooking -Context CinemaDbContext -OutputDir Data/Migrations
dotnet ef migrations add InitialCreate --project CinemaBooking --startup-project CinemaBooking --context CinemaDbContext --output-dir Data/Migrations
```

بعد تغيير النماذج مستقبلًا، اختر اسمًا يصف التغيير:

```powershell
Add-Migration AddMovieLanguage -Project CinemaBooking -StartupProject CinemaBooking -Context CinemaDbContext -OutputDir Data/Migrations
Update-Database -Project CinemaBooking -StartupProject CinemaBooking -Context CinemaDbContext
```

أو من CLI:

```powershell
dotnet ef migrations add AddMovieLanguage --project CinemaBooking --startup-project CinemaBooking --context CinemaDbContext --output-dir Data/Migrations
dotnet ef database update --project CinemaBooking --startup-project CinemaBooking --context CinemaDbContext
dotnet ef migrations list --project CinemaBooking --startup-project CinemaBooking --context CinemaDbContext
dotnet ef migrations has-pending-model-changes --project CinemaBooking --startup-project CinemaBooking --context CinemaDbContext
```

اسم `AddMovieLanguage` مثال فقط؛ لا تنفذه قبل وجود تعديل حقيقي في النموذج.

## البيانات التجريبية والصور

تضيف `SeedData` ثلاث سينمات وستة أفلام وثمانية ممثلين وعلاقات طاقم التمثيل و18 عرضًا موزعة على الأيام الثلاثة التالية لتاريخ التهيئة. جميع الأسماء والأعمال المعروضة بيانات خيالية تعليمية. الأسعار تبدأ من 120 EGP.

التهيئة قابلة للتكرار: تتحقق من السجلات الموجودة، ولا تعيد إضافة البيانات نفسها أو تستبدل تعديلات الإدارة. إذا انتهت جميع عروض فيلم تجريبي، تضيف عروضًا قادمة عند التشغيل التالي. تحمي معاملة وقفل `sp_getapplock` التهيئة من التشغيل المتزامن.

تعمل التهيئة عند بدء التطبيق بعد تطبيق الهجرات. يمكن تعطيلها بضبط `Database:SeedOnStartup` إلى `false`، أو تشغيلها مرة واحدة دون بدء خادم:

```powershell
dotnet run --project CinemaBooking --launch-profile http -- --seed
```

الصور الـ17 رسومات بديلة أصلية وموسومة بوضوح بأنها **PLACEHOLDER**، وليست صور ممثلين حقيقيين أو ملصقات أفلام مرخصة. هي مضمنة محليًا، ولا توجد خدمة صور خارجية مطلوبة. يمكن استبدالها من صفحات الإدارة بصور تملك حق استخدامها. تفاصيل المصادر والتراخيص في [ASSET_SOURCES.md](ASSET_SOURCES.md).

## مسار الزائر

1. الصفحة الرئيسية تعرض أعداد السينمات والأفلام والممثلين من قاعدة البيانات وبطاقات الأفلام.
2. من Movies يمكن البحث بالعنوان والتصفية بالنوع والسينما التي لديها عروض قادمة.
3. تفاصيل الفيلم تعرض الوصف والمدة وطاقم التمثيل والعروض القادمة. صفحات السينمات تعرض عناوينها وبرامجها.
4. يختار الزائر عرضًا، ثم يدخل الاسم ورقم الهاتف وعدد التذاكر من 1 إلى 10.
5. يعرض النموذج السعر والتذاكر المتبقية ومعاينة الإجمالي. يحسب الخادم السعر النهائي من قاعدة البيانات.
6. بعد نجاح POST يتم التحويل إلى صفحة التأكيد، التي تعرض رمز حجز عشوائيًا وتذكرة قابلة للطباعة أو الحفظ PDF من المتصفح.

رابط التأكيد يعتمد على رمز عشوائي 128-bit وليس رقم الحجز المتسلسل. من يمتلك هذا الرابط يمكنه عرض التذكرة نفسها؛ احتفظ به بشكل خاص. لا توجد قائمة حجوزات عامة أو استرجاع عبر رقم هاتف. رقم الهاتف متاح في الإدارة فقط ولا يظهر في التذكرة العامة.

## قواعد الحجز والتوقيت

- كل `Showtime` عرض مستقل له فيلم وسينما وموعد وسعر وسعة. لا توجد مقاعد مرقمة أو مدفوعات أو حسابات مستخدمين.
- تخزن `StartAt` كوقت القاهرة المحلي في `datetime2`، وتعرض جميع المواعيد على هذا الأساس، باستخدام منطقة `Egypt Standard Time` في `CairoClock`. تُرفض الأوقات غير الموجودة أو الملتبسة عند انتقال التوقيت الصيفي.
- تخزن `CreatedUtc` باستخدام `DateTime.UtcNow` وتعرض في التذكرة مع علامة UTC. لا تحوّلها إلى وقت القاهرة ثم تخزنها كـ UTC.
- داخل معاملة SQL، تقرأ `BookingService` صف العرض باستخدام `UPDLOCK, HOLDLOCK`، ثم تحسب الكمية المحجوزة وتتحقق من الموعد والسعة وتحفظ الحجز قبل إنهاء المعاملة. الطلبات المتنافسة لنفس العرض تنتظر القفل نفسه، حتى عبر أكثر من نسخة من التطبيق.
- تستخدم الإدارة القفل نفسه عند تعديل عرض أو حذفه. يوجد `rowversion` لكشف نماذج الإدارة القديمة. يمنع هذا سباق تغيير السعة أو السعر مع الحجز.
- رمز النموذج محمي بـ ASP.NET Data Protection، مرتبط بالعرض وصالح 30 دقيقة. تخزن قاعدة البيانات مفتاح إرسال فريدًا. إعادة الطلب نفسه تعيد التأكيد نفسه، وتغيير بيانات طلب سبق تنفيذه يُرفض. بعد انتهاء صلاحية النموذج يلزم إعادة تحميل صفحة الحجز.
- السعر والإجمالي ووقت الإنشاء والرمز وحقول الإدارة لا تؤخذ من POST. توجد فهارس فريدة وقيود تحقق ودقة عشرية صريحة للقيم المالية.
- جميع النماذج المعدلة للبيانات محمية بـ antiforgery على مستوى MVC، وتحتفظ بالقيم والاختيارات عند فشل التحقق.

مفاتيح Data Protection تحفظ باستخدام السلوك الافتراضي لـ ASP.NET Core في ملف المستخدم المحلي. عند نقل التطبيق أو تشغيل عدة نسخ، يلزم مشاركة مخزن مفاتيح ثابت بينها حتى تبقى الرموز والنماذج قابلة للقراءة.

## الإدارة

**الإدارة مفتوحة عمدًا في هذا الإصدار التعليمي. لا توجد مصادقة أو تفويض أو Identity أو حسابات أو أدوار.**

تحتوي على إحصاءات حقيقية، وCRUD للسينمات والممثلين والأفلام والعروض، وتعيين عدة ممثلين للفيلم، والبحث في الحجوزات بالرمز أو الاسم أو الهاتف أو الفيلم مع تصفية السينما وتاريخ العرض وصفحات نتائج.

العرض الذي لديه حجوزات لا يمكن تغيير فيلمه أو سينماه أو موعده أو سعره، ولا خفض سعته عن عدد التذاكر المحجوزة. لا توجد عملية حذف حجوزات. يمنع حذف السينمات والأفلام ذات العروض والممثلين المرتبطين بأفلام، وكذلك العروض المحجوزة. عند حذف فيلم غير مستخدم تزال علاقات طاقمه معه في عملية الحفظ نفسها. صفحة GET للحذف للعرض والتأكيد فقط؛ التنفيذ عبر POST.

رفع الصور يقبل JPEG وPNG وWebP حتى 3 MB وبحد أقصى 6000 بكسل لكل جانب و16 مليون بكسل. يفحص SkiaSharp صيغة الملف الفعلية ويفك الصورة ثم يعيد ترميزها إلى JPEG باسم مولد؛ لا يحتفظ بمحتوى الملف الخام أو بياناته الإضافية. SVG والملفات التنفيذية والمحتوى التالف مرفوضة. ترك خانة الرفع فارغة عند التعديل يبقي الصورة الحالية، والمسار المنشور من المتصفح لا يستبدل مسار قاعدة البيانات. لا يحذف التطبيق الصور القديمة تلقائيًا؛ يمكن تنظيف الملفات غير المشار إليها لاحقًا بعد مراجعة مراجعها.

## Falcon والأصول

استُخرج `Reference/falcon-v3.24.0.zip` محليًا وفُحصت صفحات widgets ولوحة التجارة وبطاقات وتفاصيل المنتجات والنماذج والجداول والطلبات والفاتورة. استُخدمت CSS وJavaScript المجمعة ومكونات Falcon المناسبة، مع تخطيط مستقل للزوار وآخر للإدارة وإزالة تنقلات وأسماء العروض التجريبية غير المرتبطة بالسينما.

يشمل `wwwroot/falcon` الملفات اللازمة للتشغيل فقط. الخطوط التي يشير إليها القالب كانت روابط Google Fonts وليست ملفات خط داخل ZIP؛ أُضيف Open Sans وPoppins محليًا مع ملفات OFL لتعمل الواجهة دون اتصال خارجي. أيقونات Font Awesome من القالب نفسه. لا يلزم تشغيل أدوات بناء Falcon.

## البناء والتحقق

```powershell
dotnet build CinemaBooking.slnx
dotnet build CinemaBooking.slnx -c Release
dotnet list CinemaBooking/CinemaBooking.csproj package --vulnerable --include-transitive
```

اختبارات التدفق تستخدم **قاعدة مستقلة** باسم `CinemaBooking_Verification` وخادمًا على المنفذ 5208. لا تشغّل خادم التطوير المعتاد على هذا المنفذ وقت الاختبارات. الاختبارات تنشئ بيانات اختبار وحجوزات داخل قاعدة التحقق وتبقيها للمراجعة؛ لا تمس حجوزات قاعدة التطوير. يلزم `sqlcmd` مع اتصال Windows بالخادم `.` للاختبارات.

في نافذة PowerShell أولى من جذر الحل:

```powershell
powershell -ExecutionPolicy Bypass -File tests/Start-Verification.ps1
```

وفي نافذة ثانية:

```powershell
python -m pip install -r tests/requirements.txt
python tests/verify_flows.py
python tests/verify_browser.py
```

يستخدم اختبار المتصفح Google Chrome المثبت محليًا عبر Playwright. تتضمن الاختبارات التنقل والتصفية وCRUD وتعيين الممثلين والتحقق من الحقول والصور وحماية POST والحجز والسعر النهائي والتأكيد والطباعة ورفض العروض المنتهية والممتلئة وحماية العروض المحجوزة وكشف التعديلات القديمة ومنع تكرار الإرسال. يرسل اختبار السعة 8 طلبات متزامنة لآخر تذكرة، ثم لآخر تذكرتين، ويتحقق من نتائج SQL الفعلية؛ ويوجد اختبار منفصل لأربعة إرساليات متزامنة للنموذج نفسه.

توجد تقارير JSON ولقطات شاشة سطح المكتب والهاتف وملف تذكرة PDF في `artifacts/verification`. راجع [VERIFICATION_AR.md](VERIFICATION_AR.md) لنتائج التنفيذ المسجلة وحدود التحقق.

## استكشاف مشاكل التشغيل

- إذا فشل اتصال SQL، تحقق من تشغيل خدمة SQL Server ومن صلاحية مستخدم Windows الحالي، أو عدّل الاتصال إلى LocalDB ثم نفذ Update-Database على الاتصال نفسه.
- إذا ظهرت رسالة هجرات معلقة، نفذ أمر تحديث القاعدة المذكور بالأعلى. لا تستبدله بـ EnsureCreated.
- إذا انتهت العروض التجريبية، أعد تشغيل التطبيق أو شغّل `--seed`، أو أضف عروضًا جديدة من الإدارة.
- إذا لم تظهر صور أثناء التعديل، اترك الملف فارغًا للاحتفاظ بالصورة الحالية أو ارفع صيغة مسموحة بالحجم المحدد. يحتاج مستخدم التشغيل صلاحية الكتابة في `wwwroot/uploads`.
- الاختبار المحلي نُفذ على Windows. عند النقل إلى Linux مع SkiaSharp أضف حزمة NativeAssets المناسبة لنظام التشغيل وبالإصدار نفسه، واضبط اتصال SQL ومخزن مفاتيح Data Protection.

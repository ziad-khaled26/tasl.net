# فحص صلاحيات جلسة Codex

تاريخ الفحص: 25 أكتوبر 2026، بتوقيت القاهرة.

| الإعداد | القيمة الفعالة الحالية | المصدر |
|---|---|---|
| `sandbox_mode` | `danger-full-access` | تعليمات الصلاحيات الأحدث المقدمة للجلسة بعد تغيير المستخدم |
| `approval_policy` | `never` | تعليمات الصلاحيات الأحدث المقدمة للجلسة |
| `permission_profile.type` | `disabled` | رسالة `environment_context` الأحدث من العميل |
| `file_system.type` | `unrestricted` | رسالة `environment_context` الأحدث من العميل |
| `windows.sandbox` | `elevated` | `C:/Users/CityLap/.codex/config.toml` |

قبل تغيير الصلاحيات، كانت تعليمات الجلسة تضبط `sandbox_mode=read-only`، وكانت بيئة العميل تعرض `permission_profile.type=managed` ووصول `read` فقط. لذلك استُخدم حينها `sandbox_permissions=require_escalated` لأوامر الكتابة، وظهرت طلبات Allow once. أمر الواجهات ونسخ الأصول الذي كان معلقًا وقت مقاطعة الدور لم يُنفذ؛ تم التأكد من ذلك بمراجعة الملفات، ثم تنفيذ الجزء الناقص فقط.

بعد التغيير لم يُرسل `sandbox_permissions` في أوامر التنفيذ. نجحت كتابة الملفات ونسخ Falcon واستعادة الحزم وتوليد الهجرات وتطبيقها وتشغيل الخادم والمتصفح دون طلب تصعيد جديد.

لم أجد `approval_policy` أو `sandbox_mode` في ملف إعداد المستخدم، ولا ملف `.codex/config.toml` داخل المشروع. لم توجد ملفات `requirements.toml` في المسارين اللذين فُحصا: `C:/ProgramData/OpenAI/Codex/requirements.toml` و`C:/Users/CityLap/.codex/requirements.toml`. هذه نتيجة فحص المسارات المذكورة وليست ادعاءً بفحص كل إعداد إداري أو سحابي ممكن.

`windows.sandbox=elevated` يختار تطبيق sandbox على Windows؛ لا يعني وحده سياسة طلب موافقات. هذا موضح في [مرجع إعدادات OpenAI الرسمي](https://learn.chatgpt.com/docs/config-file/config-reference). القيم الفعلية للجلسة ونجاح أوامر الكتابة دون موافقات هما الدليل هنا على أن اختيار Full access مطبق. لم يلزم تغيير إعدادات Codex أو تجاوز سياسة أخرى.

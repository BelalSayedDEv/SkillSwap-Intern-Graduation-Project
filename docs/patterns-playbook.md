# Backend Patterns Playbook — من SkillSwap للـ HR

> الملف ده ملخص الباترنز اللي اتكررت في مشروع SkillSwap بالأدلة من الكود،
> ومع كل باترن: أمثلة حقيقية + الغلطة المتوقعة + تمرين تعمله بإيدك في مشروع الـ HR قبل ما تكمله.
> اللغة: شرح بالمصري، والمصطلحات التقنية بالإنجليزي زي ما هي.

إحصائيات من الكود نفسه:
- `PagedResult` مذكورة 37 مرة في الـ Application
- `User.GetUserId()` مستخدمة 25 مرة في الكنترولرز
- `CompleteAsync` مستخدمة 24 مرة
- `Exists*Async` pre-checks موجودة 12 مرة
- Validation بتاعة الـ Paging مكررة بالنص في 7 أماكن
- `Enum.IsDefined` مكررة 4 مرات

---

## 1. الـ Pagination (أكتر حاجة اتكررت)

### فين؟
كل ليستة في المشروع نفس الشكل: `Count + Skip/Take + Map + PagedResult`.
أمثلة حقيقية:

```csharp
// SkillSwap.Application/Skills/Services/SkillCatalogService.cs
private static Result<PagedResult<SkillDto>>? NormalizePaging(int page, int pageSize)
{
    if (page < 1)
        return Result<PagedResult<SkillDto>>.Failure(ErrorType.Validation, "Page must be >= 1.");
    if (pageSize is < 1 or > 50)
        return Result<PagedResult<SkillDto>>.Failure(ErrorType.Validation, "PageSize must be between 1 and 50.");
    return null;
}
```

```csharp
// SkillSwap.Infrastructure/Persistence/Repositories/SkillRepositories.cs
var total = await query.CountAsync(cancellationToken);
var items = await query.OrderBy(s => s.Name)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
return new PagedResult<Skill> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
```

```csharp
// SkillSwap.Application/Common/Models/PagedResult.cs
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

نفس رسالتي الـ Validation بالنص متكررة في: الكتالوج، الـ Pending، البلاغات، طلبات التبادل (7 أماكن).

### الغلطة اللي هتقع فيها في الـ HR
- `page=0` يعدي ويعمل `Skip` بالسالب.
- `pageSize=100000` يموت الـ DB.
- تنسى الـ `Count` بعد الفلتر فالـ `TotalPages` يطلع غلط.
- تعمل `ToList` قبل الـ `Skip/Take` فتجيب الجدول كله في الميموري.

### التمرين (بإيدك في الـ HR)
اعمل `ToPagedResultAsync` extension واحدة على الـ `IQueryable<T>`:
- تأخد `page, pageSize`
- تعمل Validation جوهها (ترجع Failure ولا Throw؟ قرر بنفسك وليه)
- تعمل `Count + Skip/Take` وترجع `PagedResult<T>`
- استخدمها في 3 ليستات: الموظفين، طلبات الإجازات، الحضور
- ممنوع تنسخ `NormalizePaging` بالنص في كل سيرفس زي ما عملنا هنا — دي كانت دين تقني لضيق الوقت

### اللي يعلي مستواك هنا
- `IQueryable` vs `IEnumerable`: ليه الفلتر لازم يتحط قبل الـ `Count`؟
- `AsNoTracking` في كل قراية ليستة — ليه؟
- Keyset pagination (بعد الـ `Id`) إمتى تبقى أحسن من `Skip/Take`؟ (جداول كبيرة + ترتيب زمني)

---

## 2. الـ Ownership (أخطر باترن أمنياً)

### فين؟
25 استخدام لـ `User.GetUserId()`. القاعدة: أي داتا بتاعة حد بتتجاب بشرطين (`Id + UserId`) مش بـ `Id` لوحده.

```csharp
// UserSkillRepository.cs — الملكية جوه الـ Query نفسه
public async Task<UserSkill?> GetUserSkillAsync(Guid userId, Guid id, CancellationToken ct)
{
    return await _set.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
}
```

```csharp
// MySkillsController.cs — الـ Controller هو الوحيد اللي يعرف HttpContext
var result = await _service.AddAsync(User.GetUserId(), request, cancellationToken);
```

```csharp
// ClaimsPrincipalExtensions.cs — الـ Sub متنقلة بين Sub و NameIdentifier
var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
```

### الغلطة اللي هتقع فيها في الـ HR
موظف يغير رقم في الـ URL ويشوف Payslip زميله (IDOR). دي ثغرة بتسقط انترفيوهات. أي `GetById` في الـ HR اسأل نفسك الأول: "بتاعة مين؟" — لو الإجابة "بتاعة حد" يبقى الشرط مزدوج إجباري. والمدير اللي يشوف فريقه؟ دي Ownership أوسع (TeamId) مش إلغاء للشرط.

### التمرين
- كل endpoints الـ HR اللي فيها `{id}` راجعها واحد واحد: مين مسموح له يشوفه؟
- اعمل `GetMineAsync` و`GetTeamAsync` و`GetByIdForAdminAsync` مفصولين بدل ميثود واحدة بـ if كتير.

---

## 3. الـ State Machine (أعلى قيمة نقل)

### فين؟
اتعملت مرتين بنفس الشكل بالحرف:

```csharp
// SwapService.AcceptAsync — القبول للمستلم بس + لازم Pending
if (entity is null || entity.ReceiverId != userId)
    return Result<SwapDto>.Failure(ErrorType.NotFound, "Swap request not found.");
if (entity.Status != SwapStatus.Pending)
    return Result<SwapDto>.Failure(ErrorType.LogicError, $"Only pending requests can be accepted (current: {entity.Status}).");
entity.Status = SwapStatus.Accepted;
```

```csharp
// TrustService.ResolveAsync — مرفوض يقبل Pending كقرار
if (!Enum.IsDefined(request.Status) || request.Status == ReportStatus.Pending)
    return Result<ReportDto>.Failure(ErrorType.Validation, "Status must be Resolved or Dismissed.");
```

نفس التفكير في: الإلغاء للمرسل بس، والرفض للمستلم بس، والـ NotFound بدل Forbidden عشان منسربش الوجود.

### التمرين (انسخها في الـ HR)
`LeaveRequest`: `Pending → Approved / Rejected / Cancelled` بنفس القواعد:
- اللي يوافق هو المدير مش صاحب الطلب
- اللي يلغي هو صاحب الطلب بس
- مفيش انتقال من غير `Pending`
- رسالة الخطأ تقول الحالة الحالية عشان الموبايل يحدث شاشته

### اللي يعلي مستواك هنا
- ارسم الـ Transitions المسموحة في جدول قبل الكود (State × Action → NewState).
- لما الحالات تكتر (5+) دور على State Pattern بدل if/switch.

---

## 4. الـ Pre-check + الـ Unique Index (طبقتين مش طبقة)

### فين؟ (12 مرة)
```csharp
// MySkillsService — سؤال رخيص Any بدل تحميل صف
if (await _uow.UserSkills.ExistsAsync(userId, request.SkillId, request.Type, ct))
    return Result<MySkillDto>.Failure(ErrorType.Conflict, "Skill already added with the same type.");
```

```csharp
// UserSkillConfiguration.cs — الشبكة الأخيرة في الـ DB نفسها
builder.HasIndex(x => new { x.UserId, x.SkillId, x.Type }).IsUnique();
```

```csharp
// FavoriteUserConfiguration.cs — بفلتر عشان الـ Soft Delete
builder.HasIndex(f => new { f.UserId, f.FavoritedId }).IsUnique().HasFilter("[IsDeleted] = 0");
```

### القاعدة
الـ Check بيدي رسالة بني آدم يفهمها (`Conflict: موجودة`)، والـ Index بيحمي من السباق (اتنين Requests مع بعض). واحدة من غير التانية ناقصة: Check بس يقع تحت الـ Concurrency، وIndex بس يرجع Exception بشعة.

### التمرين في الـ HR
- اسم القسم ميتكررش (Check + Unique).
- تواريخ الإجازة متتداخلش لنفس الموظف (Overlap check زي `SetAvailability` + فكر: ينفع Index هنا؟ لأ — الـ Overlap منطق نطاقي، مفيش Index بيمنعه. إيه الحل؟ قفل تفاؤلي أو Serialized Transaction. دور عليها).

---

## 5. الـ CompleteAsync (اللي هتنساه)

### فين؟ (24 مرة)
```csharp
await _uow.UserSkills.AddAsync(entity, cancellationToken);
await _uow.CompleteAsync(cancellationToken); // من غير السطر ده مفيش حفظ ومفيش Error
```

الريبوهات بتجهز والـ UoW هو الكاشير اللي بيحاسب مرة واحدة. أشهر غلطة صامتة: تنسى السطر والداتا "تتحفظ" من غير Error ومن غير حفظ.

### التمرين
- في كل سيرفس HR جديد: بعد ما تخلص، دور على كل `Add/Remove/تعديل Tracked` واسأل "فين الـ Complete بتاعته؟"
- لما ميثود تعمل كتابتين (اشتراك + كوتا) لازم `Complete` واحدة في الآخر مش اتنين — ليه؟ (Atomicity)

---

## 6. الـ Enum Validation

### فين؟ (4 مرات)
```csharp
if (!Enum.IsDefined(request.Type))
    return Result<MySkillDto>.Failure(ErrorType.Validation, "Invalid skill type.");
```

الـ JSON ممكن يبعت `99` والـ Binder يقبله ساكت ويخزنه. أي Enum جاي من الكلاينت لازم `IsDefined` قبل أي شغل.

### التمرين في الـ HR
`LeaveType` و`LeaveStatus` و`AttendanceStatus` — نفس السطر في كل ميثود بتستقبلهم. ولو لقيت نفسك بتكرره، اعمله Validation helper واحد أو FluentValidation rule.

---

## 7. الـ Mapping اليدوي (الـ Entity مبتطلعش بره)

### فين؟
```csharp
// SwapService.Map — مع fallbacks عشان الـ Include الناقص
return new SwapDto(s.Id, ..., s.SkillOffered?.Name ?? string.Empty, ...);
```

```csharp
// MySkillsService.AddAsync — الـ CategoryName اتجابت بسؤال منفصل
// عشان GetByIdAsync مفيهاش Include
var category = await _uow.SkillCategories.GetByIdAsync(skill.CategoryId, ct);
```

### القواعد
- الـ Entity مبتطلعش بره السيرفس: لا `PasswordHash` يتسرب، ولا Overposting في الـ Update.
- أي `?.` في الـ Map معناها `Include` ناقص — ارجع للـ Repo وزوده بدل ما تخبي المشكلة بـ `?? string.Empty`.
- الـ Update على Tracked Entity بتعديل الحقول المسموحة بس — ممنوع ميثود الـ `Update` العامة على Detached (بتعمل UPDATE لكل الأعمدة).

---

## 8. الـ userId كـ Parameter (مش HttpContext في السيرفس)

الـ Controller هو الوحيد اللي يعرف `HttpContext` ويطلع الـ `userId`، والسيرفس ياخده `Guid` عادي. ليه؟ عشان السيرفس يتست من غير Mock للـ HTTP، ولو ندهته من Background Job (مفيش HttpContext أصلاً) يشتغل. الـ `UserManager` يفضل للـ Identity نفسها (Create/Password) مش عشان تجيب بيه الـ Id.

---

## الـ Generics: تنقل إيه للـ HR ومتعملش إيه؟

انقل 3 بس:
1. `IRepository<T>` — للـ CRUD المتكرر
2. `PagedResult<T>` — للـ Responses
3. `ToPagedResultAsync` — اللي هتكتبها بنفسك (التمرين رقم 1)

ومتعملش `BaseCrudService` عام — ده الفخ: أول ما الـ Business يدخل (Quota check، Ownership، Approval) الـ Generic بيتكسر وهتقعد تحاربه. الـ Generic للميكانيكا، والسيرفس لكل دومين مكتوب بإيده.

---

## خطة الـ HR المقترحة (نفس طريقة SkillSwap)

دومين واحد Vertical Slice كامل (ريبوهات ← UoW ← سيرفس ← كنترولر ← ميجريشن) وبعده اللي بعده. والمراجعة نفسها: التسمية حسب المسؤولية، والـ Ownership قبل أي `GetById`، والـ State Machine مرسومة قبل الكود.

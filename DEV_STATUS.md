# TemplateControl — Статус разработки

> **Последнее обновление**: 2026-06-18 (сессия #2)  
> **Назначение документа**: Быстрый ввод в контекст для AI-ассистентов и разработчиков между сессиями.  
> **Правило**: Обновляй этот файл в конце каждой сессии разработки!

---

## 📋 Краткое описание проекта

**TemplateControl** — библиотека кастомных Avalonia UI контролов для **промышленных HMI/SCADA-интерфейсов**.

Контролы:
- `PipeControl` — визуализация трубопроводов с 3D-эффектом
- `NumericPad` — виртуальная цифровая клавиатура для терминалов

**Стек**: C# / .NET 8 / Avalonia UI 12.0.4  
**Кодекс**: см. `CODEX.md` — обязательные архитектурные правила

---

## 🏗️ Архитектура проекта

```
TemplateControl.sln
├── TemplateControl.Library/              ← Библиотека контролов
│   ├── FittingType.cs                    ← Enum типов фитингов трубы
│   ├── PipeControl.cs                    ← Контрол трубопровода (~470 строк)
│   ├── NumericPad.cs                     ← Контрол цифровой клавиатуры (~330 строк)
│   ├── Themes/
│   │   ├── Generic.axaml                 ← Главный файл тем (ControlTheme для PipeControl + импорт)
│   │   └── NumericPadTheme.axaml         ← ControlTheme для NumericPad
│   └── TemplateControl.csproj            ← net8.0, зависимость: Avalonia 12.0.4
│
├── TemplateControl/                      ← Демо-приложение (TemplateControl.Demo)
│   ├── MainWindow.axaml                  ← Демо: 3 трубы + NumericPad
│   ├── MainWindow.axaml.cs               ← Code-behind (пустой)
│   ├── App.axaml / App.axaml.cs          ← FluentTheme + StyleInclude Generic.axaml
│   ├── Program.cs                        ← Desktop bootstrap
│   └── TemplateControl.Demo.csproj       ← WinExe, ссылается на Library
│
├── CODEX.md                              ← Кодекс разработки (обязательные правила)
└── DEV_STATUS.md                         ← ← ← ЭТОТ ФАЙЛ
```

**Git**: ❌ Репозиторий НЕ инициализирован.

---

## ✅ Реализовано (текущее состояние)

### PipeControl (`TemplateControl.Library/PipeControl.cs`)

| Функция | Статус | Описание |
|---|---|---|
| **Рендеринг труб** | ✅ Готово | Отрисовка сегментов трубы по точкам (формат `"x1,y1;x2,y2;..."`) |
| **3D-затенение** | ✅ Готово | `LinearGradientBrush` с 4 стопами имитирует объём цилиндра |
| **Фланцы (Flange)** | ✅ Готово | Пластины на концах, цвет через `FlangeColor` StyledProperty |
| **Отводы (Elbows)** | ✅ Готово | 90° и 45° отводы (enum `FittingType`) |
| **Скругление углов** | ✅ Готово | Промежуточные точки рисуют эллипс |
| **Цвет Active/Inactive** | ✅ Готово | `ActiveColor` + `InactiveColor` + `IsFilled` |
| **Design Mode** | ✅ Готово | Маркеры-точки, цвета через `DesignMarkerFill`/`DesignMarkerStroke` |
| **Hit Testing** | ✅ Готово | `ICustomHitTest` — попадание в сегменты и вершины |
| **Контекстные меню** | ✅ Готово | Управление точками и фитингами через ПКМ |
| **ControlTheme** | ✅ Готово | Тема через `ControlTheme` (Кодекс соблюдён) |
| **Enum FittingType** | ✅ Готово | Магические строки заменены на enum |
| **Кэш рендер-ресурсов** | ✅ Готово | Pen/Brush кэшируются, пересоздаются при изменении цветов |

### NumericPad (`TemplateControl.Library/NumericPad.cs`) — НОВЫЙ

| Функция | Статус | Описание |
|---|---|---|
| **Lookless TemplatedControl** | ✅ Готово | Базовый класс TemplatedControl, вся визуализация в AXAML |
| **StyledProperty с coerce** | ✅ Готово | Value, Minimum, Maximum — с принудительным приведением к границам |
| **RoutedEvents** | ✅ Готово | `ValueChangedEvent` (Bubbling) + `SubmitEvent` (Bubbling) |
| **Template Parts (PART_)** | ✅ Готово | Display, Clear, Backspace, Submit, Decimal — с null-check |
| **PseudoClasses** | ✅ Готово | `:error` (1с таймер), `:empty` (когда Value=null) |
| **Маршрутизация кликов** | ✅ Готово | Глобальный перехват `Button.ClickEvent` по CommandParameter |
| **Валидация ввода** | ✅ Готово | Pre-validation перед присвоением, игнорирование при выходе за границы |
| **Управление памятью** | ✅ Готово | Отписка от событий старых PART_ в `OnApplyTemplate` |
| **ControlTheme** | ✅ Готово | Отдельный `NumericPadTheme.axaml`, DynamicResource, AutomationProperties |
| **A11y** | ✅ Готово | `AutomationProperties.Name` на всех интерактивных элементах |

### Свойства PipeControl (StyledProperty)

```
Points              : string       — Точки трубы: "x1,y1;x2,y2;..."
PipeColor           : IBrush?      — Кастомный цвет
ShowFlanges         : bool         — Показывать фланцы (default: true)
CellSize            : double       — Размер ячейки (default: 10.0)
StartFitting        : FittingType  — Фитинг на старте (enum, default: None)
EndFitting          : FittingType  — Фитинг на конце (enum, default: None)
IsDesignMode        : bool         — Режим конструирования (default: false)
ActiveColor         : Color        — Цвет активной трубы (default: DodgerBlue)
InactiveColor       : Color        — Цвет неактивной трубы (default: Gray)
IsFilled            : bool         — Активна ли труба (default: false)
Thickness           : double       — Толщина трубы (default: 12.0)
FlangeColor         : Color        — Цвет фланцев (default: RGB(160,160,164))
FlangeBorderColor   : Color        — Цвет границы фланцев (default: Black)
DesignMarkerFill    : Color        — Заливка маркеров (default: White)
DesignMarkerStroke  : Color        — Обводка маркеров (default: DodgerBlue)
```

### Свойства NumericPad (StyledProperty)

```
Value                : decimal?    — Текущее значение (default: null, coerce)
Minimum              : decimal     — Минимум (default: 0, coerce)
Maximum              : decimal     — Максимум (default: decimal.MaxValue, coerce)
MaxLength            : int         — Макс. символов (default: 10)
ShowDecimalSeparator : bool        — Показывать кнопку "." (default: true)
```

### Демо-приложение

- **Pipeline A** — Cooling Water (Active, синяя, толщина 16, фланцы)
- **Pipeline B** — Steam Pressure (Inactive, тёмно-красная, толщина 22, отводы 90°)
- **Pipeline C** — Natural Gas (Active, зелёная, толщина 12, Design Mode)
- **NumericPad** — Демо цифровой клавиатуры (Min=0, Max=999.99)

---

## 🔲 Запланировано / TODO

### Высокий приоритет
- [ ] Инициализировать Git-репозиторий и сделать первый коммит
- [ ] Drag & Drop точек трубы в Design Mode
- [ ] Анимация потока по трубе при `IsFilled=True`
- [ ] Валидация некорректных значений `Points`

### Средний приоритет
- [ ] Дополнительные контролы: `ValveControl`, `TankControl`, `PumpControl`
- [x] ~~Рефакторинг: перевести `StartFitting`/`EndFitting` с `string` на `enum`~~ ✅ Выполнено
- [ ] NuGet-пакетирование библиотеки
- [ ] Поддержка MVVM (демо с ViewModel)
- [ ] Визуальная сетка (grid lines) на канвасе

### Низкий приоритет
- [ ] Юнит-тесты для `ParsePoints`, `HitTest`, NumericPad валидации
- [ ] XML-документация (`///`) на все публичные API
- [ ] README.md с описанием и скриншотами

---

## 🔧 Известные проблемы / Технический долг

1. ~~**Фитинги заданы строками**~~ ✅ Исправлено — используется `enum FittingType`
2. **Нет привязки к MVVM** — демо полностью декларативное, нет ViewModel
3. **`ClipToBounds = false`** — контрол рисует за своими границами (by design для фитингов)
4. **Нет Git** — проект не под системой контроля версий

---

## 📝 Журнал сессий разработки

> Добавляй запись в НАЧАЛО списка после каждой рабочей сессии.

### 2026-06-18 (сессия #2) — Рефакторинг PipeControl + создание NumericPad
- **Что сделано**:
  - **PipeControl рефакторинг по Кодексу**: `<Style>` → `<ControlTheme>`, `string` фитинги → `enum FittingType`, хардкод-цвета → `StyledProperty` (FlangeColor, FlangeBorderColor, DesignMarkerFill, DesignMarkerStroke), кэширование Pen/Brush
  - **Создан NumericPad**: полная реализация по ТЗ — lookless TemplatedControl, coerce-валидация, RoutedEvents, PART_ с управлением памятью, PseudoClasses (:error/:empty), маршрутизация кликов, ControlTheme с DynamicResource и AutomationProperties
  - **Создан CODEX.md**: Кодекс разработки заложен в систему передачи контекста
- **Новые файлы**: `FittingType.cs`, `NumericPad.cs`, `NumericPadTheme.axaml`, `CODEX.md`
- **Изменённые файлы**: `PipeControl.cs`, `Generic.axaml`, `MainWindow.axaml`
- **Сборка**: ✅ 0 ошибок, 0 предупреждений
- **Следующие шаги**: Инициализация Git, Drag&Drop для труб, анимация потока

### 2026-06-18 (сессия #1) — Начальный аудит и создание DEV_STATUS.md
- **Что сделано**: Проведён полный аудит проекта. Создан файл `DEV_STATUS.md` для отслеживания прогресса.
- **Состояние проекта**: Работоспособная библиотека `PipeControl` + демо-приложение.

---

## 📌 Инструкция для AI-ассистента

> **Прочитай этот файл первым делом** в начале каждой сессии!

1. Открой `DEV_STATUS.md` — здесь вся информация о проекте
2. **Прочитай `CODEX.md`** — обязательный кодекс разработки (архитектурные правила, стандарты контролов)
3. Посмотри секцию **«Журнал сессий»** — там последние изменения
4. Проверь секцию **«TODO»** — актуальные задачи
5. В конце сессии **ОБЯЗАТЕЛЬНО обнови** этот файл:
   - Добавь запись в «Журнал сессий» (в начало списка)
   - Обнови статусы в таблице реализованного
   - Обнови TODO-лист
   - Обнови дату в шапке файла

> ⚠️ **CODEX.md** содержит обязательные правила: TemplatedControl, StyledProperty, ControlTheme, изоляция от MVVM, Render(DrawingContext) для графики. Нарушение Кодекса запрещено.
